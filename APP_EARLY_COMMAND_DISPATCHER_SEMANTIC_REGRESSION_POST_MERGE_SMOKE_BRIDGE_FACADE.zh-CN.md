# AppEarlyCommandDispatcher Semantic Regression Post-Merge Smoke Bridge Facade

## 目标

在真正修改 `AppEarlyCommandDispatcher.cs` 之前，再压一层最薄 facade，把既有
`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter`
的多 `out` 参数形态收敛成单返回对象。

这样旧 dispatcher 真正落 patch 时，只需要：

1. 调一次 `DispatchBridge.Evaluate(args, defaultOutputRootDirectory)`
2. 判断 `decision.Handled`
3. 若已处理则读取 `decision.ExitCode / decision.Message / decision.OutputDirectory`

## 新增类型

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision`
- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchBridge`

## 约束

- `Handled=false` 时，不产生副作用，`ExitCode=0`
- `Handled=true` 时，保留 adapter 已经算出的 `ExitCode / Message / OutputDirectory`
- facade 只做形态收敛，不改变原 adapter 的命令匹配或导出行为

## 预期落地效果

后续真正修改 `AppEarlyCommandDispatcher.cs` 时，可把 patch 面缩到类似：

```csharp
var smokeDecision =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchBridge
        .Evaluate(args, defaultOutputRootDirectory);

if (smokeDecision.Handled)
{
    // dispatcher 内按既有风格输出 message / outputDirectory
    return smokeDecision.ExitCode;
}
```

## 价值

- 降低旧入口 patch 时的局部理解成本
- 减少 dispatcher 里出现多 `out` 参数的拼装噪声
- 便于后续继续扩展成统一 early-command decision 风格
