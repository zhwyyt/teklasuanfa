# 变截面 H / BOX 问题台账 V2

## 文档目的

本文档用于承接 [PROJECT_TASKLIST_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_TASKLIST_V2.zh-CN.md>) 中的任务 A：

- 问题台账重建

本文档只聚焦当前唯一主题：

- 变截面 `H / BOX` 的粗分类收敛

当前边界明确为：

- 只分析“模型导出数据 -> 主体候选 -> 截面拓扑观察 -> 粗分类”
- 暂不处理 proof 条款
- 暂不处理最终家族判定

---

## 使用规则

每个样本统一记录四件事：

1. 当前归属层级
2. 当前卡点
3. 当前证据
4. 下一步动作

层级只允许使用以下五类：

1. 输入层
2. 主体候选层
3. 截面拓扑观察层
4. proof 条款层
5. 家族映射层

当前阶段只允许把问题归到前 3 层。

---

## 分组说明

当前先按三组收口：

1. `BOX` 正向对照样本
2. 变截面 `BOX` 卡点样本
3. 变截面 `H` 卡点样本

这不是全量清单，而是当前第一批代表样本台账。后续只增量追加，不回到聊天式散落记录。

---

## A. BOX 正向对照样本

### 1. `T3-2YPGL-18`

- 当前归属层级：
  - 截面拓扑观察层已稳定
- 当前卡点：
  - 无，作为变截面 `BOX` 粗分类正向对照样本
- 当前证据：
  - `run_body_bracket_real_04_coarse_topology_rebuild_v2`
  - `SourceMemberMainClassCode = BOX`
  - `LongitudinalTypeCode = STRAIGHT`
  - `PriorityStationCount = 5`
  - `EligibleStationCount = 5`
  - `CandidatePartCount = 4`
  - `BoxStationCount = 5`
  - `ClosedLoopStationCount = 5`
  - `CoarseMainClassCode = BOX`
  - `CoarseMainClassReasonCode = MULTI_WALL_CLOSED_LOOP_CONSENSUS`
- 下一步动作：
  - 作为 `BOX` 粗分类稳定样本，后续用来对照 `YPGL-5` 这类未完全收敛样本到底差在哪一层

---

## B. 变截面 BOX 卡点样本

### 2. `T3-2YPGL-5`

- 当前归属层级：
  - 截面拓扑观察层
- 当前卡点：
  - 当前主卡点已解除
  - 该样本已回到稳定 `BOX` 粗分类
- 当前证据：
  - 旧问题阶段：
    - `run_body_bracket_real_12_orientation_h_v2`
    - `CandidatePartCount = 4`
    - `EligibleStationCount = 5`
    - `ClosedLoopStationCount = 0`
    - `CoarseMainClassReasonCode = TOPOLOGY_CONSENSUS_NOT_REACHED`
  - 最新收口阶段：
    - `run_body_bracket_real_12_box_extended_loop_v4`
    - `CandidatePartCount = 4`
    - `EligibleStationCount = 5`
    - `BoxStationCount = 5`
    - `ClosedLoopStationCount = 5`
    - `CoarseMainClassCode = BOX`
    - `CoarseMainClassSubtypeCode = VARIABLE_SECTION_BOX`
    - `CoarseMainClassReasonCode = MULTI_WALL_CLOSED_LOOP_CONSENSUS`
- 当前判断：
  - 输入层不是主矛盾
  - 候选层不是主矛盾
  - 真正 root cause 已确认在截面拓扑观察层：
    - 旧闭环证据没有把 beveled wall-midline enclosure 当成真实闭环
    - 它默认更偏向“矩形 hull 边被直接贴住”的理解
  - 当前实现收口不是简单提高全局阈值，而是：
    - 新增 extended-line loop
    - 新增端点桥接式 hull support
    - 对 wall-midline corner offset 使用局部补偿余量
- 下一步动作：
  - 保留为变截面 `BOX` 正向基线样本
  - 下一轮重点用它去对照：
    - `HXZ` 一类非真闭环样本是否仍被挡住
    - 其它变截面 `BOX` 是否也能被同一套闭环定义稳定吃进去

---

## C. 变截面 H 卡点样本

### 3. `T3-5GL-21`

- 当前归属层级：
  - 主体候选层
