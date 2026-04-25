param(
    [Parameter(Mandatory = $true)]
    [string]$BuiltUpTBracketAnswersCsv,

    [Parameter(Mandatory = $true)]
    [string]$ResultDirectory,

    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$builtUpTBracketAnswersCsv = (Resolve-Path $BuiltUpTBracketAnswersCsv).Path
$resultDirectory = (Resolve-Path $ResultDirectory).Path

if ([string]::IsNullOrWhiteSpace($OutputDirectory))
{
    $OutputDirectory = Join-Path $resultDirectory "builtupt-review-bundle"
}

$outputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
$toolsDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$buildP1ReviewBundleScript = Join-Path $toolsDirectory "Build-P1ReviewBundle.ps1"

& $buildP1ReviewBundleScript `
    -P1BracketAnswersCsv $builtUpTBracketAnswersCsv `
    -ResultDirectory $resultDirectory `
    -OutputDirectory $outputDirectory | Out-Null

$rows = @(Import-Csv $builtUpTBracketAnswersCsv)
$answersLeaf = Split-Path -Leaf $builtUpTBracketAnswersCsv
$readmePath = Join-Path $outputDirectory "README.md"
$readmeLines = @(
    "# BuiltUpT Review Bundle",
    "",
    ('This bundle contains the `{0}` BuiltUpT hard-negative-priority samples only.' -f $rows.Count),
    "",
    "## Included",
    "",
    "- ${answersLeaf}: fill this first",
    "- priority-review-pack.inline.csv: full 18-row reference sheet",
    "- p1-review-summary.csv / p1-review-summary.md / p1-review-summary.zh-CN.md: condensed comparison table for the BuiltUpT subset",
    "- batch-summary.json: batch-level summary",
    "- members\\: original xingcai member JSON files for the BuiltUpT subset",
    "- assemblies\\: corresponding offline input and recognition JSON files",
    "",
    "## Fastest review path",
    "",
    ('1. Fill `{0}`' -f $answersLeaf),
    '2. Run `Evaluate-P1BracketAnswers.ps1` on the filled CSV',
    '3. If needed, inspect the paired files in `members\\` and `assemblies\\`',
    "",
    "## Current Focus",
    "",
    '- pattern bucket: `angle_short_plate_cluster`',
    '- objective: decide whether this narrow `BuiltUpT` bucket can be safely suppressed',
    "",
    "## Copy Summary",
    "",
    ("- Review rows: {0}" -f $rows.Count),
    ("- Assembly ids: {0}" -f ((@($rows | Select-Object -ExpandProperty AssemblyId) -join ", ")))
)

Set-Content -Path $readmePath -Value $readmeLines -Encoding UTF8

[pscustomobject]@{
    OutputDirectory = $outputDirectory
    ReviewRows = $rows.Count
    Readme = $readmePath
} | Format-List
