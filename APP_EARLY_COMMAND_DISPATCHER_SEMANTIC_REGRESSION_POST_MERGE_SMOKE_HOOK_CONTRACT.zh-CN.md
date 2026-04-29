# AppEarlyCommandDispatcher Semantic Regression Post-Merge Smoke Hook Contract

## 目的

为既有 `AppEarlyCommandDispatcher` 插入 semantic regression post-merge smoke 命令时，固定最小短路约定，避免真正改 dispatcher 时还要临时决定：

- 命中后如何返回
- `exitCode / message / outputDirectory` 如何映射
- 何时继续后续分支，何时立即短路

## 已有可调用入口

- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter.cs)

推荐调用：

```csharp
if (DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter.TryRun(
        args,
        defaultOutputRootDirectory,
        out var exitCode,
        out var message,
        out var outputDirectory))
{
    // 直接沿用既有早期命令短路模式返回
}
```

## 最小短路约定

### 未命中命令

- `TryRun(...) == false`
- dispatcher 继续执行后续早期命令分支

### 命中命令且 smoke 校验通过

- `TryRun(...) == true`
- `exitCode = 0`
- `message` 返回“已导出并校验通过”的短消息
- dispatcher 直接短路返回，不进入主识别流程

### 命中命令但 smoke 校验失败

- `TryRun(...) == true`
- `exitCode = 1`
- `message` 返回缺失工件的失败摘要
- dispatcher 仍直接短路返回，不进入主识别流程

## 推荐插入位置

在 `AppEarlyCommandDispatcher` 现有最前面的早期命令判断区中，放在：

1. demo/diagnostic 命令分支附近
2. 主识别流程分支之前

原则：

- 越早判断越好
- 命中后绝不继续进入主识别流程

## 下一步

1. 将该 hook 契约落实到 `AppEarlyCommandDispatcher.cs`
2. 用最小 smoke 命令实跑一次
3. 若稳定，再决定是否继续把 post-merge workflow 并入旧 validation bundle 主入口
