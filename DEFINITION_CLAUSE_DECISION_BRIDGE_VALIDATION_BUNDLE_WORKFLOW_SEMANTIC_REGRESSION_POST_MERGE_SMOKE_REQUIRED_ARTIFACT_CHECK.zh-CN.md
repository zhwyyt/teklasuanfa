# Definition Clause Decision Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Required Artifact Check

## 目标

在 `OutputContract + OutputPathResolver` 之后，再把“哪些文件算 smoke 输出完成”固定成共享检查层，避免：

- validator 自己硬编码一套必需文件
- dispatcher 最小实跑回查再写第二套
- README / 操作文档再手工列第三套

## 代码入口

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSnapshot`
- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactInspector`

## 当前必需工件

共享检查层当前要求以下 5 个文件必须存在：

1. `manifest.json`
2. `README.md`
3. `validation.json`
4. `validation.md`
5. `bundle-section-summary.md`

这些文件名全部来自：

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract`

路径解析全部来自：

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPathResolver`

## 使用方式

```csharp
var paths =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPathResolver
        .ResolveFromOutputDirectory(outputDirectory);

var snapshot =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactInspector
        .Inspect(paths);

if (!snapshot.AllRequiredArtifactsPresent)
{
    // 统一处理 snapshot.MissingArtifacts
}
```

## 输出对象语义

- `AllRequiredArtifactsPresent`
  表示 5 个共享必需工件是否全部存在
- `MissingArtifacts`
  直接给出缺失文件名数组，便于 validator / command handler / dispatcher 回查统一输出

## 当前价值

- 继续推进“输出契约清理”主线
- 为后续最小 smoke 实跑提供单一“完成态”判定标准
- 降低 validator、dispatcher 回查与文档说明之间的漂移风险
