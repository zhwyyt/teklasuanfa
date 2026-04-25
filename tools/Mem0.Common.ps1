Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-Mem0RepoRoot
{
    return (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

function Resolve-Mem0Setting
{
    param(
        [string]$Value,
        [string]$EnvName,
        [string]$DefaultValue,
        [switch]$Mandatory
    )

    if (-not [string]::IsNullOrWhiteSpace($Value))
    {
        return $Value.Trim()
    }

    if (-not [string]::IsNullOrWhiteSpace($EnvName))
    {
        $environmentValue = [Environment]::GetEnvironmentVariable($EnvName)
        if (-not [string]::IsNullOrWhiteSpace($environmentValue))
        {
            return $environmentValue.Trim()
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($DefaultValue))
    {
        return $DefaultValue.Trim()
    }

    if ($Mandatory)
    {
        throw "缺少必需配置：参数值为空，且环境变量 '$EnvName' 未设置。"
    }

    return $null
}

function Resolve-ProjectFilePath
{
    param(
        [string]$Path,
        [string]$RepoRoot,
        [string]$DefaultRelativePath
    )

    $resolvedInput = $Path
    if ([string]::IsNullOrWhiteSpace($resolvedInput))
    {
        $resolvedInput = Join-Path $RepoRoot $DefaultRelativePath
    }
    elseif (-not [System.IO.Path]::IsPathRooted($resolvedInput))
    {
        $resolvedInput = Join-Path $RepoRoot $resolvedInput
    }

    return (Resolve-Path -LiteralPath $resolvedInput).Path
}

function Ensure-DirectoryExists
{
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path))
    {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }

    return (Resolve-Path -LiteralPath $Path).Path
}

function Trim-LineCollection
{
    param([string[]]$Lines)

    if ($null -eq $Lines -or $Lines.Count -eq 0)
    {
        return @()
    }

    $start = 0
    $end = $Lines.Count - 1

    while ($start -le $end -and [string]::IsNullOrWhiteSpace($Lines[$start]))
    {
        $start += 1
    }

    while ($end -ge $start -and [string]::IsNullOrWhiteSpace($Lines[$end]))
    {
        $end -= 1
    }

    if ($start -gt $end)
    {
        return @()
    }

    return @($Lines[$start..$end])
}

function Get-MarkdownSectionLines
{
    param(
        [string[]]$Lines,
        [string]$Heading
    )

    $headings = @()
    for ($index = 0; $index -lt $Lines.Count; $index += 1)
    {
        if ($Lines[$index] -match '^(#+)\s+(.+?)\s*$')
        {
            $headings += [pscustomobject]@{
                Index = $index
                Level = $matches[1].Length
                Text  = $matches[2].Trim()
            }
        }
    }

    $target = $headings |
        Where-Object { $_.Text -eq $Heading } |
        Select-Object -First 1

    if ($null -eq $target)
    {
        return @()
    }

    $nextHeading = $headings |
        Where-Object { $_.Index -gt $target.Index -and $_.Level -le $target.Level } |
        Select-Object -First 1

    $start = $target.Index + 1
    $end = if ($null -ne $nextHeading) { $nextHeading.Index - 1 } else { $Lines.Count - 1 }

    if ($start -gt $end)
    {
        return @()
    }

    return Trim-LineCollection -Lines @($Lines[$start..$end])
}

function Get-MarkdownSectionText
{
    param(
        [string[]]$Lines,
        [string]$Heading,
        [string]$FallbackText = ""
    )

    $sectionLines = @(Get-MarkdownSectionLines -Lines $Lines -Heading $Heading)
    if ($sectionLines.Count -eq 0)
    {
        return $FallbackText
    }

    return ($sectionLines -join "`n").Trim()
}

function Get-PhaseStatusSnapshot
{
    param([string[]]$StatusLines)

    $phaseSectionLines = @(Get-MarkdownSectionLines -Lines $StatusLines -Heading "阶段状态")
    if ($phaseSectionLines.Count -eq 0)
    {
        return @()
    }

    $currentPhase = $null
    $result = @()
    foreach ($line in $phaseSectionLines)
    {
        if ($line -match '^###\s+(.+?)\s*$')
        {
            $currentPhase = $matches[1].Trim()
            continue
        }

        if ($null -ne $currentPhase -and $line -match '^-+\s*状态：\s*`?([^`]+?)`?\s*$')
        {
            $result += [pscustomobject]@{
                Phase  = $currentPhase
                Status = $matches[1].Trim()
            }
        }
    }

    return @($result)
}

function Get-RecentNumberedItems
{
    param(
        [string[]]$Lines,
        [int]$Count = 5
    )

    $items = @(
        $Lines |
            Where-Object { $_ -match '^\d+\.\s+' } |
            ForEach-Object { $_.Trim() }
    )

    if ($items.Count -le $Count)
    {
        return $items
    }

    return @($items[($items.Count - $Count)..($items.Count - 1)])
}

function Get-LinesBeforeHeading
{
    param(
        [string[]]$Lines,
        [string]$Heading
    )

    for ($index = 0; $index -lt $Lines.Count; $index += 1)
    {
        if ($Lines[$index] -match '^(#+)\s+(.+?)\s*$' -and $matches[2].Trim() -eq $Heading)
        {
            if ($index -eq 0)
            {
                return @()
            }

            return @($Lines[0..($index - 1)])
        }
    }

    return @($Lines)
}

function Get-TextOrFallback
{
    param(
        [string]$Text,
        [string]$FallbackText
    )

    if ([string]::IsNullOrWhiteSpace($Text))
    {
        return $FallbackText
    }

    return $Text.Trim()
}

function ConvertTo-Mem0ScopeSegment
{
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value))
    {
        return "unknown-workspace"
    }

    $normalized = $Value.Trim().ToLowerInvariant()
    $normalized = [regex]::Replace($normalized, '[^a-z0-9]+', '-')
    $normalized = $normalized.Trim('-')

    if ([string]::IsNullOrWhiteSpace($normalized))
    {
        return "unknown-workspace"
    }

    return $normalized
}

