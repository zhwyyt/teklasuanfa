param(
    [Parameter(Mandatory = $true)]
    [string]$ResultDirectory,

    [string]$OutputCsv
)

$ErrorActionPreference = "Stop"

$resolvedResultDirectory = (Resolve-Path $ResultDirectory).Path
if ([string]::IsNullOrWhiteSpace($OutputCsv))
{
    $OutputCsv = Join-Path $resolvedResultDirectory "priority-review-pack.inline.csv"
}

$childScript = @'
param(
    [string]$ResolvedResultDirectory,
    [string]$OutputCsv
)

$priorityPackPath = Join-Path $ResolvedResultDirectory 'priority-review-pack.md'
$batchSummaryPath = Join-Path $ResolvedResultDirectory 'batch-summary.json'
$reviewQueuePath = Join-Path $ResolvedResultDirectory 'review-queue.csv'
$bodyChangeSummaryPath = Join-Path $ResolvedResultDirectory 'body-change-summary.md'

if (-not (Test-Path $priorityPackPath)) { throw "Missing priority-review-pack.md: $priorityPackPath" }
if (-not (Test-Path $batchSummaryPath)) { throw "Missing batch-summary.json: $batchSummaryPath" }

$batch = @(Get-Content $batchSummaryPath -Raw | ConvertFrom-Json)
$review = if (Test-Path $reviewQueuePath) { @(Import-Csv $reviewQueuePath) } else { @() }
$body = @()

if (Test-Path $bodyChangeSummaryPath)
{
    foreach ($line in (Get-Content $bodyChangeSummaryPath))
    {
        $match = [regex]::Match(
            $line,
            '^\|\s*(?<Count>\d+)\s*\|\s*(?<Baseline>[^|]+)\|\s*(?<Candidate>[^|]+)\|\s*(?<MemberProfile>[^|]+)\|\s*(?<Representative>\d+)\s*\|')

        if ($match.Success)
        {
            $body += [pscustomobject]@{
                Representative = [string]$match.Groups['Representative'].Value.Trim()
                Change = ("{0}->{1}" -f $match.Groups['Baseline'].Value.Trim(), $match.Groups['Candidate'].Value.Trim())
                MemberProfile = [string]$match.Groups['MemberProfile'].Value.Trim()
            }
        }
    }
}

$ids = @()
$inCombined = $false
foreach ($line in (Get-Content $priorityPackPath))
{
    if ($line -eq '## Combined Representatives')
    {
        $inCombined = $true
        continue
    }

    if (-not $inCombined)
    {
        continue
    }

    if ([string]::IsNullOrWhiteSpace($line))
    {
        if ($ids.Count -gt 0)
        {
            break
        }

        continue
    }

    if ($line -like '| AssemblyId *' -or $line -like '| --- *')
    {
        continue
    }

    $match = [regex]::Match($line, '^\|\s*(?<AssemblyId>\d+)\s*\|')
    if ($match.Success)
    {
        $ids += [string]$match.Groups['AssemblyId'].Value.Trim()
    }
}

$rows = foreach ($assemblyId in $ids)
{
    $summary = $batch | Where-Object { [string]$_.AssemblyId -eq $assemblyId } | Select-Object -First 1
    $reviewQueue = $review | Where-Object { [string]$_.AssemblyId -eq $assemblyId } | Select-Object -First 1
    $bodyChange = $body | Where-Object { [string]$_.Representative -eq $assemblyId } | Select-Object -First 1

    $recognitionPath = Join-Path $ResolvedResultDirectory ("assembly-{0}.recognition.json" -f $assemblyId)
    $bodyReasons = ''
    $bracketReasons = ''
    $bracketConfidence = ''

    if (Test-Path $recognitionPath)
    {
        $recognition = Get-Content $recognitionPath -Raw | ConvertFrom-Json
        $bodyReasons = @($recognition.Body.ReviewReasons) -join '|'
        $bracketReasons = @($recognition.Brackets.ReviewReasons) -join '|'
        $bracketConfidence = [string]$recognition.Brackets.BracketConfidence
    }

    $reasonTokens = @()
    if ($null -ne $reviewQueue) { $reasonTokens += 'BracketPattern' }
    if ($null -ne $bodyChange) { $reasonTokens += 'BodyChange' }

    $predictedBodyFamily = if ($null -ne $summary) { [string]$summary.BodyFamily } else { '' }
    $predictedBodyConfidence = if ($null -ne $summary) { [string]$summary.BodyConfidence } else { '' }
    $predictedBracketCount = if ($null -ne $summary) { [string]$summary.BracketCount } else { '' }
    $sourceFile = if ($null -ne $summary) { [string]$summary.SourceFile } else { '' }
    $predictedBracketPartGroups = if ($null -ne $reviewQueue) { [string]$reviewQueue.PredictedBracketPartGroups } else { '' }
    $predictedBracketProfiles = if ($null -ne $reviewQueue) { [string]$reviewQueue.PredictedBracketProfiles } else { '' }
    $predictedBracketReasons = if ($null -ne $reviewQueue) { [string]$reviewQueue.PredictedBracketReasons } else { '' }
    $bodyChangeText = if ($null -ne $bodyChange) { [string]$bodyChange.Change } else { '' }
    $memberProfile = if ($null -ne $bodyChange) { [string]$bodyChange.MemberProfile } else { '' }

    $bodyFamilyOrChange =
        if ($null -ne $reviewQueue -and $null -ne $bodyChange)
        {
            "$predictedBodyFamily || $bodyChangeText"
        }
        elseif ($null -ne $bodyChange)
        {
            $bodyChangeText
        }
        else
        {
            $predictedBodyFamily
        }

    $signature =
        if ($null -ne $reviewQueue -and $null -ne $bodyChange)
        {
            "$predictedBracketProfiles || $memberProfile"
        }
        elseif ($null -ne $reviewQueue)
        {
            $predictedBracketProfiles
        }
        else
        {
            $memberProfile
        }

    [pscustomobject]@{
        AssemblyId = $assemblyId
        Reasons = ($reasonTokens -join ', ')
        BodyFamilyOrChange = $bodyFamilyOrChange
        Signature = $signature
        SourceFile = $sourceFile
        PredictedBodyFamily = $predictedBodyFamily
        PredictedBodyConfidence = $predictedBodyConfidence
        PredictedBracketCount = $predictedBracketCount
        PredictedBracketConfidence = $bracketConfidence
        PredictedBracketPartGroups = $predictedBracketPartGroups
        PredictedBracketProfiles = $predictedBracketProfiles
        PredictedBracketReasons = $predictedBracketReasons
        BodyReviewReasons = $bodyReasons
        BracketReviewReasons = $bracketReasons
        HumanHasBracket = ''
        HumanBodyFamily = ''
        HumanBracketPartGroups = ''
        HumanVerdict = ''
        Notes = ''
    }
}

$rows | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8 -Force
Write-Host "Priority review CSV written to: $OutputCsv"
'@

$childScriptPath = Join-Path $resolvedResultDirectory ".codex-build-priority-review-child.ps1"
Set-Content -Path $childScriptPath -Value $childScript -Encoding UTF8

try
{
    & powershell -NoProfile -ExecutionPolicy Bypass -File $childScriptPath $resolvedResultDirectory $OutputCsv
    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }
}
finally
{
    if (Test-Path $childScriptPath)
    {
        Remove-Item -LiteralPath $childScriptPath -Force
    }
}
