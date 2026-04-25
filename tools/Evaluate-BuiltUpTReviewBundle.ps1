param(
    [string]$ResultDirectory,

    [string]$BuiltUpTBracketAnswersCsv,

    [string]$PriorityReviewCsv,

    [string]$ReviewQueueCsv,

    [string]$OutputMergedBracketCsv,

    [string]$OutputMergedPriorityCsv,

    [string]$OutputMergedReviewQueueCsv,

    [string]$OutputPatternDecisionMarkdown,

    [string]$OutputPatternDecisionCsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$isPackageRoot = (Test-Path (Join-Path $root "results")) -and (Test-Path (Join-Path $root "tools"))
$isWorkspaceRoot = Test-Path (Join-Path $root ".tmpresults")

if ([string]::IsNullOrWhiteSpace($ResultDirectory))
{
    if ($isPackageRoot)
    {
        $ResultDirectory = Join-Path $root "results"
    }
    elseif ($isWorkspaceRoot)
    {
        $ResultDirectory = Join-Path $root ".tmpresults\run_body_bracket_real_01_workflow"
    }
    else
    {
        throw "ResultDirectory is required when the script is not run from a recognized workspace/package root."
    }
}

$resultDirectory = (Resolve-Path $ResultDirectory).Path

if ([string]::IsNullOrWhiteSpace($BuiltUpTBracketAnswersCsv))
{
    $BuiltUpTBracketAnswersCsv = Join-Path $resultDirectory "priority-bracket-answers.p1.builtupt-first.csv"
}

if ([string]::IsNullOrWhiteSpace($PriorityReviewCsv))
{
    $PriorityReviewCsv = Join-Path $resultDirectory "priority-review-pack.inline.csv"
}

if ([string]::IsNullOrWhiteSpace($ReviewQueueCsv))
{
    $ReviewQueueCsv = Join-Path $resultDirectory "review-queue.csv"
}

if ([string]::IsNullOrWhiteSpace($OutputMergedBracketCsv))
{
    $OutputMergedBracketCsv = Join-Path $resultDirectory "priority-bracket-answers.p1.builtupt-first.merged.csv"
}

if ([string]::IsNullOrWhiteSpace($OutputMergedPriorityCsv))
{
    $OutputMergedPriorityCsv = Join-Path $resultDirectory "priority-review-pack.p1.builtupt-first.merged.csv"
}

if ([string]::IsNullOrWhiteSpace($OutputMergedReviewQueueCsv))
{
    $OutputMergedReviewQueueCsv = Join-Path $resultDirectory "review-queue.p1.builtupt-first.merged.csv"
}

if ([string]::IsNullOrWhiteSpace($OutputPatternDecisionMarkdown))
{
    $OutputPatternDecisionMarkdown = Join-Path $resultDirectory "builtupt-pattern-decision-summary.md"
}

if ([string]::IsNullOrWhiteSpace($OutputPatternDecisionCsv))
{
    $OutputPatternDecisionCsv = Join-Path $resultDirectory "builtupt-pattern-decision-summary.csv"
}

$evaluateScript = Join-Path $PSScriptRoot "Evaluate-P1BracketAnswers.ps1"

& $evaluateScript `
    -PriorityReviewCsv $PriorityReviewCsv `
    -P1BracketAnswersCsv $BuiltUpTBracketAnswersCsv `
    -ReviewQueueCsv $ReviewQueueCsv `
    -OutputMergedBracketCsv $OutputMergedBracketCsv `
    -OutputMergedPriorityCsv $OutputMergedPriorityCsv `
    -OutputMergedReviewQueueCsv $OutputMergedReviewQueueCsv `
    -OutputPatternDecisionMarkdown $OutputPatternDecisionMarkdown `
    -OutputPatternDecisionCsv $OutputPatternDecisionCsv
