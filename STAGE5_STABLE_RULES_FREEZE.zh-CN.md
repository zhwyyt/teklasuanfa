# 阶段 5 稳定规则冻结表

## 目的

本文档用于把阶段 5 当前已经验证过、可反复回归、且不应再依赖当前线程记忆维持的稳定规则集中冻结下来。

它不替代：

- [MAIN_BODY_REBUILD_STATUS.md](</I:/autoteklasuanfa/MAIN_BODY_REBUILD_STATUS.md>)
- [MAIN_BODY_REBUILD_TASKLIST.zh-CN.md](</I:/autoteklasuanfa/MAIN_BODY_REBUILD_TASKLIST.zh-CN.md>)
- [DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md>)

它只回答三件事：

1. 阶段 5 目前哪些规则已经算“稳定”
2. 每条稳定规则最后一次是在哪个真实 full-run 工件上确认的
3. 哪些主桶已经收过一轮，哪些仍未收口

---

## 使用方式

后续若出现以下情况，应先看本表：

- 想确认某个小类是否已经有稳定 proof-chain
- 想确认某条 verdict gate 是否已经正式冻结
- 想判断某个家族主桶是否已经“至少收过一轮”

若本表与状态板冲突，以状态板和任务清单为准。

---

## 已冻结稳定规则

### GKZ

- 大类语义：`BOX`
- 当前稳定结论：
  - `BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE`
  - `ClauseVerdict = Satisfied`
  - `PromotionReadiness = ReadyForPromotion`
- 当前稳定说明：
  - 这是阶段 5 最成熟的正向满足样本之一
  - 代表“闭合/对壁稳定条款成立，但路径可能被改写”
- 最近确认工件：
  - `run_body_bracket_real_04_definition_clause_v58`
  - `run_body_bracket_real_04_definition_clause_v59`
  - `run_body_bracket_real_04_definition_clause_v60`
- 当前状态：`已冻结`

### HXZ

- 大类语义：`单主板持续性`
- 当前稳定结论：
  - `PRIMARY_PLATE_CONTINUITY_CLAUSE`
  - `ClauseVerdict = Satisfied` 或窄 broken 个例
  - `PromotionReadiness = ReadyForPromotion`
- 当前稳定说明：
  - 这是阶段 5 最成熟的正向满足样本之一
  - 代表“主板持续性条款成立，但路径可能被改写”
- 最近确认工件：
  - `run_body_bracket_real_04_definition_clause_v58`
  - `run_body_bracket_real_04_definition_clause_v59`
  - `run_body_bracket_real_04_definition_clause_v60`
- 当前状态：`已冻结`

### MJ / MQMJ

- 大类语义：`单主板 + 附属/锚筋系统`
- 当前稳定结论：
  - proof-chain bootstrap 已接回主链
  - `SINGLE_PRIMARY_PLATE_PROOF`
  - `SINGLE_PRIMARY_PLATE_SYSTEM_TARGET`
  - `PRIMARY_PLATE_CONTINUITY_CLAUSE`
- 当前稳定说明：
  - `MJ / MQMJ` 已经收过一轮，不再属于“proof-chain 还没起”的主桶
  - 后续问题主要不是 proof-chain 能否挂起，而是：
    - 哪些 assembly 已足够进入 `Broken / ReviewRequired / ReadyForPromotion`
    - 哪些仍停在下游 gate
- 最近确认工件：
  - `run_body_bracket_real_04_definition_clause_v53`
  - `run_body_bracket_real_04_definition_clause_v49`
  - `run_body_bracket_real_04_definition_clause_v50`
- 当前状态：`已收过一轮，但 verdict 主桶未封板`

### YPGL

- 大类语义：`BOX` 大类下的闭合箱壁壳及其异形子类
- 当前稳定结论：
  - proof-chain bootstrap 已接回主链
  - `PAIRED_WALL_MAIN_CONTOUR_PROOF`
  - `BOX_WALL_PAIR_PROOF_TARGET`
  - `BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE`
- 当前稳定说明：
  - `YPGL` 已经收过一轮，不再属于“闭合箱壳 proof-chain 起不来”的主桶
  - 其中：
    - `T3-2YPGL-12 / 17 / 23` 已收进闭合箱壁壳主链
    - `T3-2YPGL-5` 已收进“闭合箱壳 + 折板翼缘”子类
  - 后续问题主要转为：
    - 何时进入 `Broken / Satisfied / ReviewRequired`
    - 而不是继续补 bootstrap
- 最近确认工件：
  - `run_body_bracket_real_04_definition_clause_v53`
  - `run_body_bracket_real_04_definition_clause_v54`
  - `run_body_bracket_real_04_definition_clause_v49`
  - `run_body_bracket_real_04_definition_clause_v50`
- 当前状态：`已收过一轮，但 verdict 主桶未封板`

### YPGL-BOX 单件标准截面子类

- 大类语义：`YPGL` 大类下“source 已明确为标准箱型材”的单件 `BOX`
- 当前稳定结论：
  - `LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE`
  - `ClauseVerdict = Satisfied`
  - `PromotionReadiness = ReadyForPromotion`
- 当前稳定说明：
  - 这批样本不是闭合箱壳 built-up proof-chain 没起，而是：
    - source-side 早已明确是 `StandardSection / STANDARD_BOX`
    - 阶段 5/6 之前缺少单件标准箱型材的直达语义链
  - 当前该直达链只覆盖：
    - `SynthesizedBody = BOX`
    - `1` 个 core part
    - `0` 个 review part
    - 当前仍停在 `NO_CLAUSE_ROWS`
