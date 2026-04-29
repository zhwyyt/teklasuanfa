# Mem0 工作流

## 目标

本仓库已经有一套可落地的状态真源：

- `MAIN_BODY_REBUILD_STATUS.md`：阶段状态与最近进展
- `MAIN_BODY_REBUILD_TASKLIST.zh-CN.md`：当前应做什么、先做什么

`Mem0` 在这里不负责替代这些文档，也不能接管聊天客户端内部的上下文压缩。  
它负责做两件事：

- 在会话结束或里程碑完成后，把稳定项目状态同步到外部记忆层
- 在新会话开始时，把和当前项目最相关的记忆拉回来，帮助恢复上下文

另外，现在已经补上“全局一次安装 + 项目自动分桶”的方案：

- 用户级全局插件：装一次即可供所有仓库使用
- 项目级记忆：按仓库名分桶，不和别的仓库混
- 全局级记忆：只放跨项目偏好和通用习惯

简化原则：

- Markdown 文档是真源
- Mem0 是恢复层，不是唯一状态层
- 只存稳定事实、最近里程碑、当前下一步、风险和待验证项
- 不存 secrets、原始日志、完整 diff、临时推理
- 只要 `MEM0_API_KEY` 和 `MEM0_USER_ID` 已配置，默认自动直传，不需要在网页或会话里额外点一次“保存”

---

## 已落地脚本

- [tools/Sync-Mem0ProjectState.ps1](</I:/autoteklasuanfa/tools/Sync-Mem0ProjectState.ps1>)
  - 从 `STATUS/TASKLIST` 生成项目状态摘要
  - 默认用 `direct import` 同步到 `Mem0`
  - 会把摘要预览、请求载荷、响应结果写到 `.tmpdata/mem0/`
  - 若摘要 hash 未变化，会自动跳过重复同步

- [tools/Save-Mem0Memory.ps1](</I:/autoteklasuanfa/tools/Save-Mem0Memory.ps1>)
  - 直接写入单条稳定记忆
  - 默认也是 `direct import`
  - 适合保存里程碑结论、关键决策、下一步

- [tools/Restore-Mem0ProjectContext.ps1](</I:/autoteklasuanfa/tools/Restore-Mem0ProjectContext.ps1>)
  - 从当前项目状态生成默认恢复查询
  - 先走 `v2 search`
  - 若 `v2 search` 异常或没有结果，会退回 `v1 get all + 本地相关性排序`
  - 把结果写成 `.tmpdata/mem0/restored-context.md`

- [tools/Mem0.Common.ps1](</I:/autoteklasuanfa/tools/Mem0.Common.ps1>)
  - 通用辅助函数

## 已落地 Codex 插件

仓库现在还带了一套 repo-local Codex 插件：

- [\.agents/plugins/marketplace.json](/I:/autoteklasuanfa/.agents/plugins/marketplace.json)
- [plugins/mem0/.codex-plugin/plugin.json](/I:/autoteklasuanfa/plugins/mem0/.codex-plugin/plugin.json)
- [plugins/mem0/.codex-mcp.json](/I:/autoteklasuanfa/plugins/mem0/.codex-mcp.json)
- [plugins/mem0/skills/mem0-codex/SKILL.md](/I:/autoteklasuanfa/plugins/mem0/skills/mem0-codex/SKILL.md)

这意味着你现在有两条路：

- 继续用本仓库的 PowerShell 脚本手动同步/恢复记忆
- 直接在 Codex UI 里安装本仓库自带的 `Mem0` 插件，让 MCP 工具和记忆协议自动进会话

另外我也已经在用户目录落了一套 home-local 全局插件：

- `C:\Users\Administrator\.agents\plugins\marketplace.json`
- `C:\Users\Administrator\plugins\mem0-global`
- `C:\Users\Administrator\.codex\mem0-global-prompts.md`

更推荐以后走全局插件，因为它只需要安装一次。

---

## 最小配置

至少配置两个环境变量：

```powershell
$env:MEM0_API_KEY = "your-mem0-api-key"
$env:MEM0_USER_ID = "your-user-id"
```

可选补充作用域。现在脚本会默认按仓库名生成项目 scope，所以这几个变量不是必须：

```powershell
$env:MEM0_AGENT_ID = "codex-project::autoteklasuanfa"
$env:MEM0_APP_ID = "codex-desktop"
$env:MEM0_RUN_ID = ""
```

可选平台级作用域：

```powershell
$env:MEM0_PROJECT_ID = "your-mem0-project-id"
$env:MEM0_ORG_ID = "your-mem0-org-id"
```

---

## 推荐用法

### 默认自动模式

当环境变量已经配置好时，本仓库默认按下面的自动节奏工作：

1. 新会话开始前自动恢复项目记忆
2. 里程碑完成后自动同步项目状态
3. 任务结束、任务切换、或上下文可能压缩前自动写入会话摘要

也就是说，正常情况下不需要你再手动补一句“把这条记忆存进去”，也不需要去 Mem0 网页点保存。

当前仍然保留的边界只有一条：

- Codex 没有“每次文件保存”级别的系统事件钩子，所以这里的自动上传是“会话/任务里程碑级自动”，不是“操作系统文件保存瞬间自动”

### 0. 如果你想直接走 Codex 插件安装

先在启动 Codex 的同一环境里设置：

```powershell
$env:MEM0_API_KEY = "m0-your-api-key"
```

然后：

1. 打开 Codex 的 repo plugin UI
2. 看到 `Mem0 Plugins`
3. 安装其中的 `Mem0`
4. 新开一个任务
5. 让它先执行一句：
   - `List my mem0 entities`
   - 或 `Search my memories for hello`

