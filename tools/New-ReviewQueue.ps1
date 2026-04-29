param(
    [Parameter(Mandatory = $true)]
    [string]$ResultDirectory,

    [string]$OutputCsv,

    [switch]$IncludeZeroBracketAssemblies
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$resultDirectory = (Resolve-Path $ResultDirectory).Path
if ([string]::IsNullOrWhiteSpace($OutputCsv))
{
    $OutputCsv = Join-Path $resultDirectory "review-queue.csv"
}

$summaryPath = Join-Path $resultDirectory "batch-summary.json"
if (-not (Test-Path $summaryPath))
{
    throw "未找到 batch-summary.json: $summaryPath"
}

$summaryItems = Get-Content $summaryPath -Raw | ConvertFrom-Json
if (-not $IncludeZeroBracketAssemblies)
{
    $summaryItems = @($summaryItems | Where-Object { [int]$_.BracketCount -gt 0 })
}

$rows = foreach ($item in $summaryItems)
{
    $assemblyId = [string]$item.AssemblyId
    $recognitionPath = Join-Path $resultDirectory ("assembly-" + $assemblyId + ".recognition.json")
    $inputPath = Join-Path $resultDirectory ("assembly-" + $assemblyId + ".input.json")

    $recognition = Get-Content $recognitionPath -Raw | ConvertFrom-Json
    $input = Get-Content $inputPath -Raw | ConvertFrom-Json
    $partLookup = @{}
    foreach ($part in $input.Parts)
    {
        $partLookup[[int]$part.PartId] = $part
    }

    $instanceGroups = @()
    $instanceProfiles = @()
    $instanceReasons = @()

    foreach ($instance in $recognition.Brackets.Instances)
    {
        $partIds = @($instance.PartIds | ForEach-Object { [int]$_ })
        $instanceGroups += (($partIds | ForEach-Object { $_.ToString() }) -join ",")

        $profileNames = foreach ($partId in $partIds)
        {
            if ($partLookup.ContainsKey($partId))
            {
                $part = $partLookup[$partId]
                "{0}:{1}" -f $partId, $part.ProfileString
            }
        }

        if (@($profileNames).Count -gt 0)
        {
            $instanceProfiles += (@($profileNames) -join "|")
        }

        $instanceReasons += (@($instance.ClassificationReasons) -join "|")
    }

    [pscustomobject]@{
        AssemblyId                 = $assemblyId
        SourceFile                 = [string]$item.SourceFile
        PredictedBodyFamily        = [string]$item.BodyFamily
        PredictedBodyConfidence    = [math]::Round([double]$recognition.Body.BodyConfidence, 4)
        PredictedBracketCount      = [int]$recognition.Brackets.BracketCount
        PredictedBracketPartGroups = ($instanceGroups -join "; ")
        PredictedBracketProfiles   = ($instanceProfiles -join "; ")
        PredictedBracketReasons    = ($instanceReasons -join "; ")
        HumanHasBracket            = ""
        HumanBodyFamily            = ""
        HumanBracketPartGroups     = ""
        HumanVerdict               = ""
        Notes                      = ""
    }
}

$rows | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8
Write-Host "Review queue written to: $OutputCsv"
