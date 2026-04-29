# DefinitionClause Bridge 映射表

## 文档目的

本文档把前两轮已经冻结的：

- [DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md>)
- [DEFINITION_CLAUSE_DECISION_EVIDENCE_PRECEDENCE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_EVIDENCE_PRECEDENCE.zh-CN.md>)

继续收敛成 bridge/helper 层可直接实现的映射表。

目标不是再讨论原则，而是明确：

- 输入层级组合
- 输出 `ClauseVerdict`
- 输出 `ClausePromotionReadiness`
- 默认降级路径

这样下一轮写 `DefinitionClauseDecisionBridge.cs` 时，可以直接照表落代码。

---

## 输入维度

bridge 层当前统一只看 5 个归一化维度：

1. `SourceTier`
2. `StationTier`
3. `TopologyTier`
4. `ProofCompletenessTier`
5. `LeadClauseTier`

辅助维度：

- `HasConflict`
- `HasReviewConflict`
- `CandidateDirection`

其中：

- `CandidateDirection`
  - `SATISFIED_CANDIDATE`
  - `BROKEN_CANDIDATE`
  - `MIXED_CANDIDATE`
  - `UNKNOWN_CANDIDATE`

---

## Tier 定义

### `SourceTier`

- `SOURCE_REAL_PRIMARY`
  - `real_input` 主链稳定命中
- `SOURCE_DUAL_CONSISTENT`
  - 双视图一致
- `SOURCE_RECOGNITION_ONLY`
  - 仅 `recognition_input`

### `StationTier`

- `STATION_PRIORITY_MAJORITY`
  - 多数优先站位持续成立
- `STATION_PRIORITY_MINOR`
  - 少数优先站位成立
- `STATION_LOCAL_ONLY`
  - 只在局部复杂段或端部成立
- `STATION_EPISODIC`
  - 单站位偶发

### `TopologyTier`

- `TOPOLOGY_LOST_CLOSED_LOOP`
- `TOPOLOGY_LOST_ENVELOPE`
- `TOPOLOGY_REWRITE_STRUCTURAL`
- `TOPOLOGY_DIMENSION_SWITCH`
- `TOPOLOGY_WEAK_CHANGE`

### `ProofCompletenessTier`

- `PROOF_COMPLETE`
  - `ProofType + FamilyProofTarget` 都明确
- `PROOF_TYPE_ONLY`
- `PROOF_TARGET_ONLY`
- `PROOF_UNRESOLVED`

### `LeadClauseTier`

- `CLAUSE_STABLE_FULL`
  - `LeadClauseShare=100%`
- `CLAUSE_DOMINANT`
  - `LeadClauseShare>=67%`
- `CLAUSE_WEAK`
  - `LeadClauseShare<67%`
- `CLAUSE_UNSET`

---

## 一阶映射：CandidateDirection

bridge 层先根据 effect 结构，把原始信号压成 `CandidateDirection`。

### `SATISFIED_CANDIDATE`

满足以下任一组：

1. `TopologyTier=TOPOLOGY_REWRITE_STRUCTURAL`
   - 且 `ProofCompletenessTier` 不低于 `PROOF_TYPE_ONLY`
   - 且 `LeadClauseTier` 不低于 `CLAUSE_DOMINANT`
2. `TopologyTier=TOPOLOGY_LOST_ENVELOPE`
   - 但 effect 方向指向“该零件存在时结构成立，去掉后被破坏”

### `BROKEN_CANDIDATE`

满足以下任一组：

1. `TopologyTier=TOPOLOGY_LOST_CLOSED_LOOP`
2. `TopologyTier=TOPOLOGY_LOST_ENVELOPE`
   - 且 effect 方向明确是“去掉后破坏”

### `MIXED_CANDIDATE`

满足以下任一组：

1. `LeadClauseTier=CLAUSE_DOMINANT`
   - 但存在 `HasConflict=True`
2. `LeadClauseTier=CLAUSE_WEAK`
   - 且仍有高于 `TOPOLOGY_WEAK_CHANGE` 的结构信号

### `UNKNOWN_CANDIDATE`

满足以下任一组：

