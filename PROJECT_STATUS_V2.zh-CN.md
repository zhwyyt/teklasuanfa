# 项目状态板 V2

## 文档目的

本文档是项目新的轻量状态板。

从现在开始，它用于回答五个问题：

1. 当前唯一目标是什么
2. 当前唯一工作分支是什么
3. 哪些基线已经冻结
4. 当前正在攻哪一个主题
5. 下一步立刻做什么

老的长状态板不再继续扩写，转为历史档案入口。

---

## 当前日期

- `2026-04-29`

## 当前唯一目标

在已确认正确的上游输入基线之上，只收敛：

- 模型导出数据
- 主体候选/cleaning
- 多切片粗分类

当前阶段先不处理：

- proof 条款收敛
- 最终家族判定
- 家族映射保守性
- 家族映射层运行时清理继续推进

## 当前唯一工作模式

- 同仓库推进
- 新控制面执行
- 单主题推进
- 先定位层级，再动规则

## 当前工作分支

- `codex/foundation-health-audit-sidecar-contract-20260429`

说明：

- 当前将其视为“唯一活跃正式工作分支”
- 主题聚焦为：基础几何健康检查 sidecar 输出契约与第一轮 member-level summary
- 其它历史实验分支仅保留为参考，不再并行开发
- 已合并分支 `codex/work-variable-section-proof-20260428` 已进入 `main`，远端分支已删除
- sidecar 实现草稿仍保留在本地 stash 中，后续只作为参考恢复

---

## 已冻结基线

### 1. 上游真实纵向中线重建已定版

冻结口径：

1. longitudinal solid-edge 构图
2. 主组件筛选
3. 平行主组件同里程取中点
4. 输出真实 member centerline

### 2. guide 候选范围修正已定版

冻结口径：

- `PolyBeam + Beam`

不再允许 `HasUsableGuideSegments(...)` 只认 `PolyBeam`。

### 3. 上游采样链必须消费重建后的轴

冻结口径：

- `DefaultSectionSampler`
- `DefaultAnomalyDetector`
- `ApproximateSectionIntersectionService`

都必须以重建后的 longitudinal axis 为准。

### 4. 围合证据不能再用纯计数近似

冻结口径：

- 边界覆盖
- 角点接触
- 横竖方向资格

### 5. 下游最终家族判定不再依赖上游 MainClass 猜类

冻结口径：

- `SourceMemberMainClassCode` 仅保留为 audit / sidecar 字段
- 最终家族归属只看下游工程定义链

---

## 已废弃口径

1. 旧启发式主体识别链回主线
2. 上游 MainClass 猜类驱动最终家族判定
3. “四边接触即闭环”
4. 针对单编号样本直接补最终规则

---

## 当前唯一主题

### 主题：基础几何与数据健康检查层
### 主题：变截面 `H / BOX` 粗分类基线之上的基础层体检

选择这个主题的原因：

1. 当前粗分类基线已经基本可用，但根因仍主要暴露在基础数据处理层
2. 最近典型问题已证明：很多“粗分类异常”真正根因是主轴、候选集、trace 表达失真
3. 需要先建立一层系统化健康检查，避免继续按样本逐个补洞

---

## 当前执行分层

所有问题统一按以下层级排查：

1. 输入层
2. 主体候选层
3. 截面拓扑观察层

当前阶段只允许推进前 3 层。

不允许再跳过层级，直接围绕最终结果补规则。

---

## 最近一次重要结论

1. 上游真实中线重建算法已确认正确。
2. `PolyBeam + Beam` guide 候选修正已确认正确。
3. 下游最终家族判定已摘除对上游 `SourceMemberMainClassCode` 的运行时依赖。
4. 当前最需要的不是继续加规则，而是先重建项目控制面。
5. 变截面 `H / BOX` 第一版问题台账已建立：
   - [VARIABLE_SECTION_ISSUE_LEDGER_V2.zh-CN.md](</I:/autoteklasuanfa/VARIABLE_SECTION_ISSUE_LEDGER_V2.zh-CN.md>)
6. 当前执行范围已收窄为：
   - 只做“导出数据 -> 粗分类”
   - 暂不处理 proof / 最终判定
7. 已确认当前粗分类链仍存在两个上游共性风险：
   - `BodyCandidatePartitioner` 在 `SpecialShape / BentPlate` 分支里仍过窄，很多长向主板会被直接打成 `SpecialShape`，后续粗分类观察层就看不到主体候选
   - `BodyCandidatePartitioner` 自己的 guide segment 候选仍只认 `PolyBeam`，尚未完全落实已冻结的 `PolyBeam + Beam` 口径
8. 已确认当前粗分类观察层仍残留一条 `H` 的直接信号借力路径：
   - `DIRECT_H_WITH_NARROW_CANDIDATE_SET`
   - 这条路径仍使用 `directHSignalCount`
   - 后续若继续收敛粗分类，应优先改成纯组织关系判定
9. `2026-04-28` 已在 [SectionTraceExtractor.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/SectionTraceExtractor.cs>) 完成两轮几何收紧验证：
   - trace 方向已优先改成“板面与当前截面面的交线方向”
   - trace 中心已改成“当前站位截面与板面交线在截面内的位置”，不再直接拿零件质心代打
   - 已对 `run_body_bracket_real_12` 复跑：
     - [run_body_bracket_real_12_plane_intersection_v1](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_12_plane_intersection_v1>)
     - [run_body_bracket_real_12_plane_intersection_v2](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_12_plane_intersection_v2>)
   - 当前结论不是“阈值还差一点”，而是：
     - `T3-2YPGL-5` 候选集稳定是 `4` 块主板，但粗分类仍未形成 `BOX`
     - `T3-5GL-51 / 78` 仍只能靠 `DIRECT_H_WITH_NARROW_CANDIDATE_SET` 站稳 `H`
   - 当前更深一层 root cause 已收敛到：`SectionTraceExtractor` 仍是“一块零件只输出一条 trace”，对折板/复合壁板/变截面板的组织关系表达能力不足
10. `2026-04-28` 已在粗分类观察层完成一轮“方向无关 H 组织关系 + body-like special-shape 候选纳入”的收口验证：
   - 当前 [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>) 已新增：
     - 不再依赖截面坐标朝向的 `H` 粗拓扑识别
     - 从“横两条 + 竖一条”升级为“两个平行外侧主板 + 一个中间连接主板”
   - 同时已把粗分类候选集合从：
     - `BodyCandidate only`
     - 收到：
       - `BodyCandidate + 长向稳定出现的 SpecialShape`
   - 已对真实 [run_body_bracket_real_12_orientation_h_v2](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_12_orientation_h_v2>) 复跑确认：
     - `T3-5GL-27`
       - `CandidatePartCount = 3`
       - `HStationCount = 2 / EligibleStationCount = 2`
       - `CoarseMainClassCode = H`
       - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
     - `T3-5GL-38`
       - `CandidatePartCount = 3`
       - `HStationCount = 5 / EligibleStationCount = 5`
       - `CoarseMainClassCode = H`
       - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
     - `T3-5GL-51`
       - `CandidatePartCount = 3`
       - `HStationCount = 5 / EligibleStationCount = 5`
       - 已不再依赖 `DIRECT_H_WITH_NARROW_CANDIDATE_SET`
     - `T3-5GL-78`
       - `CandidatePartCount = 3`
       - `HStationCount = 1 / EligibleStationCount = 2`
       - 已由纯拓扑共识进入 `H`
   - 当前这说明：
     - 变截面 / 折线 `H` 的一个共性 root cause 确实不是 proof，而是：
       - 观察层把 `H` 组织关系写死在坐标朝向里
       - 候选层没有把长向 `SpecialShape` 主板纳入粗分类观察
   - 当前剩余最集中的阻塞样本已进一步收缩到：
