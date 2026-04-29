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
    [string]$Query,
    [int]$TopK = 6,
    [double]$Threshold = 0.35,
    [switch]$Rerank,
    [switch]$KeywordSearch,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "Mem0.Common.ps1")

function Convert-SearchResultsToMarkdown
{
    param(
        [string]$ResolvedQuery,
        [pscustomobject]$Snapshot,
        [object[]]$Results,
        [string]$SourceLabel = "v2-search"
    )

    $Results = @($Results)

    $lines = @(
        "# Mem0 Project Context",
        "",
        "Workspace: $($Snapshot.WorkspacePath)",
        "GeneratedAtUtc: $([DateTimeOffset]::UtcNow.ToString('o'))",
        "",
        "Query:",
        "",
        $ResolvedQuery,
        "",
        "Source: $SourceLabel",
        "",
        "Results: $($Results.Count)",
        ""
    )

    if ($Results.Count -eq 0)
    {
        $lines += "- No memories returned."
        return $lines
    }

    for ($index = 0; $index -lt $Results.Count; $index += 1)
    {
        $item = $Results[$index]
        $lines += "## Result $($index + 1)"
        $lines += ""
        $lines += "- Id: $($item.id)"

        if ($item.PSObject.Properties.Name -contains "user_id" -and -not [string]::IsNullOrWhiteSpace([string]$item.user_id))
        {
            $lines += "- UserId: $($item.user_id)"
        }

        if ($item.PSObject.Properties.Name -contains "created_at" -and -not [string]::IsNullOrWhiteSpace([string]$item.created_at))
        {
            $lines += "- CreatedAt: $($item.created_at)"
        }

        if ($item.PSObject.Properties.Name -contains "updated_at" -and -not [string]::IsNullOrWhiteSpace([string]$item.updated_at))
        {
            $lines += "- UpdatedAt: $($item.updated_at)"
        }

        if ($item.PSObject.Properties.Name -contains "categories" -and $null -ne $item.categories)
        {
            $categoryValues = @($item.categories)
            if ($categoryValues.Count -gt 0)
            {
                $lines += "- Categories: $($categoryValues -join ', ')"
            }
        }

        if ($item.PSObject.Properties.Name -contains "metadata" -and $null -ne $item.metadata)
        {
            $metadataJson = $item.metadata | ConvertTo-Json -Compress -Depth 20
            $lines += "- MetadataJson: $metadataJson"
        }

        $lines += ""
        $lines += [string]$item.memory
        $lines += ""
    }

    return $lines
}

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

$snapshot = Get-ProjectStateSnapshot -RepoRoot $repoRoot -StatusPath $resolvedStatusPath -TaskListPath $resolvedTaskListPath
$resolvedQuery = if ([string]::IsNullOrWhiteSpace($Query))
{
    New-Mem0DefaultQuery -Snapshot $snapshot -AdditionalFocus ""
}
else
{
    $Query.Trim()
}

$filters = New-Mem0SearchFilters -UserId $resolvedUserId -AgentId $resolvedAgentId -AppId $resolvedAppId -RunId $resolvedRunId
$payload = @{
    query           = $resolvedQuery
    remote_filters  = New-Mem0RetrievalFilters -UserId $resolvedUserId -AppId $resolvedAppId -RunId $resolvedRunId
    local_scope     = @{
        repo           = $snapshot.WorkspaceName
        workspace_path = $repoRoot
        agent_id       = $resolvedAgentId
    }
    top_k           = $TopK
    fields          = @("id", "memory", "user_id", "created_at", "updated_at", "metadata", "categories")
}

$queryPreviewPath = Join-Path $resolvedOutputDirectory "last-restore-query.txt"
$payloadPreviewPath = Join-Path $resolvedOutputDirectory "last-restore-payload.json"
$responsePath = Join-Path $resolvedOutputDirectory "last-restore-response.json"
$markdownPath = Join-Path $resolvedOutputDirectory "restored-context.md"
$mirrorPath = Get-Mem0MirrorPath -OutputDirectory $resolvedOutputDirectory

Set-Content -LiteralPath $queryPreviewPath -Value $resolvedQuery -Encoding UTF8
Write-JsonArtifact -Path $payloadPreviewPath -Value $payload

if ($DryRun)
{
    Write-Host "Dry run 完成，未实际调用 Mem0。"
    Write-Host "查询预览：$queryPreviewPath"
    Write-Host "请求载荷：$payloadPreviewPath"
    return
}

$resolvedApiKey = Resolve-Mem0Setting -Value $ApiKey -EnvName "MEM0_API_KEY" -DefaultValue "" -Mandatory
$listResponse = $null
$listError = $null
$mirrorError = $null
$mirrorResults = @()
$results = @()
$sourceLabel = "v3-get-all-rerank"

if ($results.Count -eq 0)
{
    try
    {
        $listResponse = Invoke-Mem0GetAllMemories `
            -ApiKey $resolvedApiKey `
            -UserId $resolvedUserId `
            -AppId $resolvedAppId `
            -RunId $resolvedRunId `
            -BaseUrl $BaseUrl

        $listItems = @(Get-Mem0ResultItems -Response $listResponse)
        $projectScopedItems = @(Select-Mem0ProjectScopedResults `
            -Results $listItems `
            -Repo $snapshot.WorkspaceName `
            -RepoRoot $repoRoot `
            -AgentId $resolvedAgentId)

        $results = @(Select-RelevantMem0Memories -Results $projectScopedItems -Query $resolvedQuery -TopK $TopK)
    }
    catch
    {
        $listError = $_.Exception.Message
    }
}

$results = @($results)

if ($results.Count -eq 0)
{
    try
    {
        $mirrorEntries = @(Read-Mem0LocalMirror -Path $mirrorPath)
        $mirrorResults = @(Select-Mem0MirrorEntries `
            -Entries $mirrorEntries `
            -UserId $resolvedUserId `
            -AgentId $resolvedAgentId `
            -AppId $resolvedAppId `
            -RunId $resolvedRunId `
            -Query $resolvedQuery `
            -TopK $TopK)

        if ($mirrorResults.Count -gt 0)
        {
            $results = @($mirrorResults)
            $sourceLabel = "local-mirror-fallback"
        }
    }
    catch
    {
        $mirrorError = $_.Exception.Message
    }
}

if ($null -eq $listResponse -and $results.Count -eq 0)
{
    throw "Mem0 检索失败。v3 get-all 错误：$listError；local mirror fallback 错误：$mirrorError"
}

$artifact = @{
    query         = $resolvedQuery
    source        = $sourceLabel
    search_error  = $null
    list_error    = $listError
    mirror_error  = $mirrorError
    search_response = $null
    list_response = $listResponse
    mirror_path   = $mirrorPath
    results       = $results
}

Write-JsonArtifact -Path $responsePath -Value $artifact
$markdownLines = Convert-SearchResultsToMarkdown -ResolvedQuery $resolvedQuery -Snapshot $snapshot -Results $results -SourceLabel $sourceLabel
Set-Content -LiteralPath $markdownPath -Value $markdownLines -Encoding UTF8

Write-Host "Mem0 项目上下文恢复完成。"
Write-Host "查询预览：$queryPreviewPath"
Write-Host "响应输出：$responsePath"
Write-Host "Markdown 摘要：$markdownPath"
Write-Host "本地镜像：$mirrorPath"
Write-Host "结果数量：$($results.Count)"
