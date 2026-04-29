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

- `2026-04-28`

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

- `codex/work-variable-section-proof-20260428`

说明：

- 当前将其视为“唯一活跃正式工作分支”
- 主题聚焦为：变截面 `H / BOX` 的 proof 收敛与家族映射边界
- 其它历史实验分支仅保留为参考，不再并行开发

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

### 主题：变截面 `H / BOX` 的 proof 收敛与家族映射边界
### 主题：变截面 `H / BOX` 的粗分类收敛

选择这个主题的原因：

1. 能直接检验上游轴线修正是否真实传导到粗分类层
2. 能同时覆盖折线/变截面/多切片组织关系
3. 当前只要求粗分类正确，不再让 proof / 最终判定干扰诊断

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

---

## 当前下一步

1. 以 [run_body_bracket_real_12_box_extended_loop_v4](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_12_box_extended_loop_v4>) 作为新的 `BOX` 粗分类基线。
2. 下一轮优先做：
   - 用更多 `BOX / 非 BOX` 样本验证这套 beveled wall-midline 闭环证据的普适性
   - 特别复核它不会把 `HXZ` 一类“边条围边但不是真闭环”的样本误判成 `BOX`
3. 旧派生字段运行时依赖这一轮已先完成两刀：
   - `EndProximity` 不再参与 `assemblySpan` 选源层的旧字段读取
   - coarse observation 不再借 `ImportSynthesisKind / SourceMemberMainClassCode` 放宽 `BOX / H` 门槛
4. 下一轮旧派生字段清理重点转到：
   - 只允许继续做：
     - 主体候选层旧字段残余排查
     - 粗分类层旧字段残余排查
   - 当前最新重点已收窄为：
     - 复核主体候选层是否还有隐藏的旧字段/旧先验过强借力
     - 特别继续盯：
       - `EstimateBodyAxis(...)`
       - `EstimateBodyAxisSegments(...)`
       - `EstimateAssemblySpan(...)`
       中 `inputMainPart / SemanticRole` 是否仍过强
     - 当前已完成前两块的第一轮收口：
       - `EstimateBodyAxisSegments(...)`
       - `EstimateBodyAxis(...)`
       - `EstimateAssemblySpan(...)`
     - 主体候选层当前第一轮几何优先化已基本完成
     - 下一轮优先转为：
       - 用真实样本回归验证这轮候选层收口是否稳定
      - coarse 层如无新增回灌口子，则转为回归验证，不再继续内部拆分
   - 家族映射层 / source collector 层清理暂时停止，只保留记账
5. `H` 方向当前先暂停继续扩规则，只保留已验证的两条收口：
   - 方向无关的 `H` 组织关系
   - body-like `SpecialShape` 候选纳入
6. 主体候选层已完成两条共性前提修正：
   - guide 候选统一回到 `PolyBeam + Beam`
   - `SpecialShape` 长向主板不再被一刀切排除出 `BodyCandidate`
7. 当前 `H` 正向对照样本已基本由纯粗拓扑站稳；下一步应先观察更大样本面，再决定是否还要继续动更下游层。

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
