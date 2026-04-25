param(
    [Parameter(Mandatory = $true)]
    [string]$ReviewQueueCsv,

    [string]$OutputMarkdown
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$reviewQueueCsv = (Resolve-Path $ReviewQueueCsv).Path
if ([string]::IsNullOrWhiteSpace($OutputMarkdown))
{
    $directory = Split-Path $reviewQueueCsv -Parent
    $OutputMarkdown = Join-Path $directory "review-queue-summary.md"
}

$rows = @(Import-Csv $reviewQueueCsv)

$normalizedRows = foreach ($row in $rows)
{
    $profiles = @($row.PredictedBracketProfiles -split '\|') |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object { ($_ -split ':', 2)[1] } |
        Sort-Object

    [pscustomobject]@{
        AssemblyId = $row.AssemblyId
        BodyFamily = $row.PredictedBodyFamily
        Signature  = ($profiles -join '|')
        SourceFile = $row.SourceFile
        RawProfile = $row.PredictedBracketProfiles
    }
}

$grouped = $normalizedRows |
    Group-Object BodyFamily, Signature |
    Sort-Object Count -Descending |
    ForEach-Object {
    $first = $_.Group[0]
    [pscustomobject]@{
        Count            = $_.Count
        BodyFamily       = $first.BodyFamily
        Signature        = $first.Signature
        RepresentativeId = $first.AssemblyId
        Assemblies       = (($_.Group | Select-Object -ExpandProperty AssemblyId) -join ', ')
        SourceFiles      = (($_.Group | Select-Object -ExpandProperty SourceFile) -join '; ')
    }
    }

$lines = @(
    "# Review Queue Summary",
    "",
    "Total labeled candidates: $($rows.Count)",
    "Unique pattern groups: $($grouped.Count)",
    "",
    "| Count | BodyFamily | Representative | Signature | Assemblies |",
    "| --- | --- | --- | --- | --- |"
)

foreach ($item in $grouped)
{
    $lines += "| $($item.Count) | $($item.BodyFamily) | $($item.RepresentativeId) | $($item.Signature) | $($item.Assemblies) |"
}

Set-Content -Path $OutputMarkdown -Value $lines -Encoding UTF8
Write-Host "Review summary written to: $OutputMarkdown"
