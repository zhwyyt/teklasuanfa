# DefinitionClauseDecision Source Pipeline

## 目的

把 `DefinitionClauseDecision` 从“demo 一套、真实结果另一套”的风险，提前收敛成统一 source pipeline：

- provider 负责把某种输入转成 `SourceRow`
- mapper / sidecar 负责后续统一处理

这样后面接真实结果链时，不需要复制 demo 的整套 builder/runner，只需要补一个真实 provider。

## 当前组件

- provider 接口：
  [IDefinitionClauseDecisionSourceProvider.cs](./src/TeklaBodyBracketRecognition.App/IDefinitionClauseDecisionSourceProvider.cs)
- source pipeline：
  [DefinitionClauseDecisionSourcePipeline.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSourcePipeline.cs)
- demo provider：
  [DefinitionClauseDecisionDemoSourceProvider.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoSourceProvider.cs)

## 当前分层

### provider

职责：

- 把具体输入转成 `DefinitionClauseDecisionSourceRow`

### source pipeline

职责：

- 调 provider 拿到 `SourceRow`
- 调 mapper 生成 `Artifacts`

### sidecar workflow

职责：

- 把 `Artifacts` 和 fixture 报告落盘成 sidecar 文件

## 当前价值

现在 demo 路径已经改成：

- `DemoSourceProvider -> SourcePipeline -> Mapper -> SidecarWorkflow`

后续真实结果链只需要补一条新的 provider，例如：

- `RealCoreBodyProofSourceProvider`

而不是再复制一套 `demo builder -> mapper -> artifact -> workflow`。

## 下一步

当能安全读取真实结果对象结构时，优先新增：

- 真实结果 provider

而不是改动 demo 这条已经稳定的 pipeline。
