# DefinitionClauseDecisionBridge 设计草案

## 目标

把当前阶段 5 已经落下来的 `DefinitionClauseEffect` 证据层，再往前推进一层，形成更接近工程定义判定的中间桥：

- 不是只回答“移除此件后会发生什么效果”
- 而是开始回答“哪条定义条款正在被满足 / 被破坏 / 仍需复核”

这层桥接不直接给出最终 `BodyFamily` 结论，也不回头依赖旧的 `BodyRecognizer` 投票结果，而是把已有的 `remove-and-recompute`、`TopologyRewrite`、`ProofTarget`、`DefinitionClauseEffect` 重新组织成“定义条款判定可升级性”。

## 背景

当前阶段 5 已经能稳定输出：

- `TopologyRewritePatternCode`
- `TopologyRewriteShapeRoleCode`
- `TopologyRewriteFamilyRiskCode`
- `TopologyRewriteProofTypeCode`
- `TopologyRewriteFamilyProofTargetCode`
- `TopologyRewriteDefinitionClauseCode`
- `TopologyRewriteDefinitionClauseEffectCode`

但这些字段仍然偏“解释层”，还没有回答下面两个更关键的问题：

1. 某个零件被移除后，是“直接破坏了定义条款”，还是“只是改写了控制结构，仍可满足条款”？
2. 某个零件留在 `core/review` 的原因，能不能进一步归并成“已满足 / 已破坏 / 待复核”的定义条款结论？

## 新层目标

新增一层 `DefinitionClauseDecisionBridge`，输出两类结果：

1. `ClauseVerdict`
   - 某条定义条款对当前零件来说，处于什么判定状态
2. `ClausePromotionReadiness`
   - 这个判定状态是否已经足够支撑后续家族定义验证

## 术语

### ClauseVerdict

表示“当前零件与某定义条款的关系”。

建议首版枚举：

- `CLAUSE_SATISFIED_STABLE`
  - 条款在优先站位上稳定成立，移除此件不会破坏条款
- `CLAUSE_SATISFIED_WITH_REWRITE`
  - 条款仍可成立，但移除此件会改写控制结构或主轮廓证明路径
- `CLAUSE_BROKEN_DIRECT`
  - 移除此件会直接破坏条款
- `CLAUSE_BROKEN_MAJORITY`
  - 不一定单站位直接断裂，但会破坏多数站位持续性
- `CLAUSE_REVIEW_REQUIRED`
  - 当前证据不足以判断满足或破坏

### ClausePromotionReadiness

表示“这条条款判定是否足够提升到后续定义验证层”。

建议首版枚举：

- `READY_FOR_DEFINITION_CHECK`
- `READY_WITH_REWRITE_NOTE`
- `NOT_READY_NEEDS_REVIEW`

## 判定输入

桥接层只使用当前阶段 5 已有结构化字段，不引入新的几何重算：

- `LostClosedLoopStationIds`
- `LostBodyCoverageStationIds`
- `LostEnvelopeSupportStationIds`
- `TopologyRewriteSpanYStationIds`
- `TopologyRewriteSpanZStationIds`
- `TopologyRewriteEnvelopeControllerSwitchStationIds`
- `PriorityBodyCoverageAfterRemovalRatio`
- `EnvelopeBodyCoverageAfterRemovalRatio`
- `TopologyRewriteFamilyProofTargetCode`
- `TopologyRewriteDefinitionClauseCode`
- `TopologyRewriteDefinitionClauseEffectCode`

## 首版映射规则

### 1. 箱型闭合/对边稳定条款

适用：

- `BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE`

规则：

- 如果存在 `LostClosedLoopStationIds`
  - `ClauseVerdict = CLAUSE_BROKEN_DIRECT`
  - `ClausePromotionReadiness = READY_FOR_DEFINITION_CHECK`
- 否则如果 `DefinitionClauseEffect = BOX_OPPOSITE_WALL_STABILITY_REWRITE_EFFECT`
  - `ClauseVerdict = CLAUSE_SATISFIED_WITH_REWRITE`
  - `ClausePromotionReadiness = READY_WITH_REWRITE_NOTE`
- 否则
  - `ClauseVerdict = CLAUSE_REVIEW_REQUIRED`
  - `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`

工程含义：

- `GKZ` 当前大概率会落在这一支
- 结论不是“已经证明是箱型”，而是“箱型对壁闭合条款没有被直接打断，但控制路径被改写”

### 2. 主板持续性条款

适用：

- `PRIMARY_PLATE_CONTINUITY_CLAUSE`
- `BODY_PLATE_CONTINUITY_CLAUSE`

规则：