function Get-DefaultMem0ProjectAgentId
{
    param([string]$RepoRoot)

    $workspaceName = Split-Path -Leaf $RepoRoot
    return "codex-project::{0}" -f (ConvertTo-Mem0ScopeSegment -Value $workspaceName)
}

function Merge-Mem0Metadata
{
    param(
        [hashtable]$BaseMetadata,
        [hashtable]$AdditionalMetadata
    )

    $merged = @{}

    if ($null -ne $BaseMetadata)
    {
        foreach ($item in $BaseMetadata.GetEnumerator())
        {
            $merged[$item.Key] = $item.Value
        }
    }

    if ($null -ne $AdditionalMetadata)
    {
        foreach ($item in $AdditionalMetadata.GetEnumerator())
        {
            $merged[$item.Key] = $item.Value
        }
    }

    return $merged
}

function New-Mem0WritePayload
{
    param(
        [string]$UserId,
        [string]$AgentId,
        [string]$AppId,
        [string]$RunId,
        [string]$Text,
        [hashtable]$Metadata,
        [switch]$EnableInference,
        [switch]$AsyncMode
    )

    $payload = @{
        user_id       = $UserId
        agent_id      = $AgentId
        app_id        = $AppId
        messages      = @(
            @{
                role    = "user"
                content = $Text
            }
        )
        metadata      = $Metadata
        infer         = [bool]$EnableInference
        output_format = "v1.1"
        version       = "v2"
        async_mode    = [bool]$AsyncMode
    }

    if (-not [string]::IsNullOrWhiteSpace($RunId))
    {
        $payload.run_id = $RunId
    }

    return $payload
}

