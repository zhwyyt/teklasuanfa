param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [string]$OutputPath,

    [switch]$GenerateReviewQueue
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$toolsDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$packageRoot = Split-Path -Parent $toolsDirectory
$appDirectory = Join-Path $packageRoot "app"
$appDll = Join-Path $appDirectory "TeklaBodyBracketRecognition.App.dll"

if (-not (Test-Path $appDll))
{
    throw "未找到打包后的识别程序: $appDll"
}

$resolvedInput = (Resolve-Path $InputPath).Path
if ([string]::IsNullOrWhiteSpace($OutputPath))
{
    $leaf = Split-Path $resolvedInput -Leaf
    if ([string]::IsNullOrWhiteSpace($leaf))
    {
        $leaf = "recognition-input"
    }

    $OutputPath = Join-Path $packageRoot "results\$leaf"
}

New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

dotnet $appDll $resolvedInput $OutputPath
if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

if ($GenerateReviewQueue)
{
    $reviewQueueScript = Join-Path $toolsDirectory "New-ReviewQueue.ps1"
    & $reviewQueueScript -ResultDirectory $OutputPath
}

$summaryPath = Join-Path $OutputPath "batch-summary.json"
if (Test-Path $summaryPath)
{
    $items = Get-Content $summaryPath -Raw | ConvertFrom-Json
    [pscustomobject]@{
        Assemblies         = @($items).Count
        PositiveAssemblies = @($items | Where-Object { $_.BracketCount -gt 0 }).Count
        TotalBracketCount  = (@($items | Measure-Object BracketCount -Sum).Sum)
        OutputDirectory    = $OutputPath
    } | Format-List
}
