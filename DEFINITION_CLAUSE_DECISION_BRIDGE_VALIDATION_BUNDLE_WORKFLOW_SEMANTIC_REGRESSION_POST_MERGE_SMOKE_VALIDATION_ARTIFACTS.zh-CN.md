# DefinitionClause Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Validation Artifacts

## 目的

让最小 smoke 命令在完成导出与校验后，额外落盘一份可读的校验工件，便于后续实跑时快速区分：

- 是 post-merge 本体失败
- 还是路径解析成功但部分工件未落盘

## 当前新增对象

- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifacts.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifacts.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactBuilder.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactSerializer.cs)

## 当前输出

在 smoke 输出目录额外落盘：

- `validation.json`
- `validation.md`

## 当前约定

- `validation.json`
  - 机器可读，包含 `Succeeded / Summary / MissingPaths`
- `validation.md`
  - 人类可读，便于快速查看缺失工件

## 下一步

1. 将 smoke 命令接入 `AppEarlyCommandDispatcher`
2. 通过最小实跑直接观察 `validation.md`
3. 若路径解析稳定，再继续推进旧 validation bundle 主链的真正内建接线