function Get-ProjectStateSnapshot
{
    param(
        [string]$RepoRoot,
        [string]$StatusPath,
        [string]$TaskListPath,
        [int]$RecentMilestoneCount = 5
    )

    $statusLines = @([string[]](Get-Content -LiteralPath $StatusPath))
    $taskListLines = @([string[]](Get-Content -LiteralPath $TaskListPath))

    $historyLines = Get-LinesBeforeHeading -Lines $statusLines -Heading "当前下一步"

    return [pscustomobject]@{
        WorkspaceName       = Split-Path -Leaf $RepoRoot
        WorkspacePath       = $RepoRoot
        StatusPath          = $StatusPath
        TaskListPath        = $TaskListPath
        CurrentDate         = Get-TextOrFallback -Text (Get-MarkdownSectionText -Lines $statusLines -Heading "当前日期") -FallbackText "未提取到当前日期"
        CurrentGoal         = Get-TextOrFallback -Text (Get-MarkdownSectionText -Lines $statusLines -Heading "当前目标") -FallbackText "未提取到当前目标"
        PhaseStatuses       = @(Get-PhaseStatusSnapshot -StatusLines $statusLines)
        RecentMilestones    = @(Get-RecentNumberedItems -Lines $historyLines -Count $RecentMilestoneCount)
        StatusNextSteps     = Get-TextOrFallback -Text (Get-MarkdownSectionText -Lines $statusLines -Heading "当前下一步") -FallbackText "未提取到状态板的当前下一步"
        TaskListNextSteps   = Get-TextOrFallback -Text (Get-MarkdownSectionText -Lines $taskListLines -Heading "当前下一步") -FallbackText "未提取到任务清单的当前下一步"
        GeneratedAtUtc      = [DateTimeOffset]::UtcNow.ToString("o")
    }
}

function New-ProjectMemorySummaryText
{
    param([pscustomobject]$Snapshot)

    $phaseLines = if ($Snapshot.PhaseStatuses.Count -gt 0)
    {
        @(
            $Snapshot.PhaseStatuses |
                ForEach-Object { "- $($_.Phase)：$($_.Status)" }
        ) -join "`n"
    }
    else
    {
        "- 未提取到阶段状态"
    }

    $recentMilestoneLines = if ($Snapshot.RecentMilestones.Count -gt 0)
    {
        $Snapshot.RecentMilestones -join "`n"
    }
    else
    {
        "- 未提取到最近里程碑"
    }

    return @"
项目：$($Snapshot.WorkspaceName) 主材识别重构
工作区：$($Snapshot.WorkspacePath)
状态快照生成时间（UTC）：$($Snapshot.GeneratedAtUtc)

状态文档日期：
$($Snapshot.CurrentDate)

当前目标：
$($Snapshot.CurrentGoal)

阶段状态快照：
$phaseLines

最近里程碑：
$recentMilestoneLines

当前下一步（状态板）：
$($Snapshot.StatusNextSteps)

当前下一步（任务清单）：
$($Snapshot.TaskListNextSteps)

真源说明：
- Markdown 状态文件是真源：优先信任 MAIN_BODY_REBUILD_STATUS.md 与 MAIN_BODY_REBUILD_TASKLIST.zh-CN.md。
- Mem0 只负责辅助恢复：只保存稳定项目事实、最近里程碑、当前下一步、关键风险与待验证项。
- 不要保存 secrets、原始日志、完整 diff、临时推理或噪音对话。
"@
}

function New-Mem0DefaultQuery
{
    param(
        [pscustomobject]$Snapshot,
        [string]$AdditionalFocus
    )

    $query = @(
        "恢复 $($Snapshot.WorkspaceName) 主材识别重构上下文。"
        "优先返回与当前目标、阶段状态、最近里程碑、阶段 5 TopologyRewrite / DefinitionClause、当前下一步、风险与待验证项最相关的记忆。"
        "当前目标是：$($Snapshot.CurrentGoal)"
    ) -join " "

    if (-not [string]::IsNullOrWhiteSpace($AdditionalFocus))
    {
        $query += " 附加关注点：$AdditionalFocus"
    }

    return $query
}

