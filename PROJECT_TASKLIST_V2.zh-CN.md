# 项目任务板 V2

## 文档目的

本文档是项目新的轻量任务板。

它只保留当前活跃主题下真正需要推进的任务，不再承担历史长卷记录职责。

---

## 当前主题

- 基础几何与数据健康检查层

当前严格边界：

- 只做到：
  - 输入层
  - 主体候选层
  - 截面拓扑观察层
- 不再继续推进：
  - proof 条款层
  - 家族映射层

当前主题真源：

- [FOUNDATION_GEOMETRY_HEALTH_AUDIT_PLAN.zh-CN.md](</I:/autoteklasuanfa/FOUNDATION_GEOMETRY_HEALTH_AUDIT_PLAN.zh-CN.md>)

---

## 当前唯一目标

把“粗分类异常后再人工反查”的工作方式，
改成“先由基础健康检查层暴露根因，再决定是否需要动粗分类”。

---

## 当前首批落地范围

只做三类检查：

1. `AxisConsistency`
2. `CandidateSetConsistency`
3. `SampleTraceConsistency`

暂不进入：

1. `SectionFrameConsistency`
2. `TopologyInputConsistency`
3. 任何 proof / 家族映射判定逻辑

---

## 当前任务拆解

### A. 基础健康检查方案定版

目标：

- 把基础健康检查层的检查对象、invariant、输出工件与首批落地顺序固定下来

完成标准：

1. 明确这层只做 sidecar audit
2. 明确首批 3 类检查项
3. 明确 member / candidate / station / trace 四层输出对象
4. 明确首版不回灌粗分类主判定

当前状态：

- 已完成首版

当前产物：

- [FOUNDATION_GEOMETRY_HEALTH_AUDIT_PLAN.zh-CN.md](</I:/autoteklasuanfa/FOUNDATION_GEOMETRY_HEALTH_AUDIT_PLAN.zh-CN.md>)

当前下一步：

1. 开始第一轮最小实现
2. 先只落 member-level summary
3. 再按需要补 station / candidate detail

### B. 问题台账重建

目标：

- 把当前活跃异常样本按层级整理，而不是继续按聊天顺序散落推进

完成标准：

1. 每个样本必须归到以下层级之一：
   - 输入层
   - 主体候选层
   - 截面拓扑观察层
   - proof 条款层
   - 家族映射层
2. 每个样本必须有一句“当前卡点”
3. 每个样本必须有一句“下一步动作”

当前状态：

- 已完成第一版

当前产物：

- [VARIABLE_SECTION_ISSUE_LEDGER_V2.zh-CN.md](</I:/autoteklasuanfa/VARIABLE_SECTION_ISSUE_LEDGER_V2.zh-CN.md>)

当前结论：

1. `T3-2YPGL-5`
   - 当前应归到截面拓扑观察层
   - 当前已进一步收敛为：不是候选集缺失，而是 section trace 表达还不足以稳定证明 `BOX` 围合
2. `T3-5GL-21`
   - 已从“主体候选层”推进到“trace 表达层也有共性风险”
3. `T3-5GL-27 / 38`
   - 主要卡在截面拓扑观察层
4. `T3-2YPGL-18 / T3-5GL-51 / T3-5GL-78`
   - 可作为正向对照样本

### C. 第一轮基础健康检查落地

目标：

- 让当前已知典型 root cause 能先在健康检查层暴露，而不是等粗分类异常后人工反查

完成标准：

1. 能对主轴劫持给出明确 reason code
2. 能对候选集失守给出明确 reason code
3. 能对 sample-trace 失真给出明确 reason code
4. 输出独立 sidecar，不改粗分类主链

当前状态：

- 待开始

首批样本：

1. `T3-2GL-53 / T3-2GL-55`
   - 目标：验证 `SampleTraceConsistency`
2. `T2-13GL-9 / 10 / 16 / 21 / 24`
   - 目标：验证 `AxisConsistency`
3. `T2-13GL-23`
   - 目标：验证 `CandidateSetConsistency`

当前下一步：

