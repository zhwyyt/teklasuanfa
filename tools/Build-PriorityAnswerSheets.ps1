param(
    [Parameter(Mandatory = $true)]
    [string]$PriorityReviewCsv,

    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$priorityReviewCsv = (Resolve-Path $PriorityReviewCsv).Path
if ([string]::IsNullOrWhiteSpace($OutputDirectory))
{
    $OutputDirectory = Split-Path -Parent $priorityReviewCsv
}

$outputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$rows = @(Import-Csv $priorityReviewCsv)
if ($rows.Count -eq 0)
{
    throw "Priority review CSV is empty: $priorityReviewCsv"
}

$bracketRows = @(
    $rows |
        Where-Object { [string]$_.Reasons -like "*BracketPattern*" } |
        Sort-Object AssemblyId |
        ForEach-Object {
            [pscustomobject]@{
                AssemblyId = [string]$_.AssemblyId
                SourceFile = [string]$_.SourceFile
                PredictedBodyFamily = [string]$_.PredictedBodyFamily
                PredictedBracketCount = [string]$_.PredictedBracketCount
                PredictedBracketPartGroups = [string]$_.PredictedBracketPartGroups
                HumanHasBracket = [string]$_.HumanHasBracket
                HumanBracketPartGroups = [string]$_.HumanBracketPartGroups
                HumanBodyFamily = [string]$_.HumanBodyFamily
                HumanVerdict = [string]$_.HumanVerdict
                Notes = [string]$_.Notes
            }
        }
)

$bodyRows = @(
    $rows |
        Where-Object { [string]$_.Reasons -eq "BodyChange" } |
        Sort-Object AssemblyId |
        ForEach-Object {
            [pscustomobject]@{
                AssemblyId = [string]$_.AssemblyId
                SourceFile = [string]$_.SourceFile
                PredictedBodyFamily = [string]$_.PredictedBodyFamily
                BodyFamilyOrChange = [string]$_.BodyFamilyOrChange
                HumanBodyFamily = [string]$_.HumanBodyFamily
                HumanVerdict = [string]$_.HumanVerdict
                Notes = [string]$_.Notes
            }
        }
)

$bracketPath = Join-Path $outputDirectory "priority-bracket-answers.csv"
$bodyPath = Join-Path $outputDirectory "priority-body-answers.csv"

$bracketRows | Export-Csv -Path $bracketPath -NoTypeInformation -Encoding UTF8 -Force
$bodyRows | Export-Csv -Path $bodyPath -NoTypeInformation -Encoding UTF8 -Force

[pscustomobject]@{
    PriorityReviewCsv = $priorityReviewCsv
    OutputDirectory = $outputDirectory
    BracketAnswerSheet = $bracketPath
    BracketRows = $bracketRows.Count
    BodyAnswerSheet = $bodyPath
    BodyRows = $bodyRows.Count
} | Format-List
