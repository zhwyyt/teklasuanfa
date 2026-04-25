# DefinitionClause 证据优先级

## 文档目的

本文档用于冻结 `DefinitionClause effect -> ClauseVerdict -> ClausePromotionReadiness` 之间的证据优先级与降级规则。

阶段 5 当前已经具备较丰富的 effect 信号，但仍缺少“当多个信号同时出现时，谁优先、谁只能辅助、谁必须降级”的统一约束。

本文件的目标是：

- 固定证据来源优先级
- 固定站位稳定性优先级
- 固定拓扑信号优先级
- 固定冲突时的降级规则

这样下一轮 bridge/helper 接线时，不会在代码里临时发明 tie-break。

---

## 适用范围

本文件适用于以下输出层：

- `DefinitionClause effect`
- `ClauseVerdict`
- `ClausePromotionReadiness`

本文件不直接产生 `H / BOX / T` 家族结论，只负责给阶段 5 和阶段 6 之间的证据排序。

---

## 总原则

1. 真实输入优先于识别输入。
2. 多数稳定站位优先于局部偶发站位。
3. 闭合/包络破坏优先于普通几何改写提示。
4. 能解释成“条款满足/破坏”的信号，优先于只能解释成“结构发生变化”的信号。
5. 一旦出现高优先级冲突，宁可降级到 `MIXED / REVIEW_REQUIRED / HOLD_EFFECT_ONLY`，也不强行 promotion。

---

## 证据来源优先级

### 第一层：来源优先级

从高到低固定如下：

1. `real_input` 视图下的稳定证据
2. `real_input` 与 `recognition_input` 双视图一致的证据
3. 仅 `recognition_input` 可见的证据

约束：

- 第 3 层证据不得单独触发 `READY_FOR_PROMOTION`
- 第 3 层证据最多只能作为 `effect / hint / review-note`
- 若第 1 层与第 3 层冲突，以第 1 层为准

---

## 站位稳定性优先级

### 第二层：站位稳定性

从高到低固定如下：

1. 多数 `priority station` 持续成立
2. 少数 `priority station` 成立，但未形成多数
3. 仅 `local_complex` / 端部修剪段附近成立
4. 单站位偶发成立

约束：

- 只有第 1 层站位稳定性，才允许进入 `SATISFIED / BROKEN` 候选
- 第 2 层最多进入 `MIXED / REVIEW_REQUIRED`
- 第 3/4 层默认保留为 `INSUFFICIENT_EVIDENCE`

---

## 拓扑信号优先级

### 第三层：拓扑和控制结构信号

从高到低固定如下：

1. `LostClosedLoop`
2. `LostEnvelopeSupport`
3. `TopologyRewrite` 且已能落到明确 `ControllerRole + ShapeRole + ProofType + FamilyProofTarget`
4. `DominantDimensionSwitch`
5. 仅有普通包络变化或 trace 形态变化

约束：

- 第 1/2 层更接近“条款被破坏”的强信号
- 第 3 层是“结构发生了有意义改写”的中强信号
- 第 4/5 层默认只能作为辅助解释，不能单独触发 promotion

---

## 证明完整度优先级

### 第四层：证明对象完整度

从高到低固定如下：

1. `ProofType` 与 `FamilyProofTarget` 都明确
2. 仅 `ProofType` 明确
3. 仅 `FamilyProofTarget` 明确
4. 两者都待定

约束：

- 第 1 层才能进入 `READY_FOR_PROMOTION` 候选
- 第 2/3 层最多进入 `READY_FOR_REVIEW`
- 第 4 层默认停留在 `HOLD_EFFECT_ONLY`

---

## LeadClause 优先级

### 第五层：主条款稳定度

从高到低固定如下：

1. `LeadClauseShare = 100%` 且 `ClauseMix = 单条款稳定`
2. `LeadClauseShare >= 67%` 且 `ClauseMix = 主条款占优`
3. `LeadClauseShare < 67%`
4. `LeadClause` 缺失或不稳定

约束：

- 第 1 层可进入 `SATISFIED / BROKEN`
- 第 2 层最多进入 `MIXED / REVIEW_REQUIRED`
- 第 3/4 层默认停留在 `INSUFFICIENT_EVIDENCE`

---

## Verdict 生成优先级

### `SATISFIED`

必须同时满足：

