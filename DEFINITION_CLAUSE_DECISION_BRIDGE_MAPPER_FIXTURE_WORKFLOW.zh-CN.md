# DefinitionClause Bridge Mapper Fixture Workflow

## 文档目的

本文档说明 bridge mapper 最小 fixture 层如何独立导出工件。

这条 workflow 的定位是：

- 不依赖完整 full-run
- 不依赖旧 sidecar 工作流已经改完
- 先把新的 `tier -> verdict -> promotion` 纯函数层跑成可落盘 JSON/Markdown

---

## 当前输入

- [DefinitionClauseDecisionBridgeMapperFixtures.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapperFixtures.cs>)
- [DefinitionClauseDecisionBridgeMapperFixtureRunner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapperFixtureRunner.cs>)
- [DefinitionClauseDecisionBridgeMapper.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapper.cs>)

---

## 当前输出

- `definition-clause-decision-bridge-mapper-fixtures.json`
- `definition-clause-decision-bridge-mapper-fixtures.md`

对应实现：

- [DefinitionClauseDecisionBridgeMapperFixtureArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeMapperFixtureArtifactBuilder.cs>)
- [DefinitionClauseDecisionBridgeMapperFixtureArtifactSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeMapperFixtureArtifactSerializer.cs>)
- [DefinitionClauseDecisionBridgeMapperFixtureWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeMapperFixtureWorkflow.cs>)

---

## 当前作用

这条 workflow 先把新 bridge mapper 的最小回归结果标准化，方便下一轮：

1. 并回既有 `DefinitionClauseDecisionBridgeFixtures / Runner / ReportBuilder`
2. 接到现有 sidecar/fixture 工件链
3. 再接到更大的 demo/full-run 工作流

当前它不是最终入口，但已经把“新 mapper 层是否跑通”变成可持久化工件，而不只是内存里的结果对象。
