# Body Candidate Partition Conservative Fixture Package Audit Workflow

## 目标

把阶段 2 已有的默认 package workflow 再推进成“一次执行后自校验”的 workflow，避免后续：

- 先跑默认导出
- 再手工检查目录包是否完整

分成两段。

## 当前代码入口

- `BodyCandidatePartitionConservativeFixturePackageAuditResult.cs`
- `BodyCandidatePartitionConservativeFixturePackageAuditWorkflow.cs`

## 调用方式

```csharp
var result =
    BodyCandidatePartitionConservativeFixturePackageAuditWorkflow
        .RunDefault(outputRootDirectory);
```

## 当前行为

1. 先调用：
   - `BodyCandidatePartitionConservativeFixturePackageWorkflow.RunDefault(outputRootDirectory)`
2. 再调用：
   - `BodyCandidatePartitionConservativeFixtureOutputAuditBuilder.BuildFromOutputDirectory(...)`
3. 最后返回统一结果：
   - 默认夹具通过/失败统计
   - 目录包是否完整
   - 缺失文件列表
   - 统一 `ExitCode`

## ExitCode 语义

- `FailedCount == 0` 且 `PackageIsComplete == true`
  - `ExitCode = 0`
- 否则
  - `ExitCode = 1`

## 当前价值

- 让阶段 2 默认导出链具备最小“导出并自校验”能力
- 后续 command/app entry/dispatcher 若要走更稳妥路径，可直接选这条 workflow
- 继续沿 `body-candidate partition design` 与 `output-contract cleanup` 主线推进
