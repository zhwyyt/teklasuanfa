# DefinitionClause Bridge Validation Bundle Semantic Regression Merge

## 目的

把 semantic regression 并回旧 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 主链所需的调用面压缩成一个 `Merge(...)` 入口，避免在旧 workflow 中再分别调用：

- contribution builder
- manifest entry composer
- summary markdown composer
- README markdown composer

## 当前新增对象

- [DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeResult.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeResult.cs)
- [DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger.cs)

## 当前调用约定

`DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger.Merge(...)` 接收：

- `outputRootDirectory`
- `existingSummaryMarkdown`
- `existingReadmeMarkdown`

返回：

- `Contribution`
- `ManifestEntry`
- `SummaryMarkdown`
- `ReadmeMarkdown`

## 下一步

1. 在既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 中优先直接调用 `DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeApplicator.Apply(...)`
2. 仅在需要更细粒度控制时，才单独调用 `Merge(...)`
3. 接通后再评估是否可以删除部分中间 helper，继续缩并 semantic regression 独立入口
