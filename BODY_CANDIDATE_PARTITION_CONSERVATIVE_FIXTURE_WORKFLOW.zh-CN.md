# Body Candidate Partition Conservative Fixture Workflow

## 目标

把阶段 2 已有的：

- `signal snapshot`
- `conservative mapper`
- `fixture runner`
- `Markdown report`

继续收口成一条默认 artifact workflow，方便后续：

- 先独立落盘阶段 2 的保守回归结果
- 将真实 partitioner 结果并入前先有稳定对照基线
- 后续再接 sidecar / validation / app entry

## 当前代码入口

- `BodyCandidatePartitionConservativeFixtureArtifactModels.cs`
- `BodyCandidatePartitionConservativeFixtureArtifactBuilder.cs`
- `BodyCandidatePartitionConservativeFixtureSerializer.cs`
- `BodyCandidatePartitionConservativeFixtureWorkflow.cs`
- `BodyCandidatePartitionConservativeFixtureWorkflowResult.cs`

## 默认输出

调用：

```csharp
var result =
    BodyCandidatePartitionConservativeFixtureWorkflow.RunDefault(outputRootDirectory);
```

当前会在：

- `body-candidate-partition-conservative-fixture/`

目录下落盘：

- `body-candidate-partition-conservative-fixture.json`
- `body-candidate-partition-conservative-fixture.md`

## WorkflowResult

返回：

- `OutputDirectory`
- `JsonPath`
- `MarkdownPath`
- `TotalCount`
- `PassedCount`
- `FailedCount`

## 当前价值

- 让阶段 2 的保守分层闭环从 runner/report 再推进成可落盘 workflow
- 为后续把真实 partitioner 结果先映射到这条链上提供稳定输出面
- 在还不能安全改旧入口代码时，继续沿 tasklist 的“body-candidate partition design”主线推进
