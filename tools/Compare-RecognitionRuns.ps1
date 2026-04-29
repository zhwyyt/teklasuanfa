param(
    [Parameter(Mandatory = $true)]
    [string]$BaselineResultDirectory,

    [Parameter(Mandatory = $true)]
    [string]$CandidateResultDirectory,

    [string]$OutputMarkdown
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$baselineDirectory = (Resolve-Path $BaselineResultDirectory).Path
$candidateDirectory = (Resolve-Path $CandidateResultDirectory).Path

if ([string]::IsNullOrWhiteSpace($OutputMarkdown))
{
    $OutputMarkdown = Join-Path $candidateDirectory "run-comparison.md"
}

function Load-Summary([string]$directory)
{
    $path = Join-Path $directory "batch-summary.json"
    if (-not (Test-Path $path))
    {
        throw "未找到 batch-summary.json: $path"
    }

    return @(Get-Content $path -Raw | ConvertFrom-Json)
}

function Build-Lookup([object[]]$items)
{
    $lookup = @{}
    foreach ($item in $items)
    {
        $lookup[[string]$item.AssemblyId] = $item
    }

    return $lookup
}

$baselineItems = Load-Summary $baselineDirectory
$candidateItems = Load-Summary $candidateDirectory

$baselineLookup = Build-Lookup $baselineItems
$candidateLookup = Build-Lookup $candidateItems
$assemblyIds = @($baselineLookup.Keys + $candidateLookup.Keys | Sort-Object -Unique)

$rows = foreach ($assemblyId in $assemblyIds)
{
    $baseline = $baselineLookup[$assemblyId]
    $candidate = $candidateLookup[$assemblyId]
    $baselineBodyFamily = if ($null -ne $baseline) { [string]$baseline.BodyFamily } else { "" }
    $candidateBodyFamily = if ($null -ne $candidate) { [string]$candidate.BodyFamily } else { "" }
    $baselineBracketCount = if ($null -ne $baseline) { [int]$baseline.BracketCount } else { 0 }
    $candidateBracketCount = if ($null -ne $candidate) { [int]$candidate.BracketCount } else { 0 }
    $baselineReview = if ($null -ne $baseline) { [bool]$baseline.ReviewRequired } else { $false }
    $candidateReview = if ($null -ne $candidate) { [bool]$candidate.ReviewRequired } else { $false }
    $sourceFile = if ($null -ne $candidate) { [string]$candidate.SourceFile } elseif ($null -ne $baseline) { [string]$baseline.SourceFile } else { "" }

    [pscustomobject]@{
        AssemblyId            = $assemblyId
        BaselineBodyFamily    = $baselineBodyFamily
        CandidateBodyFamily   = $candidateBodyFamily
        BaselineBracketCount  = $baselineBracketCount
        CandidateBracketCount = $candidateBracketCount
        DeltaBracketCount     = $candidateBracketCount - $baselineBracketCount
        BaselineReview        = $baselineReview
        CandidateReview       = $candidateReview
        SourceFile            = $sourceFile
    }
}

$summary = [pscustomobject]@{
    Assemblies                  = $rows.Count
    BaselineTotalBrackets       = ($rows | Measure-Object BaselineBracketCount -Sum).Sum
    CandidateTotalBrackets      = ($rows | Measure-Object CandidateBracketCount -Sum).Sum
    BaselinePositiveAssemblies  = @($rows | Where-Object { $_.BaselineBracketCount -gt 0 }).Count
    CandidatePositiveAssemblies = @($rows | Where-Object { $_.CandidateBracketCount -gt 0 }).Count
    BaselineMaxBracketCount     = ($rows | Measure-Object BaselineBracketCount -Maximum).Maximum
    CandidateMaxBracketCount    = ($rows | Measure-Object CandidateBracketCount -Maximum).Maximum
    BodyFamilyChangedAssemblies = @($rows | Where-Object { $_.BaselineBodyFamily -ne $_.CandidateBodyFamily }).Count
}

$topReductions = @(
    $rows |
        Where-Object { $_.DeltaBracketCount -lt 0 } |
        Sort-Object DeltaBracketCount, AssemblyId |
        Select-Object -First 20 AssemblyId, BaselineBodyFamily, CandidateBodyFamily, BaselineBracketCount, CandidateBracketCount, DeltaBracketCount, SourceFile
)

$topIncreases = @(
    $rows |
        Where-Object { $_.DeltaBracketCount -gt 0 } |
        Sort-Object @{ Expression = "DeltaBracketCount"; Descending = $true }, @{ Expression = "AssemblyId"; Descending = $false } |
        Select-Object -First 20 AssemblyId, BaselineBodyFamily, CandidateBodyFamily, BaselineBracketCount, CandidateBracketCount, DeltaBracketCount, SourceFile
)

$bodyChanges = @(
    $rows |
        Where-Object { $_.BaselineBodyFamily -ne $_.CandidateBodyFamily } |
        Sort-Object AssemblyId |
        Select-Object -First 20 AssemblyId, BaselineBodyFamily, CandidateBodyFamily, BaselineBracketCount, CandidateBracketCount, SourceFile
)

$lines = @(
    "# Recognition Run Comparison",
    "",
    "Baseline: $baselineDirectory",
    "Candidate: $candidateDirectory",
    "",
    "## Summary",
    "",
    "- Assemblies: $($summary.Assemblies)",
    "- Baseline total brackets: $($summary.BaselineTotalBrackets)",
    "- Candidate total brackets: $($summary.CandidateTotalBrackets)",
    "- Baseline positive assemblies: $($summary.BaselinePositiveAssemblies)",
    "- Candidate positive assemblies: $($summary.CandidatePositiveAssemblies)",
    "- Baseline max bracket count: $($summary.BaselineMaxBracketCount)",
    "- Candidate max bracket count: $($summary.CandidateMaxBracketCount)",
    "- Body family changed assemblies: $($summary.BodyFamilyChangedAssemblies)",
    ""
)

if ($topReductions.Count -gt 0)
{
    $lines += "## Largest Bracket Reductions"
    $lines += ""
    $lines += "| AssemblyId | BaselineBody | CandidateBody | BaselineBracketCount | CandidateBracketCount | Delta | SourceFile |"
    $lines += "| --- | --- | --- | --- | --- | --- | --- |"
    foreach ($item in $topReductions)
    {
        $lines += "| $($item.AssemblyId) | $($item.BaselineBodyFamily) | $($item.CandidateBodyFamily) | $($item.BaselineBracketCount) | $($item.CandidateBracketCount) | $($item.DeltaBracketCount) | $($item.SourceFile) |"
    }
    $lines += ""
}

if ($topIncreases.Count -gt 0)
{
    $lines += "## Largest Bracket Increases"
    $lines += ""
    $lines += "| AssemblyId | BaselineBody | CandidateBody | BaselineBracketCount | CandidateBracketCount | Delta | SourceFile |"
    $lines += "| --- | --- | --- | --- | --- | --- | --- |"
    foreach ($item in $topIncreases)
    {
        $lines += "| $($item.AssemblyId) | $($item.BaselineBodyFamily) | $($item.CandidateBodyFamily) | $($item.BaselineBracketCount) | $($item.CandidateBracketCount) | +$($item.DeltaBracketCount) | $($item.SourceFile) |"
    }
    $lines += ""
}

if ($bodyChanges.Count -gt 0)
{
    $lines += "## Body Family Changes"
    $lines += ""
    $lines += "| AssemblyId | BaselineBody | CandidateBody | BaselineBracketCount | CandidateBracketCount | SourceFile |"
    $lines += "| --- | --- | --- | --- | --- | --- |"
    foreach ($item in $bodyChanges)
    {
        $lines += "| $($item.AssemblyId) | $($item.BaselineBodyFamily) | $($item.CandidateBodyFamily) | $($item.BaselineBracketCount) | $($item.CandidateBracketCount) | $($item.SourceFile) |"
    }
}

Set-Content -Path $OutputMarkdown -Value $lines -Encoding UTF8
Write-Host "Recognition comparison written to: $OutputMarkdown"
