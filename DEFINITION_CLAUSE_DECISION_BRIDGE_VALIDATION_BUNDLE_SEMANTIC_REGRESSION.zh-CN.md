# DefinitionClause Bridge Validation Bundle Semantic Regression Contribution

## 目的

让 semantic regression 的并回入口直接贴近既有 `DefinitionClauseDecisionBridgeValidationBundle*` 命名体系，避免后续在主 bundle workflow 中再显式处理多层外部 helper。

## 当前新增对象

- [DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContribution.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContribution.cs)
- [DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionBuilder.cs)
- [DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionManifestEntry.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionManifestEntry.cs)
- [DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionComposer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionComposer.cs)
- [DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger.cs)

## 当前职责

- `ContributionBuilder.Build(outputRootDirectory)`
  - 内部调用 `DefinitionClauseDecisionSemanticRegressionBundleAttachmentBuilder.Run(...)`
  - 返回 semantic regression 的 attachment
  - 同时暴露可直接并入 bundle summary / README 的 Markdown block
- `ContributionComposer`
  - 负责将 contribution 转成 manifest entry
  - 负责将 summary / README block 以统一追加规则并回既有 Markdown 文本

## 下一步

1. 在既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 中优先直接调用 `DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger.Merge(...)`
2. 将 `MergeResult.SummaryMarkdown / ReadmeMarkdown` 并回现有 bundle 汇总链
3. 将 `MergeResult.ManifestEntry` 并入 bundle manifest / README