1. 设计最小 sidecar 输出契约
2. 选定 collector / workflow 的接入位置
3. 先跑一轮 member-level summary

### D. 变截面 `BOX` 粗分类收敛

目标：

- 确认变截面箱体粗分类到底卡在：
  - priority station
  - 截面取法
  - 围合证据
  - 候选集合/拓扑共识

完成标准：

1. 至少拿一组代表样本做完整链路拆解
2. 结论要明确落在哪一层
3. 若改规则，必须是普适性规则，不允许单样本补丁

当前状态：

- 进行中

当前已定位：

1. `T3-2YPGL-5`
   - 当前主要不是最终判定
   - 输入层和候选层已确认不是主矛盾
   - `2026-04-29` 已继续完成两层收口：
     - [SectionTraceExtractor.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/SectionTraceExtractor.cs>)
       - 真实截面面与 `SolidEdges` 交线优先
       - 截面面世界坐标基点修正到真实 axis point
     - [SectionClosedLoopEvidence.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/SectionClosedLoopEvidence.cs>)
       - 新增 `HasExtendedLineLoop(...)`
       - `HasEnvelopeBoundaryLoop(...)` 新增端点桥接式支撑
       - 局部角点补偿改为 wall-midline closure allowance，不再等同于放大全局 envelope tolerance
   - 最新真实复跑 [run_body_bracket_real_12_box_extended_loop_v4](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_12_box_extended_loop_v4>) 已确认：
     - `CandidatePartCount = 4`
     - `EligibleStationCount = 5`
     - `BoxStationCount = 5`
     - `ClosedLoopStationCount = 5`
     - `CoarseMainClassCode = BOX`
     - `CoarseMainClassSubtypeCode = VARIABLE_SECTION_BOX`
     - `CoarseMainClassReasonCode = MULTI_WALL_CLOSED_LOOP_CONSENSUS`
   - 当前结论：
     - 根因不是“全局阈值差一点”
     - 而是旧闭环证据没有把 beveled wall-midline enclosure 当成真实闭环
     - 最后代码收口确实表现为角点补偿余量调整，但那是服务于新的闭环定义，不是单纯调大一个总阈值
2. 当前 `BOX` 方向下一步不该再回到 `H` 上发散，而应做：
   - 用更多 `BOX / 非 BOX` 样本验证这套闭环证据的普适性
   - 特别检查它不会把 `HXZ` 一类“边条围边但不是真闭环”的样本误吃成 `BOX`

### E. 变截面 `H` 粗分类收敛

目标：

- 拆清变截面 `H` 在当前链路里是：
  - 主体候选没立住
  - 还是截面拓扑观察没收敛

完成标准：

1. 明确候选层是否稳定
2. 明确拓扑观察为什么没形成 `H`
3. 明确哪些样本粗分类已经足够进入 `H`

当前状态：

- 已完成一轮关键收口，当前可暂时降优先级

当前已定位：

1. `T3-5GL-21`
   - 早期“主体候选层整体失守”的根因已完成一轮修正
   - 当前代表样本里已经可以保住 `CandidatePartCount = 3`
   - 但 `HStationCount = 0`
   - 当前要改判为：
     - 不再只是主体候选层问题
     - 而是已进入 section trace / topology observation 的共性表达问题
2. `T3-5GL-27 / 38`
   - 已通过 [run_body_bracket_real_12_orientation_h_v2](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_12_orientation_h_v2>) 证实：
     - `GL-27`
       - `HStationCount = 2 / EligibleStationCount = 2`
       - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
     - `GL-38`
       - `HStationCount = 5 / EligibleStationCount = 5`
       - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
   - 这说明折线/变截面 `H` 的一个共性 root cause 已被确认并收住：
     - 观察层不能把 `H` 组织关系写死成固定朝向
3. `GL-51 / 78`
   - 已继续通过同一次复跑确认：
     - 只要把长向 `SpecialShape` 主板纳入粗分类候选集合
     - 这两条样本也能从纯粗拓扑进入 `H`
   - 当前说明第二个共性 root cause 也已明确：
     - 候选层不能把长向稳定出现的 `SpecialShape` 主板挡在粗分类观察外
