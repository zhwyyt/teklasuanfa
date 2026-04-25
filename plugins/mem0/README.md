# Mem0 Project Overlay

This repository ships a repo-local Mem0 plugin for Codex, but it is meant to be a project-specific overlay, not your universal default memory workflow.

Recommended layering:

- global plugin: `Mem0 Global`
- repo plugin: `Mem0 Project Overlay` for autoteklasuanfa-specific rules

Use the global plugin for cross-project behavior.
Use this repo-local plugin only when you want autoteklasuanfa-specific recovery and writeback behavior.

For reliable writes in this repository, prefer the local direct-import scripts over raw `add_memory` calls:

- `tools/Save-Mem0Memory.ps1`
- `tools/Sync-Mem0ProjectState.ps1`

## Install

1. Export your Mem0 API key in the shell used by Codex:

```powershell
$env:MEM0_API_KEY = "m0-your-api-key"
$env:MEM0_USER_ID = "your-user-id"
```

2. In Codex, open the repo plugin marketplace and install `Mem0 Project Overlay` only if you want this repository's special rules.

3. Start a new task and verify the tools appear:

- `list_entities`
- `search_memories`
- `add_memory`

## What This Overlay Adds

- `MAIN_BODY_REBUILD_STATUS.md` and `MAIN_BODY_REBUILD_TASKLIST.zh-CN.md` as source of truth
- project scope pinned to `codex-project::autoteklasuanfa`
- priority on `main-body-rebuild`, `phase 5`, `TopologyRewrite`, `DefinitionClause`, and current next steps
- optional use of `tools/Restore-Mem0ProjectContext.ps1` and `tools/Sync-Mem0ProjectState.ps1`
- direct-import-first write behavior for durable repository memories

## Files

- `.codex-plugin/plugin.json`: plugin manifest
- `.codex-mcp.json`: Mem0 MCP server wiring
- `skills/mem0-codex/SKILL.md`: Codex memory protocol
- `skills/mem0-sdk/SKILL.md`: tool usage guidance