11. `2026-04-30` 已在主体候选层完成一轮“宽收长向结构板、强排明显小板”的收口验证：
   - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
     已新增普通 plate-like 件的几何自证通道：
     - 高 coverage 长向板
     - 厚板 / outer-side / 长宽比信号
     - 不再要求上游必须先给出 `web/wall` role
   - 已对真实 [new-input-layer-draft-run18-batch-wide-candidate](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-run18-batch-wide-candidate>) 复跑确认：
     - `T3-3GL-1`
       - `CandidatePartCount = 4`
       - `CandidatePartIds = 58667669,58667675,58667681,58668517`
       - `HStationCount = 7 / EligibleStationCount = 7`
       - `CoarseMainClassCode = H`
       - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
     - `foundation-geometry-health-audit`
       已从：
       - `MAIN_PLATE_MISSING_FROM_CANDIDATE_SET`
       收到：
       - `CANDIDATE_SET_OK`
   - 当前结论：
     - 对变截面 `H`，候选层应优先宽收“长向结构板”
     - 真正要强排除的是明显短小、端部连接、局部加劲件
     - `T3-2YPGL-5`
     - 它在 `run_body_bracket_real_12_orientation_h_v2` 中仍为：
       - `CandidatePartCount = 4`
       - `EligibleStationCount = 5`
       - `ClosedLoopStationCount = 0`
       - `CoarseMainClassReasonCode = TOPOLOGY_CONSENSUS_NOT_REACHED`
     - 说明下一步主攻点应重新回到 `BOX` 的 trace-expression / closed-loop evidence
11. `2026-04-29` 已在 [SectionClosedLoopEvidence.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/SectionClosedLoopEvidence.cs>) 完成一轮“真实斜边/倒角围合”闭环证据收口：
   - 新增 `HasExtendedLineLoop(...)`
   - `HasEnvelopeBoundaryLoop(...)` 新增“端点桥接式 hull side support”
   - 角点补偿不再死绑全局 envelope tolerance，而是按 wall-midline 闭合局部容差处理
   - 已对真实 [run_body_bracket_real_12_box_extended_loop_v4](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_12_box_extended_loop_v4>) 复跑确认：
     - `T3-2YPGL-5`
       - `CandidatePartCount = 4`
       - `EligibleStationCount = 5`
       - `BoxStationCount = 5`
       - `ClosedLoopStationCount = 5`
       - `CoarseMainClassCode = BOX`
       - `CoarseMainClassSubtypeCode = VARIABLE_SECTION_BOX`
       - `CoarseMainClassReasonCode = MULTI_WALL_CLOSED_LOOP_CONSENSUS`
     - `T3-5GL-27 / 38 / 51 / 78`
       - 仍保持 `CoarseMainClassCode = H`
       - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
   - 当前这说明：
     - `YPGL-5` 的主矛盾确实在 `closed-loop evidence`
     - root cause 不是站位丢失，也不是候选集失守，而是旧闭环证据对 beveled wall-midline enclosure 过窄
12. `2026-04-29` 已对 `run_body_bracket_real_13` 中 `T2-13GL-9 / 10 / 16 / 21 / 24` 五个漏判 `H` 样本完成一轮上游轴线核查：
   - 已确认这 `5` 个样本的导出 `AxisSegmentsLength` 只有约 `958 ~ 1016`
   - 但同一构件的 `Member.MainAxis.Length` 与长向主板 `AxisProjection.Length` 都在约 `6145 ~ 6342`
   - 已确认导出 `GuidePolyline` 两端点都稳定落在一块短腹板内：
     - `269930574 / 269947569 / 269872009 / 269897131 / 269967560`
   - 当前更精确的 root cause 不是“直接取了焊接主零件”，而是：
     - 上游 `BuildLongitudinalAxis(...)` 的 guide 候选排序，被短而直、局部更干净的腹板 guide 劫持
     - 导出层于是把这块短腹板当成了整根构件的 longitudinal axis
   - 随后 `LongitudinalAxisResolver` 又全链路优先信任 `AxisSegments`
   - 于是 part coverage / near-start / near-end / station frame 全部被带偏
   - `2026-04-29` 已按用户确认的“两刀”口径完成上游第一轮修正，且明确未走“整构件级主轴重建”：
     - 在 [Tekla2017MemberExtractor.cs](</I:/xingcaisuanfa/TeklaSectionClassifier/Tekla2017MemberExtractor.cs>) 的 `BuildLongitudinalAxis(...)` 中新增 `HasSufficientAxisSpan(...)`
       - guide 候选排序不再只看“直不直”
       - 同时要求候选至少覆盖足够长的 member 主跨度
     - 同处新增 `IsAxisConsistent(...)`
       - 若导出轴长同时显著短于 `Member.MainAxis.Length`
       - 且显著短于最长连续主板 `AxisProjection.Length`
       - 则该导出轴直接判无效，回退到 `MAIN_AXIS_CONSISTENCY_FALLBACK`
   - 已在编译后的上游 DLL 上直接反射复算这 `5` 个样本确认：
     - `GL-9`：`963.763 -> 6009.718`
     - `GL-10`：`1016.428 -> 6205.668`
     - `GL-16`：`998.131 -> 6139.165`
     - `GL-21`：`958.241 -> 5982.409`
     - `GL-24`：`991.572 -> 6109.208`
   - 当前说明：
     - 这批 `GL` 的主轴已从短腹板局部 guide 上退下来
     - 且这一步是在上游导出层收口，不是靠下游阈值防守
13. `2026-04-29` 已用新导出缓存 [run_body_bracket_real_14](</I:/xingcaisuanfa/cache/run_body_bracket_real_14>) 完成一轮真实全链路验证：
   - 下游输出目录：
     - [run_body_bracket_real_14_axis_guard_v1](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_14_axis_guard_v1>)
   - 新导出原始 member JSON 已确认：
     - `T2-13GL-9 / 10 / 16 / 21 / 24` 的 `AxisSegmentsLength` 已全部回到约 `6m`
     - 不再出现上一轮 `~1m` 短腹板轴
   - 粗分类结果：
     - `T2-13GL-9`：`H / CONST_SECTION_H / WEB_FLANGE_SECTION_CONSENSUS`
     - `T2-13GL-10`：`H / CONST_SECTION_H / WEB_FLANGE_SECTION_CONSENSUS`
     - `T2-13GL-16`：`H / CONST_SECTION_H / WEB_FLANGE_SECTION_CONSENSUS`
     - `T2-13GL-21`：`H / CONST_SECTION_H / WEB_FLANGE_SECTION_CONSENSUS`
     - `T2-13GL-24`：`H / VARIABLE_SECTION_H / WEB_FLANGE_SECTION_CONSENSUS`
   - 当前残余点：
     - `T2-13GL-23` 虽已回到 `H`
     - 但原因仍是 `DIRECT_H_WITH_NARROW_CANDIDATE_SET`
     - 说明上游主轴问题已收住，但 `H` 观察层仍残留一条旧的直接信号借力路径
   - 另一个非本轮主线样本：
     - `13HXL-1` 当前仍为 `TOPOLOGY_CONSENSUS_NOT_REACHED`
     - 可后续单独作为单板类/非 H 对照样本再看
