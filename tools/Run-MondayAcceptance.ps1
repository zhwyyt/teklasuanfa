param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$toolsDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$runScript = Join-Path $toolsDirectory "Run-PackagedRecognition.ps1"
$assertScript = Join-Path $toolsDirectory "Assert-RecognitionResult.ps1"

if ([string]::IsNullOrWhiteSpace($OutputPath))
{
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $packageRoot = Split-Path -Parent $toolsDirectory
    $OutputPath = Join-Path $packageRoot "results\monday-acceptance-$timestamp"
}

& $runScript `
    -InputPath $InputPath `
    -OutputPath $OutputPath `
    -GenerateReviewQueue

& $assertScript -ResultDirectory $OutputPath
