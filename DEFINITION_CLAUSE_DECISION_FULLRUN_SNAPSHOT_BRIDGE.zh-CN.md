# DefinitionClauseDecision Full-Run Snapshot Bridge

## 目标

把已经落盘的 `fullrun-source` 工件，与既有的 `snapshot -> sidecar` 统一链之间，增加一层稳定适配面。

这层 bridge 的职责不是重新判定 `ClauseVerdict`，而是回答：

- 哪些 `fullrun-source representative parts` 需要进入 snapshot；
- 进入 snapshot 前需要保留哪些构件级上下文；
- 哪些信息可以作为 snapshot 的稳定字段，哪些仍然只应停留在 review 提示层。

---

## 输入

bridge 的输入只认两类：

1. `DefinitionClauseDecisionFullRunAssemblySource`
2. `DefinitionClauseDecisionFullRunRepresentativePartSource`

也就是说：

- 不直接消费 `Program.cs` 里的完整 full-run 复杂对象；
- 不直接消费旧 `BodyRecognizer` 的投票结果；
- 优先消费已经通过 `fullrun-source` collector 收敛过的结果。

---

## 输出

bridge 的输出采用两层：

1. `AssemblySnapshotSeed`
2. `RepresentativePartSnapshotSeed`

其中：

### AssemblySnapshotSeed

至少保留：

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

### RepresentativePartSnapshotSeed

至少保留：

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

---

## 边界

### 可以进入 snapshot seed 的字段

- 构件标识
- 装配标识
- 代表零件标识
- `Pattern / ControllerRole / ShapeRole / FamilyRisk / ProofType / FamilyProofTarget / DefinitionClause`
- `LeadClause / LeadClauseShare / ClauseMix`
- `core / review / main` 角色

### 不应在 bridge 中提升为最终结论的字段

- `DefinitionBodyFamily`
- `ClauseVerdict`
- `ClausePromotionReadiness`

这些仍然属于后续 bridge / aggregate / sidecar 层，不应在 fullrun->snapshot 适配时提前硬编码。

---

## 推荐顺序

1. `Program.cs` 先产出 `fullrun-source` 工件
2. 用本 bridge 把 `fullrun-source` 适配成 snapshot seeds
3. 再把 snapshot seeds 送进既有：
   - `snapshot validation`
   - `snapshot roundtrip`
   - `summary sidecar`

---

## 当前最近一步

下一步应优先新增一组最小模型，用来表达：

- `DefinitionClauseDecisionFullRunAssemblySnapshotSeed`
- `DefinitionClauseDecisionFullRunRepresentativePartSnapshotSeed`

然后再补一个 bridge builder，把 `fullrun-source` 收敛进 snapshot seeds。
