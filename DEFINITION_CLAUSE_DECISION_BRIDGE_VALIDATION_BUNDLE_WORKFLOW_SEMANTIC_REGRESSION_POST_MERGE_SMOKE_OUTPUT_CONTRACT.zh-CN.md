# Definition Clause Decision Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Output Contract

## 目标

把 `post-merge smoke` 线路中已经反复出现的目录名、关键文件名冻结成独立契约，避免：

- validator 再手写一套文件名
- dispatcher smoke 文档再口头列一遍
- 未来旧 `AppEarlyCommandDispatcher` 接入后，summary/readme/manifest 的检查点出现漂移

## 契约常量

代码入口：

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract`

当前冻结的名称：

- 输出目录名  
  `definition-clause-decision-bridge-validation-bundle-semantic-regression-post-merge-smoke`
- manifest  
  `manifest.json`
- README  
  `README.md`
- smoke validation JSON  
  `validation.json`
- smoke validation Markdown  
  `validation.md`
- semantic regression bundle section Markdown  
  `bundle-section-summary.md`

## 使用约束

后续以下模块应优先引用这份 contract，而不是重新硬编码：

1. post-merge smoke workflow/export service
2. smoke validator / validation artifact builder
3. AppEarlyCommandDispatcher smoke hook 文档与最小实跑说明
4. semantic regression 并回 validation bundle 后的 summary/readme/manifest 核对逻辑

## 当前价值

- 继续完成“输出契约清理”主线，不必等待旧入口源码可读
- 为下一轮真正做最小 smoke 实跑时，提供单一命名真源
- 降低后续 validator / manifest 合并 / README 校验之间的文件名漂移风险
