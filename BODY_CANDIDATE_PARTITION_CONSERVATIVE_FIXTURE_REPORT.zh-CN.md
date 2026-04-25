# Body Candidate Partition Conservative Fixture Report

## 目标

把阶段 2 默认夹具与最小 runner 的结果再推进成稳定的 Markdown 报告层，便于：

- 人工 review 保守分层落点
- 后续并到 sidecar / validation workflow
- 在还没接旧 partitioner 之前，先看清楚每个 fixture 为什么通过或失败

## 当前代码入口

- `BodyCandidatePartitionConservativeFixtureReportBuilder`

## 输入

- `BodyCandidatePartitionConservativeFixtureResult[]`

通常来自：

```csharp
var results = BodyCandidatePartitionConservativeFixtureRunner.RunDefault();
var markdown = BodyCandidatePartitionConservativeFixtureReportBuilder.BuildMarkdown(results);
```

## 当前输出结构

### 1. 总览

- Total
- Passed
- Failed

### 2. 摘要表

按 fixture 输出：

- `Fixture`
- `Expected Lane`
- `Actual Lane`
- `Needs Review`
- `Result`

### 3. 逐项详情

每个 fixture 输出：

- expected lane
- actual lane
- needs review
- actual evidence codes
- actual review reasons
- missing expected evidence codes
- missing expected review reason fragments
- final status

## 当前价值

- 让阶段 2 的保守夹具从“只有 runner 结果对象”推进到真正可读的 review 报告
- 为后续把真实 partitioner 结果先适配进默认夹具链提供稳定人工检查面
- 为未来 sidecar / validation 落盘保留统一 Markdown 源
