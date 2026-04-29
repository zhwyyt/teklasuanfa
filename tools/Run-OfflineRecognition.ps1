param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [string]$OutputPath,

    [switch]$SkipBuild,

    [switch]$GenerateReviewQueue,

    [string]$ReviewQueueOutput
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$appProject = Join-Path $repoRoot "src\TeklaBodyBracketRecognition.App\TeklaBodyBracketRecognition.App.csproj"
$buildOutput = Join-Path $repoRoot ".tmpbuild\app-bin"
$nugetConfig = Join-Path $repoRoot "NuGet.Config"
$tmpAppData = Join-Path $repoRoot "tmpdata"
$tmpPackages = Join-Path $repoRoot ".nuget\packages"
$buildTopologyRewriteSummaryScript = Join-Path $repoRoot "tools\Build-TopologyRewriteSummary.ps1"

if ([string]::IsNullOrWhiteSpace($OutputPath))
{
    $leaf = Split-Path $InputPath -Leaf
    if ([string]::IsNullOrWhiteSpace($leaf))
    {
        $leaf = "recognition-input"
    }

    $OutputPath = Join-Path $repoRoot ".tmpresults\$leaf"
}

New-Item -ItemType Directory -Force -Path $buildOutput, $tmpAppData, $tmpPackages | Out-Null

$env:APPDATA = $tmpAppData
$env:NUGET_PACKAGES = $tmpPackages

if (-not $SkipBuild)
{
    dotnet build $appProject `
        -c Release `
        -o $buildOutput `
        --configfile $nugetConfig `
        -p:CodexWorkspaceBuildPaths=true

    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }
}

$appDll = Join-Path $buildOutput "TeklaBodyBracketRecognition.App.dll"
dotnet $appDll $InputPath $OutputPath

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

$coreBodyProofRealInputJson = Join-Path $OutputPath "core-body-proof-real-input.json"
if ((Test-Path $buildTopologyRewriteSummaryScript) -and (Test-Path $coreBodyProofRealInputJson))
{
    & $buildTopologyRewriteSummaryScript `
        -CoreBodyProofRealInputJson $coreBodyProofRealInputJson `
        -OutputMarkdown (Join-Path $OutputPath "topology-rewrite-summary.zh-CN.md")
}

$summaryPath = Join-Path $OutputPath "batch-summary.json"
if (Test-Path $summaryPath)
{
    $items = Get-Content $summaryPath -Raw | ConvertFrom-Json
    $summary = [pscustomobject]@{
        Assemblies          = @($items).Count
        PositiveAssemblies  = @($items | Where-Object { $_.BracketCount -gt 0 }).Count
        TotalBracketCount   = (@($items | Measure-Object BracketCount -Sum).Sum)
        OutputDirectory     = $OutputPath
        SummaryFile         = $summaryPath
    }

    $summary | Format-List
}
else
{
    Write-Host "Recognition completed. Output directory: $OutputPath"
}

if ($GenerateReviewQueue)
{
    $reviewQueueScript = Join-Path $repoRoot "tools\New-ReviewQueue.ps1"
    $reviewQueueArgs = @{
        ResultDirectory = $OutputPath
    }

    if (-not [string]::IsNullOrWhiteSpace($ReviewQueueOutput))
    {
        $reviewQueueArgs.OutputCsv = $ReviewQueueOutput
    }

    & $reviewQueueScript @reviewQueueArgs
}
