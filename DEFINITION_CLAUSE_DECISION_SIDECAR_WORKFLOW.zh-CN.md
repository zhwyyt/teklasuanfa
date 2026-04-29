# DefinitionClauseDecision Sidecar Workflow

## 目标

把当前已经拆出来的 `DefinitionClauseDecision` 旁路组件，收敛成统一的落盘 workflow，避免后续出现：

- 引擎接线时直接手写 JSON
- 摘要层再手写 Markdown
- fixture 自检和 summary 产物走两套不同的文件命名和编码策略

## 当前统一入口

统一 helper：

- [DefinitionClauseDecisionSidecarWorkflow.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSidecarWorkflow.cs)

它现在负责两条 sidecar 通道：

### 1. Summary sidecar

固定文件名：

- `definition-clause-decision-summary.json`
- `definition-clause-decision-summary.zh-CN.md`

调用：

- `WriteSummaryArtifacts(outputDirectory, artifacts)`

### 2. Fixture sidecar

固定文件名：

- `definition-clause-decision-fixture-report.json`
- `definition-clause-decision-fixture-report.md`

调用：

- `WriteFixtureArtifacts(outputDirectory, artifacts)`
- 或 `WriteDefaultFixtureArtifacts(outputDirectory)`

## 依赖组件

### Summary 通道

- [DefinitionClauseDecisionArtifactModels.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionArtifactModels.cs)
- [DefinitionClauseDecisionArtifactBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionArtifactBuilder.cs)
- [DefinitionClauseDecisionSidecarSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSidecarSerializer.cs)
- [DefinitionClauseDecisionPresentation.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionPresentation.cs)

### Fixture 通道

- [DefinitionClauseDecisionBridgeFixtures.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeFixtures.cs)
- [DefinitionClauseDecisionBridgeFixtureRunner.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeFixtureRunner.cs)
- [DefinitionClauseDecisionFixtureArtifactBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFixtureArtifactBuilder.cs)
- [DefinitionClauseDecisionFixtureSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFixtureSerializer.cs)
- [DefinitionClauseDecisionBridgeFixtureReportBuilder.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeFixtureReportBuilder.cs)

## 落盘约束

- JSON 一律带 BOM UTF-8
- Markdown 一律带 BOM UTF-8
- 文件命名固定，不在每个调用点重新发明

## 下一步接线

接入真实结果链时，推荐顺序：

1. 在 `Program.cs` 或等效入口先接 fixture sidecar
2. 再接 summary sidecar
3. sidecar 稳定后，再决定是否把 `ClauseVerdict / ClausePromotionReadiness` 并回主摘要

## 完成判据

当下列两类文件都能在真实 run 目录稳定落盘时，认为 sidecar workflow 接线完成：

- `definition-clause-decision-summary.json/.md`
- `definition-clause-decision-fixture-report.json/.md`
