param(
    [Parameter(Mandatory = $true)]
    [string]$ReviewQueueCsv,

    [Parameter(Mandatory = $true)]
    [string]$BaselineResultDirectory,

    [Parameter(Mandatory = $true)]
    [string]$CandidateResultDirectory,

    [string]$MemberCacheDirectory,

    [string]$OutputMarkdown
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$reviewQueueCsv = (Resolve-Path $ReviewQueueCsv).Path
$baselineDirectory = (Resolve-Path $BaselineResultDirectory).Path
$candidateDirectory = (Resolve-Path $CandidateResultDirectory).Path

if (-not [string]::IsNullOrWhiteSpace($MemberCacheDirectory))
{
    $MemberCacheDirectory = (Resolve-Path $MemberCacheDirectory).Path
}

if ([string]::IsNullOrWhiteSpace($OutputMarkdown))
{
    $OutputMarkdown = Join-Path $candidateDirectory "priority-review-pack.md"
}

$reviewRows = @(Import-Csv $reviewQueueCsv)

$bracketRepresentatives = @($reviewRows |
    ForEach-Object {
        $profiles = @($_.PredictedBracketProfiles -split '\|') |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            ForEach-Object { ($_ -split ':', 2)[1] } |
            Sort-Object

        [pscustomobject]@{
            AssemblyId      = $_.AssemblyId
            BodyFamily      = $_.PredictedBodyFamily
            Signature       = ($profiles -join '|')
            RawProfiles     = $_.PredictedBracketProfiles
            SourceFile      = $_.SourceFile
            Representative  = $_.AssemblyId
        }
    } |
    Group-Object BodyFamily, Signature |
    Sort-Object Count -Descending |
    ForEach-Object {
        $first = $_.Group[0]
        [pscustomobject]@{
            Count           = $_.Count
            AssemblyId      = $first.AssemblyId
            Reason          = "BracketPattern"
            BodyFamily      = $first.BodyFamily
            Signature       = $first.Signature
            SourceFile      = $first.SourceFile
            RelatedAssemblies = (($_.Group | Select-Object -ExpandProperty AssemblyId) -join ', ')
        }
    })

function Load-Summary([string]$directory)
{
    $path = Join-Path $directory "batch-summary.json"
    return @(Get-Content $path -Raw | ConvertFrom-Json)
}

$baseline = Load-Summary $baselineDirectory
$candidate = Load-Summary $candidateDirectory
$baseLookup = @{}
$candLookup = @{}
foreach ($item in $baseline) { $baseLookup[[string]$item.AssemblyId] = $item }
foreach ($item in $candidate) { $candLookup[[string]$item.AssemblyId] = $item }

$profileRegex = [regex]'"ProfileString"\s*:\s*"(?<profile>[^"]+)"'
$bodyChangeRows = @(
foreach ($assemblyId in ($baseLookup.Keys + $candLookup.Keys | Sort-Object -Unique))
{
    if (-not $baseLookup.ContainsKey($assemblyId) -or -not $candLookup.ContainsKey($assemblyId))
    {
        continue
    }

    $b = $baseLookup[$assemblyId]
    $c = $candLookup[$assemblyId]
    if ([string]$b.BodyFamily -eq [string]$c.BodyFamily)
    {
        continue
    }

    $profile = ""
    if (-not [string]::IsNullOrWhiteSpace($MemberCacheDirectory) -and -not [string]::IsNullOrWhiteSpace($c.SourceFile) -and (Test-Path $c.SourceFile))
    {
        $content = Get-Content $c.SourceFile -Raw
        $match = $profileRegex.Match($content)
        if ($match.Success)
        {
            $profile = $match.Groups["profile"].Value
        }
    }

    [pscustomobject]@{
        AssemblyId        = $assemblyId
        BaselineBody      = [string]$b.BodyFamily
        CandidateBody     = [string]$c.BodyFamily
        MemberProfile     = $profile
        SourceFile        = [string]$c.SourceFile
        ChangeSignature   = "{0}->{1}|{2}" -f [string]$b.BodyFamily, [string]$c.BodyFamily, $profile
    }
}
)

$bodyRepresentatives = @($bodyChangeRows |
    Group-Object ChangeSignature |
    Sort-Object Count -Descending |
    ForEach-Object {
        $first = $_.Group[0]
        [pscustomobject]@{
            Count             = $_.Count
            AssemblyId        = $first.AssemblyId
            Reason            = "BodyChange"
            BodyFamily        = "{0}->{1}" -f $first.BaselineBody, $first.CandidateBody
            Signature         = $first.MemberProfile
            SourceFile        = $first.SourceFile
            RelatedAssemblies = (($_.Group | Select-Object -ExpandProperty AssemblyId) -join ', ')
        }
    })

$combined = @(@($bracketRepresentatives) + @($bodyRepresentatives) |
    Group-Object AssemblyId |
    ForEach-Object {
        $first = $_.Group[0]
        [pscustomobject]@{
            AssemblyId      = $first.AssemblyId
            Reasons         = (($_.Group | Select-Object -ExpandProperty Reason) -join ', ')
            Signature       = (($_.Group | Select-Object -ExpandProperty Signature) -join ' || ')
            BodyFamily      = (($_.Group | Select-Object -ExpandProperty BodyFamily) -join ' || ')
            SourceFile      = $first.SourceFile
        }
    } |
    Sort-Object AssemblyId)

$lines = @(
    "# Priority Review Pack",
    "",
    "- Bracket representative groups: $(@($bracketRepresentatives).Count)",
    "- Body-change representative groups: $(@($bodyRepresentatives).Count)",
    "- Combined unique representative assemblies: $(@($combined).Count)",
    "",
    "## Combined Representatives",
    "",
    "| AssemblyId | Reasons | BodyFamily / Change | Signature | SourceFile |",
    "| --- | --- | --- | --- | --- |"
)

foreach ($item in $combined)
{
    $lines += "| $($item.AssemblyId) | $($item.Reasons) | $($item.BodyFamily) | $($item.Signature) | $($item.SourceFile) |"
}

$lines += ""
$lines += "## Bracket Representatives"
$lines += ""
$lines += "| AssemblyId | Count | BodyFamily | Signature | RelatedAssemblies |"
$lines += "| --- | --- | --- | --- | --- |"

foreach ($item in $bracketRepresentatives)
{
    $lines += "| $($item.AssemblyId) | $($item.Count) | $($item.BodyFamily) | $($item.Signature) | $($item.RelatedAssemblies) |"
}

$lines += ""
$lines += "## Body Change Representatives"
$lines += ""
$lines += "| AssemblyId | Count | Change | MemberProfile | RelatedAssemblies |"
$lines += "| --- | --- | --- | --- | --- |"

foreach ($item in $bodyRepresentatives)
{
    $lines += "| $($item.AssemblyId) | $($item.Count) | $($item.BodyFamily) | $($item.Signature) | $($item.RelatedAssemblies) |"
}

Set-Content -Path $OutputMarkdown -Value $lines -Encoding UTF8
Write-Host "Priority review pack written to: $OutputMarkdown"
