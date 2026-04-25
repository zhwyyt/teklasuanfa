# Body Candidate Partition Conservative Fixture Stage-2 Aggregate Workflow

## 目标

把阶段 2 当前已稳定跑通的 `package-audit / validation / validation-bundle` 三层结果统一折叠到一个 aggregate workflow 中。

## 当前调用链

1. `BodyCandidatePartitionConservativeFixtureStage2AggregateWorkflow.Run(...)`
2. `BodyCandidatePartitionConservativeFixturePackageAuditExportService.Export(...)`
3. `BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleWorkflow.Run(commandResult)`

## 当前 aggregate 输出

目录：

- `body-candidate-partition-conservative-fixture-stage2-aggregate`

文件：

- `summary.md`
- `README.md`
- `manifest.json`

## 当前聚合语义

- `package-audit` 结果负责稳定提供：
  - package 主工件路径
  - validation 工件路径
  - 样本计数与成功状态
- `validation-bundle` 结果负责稳定提供：
  - summary/readme/manifest 三件套路径

aggregate 自身不重新生成底层 JSON/Markdown，而是把它们组织成一个更高层的目录自描述出口。
