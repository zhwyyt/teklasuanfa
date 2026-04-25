# Body Candidate Partition Conservative Fixture Output Audit

## 目标

在阶段 2 已冻结：

- output contract
- output path resolver

的基础上，再补一层共享 output audit，回答：

- 默认目录包四件套是否齐全
- 缺了哪些文件

避免后续：

- command/export service 自己检查一套
- dispatcher hook 再检查第二套
- sidecar / validation 再写第三套

## 当前代码入口

- `BodyCandidatePartitionConservativeFixtureOutputAudit.cs`
- `BodyCandidatePartitionConservativeFixtureOutputAuditBuilder.cs`

## 当前检查文件

- `body-candidate-partition-conservative-fixture.json`
- `body-candidate-partition-conservative-fixture.md`
- `manifest.json`
- `README.md`

## 调用方式

### 从 output root directory

```csharp
var audit =
    BodyCandidatePartitionConservativeFixtureOutputAuditBuilder
        .BuildFromOutputRootDirectory(outputRootDirectory);
```

### 从 output directory

```csharp
var audit =
    BodyCandidatePartitionConservativeFixtureOutputAuditBuilder
        .BuildFromOutputDirectory(outputDirectory);
```

## 当前价值

- 继续沿“输出契约清理”主线推进
- 为阶段 2 的 command/package/dispatcher 消费方提供共享目录完整性判断
- 后续如果真实 partitioner 结果接进阶段 2 导出链，也能继续复用这套 output audit
