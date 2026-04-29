# Definition Clause Decision Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Required Artifact Summary Formatter

## 目标

在 `RequiredArtifactInspector` 已经把“缺哪些工件”统一收口之后，再把“如何把检查结果输出给人看”也冻结成单一 formatter，避免：

- smoke validator 自己拼一套 message
- dispatcher 最小实跑回查再拼第二套
- README / validation.md / 操作文档再口头描述第三套

## 代码入口

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSummaryFormatter`

## 当前提供的统一输出

### 1. Console message

```csharp
var message =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSummaryFormatter
        .BuildConsoleMessage(snapshot);
```

语义：

- 全部存在：  
  `Post-merge smoke required artifacts are complete.`
- 有缺失：  
  `Post-merge smoke is missing required artifacts: ...`

### 2. Markdown block

```csharp
var markdown =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSummaryFormatter
        .BuildMarkdownBlock(snapshot);
```

语义：

- `Status: complete` 或 `Status: incomplete`
- 缺失时稳定列出 `Missing artifacts`

## 当前价值

- 继续沿“输出契约清理”主线推进，把共享工件检查结果的呈现也纳入单一真源
- 为后续 smoke validator、dispatcher 回查与最小实跑说明提供统一文字层
- 降低后续不同入口各自拼 message 带来的文案漂移风险
