# DefinitionClause 判定升级门槛

## 文档目的

本文档把阶段 5 当前已有的 `DefinitionClause effect` 解释层，继续冻结成可执行的“判定升级门槛”。

目标不是直接替代阶段 6 的家族判定器，而是先明确：

- 什么时候条款效果只能停留在 `effect / hint`
- 什么时候可以升级成结构化 `ClauseVerdict`
- 什么时候已经具备进入家族定义判定器的 `PromotionReadiness`

本文档是阶段 5 到阶段 6 之间的闸门定义。

---

## 非目标

- 不在本阶段直接输出最终 `H / BOX / T` 家族结论。
- 不允许仅凭单个零件被删后的启发式扰动，直接判定“条款满足”。
- 不允许把 `recognition_input` 视图里由虚拟主体造成的效果，直接当作真实定义证据。

---

## 当前输入证据面

升级门槛只能使用已经进入阶段 5/sidecar 的稳定证据，不再临时引入新的启发式特征。

当前允许使用的证据包括：

- `DefinitionClause`
- `LeadClause`
- `LeadClauseShare`
- `ClauseMix`
- `TopologyRewritePattern`
- `ControllerRole`
- `ShapeRole`
- `FamilyRisk`
- `ProofType`
- `FamilyProofTarget`
- `Priority station` 持续性
- `LostBodyCoverage / LostEnvelopeSupport / LostClosedLoop`
- `TopologyRewrite*StationIds`
- `real_input` 视图下的主体候选 / 稳定区 / 迹线 / 拓扑结果

当前禁止直接作为升级依据的信号包括：

- 仅来自 `recognition_input` 的虚拟主体结果
- 未进入稳定站位的局部噪声迹线
- 只在单一站位短暂出现的闭环候选
- 未经过 grouped peer / cohort 解释的孤立零件扰动

---

## ClauseVerdict 定义

`ClauseVerdict` 用于回答：

- 该零件或该构件当前更像是“正在满足某条定义条款”
- 还是“正在破坏某条定义条款”
- 还是“证据不足 / 混合 / 只能保留效果提示”

当前冻结如下枚举：

- `SATISFIED`
  - 条款被稳定满足，且证据已经不只是 effect 提示。
- `BROKEN`
  - 删除或扰动该零件后，条款关键结构被稳定破坏。
- `MIXED`
  - 同一构件或同一候选零件组出现多个条款方向并存，尚不能收敛成单一 verdict。
- `INSUFFICIENT_EVIDENCE`
  - 当前只够输出 effect 提示，不够输出 verdict。
- `REVIEW_REQUIRED`
  - 已有较强结构信号，但仍存在边界冲突，必须进入人工复核。

---

## PromotionReadiness 定义

`ClausePromotionReadiness` 用于回答：

- 当前结果是否已经可以送入阶段 6 家族定义判定器
- 还是仍应停留在阶段 5 的 effect / verdict 层

当前冻结如下枚举：

- `READY_FOR_PROMOTION`
  - 已满足进入家族定义判定器的最低稳定性门槛。
- `READY_FOR_REVIEW`
  - 已有足够强的结构冲突或边界冲突，应优先进入 review，而不是直接 promotion。
- `HOLD_EFFECT_ONLY`
  - 当前仍只能保留 effect / hint，不应继续升级。

---

## 升级必要条件

### 从 `effect` 升到 `ClauseVerdict`

必须同时满足：

1. `LeadClause` 非空，且存在可解释的 `LeadClauseShare`
2. 证据来自 `real_input` 主链，而不是只来自 `recognition_input`
3. 关键证据不是单站位偶发，而是多数优先站位持续
4. `ProofType` 与 `FamilyProofTarget` 至少有一项不为待定
5. 当前 effect 可以解释为“满足”或“破坏”，而不是只有“有变化”

若以上任一条件不满足，则只能保留 `INSUFFICIENT_EVIDENCE`

### 从 `ClauseVerdict` 升到 `PromotionReadiness=READY_FOR_PROMOTION`

必须同时满足：

1. `ClauseVerdict=SATISFIED` 或 `ClauseVerdict=BROKEN`
2. `LeadClauseShare` 达到“主条款占优且稳定”的门槛
3. `ClauseMix` 不是“多条款均势混合”
4. `ControllerRole / ShapeRole / ProofType / FamilyProofTarget` 之间不存在明显互相冲突
5. 不依赖虚拟主体或单零件偶发扰动才能成立

