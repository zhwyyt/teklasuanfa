param(
    [Parameter(Mandatory = $true)]
    [string]$P1BracketAnswersCsv,

    [Parameter(Mandatory = $true)]
    [string]$ResultDirectory,

    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$p1BracketAnswersCsv = (Resolve-Path $P1BracketAnswersCsv).Path
$resultDirectory = (Resolve-Path $ResultDirectory).Path

if ([string]::IsNullOrWhiteSpace($OutputDirectory))
{
    $OutputDirectory = Join-Path $resultDirectory "p1-review-bundle"
}

$outputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
$membersOutput = Join-Path $outputDirectory "members"
$assembliesOutput = Join-Path $outputDirectory "assemblies"

New-Item -ItemType Directory -Force -Path $outputDirectory, $membersOutput, $assembliesOutput | Out-Null

$rows = @(Import-Csv $p1BracketAnswersCsv)
if ($rows.Count -eq 0)
{
    throw "P1 bracket answers CSV is empty: $p1BracketAnswersCsv"
}

$rowCount = $rows.Count
$answersLeaf = Split-Path -Leaf $p1BracketAnswersCsv

Copy-Item $p1BracketAnswersCsv (Join-Path $outputDirectory (Split-Path -Leaf $p1BracketAnswersCsv)) -Force

$sharedPaths = @(
    (Join-Path $resultDirectory "batch-summary.json"),
    (Join-Path $resultDirectory "priority-review-pack.inline.csv"),
    (Join-Path $resultDirectory "priority-bracket-answers.p1.csv")
)

foreach ($sharedPath in $sharedPaths)
{
    if (Test-Path $sharedPath)
    {
        Copy-Item $sharedPath (Join-Path $outputDirectory (Split-Path -Leaf $sharedPath)) -Force
    }
}

$copiedMembers = 0
$copiedAssemblies = 0
foreach ($row in $rows)
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

$gkzRows = @($rows | Where-Object { [string]$_.SourceFile -like "*GKZ*" })
$builtUpTRows = @($rows | Where-Object { [string]$_.PredictedBodyFamily -eq "BuiltUpT" })

$toolsDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$summaryScript = Join-Path $toolsDirectory "Build-P1ReviewSummary.ps1"
& $summaryScript `
    -P1BracketAnswersCsv $p1BracketAnswersCsv `
    -ResultDirectory $resultDirectory `
    -OutputDirectory $outputDirectory | Out-Null

$bundleLines = @(
    "# P1 Review Bundle",
    "",
    ('This bundle contains the top `{0}` bracket-critical samples only.' -f $rowCount),
    "",
    "## Included",
    "",
    "- ${answersLeaf}: fill this first",
    "- priority-review-pack.inline.csv: full 18-row reference sheet",
    "- p1-review-summary.csv / p1-review-summary.md / p1-review-summary.zh-CN.md: condensed comparison table for the current P1 subset",
    "- batch-summary.json: batch-level summary",
    "- members\\: original xingcai member JSON files for the current P1 subset",
    "- assemblies\\: corresponding offline input and recognition JSON files",
    "",
    "## Fastest review path",
    "",
    ('1. Fill `{0}`' -f $answersLeaf),
    '2. Run `Evaluate-P1BracketAnswers.ps1` on the filled CSV',
    '3. If needed, inspect the paired files in `members\\` and `assemblies\\`',
    "",
    "## Current P1 Split",
    "",
    "- GKZ positive-priority rows: $($gkzRows.Count)",
    "- BuiltUpT hard-negative rows: $($builtUpTRows.Count)",
    "",
    "## Copy Summary",
    "",
    "- Review rows: $($rows.Count)",
    "- Member JSON copied: $copiedMembers",
    "- Assembly JSON copied: $copiedAssemblies"
)

$readmePath = Join-Path $outputDirectory "README.md"
Set-Content -Path $readmePath -Value $bundleLines -Encoding UTF8

[pscustomobject]@{
    OutputDirectory = $outputDirectory
    ReviewRows = $rows.Count
    CopiedMemberJson = $copiedMembers
    CopiedAssemblyJson = $copiedAssemblies
    Readme = $readmePath
} | Format-List
