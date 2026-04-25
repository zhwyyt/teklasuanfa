# Definition Clause Decision Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Required Artifact Audit Decision

## 目标

在 `RequiredArtifactAuditBuilder` 已经把路径解析、工件检查和统一输出压成单次调用之后，再把：

- `audit.IsComplete`
- `console message`
- `exit code`

进一步冻结成共享 decision，避免后续 smoke command handler、dispatcher 或最小实跑入口各自再写一遍：

- 成功时 `exitCode=0`
- 缺失工件时 `exitCode=1`

## 代码入口

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecision`
- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecisionBuilder`

## 当前规则

- `audit.IsComplete == true`
  - `ExitCode = 0`
  - `ConsoleMessage = audit.ConsoleMessage`
- `audit.IsComplete == false`
  - `ExitCode = 1`
  - `ConsoleMessage = audit.ConsoleMessage`

## 使用方式

### 1. 已有 audit

```csharp
var decision =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecisionBuilder
        .Build(audit);
```

### 2. 直接从 output root directory

```csharp
var decision =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecisionBuilder
        .BuildFromOutputRootDirectory(outputRootDirectory);
```

### 3. 直接从 output directory

```csharp
var decision =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecisionBuilder
        .BuildFromOutputDirectory(outputDirectory);
```

## 当前价值

- 继续推进“输出契约清理”主线，把 smoke 输出完成态最终收敛为共享 decision
- 为未来接入 smoke handler / dispatcher 进一步缩小 patch 面
- 保证 `exit code` 语义与共享工件完成态判定保持一致
