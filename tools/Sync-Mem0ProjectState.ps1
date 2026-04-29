param(
    [string]$ApiKey,
    [string]$UserId,
    [string]$AgentId,
    [string]$AppId,
    [string]$RunId,
    [string]$ProjectId,
    [string]$OrgId,
    [string]$BaseUrl = "https://api.mem0.ai",
    [string]$StatusPath,
    [string]$TaskListPath,
    [string]$OutputDirectory,
    [int]$RecentMilestoneCount = 5,
    [switch]$EnableInference,
    [switch]$DisableInference,
    [switch]$AsyncMode,
    [string]$ExpirationDate,
    [switch]$Force,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "Mem0.Common.ps1")

$repoRoot = Get-Mem0RepoRoot
$resolvedStatusPath = Resolve-ProjectFilePath -Path $StatusPath -RepoRoot $repoRoot -DefaultRelativePath "MAIN_BODY_REBUILD_STATUS.md"
$resolvedTaskListPath = Resolve-ProjectFilePath -Path $TaskListPath -RepoRoot $repoRoot -DefaultRelativePath "MAIN_BODY_REBUILD_TASKLIST.zh-CN.md"
$resolvedOutputDirectory = Ensure-DirectoryExists -Path (Resolve-Mem0Setting -Value $OutputDirectory -EnvName "" -DefaultValue (Join-Path $repoRoot ".tmpdata\mem0"))

$resolvedUserId = Resolve-Mem0Setting -Value $UserId -EnvName "MEM0_USER_ID" -DefaultValue "" -Mandatory
$defaultAgentId = Get-DefaultMem0ProjectAgentId -RepoRoot $repoRoot
$resolvedAgentId = Resolve-Mem0Setting -Value $AgentId -EnvName "MEM0_AGENT_ID" -DefaultValue $defaultAgentId
$resolvedAppId = Resolve-Mem0Setting -Value $AppId -EnvName "MEM0_APP_ID" -DefaultValue "codex-desktop"
$resolvedRunId = Resolve-Mem0Setting -Value $RunId -EnvName "MEM0_RUN_ID" -DefaultValue ""
$resolvedProjectId = Resolve-Mem0Setting -Value $ProjectId -EnvName "MEM0_PROJECT_ID" -DefaultValue ""
$resolvedOrgId = Resolve-Mem0Setting -Value $OrgId -EnvName "MEM0_ORG_ID" -DefaultValue ""

if ($EnableInference -and $DisableInference)
{
    throw "不能同时指定 -EnableInference 与 -DisableInference。"
}

$useInference = [bool]$EnableInference -and (-not [bool]$DisableInference)

$snapshot = Get-ProjectStateSnapshot -RepoRoot $repoRoot -StatusPath $resolvedStatusPath -TaskListPath $resolvedTaskListPath -RecentMilestoneCount $RecentMilestoneCount
$summaryText = New-ProjectMemorySummaryText -Snapshot $snapshot
$summaryHash = Get-StringSha256 -Text $summaryText

$previewPath = Join-Path $resolvedOutputDirectory "last-sync-preview.md"
$payloadPreviewPath = Join-Path $resolvedOutputDirectory "last-sync-payload.json"
$responsePath = Join-Path $resolvedOutputDirectory "last-sync-response.json"
$hashPath = Join-Path $resolvedOutputDirectory "last-sync.sha256"
$mirrorPath = Get-Mem0MirrorPath -OutputDirectory $resolvedOutputDirectory

Set-Content -LiteralPath $previewPath -Value $summaryText -Encoding UTF8

$metadata = @{
    source          = "project_state_sync"
    workspace       = $snapshot.WorkspaceName
    workspace_path  = $snapshot.WorkspacePath
    status_file     = $snapshot.StatusPath
    tasklist_file   = $snapshot.TaskListPath
    sync_hash       = $summaryHash
    synced_at_utc   = $snapshot.GeneratedAtUtc
    recent_count    = $RecentMilestoneCount
    memory_scope    = "project"
    repo            = $snapshot.WorkspaceName
    write_mode      = if ($useInference) { "inference" } else { "direct_import" }
}

$payload = New-Mem0WritePayload `
    -UserId $resolvedUserId `
    -AgentId $resolvedAgentId `
    -AppId $resolvedAppId `
    -RunId $resolvedRunId `
    -Text $summaryText `
    -Metadata $metadata `
    -EnableInference:$useInference `
    -AsyncMode:$AsyncMode

if (-not [string]::IsNullOrWhiteSpace($resolvedProjectId))
{
    $payload.project_id = $resolvedProjectId
}

if (-not [string]::IsNullOrWhiteSpace($resolvedOrgId))
{
    $payload.org_id = $resolvedOrgId
}

if (-not [string]::IsNullOrWhiteSpace($ExpirationDate))
{
    $payload.expiration_date = $ExpirationDate
}

Write-JsonArtifact -Path $payloadPreviewPath -Value $payload

if ((-not $Force) -and (Test-Path -LiteralPath $hashPath))
{
    $lastHash = (Get-Content -LiteralPath $hashPath -Raw).Trim()
    if ($lastHash -eq $summaryHash)
    {
        Write-Host "项目状态摘要未变化，已跳过 Mem0 同步。"
        Write-Host "预览文件：$previewPath"
        return
    }
}

if ($DryRun)
{
    Write-Host "Dry run 完成，未实际调用 Mem0。"
    Write-Host "摘要预览：$previewPath"
    Write-Host "请求载荷：$payloadPreviewPath"
    return
}

$resolvedApiKey = Resolve-Mem0Setting -Value $ApiKey -EnvName "MEM0_API_KEY" -DefaultValue "" -Mandatory
$response = Invoke-Mem0Api -ApiKey $resolvedApiKey -Path "/v1/memories/" -Body $payload -Method "POST" -BaseUrl $BaseUrl

Write-JsonArtifact -Path $responsePath -Value $response
Set-Content -LiteralPath $hashPath -Value $summaryHash -Encoding UTF8
Update-Mem0LocalMirror `
    -Path $mirrorPath `
    -Response $response `
    -UserId $resolvedUserId `
    -AgentId $resolvedAgentId `
    -AppId $resolvedAppId `
    -RunId $resolvedRunId `
    -Metadata $metadata | Out-Null

Write-Host "Mem0 项目状态同步完成。"
Write-Host "摘要预览：$previewPath"
Write-Host "响应输出：$responsePath"
Write-Host "本地镜像：$mirrorPath"

$responseItems = @(Get-Mem0ResultItems -Response $response)
if ($responseItems.Count -gt 0 -and $responseItems[0].PSObject.Properties.Name -contains "id")
{
    Write-Host "MemoryId：$($responseItems[0].id)"
}

if ($responseItems.Count -gt 0)
{
    if ($responseItems[0].PSObject.Properties.Name -contains "memory")
    {
        Write-Host "Memory：$($responseItems[0].memory)"
    }
    elseif ($responseItems[0].PSObject.Properties.Name -contains "data" -and $null -ne $responseItems[0].data -and $responseItems[0].data.PSObject.Properties.Name -contains "memory")
    {
        Write-Host "Memory：$($responseItems[0].data.memory)"
    }
}