4. `T2-13GL-9 / 10 / 16 / 21 / 24`
   - 当前已从“疑似主体候选缺失”收敛到“上游导出轴线被短腹板劫持”
   - 已确认这批样本不是截面采样看不到腹板
   - 而是导出 `GuidePolyline / AxisSegments` 本身就落在短腹板上
   - `2026-04-29` 已完成第一轮修正：
     - `BuildLongitudinalAxis(...)` 新增候选轴跨度下限
     - 并新增导出轴与 `MainAxis / 最长连续主板` 的一致性验收
   - 当前验证结论：
     - 这 `5` 个样本的复算轴长已全部回到约 `6m` 级别
     - 已不再停留在 `1m` 左右的短腹板轴
   - 当前下一步：
     - 用真实重新导出的缓存验证这两刀在正式导出链中是否同样成立
     - 再看这批 `GL` 在下游粗分类里是否自然恢复为 `H`
   - `2026-04-29` 最新验证已完成：
     - [run_body_bracket_real_14_axis_guard_v1](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_14_axis_guard_v1>) 已确认这两步都成立
     - `GL-9 / 10 / 16 / 21 / 24` 已自然恢复为 `H`
   - 当前新的下一步：
     - 收窄剩余 `GL` 异常到 `T2-13GL-23`
     - 重点不是上游轴线，而是为什么粗分类观察层仍会落到 `DIRECT_H_WITH_NARROW_CANDIDATE_SET`
   - `2026-04-29` 最新验证已完成：
     - `T2-13GL-23` 的根因已收敛到“零厚度板型 Beam 未拿到 wall/web 主板语义”
     - 修复后已在缓存副本全链路复跑中回到：
       - `CandidatePartCount = 3`
       - `HStationCount = 2`
       - `WEB_FLANGE_SECTION_CONSENSUS`
   - 当前新的下一步：
     - 评估 `DIRECT_H_WITH_NARROW_CANDIDATE_SET` 这条旧桥现在是否只剩极少数历史残留样本
     - 若样本面允许，可计划正式清理这条旧借力路径
5. `T3-2GL-53 / T3-2GL-55`
   - 当前已从“斜板 H 判据是否过窄”重新定位到“trace 整理层把平行翼板拉成了镜像斜线”
   - 已确认：
     - 原始导出 member `Samples` 里，两件的外板仍是平行翼板、腹板仍是正交连接板
     - 问题不在模型本身
   - 当前状态：
     - 已修
   - 修复口径：
     - [SectionTraceExtractor.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/SectionTraceExtractor.cs>)
       的 `TryResolveTraceFromSolidEdges(...)`
       不再用交点云最远点对直接当 trace 方向
     - 改为：
       - 结合 `PlateNormal x sectionAxisX / PlateLongDirection / PlateWidthDirection`
       - 按“沿方向跨度大、法向散布小”的带状评分恢复代表线段方向
   - 最新验证：
     - [run_body_bracket_real_16_trace_fix_check_v1](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_16_trace_fix_check_v1>)
       - `T3-2GL-53`
         - `HStationCount = 5 / EligibleStationCount = 5`
         - `CoarseMainClassCode = H`
       - `T3-2GL-55`
         - `HStationCount = 5 / EligibleStationCount = 5`
         - `CoarseMainClassCode = H`
   - 当前结论：
     - 这类样本的主矛盾不是 `H` 判据太严
     - 而是粗分类输入的 trace 方向先被整理歪了
   - 当前下一步：
     - 继续横扫 `GL / HXZ / YPGL` 中是否还有同类“交点云被最远点对带偏”的样本
     - 若有，继续按 trace 表达层根因处理，不回到放宽 `H` 判据

### F. 分支收拢决策

目标：

- 决定是否从当前活跃分支再切出一个更干净的正式工作分支

完成标准：

1. 明确“继续沿当前分支推进”或“切新工作分支推进”
2. 明确历史分支后续只作参考，不再并行开发

当前状态：

