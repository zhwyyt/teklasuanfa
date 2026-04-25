# DefinitionClause Bridge Validation Bundle Semantic Regression Apply

## 目的

把 semantic regression 并回旧 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 的调用面进一步压缩成“单次 Apply”，让主 workflow 不必手动拆分：

- `Merge(...)`
- `summaryMarkdown = ...`
- `readmeMarkdown = ...`
- `manifestEntries.Add(...)`

## 当前新增对象

- [DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeApplicator.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeApplicator.cs)

## 当前调用约定

```csharp
DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeApplicator.Apply(
    outputRootDirectory,
    ref summaryMarkdown,
    ref readmeMarkdown,
    manifestEntries);
```

调用后：

- `summaryMarkdown` 已追加 semantic regression summary block
- `readmeMarkdown` 已追加 semantic regression readme block
- `manifestEntries` 已追加 semantic regression manifest entry

## 下一步

1. 在既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 中直接插入一次 `Apply(...)`
2. 让 workflow result / manifest / summary / README 一次性带上 semantic regression section
3. 接通后再评估能否删除部分中间 builder / composer 层，继续缩并入口