function New-Mem0SearchFilters
{
    param(
        [string]$UserId,
        [string]$AgentId,
        [string]$AppId,
        [string]$RunId
    )

    $clauses = @()
    if (-not [string]::IsNullOrWhiteSpace($UserId))
    {
        $clauses += @{ user_id = $UserId }
    }

    if (-not [string]::IsNullOrWhiteSpace($AgentId))
    {
        $clauses += @{ agent_id = $AgentId }
    }

    if (-not [string]::IsNullOrWhiteSpace($AppId))
    {
        $clauses += @{ app_id = $AppId }
    }

    if (-not [string]::IsNullOrWhiteSpace($RunId))
    {
        $clauses += @{ run_id = $RunId }
    }

    if ($clauses.Count -eq 0)
    {
        throw "搜索 Mem0 时至少需要 user_id / agent_id / app_id / run_id 中的一个过滤条件。"
    }

    if ($clauses.Count -eq 1)
    {
        return $clauses[0]
    }

    return @{ AND = $clauses }
}

function New-Mem0RetrievalFilters
{
    param(
        [string]$UserId,
        [string]$AppId,
        [string]$RunId
    )

    $clauses = @()
    if (-not [string]::IsNullOrWhiteSpace($UserId))
    {
        $clauses += @{ user_id = $UserId }
    }

    if (-not [string]::IsNullOrWhiteSpace($AppId))
    {
        $clauses += @{ app_id = $AppId }
    }

    if (-not [string]::IsNullOrWhiteSpace($RunId))
    {
        $clauses += @{ run_id = $RunId }
    }

    if ($clauses.Count -eq 0)
    {
        throw "远端枚举 Mem0 时至少需要 user_id / app_id / run_id 中的一个过滤条件。"
    }

    if ($clauses.Count -eq 1)
    {
        return $clauses[0]
    }

    return @{ AND = $clauses }
}

function Invoke-Mem0Api
{
    param(
        [string]$ApiKey,
        [string]$Path,
        [object]$Body,
        [ValidateSet("GET", "POST", "PUT", "DELETE")]
        [string]$Method = "POST",
        [string]$BaseUrl = "https://api.mem0.ai"
    )

    $uri = "{0}{1}" -f $BaseUrl.TrimEnd('/'), $Path
    $headers = @{
        Authorization = "Token $ApiKey"
        Accept        = "application/json"
    }

    if ($null -eq $Body)
    {
        return Invoke-RestMethod -Method $Method -Uri $uri -Headers $headers
    }

    $jsonBody = $Body | ConvertTo-Json -Depth 100
    return Invoke-RestMethod -Method $Method -Uri $uri -Headers $headers -ContentType "application/json" -Body $jsonBody
}

function Invoke-Mem0GetAllMemories
{
    param(
        [string]$ApiKey,
        [string]$UserId,
        [string]$AppId,
        [string]$RunId,
        [string]$BaseUrl = "https://api.mem0.ai",
        [int]$PageSize = 100
    )

    $filters = New-Mem0RetrievalFilters -UserId $UserId -AppId $AppId -RunId $RunId
    $payload = @{
        filters   = $filters
        page      = 1
        page_size = $PageSize
    }

    return Invoke-Mem0Api -ApiKey $ApiKey -Path "/v3/memories/" -Body $payload -Method "POST" -BaseUrl $BaseUrl
}

function Get-Mem0ResultItems
{
    param([object]$Response)

    if ($null -eq $Response)
    {
        return @()
    }

    if ($Response -is [System.Array])
    {
        return @($Response)
    }

    if ($Response.PSObject.Properties.Name -contains "results")
    {
        return @($Response.results)
    }

    return @($Response)
}

function Get-Mem0SearchableText
{
    param([object]$Item)

    $parts = @()

    if ($Item.PSObject.Properties.Name -contains "memory" -and -not [string]::IsNullOrWhiteSpace([string]$Item.memory))
    {
        $parts += [string]$Item.memory
    }

    if ($Item.PSObject.Properties.Name -contains "metadata" -and $null -ne $Item.metadata)
    {
        $parts += ($Item.metadata | ConvertTo-Json -Compress -Depth 20)
    }

    if ($Item.PSObject.Properties.Name -contains "categories" -and $null -ne $Item.categories)
    {
        $parts += (@($Item.categories) -join " ")
    }

    return ($parts -join " ").ToLowerInvariant()
}