---

## 保守门槛

为避免重新滑回启发式，当前先冻结保守门槛：

- `LeadClauseShare = 100%`
  - 可以作为 `SATISFIED` 或 `BROKEN` 的优先候选门槛
- `LeadClauseShare >= 67%` 且 `ClauseMix` 仍显示混合
  - 最多停留在 `MIXED`
- 出现以下任一情形时，不允许直接 promotion：
  - `FamilyProofTarget` 仍待定
  - `ProofType` 仍待定
  - `TopologyRewrite` 只说明“结构改写”，但没有落到条款满足/破坏
  - 关键站位存在未解释的 `LostClosedLoop` 与混合条款并存

---

## Verdict 映射规则

### `SATISFIED`

适用于：

- 主条款单一且稳定
- 关键控制结构在多数优先站位上持续成立
- `ProofType / FamilyProofTarget` 与条款方向一致

典型目标样本：

- `GKZ`
  - 当“箱型：闭合/对边稳定条款”稳定占优，且对壁 cohort 结构持续成立
- `HXZ`
  - 当“主板：多数站位持续性条款”稳定占优，且单主板持续结构成立

当前新增保守例外：

- `StandardSection / STANDARD_BOX` 单件型材允许走 source-semantic 直达链进入 `SATISFIED`
- 仅当同时满足：
  - `SourceSemanticBodyFamily = StandardSection`
  - `SourceSemanticSectionType = STANDARD_BOX`
  - `SynthesizedBody = true`
  - `ImportSynthesisKind = BOX`
  - `core-body-proof` 只有 `1` 个 `CoreBodyPart`
  - `0` 个 `ReviewPart`
  - 当前 assembly 仍停在 `NO_CLAUSE_ROWS`
- 才允许把 assembly 直接收紧到：
  - `LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE`
  - `ClauseVerdict = SATISFIED`
  - `PromotionReadiness = READY_FOR_PROMOTION`
- 这条规则当前只解释：
  - source-side 已明确是单件标准箱型材
  - 阶段 5/6 需要补一条“不再强制先走 built-up plate-chain”的直达语义链
- 不等于：
  - 所有 `BOX` 或所有 `YPGL` 都直接 promotion
- 当前已验证样本：
  - `T3-2YPGL-1 / 2 / 4 / 7 / 10 / 13 / 14 / 16 / 19 / 20 / 22 / 24 / 25`

### `BROKEN`

适用于：

- 删除某代表零件后，条款关键结构被稳定破坏
- 且破坏不是“删掉任意主件都空掉”的伪信号

典型触发：

- `LostClosedLoop` 与闭合条款稳定绑定
- `LostEnvelopeSupport` 与主轮廓控制边条款稳定绑定

当前新增保守例外：

- `H` 折型翼缘 assembly 小类允许在不新增旧启发式的前提下，按 assembly 聚合窄 gate 进入 `BROKEN`
- 仅当同时满足：
  - `LeadClause = H_WEB_FLANGE_CONTINUITY_CLAUSE`
  - `LeadClauseShare = 100%`
  - `ClauseMix = SINGLE_CLAUSE`
  - `LeadClauseEffectDirection = BrokenCandidate`
  - `Break/Rewrite = 2/1`
  - 且当前 assembly 仍仅停在 `ReviewRequired + ReadyForReview`
- 才允许把 assembly verdict 从 `REVIEW_REQUIRED` 收紧到 `BROKEN`
- 这条规则当前只解释：
  - “腹板/翼缘连续性破坏 effect 在 assembly 上已形成稳定主导”
- 这条规则当前已明确覆盖的真实边界包括：
  - 常规折型翼缘 `H`
  - `T3-2GL-31` 这类“开口 `H` 主轮廓稳定，但下翼缘由折板/拼接翼缘 review 件表达”的变体
- 对这类变体的保守解释是：
  - `2 x BodyCandidate` 继续承担开口 `H` 主轮廓
  - `1 x SpecialShape` 可作为不承担 envelope support 的折板/拼接翼缘 review 件并入同一 `H` 条款
- 不等于：
  - 已可直接 promotion 到阶段 6
- 所以 readiness 仍保守保持在 `READY_FOR_REVIEW`

### `MIXED`

适用于：