- 当前卡点：
  - 旧的“cleaning 后看不到 BodyCandidate”已不是当前主矛盾
  - 当前已能稳定保住 `CandidatePartCount = 3`
  - 但所有站位仍没有形成 `H`
  - 当前要改判为：已进入 trace 表达/拓扑观察层问题
- 当前证据：
  - `run_body_bracket_real_06_current_check`
  - body-family proof：
    - `BodyDescriptorFamily = UNRESOLVED`
    - `BodyDescriptorSectionType = UNRESOLVED`
    - `LongitudinalTypeCode = POLYLINE`
    - `LongitudinalSubtypeCode = NONE`
    - `LeadClauseCode = ""`
    - `LeadClauseVerdictCode = InsufficientEvidence`
    - `LeadClausePromotionReadinessCode = HoldEffectOnly`
    - `DefinitionDrivenFamilyCode = NONE`
    - `DecisionReasonCode = READINESS_NOT_PROMOTABLE`
  - section topology / cleaning：
    - `PriorityStationCount = 2`
    - `BootstrapOnly = true`
    - `CoreBodyPartIds = []`
    - `ReviewPartIds = [35349253,35349275,35349261]`
    - 5 个站位全部出现：
      - `BODY_CANDIDATE_MISSING_AFTER_CLEANING`
    - 5 个站位全部：
      - `RetainedBodyCandidateTraceCount = 0`
      - `ClosedLoopCandidate = false`
  - core-body-proof：
    - `CoreBodyPartIds = []`
    - `ReviewPartIds = [35349253,35349275,35349261]`
    - `35349261 / 35349253 / 35349275` 都只落：
      - `ProofClass = ReviewRequired`
      - `TopologyRewriteDefinitionClauseCode = NONE`
      - `Reasons = INSUFFICIENT_PERSISTENT_PROOF`
- 当前判断：
  - 现在不是“家族映射太保守”
  - 而是主体候选在 cleaning 后被整体打空，proof 根本还没进入 `H` 条款层
  - 现已进一步确认两个共性根因方向：
    - `BodyCandidatePartitioner` 的 `SpecialShape / BentPlate` 分支过窄，只允许极窄的 `PolyBeam` 例外进入 `BodyCandidate`
    - `BodyCandidatePartitioner` 自己的 guide segment 候选仍只认 `PolyBeam`，没有完全落实 `PolyBeam + Beam`
- 下一步动作：
  - 优先回到主体候选层和截面拓扑观察层
  - 重点检查：
    - 折线分段后的局部截面是否已经真正按段取正切
    - 主体候选层是否应把“长向 special-shape 主板”纳入 `BodyCandidate`
    - guide 候选是否必须统一回到 `PolyBeam + Beam`
    - 为什么 `35349275` 这种疑似腹板件在 cleaning 后仍没进稳定主体组

### 4. `T3-5GL-27`

- 当前归属层级：
  - 截面拓扑观察层
- 当前卡点：
  - 当前折线 `H` 样本在 coarse 观察层没有形成稳定共识
  - 当前阶段先不看后续 proof，只看为什么 coarse 观察层完全没收住
  - 当前最新怀疑已收敛到：不是候选层，而是单零件单 trace 的表达能力不足
- 当前证据：
  - `run_body_bracket_real_12_orientation_h_v2`
  - coarse 观察：
    - `LongitudinalTypeCode = POLYLINE`
    - `PriorityStationCount = 2`
    - `EligibleStationCount = 2`
    - `CandidatePartCount = 3`
    - `BoxStationCount = 0`
    - `HStationCount = 2`
    - `PrimaryPlateStationCount = 0`
    - `ClosedLoopStationCount = 0`
    - `CoarseMainClassCode = H`
    - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
- 当前判断：
  - 该样本已从“粗拓扑没站稳”转为“已验证站稳”的正向样本
  - 当前已证明折线 `H` 的一个 root cause 是：
    - 观察层不能把 `H` 组织关系写死在坐标朝向里
- 下一步动作：
  - 保留为折线 `H` 正向对照样本，不再作为当前主攻异常点

### 5. `T3-5GL-38`

- 当前归属层级：
  - 截面拓扑观察层
- 当前卡点：
  - 主体描述已经稳定指向 `BuiltUpH`
  - 但 coarse 观察层仍没有形成 `H` 共识
  - 当前说明即使 `BuiltUpH` 已明显暴露，纯拓扑观察层也还缺一层表达能力
