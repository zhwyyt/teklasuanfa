param(
    [Parameter(Mandatory = $true)]
    [string]$PriorityReviewCsv,

    [string]$OutputMarkdown
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$priorityReviewCsv = (Resolve-Path $PriorityReviewCsv).Path
if ([string]::IsNullOrWhiteSpace($OutputMarkdown))
{
    $outputDirectory = Split-Path -Parent $priorityReviewCsv
    $OutputMarkdown = Join-Path $outputDirectory "priority-review-checklist.md"
}

$rows = @(Import-Csv $priorityReviewCsv)
if ($rows.Count -eq 0)
{
    throw "优先复核表为空: $priorityReviewCsv"
}

$bracketRows = @($rows | Where-Object { [string]$_.Reasons -like '*BracketPattern*' } | Sort-Object AssemblyId)
$bodyOnlyRows = @($rows | Where-Object { [string]$_.Reasons -eq 'BodyChange' } | Sort-Object AssemblyId)

$lines = @(
    "# Priority Review Checklist",
    "",
    "- Total representative assemblies: $($rows.Count)",
    "- Bracket-first checks: $($bracketRows.Count)",
    "- Body-only checks: $($bodyOnlyRows.Count)",
    "",
    "## Bracket-First Checks",
    "",
    "For these rows, the fastest useful answer is: `HumanHasBracket = yes/no`.",
    "If known, also fill `HumanBracketPartGroups` and `HumanBodyFamily`.",
    "",
    "| AssemblyId | PredictedBody | PredictedBracketCount | SourceFile |",
    "| --- | --- | --- | --- |"
)

foreach ($row in $bracketRows)
{
    $lines += "| $($row.AssemblyId) | $($row.PredictedBodyFamily) | $($row.PredictedBracketCount) | $($row.SourceFile) |"
}

$lines += ""
$lines += "## Body-Only Checks"
$lines += ""
$lines += "For these rows, the fastest useful answer is: `HumanBodyFamily = ...`."
$lines += "You can leave `HumanHasBracket` empty if you are only checking body-family changes."
$lines += ""
$lines += "| AssemblyId | PredictedBody | BodyFamilyOrChange | SourceFile |"
$lines += "| --- | --- | --- | --- |"

foreach ($row in $bodyOnlyRows)
{
    $lines += "| $($row.AssemblyId) | $($row.PredictedBodyFamily) | $($row.BodyFamilyOrChange) | $($row.SourceFile) |"
}

$lines += ""
$lines += "## Suggested Next Export If You Prefer New Data"
$lines += ""
$lines += "- `6-10` confirmed bracket-positive samples, prioritizing `GKZ` compact plate-pair appendages"
$lines += "- `6-10` confirmed hard negatives, prioritizing `GL / BuiltUpT` angle-steel plus short-plate clusters"

Set-Content -Path $OutputMarkdown -Value $lines -Encoding UTF8

[pscustomobject]@{
    PriorityReviewCsv = $priorityReviewCsv
    OutputMarkdown = $OutputMarkdown
    TotalRows = $rows.Count
    BracketRows = $bracketRows.Count
    BodyOnlyRows = $bodyOnlyRows.Count
} | Format-List
