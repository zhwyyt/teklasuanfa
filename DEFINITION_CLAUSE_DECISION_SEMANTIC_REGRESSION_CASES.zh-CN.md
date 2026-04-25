# DefinitionClause 语义回归样本契约

## 目的

在 `DefinitionClauseDecisionBridge` 的语义层新增 `TopologyTier=None`，并将 `ProofCompletenessTier` 从原先的“部分证明”进一步拆成 `TypeOnly` 与 `TargetOnly` 之后，需要先把最小回归样本的落点冻结下来，避免后续把新 mapper / effect-adapter / validation bundle 并回旧 bridge 链时重新把这两个语义收敛掉。

本文件只冻结“样本名称 -> tier 预期 -> verdict 预期 -> promotion 预期”，不直接决定最终代码组织方式。

## 样本 1：`SYNTHETIC_NONE_TOPOLOGY`

### 设计意图

- 验证“没有任何拓扑级改写信号”时，bridge 不会再把这类样本默认抬成 `WeakChange`
- 验证即使来源、站位、主导条款占比都足够稳，只要拓扑层没有实际结构变化，也只能停在 `effect-only`

### 约束输入

- `SourceTier = RealPrimary`
- `StationTier = PriorityMajority`
- `TopologyTier = None`
- `ProofCompletenessTier = Complete`
- `LeadClauseTier = StableFull`
- 无显式冲突、无 mixed 证据

### 预期输出

- `CandidateDirection = Unknown`
- `ClauseVerdict = InsufficientEvidence`
- `ClausePromotionReadiness = HoldEffectOnly`

### 验证重点

- `None` 必须与 `WeakChange` 分离，不能因为“真实主样本 + 站位稳定 + 条款占比高”而被动升级
- 该样本用于锁定“没有拓扑改写，就没有 definition verdict”

## 样本 2：`SYNTHETIC_TARGET_ONLY_REVIEW`

### 设计意图

- 验证 `TargetOnly` 不再与 `TypeOnly` 混成同一层级
- 验证“证明目标已基本明确，但证明对象类型仍未闭合”的样本，应该停在 `review`，而不是直接 promotion

### 约束输入

- `SourceTier = RealPrimary`
- `StationTier = PriorityMajority`
- `TopologyTier = RewriteStructural`
- `ProofCompletenessTier = TargetOnly`
- `LeadClauseTier = StableFull`
- 无显式冲突、无 mixed 证据

### 预期输出

- `CandidateDirection = SatisfiedCandidate`
- `ClauseVerdict = ReviewRequired`
- `ClausePromotionReadiness = ReadyForReview`

### 验证重点

- `TargetOnly` 必须显式落在 `ReviewRequired`
- 该样本用于锁定“证明目标已收敛，但对象类型仍未完成”的中间态，不允许直接掉到 `HoldEffectOnly`，也不允许越级到 `ReadyForPromotion`

## 并回顺序

1. 先把上述两类样本补进 `DefinitionClauseDecisionBridgeMapperFixtures`
2. 再把同名样本补进 `DefinitionClauseDecisionBridgeEffectAdapterFixtures`
3. 然后用现有 mapper / effect-adapter / validation bundle 三条新验证链复核：
   - `TopologyTier=None` 没有被错误提升
   - `TargetOnly` 与 `TypeOnly` 的 verdict / readiness 没有重新合流
4. 最后再把这套语义回归样本并回旧 bridge / fixture / sidecar 导出链