- 已完成

当前结论：

1. 已采用“切新工作分支推进”。
2. 定版基线 PR `#1` 已合并进 `main`。
3. 本地 `main` 已同步到 merge commit `aa144a1`。
4. 当前新分支为：
   - `codex/foundation-health-audit-sidecar-contract-20260429`
5. 远端已合并旧分支 `codex/work-variable-section-proof-20260428` 已删除。
6. sidecar 实现草稿仍保留在本地 stash，不作为当前契约分支的已落地实现。

### G. 旧派生字段运行时依赖排查

目标：

- 把还在运行时吃旧结论字段的口子系统按层捋清
- 区分：
  - 已退到 audit/展示
  - 仍在 bridge
  - 仍直接改写当前运行时判定

完成标准：

1. 每个旧字段都要落到具体消费点
2. 每个消费点都要标清：
   - 属于候选层 / 粗分类层 / 家族映射层 / source collector
3. 至少给出下一轮清理顺序，不再只是“发现还有引用”

当前状态：

- 第二轮进行中

当前已定位：

1. `EndProximity / NearMemberStart / NearMemberEnd`
   - 主分区判定层的 `nearStableZone` 已改成现场重算
   - `2026-04-29` 进一步确认并已修：
     - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
       两个 `EstimateAssemblySpan(...)` 重载里的 `anchoredParts`
       现也改成基于当前 `projectedInterval + usableSpan` 现场重算端部关系
   - 当前结论：
     - 这条旧字段已从主体候选层主路径再退了一步
     - 当前至少不再直接影响 `span source` 选择
   - 当前下一步：
     - 后续继续扫是否还有别的前置层残余读取点

2. `ImportSynthesisKind`
   - 当前仍在三层同时参与运行时：
     - coarse observation 的 `direct signal`
     - `BodyFamilyDefinitionEvaluator` 的 family gate
     - `DefinitionClauseDecisionFullRunSourceCollector` 的 target-selection fallback
   - 当前结论：
     - 它仍是最大的旧派生运行时字段之一
     - 且已不是“只在阶段 6 历史桥接里残留”
   - 当前下一步：
    - `2026-04-29` 已先从 coarse observation 层摘掉
    - `2026-04-29` 已继续从 family gate / source collector 摘掉第一轮旧兜底
    - `2026-04-29` 已进一步关闭 `NO_CLAUSE_ROWS + BuiltUpBox descriptor` 直接 adjudicate `BOX` 的直通口
    - 下一步转为：
       - 只继续排查它在主体候选层 / 粗分类层是否还有残余借力
       - 家族映射层剩余 bridge 暂停，不再继续动