14. `2026-04-29` 已继续对 `T2-13GL-23` 完成一轮根因修复验证：
   - 已确认它并不是 trace 层看不见腹板
   - 而是中间腹板 `269947855` 在上游角色评分里因为 `Thickness = 0`
     - 未触发 `wall_candidate`
     - 落成 `Role = other / default`
     - 随后在候选层只被分到 `BodyAccessoryCandidate`
   - 当前已做两处普适性修正：
     - [Tekla2017MemberExtractor.cs](</I:/xingcaisuanfa/TeklaSectionClassifier/Tekla2017MemberExtractor.cs>)
       - `ReadThickness(...)` 不再只认 `ContourPlate`
       - `PL*` 板型 `Beam` 现可从 `ProfileString` 解析厚度
     - [FirstPassPartRoleScorer.cs](</I:/xingcaisuanfa/TeklaSectionClassifier/FirstPassPartRoleScorer.cs>)
       - 当 `part.Thickness == 0` 时
       - 现会回退使用 `ProfileString` 解析有效厚度参与 `wall_candidate` / `flange_candidate` 判别
   - 已用编译后的上游 DLL 直接验证：
     - `269947855` 已从 `other/default` 变成 `wall_candidate`
   - 并在缓存副本
     [run_body_bracket_real_14_reclass_gl23_v1](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_14_reclass_gl23_v1>)
     对下游复跑确认：
     - `CandidatePartCount = 3`
     - `CandidatePartIds = 269947836,269947855,269947890`
     - `HStationCount = 2`
     - `CoarseMainClassReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
   - 当前说明：
     - `GL-23` 已脱离 `DIRECT_H_WITH_NARROW_CANDIDATE_SET`
     - 这次修复的是“零厚度板型 Beam 的主板语义识别”这一层共性问题
15. `2026-04-29` 已继续完成“旧派生字段运行时依赖”第二轮排查收口：
   - 当前真源文档：
     - [OLD_DERIVED_RUNTIME_DEPENDENCY_AUDIT.zh-CN.md](</I:/autoteklasuanfa/OLD_DERIVED_RUNTIME_DEPENDENCY_AUDIT.zh-CN.md>)
   - 已进一步确认：
     - `ImportSynthesisKind`
       - 仍同时参与：
         - coarse observation 的 `direct signal`
         - 家族映射 bridge gate
         - full-run source collector 的 target-selection fallback
       - 不是“仅展示字段”
     - `SourceMemberMainClassCode`
       - 最终家族判定虽已去依赖
       - 但 coarse observation 里仍通过 `directBoxSignalCount / directHSignalCount` 直接放宽粗分类通过门槛
     - `EndProximity / NearMemberStart / NearMemberEnd`
       - 不仅历史上污染过 `nearStableZone`
       - 当前还在 [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>) 的两个 `EstimateAssemblySpan(...)` 重载里，经 `anchoredParts = !IsSingleEndedLocalPart(part)` 间接参与跨度源选择
   - 当前说明：
     - 这轮排查已经证明：
       - `EndProximity` 还不是“只剩展示残留”
       - `SourceMemberMainClassCode / ImportSynthesisKind` 也还没有完全退到 audit
     - 后续若继续做“导出数据 -> 候选 -> 粗分类”的纯化，优先顺序应是：
       1. 先切掉 `EndProximity` 在 `assemblySpan` 选源层的残余依赖
       2. 再切 `ImportSynthesisKind / SourceMemberMainClassCode` 在 coarse observation 的 direct signal
16. `2026-04-29` 已把上条顺序真正落实到代码：
   - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
     - 两个 `EstimateAssemblySpan(...)` 重载里的 `anchoredParts`
       已改为基于当前 `projectedInterval + usableSpan` 现场重算端部关系
     - 不再经 `IsSingleEndedLocalPart(part)` 读取 importer 带入的旧 `NearMemberStart / NearMemberEnd`
   - [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
     - 已删除粗分类运行时对：
       - `ImportSynthesisKind`
       - `SourceMemberMainClassCode`
       的 `directBoxSignalCount / directHSignalCount` 借力
     - `BOX / H` 当前只允许依赖：
       - 主板候选集合
       - 多切片站位比例
       - 闭环/腹板翼缘组织关系
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
   - 结果：
     - `0` warning
     - `0` error
17. `2026-04-29` 已继续把 `ImportSynthesisKind` 从家族映射 / source collector 再退一层：
   - [BodyFamilyDefinitionEvaluator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs>)
     - 已把多处：
       - `ImportSynthesisKind || BodyDescriptorFamily`
     - 收到：
       - `BodyDescriptorFamily`
     - 也就是：
       - `H / BOX` 的 family gate 不再因为 importer 曾经猜过 `H / BOX` 就放行
       - 只认当前 descriptor 与 clause/readiness 自身是否站住
     - 同时 `NO_CLAUSE_ROWS` 下的 `BOX` descriptor bridge
       已去掉 `ImportSynthesisKind = BOX` 的附加要求
   - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
     - `STANDARD_ROD + BOX` 的排除条件已改成只看当前 `BuiltUpBox` descriptor
     - 并删除了两条：
       - `CoreBodyPartIds > 1 && ReviewPartIds = 0 && ImportSynthesisKind 非空`
       - `CoreBodyPartIds = 0 && ReviewPartIds = 0 && ImportSynthesisKind 非空`
       的 target fallback 旧兜底
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
   - 结果：
     - `0` warning
     - `0` error
18. `2026-04-29` 已继续关闭一个仍过宽的 descriptor 直通口：
   - [BodyFamilyDefinitionEvaluator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs>)
     - 已删除：
       - `ShouldPromoteSemanticBoxWithoutClause(...)`
     - 也就是先前这条：
       - `NO_CLAUSE_ROWS`
       - `LeadClause = NONE`
       - `BuiltUpBox / BUILTUP_BOX_VARIANT`
       就可直接 adjudicate 成 `BOX`
       的旁路已经关闭
   - 当前说明：
     - descriptor 现在仍可作为 bridge / hint
     - 但在“没有主条款”时，不能再直接替 proof 给出最终 `BOX`
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
   - 结果：
     - `0` warning
     - `0` error
19. `2026-04-29` 边界重新收回确认：
   - 后续当前活跃推进面重新严格限定为：
     - 导出数据
     - 主体候选
     - 粗分类
   - 本轮已经触及的：
     - [BodyFamilyDefinitionEvaluator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs>)
     - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
     的家族映射层清理
     当前统一改判为：
     - 已记账的下游清理尝试
     - 暂不继续扩展
     - 后续没有用户明确指令，不再沿该层继续推进
20. `2026-04-29` 已继续按新边界只扫“主体候选层 / 粗分类层”旧字段残余：
   - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
     - 当前确认：
       - `NearMemberStart / NearMemberEnd`
         已不再参与 `nearStableZone` 与 `assemblySpan` 选源
       - 剩余 `SemanticRole / SemanticRoleScore`
         当前仍按阶段 2 合法外部先验处理，不改判为旧结论残留
   - [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
     - 新确认并已修：
       - `SelectCandidateParts(...)` 原先会吃：
         - `proofParts`
         - `BodyDescriptorCorePartIds`
       - 这会形成 proof/summary 对 coarse candidate selection 的反向倒灌
       - 当前已改为只基于：
         - `PartitionClass`
         - `LongitudinalCoverageEstimate`
         选择 coarse candidate parts
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
   - 结果：
     - `0` warning
     - `0` error
21. `2026-04-29` 已继续收掉当前边界内两处“摘要/旧字段残余”：
   - [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
     - `ResolveFamilyVariability(...)` 已删除
       - `LongitudinalTypeCode = POLYLINE => 直接判变化截面`
       这条摘要直达
     - 当前 `VARIABLE_SECTION_*` 只允许来自：
       - 多切片站位自身的 `SpanY / SpanZ / part-count / segment-count` 变化
   - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
     - 已删除不再被主路径调用的
       `IsSingleEndedLocalPart(PartFeature part)`
     - 避免未来误把 importer 带入的旧 `NearMemberStart / NearMemberEnd`
       再次接回主体候选判定
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
   - 结果：
     - `0` warning
     - `0` error
22. `2026-04-29` 已继续复核 coarse 层 `BodyDescriptor*` 残余依赖边界：
   - [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
     当前确认：
     - `BodyDescriptorFamily / BodyDescriptorSectionType`
       在 coarse 层已不再参与：
       - `BOX / H / PRIMARY_PLATE_BODY` 主判定
       - `VARIABLE_SECTION_*` 小类判定
     - 唯一剩余判定用途是：
       - `BodyDescriptorFamily = StandardSection`
       - 走 `ATTRIBUTE_DIRECT_BYPASS`
   - 当前结论：
     - 这条属于已冻结的“属性直达分叉路线”
     - 不是 proof/summary 对粗分类结果的回灌
     - 因此当前边界内无需继续改动该分支
   - 同时已在代码层继续把依赖边界写实：
     - [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
       的 `ResolveDecision(...)`
       已不再接收整份 `BodyMaterialSummary`
     - `IsAttributeDirectBypass(...)`
       也已从“吃 summary 对象”
       收到“只吃 `bodyDescriptorFamily` 单字段”
   - 当前最新重点收窄为：
     - 继续扫 coarse 层是否还有别的 summary/descriptor truth 借力口子
     - 若没有，则当前边界内的 coarse 判定主链可视为基本纯化完成
23. `2026-04-29` 已继续确认 coarse observation 的输出消费边界：
   - [DefinitionDrivenSidecarCoordinator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionDrivenSidecarCoordinator.cs>)
     当前确认：
     - `CoarseMainClassObservationCollector.Collect(artifacts)`
       的结果只流向：
       - [CoarseMainClassObservationWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationWorkflow.cs>)
       - sidecar 工件导出
     - 未再回流参与：
       - `BodyFamilyDefinitionEvaluator`
       - `BodyProfileResolver`
       - 其它运行时主判定链
   - 当前结论：
     - coarse 观察层不仅内部主判定链已基本纯化
     - 其输出结果本身当前也仅作 observation/sidecar，不再反向改写主流程
   - 当前判断：
     - 在“导出数据 -> 主体候选 -> 粗分类”这条活跃边界内，
       coarse 主链已可暂时判定为“基本纯化完成”
24. `2026-04-29` 已回到主体候选层继续收“先验压过几何”的两处强口子：
   - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
     - `isInputMainPart && IsPrimaryBodyRole(...)`
       已不再无条件直通 `BodyCandidate`
     - 当前改为：
       - `coverage >= 0.20` 才进 `BodyCandidate`
       - 否则根据 `nearStableZone` 回落到
         `BodyAccessoryCandidate / LocalStiffenerCandidate`
   - 同文件 `ShouldPromoteSpecialShapeToBodyCandidate(...)`
     - 已改为：
       - 只要 `SpecialShape/BentPlate` 本身
         `plate-like + non-tiny + in main component + near-stable + coverage >= 0.55`
       - 即可仅凭当前层几何进入 `BodyCandidate`
     - 不再要求必须先有：
       - `inputMainPart`
       - `PrimaryBodyRole`
       - `OuterSideCandidate`
       这类上游信号才能放行高覆盖主板
   - 当前结论：
     - 主体候选层已把两条最强的“身份直通”改成“几何优先、先验降阈值辅助”
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
   - 结果：
     - `0` warning
     - `0` error
25. `2026-04-29` 已继续把主体候选层的“几何骨架前提”从身份优先收到覆盖优先：
   - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
     - `EstimateBodyAxis(...)`
       当前已不再只要 `inputMainPart` 有可用方向就直接优先拿走主轴
     - 新口径为：
       - `inputMainPart` 只有在自身长度不明显短于整批非 tiny 候选时
       - 才可继续作为主轴优先来源
       - 否则回到“更长几何件优先，role 只做次级排序”
   - 同文件：
     - `EstimateAssemblySpan(...)` 两个重载
       当前已不再只要存在 `primary role` 集合，就直接把它作为整体跨度源
     - 新增 `SelectSpanSourceParts(...)`
       改成先比较：
       - `primaryBodyParts` 的真实跨度
       - 与 `anchored / usable` 集合的参考跨度
       - 只有覆盖跨度足够时，`primary role` 集合才有资格定义整体 assembly span
   - 当前结论：
     - 主体候选层现在不仅分类分支更偏几何优先
     - 连主轴与跨度这类上游几何前提，也开始摆脱 `inputMainPart / SemanticRole` 的过强牵引
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
   - 结果：
     - `0` warning
     - `0` error
26. `2026-04-29` 已继续收 `EstimateBodyAxisSegments(...)` 的导向线候选排序先验：
   - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
     - 当前排序主链仍保持：
       - `ProgressRatio`
       - `Length`
     - 但 `inputMainPart / PrimaryBodyRole`
       已进一步收窄为：
       - 只有当该候选线长度本身已接近最长导向线候选时
       - 才允许参与平局裁决
   - 当前结论：
     - 偏短导向线不再能靠身份/role 在接近场景里翻盘
     - 导向线候选排序这层也已收到“几何主导、先验只做近似平局裁决”
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
   - 结果：
     - `0` warning
     - `0` error
27. `2026-04-29` 已确认 `T3-2GL-53 / T3-2GL-55` 的 `H` 漏判根因在 `SectionTraceExtractor` 的截面线段整理，而不是模型真实不平行：
   - 已核对原始导出
     [member_T3-2GL-53.json](</I:/xingcaisuanfa/cache/run_body_bracket_real_16/members/member_T3-2GL-53.json>)
     与
     [member_T3-2GL-55.json](</I:/xingcaisuanfa/cache/run_body_bracket_real_16/members/member_T3-2GL-55.json>)
     中的 `Samples`
     - 上游样本截面里两块外板仍是标准平行翼板
     - 中间腹板仍是正交连接板
   - 已确认旧
     [SectionTraceExtractor.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/SectionTraceExtractor.cs>)
     在 `TryResolveTraceFromSolidEdges(...)` 里，是用交点云的“最远点对”直接还原代表线段
     - 对矩形/斜边/端部倒角板件，这会把真实平行翼板误拉成对角线或镜像斜线
   - 当前已改为：
     - 交点云先结合板件自身可解释的截面方向候选
       - `PlateNormal x sectionAxisX`
       - `PlateLongDirection`
       - `PlateWidthDirection`
     - 再按“沿方向跨度大、法向散布小”的带状评分选择代表方向
     - 不再让单纯最远点对主导 trace 方向
   - 已对真实
     [run_body_bracket_real_16_trace_fix_check_v1](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_16_trace_fix_check_v1>)
     复跑确认：
     - `T3-2GL-53`
       - 两块外板 trace 已从镜像斜线回到平行表达
       - `HStationCount = 5 / EligibleStationCount = 5`
       - `CoarseMainClassCode = H`
     - `T3-2GL-55`
       - 两块外板 trace 已从镜像斜线回到平行表达
       - `HStationCount = 5 / EligibleStationCount = 5`
       - `CoarseMainClassCode = H`
   - 当前说明：
     - 这两个样本此前的主矛盾不是 `H` 规则门槛过严
     - 而是粗分类输入的 `section trace` 被整理歪了
     - trace 层回正后，现有 `H` 粗拓扑规则即可自然通过
28. `2026-04-29` 当前活跃主题已正式切换：
   - 不再继续以“新样本异常 -> 直接改粗分类规则”为主节奏
   - 改为先建立基础健康检查层
   - 新方案真源：
     - [FOUNDATION_GEOMETRY_HEALTH_AUDIT_PLAN.zh-CN.md](</I:/autoteklasuanfa/FOUNDATION_GEOMETRY_HEALTH_AUDIT_PLAN.zh-CN.md>)
   - 当前明确：
     - 这层只覆盖输入层 / 主体候选层 / 截面拓扑观察层
     - 只做 sidecar audit，不回灌粗分类主判定
   - 第一批检查项固定为：
     - `AxisConsistency`
     - `CandidateSetConsistency`
     - `SampleTraceConsistency`
   - 目的不是替当前粗分类再加一套隐式判定器，
     而是让后续异常样本先在基础层暴露根因码
29. `2026-04-29` 已完成定版基线 PR 后的分支收拢：
   - GitHub PR `#1` 已合并进 `main`
   - 本地 `main` 已同步到 merge commit `aa144a1`
   - 已从最新 `main` 新建当前工作分支：
     - `codex/foundation-health-audit-sidecar-contract-20260429`
   - 远端已合并旧分支：
     - `codex/work-variable-section-proof-20260428`
     已删除
   - 本地 sidecar 实现草稿暂存保留：
     - `stash@{1}`：`hold foundation health audit sidecar implementation draft before baseline PR`
   - 生成物暂存保留：
     - `stash@{0}`：`hold generated pycache before sidecar branch sync`
