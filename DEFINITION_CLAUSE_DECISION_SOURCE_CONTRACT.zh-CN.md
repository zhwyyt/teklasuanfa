# DefinitionClauseDecision SourceRow 契约

## 目的

定义 `DefinitionClauseDecision` 从真实结果链进入 sidecar 时的最小中间输入，避免后续出现：

- `Program.cs` 直接手拼 `RepresentativeRow`
- 不同入口对“什么算代表零件”理解不一致
- sidecar builder 和主结果链之间没有稳定适配层

## 当前输入模型

- [DefinitionClauseDecisionSourceModels.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSourceModels.cs)

核心类型：

- `DefinitionClauseDecisionSourceRow`

## SourceRow 字段

每条 `SourceRow` 至少包含：

- `MemberId`
- `AssemblyId`
- `PartId`
- `PartName`
- `IsRepresentative`
- `DefinitionClauseCode`
- `DefinitionClauseLabelZh`
- `DefinitionClauseEffectCode`
- `DefinitionClauseEffectLabelZh`
- `ClauseVerdictCode`
- `ClauseVerdictLabelZh`
- `ClausePromotionReadinessCode`
- `ClausePromotionReadinessLabelZh`

## 适配逻辑

当前统一适配器：

- [DefinitionClauseDecisionMapper.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionMapper.cs)

它负责：

1. 从 `SourceRow` 过滤出 `IsRepresentative = true` 的代表零件
2. 映射成 `DefinitionClauseDecisionRepresentativeRow`
3. 再按 `MemberId + AssemblyId` 聚合成 `DefinitionClauseDecisionAggregateRow`
4. 最终输出 `DefinitionClauseDecisionArtifacts`

## 约束

### IsRepresentative

用于告诉 sidecar：

- 这条零件是否应进入代表零件表

首轮接线中，建议直接沿用当前阶段 5 已经在 `core-body-proof-summary` 中选出来的代表件集合，不要重新发明一套筛选规则。

### ClauseVerdict / ClausePromotionReadiness

这两个字段在 SourceRow 上就应已经完成判定，不应在 mapper 里二次推断。

换句话说：

- bridge / engine 负责“判定”
- mapper 负责“适配”
- artifact builder / presentation 负责“输出”

## 推荐接线顺序

1. 先在引擎或旁路结果构建阶段生成 `SourceRow`
2. 再调用 `DefinitionClauseDecisionMapper.BuildArtifacts(...)`
3. 然后走 `DefinitionClauseDecisionSidecarWorkflow`

## 完成判据

当 `Program.cs` 或等效入口能从真实结果稳定生成 `SourceRow` 并调用 mapper，说明 `DefinitionClauseDecision` 已具备接入 sidecar 的最小闭环。
