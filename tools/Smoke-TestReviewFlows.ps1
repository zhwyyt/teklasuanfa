param(
    [string]$WorkspaceToolsDirectory,

    [string]$WorkspaceResultDirectory,

    [string]$PackageToolsDirectory,

    [string]$PackageResultDirectory,

    [string]$OutputRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$isWorkspaceRoot = Test-Path (Join-Path $root "src")
$isPackageRoot = (Test-Path (Join-Path $root "app")) -and (Test-Path (Join-Path $root "results")) -and (Test-Path (Join-Path $root "tools"))
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

if ([string]::IsNullOrWhiteSpace($WorkspaceToolsDirectory) -and $isWorkspaceRoot)
{
    $WorkspaceToolsDirectory = Join-Path $root "tools"
}

if ([string]::IsNullOrWhiteSpace($WorkspaceResultDirectory) -and $isWorkspaceRoot)
{
    $WorkspaceResultDirectory = Join-Path $root ".tmpresults\run_body_bracket_real_01_workflow"
}

if ([string]::IsNullOrWhiteSpace($PackageToolsDirectory))
{
    if ($isPackageRoot)
    {
        $PackageToolsDirectory = Join-Path $root "tools"
    }
    elseif ($isWorkspaceRoot)
    {
        $PackageToolsDirectory = Join-Path $root ".deliverables\tekla-body-bracket-testable-latest\tools"
    }
}

if ([string]::IsNullOrWhiteSpace($PackageResultDirectory))
{
    if ($isPackageRoot)
    {
        $PackageResultDirectory = Join-Path $root "results"
    }
    elseif ($isWorkspaceRoot)
    {
        $PackageResultDirectory = Join-Path $root ".deliverables\tekla-body-bracket-testable-latest\results"
    }
}

if ([string]::IsNullOrWhiteSpace($OutputRoot))
{
    if ($isPackageRoot)
    {
        $OutputRoot = Join-Path $root ("results\review-flow-smoketests-" + $timestamp)
    }
    else
    {
        $OutputRoot = Join-Path $root (".tmpresults\review-flow-smoketests-" + $timestamp)
    }
}

New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null

function Test-FlowInputs {
    param(
        [string]$ToolsDirectory,
        [string]$ResultDirectory,
        [string[]]$RequiredFiles
    )

    if ([string]::IsNullOrWhiteSpace($ToolsDirectory) -or [string]::IsNullOrWhiteSpace($ResultDirectory))
    {
        return $false
    }

    if (-not (Test-Path $ToolsDirectory) -or -not (Test-Path $ResultDirectory))
    {
        return $false
    }

    $p1EvaluateScript = Join-Path $ToolsDirectory "Evaluate-P1BracketAnswers.ps1"
    $builtUpTEvaluateScript = Join-Path $ToolsDirectory "Evaluate-BuiltUpTReviewBundle.ps1"
    if ((-not (Test-Path $p1EvaluateScript)) -or (-not (Test-Path $builtUpTEvaluateScript)))
    {
        return $false
    }

    foreach ($name in $RequiredFiles)
    {
        if (-not (Test-Path (Join-Path $ResultDirectory $name)))
        {
            return $false
        }
    }

    return $true
}

function Test-NonEmptyCsv {
    param(
        [string]$CsvPath
    )

    if (-not (Test-Path $CsvPath))
    {
        return $false
    }

    $rows = @(Import-Csv $CsvPath)
    return $rows.Count -gt 0
}

function New-SmokeCsv {
    param(
        [string]$SourceCsv,
        [string]$OutputCsv,
        [ValidateSet("Full", "BuiltUpT")]
        [string]$Mode
    )

    $rows = @(Import-Csv $SourceCsv)
    if ($rows.Count -eq 0)
    {
        throw "Source CSV is empty: $SourceCsv"
    }

    foreach ($row in $rows)
    {
        switch ($Mode)
        {
            "Full" {
                if ([string]$row.PatternHint -eq "compact_gkz_plate_pair")
                {
                    $row.HumanHasBracket = "yes"
                    $row.HumanVerdict = "keep"
                    $row.Notes = "smoke test"
                }
                elseif ([string]$row.PatternHint -eq "angle_short_plate_cluster")
                {
                    $row.HumanHasBracket = "no"
                    $row.HumanVerdict = "suppress"
                    $row.Notes = "smoke test"
                }
            }
            "BuiltUpT" {
                $row.HumanHasBracket = "no"
                $row.HumanVerdict = "suppress"
                $row.Notes = "smoke test"
            }
        }
    }

    $rows | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8 -Force
}

function Assert-PatternSummary {
    param(
        [string]$PatternCsv,
        [ValidateSet("Full", "BuiltUpT")]
        [string]$Mode
    )

    $rows = @(Import-Csv $PatternCsv)
    if ($rows.Count -eq 0)
    {
        throw "Pattern decision summary is empty: $PatternCsv"
    }

    switch ($Mode)
    {
        "Full" {
            if (@($rows).Count -ne 2)
            {
                throw "Expected 2 pattern buckets in full smoke test, got $(@($rows).Count): $PatternCsv"
            }

            $angleRow = @($rows | Where-Object { $_.PatternHint -eq "angle_short_plate_cluster" }) | Select-Object -First 1
            $gkzRow = @($rows | Where-Object { $_.PatternHint -eq "compact_gkz_plate_pair" }) | Select-Object -First 1
            if ($null -eq $angleRow -or $null -eq $gkzRow)
            {
                throw "Full smoke test did not produce both expected buckets: $PatternCsv"
            }

            if (($angleRow.Recommendation -notmatch "safe candidate") -or ($gkzRow.Recommendation -notmatch "protect or boost"))
            {
                throw "Full smoke test recommendations do not match expectations: $PatternCsv"
            }
        }
        "BuiltUpT" {
            if (@($rows).Count -ne 1)
            {
                throw "Expected 1 pattern bucket in BuiltUpT smoke test, got $(@($rows).Count): $PatternCsv"
            }

            $row = $rows[0]
            if (($row.PatternHint -ne "angle_short_plate_cluster") -or ($row.Recommendation -notmatch "safe candidate"))
            {
                throw "BuiltUpT smoke test summary does not match expectations: $PatternCsv"
            }
        }
    }
}

function Invoke-SmokeFlow {
    param(
        [string]$FlowName,
        [string]$ToolsDirectory,
        [string]$ResultDirectory,
        [string]$SourceCsvName,
        [ValidateSet("Full", "BuiltUpT")]
        [string]$Mode,
        [string]$OutputRoot
    )

    $flowDirectory = Join-Path $OutputRoot $FlowName
    New-Item -ItemType Directory -Force -Path $flowDirectory | Out-Null

    $sourceCsv = Join-Path $ResultDirectory $SourceCsvName
    $leaf = [System.IO.Path]::GetFileNameWithoutExtension($SourceCsvName)
    $smokeCsv = Join-Path $flowDirectory ($leaf + ".smoketest.csv")
    if ($Mode -eq "BuiltUpT")
    {
        $mergedBracketCsv = Join-Path $flowDirectory "priority-bracket-answers.p1.builtupt-first.smoketest.merged.csv"
        $mergedPriorityCsv = Join-Path $flowDirectory "priority-review-pack.p1.builtupt-first.smoketest.merged.csv"
        $mergedReviewQueueCsv = Join-Path $flowDirectory "review-queue.p1.builtupt-first.smoketest.merged.csv"
        $patternMarkdown = Join-Path $flowDirectory "builtupt-pattern-decision-summary.smoketest.md"
        $patternCsv = Join-Path $flowDirectory "builtupt-pattern-decision-summary.smoketest.csv"
    }
    else
    {
        $mergedBracketCsv = Join-Path $flowDirectory "priority-bracket-answers.p1.smoketest.merged.csv"
        $mergedPriorityCsv = Join-Path $flowDirectory "priority-review-pack.p1.smoketest.merged.csv"
        $mergedReviewQueueCsv = Join-Path $flowDirectory "review-queue.p1.smoketest.merged.csv"
        $patternMarkdown = Join-Path $flowDirectory "p1-pattern-decision-summary.smoketest.md"
        $patternCsv = Join-Path $flowDirectory "p1-pattern-decision-summary.smoketest.csv"
    }

    New-SmokeCsv -SourceCsv $sourceCsv -OutputCsv $smokeCsv -Mode $Mode

    if ($Mode -eq "BuiltUpT")
    {
        & (Join-Path $ToolsDirectory "Evaluate-BuiltUpTReviewBundle.ps1") `
            -PriorityReviewCsv (Join-Path $ResultDirectory "priority-review-pack.inline.csv") `
            -BuiltUpTBracketAnswersCsv $smokeCsv `
            -ReviewQueueCsv (Join-Path $ResultDirectory "review-queue.csv") `
            -OutputMergedBracketCsv $mergedBracketCsv `
            -OutputMergedPriorityCsv $mergedPriorityCsv `
            -OutputMergedReviewQueueCsv $mergedReviewQueueCsv `
            -OutputPatternDecisionMarkdown $patternMarkdown `
            -OutputPatternDecisionCsv $patternCsv | Out-String | Out-Null
    }
    else
    {
        & (Join-Path $ToolsDirectory "Evaluate-P1BracketAnswers.ps1") `
            -PriorityReviewCsv (Join-Path $ResultDirectory "priority-review-pack.inline.csv") `
            -P1BracketAnswersCsv $smokeCsv `
            -ReviewQueueCsv (Join-Path $ResultDirectory "review-queue.csv") `
            -OutputMergedBracketCsv $mergedBracketCsv `
            -OutputMergedPriorityCsv $mergedPriorityCsv `
            -OutputMergedReviewQueueCsv $mergedReviewQueueCsv `
            -OutputPatternDecisionMarkdown $patternMarkdown `
            -OutputPatternDecisionCsv $patternCsv | Out-String | Out-Null
    }

    foreach ($path in @($mergedBracketCsv, $mergedPriorityCsv, $mergedReviewQueueCsv, $patternMarkdown, $patternCsv))
    {
        if (-not (Test-Path $path))
        {
            throw "Expected smoke-test artifact not found: $path"
        }
    }

    Assert-PatternSummary -PatternCsv $patternCsv -Mode $Mode
    $patternRows = @(Import-Csv $patternCsv)

    return [pscustomobject]@{
        FlowName = $FlowName
        Mode = $Mode
        OutputDirectory = $flowDirectory
        PatternBuckets = @($patternRows).Count
        Patterns = (@($patternRows | Select-Object -ExpandProperty PatternHint) -join ", ")
    }
}

$results = @()
$hasBaseInputs = $false
$hasApplicableFlow = $false

if (Test-FlowInputs `
        -ToolsDirectory $WorkspaceToolsDirectory `
        -ResultDirectory $WorkspaceResultDirectory `
        -RequiredFiles @("priority-review-pack.inline.csv", "review-queue.csv"))
{
    $hasBaseInputs = $true

    if (Test-NonEmptyCsv (Join-Path $WorkspaceResultDirectory "priority-bracket-answers.p1.csv"))
    {
        $results += Invoke-SmokeFlow `
            -FlowName "workspace-p1-full" `
            -ToolsDirectory $WorkspaceToolsDirectory `
            -ResultDirectory $WorkspaceResultDirectory `
            -SourceCsvName "priority-bracket-answers.p1.csv" `
            -Mode "Full" `
            -OutputRoot $OutputRoot
        $hasApplicableFlow = $true
    }

    if (Test-NonEmptyCsv (Join-Path $WorkspaceResultDirectory "priority-bracket-answers.p1.builtupt-first.csv"))
    {
        $results += Invoke-SmokeFlow `
            -FlowName "workspace-p1-builtupt" `
            -ToolsDirectory $WorkspaceToolsDirectory `
            -ResultDirectory $WorkspaceResultDirectory `
            -SourceCsvName "priority-bracket-answers.p1.builtupt-first.csv" `
            -Mode "BuiltUpT" `
            -OutputRoot $OutputRoot
        $hasApplicableFlow = $true
    }
}

if (Test-FlowInputs `
        -ToolsDirectory $PackageToolsDirectory `
        -ResultDirectory $PackageResultDirectory `
        -RequiredFiles @("priority-review-pack.inline.csv", "review-queue.csv"))
{
    $hasBaseInputs = $true

    if (Test-NonEmptyCsv (Join-Path $PackageResultDirectory "priority-bracket-answers.p1.csv"))
    {
        $results += Invoke-SmokeFlow `
            -FlowName "package-p1-full" `
            -ToolsDirectory $PackageToolsDirectory `
            -ResultDirectory $PackageResultDirectory `
            -SourceCsvName "priority-bracket-answers.p1.csv" `
            -Mode "Full" `
            -OutputRoot $OutputRoot
        $hasApplicableFlow = $true
    }

    if (Test-NonEmptyCsv (Join-Path $PackageResultDirectory "priority-bracket-answers.p1.builtupt-first.csv"))
    {
        $results += Invoke-SmokeFlow `
            -FlowName "package-p1-builtupt" `
            -ToolsDirectory $PackageToolsDirectory `
            -ResultDirectory $PackageResultDirectory `
            -SourceCsvName "priority-bracket-answers.p1.builtupt-first.csv" `
            -Mode "BuiltUpT" `
            -OutputRoot $OutputRoot
        $hasApplicableFlow = $true
    }
}

if (-not $hasBaseInputs)
{
    throw "No valid workspace/package review-flow inputs were found."
}

if (-not $hasApplicableFlow)
{
    $results += [pscustomobject]@{
        FlowName = "no-applicable-review-source"
        Mode = "Skipped"
        OutputDirectory = $OutputRoot
        PatternBuckets = 0
        Patterns = "(none)"
    }
}

$results | Format-Table -AutoSize
