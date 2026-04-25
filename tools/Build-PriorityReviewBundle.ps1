param(
    [Parameter(Mandatory = $true)]
    [string]$PriorityReviewCsv,

    [Parameter(Mandatory = $true)]
    [string]$ResultDirectory,

    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$priorityReviewCsv = (Resolve-Path $PriorityReviewCsv).Path
$resultDirectory = (Resolve-Path $ResultDirectory).Path

if ([string]::IsNullOrWhiteSpace($OutputDirectory))
{
    $OutputDirectory = Join-Path $resultDirectory "priority-review-bundle"
}

$outputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
$membersOutput = Join-Path $outputDirectory "members"
$assembliesOutput = Join-Path $outputDirectory "assemblies"

New-Item -ItemType Directory -Force -Path $outputDirectory, $membersOutput, $assembliesOutput | Out-Null

$priorityRows = @(Import-Csv $priorityReviewCsv)
if ($priorityRows.Count -eq 0)
{
    throw "优先复核表为空: $priorityReviewCsv"
}

Copy-Item $priorityReviewCsv (Join-Path $outputDirectory (Split-Path -Leaf $priorityReviewCsv)) -Force

$summaryPath = Join-Path $resultDirectory "batch-summary.json"
$priorityPackPath = Join-Path $resultDirectory "priority-review-pack.md"
$inlinePath = Join-Path $resultDirectory "priority-review-pack.inline.csv"

foreach ($sharedPath in @($summaryPath, $priorityPackPath, $inlinePath))
{
    if (Test-Path $sharedPath)
    {
        Copy-Item $sharedPath (Join-Path $outputDirectory (Split-Path -Leaf $sharedPath)) -Force
    }
}

$copiedMembers = 0
$copiedAssemblies = 0
$bundleLines = @(
    "# Priority Review Bundle",
    "",
    "This bundle contains the current `18` representative review samples.",
    "",
    "## Included",
    "",
    "- priority-review-pack.inline.csv: fill this first",
    "- priority-review-pack.md: grouped representative summary",
    "- batch-summary.json: batch-level summary",
    "- members\\: original xingcai member JSON files for the representative assemblies",
    "- assemblies\\: corresponding offline input and recognition JSON files",
    "",
    "## Fastest review path",
    "",
    '1. Fill `priority-review-pack.inline.csv`',
    '2. Run `Evaluate-PriorityReviewPack.ps1` on the filled CSV',
    '3. If needed, inspect the paired files in `members\\` and `assemblies\\`'
)

foreach ($row in $priorityRows)
{
    $assemblyId = [string]$row.AssemblyId
    $sourceFile = [string]$row.SourceFile
    if (-not [string]::IsNullOrWhiteSpace($sourceFile) -and (Test-Path $sourceFile))
    {
        Copy-Item $sourceFile (Join-Path $membersOutput (Split-Path -Leaf $sourceFile)) -Force
        $copiedMembers++
    }

    foreach ($suffix in @("input", "recognition"))
    {
        $source = Join-Path $resultDirectory ("assembly-{0}.{1}.json" -f $assemblyId, $suffix)
        if (Test-Path $source)
        {
            Copy-Item $source (Join-Path $assembliesOutput (Split-Path -Leaf $source)) -Force
            $copiedAssemblies++
        }
    }
}

$bundleLines += ""
$bundleLines += "## Copy Summary"
$bundleLines += ""
$bundleLines += "- Review rows: $($priorityRows.Count)"
$bundleLines += "- Member JSON copied: $copiedMembers"
$bundleLines += "- Assembly JSON copied: $copiedAssemblies"

$readmePath = Join-Path $outputDirectory "README.md"
Set-Content -Path $readmePath -Value $bundleLines -Encoding UTF8

[pscustomobject]@{
    OutputDirectory = $outputDirectory
    ReviewRows = $priorityRows.Count
    CopiedMemberJson = $copiedMembers
    CopiedAssemblyJson = $copiedAssemblies
    Readme = $readmePath
} | Format-List