30. `2026-04-30` 已把 V2 当前主题从“只停在输出契约”推进到“collector / workflow 已接线可落盘”：
   - 已确认输出契约草稿
     [FOUNDATION_GEOMETRY_HEALTH_AUDIT_OUTPUT_CONTRACT.zh-CN.md](</I:/autoteklasuanfa/FOUNDATION_GEOMETRY_HEALTH_AUDIT_OUTPUT_CONTRACT.zh-CN.md>)
     可直接作为首版 sidecar 契约
   - App 层已新增并接入：
     - [FoundationGeometryHealthAuditModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/FoundationGeometryHealthAuditModels.cs>)
     - [FoundationGeometryHealthAuditCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/FoundationGeometryHealthAuditCollector.cs>)
     - [FoundationGeometryHealthAuditArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/FoundationGeometryHealthAuditArtifactBuilder.cs>)
     - [FoundationGeometryHealthAuditSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/FoundationGeometryHealthAuditSerializer.cs>)
     - [FoundationGeometryHealthAuditWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/FoundationGeometryHealthAuditWorkflow.cs>)
   - [DefinitionDrivenSidecarCoordinator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionDrivenSidecarCoordinator.cs>)
     当前已并列输出 `foundation-geometry-health-audit.json/.zh-CN.md`
   - 首版当前只启用：
     - `AxisConsistency`
     - `CandidateSetConsistency`
     - `SampleTraceConsistency`
   - 并按契约保留：
     - `SectionFrameConsistency = NOT_EVALUATED`
     - `TopologyInputConsistency = NOT_EVALUATED`
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
     - `dotnet run --project I:\autoteklasuanfa\src\TeklaBodyBracketRecognition.App\TeklaBodyBracketRecognition.App.csproj -- I:\xingcaisuanfa\cache\run_body_bracket_real_14\members\member_T2-13GL-9.json I:\autoteklasuanfa\.tmpresults\foundation-health-audit-smoke-20260430-gl9`
   - 结果：
     - `0` warning
     - `0` error
     - 输出目录
       [foundation-health-audit-smoke-20260430-gl9](</I:/autoteklasuanfa/.tmpresults/foundation-health-audit-smoke-20260430-gl9>)
       已真实落出：
       - `foundation-geometry-health-audit.json`
       - `foundation-geometry-health-audit.zh-CN.md`
     - `T2-13GL-9` 在新导出缓存下当前健康检查结果为：
       - `OverallHealthStatus = PASS`
       - `AxisConsistency = PASS`
       - `CandidateSetConsistency = PASS`
       - `SampleTraceConsistency = PASS`
