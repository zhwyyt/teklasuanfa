param(
    [Parameter(Mandatory = $true)]
    [string]$PriorityReviewCsv,

    [string]$ReviewQueueCsv,

    [string]$OutputMergedCsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$priorityReviewCsv = (Resolve-Path $PriorityReviewCsv).Path
$rows = @(Import-Csv $priorityReviewCsv)
if ($rows.Count -eq 0)
{
    throw "优先复核表为空: $PriorityReviewCsv"
}

function Normalize-YesNo([string]$value)
{
    if ([string]::IsNullOrWhiteSpace($value))
    {
        return ""
    }

    $normalized = ($value.Trim().ToLowerInvariant() -replace "[\s_\-\\/]+", "")
    switch ($normalized)
    {
        { $_ -in @("y", "yes", "true", "1", "是", "是的", "有", "有牛腿", "存在", "真", "对", "阳性", "tp") } { return "yes" }
        { $_ -in @("n", "no", "false", "0", "否", "没有", "无", "无牛腿", "不存在", "假", "阴性", "fp") } { return "no" }
        default { return "" }
    }
}

function Normalize-BodyFamily([string]$value)
{
    if ([string]::IsNullOrWhiteSpace($value))
    {
        return ""
    }

    $trimmed = $value.Trim()
    $normalized = ($trimmed.ToLowerInvariant() -replace "[\s_\-\\/]+", "")

    switch ($normalized)
    {
        { $_ -in @("unknown", "unk", "na", "未知", "未判定", "待定") } { return "Unknown" }
        { $_ -in @("builtuph", "h", "h型", "h形", "焊接h", "焊接h型", "焊接h形", "组合h", "bh", "hsection", "weldedh") } { return "BuiltUpH" }
        { $_ -in @("builtupbox", "box", "箱", "箱型", "箱形", "焊接箱", "焊接箱型", "焊接箱形", "组合箱", "组合箱型", "boxsection") } { return "BuiltUpBox" }
        { $_ -in @("builtupt", "t", "t型", "t形", "焊接t", "焊接t型", "焊接t形", "组合t") } { return "BuiltUpT" }
        { $_ -in @("builtupcross", "cross", "十字", "十字型", "十字形", "十型", "十形", "焊接十字", "组合十字") } { return "BuiltUpCross" }
        { $_ -in @("irregular", "异形", "异型", "不规则", "非标") } { return "Irregular" }
        default { return $trimmed }
    }
}

function Normalize-PartGroups([string]$value)
{
    if ([string]::IsNullOrWhiteSpace($value))
    {
        return ""
    }

    $normalized = $value.Trim()
    $normalized = $normalized -replace "[，、；;|/]+", ","
    $normalized = $normalized -replace "\s+", ""
    $normalized = $normalized -replace ",{2,}", ","
    return $normalized.Trim(",")
}

$labeledBracketRows = @($rows | Where-Object { Normalize-YesNo $_.HumanHasBracket })
$labeledBodyRows = @($rows | Where-Object { -not [string]::IsNullOrWhiteSpace($_.HumanBodyFamily) })
$labeledPartRows = @($rows | Where-Object { -not [string]::IsNullOrWhiteSpace($_.HumanBracketPartGroups) })

$tp = 0
$tn = 0
$fp = 0
$fn = 0

foreach ($row in $labeledBracketRows)
{
    $predictedPositive = [int]$row.PredictedBracketCount -gt 0
    $humanPositive = (Normalize-YesNo $row.HumanHasBracket) -eq "yes"

    if ($predictedPositive -and $humanPositive) { $tp++; continue }
    if ($predictedPositive -and -not $humanPositive) { $fp++; continue }
    if (-not $predictedPositive -and $humanPositive) { $fn++; continue }
    $tn++
}

$bodyCorrect = 0
foreach ($row in $labeledBodyRows)
{
    if ((Normalize-BodyFamily $row.PredictedBodyFamily) -eq (Normalize-BodyFamily $row.HumanBodyFamily))
    {
        $bodyCorrect++
    }
}

$partGroupExact = 0
foreach ($row in $labeledPartRows)
{
    if ((Normalize-PartGroups $row.PredictedBracketPartGroups) -eq (Normalize-PartGroups $row.HumanBracketPartGroups))
    {
        $partGroupExact++
    }
}

$precision = if (($tp + $fp) -eq 0) { 0.0 } else { $tp / ($tp + $fp) }
$recall = if (($tp + $fn) -eq 0) { 0.0 } else { $tp / ($tp + $fn) }
$accuracy = if (($tp + $tn + $fp + $fn) -eq 0) { 0.0 } else { ($tp + $tn) / ($tp + $tn + $fp + $fn) }
$f1 = if (($precision + $recall) -eq 0) { 0.0 } else { 2.0 * $precision * $recall / ($precision + $recall) }
$bodyAccuracy = if ($labeledBodyRows.Count -eq 0) { 0.0 } else { $bodyCorrect / $labeledBodyRows.Count }
$partExactRate = if ($labeledPartRows.Count -eq 0) { 0.0 } else { $partGroupExact / $labeledPartRows.Count }

$falsePositives = @(
    $labeledBracketRows |
        Where-Object { ([int]$_.PredictedBracketCount -gt 0) -and ((Normalize-YesNo $_.HumanHasBracket) -eq "no") } |
        Select-Object AssemblyId, PredictedBodyFamily, PredictedBracketCount, PredictedBracketPartGroups, SourceFile
)

$falseNegatives = @(
    $labeledBracketRows |
        Where-Object { ([int]$_.PredictedBracketCount -le 0) -and ((Normalize-YesNo $_.HumanHasBracket) -eq "yes") } |
        Select-Object AssemblyId, PredictedBodyFamily, PredictedBracketCount, HumanBracketPartGroups, SourceFile
)

$report = [pscustomobject]@{
    PriorityReviewCsv         = $priorityReviewCsv
    TotalRows                 = $rows.Count
    LabeledBracketRows        = $labeledBracketRows.Count
    LabeledBodyRows           = $labeledBodyRows.Count
    LabeledBracketPartRows    = $labeledPartRows.Count
    BracketTruePositive       = $tp
    BracketFalsePositive      = $fp
    BracketFalseNegative      = $fn
    BracketTrueNegative       = $tn
    BracketPrecision          = [math]::Round($precision, 4)
    BracketRecall             = [math]::Round($recall, 4)
    BracketF1                 = [math]::Round($f1, 4)
    BracketAccuracy           = [math]::Round($accuracy, 4)
    BodyFamilyAccuracy        = [math]::Round($bodyAccuracy, 4)
    BracketPartGroupExactRate = [math]::Round($partExactRate, 4)
    UnlabeledRows             = @($rows | Where-Object { [string]::IsNullOrWhiteSpace($_.HumanHasBracket) -and [string]::IsNullOrWhiteSpace($_.HumanBodyFamily) -and [string]::IsNullOrWhiteSpace($_.HumanBracketPartGroups) }).Count
}

$report | Format-List

if ($falsePositives.Count -gt 0)
{
    Write-Host ""
    Write-Host "False Positives:"
    $falsePositives | Format-Table -AutoSize
}

if ($falseNegatives.Count -gt 0)
{
    Write-Host ""
    Write-Host "False Negatives:"
    $falseNegatives | Format-Table -AutoSize
}

if (-not [string]::IsNullOrWhiteSpace($ReviewQueueCsv))
{
    $toolsDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
    $applyScript = Join-Path $toolsDirectory "Apply-PriorityReviewLabels.ps1"

    if ([string]::IsNullOrWhiteSpace($OutputMergedCsv))
    {
        $reviewQueueDirectory = Split-Path -Parent (Resolve-Path $ReviewQueueCsv).Path
        $OutputMergedCsv = Join-Path $reviewQueueDirectory "review-queue.merged.csv"
    }

    Write-Host ""
    Write-Host "Merging labels into review queue..."
    Write-Host ""

    & $applyScript `
        -PriorityReviewCsv $PriorityReviewCsv `
        -ReviewQueueCsv $ReviewQueueCsv `
        -OutputCsv $OutputMergedCsv
}
