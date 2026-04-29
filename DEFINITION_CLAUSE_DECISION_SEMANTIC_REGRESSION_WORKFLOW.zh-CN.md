# DefinitionClause Semantic Regression Workflow

## 目的

将 `DefinitionClauseDecisionSemanticRegressionCatalog` 与 `DefinitionClauseDecisionSemanticRegressionSnapshot` 固化成一条独立 sidecar 输出链，先把语义回归样本落成稳定 JSON / Markdown 工件，再逐步并回既有 `fixture / artifact / validation bundle` 导出链。

## 当前输出

工作流入口：

- [DefinitionClauseDecisionSemanticRegressionWorkflow.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSemanticRegressionWorkflow.cs)

输出目录：

- `definition-clause-semantic-regression/`

目录内固定两份文件：

- `definition-clause-semantic-regression-snapshot.json`
- `definition-clause-semantic-regression-snapshot.md`

目录内同时保留自描述文件：

- `manifest.json`
- `README.md`

目录内还会补充 bundle 集成辅助文件：

- `bundle-section.json`
- `bundle-section-summary.md`

## 当前职责拆分

- `DefinitionClauseDecisionSemanticRegressionCatalog`
  - 冻结最小语义回归样本
- `DefinitionClauseDecisionSemanticRegressionSnapshot`
  - 将 catalog 转成稳定字符串快照
- `DefinitionClauseDecisionSemanticRegressionSnapshotReportBuilder`
  - 将快照转成 Markdown 报告
- `DefinitionClauseDecisionSemanticRegressionArtifactBuilder`
  - 将 snapshot/report 收成应用层 artifact
- `DefinitionClauseDecisionSemanticRegressionArtifactSerializer`
  - 负责带 BOM UTF-8 落盘 JSON / Markdown
- `DefinitionClauseDecisionSemanticRegressionWorkflow`
  - 负责输出目录命名、`manifest.json` / `README.md` 生成，以及整体协调

## 下一步并回顺序

1. 优先将该 workflow 的 JSON / Markdown 输出并入现有 validation bundle
2. 然后把 semantic regression snapshot 与既有 `DefinitionClauseDecisionBridgeMapperFixtures`
   和 `DefinitionClauseDecisionBridgeEffectAdapterFixtures` 对齐
3. 最后再把这条独立 workflow 缩并回既有 `fixture / sidecar` 统一出口