31. `2026-04-30` 已把“新输入层”从口头方向收口成独立执行清单：
   - 新增
     [NEW_INPUT_LAYER_EXECUTION_CHECKLIST.zh-CN.md](</I:/autoteklasuanfa/NEW_INPUT_LAYER_EXECUTION_CHECKLIST.zh-CN.md>)
   - 当前定位明确为：
     - `V3` 候选 / 预研执行面
     - 暂不替代当前 V2 活跃真源
   - 当前结论固定为：
     - 先做新输入层输出契约与最小归一化原型
     - 先与当前输入链 A/B 对照
     - 待切换门槛冻结后，再决定是否正式升格为 `V3`
32. `2026-04-30` 已继续完成“新输入层”阶段 1 的第一版契约草案：
   - 新增
     [NEW_INPUT_LAYER_OUTPUT_CONTRACT_DRAFT.zh-CN.md](</I:/autoteklasuanfa/NEW_INPUT_LAYER_OUTPUT_CONTRACT_DRAFT.zh-CN.md>)
   - 当前草案已明确：
     - 顶层对象：
       - `Member`
       - `NormalizedLongitudinalPath`
       - `NormalizedPart`
       - `StationQueryContext`
     - 字段必须分层：
       - `RawFact`
       - `NormalizedRepresentation`
       - `DerivedAid`
     - 当前缓存中的：
       - `AxisSegments / GuidePolyline / MainAxis / BoundingBox / SolidEdges`
         保持 `RawFact`
       - `PartRoles / MainClass / OuterSideCandidate / EndProximity`
         只允许作为 `Hint`
     - 当前阶段禁止：
       - 让新输入层直接回灌 `BOX / H / family` 主判定
   - 当前结论：
     - 可以开始进入“最小归一化原型”阶段
     - 但仍应保持并行输出，不替换默认输入链
