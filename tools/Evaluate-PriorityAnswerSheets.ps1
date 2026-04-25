param(
    [Parameter(Mandatory = $true)]
    [string]$PriorityReviewCsv,

    [Parameter(Mandatory = $true)]
    [string]$BracketAnswersCsv,

    [Parameter(Mandatory = $true)]
    [string]$BodyAnswersCsv,

    [string]$OutputMergedPriorityCsv,

    [string]$ReviewQueueCsv,

    [string]$OutputMergedReviewQueueCsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$toolsDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$mergeScript = Join-Path $toolsDirectory "Merge-PriorityAnswerSheets.ps1"
$evaluateScript = Join-Path $toolsDirectory "Evaluate-PriorityReviewPack.ps1"

if ([string]::IsNullOrWhiteSpace($OutputMergedPriorityCsv))
{
    $outputDirectory = Split-Path -Parent (Resolve-Path $PriorityReviewCsv).Path
    $OutputMergedPriorityCsv = Join-Path $outputDirectory "priority-review-pack.merged.csv"
}

& $mergeScript `
    -PriorityReviewCsv $PriorityReviewCsv `
    -BracketAnswersCsv $BracketAnswersCsv `
    -BodyAnswersCsv $BodyAnswersCsv `
    -OutputCsv $OutputMergedPriorityCsv

Write-Host ""
Write-Host "Merged priority review sheet created. Evaluating..."
Write-Host ""

if ([string]::IsNullOrWhiteSpace($ReviewQueueCsv))
{
    & $evaluateScript -PriorityReviewCsv $OutputMergedPriorityCsv
}
else
{
    if ([string]::IsNullOrWhiteSpace($OutputMergedReviewQueueCsv))
    {
        $reviewQueueDirectory = Split-Path -Parent (Resolve-Path $ReviewQueueCsv).Path
        $OutputMergedReviewQueueCsv = Join-Path $reviewQueueDirectory "review-queue.merged.csv"
    }

    & $evaluateScript `
        -PriorityReviewCsv $OutputMergedPriorityCsv `
        -ReviewQueueCsv $ReviewQueueCsv `
        -OutputMergedCsv $OutputMergedReviewQueueCsv
}