- 当前证据：
  - `run_body_bracket_real_12_orientation_h_v2`
  - coarse 观察：
    - `BodyDescriptorFamily = BuiltUpH`
    - `LongitudinalTypeCode = STRAIGHT`
    - `PriorityStationCount = 5`
    - `EligibleStationCount = 5`
    - `HStationCount = 5`
    - `PrimaryPlateStationCount = 0`
    - `CoarseMainClassCode = H`
    - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
- 当前判断：
  - 该样本已从“描述层已暴露、粗分类没站稳”转为“纯粗拓扑已站稳”
  - 当前已证明恒定截面 `BuiltUpH` 也需要方向无关的 `H` 组织关系识别
- 下一步动作：
  - 保留为恒定截面 `H` 正向对照样本

---

## D. H 正向对照样本

### 6. `T3-5GL-51`

- 当前归属层级：
  - 截面拓扑观察层已稳定
- 当前卡点：
  - 无，已完成从“借力 H”到“纯粗拓扑 H”的收口验证
- 当前证据：
  - `run_body_bracket_real_12_orientation_h_v2`
  - coarse 观察：
    - `CandidatePartCount = 3`
    - `HStationCount = 5`
    - `CoarseMainClassCode = H`
    - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
    - 第三个关键主板 `35471865` 虽然在 partition 中仍是 `SpecialShape`
    - 但它具备：
      - `LongitudinalCoverageEstimate = 1`
      - `PriorityStationPresenceRatio = 1`
      - 已被纳入 body-like 粗分类候选集合
- 下一步动作：
  - 保留为“长向 `SpecialShape` 主板也应参与粗分类”的正向对照样本

### 7. `T3-5GL-78`

- 当前归属层级：
  - 截面拓扑观察层已稳定
- 当前卡点：
  - 无，已完成从“借力 H”到“纯粗拓扑 H”的收口验证
- 当前证据：
  - `run_body_bracket_real_12_orientation_h_v2`
  - coarse 观察：
    - `PriorityStationCount = 2`
    - `CandidatePartCount = 3`
    - `HStationCount = 1`
    - `CoarseMainClassCode = H`
    - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
    - 第三个关键主板 `35431907` 在 partition 中仍是 `SpecialShape`
    - 但它具备完整长向覆盖，已被纳入 body-like 候选集合
- 下一步动作：
  - 保留为“较少 priority station 也能纯拓扑站稳 H”的正向对照样本

---

## 当前阶段结论

当前第一版台账已经能把问题明显分成三类：

1. `YPGL-5`
   - 当前仍是最核心残余样本
   - 在 `run_body_bracket_real_12_orientation_h_v2` 中：
     - `CandidatePartCount = 4`
     - `EligibleStationCount = 5`
     - `ClosedLoopStationCount = 0`
     - `CoarseMainClassReasonCode = TOPOLOGY_CONSENSUS_NOT_REACHED`
   - 当前主矛盾已进一步收紧到：
     - `trace-expression`
     - `closed-loop evidence`

2. `T3-5GL-21`
   - 仍需后续回到新样本上继续核
   - 但 `GL-27 / 38 / 51 / 78` 这四条样本已经证明：
     - `H` 方向当前的共性问题并不只是在 proof
     - 主要是粗分类观察层的方向依赖和候选边界问题

3. `T3-5GL-27 / 38 / 51 / 78`
   - 这一组 `H` 样本已基本转为正向对照
   - 当前可以支持下一阶段把主攻重心收回 `BOX`

这说明后续推进顺序应该是：

1. 先攻变截面 `BOX` 的 `trace-expression / closed-loop evidence`
2. `H` 方向当前以回归验证为主，不再继续主攻扩规则
3. 当前阶段不再讨论 proof / 家族映射

---

## 下一步建议

按当前主题，下一轮建议优先做：

1. 以 `T3-2YPGL-5` 为核心拆变截面 `BOX`
2. 以 `T3-5GL-21` 与 `T3-5GL-27` 为核心拆折线 `H`
3. 继续用 `T3-2YPGL-18 / T3-5GL-51 / T3-5GL-78` 做正向对照
4. 在真正动规则前，先统一确认两条共性整改方向：
   - 主体候选层不再把长向 `SpecialShape` 主板一刀切排除
   - 粗分类观察层逐步去掉对 `directHSignalCount / directBoxSignalCount` 的运行时借力
