# Body Candidate Partition Conservative Fixture Commandline Contract

## 目标

在阶段 2 已有 package workflow 的基础上，再补一层最小 command/export/app-entry 契约，方便后续一旦能够安全接入早期命令分发器，就可以直接暴露一个阶段 2 默认导出入口。

## 当前代码入口

- `BodyCandidatePartitionConservativeFixtureCommandResult.cs`
- `BodyCandidatePartitionConservativeFixtureExportService.cs`
- `BodyCandidatePartitionConservativeFixtureCommandHandler.cs`
- `BodyCandidatePartitionConservativeFixtureAppEntry.cs`

## 当前命令

```text
--body-candidate-partition-conservative-fixture
```

## 可选参数

```text
--output-root <directory>
```

如果未提供，则使用调用方传入的 `defaultOutputRootDirectory`。

## 当前行为

命中命令后，将调用：

- `BodyCandidatePartitionConservativeFixturePackageWorkflow.RunDefault(outputRootDirectory)`

并输出：

- `Output directory`
- `JSON`
- `Markdown`
- `Manifest`
- `README`
- `Passed x/y`

## ExitCode 规则

- `FailedCount == 0` 时：`ExitCode = 0`
- `FailedCount > 0` 时：`ExitCode = 1`

## 当前价值

- 为阶段 2 默认导出链提供最小可运行入口
- 后续一旦能安全读取 `AppEarlyCommandDispatcher.cs`，即可像其他早期命令一样接入
- 继续沿 `body-candidate partition design` 主线推进，而不提前碰旧 heuristics 本体
