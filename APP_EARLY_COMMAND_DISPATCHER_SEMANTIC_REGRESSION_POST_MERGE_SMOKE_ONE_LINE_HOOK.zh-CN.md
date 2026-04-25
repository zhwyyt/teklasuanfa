# AppEarlyCommandDispatcher Semantic Regression Post-Merge Smoke One-Line Hook

## 目标

把旧 `AppEarlyCommandDispatcher.cs` 的最终改动面收敛成“一次调用 + 一次 return”。

在已经有：

- `...SmokeDispatcherAdapter`
- `...SmokeDispatchDecision`
- `...SmokeDispatchBridge`

的基础上，再补一层直接面向旧 dispatcher 的 hook：

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeAppEarlyCommandDispatcherHook`

## Hook 责任

该 hook 负责：

1. 调用 `DispatchBridge.Evaluate(args, defaultOutputRootDirectory)`
2. 若未命中命令，则返回 `false`
3. 若命中命令，则在内部打印 `Message / OutputDirectory`
4. 通过 `out int exitCode` 把最终退出码交还旧 dispatcher

这样旧 dispatcher 不需要再理解：

- 多 `out` 参数 adapter
- `Handled / Message / OutputDirectory` decision 对象
- 何时输出目录路径

## 预期 patch 形态

后续真正修改 `AppEarlyCommandDispatcher.cs` 时，目标 patch 可以压缩为：

```csharp
if (DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeAppEarlyCommandDispatcherHook.TryRun(
        args,
        defaultOutputRootDirectory,
        out var smokeExitCode))
{
    return smokeExitCode;
}
```

## 约束

- 未命中命令：`return false`，且 `exitCode=0`
- 命中命令：`return true`，并保证 `exitCode` 与 smoke command handler 一致
- 用户可读输出全部封装在 hook 内部，避免旧 dispatcher 再额外拼 message

## 价值

- 让旧入口 patch 面缩到最低
- 降低未来读旧代码时的上下文需求
- 让 smoke 命令的输出格式在 hook 层集中管理
