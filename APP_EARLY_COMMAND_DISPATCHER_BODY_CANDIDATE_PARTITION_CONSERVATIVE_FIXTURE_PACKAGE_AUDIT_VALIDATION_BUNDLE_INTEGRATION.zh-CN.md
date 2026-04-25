# AppEarlyCommandDispatcher Body Candidate Partition Conservative Fixture Package Audit Validation Bundle Integration

## 目标

把阶段 2 validation bundle workflow 直接挂到 `AppEarlyCommandDispatcher`，让 bundle summary/readme/manifest 能像 validation 命令一样被真实应用入口触发。

## 推荐接线点

建议优先级：

1. `DefinitionClauseDecisionDemoAppEntry`
2. `BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAppEarlyCommandDispatcherHook`
3. `BodyCandidatePartitionConservativeFixturePackageAuditValidationAppEarlyCommandDispatcherHook`
4. `BodyCandidatePartitionConservativeFixturePackageAuditAppEarlyCommandDispatcherHook`

## 当前价值

- 验证阶段 2 已不止有“单个工件命令”，而是已有独立 bundle 出口
- 为后续统一 validation/audit summary 聚合提供可直接复用的真实入口
