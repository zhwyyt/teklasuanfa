# DefinitionClauseDecision Sidecar 契约

## 目的

定义独立 sidecar 结果文件的最小结构，使 `DefinitionClauseDecision` 能在尚未完全并回主摘要前，先独立落盘并生成：

- `definition-clause-decision-summary.json`
- `definition-clause-decision-summary.zh-CN.md`

## JSON 顶层结构

顶层对象固定包含：

- `RepresentativeRows`
- `AggregateRows`
- `ReviewRows`

## RepresentativeRows

每个元素对应一个代表零件，至少包含：

- `MemberId`
- `AssemblyId`
- `PartId`
- `PartName`
- `DefinitionClauseCode`
- `DefinitionClauseLabelZh`
- `DefinitionClauseEffectCode`
- `DefinitionClauseEffectLabelZh`
- `ClauseVerdictCode`
- `ClauseVerdictLabelZh`
- `ClausePromotionReadinessCode`
- `ClausePromotionReadinessLabelZh`
- `ReviewHint`

## AggregateRows

每个元素对应一个构件级聚合行，至少包含：

- `MemberId`
- `AssemblyId`
- `ClauseVerdicts`
- `LeadClauseVerdictCode`
- `LeadClauseVerdictLabelZh`
- `LeadClauseVerdictShare`
- `ClauseVerdictMixStatus`
- `ClausePromotionReadinesses`
- `LeadClausePromotionReadinessCode`
- `LeadClausePromotionReadinessLabelZh`
- `LeadClausePromotionReadinessShare`
- `ClausePromotionReadinessMixStatus`

## ReviewRows

每个元素对应一个 review 入口行，至少包含：

- `MemberId`
- `AssemblyId`
- `LeadClauseVerdictLabelZh`
- `LeadClausePromotionReadinessLabelZh`
- `ClauseVerdictMixStatus`
- `ClausePromotionReadinessMixStatus`
- `ReviewHint`

## 生成顺序

推荐顺序：

1. 先从引擎结果映射 `RepresentativeRows`
2. 再由聚合 helper 生成 `AggregateRows`
3. 最后由 artifact builder 生成 `ReviewRows`
4. 写出 JSON
5. 调 `Build-DefinitionClauseDecisionSummary.ps1` 生成 Markdown

## 当前脚本入口

独立 Markdown 重建脚本：

- [Build-DefinitionClauseDecisionSummary.ps1](./tools/Build-DefinitionClauseDecisionSummary.ps1)

输入：

- `definition-clause-decision-summary.json`

输出：

- `definition-clause-decision-summary.zh-CN.md`

## 首轮要求

首轮 sidecar 接线完成后，至少要能看到：

- `GKZ` 出现 `定义条款仍成立，但控制路径被改写`
- `HXZ` 出现 `定义条款仍成立，但多数站位路径被改写`
- `MJ` 出现 `定义条款被直接破坏`
- `GL / YPGL` 仍保守维持 `定义条款仍需复核`
