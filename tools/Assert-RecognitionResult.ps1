param(
    [Parameter(Mandatory = $true)]
    [string]$ResultDirectory,

    [int]$ExpectedAssemblies = 246,

    [int]$ExpectedPositiveAssemblies = 7,

    [int]$ExpectedTotalBracketCount = 7,

    [int]$ExpectedMaxBracketCount = 1,

    [int]$ExpectedReviewQueueRows = 7,

    [int]$ExpectedPriorityReviewRows = 18,

    [int]$ExpectedBracketPriorityRows = 10,

    [int]$ExpectedBodyOnlyRows = 8,

    [int]$ExpectedBundleMemberJson = 18,

    [int]$ExpectedBundleAssemblyJson = 36
)

if ($PSVersionTable.PSEdition -ne "Core")
{
    $pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
    if ($null -eq $pwsh)
    {
        throw "pwsh is required to run Assert-RecognitionResult.ps1 reliably."
    }

    $argList = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $PSCommandPath)
    foreach ($entry in $PSBoundParameters.GetEnumerator())
    {
        $argList += "-$($entry.Key)"
        $argList += [string]$entry.Value
    }

    & $pwsh.Source @argList
    exit $LASTEXITCODE
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$resultDirectory = (Resolve-Path $ResultDirectory).Path
$summaryPath = Join-Path $resultDirectory "batch-summary.json"
if (-not (Test-Path $summaryPath))
{
    throw "Missing batch-summary.json: $summaryPath"
}

$summaryItems = @(Get-Content $summaryPath -Raw | ConvertFrom-Json)
$assemblies = $summaryItems.Count
$positiveAssemblies = 0
$totalBracketCount = 0
$maxBracketCount = 0
foreach ($item in $summaryItems)
{
    $bracketCount = [int]$item.BracketCount
    $totalBracketCount += $bracketCount
    if ($bracketCount -gt 0)
    {
        $positiveAssemblies++
    }

    if ($bracketCount -gt $maxBracketCount)
    {
        $maxBracketCount = $bracketCount
    }
}

$reviewQueueRows = $null
$reviewQueuePath = Join-Path $resultDirectory "review-queue.csv"
if (Test-Path $reviewQueuePath)
{
    $reviewQueueRows = @(Import-Csv $reviewQueuePath).Count
}

$priorityRows = $null
$bracketPriorityRows = $null
$bodyOnlyRows = $null
$priorityPath = Join-Path $resultDirectory "priority-review-pack.inline.csv"
if (Test-Path $priorityPath)
{
    $priorityItems = @(Import-Csv $priorityPath)
    $priorityRows = $priorityItems.Count
    $bracketPriorityRows = @($priorityItems | Where-Object { [string]$_.Reasons -like "*BracketPattern*" }).Count
    $bodyOnlyRows = @($priorityItems | Where-Object { [string]$_.Reasons -eq "BodyChange" }).Count
}

$bundleMemberJson = $null
$bundleAssemblyJson = $null
$bundleDirectory = Join-Path $resultDirectory "priority-review-bundle"
if (Test-Path $bundleDirectory)
{
    $membersDirectory = Join-Path $bundleDirectory "members"
    $assembliesDirectory = Join-Path $bundleDirectory "assemblies"

    if (Test-Path $membersDirectory)
    {
        $bundleMemberJson = @(Get-ChildItem $membersDirectory -File).Count
    }

    if (Test-Path $assembliesDirectory)
    {
        $bundleAssemblyJson = @(Get-ChildItem $assembliesDirectory -File).Count
    }
}

$checks = @(
    [pscustomobject]@{ Name = "Assemblies"; Actual = $assemblies; Expected = $ExpectedAssemblies },
    [pscustomobject]@{ Name = "PositiveAssemblies"; Actual = $positiveAssemblies; Expected = $ExpectedPositiveAssemblies },
    [pscustomobject]@{ Name = "TotalBracketCount"; Actual = $totalBracketCount; Expected = $ExpectedTotalBracketCount },
    [pscustomobject]@{ Name = "MaxBracketCount"; Actual = $maxBracketCount; Expected = $ExpectedMaxBracketCount }
)

if ($null -ne $reviewQueueRows)
{
    $checks += [pscustomobject]@{ Name = "ReviewQueueRows"; Actual = $reviewQueueRows; Expected = $ExpectedReviewQueueRows }
}

if ($null -ne $priorityRows)
{
    $checks += [pscustomobject]@{ Name = "PriorityReviewRows"; Actual = $priorityRows; Expected = $ExpectedPriorityReviewRows }
    $checks += [pscustomobject]@{ Name = "BracketPriorityRows"; Actual = $bracketPriorityRows; Expected = $ExpectedBracketPriorityRows }
    $checks += [pscustomobject]@{ Name = "BodyOnlyRows"; Actual = $bodyOnlyRows; Expected = $ExpectedBodyOnlyRows }
}

if ($null -ne $bundleMemberJson)
{
    $checks += [pscustomobject]@{ Name = "BundleMemberJson"; Actual = $bundleMemberJson; Expected = $ExpectedBundleMemberJson }
}

if ($null -ne $bundleAssemblyJson)
{
    $checks += [pscustomobject]@{ Name = "BundleAssemblyJson"; Actual = $bundleAssemblyJson; Expected = $ExpectedBundleAssemblyJson }
}

$mismatches = @($checks | Where-Object { $_.Actual -ne $_.Expected })

[pscustomobject]@{
    ResultDirectory = $resultDirectory
    Assemblies = $assemblies
    PositiveAssemblies = $positiveAssemblies
    TotalBracketCount = $totalBracketCount
    MaxBracketCount = $maxBracketCount
    ReviewQueueRows = $reviewQueueRows
    PriorityReviewRows = $priorityRows
    BracketPriorityRows = $bracketPriorityRows
    BodyOnlyRows = $bodyOnlyRows
    BundleMemberJson = $bundleMemberJson
    BundleAssemblyJson = $bundleAssemblyJson
    CheckCount = $checks.Count
    MismatchCount = $mismatches.Count
} | Format-List

if ($mismatches.Count -gt 0)
{
    Write-Host ""
    Write-Host "Mismatches:"
    $mismatches | Format-Table -AutoSize
    throw "Acceptance validation failed with $($mismatches.Count) mismatches."
}

Write-Host ""
Write-Host "Acceptance checks passed."
