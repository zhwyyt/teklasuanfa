# Body Candidate Partition Conservative Fixture Package Audit Commandline Contract

## 目标

在不改动旧 `AppEarlyCommandDispatcher` 的前提下，先把阶段 2 `body-candidate partition conservative fixture package audit` 收口成一条可独立暴露、可后续接线的命令入口。

## 命令名

- `--body-candidate-partition-conservative-fixture-package-audit`

## 可选参数

- `--output-root <directory>`

未显式传入时，沿用调用方给出的默认输出根目录。

## 当前调用链

1. `BodyCandidatePartitionConservativeFixturePackageAuditCommandHandler`
2. `BodyCandidatePartitionConservativeFixturePackageAuditExportService`
3. `BodyCandidatePartitionConservativeFixturePackageAuditWorkflow`

## 返回约定

`BodyCandidatePartitionConservativeFixturePackageAuditCommandResult` 作为统一出口，至少暴露：

- `ExitCode`
- `OutputDirectory`
- `JsonPath`
- `MarkdownPath`
- `ManifestPath`
- `ReadmePath`
- `ValidationJsonPath`
- `ValidationMarkdownPath`
- `ValidationSucceeded`
- `TotalCount`
- `PassedCount`
- `FailedCount`
- `PackageIsComplete`
- `MissingFiles`

## 退出码语义

- `0`：阶段 2 fixture 全部通过，且 package audit 必需工件完整。
- `1`：存在 fixture 失败，或 package audit 缺工件。
- `-1`：当前参数不属于本命令，留给上层 dispatcher 继续分发。

## 设计约束

- 该层不直接依赖旧 dispatcher 具体实现。
- `ExportService` 通过反射调用既有 `BodyCandidatePartitionConservativeFixturePackageAuditWorkflow`，并在同一次命令链中继续补写 `validation.json / validation.md`。
- 一旦旧入口源码可安全读取，只需插入一次 `BodyCandidatePartitionConservativeFixturePackageAuditAppEarlyCommandDispatcherHook.TryRun(...)` 调用即可接线。
