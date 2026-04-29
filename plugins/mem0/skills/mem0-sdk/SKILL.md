---
name: mem0-sdk
description: Repo-local Mem0 guidance for autoteklasuanfa. Focuses on project-scoped memory and leaves cross-project behavior to the global plugin.
---

# Mem0 MCP Tools

Prefer these tools:

- `search_memories`: semantic retrieval for relevant context
- `add_memory`: convenience write path when direct import scripts are unavailable
- `get_memories`: inspect scoped memory history when search is too narrow
- `update_memory`: correct a known memory by id
- `delete_memory`: remove a bad or stale memory by id
- `list_entities`: inspect current users, agents, apps, or runs stored in Mem0

For this repository, the default durable write path is:

- `tools/Save-Mem0Memory.ps1`
- `tools/Sync-Mem0ProjectState.ps1`

## Recommended Scoping

When possible, scope memories with stable identifiers such as:

- `user_id`
- `agent_id`
- `app_id`
- `run_id`

Use a two-layer model:

- project memory: `agent_id = codex-project::<repo-name>`
- global memory: `agent_id = codex-global`

For this repository, the concrete project scope is `agent_id = codex-project::autoteklasuanfa`.
Use project scope for repo-specific state, and global scope only for cross-project user preferences or durable coding conventions.

## Retrieval Heuristics

- Search project memory first; do not dump all memories by default.
- Keep the query close to the current repo and task.
- Start with a small result count and only broaden if the first search misses context.
- Search global memory only after the project search or when the task clearly needs cross-project context.
- Prefer repository status files over memory when they conflict.

## Write Heuristics

- Save only information that improves future tasks.
- Prefer short, factual, self-contained memories.
- Update or delete incorrect memories instead of piling on contradictory duplicates.
- Keep this repo-local layer focused on autoteklasuanfa-specific state, not generic cross-project preferences.
- Prefer direct import (`infer=false`) for durable writes in this repository.
