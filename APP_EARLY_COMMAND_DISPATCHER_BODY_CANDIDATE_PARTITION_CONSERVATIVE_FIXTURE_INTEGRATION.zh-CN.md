# AppEarlyCommandDispatcher Body Candidate Partition Conservative Fixture Integration

## 目标

在阶段 2 已有：

- `--body-candidate-partition-conservative-fixture`
- `command handler`
- `export service`
- `package workflow`

的基础上，再补最薄的 dispatcher adapter/hook 层。这样一旦后续能安全读取并修改
`AppEarlyCommandDispatcher.cs`，就不需要再为阶段 2 默认导出链重新设计接线面。

## 当前代码入口

- `BodyCandidatePartitionConservativeFixtureDispatcherAdapter.cs`
- `BodyCandidatePartitionConservativeFixtureAppEarlyCommandDispatcherHook.cs`

## Adapter 职责

`BodyCandidatePartitionConservativeFixtureDispatcherAdapter.TryRun(...)`

负责：

1. 判断是否命中 `--body-candidate-partition-conservative-fixture`
2. 调用 `BodyCandidatePartitionConservativeFixtureCommandHandler`
3. 返回：
   - `exitCode`
   - `message`
   - `outputDirectory`

## Hook 职责

`BodyCandidatePartitionConservativeFixtureAppEarlyCommandDispatcherHook.TryRun(...)`

负责：

1. 调用 dispatcher adapter
2. 如果命中，则统一打印：
   - 完成消息
   - 输出目录
3. 返回最终 `exitCode`

## 后续预期 patch 形态

一旦可以安全修改 `AppEarlyCommandDispatcher.cs`，预期只需要插入类似：

```csharp
if (BodyCandidatePartitionConservativeFixtureAppEarlyCommandDispatcherHook.TryRun(
        args,
        defaultOutputRootDirectory,
        out var stage2ExitCode))
{
    return stage2ExitCode;
}
```

## 当前价值

- 让阶段 2 默认导出链具备与 smoke 线路相同的“可接 dispatcher”薄入口
- 后续接入旧 dispatcher 时，不必再回头补命令/消息/输出目录语义
- 继续沿 `body-candidate partition design` 主线推进，而不提前碰旧 heuristics 本体