1. `SourceTier=SOURCE_RECOGNITION_ONLY`
2. `StationTier` 低于 `STATION_PRIORITY_MINOR`
3. `ProofCompletenessTier=PROOF_UNRESOLVED`
   - 且 `LeadClauseTier` 低于 `CLAUSE_DOMINANT`

---

## 二阶映射：ClauseVerdict

### 表 A：`SATISFIED`

必须同时满足：

- `CandidateDirection=SATISFIED_CANDIDATE`
- `SourceTier != SOURCE_RECOGNITION_ONLY`
- `StationTier = STATION_PRIORITY_MAJORITY`
- `LeadClauseTier = CLAUSE_STABLE_FULL`
- `ProofCompletenessTier = PROOF_COMPLETE`
- `HasConflict = False`

### 表 B：`BROKEN`

必须同时满足：

- `CandidateDirection=BROKEN_CANDIDATE`
- `SourceTier = SOURCE_REAL_PRIMARY`
- `StationTier = STATION_PRIORITY_MAJORITY`
- `TopologyTier` 为：
  - `TOPOLOGY_LOST_CLOSED_LOOP`
  - 或 `TOPOLOGY_LOST_ENVELOPE`
- `HasConflict = False`

### 表 C：`MIXED`

满足以下任一组：

1. `CandidateDirection=MIXED_CANDIDATE`
2. `LeadClauseTier=CLAUSE_DOMINANT`
   - 但 `HasConflict=True`
3. `LeadClauseTier=CLAUSE_DOMINANT`
   - 且 `ProofCompletenessTier != PROOF_COMPLETE`

### 表 D：`REVIEW_REQUIRED`

满足以下任一组：

1. `CandidateDirection=SATISFIED_CANDIDATE`
   - 且 `HasReviewConflict=True`
2. `CandidateDirection=BROKEN_CANDIDATE`
   - 且 `HasReviewConflict=True`
3. `LeadClauseTier=CLAUSE_STABLE_FULL`
   - 但 `ProofCompletenessTier != PROOF_COMPLETE`

### 表 E：`INSUFFICIENT_EVIDENCE`

默认兜底：

- 以上都不满足
- 或 `CandidateDirection=UNKNOWN_CANDIDATE`

---

## 三阶映射：ClausePromotionReadiness

### 表 F：`READY_FOR_PROMOTION`

必须同时满足：

- `ClauseVerdict=SATISFIED` 或 `BROKEN`
- `SourceTier != SOURCE_RECOGNITION_ONLY`
- `StationTier = STATION_PRIORITY_MAJORITY`
- `ProofCompletenessTier = PROOF_COMPLETE`
- `LeadClauseTier = CLAUSE_STABLE_FULL`
- `HasConflict=False`
- `HasReviewConflict=False`

### 表 G：`READY_FOR_REVIEW`

满足以下任一组：

1. `ClauseVerdict=REVIEW_REQUIRED`
2. `ClauseVerdict=BROKEN`
   - 但仍需人工确认破坏是否真的代表家族边界
3. `ClauseVerdict=SATISFIED`
   - 但 `HasReviewConflict=True`

### 表 H：`HOLD_EFFECT_ONLY`

默认兜底：

- `ClauseVerdict=MIXED`
- `ClauseVerdict=INSUFFICIENT_EVIDENCE`
- 或 `SourceTier=SOURCE_RECOGNITION_ONLY`
- 或 `StationTier` 低于 `STATION_PRIORITY_MAJORITY`

---

## 降级矩阵

### 冲突一：来源冲突

- 若 `SourceTier=SOURCE_RECOGNITION_ONLY`
  - 最高只能到：
    - `ClauseVerdict=INSUFFICIENT_EVIDENCE`
    - `PromotionReadiness=HOLD_EFFECT_ONLY`

### 冲突二：条款稳定但证明对象未定

- 若 `LeadClauseTier=CLAUSE_STABLE_FULL`
  - 但 `ProofCompletenessTier != PROOF_COMPLETE`
  - 则：
    - `SATISFIED/BROKEN -> REVIEW_REQUIRED`
    - `READY_FOR_PROMOTION -> READY_FOR_REVIEW`

