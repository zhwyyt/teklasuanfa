param(
    [Parameter(Mandatory = $true)]
    [string]$PriorityReviewCsv,

    [Parameter(Mandatory = $true)]
    [string]$ReviewQueueCsv,

    [string]$OutputCsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$priorityReviewCsv = (Resolve-Path $PriorityReviewCsv).Path
$reviewQueueCsv = (Resolve-Path $ReviewQueueCsv).Path

if ([string]::IsNullOrWhiteSpace($OutputCsv))
{
    $reviewQueueDirectory = Split-Path -Parent $reviewQueueCsv
    $OutputCsv = Join-Path $reviewQueueDirectory "review-queue.merged.csv"
}

$priorityRows = @(Import-Csv $priorityReviewCsv)
$reviewRows = @(Import-Csv $reviewQueueCsv)

if ($priorityRows.Count -eq 0)
{
    throw "优先复核表为空: $priorityReviewCsv"
}

if ($reviewRows.Count -eq 0)
{
    throw "评审队列为空: $reviewQueueCsv"
}

$priorityLookup = @{}
foreach ($row in $priorityRows)
{
    $assemblyId = [string]$row.AssemblyId
    if (-not [string]::IsNullOrWhiteSpace($assemblyId))
    {
        $priorityLookup[$assemblyId] = $row
    }
}

$copiedLabelCells = 0
$matchedAssemblies = 0
$mergedRows = foreach ($row in $reviewRows)
{
    $assemblyId = [string]$row.AssemblyId
    if (-not $priorityLookup.ContainsKey($assemblyId))
    {
        $row
        continue
    }

    $matchedAssemblies++
    $labelRow = $priorityLookup[$assemblyId]

    foreach ($column in @("HumanHasBracket", "HumanBodyFamily", "HumanBracketPartGroups", "HumanVerdict", "Notes"))
    {
        $value = [string]$labelRow.$column
        if (-not [string]::IsNullOrWhiteSpace($value))
        {
            $row.$column = $value.Trim()
            $copiedLabelCells++
        }
    }

    $row
}

$mergedRows | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8

[pscustomobject]@{
    PriorityReviewCsv = $priorityReviewCsv
    ReviewQueueCsv = $reviewQueueCsv
    OutputCsv = $OutputCsv
    PriorityRows = $priorityRows.Count
    ReviewQueueRows = $reviewRows.Count
    MatchedAssemblies = $matchedAssemblies
    CopiedLabelCells = $copiedLabelCells
} | Format-List
