# DefinitionClauseDecision Full-Run Source Artifact 契约

## 目的

本文档定义 `DefinitionClauseDecision` 在真实 full-run 结果链接线时，第一层可落盘工件的文件格式与职责边界。

这层工件不直接输出 `ClauseVerdict`，而是输出：

- 当前 full-run 结果里，哪些构件进入了 `DefinitionClauseDecision` 旁路；
- 当前有哪些代表零件被送入 `representative-part source`；
- 每个代表零件带着哪些 `TopologyRewrite / DefinitionClause / core-review role` 证据进入后续 snapshot / sidecar。

---

## 文件目标

真实 full-run 结果接入后，应优先落出两类文件：

1. `definition-clause-decision-fullrun-source.json`
2. `definition-clause-decision-fullrun-source.zh-CN.md`

说明：

- `.json` 面向后续脚本和 sidecar 管道；
- `.md` 面向人工 review；
- 它们都位于 `snapshot / summary sidecar` 之前，属于“真实输入自检层”。

---

## JSON 结构

顶层对象包含三部分：

1. `Assemblies`
2. `RepresentativeParts`
3. `Summary`

### Assemblies

每项对应一个 full-run 构件级输入快照，至少包含：

- `SourceFile`
- `MemberId`
- `AssemblyId`
- `InputMainPartId`
- `ViewKind`
- `BodyDescriptorFamily`
- `BodyDescriptorSectionType`
- `HasTopologyRewrite`
- `LeadClauseCode`
- `LeadClauseLabelZh`
- `LeadClauseShare`
- `ClauseMix`

### RepresentativeParts

每项对应一个进入 `DefinitionClauseDecision` 旁路的代表零件，至少包含：

- `SourceFile`
- `MemberId`
- `AssemblyId`
- `InputMainPartId`
- `ViewKind`
- `RepresentativePartId`
- `RepresentativePartName`
- `RepresentativePartProfile`
- `IsInputMainPart`
- `IsCurrentCorePart`
- `IsCurrentReviewPart`
- `TopologyRewritePatternCode`
- `TopologyRewritePatternLabelZh`
- `TopologyRewriteControllerRoleCode`
- `TopologyRewriteControllerRoleLabelZh`
- `TopologyRewriteShapeRoleCode`
- `TopologyRewriteShapeRoleLabelZh`
- `TopologyRewriteFamilyRiskCode`
- `TopologyRewriteFamilyRiskLabelZh`
- `TopologyRewriteProofTypeCode`
- `TopologyRewriteProofTypeLabelZh`
- `TopologyRewriteFamilyProofTargetCode`
- `TopologyRewriteFamilyProofTargetLabelZh`
- `TopologyRewriteDefinitionClauseCode`
- `TopologyRewriteDefinitionClauseLabelZh`
- `LeadClauseCode`
- `LeadClauseLabelZh`
- `LeadClauseShare`
- `ClauseMix`
- `ReviewPromptZh`

### Summary

至少包含：

- `AssemblyCount`
- `RepresentativePartCount`
- `AssembliesWithTopologyRewrite`
- `CoreRepresentativePartCount`
- `ReviewRepresentativePartCount`
- `InputMainPartRepresentativeCount`
- `LeadClauseBreakdown`
- `DefinitionClauseBreakdown`
- `PatternBreakdown`

---

## Markdown 摘要要求

`definition-clause-decision-fullrun-source.zh-CN.md` 至少包含：

1. 总览
2. 条款分布
3. 模式分布
4. 代表零件角色分布
5. 代表样本表

其中代表样本表至少展示：

- `MemberId`
- `AssemblyId`
- `RepresentativePartId`
- `RepresentativePartName`
- `Role(core/review/main)`
- `Pattern`
- `DefinitionClause`
- `LeadClause`
- `ReviewPromptZh`

---

## 使用顺序

真实链接线后，推荐 review 顺序：

1. `definition-clause-decision-fullrun-source.json/.md`
2. `definition-clause-decision-snapshot-validation.json/.md`
3. `definition-clause-decision-snapshot-roundtrip.json/.md`
4. `definition-clause-decision-summary.json/.md`

也就是说，先确认“真实 full-run 输入对不对”，再确认 snapshot 和 sidecar 是否闭合。

---

## 非目标

本工件不负责：

- 最终家族结论
- `DefinitionBodyFamily`
- 阶段 6 家族判定器
- 阶段 7 built-up 细分

它只负责一件事：

**把当前 full-run 结果进入 `DefinitionClauseDecision` 旁路前的真实输入稳定落盘。**
