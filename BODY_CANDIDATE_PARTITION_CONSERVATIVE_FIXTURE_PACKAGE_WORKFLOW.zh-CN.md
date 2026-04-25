# Body Candidate Partition Conservative Fixture Package Workflow

## 目标

在阶段 2 已有默认 artifact/workflow 的基础上，再补一层“可交付目录包”包装，让后续无论是：

- 默认夹具自检
- 真实 partitioner 中间结果并入
- 未来 sidecar / validation / app entry

都能直接拿到自描述目录，而不是只有 `json + md` 两个裸文件。

## 当前代码入口

- `BodyCandidatePartitionConservativeFixtureManifest.cs`
- `BodyCandidatePartitionConservativeFixtureManifestSerializer.cs`
- `BodyCandidatePartitionConservativeFixtureOutputReadmeBuilder.cs`
- `BodyCandidatePartitionConservativeFixturePackageWorkflow.cs`
- `BodyCandidatePartitionConservativeFixturePackageWorkflowResult.cs`

## 调用方式

```csharp
var result =
    BodyCandidatePartitionConservativeFixturePackageWorkflow.RunDefault(outputRootDirectory);
```

## 当前输出

在现有：

- `body-candidate-partition-conservative-fixture.json`
- `body-candidate-partition-conservative-fixture.md`

基础上，再补：

- `manifest.json`
- `README.md`

## WorkflowResult

返回：

- `OutputDirectory`
- `JsonPath`
- `MarkdownPath`
- `ManifestPath`
- `ReadmePath`
- `TotalCount`
- `PassedCount`
- `FailedCount`

## 当前价值

- 让阶段 2 默认 workflow 从“可落盘”继续推进到“可交付目录包”
- 为后续把真实 partitioner 或中间结果先接进 stage 2 闭环提供稳定输出面
- 继续沿 `body-candidate partition design` 主线推进，而不提前碰旧 heuristics 本体
