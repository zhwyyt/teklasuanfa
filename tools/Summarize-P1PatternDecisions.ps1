param(
    [Parameter(Mandatory = $true)]
    [string]$P1BracketAnswersCsv,

    [string]$OutputMarkdown,

    [string]$OutputCsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Decode-HtmlText {
    param([string]$Value)

    return [System.Net.WebUtility]::HtmlDecode($Value)
}

function Normalize-YesNo {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value))
    {
        return ""
    }

    $normalized = ($Value.Trim().ToLowerInvariant() -replace "[\s_\-\\/]+", "")
    $yesAliases = @(
        "y", "yes", "true", "1", "tp",
        (Decode-HtmlText "&#26159;"),
        (Decode-HtmlText "&#26159;&#30340;"),
        (Decode-HtmlText "&#26377;"),
        (Decode-HtmlText "&#26377;&#29275;&#33151;"),
        (Decode-HtmlText "&#23384;&#22312;"),
        (Decode-HtmlText "&#30495;"),
        (Decode-HtmlText "&#23545;"),
        (Decode-HtmlText "&#38451;&#24615;")
    )
    $noAliases = @(
        "n", "no", "false", "0", "fp",
        (Decode-HtmlText "&#21542;"),
        (Decode-HtmlText "&#27809;&#26377;"),
        (Decode-HtmlText "&#26080;"),
        (Decode-HtmlText "&#26080;&#29275;&#33151;"),
        (Decode-HtmlText "&#19981;&#23384;&#22312;"),
        (Decode-HtmlText "&#20551;"),
        (Decode-HtmlText "&#38452;&#24615;")
    )
    switch ($normalized)
    {
        { $_ -in $yesAliases } { return "yes" }
        { $_ -in $noAliases } { return "no" }
        default { return "" }
    }
}

function Get-PatternRecommendation {
    param(
        [string]$PatternHint,
        [int]$TotalRows,
        [int]$LabeledRows,
        [int]$YesRows,
        [int]$NoRows
    )

    if ($LabeledRows -eq 0)
    {
        return "no decision yet; waiting for truth labels"
    }

    if ($LabeledRows -lt $TotalRows)
    {
        return "partial labels only; wait for the full bucket before changing rules"
    }

    if ($YesRows -gt 0 -and $NoRows -gt 0)
    {
        return "mixed truth inside the same bucket; keep manual review and avoid hard suppression"
    }

    switch ($PatternHint)
    {
        "angle_short_plate_cluster" {
            if ($NoRows -eq $TotalRows) { return "all labeled false; safe candidate for a narrow suppression rule" }
            if ($YesRows -eq $TotalRows) { return "all labeled true; do not suppress this bucket" }
            return "needs more truth"
        }
        "compact_gkz_plate_pair" {
            if ($YesRows -eq $TotalRows) { return "all labeled true; protect or boost this bucket, do not suppress" }
            if ($NoRows -eq $TotalRows) { return "all labeled false; surprising result, verify before suppressing due to high blast radius" }
            return "needs more truth"
        }
        default {
            if ($NoRows -eq $TotalRows) { return "all labeled false; candidate for a narrow suppression rule" }
            if ($YesRows -eq $TotalRows) { return "all labeled true; keep this bucket" }
            return "needs more truth"
        }
    }
}

$p1BracketAnswersCsv = (Resolve-Path $P1BracketAnswersCsv).Path
$rows = @(Import-Csv $p1BracketAnswersCsv)
if ($rows.Count -eq 0)
{
    throw "P1 bracket answers CSV is empty: $p1BracketAnswersCsv"
}

$outputDirectory = Split-Path -Parent $p1BracketAnswersCsv
if ([string]::IsNullOrWhiteSpace($OutputMarkdown))
{
    $OutputMarkdown = Join-Path $outputDirectory "p1-pattern-decision-summary.md"
}

if ([string]::IsNullOrWhiteSpace($OutputCsv))
{
    $OutputCsv = Join-Path $outputDirectory "p1-pattern-decision-summary.csv"
}

$summaryRows = @(
foreach ($group in @($rows | Group-Object PatternHint | Sort-Object Name))
{
    $bucketRows = @($group.Group)
    $labeledRows = @($bucketRows | Where-Object { Normalize-YesNo $_.HumanHasBracket })
    $yesRows = @($labeledRows | Where-Object { (Normalize-YesNo $_.HumanHasBracket) -eq "yes" })
    $noRows = @($labeledRows | Where-Object { (Normalize-YesNo $_.HumanHasBracket) -eq "no" })
    $unlabeledRows = @($bucketRows | Where-Object { [string]::IsNullOrWhiteSpace((Normalize-YesNo $_.HumanHasBracket)) })

    [pscustomobject]@{
        PatternHint = [string]$group.Name
        TotalRows = $bucketRows.Count
        LabeledRows = $labeledRows.Count
        YesRows = $yesRows.Count
        NoRows = $noRows.Count
        UnlabeledRows = $unlabeledRows.Count
        AssemblyIds = (@($bucketRows | Select-Object -ExpandProperty AssemblyId) -join ", ")
        Recommendation = Get-PatternRecommendation `
            -PatternHint ([string]$group.Name) `
            -TotalRows $bucketRows.Count `
            -LabeledRows $labeledRows.Count `
            -YesRows $yesRows.Count `
            -NoRows $noRows.Count
    }
}
)

$summaryRows | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8 -Force

$lines = @(
    "# P1 Pattern Decision Summary",
    "",
    "This summary rolls the filled 6-row P1 sheet up to pattern-level decisions.",
    "",
    "| PatternHint | Total | Labeled | Yes | No | Unlabeled | Recommendation |",
    "| --- | --- | --- | --- | --- | --- | --- |"
)

foreach ($row in $summaryRows)
{
    $lines += "| $($row.PatternHint) | $($row.TotalRows) | $($row.LabeledRows) | $($row.YesRows) | $($row.NoRows) | $($row.UnlabeledRows) | $($row.Recommendation) |"
}

$lines += ""
$lines += "## Details"
$lines += ""
foreach ($row in $summaryRows)
{
    $lines += "### $($row.PatternHint)"
    $lines += ""
    $lines += "- Assemblies: $($row.AssemblyIds)"
    $lines += "- Recommendation: $($row.Recommendation)"
    $lines += ""
}

Set-Content -Path $OutputMarkdown -Value $lines -Encoding UTF8

[pscustomobject]@{
    P1BracketAnswersCsv = $p1BracketAnswersCsv
    OutputMarkdown = $OutputMarkdown
    OutputCsv = $OutputCsv
    PatternBuckets = @($summaryRows).Count
    LabeledRows = @($rows | Where-Object { Normalize-YesNo $_.HumanHasBracket }).Count
} | Format-List
