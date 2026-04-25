# Agent Workflow

## Canonical State

- `MAIN_BODY_REBUILD_STATUS.md` is the primary phase status board.
- `MAIN_BODY_REBUILD_TASKLIST.zh-CN.md` is the current execution order and next-action list.
- `MEM0_WORKFLOW.zh-CN.md` documents the external memory workflow.

Treat the Markdown files as source of truth. Mem0 is only a recovery layer.

## Session Start

1. Read `MAIN_BODY_REBUILD_STATUS.md`.
2. Read `MAIN_BODY_REBUILD_TASKLIST.zh-CN.md`.
3. If `MEM0_API_KEY` and `MEM0_USER_ID` are configured, run `tools/Restore-Mem0ProjectContext.ps1` before doing substantial work.
4. Use restored memories as hints, but resolve conflicts in favor of the Markdown state files.

## Universal Mem0 Protocol

Before starting any substantial task:

1. Search Mem0 for memories related to the current repository, task, module, and goal.
2. Summarize the recovered decisions, conventions, risks, user preferences, and current phase before proceeding.
3. Prefer repository source-of-truth files such as `README`, `STATUS`, `TASKLIST`, `PLAN`, and `DECISIONS` over Mem0 when they conflict.

After meaningful progress:

1. If `MEM0_API_KEY` and `MEM0_USER_ID` are configured, automatically write durable memory without waiting for an extra user prompt.
2. Prefer local direct-import scripts for durable writes:
   - `tools/Save-Mem0Memory.ps1` for one stable fact or decision
   - `tools/Sync-Mem0ProjectState.ps1` for state-board snapshots
3. Focus on milestone outcomes, stable decisions, debugging conclusions worth keeping, workflow conventions, risks, and exact next steps.
4. Treat `add_memory` as optional convenience, not the default canonical write path for this repository.
5. Update or delete incorrect memories instead of stacking contradictory duplicates.

Before task end, task switch, or likely context compaction:

1. If `MEM0_API_KEY` and `MEM0_USER_ID` are configured, automatically save a session summary to Mem0 through the local direct-import workflow.
2. Include:
   - current goal
   - completed work
   - key decisions
   - blockers or risks
   - exact next step

Always tell the user what was recovered from Mem0 before relying on it for further work.

## Milestone Sync

1. Update `MAIN_BODY_REBUILD_STATUS.md` and `MAIN_BODY_REBUILD_TASKLIST.zh-CN.md` after meaningful progress.
2. When Mem0 is configured, run `tools/Sync-Mem0ProjectState.ps1` automatically after milestone-sized updates instead of waiting for a manual reminder.
3. Keep memories focused on stable facts, recent milestones, active next steps, and key risks.

## Never Store In Mem0

- Secrets or credentials
- Raw logs
- Full diffs
- Temporary reasoning traces
- One-off noisy chat turns

## Reusable Prompts

Use or adapt these prompts when the user wants a simple Mem0 workflow without project-specific tuning.

### Session Start Prompt

```text
Before starting work, search Mem0 for memories related to the current repository, task, module, and goal. Summarize the recovered context first, then continue. Prefer repository state files and docs as source of truth when they conflict with memory.
```

### Milestone Sync Prompt

```text
Save the stable results of this work to Mem0 using the local direct-import workflow. Keep only durable information that will help future sessions: what was completed, key decisions, important risks, workflow conventions, and the exact next step.
```

### Pre-Compaction Prompt

```text
Before context is compacted or this session ends, write a Mem0 session summary using the local direct-import workflow. Include the current goal, completed work, key decisions, blockers or risks, and the exact next step. Do not store secrets, raw logs, full diffs, or transient reasoning.
```