33. `2026-04-30` 已继续完成“新输入层”阶段 2 的最小归一化原型接线：
   - App 层已新增并接入：
     - [NewInputLayerDraftModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftModels.cs>)
     - [NewInputLayerDraftCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftCollector.cs>)
     - [NewInputLayerDraftArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftArtifactBuilder.cs>)
     - [NewInputLayerDraftSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftSerializer.cs>)
     - [NewInputLayerDraftWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftWorkflow.cs>)
   - 并已接入：
     - [DefinitionDrivenSidecarCoordinator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionDrivenSidecarCoordinator.cs>)
     - [DefinitionDrivenSidecarCoordinatorResult.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionDrivenSidecarCoordinatorResult.cs>)
   - 当前首版原型只并行输出：
     - `NormalizedLongitudinalPath`
     - `NormalizedPart`
     - `StationQueryContext`
   - 已执行：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
     - `dotnet run --project I:\autoteklasuanfa\src\TeklaBodyBracketRecognition.App\TeklaBodyBracketRecognition.App.csproj -- I:\xingcaisuanfa\cache\run_body_bracket_real_14\members\member_T2-13GL-9.json I:\autoteklasuanfa\.tmpresults\new-input-layer-draft-smoke-20260430-gl9`
   - 结果：
     - `0` warning
     - `0` error
     - 输出目录
       [new-input-layer-draft-smoke-20260430-gl9](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-smoke-20260430-gl9>)
       已真实落出：
       - `new-input-layer-draft.json`
       - `new-input-layer-draft.zh-CN.md`
   - 当前首个 smoke 结论：
     - `T2-13GL-9` 的路径当前已稳定落成：
       - `PathModelType = RAW_AXIS_SEGMENTS`
     - 当前 `18` 个零件中：
       - `12` 个已落为 `PLATE_BOUNDARY_THICKNESS`
       - `6` 个已落为 `PATH_SECTION_SWEEP`
     - 同时也已明确暴露当前缓存边界：
     - 多个板件仍报 `BOUNDARY_UNRESOLVED`
     - 多个零件仍报 `THICKNESS_INFERRED_FROM_SIZE`
     - 这说明新输入层原型已经开始把“当前缓存还缺哪些强语义信息”显式暴露出来
34. `2026-04-30` 已对新输入层原型继续补三条代表样本 smoke，对照当前输入层缺口的暴露方式：
   - 输出目录：
     - [new-input-layer-draft-smoke-20260430-gl23](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-smoke-20260430-gl23>)
     - [new-input-layer-draft-smoke-20260430-gl53](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-smoke-20260430-gl53>)
     - [new-input-layer-draft-smoke-20260430-gl55](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-smoke-20260430-gl55>)
   - 当前早期观察：
     - `T2-13GL-23`
       - `PathModelType = RAW_AXIS_SEGMENTS`
       - `PartModelTypeBreakdown = PLATE_BOUNDARY_THICKNESS 14 / PATH_SECTION_SWEEP 5`
       - `WarningBreakdown = BOUNDARY_UNRESOLVED 6 / THICKNESS_INFERRED_FROM_SIZE 6`
     - `T3-2GL-53`
       - `PathModelType = RAW_AXIS_SEGMENTS`
       - `PartModelTypeBreakdown = PLATE_BOUNDARY_THICKNESS 70 / PATH_SECTION_SWEEP 4`
       - `WarningBreakdown = BOUNDARY_UNRESOLVED 5 / THICKNESS_INFERRED_FROM_SIZE 3`
     - `T3-2GL-55`
       - `PathModelType = RAW_AXIS_SEGMENTS`
       - `PartModelTypeBreakdown = PLATE_BOUNDARY_THICKNESS 25 / PATH_SECTION_SWEEP 2`
       - `WarningBreakdown = BOUNDARY_UNRESOLVED 3`
   - 当前结论：
     - 这批代表样本在“路径层”当前都能稳定落到 `RAW_AXIS_SEGMENTS`
     - 新输入层原型当前真正暴露出的短板，不是在 path，而是在：
       - 板件 boundary 仍未被可靠参数化
       - 一部分零件 thickness 仍只能靠尺寸推断
     - 也就是：
     - 新原型已经把当前输入层的主要缺口收窄到 `plate boundary / thickness semantics`
     - 而不是继续停留在“路径到底从哪来”的混沌状态
35. `2026-04-30` 已在原始导出插件 [I:\xingcaisuanfa](</I:/xingcaisuanfa>) 上完成“旧导出 + 新输入层侧车导出”并行接线：
   - 仍保留原有：
     - `member_*.json`
     - `batch_summary.json`
   - 同次导出新增：
     - `member_*.input-layer-draft.json`
     - `input_layer_draft_summary.json`
   - 当前实现位置：
     - [JsonCacheWriter.cs](</I:/xingcaisuanfa/TeklaSectionClassifier/JsonCacheWriter.cs>)
     - [InputLayerDraftExport.cs](</I:/xingcaisuanfa/TeklaSectionClassifier/InputLayerDraftExport.cs>)
     - [Tekla2017MemberExtractor.cs](</I:/xingcaisuanfa/TeklaSectionClassifier/Tekla2017MemberExtractor.cs>)
     - [Models.cs](</I:/xingcaisuanfa/TeklaSectionClassifier/Models.cs>)
   - 当前新增侧车重点补出的上游事实：
     - `PartModelType = PLATE_BOUNDARY_THICKNESS / PATH_SECTION_SWEEP`
     - `ThicknessSource / ThicknessSourceDetail`
     - `BoundaryLoopKind / BoundarySource / BoundaryPoints`
     - 完整 `AxisProjection.Start / End / Length / CoverageRatio`
     - `WarningCodes`
   - 当前策略：
     - 只做加法，不替换旧 `member_*.json`
     - 先让旧链与新侧车长期并行，便于样本对照和后续 importer A/B 消费
   - 已验证：
     - `dotnet build I:\xingcaisuanfa\TeklaSectionClassifier.Runner\TeklaSectionClassifier.Runner.csproj`
     - `0` error
   - 当前结论：
     - 新输入层下一阶段已不必停留在“纸面契约”
     - 可以直接基于 exporter 真实吐出的 sidecar 数据，继续做 importer 对接与样本对照
36. `2026-04-30` 已继续完成 `autoteklasuanfa` 对 exporter 新侧车的 importer/collector 对接：
   - 已在：
     - [NewInputLayerDraftCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftCollector.cs>)
     - [NewInputLayerDraftModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftModels.cs>)
     - [XingcaiCacheImporter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs>)
     完成增量接线
   - 当前行为改为：
     - 优先读取同名 `member_*.input-layer-draft.json`
     - 找不到时才回退到旧 `member_*.json` 推断逻辑
   - 当前已真正贯通到 app 输出的新增字段：
     - `ThicknessSource`
     - `ThicknessSourceDetail`
     - `BoundarySource`
     - `ProjectionStart / ProjectionEnd / CoverageRatio`
     - exporter 侧 `WarningCodes`
   - 已验证：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
     - `dotnet run --project I:\autoteklasuanfa\src\TeklaBodyBracketRecognition.App\TeklaBodyBracketRecognition.App.csproj -- I:\xingcaisuanfa\cache\run_body_bracket_real_18\members\member_T3-2GL-53.json I:\autoteklasuanfa\.tmpresults\new-input-layer-draft-run18-gl53`
   - 结果：
     - `0` warning
     - `0` error
     - 输出目录：
       [new-input-layer-draft-run18-gl53](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-run18-gl53>)
   - 当前 smoke 结论：
     - app 侧 artifact 已不再只依赖旧链推断
     - `THICKNESS_INFERRED_FROM_BBOX`
       与 `BOUNDARY_UNRESOLVED`
       已能从 exporter 新侧车真实透传到最终 draft artifact
     - 新输入层下一步可以进入更稳定的 A/B 对照，而不是继续停留在“字段有没有导出来”的阶段