如果工具能响应，说明插件链已经通了。

### 0.5 全局插件的推荐工作方式

全局插件现在采用两层记忆模型：

- 项目记忆：
  - `agent_id = codex-project::<repo-name>`
  - 用来保存当前仓库的状态、决策、风险、下一步
- 全局记忆：
  - `agent_id = codex-global`
  - 只保存跨项目偏好、写作习惯、通用工作流约定

推荐检索顺序：

1. 先搜当前仓库的项目记忆
2. 不够时再搜全局偏好
3. 如果和仓库文档冲突，以仓库文档为准

### 1. 开新会话时先恢复上下文

```powershell
pwsh -NoProfile -File I:\autoteklasuanfa\tools\Restore-Mem0ProjectContext.ps1
```

如果你想围绕某个具体问题恢复记忆：

```powershell
pwsh -NoProfile -File I:\autoteklasuanfa\tools\Restore-Mem0ProjectContext.ps1 `
  -Query "为什么 GL / MJ / YPGL 仍停在 effect/review 层？"
```

结果会写到：

- `.tmpdata/mem0/restored-context.md`
- `.tmpdata/mem0/last-restore-response.json`

### 2. 里程碑完成后同步项目状态

先更新：

- `MAIN_BODY_REBUILD_STATUS.md`
- `MAIN_BODY_REBUILD_TASKLIST.zh-CN.md`

再执行：

```powershell
pwsh -NoProfile -File I:\autoteklasuanfa\tools\Sync-Mem0ProjectState.ps1
```

如果你明确想走旧的“推断式提炼”，再额外加：

```powershell
pwsh -NoProfile -File I:\autoteklasuanfa\tools\Sync-Mem0ProjectState.ps1 `
  -EnableInference
```

结果会写到：

- `.tmpdata/mem0/last-sync-preview.md`
- `.tmpdata/mem0/last-sync-payload.json`
- `.tmpdata/mem0/last-sync-response.json`

### 3. 直接写入一条稳定记忆

```powershell
pwsh -NoProfile -File I:\autoteklasuanfa\tools\Save-Mem0Memory.ps1 `
  -Text "autoteklasuanfa is in phase 5, focused on TopologyRewrite and DefinitionClause."
```

如果要写中文摘要：

```powershell
pwsh -NoProfile -File I:\autoteklasuanfa\tools\Save-Mem0Memory.ps1 `
  -Text "autoteklasuanfa 当前处于阶段 5，重点在 TopologyRewrite 与 DefinitionClause。"
```

### 4. 无 API Key 时先做 dry run

```powershell
pwsh -NoProfile -File I:\autoteklasuanfa\tools\Sync-Mem0ProjectState.ps1 `
  -UserId "local-dev" `
  -DryRun
```

```powershell
pwsh -NoProfile -File I:\autoteklasuanfa\tools\Save-Mem0Memory.ps1 `
  -UserId "local-dev" `
  -Text "dry-run memory" `
  -DryRun
```

```powershell
pwsh -NoProfile -File I:\autoteklasuanfa\tools\Restore-Mem0ProjectContext.ps1 `
  -UserId "local-dev" `
  -DryRun
```

这样可以先检查摘要内容和查询内容是否合理。

---

## 结合当前项目的建议

当前主线状态已经明确：

- 当前目标：把主材识别重构为“工程定义驱动的主体截面证明器”
- 当前重点：阶段 5 `TopologyRewrite / DefinitionClause` 从 effect 提示继续收紧到更接近定义判定
- 当前优先样本：`GKZ / HXZ / GL / MJ / YPGL`
- 当前真源文件：
  - [MAIN_BODY_REBUILD_STATUS.md](</I:/autoteklasuanfa/MAIN_BODY_REBUILD_STATUS.md>)
  - [MAIN_BODY_REBUILD_TASKLIST.zh-CN.md](</I:/autoteklasuanfa/MAIN_BODY_REBUILD_TASKLIST.zh-CN.md>)

因此推荐节奏是：

1. 新会话先读状态文档
2. 再执行一次 `Restore-Mem0ProjectContext.ps1`
3. 开始本轮编码/验证
4. 里程碑完成后更新状态文档
5. 最后执行 `Sync-Mem0ProjectState.ps1`
6. 如果只想补一条简短稳定事实，再执行 `Save-Mem0Memory.ps1`

在默认自动模式下，第 5 步和第 6 步都可以由 agent 自动完成，不必等你再提醒一次。

---

## 可选：把 Mem0 接到支持 MCP 的客户端

如果你的 AI 客户端支持 MCP，也可以把 `Mem0 MCP` 配进去，让客户端具备 `add_memory / search_memories / update_memory` 能力。

官方文档当前给出的远程 MCP 地址是：

- `https://mcp.mem0.ai/mcp`

但即使开了 MCP，也仍建议保留本仓库里的脚本工作流，因为：

- 项目状态提取逻辑已经和本仓库文档结构绑定
- 当前脚本会自动从 `STATUS/TASKLIST` 生成更适合本项目的摘要
- 脚本会落本地审计文件，便于复盘

参考：

- [Mem0 MCP](https://docs.mem0.ai/platform/mem0-mcp)
- [Mem0 Add Memories](https://docs.mem0.ai/api-reference/memory/add-memories)
- [Mem0 Search Memories](https://docs.mem0.ai/api-reference/memory/search-memories)
- [Mem0 Direct Import](https://docs.mem0.ai/platform/features/direct-import)
