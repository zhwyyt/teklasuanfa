# AppEarlyCommandDispatcher Semantic Regression Post-Merge Smoke Integration

## 目的

把 `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmoke` 命令入口进一步收敛成可被既有 `AppEarlyCommandDispatcher` 直接消费的 adapter，避免在 dispatcher 内部关心 smoke 命令自己的结果对象结构。

## 当前新增对象

- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter.cs)

## 调用约定

```csharp
if (DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter.TryRun(
        args,
        defaultOutputRootDirectory,
        out var exitCode,
        out var message,
        out var outputDirectory))
{
    // 命中命令，dispatcher 可直接短路返回
}
```

## adapter 负责内容

- 调用 `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeAppEntry.TryRun(...)`
- 输出：
  - `exitCode`
  - `message`
  - `outputDirectory`
- 返回：
  - `true`：命中命令，应由 dispatcher 短路
  - `false`：未命中命令，继续后续 dispatcher 分支

## 下一步

1. 在既有 `AppEarlyCommandDispatcher` 中插入一次 `TryRun(...)`
2. 命中后沿用既有早期命令短路模式返回
3. 再以最小命令实跑一次 post-merge smoke，验证路径解析与工件落盘稳定
