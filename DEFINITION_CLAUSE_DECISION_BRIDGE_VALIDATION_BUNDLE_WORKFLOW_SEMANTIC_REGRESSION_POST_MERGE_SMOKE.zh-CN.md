# DefinitionClause Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke

## 目的

在真正改写既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 主入口之前，先提供一条最小 smoke workflow：

1. 执行旧 validation bundle workflow
2. 执行 semantic regression post-merge
3. 将关键输出路径整理成独立 smoke `manifest.json + README.md`

## 当前新增对象

- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeManifest.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeManifest.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeSerializer.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeReadmeBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeReadmeBuilder.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeWorkflow.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeWorkflow.cs)

## 输出目录

- `definition-clause-decision-bridge-validation-bundle-semantic-regression-post-merge-smoke/`

目录内固定文件：

- `manifest.json`
- `README.md`

## 下一步

1. 将该 smoke workflow 接到最小 smoke / demo / sidecar 入口
2. 实跑后确认：
   - 旧 validation bundle workflow 的输出路径解析稳定
   - semantic regression summary / README / manifest 后合并稳定
3. 验证稳定后，再决定直接修改旧 validation bundle 主入口还是继续保留 post-merge 包装层