function Get-Mem0MetadataValue
{
    param(
        [object]$Item,
        [string]$Key
    )

    if ($null -eq $Item)
    {
        return $null
    }

    $metadata = $null
    if ($Item -is [System.Collections.IDictionary])
    {
        if ($Item.Contains("metadata"))
        {
            $metadata = $Item["metadata"]
        }
    }
    elseif ($Item.PSObject.Properties.Name -contains "metadata")
    {
        $metadata = $Item.metadata
    }

    if ($null -eq $metadata)
    {
        return $null
    }

    if ($metadata -is [System.Collections.IDictionary])
    {
        if ($metadata.Contains($Key))
        {
            return [string]$metadata[$Key]
        }

        return $null
    }

    if ($metadata.PSObject.Properties.Name -contains $Key)
    {
        return [string]$metadata.$Key
    }

    return $null
}

function Select-Mem0ProjectScopedResults
{
    param(
        [object[]]$Results,
        [string]$Repo,
        [string]$RepoRoot,
        [string]$AgentId
    )

    $items = @($Results)
    if ($items.Count -eq 0)
    {
        return @()
    }

    $projectScoped = @(
        $items |
            Where-Object {
                $repoValue = Get-Mem0MetadataValue -Item $_ -Key "repo"
                $workspaceValue = Get-Mem0MetadataValue -Item $_ -Key "workspace"
                $workspacePathValue = Get-Mem0MetadataValue -Item $_ -Key "workspace_path"
                $agentValue = if ($_.PSObject.Properties.Name -contains "agent_id") { [string]$_.agent_id } else { $null }

                (($repoValue -eq $Repo) -or
                 ($workspaceValue -eq $Repo) -or
                 ($workspacePathValue -eq $RepoRoot) -or
                 ((-not [string]::IsNullOrWhiteSpace($AgentId)) -and ($agentValue -eq $AgentId)))
            }
    )

    if ($projectScoped.Count -gt 0)
    {
        return @($projectScoped)
    }

    return @($items)
}

function Select-RelevantMem0Memories
{
    param(
        [object[]]$Results,
        [string]$Query,
        [int]$TopK = 6
    )

    $resultItems = @($Results)
    if ($resultItems.Count -eq 0)
    {
        return @()
    }

    $tokens = @(
        [regex]::Matches(($Query ?? "").ToLowerInvariant(), '[\p{L}\p{Nd}_:-]{2,}') |
            ForEach-Object { $_.Value } |
            Select-Object -Unique
    )

    $scored = @(
        foreach ($item in $resultItems)
        {
            $searchableText = Get-Mem0SearchableText -Item $item
            $score = 0
            foreach ($token in $tokens)
            {
                if ($searchableText.Contains($token))
                {
                    $score += 1
                }
            }

            [pscustomobject]@{
                Score = $score
                Item  = $item
            }
        }
    )

    $positive = @($scored | Where-Object { $_.Score -gt 0 } | Sort-Object Score -Descending)
    if ($positive.Count -gt 0)
    {
        return @($positive | Select-Object -First $TopK | ForEach-Object { $_.Item })
    }

    return @($resultItems | Select-Object -First $TopK)
}

function Write-JsonArtifact
{
    param(
        [string]$Path,
        [object]$Value
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory))
    {
        Ensure-DirectoryExists -Path $directory | Out-Null
    }

    $json = $Value | ConvertTo-Json -Depth 100
    Set-Content -LiteralPath $Path -Value $json -Encoding UTF8
}

function Get-Mem0MirrorPath
{
    param([string]$OutputDirectory)

    return Join-Path $OutputDirectory "local-memory-mirror.json"
}