- 如果存在 `LostBodyCoverageStationIds`
  - `ClauseVerdict = CLAUSE_BROKEN_MAJORITY`
  - `ClausePromotionReadiness = READY_FOR_DEFINITION_CHECK`
- 否则如果 `PriorityBodyCoverageAfterRemovalRatio < 0.999`
  - `ClauseVerdict = CLAUSE_BROKEN_MAJORITY`
  - `ClausePromotionReadiness = READY_FOR_DEFINITION_CHECK`
- 否则如果 `DefinitionClauseEffect` 是 `*_REWRITE_EFFECT`
  - `ClauseVerdict = CLAUSE_SATISFIED_WITH_REWRITE`
  - `ClausePromotionReadiness = READY_WITH_REWRITE_NOTE`
- 否则
  - `ClauseVerdict = CLAUSE_REVIEW_REQUIRED`
  - `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`

工程含义：

- `HXZ` 当前大概率会落在这一支
- 结论不是“已经是单主板家族”，而是“主板持续性条款仍可追踪，但路径被改写”

### 3. 多板簇排除条款

适用：

- `CLUSTER_DIRECT_CONTROL_EXCLUSION_CLAUSE`
- `CLUSTER_ACCESSORY_EXCLUSION_CLAUSE`

规则：

- 如果 `DefinitionClauseEffect = CLUSTER_DIRECT_CONTROLLER_NOT_EXCLUDABLE_EFFECT`
  - `ClauseVerdict = CLAUSE_BROKEN_DIRECT`
  - `ClausePromotionReadiness = READY_FOR_DEFINITION_CHECK`
- 如果 `DefinitionClauseEffect = CLUSTER_ACCESSORY_EXCLUSION_SUPPORTED_EFFECT`
  - `ClauseVerdict = CLAUSE_SATISFIED_STABLE`
  - `ClausePromotionReadiness = READY_FOR_DEFINITION_CHECK`
- 否则
  - `ClauseVerdict = CLAUSE_REVIEW_REQUIRED`
  - `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`

工程含义：

- `MJ` 这类样本可先用这一支把“能否当附属件排除”稳定下来

### 4. 包络控制重分配条款

适用：

- `CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE`
- `GENERAL_DEFINITION_REVIEW_CLAUSE`

规则：

- 一律先进入
  - `ClauseVerdict = CLAUSE_REVIEW_REQUIRED`
  - `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`

工程含义：

- `YPGL / GL` 这类混合样本先保守，不急着升级成定义判定

## 建议新增字段

在 `CoreBodyProofPartResult` 上新增：

- `TopologyRewriteClauseVerdictCode`
- `TopologyRewriteClauseVerdictLabelZh`
- `TopologyRewriteClausePromotionReadinessCode`
- `TopologyRewriteClausePromotionReadinessLabelZh`

在聚合摘要里新增：

- `ClauseVerdicts`
- `LeadClauseVerdict`
- `LeadClauseVerdictShare`
- `ClausePromotionReadinesses`
- `LeadClausePromotionReadiness`

## 输出要求

### 代表零件明细

每个代表零件除了已有字段外，再展示：

- `DefinitionClause`
- `DefinitionClauseEffect`
- `ClauseVerdict`
- `ClausePromotionReadiness`

### 构件级聚合

每个构件补充：

- 主条款判定
- 主条款判定占比
- 主条款提升准备度
- 是否混合判定

## 与现有层的边界

`DefinitionClauseDecisionBridge` 不负责：

- 直接给出最终 `BuiltUpH / BuiltUpBox / Irregular`
- 直接改写 `BodyFamily`
- 替代后续家族定义验证器

它只负责把“效果层”再推进到“定义条款判定层”，为后续真正的家族定义验证器提供更干净的输入。

## 实施顺序

### 第一步

在 `CoreBodyProofEngine` 里新增桥接方法：

- `AttachTopologyRewriteClauseVerdicts(...)`

### 第二步

把桥接字段写进：

- `core-body-proof-summary.zh-CN.md`
- `topology-rewrite-summary.zh-CN.md`
- 独立摘要重建脚本

### 第三步

在 `real_04` 上重点检查四类样本：

- `GKZ`
- `HXZ`
- `MJ`
- `GL / YPGL`

### 第四步

确认这层稳定后，再进入下一层：

- “哪条定义条款已经足够升级成家族验证前提”

## 预期收益

完成这层后，阶段 5 会从：

- `Pattern / Effect / Risk / ProofTarget`

推进到：

- `DefinitionClause / DefinitionClauseEffect / ClauseVerdict / PromotionReadiness`

这样后面进入真正的家族定义验证时，就不会再直接面对一堆混杂的几何提示，而是面对更接近工程判定的中间证据。
