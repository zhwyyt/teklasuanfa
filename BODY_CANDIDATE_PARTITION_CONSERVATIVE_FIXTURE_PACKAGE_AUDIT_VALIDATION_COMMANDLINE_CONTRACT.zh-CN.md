# Body Candidate Partition Conservative Fixture Package Audit Validation Commandline Contract

## 目标

把阶段 2 `package-audit validation` 从单纯 workflow 旁路提升为可直接由应用入口触发的一等命令，便于后续继续统一到 validation / audit / bundle 链。

## 命令名

- `--body-candidate-partition-conservative-fixture-package-audit-validation`

## 可选参数

- `--output-root <directory>`

未显式传入时，沿用调用方给出的默认输出根目录。

## 当前调用链

1. `BodyCandidatePartitionConservativeFixturePackageAuditValidationCommandHandler`
2. `BodyCandidatePartitionConservativeFixturePackageAuditValidationExportService`
3. `BodyCandidatePartitionConservativeFixturePackageAuditValidationWorkflow`

## 返回约定

`BodyCandidatePartitionConservativeFixturePackageAuditValidationCommandResult` 作为统一出口，至少暴露：

- `ExitCode`
- `OutputDirectory`
- `ValidationJsonPath`
- `ValidationMarkdownPath`
- `ValidationSucceeded`

## 退出码语义

- `0`：validation workflow 成功，`validation.json / validation.md` 已落出且校验通过。
- `1`：validation workflow 运行完成但处于失败态。
- `-1`：当前参数不属于本命令，留给上层 dispatcher 继续分发。

## 设计约束

- 该命令允许内部继续复用既有 `package-audit` 结果，但对外只暴露 validation 工件链。
- 该命令不替代现有 `--body-candidate-partition-conservative-fixture-package-audit`，而是为后续统一 validation / audit / bundle 入口提供更窄、更稳定的子命令面。