function Read-Mem0LocalMirror
{
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path))
    {
        return @()
    }

    $content = Get-Content -LiteralPath $Path -Raw
    if ([string]::IsNullOrWhiteSpace($content))
    {
        return @()
    }

    $parsed = $content | ConvertFrom-Json
    return @($parsed)
}

function Write-Mem0LocalMirror
{
    param(
        [string]$Path,
        [object[]]$Entries
    )

    Write-JsonArtifact -Path $Path -Value @($Entries)
}

function ConvertTo-Mem0MirrorEntries
{
    param(
        [object]$Response,
        [string]$UserId,
        [string]$AgentId,
        [string]$AppId,
        [string]$RunId,
        [hashtable]$Metadata
    )

    $items = @(Get-Mem0ResultItems -Response $Response)
    $entries = @()
    foreach ($item in $items)
    {
        $memoryText = $null
        if ($item.PSObject.Properties.Name -contains "memory")
        {
            $memoryText = [string]$item.memory
        }
        elseif ($item.PSObject.Properties.Name -contains "data" -and $null -ne $item.data -and $item.data.PSObject.Properties.Name -contains "memory")
        {
            $memoryText = [string]$item.data.memory
        }

        if ([string]::IsNullOrWhiteSpace($memoryText))
        {
            continue
        }

        $entries += [pscustomobject]@{
            id         = if ($item.PSObject.Properties.Name -contains "id") { [string]$item.id } else { "" }
            memory     = $memoryText
            user_id    = $UserId
            agent_id   = $AgentId
            app_id     = $AppId
            run_id     = $RunId
            metadata   = $Metadata
            created_at = [DateTimeOffset]::UtcNow.ToString("o")
            source     = "local_mirror"
        }
    }

    return @($entries)
}

function Update-Mem0LocalMirror
{
    param(
        [string]$Path,
        [object]$Response,
        [string]$UserId,
        [string]$AgentId,
        [string]$AppId,
        [string]$RunId,
        [hashtable]$Metadata
    )

    $existing = @(Read-Mem0LocalMirror -Path $Path)
    $incoming = @(ConvertTo-Mem0MirrorEntries -Response $Response -UserId $UserId -AgentId $AgentId -AppId $AppId -RunId $RunId -Metadata $Metadata)

    if ($incoming.Count -eq 0)
    {
        return @($existing)
    }

    $combined = @($existing)
    foreach ($entry in $incoming)
    {
        $replaced = $false
        for ($index = 0; $index -lt $combined.Count; $index += 1)
        {
            $current = $combined[$index]
            if ((-not [string]::IsNullOrWhiteSpace([string]$entry.id)) -and ([string]$current.id -eq [string]$entry.id))
            {
                $combined[$index] = $entry
                $replaced = $true
                break
            }
        }

        if (-not $replaced)
        {
            $combined += $entry
        }
    }

    Write-Mem0LocalMirror -Path $Path -Entries $combined
    return @($combined)
}

function Select-Mem0MirrorEntries
{
    param(
        [object[]]$Entries,
        [string]$UserId,
        [string]$AgentId,
        [string]$AppId,
        [string]$RunId,
        [string]$Query,
        [int]$TopK = 6
    )

    $filtered = @(
        @($Entries) |
            Where-Object {
                (([string]$_.user_id) -eq $UserId) -and
                (([string]$_.agent_id) -eq $AgentId) -and
                (([string]$_.app_id) -eq $AppId) -and
                ([string]::IsNullOrWhiteSpace($RunId) -or ([string]$_.run_id) -eq $RunId)
            }
    )

    return @(Select-RelevantMem0Memories -Results $filtered -Query $Query -TopK $TopK)
}

function Get-StringSha256
{
    param([string]$Text)

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try
    {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        $hashBytes = $sha256.ComputeHash($bytes)
        return ([System.BitConverter]::ToString($hashBytes)).Replace("-", "").ToLowerInvariant()
    }
    finally
    {
        $sha256.Dispose()
    }
}
