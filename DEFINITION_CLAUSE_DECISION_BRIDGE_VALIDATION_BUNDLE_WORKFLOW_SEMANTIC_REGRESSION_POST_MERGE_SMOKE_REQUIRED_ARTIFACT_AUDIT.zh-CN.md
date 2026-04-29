# Definition Clause Decision Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Required Artifact Audit

## 目标

把已经拆开的：

- `OutputPathResolver`
- `RequiredArtifactInspector`
- `RequiredArtifactSummaryFormatter`

再压成一次调用即可拿到的统一 audit 结果，避免后续 smoke validator、dispatcher 回查或最小实跑入口还要自己把三层 helper 串起来。

## 代码入口

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit`
- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditBuilder`

## 调用方式

### 1. 从 output root directory

```csharp
var audit =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditBuilder
        .BuildFromOutputRootDirectory(outputRootDirectory);
```

### 2. 从 output directory

```csharp
var audit =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditBuilder
        .BuildFromOutputDirectory(outputDirectory);
```

## 当前返回内容

- `OutputPaths`
- `Snapshot`
- `ConsoleMessage`
- `MarkdownBlock`
- `IsComplete`

## 当前价值

- 继续推进“输出契约清理”主线，把 smoke 输出完成态再收敛成单次调用结果
- 为后续接入 smoke validator 或 dispatcher 回查进一步缩小 patch 面
- 让未来旧入口代码只需关心：
  - `audit.IsComplete`
  - `audit.ConsoleMessage`
  - `audit.MarkdownBlock`
