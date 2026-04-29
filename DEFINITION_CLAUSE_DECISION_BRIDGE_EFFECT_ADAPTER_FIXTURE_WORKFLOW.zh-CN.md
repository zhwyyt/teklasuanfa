# DefinitionClause Bridge Effect Adapter Fixture Workflow

## 文档目的

本文档固定 `EffectSnapshot -> RawInputs -> Tier -> Verdict -> PromotionReadiness` 这条新主链的最小 fixture 工件流。

这条 workflow 的定位是：

- 先独立验证新 `EffectAdapter` 主链
- 不等待旧 bridge/fixture sidecar 全部并回后才有工件
- 先把“适配链是否跑通”标准化成 JSON/Markdown 输出

---

## 当前输入

- [DefinitionClauseDecisionBridgeEffectAdapterFixtures.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectAdapterFixtures.cs>)
- [DefinitionClauseDecisionBridgeEffectAdapterFixtureRunner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectAdapterFixtureRunner.cs>)
- [DefinitionClauseDecisionBridgeEffectAdapter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectAdapter.cs>)

---

## 当前输出

- `definition-clause-decision-bridge-effect-adapter-fixtures.json`
- `definition-clause-decision-bridge-effect-adapter-fixtures.md`

对应实现：

- [DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactModels.cs>)
- [DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactBuilder.cs>)
- [DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactSerializer.cs>)
- [DefinitionClauseDecisionBridgeEffectAdapterFixtureWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeEffectAdapterFixtureWorkflow.cs>)

---

## 当前作用

这条 workflow 把新 `EffectAdapter` 主链推进到“可独立导出工件”的层级，方便下一轮：

1. 并回旧 `DefinitionClauseDecisionBridgeFixtures / Runner / ReportBuilder`
2. 并回旧 `fixture/sidecar` 导出链
3. 最后再让 full-run 统一消费

也就是说，当前它不是最终长期入口，但已经让“新适配链是否可用”从内存结果升级成了标准工件。
