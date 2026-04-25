# DefinitionClauseDecision Demo Workflow

## 目的

在真实引擎字段尚未完全接进 `DefinitionClauseDecision` sidecar 前，先用 bridge fixture 构造一条最小闭环，验证这条路径已经可以跑通：

- `Bridge -> SourceRow -> Mapper -> Artifact -> Workflow`

## 当前入口

- [DefinitionClauseDecisionDemoSourceBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoSourceBuilder.cs)
- [DefinitionClauseDecisionDemoWorkflowRunner.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoWorkflowRunner.cs)
- [DefinitionClauseDecisionSidecarManifest.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSidecarManifest.cs)

这个 builder 会：

1. 读取默认 bridge fixtures
2. 直接调用 `DefinitionClauseDecisionBridge.Evaluate(...)`
3. 把结果映射成 `DefinitionClauseDecisionSourceRow`

当前还新增了一层：

- [DefinitionClauseDecisionDemoSnapshotBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoSnapshotBuilder.cs)

也就是说，demo 最小闭环现在已经可以走：

- `fixtures/source rows -> demo snapshot -> snapshot export service -> sidecar`

## 目的不是替代真实接线

这条 demo 路径只用于验证：

- `SourceRow` 契约是否足够
- `Mapper` 是否能正确产出 `RepresentativeRows / AggregateRows`
- `SidecarWorkflow` 是否能正确落盘 summary/fixture 两条 sidecar

它不负责：

- 替代真实 `Program.cs`
- 替代引擎输出
- 替代真实样本 full-run

## 当前最小闭环

理论上的最小闭环顺序：

1. `DefinitionClauseDecisionDemoSourceBuilder.BuildFromDefaultFixtures()`
2. `DefinitionClauseDecisionMapper.BuildArtifacts(...)`
3. `DefinitionClauseDecisionSidecarWorkflow.WriteSummaryArtifacts(...)`
4. `DefinitionClauseDecisionSidecarWorkflow.WriteDefaultFixtureArtifacts(...)`

而当前统一入口 `DefinitionClauseDecisionDemoWorkflowRunner.Run(...)` 会直接返回：

- `Artifacts`
- `Manifest`

其中 `Manifest` 会固定给出：

- `definition-clause-decision-summary.json`
- `definition-clause-decision-summary.zh-CN.md`
- `definition-clause-decision-fixture-report.json`
- `definition-clause-decision-fixture-report.md`

## 通过标准

如果这条 demo 路径能稳定落出：

- `definition-clause-decision-summary.json/.md`
- `definition-clause-decision-fixture-report.json/.md`

且内容能体现：

- `GKZ / HXZ -> CLAUSE_SATISFIED_WITH_REWRITE`
- `MJ -> CLAUSE_BROKEN_DIRECT`
- `GL / YPGL -> CLAUSE_REVIEW_REQUIRED`

就说明 `DefinitionClauseDecision` sidecar 的最小闭环已经具备，只差把真实引擎字段接进来。
