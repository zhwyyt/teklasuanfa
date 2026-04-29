# AppEarlyCommandDispatcher Body Candidate Partition Conservative Fixture Stage-2 Aggregate Integration

## 目标

把阶段 2 aggregate 入口直接挂到 `AppEarlyCommandDispatcher`，让目前已经稳定的三层输出能通过一个顶层命令统一触发。

## 推荐优先级

1. `DefinitionClauseDecisionDemoAppEntry`
2. `BodyCandidatePartitionConservativeFixtureStage2AggregateAppEarlyCommandDispatcherHook`
3. `BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAppEarlyCommandDispatcherHook`
4. `BodyCandidatePartitionConservativeFixturePackageAuditValidationAppEarlyCommandDispatcherHook`
5. `BodyCandidatePartitionConservativeFixturePackageAuditAppEarlyCommandDispatcherHook`

## 当前价值

- 阶段 2 首次拥有统一的顶层 aggregate 命令
- 后续若还要继续补 smoke checklist / audit checklist / release checklist，可优先挂在 aggregate 层，而不是继续增加平行命令数量
