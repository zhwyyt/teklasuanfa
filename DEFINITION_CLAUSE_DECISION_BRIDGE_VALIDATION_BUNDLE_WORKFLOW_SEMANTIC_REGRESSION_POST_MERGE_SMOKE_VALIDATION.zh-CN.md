# DefinitionClause Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Validation

## 目的

让最小 smoke 命令不仅能触发 post-merge workflow，还能立即校验关键输出工件是否全部落盘，避免后续把 smoke 接进 `AppEarlyCommandDispatcher` 后还需要人工翻目录判断成功与否。

## 当前新增对象

- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationResult.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationResult.cs)
- [DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidator.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidator.cs)

## 当前校验范围

- smoke 自身：
  - `manifest.json`
  - `README.md`
- validation bundle 后合并输出：
  - summary
  - README
  - manifest
- semantic regression 输出：
  - snapshot JSON / Markdown
  - manifest / README
  - bundle-section JSON / Markdown

## 当前命令行为

- 若全部文件存在：
  - `ExitCode = 0`
- 若存在缺失：
  - `ExitCode = 1`
  - `Message` 中返回校验失败摘要

## 下一步

1. 将 smoke 命令接入 `AppEarlyCommandDispatcher`
2. 通过最小命令实跑，优先观察路径解析是否稳定
3. 若稳定，再决定是否将 post-merge 路线直接并回旧 validation bundle 主入口
