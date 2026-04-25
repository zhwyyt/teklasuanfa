# Body Candidate Partition Conservative Fixture Output Contract

## 目标

在阶段 2 已有 package workflow 的基础上，把默认输出目录名和关键文件名冻结成单一真源，避免后续：

- command/export service 自己拼一套
- package workflow 再写一套
- dispatcher hook / 未来 sidecar 再口头约定第三套

## 当前代码入口

- `BodyCandidatePartitionConservativeFixtureOutputContract.cs`
- `BodyCandidatePartitionConservativeFixtureOutputPaths.cs`
- `BodyCandidatePartitionConservativeFixtureOutputPathResolver.cs`

## 当前冻结的名称

- 输出目录  
  `body-candidate-partition-conservative-fixture`
- JSON  
  `body-candidate-partition-conservative-fixture.json`
- Markdown  
  `body-candidate-partition-conservative-fixture.md`
- Manifest  
  `manifest.json`
- README  
  `README.md`

## 解析方式

### 从 output root directory

```csharp
var paths =
    BodyCandidatePartitionConservativeFixtureOutputPathResolver
        .ResolveFromOutputRootDirectory(outputRootDirectory);
```

### 从 output directory

```csharp
var paths =
    BodyCandidatePartitionConservativeFixtureOutputPathResolver
        .ResolveFromOutputDirectory(outputDirectory);
```

## 当前价值

- 继续沿“输出契约清理”主线推进
- 为阶段 2 的 command/package/dispatcher 消费方提供单一命名真源
- 后续即使切换真实 partitioner 结果，也不必重新发明输出目录约定