- `LeadClause` 占优但未绝对稳定
- 或构件内部存在多个条款方向并存

当前默认把下列样本保守留在 `MIXED / HOLD_EFFECT_ONLY`：

- `GL`
- `MJ`
- `YPGL`

### `INSUFFICIENT_EVIDENCE`

适用于：

- 只有 effect 提示，没有稳定条款结构
- `ProofType / FamilyProofTarget` 都还待定
- 证据仍高度依赖虚拟主体或局部噪声站位

### `REVIEW_REQUIRED`

适用于：

- 已有较强条款方向
- 但 `ControllerRole / ShapeRole / FamilyProofTarget` 仍存在边界冲突
- 或构件整体很可能正确，但代表零件级 verdict 仍不安全

---

## PromotionReadiness 映射规则

### `READY_FOR_PROMOTION`

当前只给最保守样本：

- `GKZ`
  - 当 `LeadClause=箱型：闭合/对边稳定条款`
  - 且 `LeadClauseShare=100%`
  - 且 `FamilyProofTarget=箱型对壁闭合证明目标`
- `HXZ`
  - 当 `LeadClause=主板：多数站位持续性条款`
  - 且 `LeadClauseShare=100%`
  - 且 `ProofType=单主板主轮廓证明`

### `READY_FOR_REVIEW`

适用于：

- `ClauseVerdict=BROKEN`
- 或 `ClauseVerdict=REVIEW_REQUIRED`
- 或虽有较强主条款，但边界冲突无法自动化消解

当前补充：

- 对 `H_WEB_FLANGE_CONTINUITY_CLAUSE` 的折型翼缘 assembly 小类，即使已按窄 gate 进入 `BROKEN`
- 只要其 `BROKEN` 仍来自 `BrokenCandidate + StableFull + B2/R1` 的 assembly 聚合例外
- 当前仍一律保持 `READY_FOR_REVIEW`
- 暂不进入 `READY_FOR_PROMOTION`

### `HOLD_EFFECT_ONLY`

适用于：

- `MIXED`
- `INSUFFICIENT_EVIDENCE`
- 仍需更多稳定站位或更清晰证明对象才能升级

---

## 首轮样本落点

当前先冻结首轮预期：

- `GKZ`
  - 目标：`ClauseVerdict=SATISFIED`
  - 目标：`PromotionReadiness=READY_FOR_PROMOTION`
- `HXZ`
  - 目标：`ClauseVerdict=SATISFIED`
  - 目标：`PromotionReadiness=READY_FOR_PROMOTION`
- `GL`
  - 普通 `GL` 目标：`ClauseVerdict=MIXED`
  - 普通 `GL` 目标：`PromotionReadiness=HOLD_EFFECT_ONLY`
  - 折型翼缘 `H` 子类例外：
    - 当 assembly 满足
      - `H_WEB_FLANGE_CONTINUITY_CLAUSE`
      - `StableFull`
      - `BrokenCandidate`
      - `Break/Rewrite = 2/1`
    - 允许目标收紧到：
      - `ClauseVerdict=BROKEN`
      - `PromotionReadiness=READY_FOR_REVIEW`
- `MJ`
  - 目标：先允许 `SATISFIED` 候选，但默认保守停在 `READY_FOR_REVIEW` 或 `HOLD_EFFECT_ONLY`
- `YPGL`
  - 目标：`ClauseVerdict=MIXED`
  - 目标：`PromotionReadiness=HOLD_EFFECT_ONLY`

---

## 对实现层的约束

下一轮接线时应按以下顺序推进：

1. 先在 bridge/helper 层输出 `ClauseVerdict / ClausePromotionReadiness`
2. 再接到结果模型与 sidecar 摘要
3. 再跑 `fixture -> summary -> full-run`
4. 最后才考虑是否把它提升为阶段 6 的家族定义输入

不允许跳过 bridge/fixture，直接在 `Program.cs` 或摘要脚本里临时拼 verdict。

---

## 当前结论

阶段 5 现在已经不缺“条款效果”提示，缺的是“何时允许升级”的保守闸门。

本文件的作用就是先把这个闸门冻结下来，确保下一轮把 `ClauseVerdict / ClausePromotionReadiness` 接进代码时：

- 有严格定义
- 有样本预期
- 有保守边界
- 有从阶段 5 进入阶段 6 的最小升级条件
