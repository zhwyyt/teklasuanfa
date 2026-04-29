param(
    [Parameter(Mandatory = $true)]
    [string]$ResultDirectory,

    [string]$BaselineResultDirectory,

    [string]$MemberCacheDirectory,

    [switch]$IncludeZeroBracketAssemblies
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$resultDirectory = (Resolve-Path $ResultDirectory).Path
if ([string]::IsNullOrWhiteSpace($BaselineResultDirectory))
{
    $BaselineResultDirectory = $resultDirectory
}
$baselineResultDirectory = (Resolve-Path $BaselineResultDirectory).Path

if (-not [string]::IsNullOrWhiteSpace($MemberCacheDirectory))
{
    $MemberCacheDirectory = (Resolve-Path $MemberCacheDirectory).Path
}

$toolsDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path

$newReviewQueueScript = Join-Path $toolsDirectory "New-ReviewQueue.ps1"
$summarizeReviewQueueScript = Join-Path $toolsDirectory "Summarize-ReviewQueue.ps1"
$buildPatternBucketSummaryScript = Join-Path $toolsDirectory "Build-PatternBucketSummary.ps1"
$buildTopologyRewriteSummaryScript = Join-Path $toolsDirectory "Build-TopologyRewriteSummary.ps1"
$compareRunsScript = Join-Path $toolsDirectory "Compare-RecognitionRuns.ps1"
$summarizeBodyChangesScript = Join-Path $toolsDirectory "Summarize-BodyChanges.ps1"
$buildPriorityPackScript = Join-Path $toolsDirectory "Build-PriorityReviewPack.ps1"
$buildPriorityCsvScript = Join-Path $toolsDirectory "Build-PriorityReviewCsv.ps1"
$buildPriorityAnswerSheetsScript = Join-Path $toolsDirectory "Build-PriorityAnswerSheets.ps1"
$buildPriorityChecklistScript = Join-Path $toolsDirectory "Build-PriorityReviewChecklist.ps1"
$buildPriorityBundleScript = Join-Path $toolsDirectory "Build-PriorityReviewBundle.ps1"
$buildP1AnswerSheetScript = Join-Path $toolsDirectory "Build-P1BracketAnswerSheet.ps1"
$buildP1BundleScript = Join-Path $toolsDirectory "Build-P1ReviewBundle.ps1"

$reviewQueueArgs = @{
    ResultDirectory = $resultDirectory
}
if ($IncludeZeroBracketAssemblies)
{
    $reviewQueueArgs.IncludeZeroBracketAssemblies = $true
}

& $newReviewQueueScript @reviewQueueArgs

$reviewQueueCsv = Join-Path $resultDirectory "review-queue.csv"
& $summarizeReviewQueueScript -ReviewQueueCsv $reviewQueueCsv
& $buildPatternBucketSummaryScript -ReviewQueueCsv $reviewQueueCsv

$coreBodyProofRealInputJson = Join-Path $resultDirectory "core-body-proof-real-input.json"
if ((Test-Path $buildTopologyRewriteSummaryScript) -and (Test-Path $coreBodyProofRealInputJson))
{
    & $buildTopologyRewriteSummaryScript `
        -CoreBodyProofRealInputJson $coreBodyProofRealInputJson `
        -OutputMarkdown (Join-Path $resultDirectory "topology-rewrite-summary.zh-CN.md")
}

& $compareRunsScript `
    -BaselineResultDirectory $baselineResultDirectory `
    -CandidateResultDirectory $resultDirectory

$bodyArgs = @{
    BaselineResultDirectory = $baselineResultDirectory
    CandidateResultDirectory = $resultDirectory
}
if (-not [string]::IsNullOrWhiteSpace($MemberCacheDirectory))
{
    $bodyArgs.MemberCacheDirectory = $MemberCacheDirectory
}
& $summarizeBodyChangesScript @bodyArgs

$priorityPackArgs = @{
    ReviewQueueCsv = $reviewQueueCsv
    BaselineResultDirectory = $baselineResultDirectory
    CandidateResultDirectory = $resultDirectory
}
if (-not [string]::IsNullOrWhiteSpace($MemberCacheDirectory))
{
    $priorityPackArgs.MemberCacheDirectory = $MemberCacheDirectory
}
& $buildPriorityPackScript @priorityPackArgs

& $buildPriorityCsvScript -ResultDirectory $resultDirectory

$priorityReviewCsv = Join-Path $resultDirectory "priority-review-pack.inline.csv"
& $buildPriorityAnswerSheetsScript -PriorityReviewCsv $priorityReviewCsv
& $buildPriorityChecklistScript -PriorityReviewCsv $priorityReviewCsv
& $buildPriorityBundleScript -PriorityReviewCsv $priorityReviewCsv -ResultDirectory $resultDirectory

$priorityBracketAnswersCsv = Join-Path $resultDirectory "priority-bracket-answers.csv"
& $buildP1AnswerSheetScript `
    -PriorityBracketAnswersCsv $priorityBracketAnswersCsv `
    -ReviewQueueCsv $reviewQueueCsv

$p1BracketAnswersCsv = Join-Path $resultDirectory "priority-bracket-answers.p1.csv"
$p1BundleDirectory = Join-Path $resultDirectory "p1-review-bundle"
$p1Rows = @()
if (Test-Path $p1BracketAnswersCsv)
{
    $p1Rows = @(Import-Csv $p1BracketAnswersCsv)
}

if ($p1Rows.Count -gt 0)
{
    & $buildP1BundleScript -P1BracketAnswersCsv $p1BracketAnswersCsv -ResultDirectory $resultDirectory
}

[pscustomobject]@{
    ResultDirectory = $resultDirectory
    BaselineResultDirectory = $baselineResultDirectory
    ReviewQueueCsv = $reviewQueueCsv
    PatternBucketSummary = (Join-Path $resultDirectory "pattern-bucket-summary.md")
    TopologyRewriteSummary = (Join-Path $resultDirectory "topology-rewrite-summary.zh-CN.md")
    PriorityReviewCsv = $priorityReviewCsv
    PriorityBracketAnswersCsv = $priorityBracketAnswersCsv
    P1BracketAnswersCsv = $p1BracketAnswersCsv
    PriorityBundle = (Join-Path $resultDirectory "priority-review-bundle")
    P1Bundle = $(if ($p1Rows.Count -gt 0) { $p1BundleDirectory } else { "" })
    P1RowCount = $p1Rows.Count
} | Format-List
