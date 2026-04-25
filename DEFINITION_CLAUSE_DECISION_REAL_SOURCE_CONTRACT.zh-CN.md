# DefinitionClauseDecision 真实结果接线契约

## 目的

本文档定义：

- 当前 full-run 结果链里，哪些结果可以作为 `DefinitionClauseDecision` 的真实输入；
- 这些真实输入进入 `snapshot -> source pipeline -> sidecar` 之前，最小需要保留哪些字段；
- 哪些字段来自稳定证据，哪些字段只作为 review 辅助，不应被误当成最终家族标签。

目标不是重新定义 `ClauseVerdict`，而是把“真实结果如何接进 sidecar”这层固定下来，避免后续继续在 `Program.cs` 里临时拼字段。

---

## 输入来源

真实结果侧，优先从以下结果对象抽取：

1. `CoreBodyProofViewOutput`
2. `BodyMaterialSummary`
3. `BodyMaterialExplanation`

原则：

- `CoreBodyProofViewOutput` 提供主体核心证明与 `TopologyRewrite* / DefinitionClause*` 这类证据字段；
- `BodyMaterialSummary` 提供构件、装配、输入主件、真实种子零件等主体上下文；
- `BodyMaterialExplanation` 提供中文 review 解释和当前主体描述层输出；
- 任何“最终家族结论”都不能直接从旧 `BodyRecognizer` 投票字段抄过来冒充定义驱动结论。

---

## 最小真实输入行

真实结果接线时，每个“候选代表零件”至少应具备以下字段：

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

说明：

- `ViewKind` 目前至少区分 `real_input / recognition_input`；
- `RepresentativePartId` 允许来自 `CoreBodyPartIds` 或 `ReviewPartIds`，但必须显式标出当前角色；
- `LeadClause*` 与 `ClauseMix` 属于“条款稳定度提示”，不能直接当成 verdict。

---

## 真实 snapshot 的层次

真实 snapshot 采用两层：

1. `AssemblySnapshot`
2. `RepresentativePartSnapshot`

### AssemblySnapshot

负责表达构件级上下文：

- 构件编号
- 装配编号
- 输入主件编号
- 当前主体描述家族
- 当前主体描述截面类型
- 是否存在 `TopologyRewrite`
- 当前主条款稳定度

### RepresentativePartSnapshot

负责表达“真正要送入 `DefinitionClauseDecision` bridge”的代表零件：

- 是哪块零件
- 当前在 `core / review` 中的角色
- 命中了哪一类 `TopologyRewrite` 证据
- 当前条款提示是什么
- 为什么需要 review

原则：

- `AssemblySnapshot` 用于聚合；
- `RepresentativePartSnapshot` 用于进入 `SourcePipeline`；
- 后续 sidecar 只消费 `RepresentativePartSnapshot` 及其向上聚合结果，不直接消费 full-run 原始复杂对象。

---

## 角色边界

### 可以直接进入真实 snapshot 的字段

- 构件标识
- 装配标识
- 代表零件标识
- 当前 `TopologyRewrite*`
- 当前 `DefinitionClause*`
- 当前 `LeadClause* / ClauseMix`
- 当前 `core / review` 角色

### 只能作为 review 辅助的字段

- `BodyDescriptorFamily`
- `BodyDescriptorSectionType`
- `ImportSynthesisKind`
- 旧 `BodyRecognizer` 投票分数

这些字段可以进入 sidecar 附注，但不能参与 `ClauseVerdict` 的定义驱动结论。

---

## 输出目标

真实结果接线完成后，应至少能独立落出：

1. `definition-clause-decision-snapshot.json`
2. `definition-clause-decision-snapshot-validation.json/.md`
3. `definition-clause-decision-snapshot-roundtrip.json/.md`
4. `definition-clause-decision-summary.json/.md`

并满足以下顺序：

1. 先看 `snapshot validation`
2. 再看 `snapshot roundtrip`
3. 最后看 `summary`

---

## 非目标

本契约当前不解决：

- 最终 `DefinitionBodyFamily`
- 阶段 6 家族判定器
- 阶段 7 built-up 细分
- 对旧 `BodyRecognizer` 的替换

它只解决一件事：

**如何把当前 full-run 的真实证据，稳定地接进 `DefinitionClauseDecision` sidecar。**
