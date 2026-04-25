---
name: mem0-codex
description: Repo-local Mem0 overlay for autoteklasuanfa. Applies project-specific source-of-truth and milestone rules on top of the global Mem0 workflow.
---

# Mem0 Codex Project Overlay

Use this skill whenever Mem0 MCP tools are available in the current session.

This repo-local plugin is not the default global memory workflow.
Assume a global Mem0 plugin may already be installed.
This overlay exists only to add autoteklasuanfa-specific recovery and writeback rules.

## Scoping Model

Use the project scope for this repository:

- Global preferences agent id: `codex-global`
- Project memories agent id: `codex-project::autoteklasuanfa`
- Default app id: `codex-desktop`

Project-specific memories should include metadata such as:

- `memory_scope: project`
- `repo: autoteklasuanfa`
- `workflow: main-body-rebuild`
- `repo_path: <workspace-path>` when available

Cross-project user habits or preferences should use:

- `agent_id: codex-global`
- `metadata.memory_scope: global`

## Recovery Order For This Repository

1. Read `MAIN_BODY_REBUILD_STATUS.md`.
2. Read `MAIN_BODY_REBUILD_TASKLIST.zh-CN.md`.
3. Search project memories using `agent_id = codex-project::autoteklasuanfa`.
4. If project memories are sparse, search global preferences using `agent_id = codex-global`.
5. Resolve conflicts in favor of the repository Markdown files.

Bias retrieval toward the active project thread:

- `main body rebuild`
- `phase 5`
- `TopologyRewrite`
- `DefinitionClause`
- `GKZ / HXZ / GL / MJ / YPGL`
- current milestone, risks, and next steps

## After Significant Work

For durable project memory in this repository, prefer the local direct-import scripts over `add_memory`.
If `MEM0_API_KEY` and `MEM0_USER_ID` are configured, do not wait for an extra user reminder before writing durable memory.

Preferred write path:

1. `tools/Save-Mem0Memory.ps1` for one stable fact, decision, or next step
2. `tools/Sync-Mem0ProjectState.ps1` for full `STATUS/TASKLIST` snapshots
3. Use `add_memory` only when shell access is unavailable or a lighter-weight note is enough

When the information is repo-specific, save it under `agent_id = codex-project::autoteklasuanfa`.
Do not use this overlay to store broad cross-project preferences unless the task explicitly requires that.

Good memory categories:

- `decision`
- `task_learning`
- `anti_pattern`
- `user_preference`
- `environmental`
- `convention`

Good memory examples:

- stable main-body rebuild phase transitions
- successful or failed strategies around `TopologyRewrite` / `DefinitionClause`
- durable repository conventions tied to `STATUS/TASKLIST`
- milestone outcomes that future sessions must recover quickly

## Before Losing Context

Before task end or likely context compaction, store a session summary with metadata type `session_state`.
If Mem0 is configured, do this automatically instead of waiting for a manual prompt.

Include:

- goal
- what changed
- files touched
- decisions made
- risks or follow-ups
- exact next step
- repo scope

For this repository, prefer a summary that references:

- current phase
- relevant sample families
- latest validated run or deliverable
- next proof-engine or stage-4.5 follow-up

## For This Repository

Prefer the Markdown state files as source of truth:

- `MAIN_BODY_REBUILD_STATUS.md`
- `MAIN_BODY_REBUILD_TASKLIST.zh-CN.md`

When they exist, recover context in this order:

1. read the status/tasklist files
2. call `search_memories`
3. resolve conflicts in favor of the Markdown files

After milestone-sized work:

1. update the Markdown status files
2. optionally run `tools/Sync-Mem0ProjectState.ps1`
3. run `tools/Save-Mem0Memory.ps1` if a short direct-import note is enough
4. use `add_memory` only as a secondary path, not the default durable write path

## Role Boundary

Use the global Mem0 plugin for:

- cross-project user preferences
- general habits
- broad coding conventions

Use this repo-local overlay for:

- autoteklasuanfa-specific state recovery
- `STATUS/TASKLIST`-first workflow
- main-body-rebuild milestones, risks, and next steps
- project-scoped writeback that should not mix with other repositories

## Never Store

- secrets or credentials
- raw logs
- full diffs
- transient chain-of-thought
- noisy chat filler
