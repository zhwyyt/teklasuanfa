# DefinitionClauseDecision Snapshot 契约

## 目的

在还不能安全绑定真实结果对象具体结构时，先冻结一层中间快照契约。  
这样后续真实结果链只要能先抽成 snapshot，就已经能走统一 source pipeline 和 sidecar 输出。

## 当前模型

- [DefinitionClauseDecisionSnapshotModels.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotModels.cs)

包含三层：

- `DefinitionClauseDecisionSnapshot`
- `DefinitionClauseDecisionSnapshotMember`
- `DefinitionClauseDecisionSnapshotPart`

## 最小字段

### Member 层

- `MemberId`
- `AssemblyId`

### Part 层

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

## 当前 provider

- [DefinitionClauseDecisionSnapshotSourceProvider.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotSourceProvider.cs)

职责：

- 把 snapshot 直接映射成 `DefinitionClauseDecisionSourceRow`

## 当前导出服务

- [DefinitionClauseDecisionSnapshotExportService.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotExportService.cs)

职责：

1. 用 snapshot provider 生成 `SourceRow`
2. 走 `DefinitionClauseDecisionSourcePipeline`
3. 走 `DefinitionClauseDecisionSidecarWorkflow`
4. 直接导出 summary sidecar

## 当前 demo 桥接

- [DefinitionClauseDecisionDemoSnapshotBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoSnapshotBuilder.cs)
- [DefinitionClauseDecisionDemoWorkflowRunner.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoWorkflowRunner.cs)

当前 demo 路径已经改成：

- `fixtures/source rows -> demo snapshot -> snapshot export service -> sidecar`

这意味着 snapshot 层已经不只是“给未来真实结果预留”，而是已经被 demo 最小闭环实际使用。

## 当前价值

后续真实结果链接入时，优先策略是：

1. 先从真实结果对象抽出 snapshot
2. 再调用 snapshot export service

而不是：

- 直接在复杂结果对象上手写 sidecar 逻辑
- 或绕开 source pipeline 另写一套导出路径

## 当前落盘

snapshot 现在已经是 demo 导出目录里的一级产物：

- `definition-clause-decision-snapshot.json`
- `definition-clause-decision-snapshot-validation.json`
- `definition-clause-decision-snapshot-validation.md`

并且新增了 round-trip 自检：

- `definition-clause-decision-snapshot-roundtrip.json`
- `definition-clause-decision-snapshot-roundtrip.md`

这样后续真实 extractor 一旦接进来，我们可以先直接审查 snapshot，再看 summary / fixture 两条 sidecar 是否符合预期。

## 下一步

当能够安全读到真实 `CoreBodyProof` 结果结构时，优先新增：

- `RealCoreBodyProofToDefinitionClauseDecisionSnapshotExtractor`

而不是直接修改 sidecar builder / mapper / workflow。
