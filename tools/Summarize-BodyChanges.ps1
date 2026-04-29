param(
    [Parameter(Mandatory = $true)]
    [string]$BaselineResultDirectory,

    [Parameter(Mandatory = $true)]
    [string]$CandidateResultDirectory,

    [string]$MemberCacheDirectory,

    [string]$OutputMarkdown
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$baselineDirectory = (Resolve-Path $BaselineResultDirectory).Path
$candidateDirectory = (Resolve-Path $CandidateResultDirectory).Path

if (-not [string]::IsNullOrWhiteSpace($MemberCacheDirectory))
{
    $MemberCacheDirectory = (Resolve-Path $MemberCacheDirectory).Path
}

if ([string]::IsNullOrWhiteSpace($OutputMarkdown))
{
    $OutputMarkdown = Join-Path $candidateDirectory "body-change-summary.md"
}

function Load-Summary([string]$directory)
{
    $path = Join-Path $directory "batch-summary.json"
    if (-not (Test-Path $path))
    {
        throw "未找到 batch-summary.json: $path"
    }

    return @(Get-Content $path -Raw | ConvertFrom-Json)
}

$baselineItems = Load-Summary $baselineDirectory
$candidateItems = Load-Summary $candidateDirectory

$baselineLookup = @{}
$candidateLookup = @{}
foreach ($item in $baselineItems) { $baselineLookup[[string]$item.AssemblyId] = $item }
foreach ($item in $candidateItems) { $candidateLookup[[string]$item.AssemblyId] = $item }

$memberProfileLookup = @{}
if (-not [string]::IsNullOrWhiteSpace($MemberCacheDirectory))
{
    $profileRegex = [regex]'"AssemblyId"\s*:\s*"(?<assembly>[^"]+)".+?"ProfileString"\s*:\s*"(?<profile>[^"]+)"'
}

$rows = @(
foreach ($assemblyId in ($baselineLookup.Keys + $candidateLookup.Keys | Sort-Object -Unique))
{
    if (-not $baselineLookup.ContainsKey($assemblyId) -or -not $candidateLookup.ContainsKey($assemblyId))
    {
        continue
    }

    $baseline = $baselineLookup[$assemblyId]
    $candidate = $candidateLookup[$assemblyId]
    if ([string]$baseline.BodyFamily -eq [string]$candidate.BodyFamily)
    {
        continue
    }

    [pscustomobject]@{
        AssemblyId            = $assemblyId
        BaselineBodyFamily    = [string]$baseline.BodyFamily
        CandidateBodyFamily   = [string]$candidate.BodyFamily
        MemberProfile         = ""
        BaselineBracketCount  = [int]$baseline.BracketCount
        CandidateBracketCount = [int]$candidate.BracketCount
        SourceFile            = [string]$candidate.SourceFile
    }
}
)

if (-not [string]::IsNullOrWhiteSpace($MemberCacheDirectory))
{
    foreach ($row in $rows)
    {
        if ([string]::IsNullOrWhiteSpace($row.SourceFile) -or -not (Test-Path $row.SourceFile))
        {
            continue
        }

        $content = Get-Content $row.SourceFile -Raw
        $match = $profileRegex.Match($content)
        if ($match.Success)
        {
            $row.MemberProfile = $match.Groups["profile"].Value
        }
    }
}

$grouped = @($rows |
    Group-Object BaselineBodyFamily, CandidateBodyFamily, MemberProfile |
    Sort-Object Count -Descending |
    ForEach-Object {
        $first = $_.Group[0]
        [pscustomobject]@{
            Count            = $_.Count
            BaselineBody     = $first.BaselineBodyFamily
            CandidateBody    = $first.CandidateBodyFamily
            MemberProfile    = $first.MemberProfile
            RepresentativeId = $first.AssemblyId
            Assemblies       = (($_.Group | Select-Object -ExpandProperty AssemblyId) -join ', ')
        }
    })

$lines = @(
    "# Body Change Summary",
    "",
    "Baseline: $baselineDirectory",
    "Candidate: $candidateDirectory",
    "",
    "Changed assemblies: $(@($rows).Count)",
    "Unique change groups: $(@($grouped).Count)",
    "",
    "| Count | BaselineBody | CandidateBody | MemberProfile | Representative | Assemblies |",
    "| --- | --- | --- | --- | --- | --- |"
)

foreach ($item in $grouped)
{
    $lines += "| $($item.Count) | $($item.BaselineBody) | $($item.CandidateBody) | $($item.MemberProfile) | $($item.RepresentativeId) | $($item.Assemblies) |"
}

Set-Content -Path $OutputMarkdown -Value $lines -Encoding UTF8
Write-Host "Body change summary written to: $OutputMarkdown"
