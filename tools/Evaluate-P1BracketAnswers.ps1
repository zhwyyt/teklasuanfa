param(
    [Parameter(Mandatory = $true)]
    [string]$PriorityReviewCsv,

    [Parameter(Mandatory = $true)]
    [string]$P1BracketAnswersCsv,

    [string]$PriorityBracketAnswersCsv,

    [string]$BodyAnswersCsv,

    [string]$OutputMergedBracketCsv,

    [string]$OutputMergedPriorityCsv,

    [string]$ReviewQueueCsv,

    [string]$OutputMergedReviewQueueCsv,

    [string]$OutputPatternDecisionMarkdown,

    [string]$OutputPatternDecisionCsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$priorityReviewCsv = (Resolve-Path $PriorityReviewCsv).Path
$p1BracketAnswersCsv = (Resolve-Path $P1BracketAnswersCsv).Path
$defaultDirectory = Split-Path -Parent $priorityReviewCsv

if ([string]::IsNullOrWhiteSpace($PriorityBracketAnswersCsv))
{
    $PriorityBracketAnswersCsv = Join-Path $defaultDirectory "priority-bracket-answers.csv"
}

if ([string]::IsNullOrWhiteSpace($BodyAnswersCsv))
{
    $BodyAnswersCsv = Join-Path $defaultDirectory "priority-body-answers.csv"
}

$priorityBracketAnswersCsv = (Resolve-Path $PriorityBracketAnswersCsv).Path
$bodyAnswersCsv = (Resolve-Path $BodyAnswersCsv).Path

if ([string]::IsNullOrWhiteSpace($OutputMergedBracketCsv))
{
    $OutputMergedBracketCsv = Join-Path $defaultDirectory "priority-bracket-answers.p1.merged.csv"
}

if ([string]::IsNullOrWhiteSpace($OutputMergedPriorityCsv))
{
    $OutputMergedPriorityCsv = Join-Path $defaultDirectory "priority-review-pack.p1.merged.csv"
}

$baseBracketRows = @(Import-Csv $priorityBracketAnswersCsv)
$p1Rows = @(Import-Csv $p1BracketAnswersCsv)

if ($baseBracketRows.Count -eq 0)
{
    throw "Priority bracket answers CSV is empty: $priorityBracketAnswersCsv"
}

if ($p1Rows.Count -eq 0)
{
    throw "P1 bracket answers CSV is empty: $p1BracketAnswersCsv"
}

$p1Lookup = @{}
foreach ($row in $p1Rows)
{
    $assemblyId = [string]$row.AssemblyId
    if (-not [string]::IsNullOrWhiteSpace($assemblyId))
    {
        $p1Lookup[$assemblyId] = $row
    }
}

$mergedRows = foreach ($row in $baseBracketRows)
{
    $assemblyId = [string]$row.AssemblyId
    if (-not $p1Lookup.ContainsKey($assemblyId))
    {
        $row
        continue
    }

    $p1Row = $p1Lookup[$assemblyId]
    foreach ($column in @("HumanHasBracket", "HumanBracketPartGroups", "HumanBodyFamily", "HumanVerdict", "Notes"))
    {
        $value = [string]$p1Row.$column
        if (-not [string]::IsNullOrWhiteSpace($value))
        {
            $row.$column = $value.Trim()
        }
    }

    $row
}

$mergedRows | Export-Csv -Path $OutputMergedBracketCsv -NoTypeInformation -Encoding UTF8 -Force

$toolsDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$evaluateScript = Join-Path $toolsDirectory "Evaluate-PriorityAnswerSheets.ps1"
$patternDecisionScript = Join-Path $toolsDirectory "Summarize-P1PatternDecisions.ps1"

Write-Host "Merged P1 bracket answers into: $OutputMergedBracketCsv"
Write-Host ""

if ([string]::IsNullOrWhiteSpace($ReviewQueueCsv))
{
    & $evaluateScript `
        -PriorityReviewCsv $priorityReviewCsv `
        -BracketAnswersCsv $OutputMergedBracketCsv `
        -BodyAnswersCsv $bodyAnswersCsv `
        -OutputMergedPriorityCsv $OutputMergedPriorityCsv
}
else
{
    & $evaluateScript `
        -PriorityReviewCsv $priorityReviewCsv `
        -BracketAnswersCsv $OutputMergedBracketCsv `
        -BodyAnswersCsv $bodyAnswersCsv `
        -OutputMergedPriorityCsv $OutputMergedPriorityCsv `
        -ReviewQueueCsv $ReviewQueueCsv `
        -OutputMergedReviewQueueCsv $OutputMergedReviewQueueCsv
}

Write-Host ""
Write-Host "Building P1 pattern decision summary..."
Write-Host ""

& $patternDecisionScript `
    -P1BracketAnswersCsv $p1BracketAnswersCsv `
    -OutputMarkdown $OutputPatternDecisionMarkdown `
    -OutputCsv $OutputPatternDecisionCsv