- 来源优先级为第 1 层或第 2 层
- 站位稳定性为第 1 层
- 主条款稳定度为第 1 层
- 证明完整度至少为第 1 层
- 没有更高优先级的 `BROKEN` 信号冲突

### `BROKEN`

必须同时满足：

- 来源优先级为第 1 层
- 站位稳定性为第 1 层
- 拓扑信号优先级为第 1 或第 2 层
- 破坏方向与当前 `LeadClause` 可以稳定对齐

### `MIXED`

适用于：

- 主条款占优但不绝对稳定
- 或多个条款方向都被高于普通噪声的证据命中

### `REVIEW_REQUIRED`

适用于：

- 已有较高优先级条款方向
- 但证明完整度不足、控制角色冲突、或样本边界不适合自动化 promotion

### `INSUFFICIENT_EVIDENCE`

适用于：

- 只命中低层来源、低层站位、低层拓扑或低层证明完整度

---

## PromotionReadiness 生成优先级

### `READY_FOR_PROMOTION`

必须同时满足：

- `ClauseVerdict = SATISFIED` 或 `BROKEN`
- 来源优先级不低于第 2 层
- 站位稳定性为第 1 层
- 主条款稳定度为第 1 层
- 证明完整度为第 1 层
- 没有 `MIXED / REVIEW_REQUIRED` 级别的冲突

### `READY_FOR_REVIEW`

适用于：

- `ClauseVerdict = REVIEW_REQUIRED`
- 或 `ClauseVerdict = BROKEN` 但仍需人工确认破坏方向是否代表真实家族边界
- 或 `ClauseVerdict = SATISFIED` 但证明对象仍存在边界冲突

### `HOLD_EFFECT_ONLY`

适用于：

- `MIXED`
- `INSUFFICIENT_EVIDENCE`
- 仅 `recognition_input` 命中
- 仅单站位偶发命中

---

## 冲突降级规则

出现以下情况时必须降级，不允许强行保持高 verdict：

1. `real_input` 与 `recognition_input` 方向冲突
2. `LeadClauseShare` 高，但 `ProofType / FamilyProofTarget` 仍待定
3. `LostClosedLoop` 与 `TopologyRewrite` 指向不同家族方向
4. `ControllerRole` 与 `ShapeRole` 无法组成稳定证明对象
5. 样本整体像正确，但代表零件级别仍无法唯一定位

降级顺序固定为：

- 优先从 `READY_FOR_PROMOTION` 降到 `READY_FOR_REVIEW`
- 若冲突仍未解，再降到 `HOLD_EFFECT_ONLY`
- verdict 则从 `SATISFIED / BROKEN` 降到 `REVIEW_REQUIRED`，必要时再降到 `MIXED`

---

## 首轮样本约束

### `GKZ`

- 允许作为最高优先级正例
- 目标：`real_input + 多数 priority station + 单条款稳定 + 对壁闭合证明对象`

### `HXZ`

- 允许作为最高优先级单主板正例
- 目标：`real_input + 多数 priority station + 单条款稳定 + 单主板主轮廓证明`

### `GL`

- 默认混合样本
- 即使 `LeadClause` 占优，也应优先停在 `MIXED / HOLD_EFFECT_ONLY`

### `MJ`

- 可作为“条款稳定但 promotion 仍保守”的候选样本
- 首轮默认不直接给 `READY_FOR_PROMOTION`

### `YPGL`

- 默认混合样本
- 当前只保留 effect / mixed / review 路径

---

## 对实现层的要求

下一轮 bridge/helper 接线时，应按以下顺序落地：

1. 先把原始 effect 信号规整成“来源层级 / 站位层级 / 拓扑层级 / 证明完整度层级 / 主条款稳定度层级”
2. 再根据本文件生成 `ClauseVerdict`
3. 再根据本文件生成 `ClausePromotionReadiness`
4. 最后才把结果送进 sidecar / summary / full-run

不允许在 `Program.cs`、Markdown 摘要或 PowerShell 脚本里绕过这套优先级直接拼 verdict。

---

## 当前结论

阶段 5 到阶段 6 的关键，不是“再多收集一点 effect”，而是先把 effect 排序。

本文件把排序规则固定下来后，下一轮实现就可以从：

- effect 很多但难解释

推进到：

- effect 有主次
- verdict 有保守边界
- promotion 有统一闸门
