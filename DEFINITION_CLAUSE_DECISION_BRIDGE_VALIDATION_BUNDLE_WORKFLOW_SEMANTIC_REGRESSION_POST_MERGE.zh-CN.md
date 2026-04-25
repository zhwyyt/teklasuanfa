# DefinitionClause Bridge Validation Bundle Workflow Semantic Regression Post Merge

## 目的

在尚未直接改写既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 源码的前提下，先提供一条可运行的“后合并包装层”：

1. 反射调用既有 validation bundle workflow
2. 解析其输出目录、summary、README、manifest 路径
3. 将 semantic regression 的 summary / README / manifest entry 后合并进现有工件

## 当前新增对象

- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeResult.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeResult.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMerge.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMerge.cs)

## 当前策略

- 通过反射寻找 `DefinitionClauseDecisionBridgeValidationBundleWorkflow.Run(string)`
- 优先读取 workflow result 上常见的字符串属性：
  - `OutputDirectory`
  - `SummaryPath / SummaryMarkdownPath / ValidationSummaryPath`
  - `ReadmePath / ReadmeMarkdownPath / ValidationReadmePath`
  - `ManifestPath / ManifestJsonPath / ValidationManifestPath`
- 若属性缺失，则回退到输出目录下的文件名启发式搜索
- manifest 当前采取“保守追加”策略：
  - 追加顶层 `semanticRegression`
  - 若已有 `sections` 数组，则再追加一份 section entry

## 下一步

1. 先将这条 post-merge workflow 接到最小 smoke / demo / sidecar 入口
2. 再决定是否直接把旧 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 本体改成内建 semantic regression
3. 当主链稳定后，逐步删除多余的中间 merger/applicator/helper
