# DefinitionClauseDecision 输出契约

## 目的

为阶段 5 新增的 `ClauseVerdict / ClausePromotionReadiness` 固定输出字段和摘要字段，避免后续接线时出现：

- 引擎字段已加，但摘要列名反复变动
- 工作区结果、独立重建脚本、交付包结果三套字段不一致
- `review` 产物无法直接判断“这条定义条款现在到了哪一步”

## 单零件字段

单个 `CoreBodyProofPartResult` 上新增并固定以下字段：

### 条款判定

- `TopologyRewriteClauseVerdictCode`
- `TopologyRewriteClauseVerdictLabelZh`

允许值：

- `NONE`
- `CLAUSE_SATISFIED_STABLE`
- `CLAUSE_SATISFIED_WITH_REWRITE`
- `CLAUSE_BROKEN_DIRECT`
- `CLAUSE_BROKEN_MAJORITY`
- `CLAUSE_REVIEW_REQUIRED`

### 条款提升准备度

- `TopologyRewriteClausePromotionReadinessCode`
- `TopologyRewriteClausePromotionReadinessLabelZh`

允许值：

- `NONE`
- `READY_FOR_DEFINITION_CHECK`
- `READY_WITH_REWRITE_NOTE`
- `NOT_READY_NEEDS_REVIEW`

## 代表零件摘要列

`core-body-proof-summary.zh-CN.md` 和 `topology-rewrite-summary.zh-CN.md` 的代表零件表中，固定补充以下列：

- `DefinitionClause`
- `DefinitionClauseEffect`
- `ClauseVerdict`
- `ClausePromotionReadiness`

顺序要求：

1. `DefinitionClause`
2. `DefinitionClauseEffect`
3. `ClauseVerdict`
4. `ClausePromotionReadiness`

这样 review 时先看“条款是什么”，再看“效果是什么”，最后看“当前判定到哪一步”。

## 构件级聚合字段

每个构件级 summary 固定生成以下聚合字段：

### 条款判定聚合

- `ClauseVerdicts`
- `LeadClauseVerdict`
- `LeadClauseVerdictShare`
- `ClauseVerdictMixStatus`

### 提升准备度聚合

- `ClausePromotionReadinesses`
- `LeadClausePromotionReadiness`
- `LeadClausePromotionReadinessShare`
- `ClausePromotionReadinessMixStatus`

## 中文文案约束

### 条款判定中文

- `CLAUSE_SATISFIED_STABLE` -> `定义条款稳定成立`
- `CLAUSE_SATISFIED_WITH_REWRITE` -> `定义条款仍成立，但路径被改写`
- `CLAUSE_BROKEN_DIRECT` -> `定义条款被直接破坏`
- `CLAUSE_BROKEN_MAJORITY` -> `定义条款在多数站位被破坏`
- `CLAUSE_REVIEW_REQUIRED` -> `定义条款仍需复核`

### 提升准备度中文

- `READY_FOR_DEFINITION_CHECK` -> `已具备进入定义验证的证据`
- `READY_WITH_REWRITE_NOTE` -> `可进入定义验证，但需附带改写说明`
- `NOT_READY_NEEDS_REVIEW` -> `仍需复核后才能进入定义验证`

## 聚合语义

### LeadClauseVerdict

表示当前构件在代表零件集合中，占比最高的条款判定。

### LeadClausePromotionReadiness

表示当前构件在代表零件集合中，占比最高的提升准备度。

### MixStatus

用于告诉 review 人员“当前结论是否已经单一收敛”。

推荐文案：

- 单一结论
- 主结论占优，但仍属混合结论
- 混合结论

## review 视角解释

### `ClauseVerdict`

回答：

- “这条定义条款现在是成立、被破坏，还是仍需复核？”

### `ClausePromotionReadiness`

回答：

- “这条判定是否已经足够往下一层家族定义验证推进？”

## 交付包要求

交付包中至少以下文件要同步包含这层字段：

- `core-body-proof-summary.zh-CN.md`
- `topology-rewrite-summary.zh-CN.md`
- 独立重建后的 topology rewrite 摘要

## 首轮落地要求

首轮接线完成后，至少要确保：

- `GKZ` 能出现 `定义条款仍成立，但路径被改写`
- `HXZ` 能出现 `定义条款仍成立，但路径被改写`
- `MJ` 能出现 `定义条款被直接破坏`
- `GL / YPGL` 仍能保守维持 `定义条款仍需复核`