37. `2026-04-30` 已完成 `run_body_bracket_real_18` 的批量 A/B 首轮确认，并顺手修复目录扫描误吃 sidecar 的批处理问题：
   - 已修：
     - [XingcaiCacheImporter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs>)
       的 `ResolveMemberFiles(...)`
       现在会排除：
       - `member_*.input-layer-draft.json`
     - 避免批处理时同一个 assembly 因 sidecar/legacy 双读而重复进入 pipeline
   - 已验证：
     - `dotnet run --project I:\autoteklasuanfa\src\TeklaBodyBracketRecognition.App\TeklaBodyBracketRecognition.App.csproj -- I:\xingcaisuanfa\cache\run_body_bracket_real_18\members I:\autoteklasuanfa\.tmpresults\new-input-layer-draft-run18-batch`
   - 结果：
     - 已完成 `14` 个 assembly 的离线分析
     - 输出目录：
       [new-input-layer-draft-run18-batch](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-run18-batch>)
   - 当前 A/B 收敛结论非常明确：
     - `BOUNDARY_UNRESOLVED = 203`
     - 其中：
       - `Beam = 185`
       - `PolyBeam = 18`
       - 其它 `PartType = 0`
     - 说明 exporter 新侧车上线后，剩余边界缺口已高度集中到：
       - `Beam / PolyBeam`
   - 当前结论：
     - plate-like `ContourPlate/BentPlate` 的边界输出已基本够用
     - 下一轮 exporter 工作不该再平均撒网
     - 应明确收窄为：
       - `Beam / PolyBeam` 板件边界参数化
38. `2026-04-30` 已把“每次跑完批量数据自动生成中文 Excel 结果表”接入离线主流程：
   - 已新增：
     - [CoarseClassificationExcelExporter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseClassificationExcelExporter.cs>)
   - 并已接入：
     - [OfflineRecognitionBatchRunner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/OfflineRecognitionBatchRunner.cs>)
   - 当前行为：
     - 每次 `OfflineRecognitionBatchRunner` 完成后
     - 都会在输出目录自动生成：
       - `粗分类结果.xlsx`
   - 当前 Excel 固定包含：
     - `粗分类结果表`
     - `边界缺口汇总`
     - `边界缺口明细`
   - 当前表头已统一改为中文优先，便于直接人工核对：
     - `来源主类中文`
     - `当前粗分类中文`
     - `是否与来源一致`
     - `判定原因中文`
     - `边界缺口数`
     - `主要缺口类型`
   - 已验证：
     - `dotnet run --project I:\autoteklasuanfa\src\TeklaBodyBracketRecognition.App\TeklaBodyBracketRecognition.App.csproj -- I:\xingcaisuanfa\cache\run_body_bracket_real_18\members I:\autoteklasuanfa\.tmpresults\new-input-layer-draft-run18-batch-autoexcel`
   - 输出目录已真实落出：
       [粗分类结果.xlsx](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-run18-batch-autoexcel/粗分类结果.xlsx>)
39. `2026-04-30` 已把 `T3-2YPGL-5` 的“延长线可围合但侧壁未进外圈支持集”根因收口到拓扑/闭环衔接层：
   - 已确认此前真正把 `YPGL-5` 判坏的不是候选集缺失，也不是 sample-level 截面不闭合，而是：
     - `SectionTopologyAnalyzer`
       先按 `TouchesEnvelope(...)` 只把“直接碰到端点包络”的 trace 认作 `OuterEnvelopeTraceIds`
     - `SectionClosedLoopEvidence.HasTrueClosedLoop(...)`
       又只拿这批 `envelopeSegments` 去跑 `HasExtendedLineLoop(...)`
     - 于是 `YPGL-5` 两块侧壁虽然在“代表线延长后”可以与上下板形成真实围合，
       但因为端点本身略缩进，仍被长期留在 `InternalTraceIds`
   - 当前已在：
     - [SectionClosedLoopEvidence.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/SectionClosedLoopEvidence.cs>)
     - [SectionTopologyAnalyzer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/SectionTopologyAnalyzer.cs>)
     完成收口：
     - `HasTrueClosedLoop(...)` 不再在 `envelopeSegments < 4` 时直接失败
     - 当前会在 body-candidate 全集上继续尝试 `SupportsExtendedLineLoop(...)`
     - 拓扑层若检测到 body-candidate 全集可形成 extended-line loop，
       会把这批支持线作为真实外圈支持集，而不再硬锁死在端点包络触边集合
   - 已对真实
     [new-input-layer-draft-run18-extended-loop-support-v1](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-run18-extended-loop-support-v1>)
     复跑确认：
     - `T3-2YPGL-5`
       - `CandidatePartCount = 4`
       - `BoxStationCount = 5 / EligibleStationCount = 5`
       - `ClosedLoopStationCount = 5`
       - `CoarseMainClassCode = BOX`
       - `CoarseMainClassReasonCode = MULTI_WALL_CLOSED_LOOP_CONSENSUS`
     - 同批对照：
       - `T3-1HXZ-8` 仍保持 `TOPOLOGY_CONSENSUS_NOT_REACHED`
       - `T3-3GL-1` 仍保持 `H + WEB_FLANGE_SECTION_CONSENSUS`
   - 当前结论：
     - 这次修的是“围合支持集如何从代表线恢复”这条更深的共性口子
     - 它能覆盖一类“端点包络看起来未闭、但代表线延长后真实闭合”的异形箱体
     - 但还不能承诺“所有异形截面”都已一次性彻底正确表达，后续仍需继续用更多 `BOX / 非 BOX` 样本压回归面
