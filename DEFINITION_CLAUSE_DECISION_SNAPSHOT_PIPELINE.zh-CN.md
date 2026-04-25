# DefinitionClauseDecision Snapshot Pipeline

## 目的

把 `DefinitionClauseDecision` 的 snapshot 层再收敛成统一 pipeline，避免后续真实结果链接入时：

- 直接在不同入口手写 snapshot 构造
- extractor / export / sidecar workflow 混在一起

## 当前组件

- extractor 接口：
  [IDefinitionClauseDecisionSnapshotExtractor.cs](./src/TeklaBodyBracketRecognition.App/IDefinitionClauseDecisionSnapshotExtractor.cs)
- snapshot pipeline：
  [DefinitionClauseDecisionSnapshotPipeline.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotPipeline.cs)
- demo extractor：
  [DefinitionClauseDecisionDemoSnapshotExtractor.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoSnapshotExtractor.cs)

## 当前分层

### extractor

职责：

- 把某种输入抽成 `DefinitionClauseDecisionSnapshot`

### snapshot pipeline

职责：

1. 调 extractor 拿到 snapshot
2. 调 `DefinitionClauseDecisionSnapshotExportService`
3. 统一走后续 sidecar 导出

## 当前 demo 路径

现在 demo runner 已经改成：

- `DemoSnapshotExtractor -> SnapshotPipeline -> SnapshotExportService -> Sidecar`

这意味着 demo 路已经和未来真实结果路对齐到了同一层。

## 后续真实结果接线建议

后面优先新增：

- `RealCoreBodyProofSnapshotExtractor`

然后直接走：

- `RealCoreBodyProofSnapshotExtractor -> SnapshotPipeline -> SnapshotExportService -> Sidecar`

而不是：

- 直接在真实结果入口手写 sidecar 导出逻辑
- 或绕开 snapshot 层另写一条导出链