### 冲突三：结构强信号但条款混合

- 若 `TopologyTier` 为强信号
  - 但 `LeadClauseTier=CLAUSE_DOMINANT`
  - 且 `HasConflict=True`
  - 则：
    - `SATISFIED/BROKEN -> MIXED`
    - `READY_FOR_PROMOTION -> HOLD_EFFECT_ONLY`

### 冲突四：review 冲突

- 若 `HasReviewConflict=True`
  - 则：
    - `SATISFIED/BROKEN -> REVIEW_REQUIRED`
    - `READY_FOR_PROMOTION -> READY_FOR_REVIEW`

---

## 首轮样本映射表

### `GKZ`

- 目标输入组合：
  - `SOURCE_REAL_PRIMARY`
  - `STATION_PRIORITY_MAJORITY`
  - `TOPOLOGY_REWRITE_STRUCTURAL` 或 `TOPOLOGY_LOST_CLOSED_LOOP`
  - `PROOF_COMPLETE`
  - `CLAUSE_STABLE_FULL`
- 目标输出：
  - `ClauseVerdict=SATISFIED`
  - `ClausePromotionReadiness=READY_FOR_PROMOTION`

### `HXZ`

- 目标输入组合：
  - `SOURCE_REAL_PRIMARY`
  - `STATION_PRIORITY_MAJORITY`
  - `TOPOLOGY_REWRITE_STRUCTURAL`
  - `PROOF_COMPLETE`
  - `CLAUSE_STABLE_FULL`
- 目标输出：
  - `ClauseVerdict=SATISFIED`
  - `ClausePromotionReadiness=READY_FOR_PROMOTION`

### `GL`

- 目标输入组合：
  - `SOURCE_REAL_PRIMARY`
  - `STATION_PRIORITY_MAJORITY`
  - `TOPOLOGY_REWRITE_STRUCTURAL`
  - `PROOF_TYPE_ONLY` 或 `PROOF_UNRESOLVED`
  - `CLAUSE_DOMINANT`
  - `HasConflict=True`
- 目标输出：
  - `ClauseVerdict=MIXED`
  - `ClausePromotionReadiness=HOLD_EFFECT_ONLY`

### `MJ`

- 目标输入组合：
  - `SOURCE_REAL_PRIMARY`
  - `STATION_PRIORITY_MAJORITY`
  - `CLAUSE_STABLE_FULL`
  - 但 `HasReviewConflict=True` 或 `ProofCompletenessTier != PROOF_COMPLETE`
- 目标输出：
  - `ClauseVerdict=REVIEW_REQUIRED`
  - `ClausePromotionReadiness=READY_FOR_REVIEW`

### `YPGL`

- 目标输入组合：
  - `SOURCE_REAL_PRIMARY`
  - `STATION_PRIORITY_MINOR` 或 `STATION_PRIORITY_MAJORITY`
  - `CLAUSE_DOMINANT`
  - `HasConflict=True`
- 目标输出：
  - `ClauseVerdict=MIXED`
  - `ClausePromotionReadiness=HOLD_EFFECT_ONLY`

---

## 建议的实现步骤

下一轮在代码里实现时，应按以下顺序：

1. 定义 5 个 tier 枚举
2. 定义 `CandidateDirection`
3. 实现 `MapVerdict(...)`
4. 实现 `MapPromotionReadiness(...)`
5. 用 `GKZ / HXZ / GL / MJ / YPGL` 五组 fixture 先做最小回归

bridge 层不要直接读取原始复杂对象做大 if-else，应该先把输入规整成 tier，再查映射表。

---

## 当前结论

前两轮已经固定了“升级闸门”和“证据优先级”，这一轮把它们继续收敛成了 bridge 可直接实现的映射表。

下一轮如果环境允许稳定读取代码，最自然的落点就是：

- 在 `DefinitionClauseDecisionBridge.cs` 里先补 tier 归一化
- 再把 `ClauseVerdict / ClausePromotionReadiness` 的映射实现成纯函数
- 然后用现有 fixture 和 sidecar 链做第一轮 smoke