4. coarse candidate selection 层级倒灌
   - `2026-04-29` 新确认：
     - [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
       的 `SelectCandidateParts(...)`
       原先会吃：
       - `proofParts`
       - `BodyDescriptorCorePartIds`
   - 当前结论：
     - 这不是展示字段残留
     - 而是 proof/summary 结果反向参与 coarse 候选集构造
   - 当前状态：
     - 已修
   - 当前下一步：
     - 继续扫 coarse 层是否还有同类 proof/summary 回灌口子

5. `LongitudinalTypeCode` 在 coarse 变截面小类中的摘要直达
   - `2026-04-29` 新确认并已修：
     - [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
       的 `ResolveFamilyVariability(...)`
       原先会先吃：
       - `summary.LongitudinalTypeCode == POLYLINE`
       然后直接把小类打成 `VARIABLE_SECTION_*`
   - 当前结论：
     - 这条虽不改 `BOX/H/PRIMARY_PLATE_BODY` 大类
     - 但仍属于“摘要字段替当前层先下结论”
   - 当前状态：
     - 已修
   - 当前下一步：
     - 继续确认 coarse 层剩余 summary 字段是否都只做旁路说明或属性直达，不再偷偷改当前层结论

6. `BodyDescriptorFamily` 在 coarse 层的剩余分叉
   - `2026-04-29` 新确认：
     - [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
       当前唯一剩余的 descriptor 判定分叉是：
       - `BodyDescriptorFamily = StandardSection`
       - `ATTRIBUTE_DIRECT_BYPASS`
   - 当前结论：
     - 这条符合已冻结的“属性直达分叉路线”
     - 不属于 descriptor 替粗分类层偷下 `BOX/H` 结论
   - 当前状态：
     - 已复核通过
   - 当前下一步：
     - 继续扫 coarse 层是否还有其它 descriptor/summary truth 参与真实判定
   - `2026-04-29` 当前补充：
     - 已把 [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
       的 `ResolveDecision(...)`
       从“接收整份 summary”
       收到“不再接收 summary”
     - `IsAttributeDirectBypass(...)`
       也已收到只接 `bodyDescriptorFamily`
     - 目的不是改规则，而是把 coarse 判定函数的真实依赖边界写实

7. coarse observation 输出是否回流主流程
   - `2026-04-29` 新确认：
     - [DefinitionDrivenSidecarCoordinator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionDrivenSidecarCoordinator.cs>)
       中，`CoarseMainClassObservationCollector` 的结果只进入：
       - [CoarseMainClassObservationWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationWorkflow.cs>)
       - sidecar 输出
     - 未再参与其它运行时主判定
   - 当前结论：
     - coarse 观察层当前是真正的 observation sidecar
     - 不是“看起来像 sidecar，实际上又回灌主链”
   - 当前状态：
     - 已复核通过
   - 当前下一步：
     - 当前主攻点可从 coarse 内部清理，转回主体候选层残余借力排查

8. 主体候选层 `inputMainPart / SpecialShape` 强先验收口
   - `2026-04-29` 新确认并已修：
     - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
       的 `isInputMainPart && IsPrimaryBodyRole(...)`
       原先会绕开 coverage 主门槛，直接进 `BodyCandidate`
   - 当前结论：
     - 这条属于“身份先验压过当前层几何事实”
   - 当前状态：
     - 已修
   - 当前新口径：
     - `inputMainPart + primary role`
       现在也必须先满足最小 `coverage`
     - 否则只落到 `BodyAccessoryCandidate / LocalStiffenerCandidate`

9. 主体候选层 `SpecialShape` 几何自证通道
   - `2026-04-29` 新确认并已修：
     - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
       的 `ShouldPromoteSpecialShapeToBodyCandidate(...)`
       原先即使几何上已是长向稳定主板，
       也必须先有 `inputMainPart / role / outer-side` 信号才能放行
   - 当前结论：
     - 这条会让上游 role 缺失时，当前层几何无法自证
   - 当前状态：
     - 已修
   - 当前新口径：
     - 高 coverage 的 plate-like `SpecialShape/BentPlate`
       已可仅凭当前层几何进入 `BodyCandidate`
     - role / main-part / outer-side
       只再承担低 coverage 情况下的辅助放行

10. 主体候选层主轴选择的身份优先收口
   - `2026-04-29` 新确认并已修：
     - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
       的 `EstimateBodyAxis(...)`
       原先会优先采用 `inputMainPart` 的长向
   - 当前结论：
     - 若 `inputMainPart` 本身偏短，这条会让几何骨架前提被身份先验劫持
   - 当前状态：
     - 已修
   - 当前新口径：
     - `inputMainPart` 只有在长度不明显短于整批候选时，才继续保留主轴优先权
     - 否则回到更长几何件优先

11. 主体候选层 assembly span 选源的 role 优先收口
   - `2026-04-29` 新确认并已修：
     - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
       的两个 `EstimateAssemblySpan(...)`
       原先只要 `primaryBodyParts` 非空，就直接拿它们定义整体跨度
   - 当前结论：
     - 这会让一批 role 更“像主板”但覆盖不完整的零件，劫持整体 span 分母
   - 当前状态：
     - 已修
   - 当前新口径：
     - 先比较 `primary / anchored / usable` 的真实跨度
     - 只有 primary 集合覆盖足够完整时，才允许它定义 assembly span

12. 主体候选层 guide-axis 候选排序的身份平局权收口
   - `2026-04-29` 新确认并已修：
     - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
       的 `EstimateBodyAxisSegments(...)`
       原先在：
       - `ProgressRatio`
       - `Length`
       之后，会直接用：
       - `inputMainPart`
       - `PrimaryBodyRole`
       做平局排序
   - 当前结论：
     - 若几条导向线接近，这条仍可能让偏短候选线靠身份先验翻盘
   - 当前状态：
     - 已修
   - 当前新口径：
     - 只有当该候选线长度已接近最长导向线候选时
     - `inputMainPart / role`
       才允许参与平局裁决

8. 主体候选层 `inputMainPart / SpecialShape` 强先验收口
   - `2026-04-29` 新确认并已修：
     - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
       的 `isInputMainPart && IsPrimaryBodyRole(...)`
       原先会绕开 coverage 主门槛，直接进 `BodyCandidate`
   - 当前结论：
     - 这条属于“身份先验压过当前层几何事实”
   - 当前状态：
     - 已修
   - 当前新口径：
     - `inputMainPart + primary role`
       现在也必须先满足最小 `coverage`
     - 否则只落到 `BodyAccessoryCandidate / LocalStiffenerCandidate`

9. 主体候选层 `SpecialShape` 几何自证通道
   - `2026-04-29` 新确认并已修：
     - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
       的 `ShouldPromoteSpecialShapeToBodyCandidate(...)`
       原先即使几何上已是长向稳定主板，
       也必须先有 `inputMainPart / role / outer-side` 信号才能放行
   - 当前结论：
     - 这条会让上游 role 缺失时，当前层几何无法自证
   - 当前状态：
     - 已修
   - 当前新口径：
     - 高 coverage 的 plate-like `SpecialShape/BentPlate`
       已可仅凭当前层几何进入 `BodyCandidate`
     - role / main-part / outer-side
       只再承担低覆盖情况下的辅助放行

3. `SourceMemberMainClassCode`
   - 最终家族判定虽已去依赖
   - 但 [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>) 里仍通过：
     - `directBoxSignalCount`
     - `directHSignalCount`
   - 直接放宽 `BOX / H` 的粗分类通过门槛
   - 当前结论：
     - `2026-04-29` 已从 coarse observation 的运行时通过门槛中摘除
     - 当前粗分类已回到纯候选+多切片拓扑口径
   - 当前下一步：
     - 后续仅保留 observation / audit 展示，不再重新混回判定逻辑

---

## 当前阻塞项

1. 当前还缺一层独立的基础健康检查 sidecar
2. 现在大多数 root cause 仍要靠人工顺藤摸瓜，无法先按 invariant 自动暴露
3. 如果不先补健康检查层，后续仍容易把输入失真误判成粗分类规则问题
4. `SectionFrameConsistency / TopologyInputConsistency` 还未进入第一轮实现，折线段和站位质量问题暂时仍要人工复核

---

## 当前执行规则

1. 每次只推进一个主题，不并行发散。
2. 每个异常先判层级，再决定是否动规则。
3. 不允许把上游猜类重新混入最终家族结论。
4. 不允许为了吃掉单编号样本直接补特判。

---

## 本轮推荐顺序

1. 先做 A：基础健康检查方案定版
   - 已完成
2. 再做 C：第一轮基础健康检查落地
   - 先只落 member-level summary
   - 先实现 `AxisConsistency / CandidateSetConsistency / SampleTraceConsistency`
3. `BOX / H` 当前只作为对照样本来源，不再先扩粗分类规则
4. 旧派生字段与分支收拢继续记账，但不抢当前主题

---

## 真源说明

任务推进以本文档为准。

高层约束看：

- [PROJECT_STATUS_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_STATUS_V2.zh-CN.md>)
- [PROJECT_REFOCUS_PLAN.zh-CN.md](</I:/autoteklasuanfa/PROJECT_REFOCUS_PLAN.zh-CN.md>)
- [GITHUB_SYNC_POLICY.zh-CN.md](</I:/autoteklasuanfa/GITHUB_SYNC_POLICY.zh-CN.md>)
