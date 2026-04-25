param(
    [Parameter(Mandatory = $true)]
    [string]$P1BracketAnswersCsv,

    [Parameter(Mandatory = $true)]
    [string]$ResultDirectory,

    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Join-Values {
    param(
        [object[]]$Values,
        [string]$Separator = "; "
    )

    $items = @($Values | Where-Object { $null -ne $_ -and -not [string]::IsNullOrWhiteSpace([string]$_) } | ForEach-Object { [string]$_ })
    if ($items.Count -eq 0)
    {
        return ""
    }

    return ($items -join $Separator)
}

function Split-PartIds {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value))
    {
        return @()
    }

    return @(
        ($Value -replace "[，、；;|/]+", "," -replace "\s+", "").Split(",", [System.StringSplitOptions]::RemoveEmptyEntries) |
            ForEach-Object { $_.Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Get-RoleCountsText {
    param([object[]]$Rows)

    if ($null -eq $Rows -or $Rows.Count -eq 0)
    {
        return ""
    }

    $counts = @(
        $Rows |
            Group-Object Role |
            Sort-Object Name |
            ForEach-Object { "{0}:{1}" -f $_.Name, $_.Count }
    )

    return Join-Values -Values $counts -Separator ", "
}

function Get-ValueCountsText {
    param([object[]]$Values)

    $items = @(
        $Values |
            Where-Object { $null -ne $_ -and -not [string]::IsNullOrWhiteSpace([string]$_) } |
            ForEach-Object { [string]$_ }
    )

    if ($items.Count -eq 0)
    {
        return ""
    }

    $counts = @(
        $items |
            Group-Object |
            Sort-Object Name |
            ForEach-Object { "{0}x{1}" -f $_.Name, $_.Count }
    )

    return Join-Values -Values $counts -Separator "; "
}

function Shorten-Text {
    param(
        [string]$Value,
        [int]$MaxLength = 80
    )

    if ([string]::IsNullOrWhiteSpace($Value) -or $Value.Length -le $MaxLength)
    {
        return $Value
    }

    return $Value.Substring(0, $MaxLength - 3) + "..."
}

function Escape-MarkdownCell {
    param([string]$Value)

    if ($null -eq $Value)
    {
        return ""
    }

    return ([string]$Value).Replace("|", "\|")
}

function Decode-HtmlText {
    param([string]$Value)

    return [System.Net.WebUtility]::HtmlDecode($Value)
}

function Get-P1PatternInfo {
    param(
        [string]$Track,
        [string]$PredictedBodyFamily,
        [int]$PredictedBracketPartCount,
        [string]$PredictedBracketNameSummary,
        [string]$PredictedBracketProfileSummary
    )

    $nameSummary = if ($null -eq $PredictedBracketNameSummary) { "" } else { $PredictedBracketNameSummary }
    $profileSummary = if ($null -eq $PredictedBracketProfileSummary) { "" } else { $PredictedBracketProfileSummary }

    if ($Track -eq "GKZ positive" -and $PredictedBracketPartCount -eq 2 -and $nameSummary -like "*GKZ*")
    {
        return [pscustomobject]@{
            PatternHint = "compact_gkz_plate_pair"
            ReviewPromptZh = Decode-HtmlText "&#20248;&#20808;&#30830;&#35748;&#65306;&#36825;&#20004;&#29255; GKZ &#26495;&#26159;&#21542;&#26500;&#25104;&#30495;&#27491;&#22806;&#25361;&#29275;&#33151;&#65292;&#32780;&#19981;&#26159;&#26222;&#36890;&#23616;&#37096;&#26495;&#23545;"
        }
    }

    if ($PredictedBodyFamily -eq "BuiltUpT" -and $PredictedBracketPartCount -ge 4 -and ($nameSummary -like "*MQMJ*" -or $nameSummary -like "*MQB-*"))
    {
        return [pscustomobject]@{
            PatternHint = "angle_short_plate_cluster"
            ReviewPromptZh = Decode-HtmlText "&#20248;&#20808;&#30830;&#35748;&#65306;&#36825;&#32452;&#35282;&#38050;/&#30701;&#26495;&#31751;&#26159;&#21542;&#21482;&#26159;&#23616;&#37096;&#38468;&#20214;&#65292;&#32780;&#19981;&#26159;&#29275;&#33151;"
        }
    }

    if ($profileSummary -like "*PL*" -and $PredictedBracketPartCount -eq 2)
    {
        return [pscustomobject]@{
            PatternHint = "two_plate_appendage"
            ReviewPromptZh = Decode-HtmlText "&#20248;&#20808;&#30830;&#35748;&#65306;&#36825;&#20004;&#29255;&#26495;&#26159;&#21542;&#24418;&#25104;&#29420;&#31435;&#22806;&#25361;&#29275;&#33151;"
        }
    }

    return [pscustomobject]@{
        PatternHint = "manual_shape_check"
        ReviewPromptZh = Decode-HtmlText "&#20248;&#20808;&#30830;&#35748;&#65306;&#36825;&#32452;&#20505;&#36873;&#20214;&#26159;&#21542;&#24418;&#25104;&#29420;&#31435;&#22806;&#25361;&#29275;&#33151;"
    }
}

function Get-FirstRegexValue {
    param(
        [string]$InputText,
        [string]$Pattern
    )

    $match = [regex]::Match($InputText, $Pattern)
    if (-not $match.Success)
    {
        return ""
    }

    return $match.Groups["value"].Value
}

function Get-MemberLabels {
    param([string]$InputText)

    $labelsBlock = Get-FirstRegexValue -InputText $InputText -Pattern '"Labels":\[(?<value>[^\]]*)\]'
    if ([string]::IsNullOrWhiteSpace($labelsBlock))
    {
        return @()
    }

    return @([regex]::Matches($labelsBlock, '"(?<value>[^"]+)"') | ForEach-Object { $_.Groups["value"].Value })
}

function Get-MemberPartRoles {
    param([string]$InputText)

    $matches = [regex]::Matches(
        $InputText,
        '{"PartId":"(?<PartId>[^"]+)","Reason":"(?<Reason>(?:\\.|[^"])*)","Role":"(?<Role>[^"]+)","Score":(?<Score>[^}]+)}'
    )

    return @(
        $matches | ForEach-Object {
            [pscustomobject]@{
                PartId = $_.Groups["PartId"].Value
                Reason = $_.Groups["Reason"].Value
                Role = $_.Groups["Role"].Value
                Score = $_.Groups["Score"].Value
            }
        }
    )
}

$p1BracketAnswersCsv = (Resolve-Path $P1BracketAnswersCsv).Path
$resultDirectory = (Resolve-Path $ResultDirectory).Path

if ([string]::IsNullOrWhiteSpace($OutputDirectory))
{
    $OutputDirectory = Join-Path $resultDirectory "p1-review-bundle"
}

$outputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$rows = @(Import-Csv $p1BracketAnswersCsv)
if ($rows.Count -eq 0)
{
    throw "P1 bracket answers CSV is empty: $p1BracketAnswersCsv"
}

$rowCount = $rows.Count

$summaryRows = foreach ($row in $rows)
{
    $assemblyId = [string]$row.AssemblyId
    $sourceFile = [string]$row.SourceFile
    $memberRaw = ""
    if (-not [string]::IsNullOrWhiteSpace($sourceFile) -and (Test-Path $sourceFile))
    {
        $memberRaw = Get-Content $sourceFile -Raw
    }

    $inputPath = Join-Path $resultDirectory ("assembly-{0}.input.json" -f $assemblyId)
    $inputData = Get-Content $inputPath -Raw | ConvertFrom-Json
    $recognitionPath = Join-Path $resultDirectory ("assembly-{0}.recognition.json" -f $assemblyId)
    $recognition = Get-Content $recognitionPath -Raw | ConvertFrom-Json

    $partRoleRows = @(Get-MemberPartRoles -InputText $memberRaw)
    $predictedPartIds = @(Split-PartIds $row.PredictedBracketPartGroups)
    $predictedRoleRows = @($partRoleRows | Where-Object { $predictedPartIds -contains ([string]$_.PartId) })

    $memberParts = @($inputData.Parts)
    $predictedParts = @($memberParts | Where-Object { $predictedPartIds -contains ([string]$_.PartId) })
    $predictedPartNames = @($predictedParts | ForEach-Object { "{0}:{1}" -f $_.PartId, $_.Name })
    $predictedPartProfiles = @($predictedParts | ForEach-Object { "{0}:{1}" -f $_.PartId, $_.ProfileString })
    $predictedNameSummary = Get-ValueCountsText -Values @($predictedParts | ForEach-Object { $_.Name })
    $predictedProfileSummary = Get-ValueCountsText -Values @($predictedParts | ForEach-Object { $_.ProfileString })

    $firstBracket = $null
    if ($null -ne $recognition.Brackets -and $recognition.Brackets.BracketCount -gt 0)
    {
        $instances = @($recognition.Brackets.Instances)
        if ($instances.Count -gt 0)
        {
            $firstBracket = $instances[0]
        }
    }

    $track =
        if ($sourceFile -like "*GKZ*")
        {
            "GKZ positive"
        }
        elseif ([string]$row.PredictedBodyFamily -eq "BuiltUpT")
        {
            "BuiltUpT hard negative"
        }
        else
        {
            "GKZ positive"
        }

    $patternInfo = Get-P1PatternInfo `
        -Track $track `
        -PredictedBodyFamily ([string]$row.PredictedBodyFamily) `
        -PredictedBracketPartCount $predictedPartIds.Count `
        -PredictedBracketNameSummary $predictedNameSummary `
        -PredictedBracketProfileSummary $predictedProfileSummary

    [pscustomobject]@{
        AssemblyId = $assemblyId
        Track = $track
        SourceFile = if ([string]::IsNullOrWhiteSpace($sourceFile)) { "" } else { Split-Path -Leaf $sourceFile }
        PredictedBodyFamily = [string]$row.PredictedBodyFamily
        PredictedBracketPartGroups = [string]$row.PredictedBracketPartGroups
        MemberMainClass = Get-FirstRegexValue -InputText $memberRaw -Pattern '"MainClass":(?<value>-?\d+)'
        MemberConfidence = Get-FirstRegexValue -InputText $memberRaw -Pattern '"Confidence":(?<value>-?\d+(?:\.\d+)?)'
        MemberKeyDimensions = Get-FirstRegexValue -InputText $memberRaw -Pattern '"KeyDimensionsDisplay":"(?<value>[^"]*)"'
        MemberNeedsManualReview = Get-FirstRegexValue -InputText $memberRaw -Pattern '"NeedsManualReview":(?<value>true|false)'
        MemberReviewReason = Get-FirstRegexValue -InputText $memberRaw -Pattern '"ReviewReason":"(?<value>[^"]*)"'
        MemberLabels = Join-Values -Values @(Get-MemberLabels -InputText $memberRaw)
        MemberRoleCounts = Get-RoleCountsText -Rows $partRoleRows
        PredictedBracketRoleCounts = Get-RoleCountsText -Rows $predictedRoleRows
        PredictedBracketPartCount = $predictedPartIds.Count
        PredictedBracketNameSummary = $predictedNameSummary
        PredictedBracketProfileSummary = $predictedProfileSummary
        PatternHint = [string]$patternInfo.PatternHint
        ReviewPromptZh = [string]$patternInfo.ReviewPromptZh
        PredictedBracketPartNames = Join-Values -Values $predictedPartNames
        PredictedBracketPartProfiles = Join-Values -Values $predictedPartProfiles
        BodyConfidence = [math]::Round([double]$recognition.Body.BodyConfidence, 4)
        BodyReviewReasons = Join-Values -Values @($recognition.Body.ReviewReasons)
        BracketConfidence = if ($null -ne $recognition.Brackets) { [math]::Round([double]$recognition.Brackets.BracketConfidence, 4) } else { 0 }
        BracketReviewReasons = if ($null -ne $recognition.Brackets) { Join-Values -Values @($recognition.Brackets.ReviewReasons) } else { "" }
        BracketScore = if ($null -ne $firstBracket) { [string]$firstBracket.Score } else { "" }
        BracketPlateCount = if ($null -ne $firstBracket) { [string]$firstBracket.PlateCount } else { "" }
        BracketReasons = if ($null -ne $firstBracket) { Join-Values -Values @($firstBracket.ClassificationReasons) } else { "" }
        AppendageClusters = if ($null -ne $recognition.Brackets) { Join-Values -Values @($recognition.Brackets.AppendageClusters) } else { "" }
    }
}

$summaryCsvPath = Join-Path $outputDirectory "p1-review-summary.csv"
$summaryMdPath = Join-Path $outputDirectory "p1-review-summary.md"
$summaryZhMdPath = Join-Path $outputDirectory "p1-review-summary.zh-CN.md"

$summaryRows | Export-Csv -Path $summaryCsvPath -NoTypeInformation -Encoding UTF8 -Force

$lines = @(
    "# P1 Review Summary",
    "",
    ("This summary condenses the top `{0}` bracket-critical samples into one table." -f $rowCount),
    "",
    "| AssemblyId | Track | PredictedBody | PatternHint | CandidateShape | MemberHints | Roles | BodyConf | BracketConf | BracketReasons |",
    "| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |"
)

$linesZh = @(
    (Decode-HtmlText "# P1 &#29275;&#33151;&#20248;&#20808;&#22797;&#26680;&#25688;&#35201;"),
    "",
    ([string]::Format((Decode-HtmlText "&#36825;&#20221;&#25688;&#35201;&#25226;&#26368;&#20851;&#38190;&#30340; {0} &#20010;&#29275;&#33151;&#26679;&#26412;&#21387;&#25104;&#20102;&#19968;&#24352;&#34920;&#65292;&#26041;&#20415;&#30452;&#25509;&#20154;&#24037;&#30830;&#35748;&#12290;"), $rowCount)),
    "",
    (Decode-HtmlText "&#21028;&#35835;&#37325;&#28857;&#65306;"),
    "",
    (Decode-HtmlText "- `GKZ positive` &#36825; `3` &#26465;&#65292;&#24403;&#21069;&#26356;&#20687; `BuiltUpH/BOX` &#20027;&#20307;&#36793;&#19978;&#30340; `2` &#29255;&#32039;&#20945;&#26495;&#23545;"),
    (Decode-HtmlText "- `BuiltUpT hard negative` &#36825; `3` &#26465;&#65292;&#24403;&#21069;&#26356;&#20687; `BuiltUpT/L` &#20027;&#20307;&#36793;&#19978;&#30340; `4-5` &#20214;&#35282;&#38050;/&#30701;&#26495;&#31751;"),
    (Decode-HtmlText "- `CandidateShape` &#30475;&#20505;&#36873;&#20214;&#25968;&#37327;&#12289;&#21517;&#31216;&#27719;&#24635;&#12289;&#22411;&#26448;&#27719;&#24635;"),
    (Decode-HtmlText "- `BracketReasons` &#30475;&#24403;&#21069;&#35268;&#21017;&#20026;&#20160;&#20040;&#25226;&#23427;&#24403;&#25104;&#29275;&#33151;&#20505;&#36873;"),
    "",
    (Decode-HtmlText "| AssemblyId | &#20998;&#32452; | &#39044;&#27979;&#20027;&#20307; | &#27169;&#24335;&#25552;&#31034; | &#20505;&#36873;&#20214;&#24418;&#24577; | &#26500;&#20214;&#25552;&#31034; | &#35282;&#33394;&#32479;&#35745; | &#20027;&#20307;&#32622;&#20449;&#24230; | &#29275;&#33151;&#32622;&#20449;&#24230; | &#29275;&#33151;&#21028;&#23450;&#32447;&#32034; | &#20248;&#20808;&#22797;&#26680;&#38382;&#39064; |"),
    "| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |"
)

foreach ($row in $summaryRows)
{
    $memberHints = Escape-MarkdownCell (Shorten-Text -Value (Join-Values -Values @($row.MemberKeyDimensions, $row.MemberReviewReason)) -MaxLength 80)
    $candidateShape = Escape-MarkdownCell (Shorten-Text -Value (Join-Values -Values @(
        ("parts={0}" -f $row.PredictedBracketPartCount),
        $row.PredictedBracketNameSummary,
        $row.PredictedBracketProfileSummary
    )) -MaxLength 100)
    $roleText = Escape-MarkdownCell $row.PredictedBracketRoleCounts
    $patternHint = Escape-MarkdownCell $row.PatternHint
    $bracketReasons = Escape-MarkdownCell (Shorten-Text -Value $row.BracketReasons -MaxLength 120)
    $reviewPromptZh = Escape-MarkdownCell (Shorten-Text -Value $row.ReviewPromptZh -MaxLength 50)

    $lines += "| $($row.AssemblyId) | $($row.Track) | $($row.PredictedBodyFamily) | $patternHint | $candidateShape | $memberHints | $roleText | $($row.BodyConfidence) | $($row.BracketConfidence) | $bracketReasons |"
    $linesZh += "| $($row.AssemblyId) | $($row.Track) | $($row.PredictedBodyFamily) | $patternHint | $candidateShape | $memberHints | $roleText | $($row.BodyConfidence) | $($row.BracketConfidence) | $bracketReasons | $reviewPromptZh |"
}

$lines += ""
$lines += "## Generated Files"
$lines += ""
$lines += '- `p1-review-summary.csv`'
$lines += '- `p1-review-summary.md`'

$linesZh += ""
$linesZh += Decode-HtmlText "## &#24050;&#29983;&#25104;&#25991;&#20214;"
$linesZh += ""
$linesZh += '- `p1-review-summary.csv`'
$linesZh += '- `p1-review-summary.md`'
$linesZh += '- `p1-review-summary.zh-CN.md`'

Set-Content -Path $summaryMdPath -Value $lines -Encoding UTF8
Set-Content -Path $summaryZhMdPath -Value $linesZh -Encoding UTF8

[pscustomobject]@{
    OutputDirectory = $outputDirectory
    SummaryCsv = $summaryCsvPath
    SummaryMd = $summaryMdPath
    SummaryZhMd = $summaryZhMdPath
    Rows = $summaryRows.Count
} | Format-List