- 最近确认工件：
  - `run_body_bracket_real_04_definition_clause_v71`
- 当前状态：`已冻结`

### 普通 GL

- 大类语义：`GL` 混合主桶
- 当前稳定结论：
  - 仍按保守主线处理
  - 默认不因局部 effect 直接进入单一 `Broken / Satisfied`
- 当前稳定说明：
  - 普通 `GL` 当前仍是阶段 5 最大未收口主桶之一
  - 已确认不能把 `GL-H` 折型翼缘子类的 narrow gate 外推到全部 `GL`
- 最近确认工件：
  - `run_body_bracket_real_04_definition_clause_v48`
  - `run_body_bracket_real_04_definition_clause_v56`
  - `run_body_bracket_real_04_definition_clause_v57`
  - `run_body_bracket_real_04_definition_clause_v60`
- 当前状态：`未封板`

### GL-H 折型翼缘子类

- 大类语义：`GL` 大类下的 `H` 主家族折型翼缘子类
- 当前稳定结论：
  - proof-chain 已稳定：
    - `H_MAIN_CONTOUR_PROOF`
    - `H_WEB_FLANGE_PROOF_TARGET`
    - `H_WEB_FLANGE_CONTINUITY_CLAUSE`
  - effect 聚合已稳定：
    - `2 x H_WEB_FLANGE_CONTINUITY_BREAK_EFFECT`
    - `1 x H_WEB_FLANGE_CONTINUITY_REWRITE_EFFECT`
    - `LeadClauseEffectDirection = BrokenCandidate`
  - assembly gate 已稳定：
    - `BrokenCandidate + StableFull + B2/R1`
    - `=> Broken + ReadyForReview`
- 当前稳定说明：
  - 这是 `GL` 大类里的窄子类例外
  - 不可外推到普通 `GL`
  - 当前 `BROKEN` 来自 assembly 聚合 gate，不来自旧启发式
  - 当前已确认该子类也覆盖：
    - `T3-2GL-31` 这类“下翼缘不是单整块板，而是由折板/拼接翼缘 review 件表达”的开口 `H` 变体
    - `T3-2GL-11 / 17` 这类“纯开口 + 对称双主体板”的折板 `H` 变体
    - `T3-2GL-46` 这类“弱单站位闭环 + review 翼缘不承担 envelope support”的折板 `H` 变体
  - 对这类样本的冻结口径是：
    - 主语义仍是开口 `H`
    - 不因局部拼接翼缘表达而回退成 `BOX` 或“未知异形”
- 最近确认工件：
  - `run_body_bracket_real_04_definition_clause_v55`
  - `run_body_bracket_real_04_definition_clause_v56`
  - `run_body_bracket_real_04_definition_clause_v57`
  - `run_body_bracket_real_04_definition_clause_v58`
  - `run_body_bracket_real_04_definition_clause_v59`
  - `run_body_bracket_real_04_definition_clause_v60`
  - `run_body_bracket_real_04_definition_clause_v70`
  - `run_body_bracket_real_04_definition_clause_v73`
- 当前状态：`已冻结`

---

## 已冻结的 assembly 级窄 gate

### H 折型翼缘 broken gate

- 作用范围：
  - 仅限 `GL-H` 折型翼缘子类
- 触发条件：
  - `LeadClause = H_WEB_FLANGE_CONTINUITY_CLAUSE`
  - `LeadClauseShare = 100%`
  - `ClauseMix = SINGLE_CLAUSE`
  - `LeadClauseEffectDirection = BrokenCandidate`
  - `LeadClauseBreakEffectCount = 2`
  - `LeadClauseRewriteEffectCount = 1`
  - 且当前 assembly 原本停在 `ReviewRequired + ReadyForReview`
- 结果：
  - `LeadClauseVerdict = Broken`
  - `LeadClausePromotionReadiness = ReadyForReview`
- 最近确认工件：
  - `run_body_bracket_real_04_definition_clause_v60`
  - `run_body_bracket_real_04_definition_clause_v70`
- 当前状态：`已冻结`

---

## 当前不应重复投入的方向

以下方向当前不应再当作主线重复投入：

- 再去证明 `MJ / YPGL` 的 proof-chain 能不能挂起
- 再去证明 `GL-H` 折型翼缘子类是否仍属于 `H` 主家族
- 再去把 `GL-H` 的 effect direction 从文本里猜出来

这些都已经在真实工件层收过一轮以上。

---

## 当前仍未封板的主桶

优先级从高到低：

1. 普通 `GL`
   - 仍未形成足够窄、足够稳的 verdict gate
2. `MJ`
   - proof-chain 已收过一轮，但 verdict/readiness 还未系统收口
3. `YPGL`
   - proof-chain 已收过一轮，但 verdict/readiness 还未系统收口

---

## 收尾判据

若要认为“阶段 5 稳定规则已足够冻结，不再靠线程记忆维持”，至少还需满足：

1. 普通 `GL` 再收敛一轮 verdict 主桶
2. `MJ / YPGL` 的下游 gate 再收紧一轮
3. 每条已稳定窄 gate 都能在：
   - 状态板
   - 任务清单
   - verdict gate 文档
   - expected cases 文档
   - 本冻结表
   这五处之一被直接找到，而不是只留在聊天上下文里
