# DefinitionClauseDecision 接线计划

## 目标

把已经落好的三个部件接进现有阶段 5 链路：

- `DefinitionClauseDecisionBridge`
- `DefinitionClauseDecisionAggregate`
- `DEFINITION_CLAUSE_DECISION_EXPECTED_CASES`

并让工作流结果、摘要、交付包都能看到：

- `ClauseVerdict`
- `ClausePromotionReadiness`

## 接线顺序

### 1. 引擎层

在 `CoreBodyProofEngine` 里新增一段接线：

- `AttachTopologyRewriteClauseDecisions(...)`

每个候选零件需要构造 `DefinitionClauseDecisionBridgeInput`，把当前已有字段映射进去，然后回写：

- `TopologyRewriteClauseVerdictCode`
- `TopologyRewriteClauseVerdictLabelZh`
- `TopologyRewriteClausePromotionReadinessCode`
- `TopologyRewriteClausePromotionReadinessLabelZh`

## 2. 结果模型

在 `CoreBodyProofPartResult` 上补四个字段：

- `TopologyRewriteClauseVerdictCode`
- `TopologyRewriteClauseVerdictLabelZh`
- `TopologyRewriteClausePromotionReadinessCode`
- `TopologyRewriteClausePromotionReadinessLabelZh`

默认值：

- `NONE`
- `无条款判定`
- `NONE`
- `无提升准备度`

## 3. 摘要层

### 核心证明摘要

在 `core-body-proof-summary.zh-CN.md` 里新增：

- `ClauseVerdict`
- `ClausePromotionReadiness`

### TopologyRewrite 摘要

利用 `DefinitionClauseDecisionAggregate` 生成：

- `ClauseVerdicts`
- `LeadClauseVerdict`
- `LeadClauseVerdictShare`
- `ClauseVerdictMixStatus`
- `ClausePromotionReadinesses`
- `LeadClausePromotionReadiness`
- `LeadClausePromotionReadinessShare`
- `ClausePromotionReadinessMixStatus`

## 4. 独立脚本

更新：

- `Build-TopologyRewriteSummary.ps1`

要求：

- 可重建 `ClauseVerdict / ClausePromotionReadiness` 聚合
- WinPS 兼容
- 保持 UTF-8 with BOM

## 5. 首轮回归

重点检查以下样本组：

- `GKZ`
- `HXZ`
- `MJ`
- `GL`
- `YPGL`

要求和预期落点见：

- [DEFINITION_CLAUSE_DECISION_EXPECTED_CASES.zh-CN.md](./DEFINITION_CLAUSE_DECISION_EXPECTED_CASES.zh-CN.md)

## 6. 交付包

接线通过后，更新：

- 工作区结果目录
- 状态页
- 任务清单
- 交付包

## 完成判据

满足以下条件才算这一轮接线完成：

1. `CoreBodyProofEngine` 已回写四个 decision 字段
2. 工作区 full-run 已生成新的 `core-body-proof-summary.zh-CN.md`
3. `topology-rewrite-summary.zh-CN.md` 已出现 `LeadClauseVerdict / LeadClausePromotionReadiness`
4. `GKZ / HXZ / MJ / GL / YPGL` 五类样本的落点符合预期清单
