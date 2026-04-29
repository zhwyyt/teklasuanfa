param(
    [Parameter(Mandatory = $true)]
    [string]$Text,
    [string]$ApiKey,
    [string]$UserId,
    [string]$AgentId,
    [string]$AppId,
    [string]$RunId,
    [string]$ProjectId,
    [string]$OrgId,
    [string]$BaseUrl = "https://api.mem0.ai",
    [string]$Repo,
    [string]$Kind = "durable_fact",
    [string]$MemoryScope = "project",
    [string]$MetadataJson,
    [string]$OutputDirectory,
    [switch]$EnableInference,
    [switch]$AsyncMode,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "Mem0.Common.ps1")

$repoRoot = Get-Mem0RepoRoot
$workspaceName = Split-Path -Leaf $repoRoot
$resolvedOutputDirectory = Ensure-DirectoryExists -Path (Resolve-Mem0Setting -Value $OutputDirectory -EnvName "" -DefaultValue (Join-Path $repoRoot ".tmpdata\mem0"))

$resolvedUserId = Resolve-Mem0Setting -Value $UserId -EnvName "MEM0_USER_ID" -DefaultValue "" -Mandatory
$defaultAgentId = Get-DefaultMem0ProjectAgentId -RepoRoot $repoRoot
$resolvedAgentId = Resolve-Mem0Setting -Value $AgentId -EnvName "MEM0_AGENT_ID" -DefaultValue $defaultAgentId
$resolvedAppId = Resolve-Mem0Setting -Value $AppId -EnvName "MEM0_APP_ID" -DefaultValue "codex-desktop"
$resolvedRunId = Resolve-Mem0Setting -Value $RunId -EnvName "MEM0_RUN_ID" -DefaultValue ""
$resolvedProjectId = Resolve-Mem0Setting -Value $ProjectId -EnvName "MEM0_PROJECT_ID" -DefaultValue ""
$resolvedOrgId = Resolve-Mem0Setting -Value $OrgId -EnvName "MEM0_ORG_ID" -DefaultValue ""
$resolvedRepo = Resolve-Mem0Setting -Value $Repo -EnvName "" -DefaultValue $workspaceName

$baseMetadata = @{
    memory_scope = $MemoryScope
    repo         = $resolvedRepo
    kind         = $Kind
    workspace    = $workspaceName
    workspace_path = $repoRoot
    write_mode   = if ($EnableInference) { "inference" } else { "direct_import" }
    saved_at_utc = [DateTimeOffset]::UtcNow.ToString("o")
}

$additionalMetadata = $null
if (-not [string]::IsNullOrWhiteSpace($MetadataJson))
{
    $additionalMetadata = $MetadataJson | ConvertFrom-Json -AsHashtable
}

$metadata = Merge-Mem0Metadata -BaseMetadata $baseMetadata -AdditionalMetadata $additionalMetadata
$payload = New-Mem0WritePayload `
    -UserId $resolvedUserId `
    -AgentId $resolvedAgentId `
    -AppId $resolvedAppId `
    -RunId $resolvedRunId `
    -Text $Text `
    -Metadata $metadata `
    -EnableInference:$EnableInference `
    -AsyncMode:$AsyncMode

if (-not [string]::IsNullOrWhiteSpace($resolvedProjectId))
{
    $payload.project_id = $resolvedProjectId
}

if (-not [string]::IsNullOrWhiteSpace($resolvedOrgId))
{
    $payload.org_id = $resolvedOrgId
}

$previewPath = Join-Path $resolvedOutputDirectory "last-save-preview.txt"
$payloadPreviewPath = Join-Path $resolvedOutputDirectory "last-save-payload.json"
$responsePath = Join-Path $resolvedOutputDirectory "last-save-response.json"
$mirrorPath = Get-Mem0MirrorPath -OutputDirectory $resolvedOutputDirectory

Set-Content -LiteralPath $previewPath -Value $Text -Encoding UTF8
Write-JsonArtifact -Path $payloadPreviewPath -Value $payload

if ($DryRun)
{
    Write-Host "Dry run 完成，未实际调用 Mem0。"
    Write-Host "文本预览：$previewPath"
    Write-Host "请求载荷：$payloadPreviewPath"
    return
}

$resolvedApiKey = Resolve-Mem0Setting -Value $ApiKey -EnvName "MEM0_API_KEY" -DefaultValue "" -Mandatory
$response = Invoke-Mem0Api -ApiKey $resolvedApiKey -Path "/v1/memories/" -Body $payload -Method "POST" -BaseUrl $BaseUrl
Write-JsonArtifact -Path $responsePath -Value $response
Update-Mem0LocalMirror `
    -Path $mirrorPath `
    -Response $response `
    -UserId $resolvedUserId `
    -AgentId $resolvedAgentId `
    -AppId $resolvedAppId `
    -RunId $resolvedRunId `
    -Metadata $metadata | Out-Null

Write-Host "Mem0 单条记忆写入完成。"
Write-Host "文本预览：$previewPath"
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
