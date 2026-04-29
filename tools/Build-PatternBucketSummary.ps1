param(
    [Parameter(Mandatory = $true)]
    [string]$ReviewQueueCsv,

    [string]$OutputMarkdown,

    [string]$OutputCsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-PatternBucket {
    param(
        [string]$SourceFile,
        [string]$PredictedBodyFamily,
        [string]$PredictedBracketPartGroups,
        [string]$PredictedBracketProfiles
    )

    $partCount = 0
    if (-not [string]::IsNullOrWhiteSpace($PredictedBracketPartGroups))
    {
        $partCount = @(
            $PredictedBracketPartGroups.Split(",", [System.StringSplitOptions]::RemoveEmptyEntries) |
                ForEach-Object { $_.Trim() } |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        ).Count
    }

    if ($SourceFile -like "*GKZ*" -and $PredictedBodyFamily -eq "BuiltUpH" -and $partCount -eq 2)
    {
        return "compact_gkz_plate_pair"
    }

    if ($PredictedBodyFamily -eq "BuiltUpT" -and
        $partCount -ge 4 -and
        $PredictedBracketProfiles -like "*CC52.5-4-15-34*" -and
        $PredictedBracketProfiles -like "*PL12*130*")
    {
        return "angle_short_plate_cluster"
    }

    if ($partCount -eq 1)
    {
        return "single_plate_appendage"
    }

    if ($PredictedBodyFamily -eq "Irregular")
    {
        return "irregular_misc"
    }

    return "other"
}

function Get-BucketRecommendation {
    param([string]$Bucket)

    switch ($Bucket)
    {
        "angle_short_plate_cluster" { return "low-risk suppression candidate after truth confirmation" }
        "compact_gkz_plate_pair" { return "high-blast-radius bucket; do not suppress before truth confirmation" }
        "single_plate_appendage" { return "keep under manual review until more truth is available" }
        "irregular_misc" { return "keep under manual review until more truth is available" }
        default { return "manual review" }
    }
}

$reviewQueueCsv = (Resolve-Path $ReviewQueueCsv).Path
$reviewRows = @(Import-Csv $reviewQueueCsv)

if ($reviewRows.Count -eq 0)
{
    throw "Review queue CSV is empty: $reviewQueueCsv"
}

$outputDirectory = Split-Path -Parent $reviewQueueCsv
if ([string]::IsNullOrWhiteSpace($OutputMarkdown))
{
    $OutputMarkdown = Join-Path $outputDirectory "pattern-bucket-summary.md"
}

if ([string]::IsNullOrWhiteSpace($OutputCsv))
{
    $OutputCsv = Join-Path $outputDirectory "pattern-bucket-summary.csv"
}

$annotatedRows = foreach ($row in $reviewRows)
{
    $bucket = Get-PatternBucket `
        -SourceFile ([string]$row.SourceFile) `
        -PredictedBodyFamily ([string]$row.PredictedBodyFamily) `
        -PredictedBracketPartGroups ([string]$row.PredictedBracketPartGroups) `
        -PredictedBracketProfiles ([string]$row.PredictedBracketProfiles)

    $predictedCount = 0
    if (-not [string]::IsNullOrWhiteSpace([string]$row.PredictedBracketCount))
    {
        [void][int]::TryParse([string]$row.PredictedBracketCount, [ref]$predictedCount)
    }

    [pscustomobject]@{
        AssemblyId = [string]$row.AssemblyId
        PatternBucket = $bucket
        SourceFile = [string]$row.SourceFile
        PredictedBodyFamily = [string]$row.PredictedBodyFamily
        PredictedBracketCount = $predictedCount
        PredictedBracketProfiles = [string]$row.PredictedBracketProfiles
        PredictedBracketReasons = [string]$row.PredictedBracketReasons
        Recommendation = Get-BucketRecommendation -Bucket $bucket
    }
}

$totalAssemblies = @($annotatedRows | Select-Object -ExpandProperty AssemblyId -Unique).Count
$totalBrackets = ($annotatedRows | Measure-Object -Property PredictedBracketCount -Sum).Sum

$summaryRows = foreach ($group in @($annotatedRows | Group-Object PatternBucket | Sort-Object Name))
{
    $bucketRows = @($group.Group)
    $assemblyIds = @($bucketRows | Select-Object -ExpandProperty AssemblyId)
    $bucketBrackets = ($bucketRows | Measure-Object -Property PredictedBracketCount -Sum).Sum

    [pscustomobject]@{
        PatternBucket = [string]$group.Name
        AssemblyCount = $assemblyIds.Count
        TotalBracketCount = $bucketBrackets
        ProjectedAssemblyCountIfSuppressed = $totalAssemblies - $assemblyIds.Count
        ProjectedBracketCountIfSuppressed = $totalBrackets - $bucketBrackets
        AssemblyIds = ($assemblyIds -join ", ")
        Recommendation = Get-BucketRecommendation -Bucket ([string]$group.Name)
    }
}

$summaryRows | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8 -Force

$lines = @(
    "# Pattern Bucket Summary",
    "",
    "This summary groups the current review-queue candidates into pattern buckets and estimates the blast radius if a bucket were suppressed.",
    "",
    "| PatternBucket | Assemblies | Brackets | ProjectedAssembliesIfSuppressed | ProjectedBracketsIfSuppressed | Recommendation |",
    "| --- | --- | --- | --- | --- | --- |"
)

foreach ($row in $summaryRows)
{
    $lines += "| $($row.PatternBucket) | $($row.AssemblyCount) | $($row.TotalBracketCount) | $($row.ProjectedAssemblyCountIfSuppressed) | $($row.ProjectedBracketCountIfSuppressed) | $($row.Recommendation) |"
}

$lines += ""
$lines += "## Details"
$lines += ""

foreach ($row in $summaryRows)
{
    $lines += "### $($row.PatternBucket)"
    $lines += ""
    $lines += "- Assemblies: $($row.AssemblyCount)"
    $lines += "- Brackets: $($row.TotalBracketCount)"
    $lines += "- Projected assemblies if suppressed: $($row.ProjectedAssemblyCountIfSuppressed)"
    $lines += "- Projected brackets if suppressed: $($row.ProjectedBracketCountIfSuppressed)"
    $lines += "- Recommendation: $($row.Recommendation)"
    $lines += "- AssemblyIds: $($row.AssemblyIds)"
    $lines += ""
}

Set-Content -Path $OutputMarkdown -Value $lines -Encoding UTF8

[pscustomobject]@{
    ReviewQueueCsv = $reviewQueueCsv
    OutputMarkdown = $OutputMarkdown
    OutputCsv = $OutputCsv
    BucketCount = @($summaryRows).Count
    TotalAssemblies = $totalAssemblies
    TotalBrackets = $totalBrackets
} | Format-List
