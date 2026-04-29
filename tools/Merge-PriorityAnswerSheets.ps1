param(
    [Parameter(Mandatory = $true)]
    [string]$PriorityReviewCsv,

    [Parameter(Mandatory = $true)]
    [string]$BracketAnswersCsv,

    [Parameter(Mandatory = $true)]
    [string]$BodyAnswersCsv,

    [string]$OutputCsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$priorityReviewCsv = (Resolve-Path $PriorityReviewCsv).Path
$bracketAnswersCsv = (Resolve-Path $BracketAnswersCsv).Path
$bodyAnswersCsv = (Resolve-Path $BodyAnswersCsv).Path

if ([string]::IsNullOrWhiteSpace($OutputCsv))
{
    $outputDirectory = Split-Path -Parent $priorityReviewCsv
    $OutputCsv = Join-Path $outputDirectory "priority-review-pack.merged.csv"
}

$priorityRows = @(Import-Csv $priorityReviewCsv)
$bracketRows = @(Import-Csv $bracketAnswersCsv)
$bodyRows = @(Import-Csv $bodyAnswersCsv)

$bracketLookup = @{}
foreach ($row in $bracketRows)
{
    $assemblyId = [string]$row.AssemblyId
    if (-not [string]::IsNullOrWhiteSpace($assemblyId))
    {
        $bracketLookup[$assemblyId] = $row
    }
}

$bodyLookup = @{}
foreach ($row in $bodyRows)
{
    $assemblyId = [string]$row.AssemblyId
    if (-not [string]::IsNullOrWhiteSpace($assemblyId))
    {
        $bodyLookup[$assemblyId] = $row
    }
}

$mergedRows = foreach ($row in $priorityRows)
{
    $assemblyId = [string]$row.AssemblyId
    $bracket = if ($bracketLookup.ContainsKey($assemblyId)) { $bracketLookup[$assemblyId] } else { $null }
    $body = if ($bodyLookup.ContainsKey($assemblyId)) { $bodyLookup[$assemblyId] } else { $null }

    if ($null -ne $bracket)
    {
        foreach ($column in @("HumanHasBracket", "HumanBracketPartGroups", "HumanBodyFamily", "HumanVerdict", "Notes"))
        {
            $value = [string]$bracket.$column
            if (-not [string]::IsNullOrWhiteSpace($value))
            {
                $row.$column = $value.Trim()
            }
        }
    }

    if ($null -ne $body)
    {
        foreach ($column in @("HumanBodyFamily", "HumanVerdict", "Notes"))
        {
            $value = [string]$body.$column
            if (-not [string]::IsNullOrWhiteSpace($value))
            {
                $row.$column = $value.Trim()
            }
        }
    }

    $row
}

$mergedRows | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8 -Force

[pscustomobject]@{
    PriorityReviewCsv = $priorityReviewCsv
    BracketAnswersCsv = $bracketAnswersCsv
    BodyAnswersCsv = $bodyAnswersCsv
    OutputCsv = $OutputCsv
    PriorityRows = $priorityRows.Count
    BracketRows = $bracketRows.Count
    BodyRows = $bodyRows.Count
} | Format-List
