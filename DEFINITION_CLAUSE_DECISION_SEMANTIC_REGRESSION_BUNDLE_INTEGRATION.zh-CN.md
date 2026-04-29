# DefinitionClause Semantic Regression Bundle Integration

## 目的

在不直接修改既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 主入口的前提下，先把 `DefinitionClauseDecisionSemanticRegressionWorkflow` 的输出收敛成可被 validation bundle 吸纳的 section 形态。

## 当前新增对象

- [DefinitionClauseDecisionSemanticRegressionBundleSection.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSemanticRegressionBundleSection.cs)
- [DefinitionClauseDecisionSemanticRegressionBundleSectionBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSemanticRegressionBundleSectionBuilder.cs)
- [DefinitionClauseDecisionSemanticRegressionBundleSectionMarkdownBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSemanticRegressionBundleSectionMarkdownBuilder.cs)
- [DefinitionClauseDecisionSemanticRegressionBundleAttachment.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSemanticRegressionBundleAttachment.cs)
- [DefinitionClauseDecisionSemanticRegressionBundleAttachmentBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSemanticRegressionBundleAttachmentBuilder.cs)

## 当前约束

- section key 固定为 `semantic-regression`
- section 至少暴露以下路径：
  - snapshot JSON
  - snapshot Markdown
  - manifest JSON
  - README
- section 还必须暴露 `RowCount`，方便统一 bundle summary / README 直接汇总

## 当前 sidecar 自带输出

`DefinitionClauseDecisionSemanticRegressionWorkflow` 现已在自身输出目录中额外落盘：

- `bundle-section.json`
- `bundle-section-summary.md`

这样下一轮把 semantic regression 并入既有 validation bundle 时，可以先按“收现成 section 文件”的方式接线，而不用在 bundle 主入口里重复拼装字段。

## 推荐并回顺序

1. 在既有 validation bundle workflow 里优先直接调用 `DefinitionClauseDecisionSemanticRegressionBundleAttachmentBuilder.Run(...)`
2. 将返回的 `Section` 与 `SummaryMarkdownBlock` 追加到现有 bundle summary / README / manifest builder
3. 如需旁路调试，再单独调用 `DefinitionClauseDecisionSemanticRegressionWorkflow.Run(...)`
4. 待 mapper / effect-adapter fixtures 与 semantic regression snapshot 对齐后，再决定是否保留独立 workflow 入口
