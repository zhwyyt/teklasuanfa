param(
    [Parameter(Mandatory = $true)]
    [string]$PriorityBracketAnswersCsv,

    [string]$ReviewQueueCsv,

    [string]$P1RequestCsv,

    [string]$OutputCsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Decode-HtmlText {
    param([string]$Value)

    return [System.Net.WebUtility]::HtmlDecode($Value)
}

function Get-P1PatternInfo {
    param(
        [string]$SourceFile,
        [string]$PredictedBodyFamily,
        [string]$PredictedBracketPartGroups,
        [string]$PredictedBracketProfiles
    )

    $partCount = 0
    if (-not [string]::IsNullOrWhiteSpace($PredictedBracketPartGroups))
    {
        $partCount = @(
            $PredictedBracketPartGroups.Split(",", [System.StringSplitOptions]::RemoveEmptyEntries) |
                ForEach-Object { $_.Trim() } |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        ).Count
    }

    if ($SourceFile -like "*GKZ*" -and $PredictedBodyFamily -eq "BuiltUpH" -and $partCount -eq 2)
    {
        return [pscustomobject]@{
            PatternHint = "compact_gkz_plate_pair"
            ReviewPromptZh = Decode-HtmlText "&#20248;&#20808;&#30830;&#35748;&#65306;&#36825;&#20004;&#29255; GKZ &#26495;&#26159;&#21542;&#26500;&#25104;&#30495;&#27491;&#22806;&#25361;&#29275;&#33151;&#65292;&#32780;&#19981;&#26159;&#26222;&#36890;&#23616;&#37096;&#26495;&#23545;"
        }
    }

    if ($PredictedBodyFamily -eq "BuiltUpT" -and
        $partCount -ge 4 -and
        $PredictedBracketProfiles -like "*CC52.5-4-15-34*" -and
        $PredictedBracketProfiles -like "*PL12*130*")
    {
        return [pscustomobject]@{
            PatternHint = "angle_short_plate_cluster"
            ReviewPromptZh = Decode-HtmlText "&#20248;&#20808;&#30830;&#35748;&#65306;&#36825;&#32452;&#35282;&#38050;/&#30701;&#26495;&#31751;&#26159;&#21542;&#21482;&#26159;&#23616;&#37096;&#38468;&#20214;&#65292;&#32780;&#19981;&#26159;&#29275;&#33151;"
        }
    }

    return [pscustomobject]@{
        PatternHint = "manual_shape_check"
        ReviewPromptZh = Decode-HtmlText "&#20248;&#20808;&#30830;&#35748;&#65306;&#36825;&#32452;&#20505;&#36873;&#20214;&#26159;&#21542;&#24418;&#25104;&#29420;&#31435;&#22806;&#25361;&#29275;&#33151;"
    }
}

function Get-PreferredString {
    param(
        [object]$PrimaryRow,
        [object]$FallbackRow,
        [string]$PropertyName
    )

    if ($null -ne $PrimaryRow -and $PrimaryRow.PSObject.Properties.Name -contains $PropertyName)
    {
        $primaryValue = [string]$PrimaryRow.$PropertyName
        if (-not [string]::IsNullOrWhiteSpace($primaryValue))
        {
            return $primaryValue
        }
    }

    if ($null -ne $FallbackRow -and $FallbackRow.PSObject.Properties.Name -contains $PropertyName)
    {
        return [string]$FallbackRow.$PropertyName
    }

    return ""
}

$priorityBracketAnswersCsv = (Resolve-Path $PriorityBracketAnswersCsv).Path
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

if (-not [string]::IsNullOrWhiteSpace($ReviewQueueCsv))
{
    $ReviewQueueCsv = (Resolve-Path $ReviewQueueCsv).Path
}

if ([string]::IsNullOrWhiteSpace($P1RequestCsv))
{
    $rootCandidate = Join-Path $repoRoot "NEXT_EXPORT_REQUEST_P1_ONLY.csv"
    $docsCandidate = Join-Path $repoRoot "docs\NEXT_EXPORT_REQUEST_P1_ONLY.csv"
    if (Test-Path $rootCandidate)
    {
        $P1RequestCsv = $rootCandidate
    }
    elseif (Test-Path $docsCandidate)
    {
        $P1RequestCsv = $docsCandidate
    }
    else
    {
        $P1RequestCsv = $rootCandidate
    }
}

$p1RequestCsv = (Resolve-Path $P1RequestCsv).Path

if ([string]::IsNullOrWhiteSpace($OutputCsv))
{
    $outputDirectory = Split-Path -Parent $priorityBracketAnswersCsv
    $OutputCsv = Join-Path $outputDirectory "priority-bracket-answers.p1.csv"
}

$priorityRows = @(Import-Csv $priorityBracketAnswersCsv)
$requestRows = @(Import-Csv $p1RequestCsv)
$reviewRows = @()
if (-not [string]::IsNullOrWhiteSpace($ReviewQueueCsv))
{
    $reviewRows = @(Import-Csv $ReviewQueueCsv)
}

if ($priorityRows.Count -eq 0)
{
    throw "Priority bracket answers CSV is empty: $priorityBracketAnswersCsv"
}

if ($requestRows.Count -eq 0)
{
    throw "P1 request CSV is empty: $p1RequestCsv"
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

$reviewLookup = @{}
foreach ($row in $reviewRows)
{
    $assemblyId = [string]$row.AssemblyId
    if (-not [string]::IsNullOrWhiteSpace($assemblyId))
    {
        $reviewLookup[$assemblyId] = $row
    }
}

$outputRows = foreach ($requestRow in $requestRows)
{
    $assemblyId = [string]$requestRow.AssemblyId
    $source = $null
    if ($priorityLookup.ContainsKey($assemblyId))
    {
        $source = $priorityLookup[$assemblyId]
    }
    elseif ($reviewLookup.ContainsKey($assemblyId))
    {
        $source = $reviewLookup[$assemblyId]
    }

    if ($null -eq $source)
    {
        continue
    }

    $reviewSource = $null
    if ($reviewLookup.ContainsKey($assemblyId))
    {
        $reviewSource = $reviewLookup[$assemblyId]
    }

    $sourceFile = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "SourceFile"
    $predictedBodyFamily = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "PredictedBodyFamily"
    $predictedBracketCount = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "PredictedBracketCount"
    $predictedBracketPartGroups = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "PredictedBracketPartGroups"
    $predictedBracketProfiles = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "PredictedBracketProfiles"
    $humanHasBracket = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "HumanHasBracket"
    $humanBracketPartGroups = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "HumanBracketPartGroups"
    $humanBodyFamily = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "HumanBodyFamily"
    $humanVerdict = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "HumanVerdict"
    $notes = Get-PreferredString -PrimaryRow $source -FallbackRow $reviewSource -PropertyName "Notes"

    $patternInfo = Get-P1PatternInfo `
        -SourceFile $sourceFile `
        -PredictedBodyFamily $predictedBodyFamily `
        -PredictedBracketPartGroups $predictedBracketPartGroups `
        -PredictedBracketProfiles $predictedBracketProfiles

    [pscustomobject]@{
        AssemblyId = [string]$source.AssemblyId
        SourceFile = $sourceFile
        PredictedBodyFamily = $predictedBodyFamily
        PredictedBracketCount = $predictedBracketCount
        PredictedBracketPartGroups = $predictedBracketPartGroups
        PredictedBracketProfiles = $predictedBracketProfiles
        PatternHint = [string]$patternInfo.PatternHint
        ReviewPromptZh = [string]$patternInfo.ReviewPromptZh
        HumanHasBracket = $humanHasBracket
        HumanBracketPartGroups = $humanBracketPartGroups
        HumanBodyFamily = $humanBodyFamily
        HumanVerdict = $humanVerdict
        Notes = $notes
    }
}

$outputRows | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8 -Force

[pscustomobject]@{
    PriorityBracketAnswersCsv = $priorityBracketAnswersCsv
    ReviewQueueCsv = $ReviewQueueCsv
    P1RequestCsv = $p1RequestCsv
    OutputCsv = $OutputCsv
    SourceRows = $priorityRows.Count
    ReviewRows = $reviewRows.Count
    P1Rows = @($outputRows).Count
} | Format-List
