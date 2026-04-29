# DefinitionClause Validation Bundle Semantic Regression Post-Merge Smoke Commandline Contract

## 目的

为 `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeWorkflow` 提供一条独立命令行入口契约，方便后续接入 `Program.cs` 或既有早期 dispatcher。

## 当前新增对象

- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandResult.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandResult.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeExportService.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeExportService.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandHandler.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandHandler.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeAppEntry.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeAppEntry.cs)

## 命令格式

```text
--definition-clause-validation-bundle-semantic-regression-post-merge-smoke
```

可选参数：

```text
--output-root <path>
```

## 约定

- 命令命中后：
  - 执行 post-merge smoke workflow
  - 校验关键输出文件是否落盘
  - 返回 `OutputDirectory / ManifestPath / ReadmePath`
- 未命中时：
  - `Handled = false`
  - 不影响主识别流程

命中后退出码约定：

- `0`：post-merge smoke 导出且关键输出校验通过
- `1`：导出完成但关键输出存在缺失文件

## 下一步

1. 将该入口并入 `Program.cs` 或既有 `AppEarlyCommandDispatcher`
2. 用最小 smoke 命令实跑一次，确认 post-merge 路径解析与工件落盘稳定