40. `2026-04-30` 已把“路径+截面 / 边界+厚度 / 退化近似 / 表达等级 / 失真风险”正式落到新输入层工件：
   - 已在：
     - [NewInputLayerDraftModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftModels.cs>)
     - [NewInputLayerDraftCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftCollector.cs>)
     - [NewInputLayerDraftArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/NewInputLayerDraftArtifactBuilder.cs>)
     - [CoarseClassificationExcelExporter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseClassificationExcelExporter.cs>)
     完成收口
   - 当前行为改为：
     - 不再只输出 `PartModelType + WarningCodes`
     - 每个零件都会显式给出：
       - `RepresentationKind`
       - `RepresentationLevel`
       - `DistortionRiskCode`
       - `DistortionRiskReasons`
       - `DegradationReasonCodes`
   - 当前首版口径：
     - 能形成真实 `path + section` 的，落 `PATH_SECTION_SWEEP`
     - 能形成真实 `boundary + thickness` 的，落 `PLATE_BOUNDARY_THICKNESS`
     - 只有边界/截面拿不全时，才显式落：
       - `APPROX_PATH_SECTION_SWEEP`
       - `APPROX_BOUNDARY_THICKNESS`
       - `TRACE_ONLY_FALLBACK`
   - 已对真实
     [new-input-layer-draft-run18-representation-v1](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-run18-representation-v1>)
     复跑确认：
     - `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
       - `0` warning
       - `0` error
     - `run_body_bracket_real_18` 批量输出中：
       - `RepresentationKindBreakdown`
         - `PLATE_BOUNDARY_THICKNESS = 382`
         - `APPROX_BOUNDARY_THICKNESS = 203`
         - `PATH_SECTION_SWEEP = 67`
       - `RepresentationLevelBreakdown`
         - `EXACT = 449`
         - `APPROXIMATE = 203`
       - `DistortionRiskBreakdown`
         - `LOW = 449`
         - `HIGH = 203`
     - `T3-2YPGL-5` 当前已能直接区分：
       - `ContourPlate` 主板：
         - `边界+厚度 / 精确表达 / 低风险`
       - `Beam / PolyBeam` 且边界仍未补齐的板件：
         - `近似边界+厚度 / 高风险`
   - 当前 Excel 也已同步补强：
     - 主表新增：
       - `精确/近精确数`
       - `近似/退化数`
       - `高风险数`
     - 新增工作表：
       - `表达等级汇总`
   - 当前结论：
     - “失真”不再只是藏在 warning 里
     - 已变成可直接核对的主字段
   - 下一步真正该补的是 exporter 侧 `Beam / PolyBeam` richer boundary/section 语义，而不是让下游继续隐式猜
41. `2026-04-30` 已把 `T3-1HXZ-8` 这类“典型钢板 + 窄边板/包边板”从粗分类空档里收进 `PRIMARY_PLATE_BODY`：
   - 已确认该样本不是输入层问题：
     - `foundation-geometry-health-audit = PASS`
     - `CandidatePartCount = 5`
     - `PriorityStationCount = 5`
     - 轴线 / 候选集 / trace 都已通过
   - 真正根因是：
     - [CoarseMainClassObservationCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs>)
       旧逻辑只允许：
       - `BOX`
       - `H`
       - `candidatePartCount <= 2` 的 `PRIMARY_PLATE_BODY`
     - 像 `T3-1HXZ-8` 这种：
       - `1` 块绝对主导大板
       - `+ 4` 块窄边板/包边板
       - 虽然工程语义上明显仍是单主板体
       - 但因为 `candidatePartCount = 5`
         会长期落到 `TOPOLOGY_CONSENSUS_NOT_REACHED`
   - 当前已新增保守口径：
     - 当多数切片满足：
       - 存在 `1` 块绝对主导的大板
       - 其余板件宽度显著更小
       - 且大多附着在主板端部/边部
     - 允许仍判为：
       - `PRIMARY_PLATE_BODY`
       - `ReasonCode = DOMINANT_PRIMARY_PLATE_WITH_EDGE_RETURNS`
   - 已对真实
     [new-input-layer-draft-run18-hxz8-primaryplate-v1](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-run18-hxz8-primaryplate-v1>)
     复跑确认：
     - `T3-1HXZ-8`
       - `PrimaryPlateStationCount = 5`
       - `PrimaryPlateStationRatio = 1.0`
       - `CoarseMainClassCode = PRIMARY_PLATE_BODY`
       - `CoarseMainClassSubtypeCode = CONST_PRIMARY_PLATE`
       - `CoarseMainClassReasonCode = DOMINANT_PRIMARY_PLATE_WITH_EDGE_RETURNS`
   - 当前说明：
     - 这步修的是“单主板 + 窄边板”这一类典型钢板体的粗分类表达缺口
     - 不是把 `HXZ` 一揽子抬成单板
     - 也没有放宽 `BOX / H` 判据
42. `2026-04-30` 已继续把“单主板 + 多块边缘附属板”的粗分类口径从样本特征收口到更抽象的结构条件，并在 `run20` 上完成整组验证：
   - 已确认上一轮 `DOMINANT_PRIMARY_PLATE_WITH_EDGE_RETURNS`
     的真正窄口子不是“小板数量”，而是两个过强假设：
     - 主导板占全部 trace 宽度比例必须 `>= 0.68`
     - 次级板件默认应与主导板方向近似平行
   - `T3-1HXZ-1 / 5 / 6 / 9`
     这几条在 `run20` 上已证明：
     - 候选集仍是 `5` 件
     - 轴线 / 候选 / trace 均正常
     - 真实失败点只是：
       - `DominantShare ≈ 0.663`
       - 且主导板是斜放代表线
       - 之前被过窄阈值卡在外面
   - 同时也已确认：
     - 若只简单下调占比阈值
     - 会把 `T3-2MJ-13`
       这类“主板 + 少量边板”的非目标样本误吞进来
   - 当前已把口径收到：
     - 存在 `1` 块主导板
     - 次级板件宽度显著更小
     - 次级板件在主导板两端/边缘形成附着
     - 且必须有至少 `3` 块独立次级边缘附属板
   - 已对真实
     [new-input-layer-draft-run20-primaryplate-v3](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-run20-primaryplate-v3>)
     复跑确认：
     - `T3-1HXZ-1 / 2 / 3 / 5 / 6 / 7 / 8 / 9`
       已全部回到：
       - `PRIMARY_PLATE_BODY`
       - 其中 `1 / 2 / 3 / 5 / 6 / 7 / 8 / 9`
         为 `DOMINANT_PRIMARY_PLATE_WITH_EDGE_RETURNS`
       - `4`
         仍为 `SINGLE_PLATE_STATION_MAJORITY`
     - `T3-2MJ-13`
       仍保持：
       - `TOPOLOGY_CONSENSUS_NOT_REACHED`
       - 未被该规则误吞
   - 当前说明：
     - 这一步已经把“单主板 + 多块边缘附属板”从具体样本数目抽象成了更稳定的组织关系
     - 后续若再扩样本，优先验证：
       - `3` 块边板
       - 非对称边板
       - 更短主板
       这些变体是否仍能稳定命中

---

## 当前下一步

1. 以 [PROJECT_TASKLIST_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_TASKLIST_V2.zh-CN.md>) 作为当前执行顺序真源。
2. 新输入层当前已具备 exporter 侧真实并行输出，不再只停留在 app 内 draft 原型。
3. 下一步优先补 exporter 侧 richer 语义，而不是再扩 importer 猜测：
   - 重点只收窄到：
     - `Beam / PolyBeam`
   - 优先新增或补强：
     - `RepresentationKind / RepresentationLevel / DistortionRiskCode`
     - 可直接支撑 `path + section` 或 `boundary + thickness` 的显式字段
4. 第一轮 exporter 代表样本继续固定为：
   - `T3-1GKZ-6`
   - `T3-5GKZ-5`
   - `T3-2GKL-6`
   - `T3-2YPGL-9`
5. 当前阶段继续冻结：
   - 不直接替换旧输入链
   - 不回到 proof / 家族映射层
   - 不把 audit 结果回灌成新的粗分类借力

---

## 真源说明

当前新的执行真源按优先级为：

1. [PROJECT_STATUS_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_STATUS_V2.zh-CN.md>)
2. [PROJECT_TASKLIST_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_TASKLIST_V2.zh-CN.md>)
3. [PROJECT_REFOCUS_PLAN.zh-CN.md](</I:/autoteklasuanfa/PROJECT_REFOCUS_PLAN.zh-CN.md>)
4. [GITHUB_SYNC_POLICY.zh-CN.md](</I:/autoteklasuanfa/GITHUB_SYNC_POLICY.zh-CN.md>)

以下文档改为历史档案，不再作为活跃执行面：

1. [MAIN_BODY_REBUILD_STATUS.md](</I:/autoteklasuanfa/MAIN_BODY_REBUILD_STATUS.md>)
2. [MAIN_BODY_REBUILD_TASKLIST.zh-CN.md](</I:/autoteklasuanfa/MAIN_BODY_REBUILD_TASKLIST.zh-CN.md>)
