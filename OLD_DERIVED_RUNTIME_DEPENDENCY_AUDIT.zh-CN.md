# 旧派生字段运行时依赖排查表

> 日期：`2026-04-29`
>
> 目的：
>
> - 系统盘点当前项目里“旧流程先算好的结论字段”还有哪些仍在运行时被消费
> - 区分：
>   - `审计/展示用途`
>   - `运行时判定用途`
> - 为后续“去旧派生字段运行时依赖”提供统一真源

---

## 判定口径

- `旧派生字段`：
  不是当前层现场重算出的几何/拓扑事实，而是上游或旧流程先计算好的标签、提示、主类、端部关系、导入重建类型等。
- `运行时依赖`：
  当前流程在真正决定：
  - 候选集
  - 粗分类
  - 家族归属
  - 代表零件选择
  - proof/source 关键落点
  时，会直接读取该字段。
- `审计用途`：
  仅用于 sidecar、对账、说明、展示、统计，不参与当前运行时判定。

---

## 总表

| 字段 | 来源 | 当前主要导入点 | 当前运行时用途 | 风险等级 | 当前判断 |
| --- | --- | --- | --- | --- | --- |
| `EndProximity / NearMemberStart / NearMemberEnd` | 上游导出缓存的端部接近判断 | [XingcaiCacheImporter.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs:504) | 仍进入 `PartInput` / `PartFeature`；[BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:70) 的 `nearStableZone` 已改为现场重算，但 `EstimateAssemblySpan(...)` 两个重载里的 `anchoredParts` 仍经 [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:323) / [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:373) 间接读取旧值 | 高 | **仍有残余运行时依赖**。主分区判定已切开，但 assembly span 选源仍会被旧端部字段影响 |
| `SourceMemberMainClassCode` | 上游 `Classification.MainClass` | [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:134) | 当前用于 [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:74) 构造 `directCode`，并继续参与 `directBoxSignalCount / directHSignalCount` 的粗分类阈值判定 | 中 | **主家族判定已去依赖，但粗分类观察层仍不是纯拓扑口径** |
| `ImportSynthesisKind` | importer 导入阶段的虚拟主体重建类型 | `BodyMaterialSummary` / `PipelineArtifact` / `FullRunSource` | 仍参与 [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:244) 多条家族映射分支；也参与 [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:79) direct signal 计数；[DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:1199) 一带还用于 standard-section target 选择兜底 | 高 | **仍是当前最大的旧派生运行时依赖之一，且已跨粗分类/家族映射/source collector 三层** |
| `PartRoles -> SemanticRole / SemanticRoleScore` | 上游 first-pass part role scorer 的角色结论 | [XingcaiCacheImporter.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs:502) | 直接驱动 [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:160) 的主体候选分层；也参与 `SourceBodySeedPart` 收集 | 高 | **仍是阶段 2 的核心运行时输入** |
| `BodyDescriptorFamily / BodyDescriptorSectionType` | 当前 `BodyMaterialSupport` 基于 source semantic + proof 摘要生成的主体描述字段 | `BodyMaterialSummary` | 参与 [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:245)、[CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:618) | 中 | **不是旧启发式字段，但仍是“摘要派生字段”参与运行时判定，需要注意边界** |

---

## 逐项分析

### 1. `EndProximity / NearMemberStart / NearMemberEnd`

#### 导入点

- [XingcaiCacheImporter.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs:504)
- [XingcaiCacheImporter.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs:505)

当前 importer 仍会把缓存里的：

- `part.EndProximity.NearStart`
- `part.EndProximity.NearEnd`

直接写进 `PartInput`。

#### 当前运行时消费

- [PartFeatureExtractor.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/PartFeatureExtractor.cs:66)
- [PartFeatureExtractor.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/PartFeatureExtractor.cs:67)
- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:323)
- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:373)

此前 [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs) 会直接用：

- `part.NearMemberStart`
- `part.NearMemberEnd`

判 `IsSingleEndedLocalPart(...)`。

当前确认有两层消费要分开看：

1. 主分区判定层  
   `PartitionPart(...)` 里的 `nearStableZone`
2. assembly span 选源层  
   `EstimateAssemblySpan(...)` 里的 `anchoredParts`

#### 当前状态

- `2026-04-29` 已修主分区层：
  [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:70)
  现已改为：
  - 先用当前 provisional axis 重算 `projectedInterval`
  - 再现场推导 `EstimateEndProximity(...)`
  - 再判 `nearStableZone`

- `2026-04-29` 新补充确认：
  `EstimateAssemblySpan(...)` 两个重载里仍有：
  - [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:323)
  - [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:373)

  这里的：

  - `part.PartId == inputMainPartId || !IsSingleEndedLocalPart(part)`

  仍会经 `IsSingleEndedLocalPart(part)` 间接读取 importer 带进来的旧 `NearMemberStart / NearMemberEnd`。

#### 风险判断

- 这是已经被证实出过问题的旧派生字段。
- `T2-13GL-20` 就是典型案例：
  - `coverage / projectedInterval` 是新口径
  - `EndProximity` 是旧口径
  - 同层混用后造成长腹板掉成附件候选
- 当前新发现说明：
  - 这条旧字段并不是只剩“展示残留”
  - 它还会影响 `assemblySpan` 的跨度来源筛选
  - 也就是会继续影响后续 `coverage` 的分母和整体候选判定稳定性

#### 后续建议

- 分区主判定里已经部分去依赖，但不能算“这条字段已清完”。
- 下一步应优先改成：
  - `anchoredParts` 也基于当前 `projectedInterval + assemblySpan` 现场重算端部关系
  - 不再经 `IsSingleEndedLocalPart(part)` 读取旧输入字段
- 之后再继续扫：
  - 是否还有别的运行时逻辑仍直接读取 `NearMemberStart / NearMemberEnd`
  - 若只是遗留字段，再进一步降级为审计用途

---

### 2. `SourceMemberMainClassCode`

#### 来源

- 上游缓存 `Classification.MainClass`
- 当前解析点：
  [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:134)

#### 当前运行时消费

- [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:74)
- [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:423)
- [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:440)
- [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:458)

当前用途是：

- 构造 `directCode`
- 转成 `directBoxSignalCount / directHSignalCount`
- 再直接参与：
  - `BOX` 的低闭环比例放宽分支
  - `BOX` 的 `CLOSED_LOOP_DIRECT_SIGNAL_CONSENSUS`
  - `H` 的低站位比例放宽分支

#### 当前状态

- 阶段 6 主家族判定对 `SourceMemberMainClassCode` 的依赖此前已经正式摘除。
- 但 coarse observation 层仍保留 direct signal 概念。
- `DIRECT_H_WITH_NARROW_CANDIDATE_SET` 已删除，不再允许靠 `Source H` 直接抬成 H。

#### 风险判断

- 风险比 `EndProximity` 小，但它不是“只做展示”。
- 当前 residual risk 在于：
  - `directBoxSignalCount`
  - `directHSignalCount`
  仍会直接改写粗分类的通过门槛。
- 换句话说：
  - 它已经不影响最终家族判定
  - 但仍影响“粗分类是否能较低证据通过”

#### 后续建议

- 如果粗分类目标继续收纯工程拓扑口径，
  则下一轮应评估：
  - 是否把 `SourceMemberMainClassCode` 完全降为 observation/audit
  - 不再参与 coarse decision 的 direct signal 计数

---

### 3. `ImportSynthesisKind`

#### 来源

- importer 离线导入阶段的重建类型
- 典型值：
  - `H`
  - `BOX`

#### 当前运行时消费点

1. [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:244)
2. [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:259)
3. [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:271)
4. [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:286)
5. [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:302)
6. [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:481)
7. [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:79)
8. [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:83)
9. [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:1199)

#### 当前角色

它已经不只是展示字段，而是还在承担三种运行时作用：

1. 家族映射分支兜底/提升
   - `ShouldPromoteGenericHFromPrimaryPlate(...)`
   - `ShouldPromoteSemanticBoxFromBodyPlate(...)`
   - `ShouldPromoteSemanticBoxWithoutClause(...)`
   - `ShouldPromoteReviewReadyGenericH(...)`
   - `LooksLikeStableHFamily(...)`

2. coarse observation 的 direct signal
   - `ImportSynthesisKind = H/BOX`
   - 被算进 `directHSignalCount / directBoxSignalCount`

3. full-run source collector 的 target 选择辅助
   - 尤其在 `STANDARD_ROD + BOX` 这一类组合里
   - 以及 “proof core/review 都空时是否允许 fallback 选目标 part” 这一类兜底分支

#### 进一步分级

结合当前代码，`ImportSynthesisKind` 的消费点可以先分成三档：

1. `应优先清理`
   - [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:79)
   - [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:83)
   - 原因：
     - 当前粗分类主题已经明确要求“纯组织关系/多切片拓扑”
     - 这里再把 importer 结论混回 `direct signal`，方向上就是逆行

2. `桥接保留，但要继续缩`
   - [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:244)
   - [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:259)
   - [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:286)
   - [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:302)
   - 原因：
     - 这些分支通常都已经和 `LeadClause / Verdict / Readiness / BodyDescriptor` 一起联合使用
     - 现在更像历史桥接闸门，不是单点硬判

3. `高风险兜底，需重点复核`
   - [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:271)
   - [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:481)
   - [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:1199)
   - [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:1215)
   - [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:1228)
   - [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:1247)
   - [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:1254)
   - [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs:1260)
   - 原因：
     - 这些地方要么在 `NO_CLAUSE_ROWS` 时直接兜家族
     - 要么在 source collector 里决定“没有更强证据时到底选谁”
     - 最容易把 importer 的旧判断重新偷渡成当前运行时事实

#### 风险判断

- 这是目前最该警惕的旧派生运行时字段之一。
- 原因不是它一定错，而是它本质上是：
  - “导入阶段怎么重建虚拟主体”
  - 不是“当前工程定义链已经证明的最终结论”

#### 后续建议

- 分三步处理更合适：
  1. 先从粗分类观察层摘掉
     - 因为这层当前主题最明确，就是“只收敛粗分类，不吃旧结论字段”
  2. 再收家族映射里的 `NO_CLAUSE_ROWS / deferred H hint`
     - 也就是先拆最像“直接借老结论补洞”的分支
  3. 最后再审 source collector
     - 判断哪些 target-selection 是合法的 source bridge
     - 哪些其实已经把 `ImportSynthesisKind` 当成运行时真相

---

### 4. `SemanticRole / SemanticRoleScore`

#### 来源

- 上游 `Classification.PartRoles`
- importer 映射点：
  [XingcaiCacheImporter.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs:502)

#### 当前运行时消费

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:160)
- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:172)
- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs:177)

它现在仍直接控制：

- 哪些 part 进 `BodyCandidate`
- 哪些 part 进 `BodyAccessoryCandidate`
- 候选集 span source 选择
- 主体 seed 收集

#### 风险判断

- 这是当前阶段 2 仍然合法存在的运行时输入。
- 但它和 `EndProximity`、`ImportSynthesisKind` 不同：
  - 它不是“最终类结论”
  - 更像“上游 part-level 先验语义提示”

#### 当前问题

- `GL-20` 暴露过：
  - 上游 sample-level 已经看出 H
  - 但 part-level role 没把长腹板打成 `Web/WallCandidate`
  - 导致候选集收缩

#### 后续建议

- 当前不建议直接删除依赖。
- 更合理的方向是：
  - 把它明确定位成“阶段 2 的外部先验”
  - 再逐步补强：
    - 当前层几何自证
    - role 缺失时的同层补偿

---

### 5. `BodyDescriptorFamily / BodyDescriptorSectionType`

#### 来源

- [BodyMaterialSupport.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyMaterialSupport.cs:113)

它们不是旧启发式字段，当前已经来自：

- source semantic
- 当前 proof 主链摘要

#### 当前运行时消费

- [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:245)
- [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs:260)
- [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs:618)

#### 风险判断

- 这类字段不属于“旧流程残留结论”，但属于“摘要派生字段”。
- 风险不在“旧”，而在：
  - 如果摘要字段继续反向驱动运行时主判定
  - 会形成“上游摘要反过来压当前层判定”的耦合

#### 后续建议

- 保留它们作为：
  - sidecar
  - audit
  - 阶段间桥接摘要
- 但后续每个使用点都要明确：
  - 它是“辅助一致性判断”
  - 还是“真正的运行时主判据”

---

## 当前风险分级

### A. 高优先级继续收口

1. `ImportSynthesisKind`
2. `EndProximity / NearMemberStart / NearMemberEnd` 的残余运行时读取

### B. 中优先级评估是否彻底降级

1. `SourceMemberMainClassCode`
2. `BodyDescriptorFamily / BodyDescriptorSectionType` 在粗分类 direct signal / bypass 中的角色

### C. 暂不去依赖，但要承认它是外部先验

1. `SemanticRole / SemanticRoleScore`

---

## 当前结论

截至 `2026-04-29`，当前项目里“旧派生字段运行时依赖”最值得继续盯的，不是一个点，而是两条半主线：

1. `ImportSynthesisKind`
   - 仍深度参与家族映射、coarse direct signal、source target 选择
   - 是当前最大的旧派生字段运行时残留

2. `EndProximity`
   - 已经在分区主判定里暴露过真实问题
   - 当前主判定已修，但 `assemblySpan` 选源里仍残留真实运行时读取

3. `SourceMemberMainClassCode`
   - 最终家族判定虽然已经去依赖
   - 但粗分类观察层仍不是纯 observation
   - 它还在直接改粗分类阈值

---

## 下一步建议顺序

1. `2026-04-29` 已先清 `EndProximity` 在 `assemblySpan` 选源层的残余依赖
   - [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
   - 两个 `EstimateAssemblySpan(...)` 重载中的 `anchoredParts`
     已改为基于当前 `projectedInterval + usableSpan` 现场重算端部关系
   - 不再经 `IsSingleEndedLocalPart(part)` 读取 importer 带入的旧 `NearMemberStart / NearMemberEnd`

2. `2026-04-29` 已清 `ImportSynthesisKind / SourceMemberMainClassCode` 在 coarse observation 的 direct signal
   - [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs)
   - 已删除：
     - `directBoxSignalCount`
     - `directHSignalCount`
     - `CLOSED_LOOP_DIRECT_SIGNAL_CONSENSUS`
   - 当前 `BOX / H` 粗分类通过门槛已回到：
     - 候选集
     - 多切片站位比例
     - 闭环/组织关系本身

3. `2026-04-29` 已继续收掉 `ImportSynthesisKind` 在家族映射 / source collector 的第一轮桥接
   - [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs)
     - 已把以下 gate 从：
       - `ImportSynthesisKind || BodyDescriptorFamily`
     - 收到：
       - `BodyDescriptorFamily`
     - 涉及：
       - `ShouldPromoteGenericHFromPrimaryPlate(...)`
       - `ShouldPromoteSemanticBoxFromBodyPlate(...)`
       - `ShouldPromoteReviewReadyGenericH(...)`
       - `IsVariableSectionBoxReviewReady(...)`
       - `LooksLikeStableHFamily(...)`
     - 同时 `ShouldPromoteSemanticBoxWithoutClause(...)`
       已去掉对 `ImportSynthesisKind = BOX` 的要求，只保留：
       - `BodyDescriptorFamily = BuiltUpBox`
       - `BodyDescriptorSectionType = BUILTUP_BOX_VARIANT`
       - `NO_CLAUSE_ROWS`
       - `!SourceSemanticPriorityApplied`
       - `!HasTopologyRewrite`
   - [DefinitionClauseDecisionFullRunSourceCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs)
     - `STANDARD_ROD + BOX` 的排除口径已从 `ImportSynthesisKind = BOX`
       收到当前 `BuiltUpBox` 描述字段
     - 已删除“`CoreBodyPartIds > 1 / CoreBodyPartIds = 0` 且 `ImportSynthesisKind 非空` 就允许 fallback 选 target part”
       这两条旧兜底
   - [BodyFamilyDefinitionEvaluator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs)
     - 已继续删除：
       - `ShouldPromoteSemanticBoxWithoutClause(...)`
     - 也就是：
       - `NO_CLAUSE_ROWS`
       - `LeadClause = NONE`
       - `BodyDescriptorFamily = BuiltUpBox`
       - `BodyDescriptorSectionType = BUILTUP_BOX_VARIANT`
       这一条“仅凭 descriptor 直接 adjudicate 成 BOX”的直通口已关闭
     - 当前改为：
       - 没有主条款时，不再让 descriptor 直接替 proof 出最终家族
       - 只能回到 deferred / hint 路线

4. 最后继续做 `ImportSynthesisKind` 在非粗分类层的完全 observation 化评估
   - 当前还需复核：
     - 是否仍有展示/说明文案把它误写成主判据
     - 家族映射层里剩余 descriptor bridge 是否还要进一步收窄
   - `SourceMemberMainClassCode` 当前则已基本退到 observation / audit

> `2026-04-29` 当前边界补充：
>
> - 本表仍继续保留全局排查记录。
> - 但当前活跃执行面已收回到：
>   - 导出数据
>   - 主体候选
>   - 粗分类
> - 因此：
>   - `BodyFamilyDefinitionEvaluator.cs`
>   - `DefinitionClauseDecisionFullRunSourceCollector.cs`
>   相关的家族映射 / source collector 清理
>   当前只记账，不再继续推进。

## 当前边界内补充结论

### 1. 主体候选层

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
  当前已确认：
  - `NearMemberStart / NearMemberEnd`
    已不再参与：
    - `nearStableZone`
    - `assemblySpan` 选源
  - 剩余大量 `SemanticRole / SemanticRoleScore`
    仍然存在，但当前按项目约束仍归类为：
    - 阶段 2 合法外部先验
    - 不是“旧最终结论回灌”

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
  `2026-04-29` 再补充确认并已修：
  - `isInputMainPart && IsPrimaryBodyRole(...)`
    原先会绕开 coverage 主门槛，直接进入 `BodyCandidate`
  - 当前已改为：
    - 也必须先满足最小 `coverage`
    - 否则回落到 `BodyAccessoryCandidate / LocalStiffenerCandidate`
  - 当前说明：
    - `inputMainPart` 仍保留为先验
    - 但已不再是主体候选层的身份直通口

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
  `2026-04-29` 再补充确认并已修：
  - `ShouldPromoteSpecialShapeToBodyCandidate(...)`
    原先即便几何上已满足“长向稳定主板”，
    仍要求先有：
    - `isInputMainPart`
    - `PrimaryBodyRole`
    - `OuterSideCandidate`
    之一才允许放行
  - 当前已改为：
    - 当 `plate-like + non-tiny + in main component + near-stable + coverage >= 0.55`
      时，可仅凭当前层几何进入 `BodyCandidate`
    - 上游 role/main-part/outer-side
      只再作为低 coverage 情况下的辅助信号

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
  `2026-04-29` 再补充确认并已修：
  - `EstimateBodyAxis(...)`
    原先会直接优先采用 `inputMainPart` 的长向
  - 当前已改为：
    - `inputMainPart` 只有在自身长度不明显短于整批非 tiny 候选时
      才可继续保留主轴优先权
    - 否则回到“更长几何件优先，role 次级排序”
  - 当前说明：
    - 主轴选择这层也已开始从“身份优先”收回到“几何覆盖优先”

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
  `2026-04-29` 再补充确认并已修：
  - 两个 `EstimateAssemblySpan(...)`
    原先都存在：
    - 只要 `primaryBodyParts` 非空
    - 就直接拿它们定义整体 span source
  - 当前已改为：
    - 新增 `SelectSpanSourceParts(...)`
    - 先比较 `primary / anchored / usable` 的真实跨度完整度
    - 只有 `primaryBodyParts` 覆盖足够完整时，才允许其定义 assembly span
  - 当前说明：
    - `SemanticRole` 仍保留为候选层先验
    - 但已不再天然拥有整体 span 分母的定义权

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
  `2026-04-29` 再补充确认并已修：
  - `EstimateBodyAxisSegments(...)`
    原先在 `ProgressRatio / Length` 之后，
    仍会直接用：
    - `inputMainPart`
    - `PrimaryBodyRole`
    对 guide 候选做平局排序
  - 当前已改为：
    - 只有当该 guide 候选长度本身已接近最长候选时
    - `inputMainPart / role`
      才允许参与平局裁决
  - 当前说明：
    - guide 选线这层也已从“身份可翻盘”
      收到“几何先站住，身份只能做近似平局辅助”

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
  `2026-04-29` 再补充确认并已修：
  - `isInputMainPart && IsPrimaryBodyRole(...)`
    原先会绕开 coverage 主门槛，直接进入 `BodyCandidate`
  - 当前已改为：
    - 也必须先满足最小 `coverage`
    - 否则回落到 `BodyAccessoryCandidate / LocalStiffenerCandidate`
  - 当前说明：
    - `inputMainPart` 仍保留为先验
    - 但已不再是主体候选层的身份直通口

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
  `2026-04-29` 再补充确认并已修：
  - `ShouldPromoteSpecialShapeToBodyCandidate(...)`
    原先即便几何上已满足“长向稳定主板”，
    仍要求先有：
    - `isInputMainPart`
    - `PrimaryBodyRole`
    - `OuterSideCandidate`
    之一才允许放行
  - 当前已改为：
    - 当 `plate-like + non-tiny + in main component + near-stable + coverage >= 0.55`
      时，可仅凭当前层几何进入 `BodyCandidate`
    - 上游 role/main-part/outer-side
      只再作为低 coverage 情况下的辅助信号

### 2. 粗分类层

- [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs)
  `2026-04-29` 新确认并已修：
  - `SelectCandidateParts(...)`
    原先会同时吃：
    - `proofParts`
    - `BodyDescriptorCorePartIds`
  - 这等于让：
    - proof 层结果
    - summary/descriptor 结果
    反向参与 coarse candidate selection
  - 当前已改为：
    - 只基于 `BodyCandidatePartitionItem`
    - `PartitionClass`
    - `LongitudinalCoverageEstimate`
    选择 coarse candidate parts

- 当前这说明：
  - 粗分类层此前真正残留的一条“层级倒灌”口子
    不是 `ImportSynthesisKind`
    而是：
    - proof/descriptor 回灌 candidate selection
  - 这条口子已被关闭

- [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs)
  `2026-04-29` 再补充确认并已修：
  - `ResolveFamilyVariability(...)`
    原先会先吃：
    - `summary.LongitudinalTypeCode == POLYLINE`
  - 然后直接返回：
    - `IsVariable = true`
    - `VariationScore = 1`
  - 这意味着：
    - 变化截面小类并不完全来自当前层多切片观察
    - 而是被摘要字段提前定性
  - 当前已改为：
    - 只基于 eligible stations 自身的：
      - `SpanY / SpanZ`
      - `DistinctPartCount`
      - `CandidateSegmentCount`
      现场判断是否属于 `VARIABLE_SECTION_*`

- [CoarseMainClassObservationCollector.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationCollector.cs)
  `2026-04-29` 再补充复核：
  - `BodyDescriptorFamily / BodyDescriptorSectionType`
    在 coarse 层当前不再参与：
    - 大类主判定
    - 变化截面小类判定
  - 唯一剩余判定用途是：
    - `BodyDescriptorFamily = StandardSection`
    - 命中 `ATTRIBUTE_DIRECT_BYPASS`
  - 这条按当前项目冻结口径属于：
    - 合法的“属性直达分叉”
    - 不是摘要字段反向决定 `BOX/H/PRIMARY_PLATE_BODY`
  - `2026-04-29` 当前进一步收口：
    - `ResolveDecision(...)`
      已不再接收整份 `BodyMaterialSummary`
    - `IsAttributeDirectBypass(...)`
      已收到只接 `bodyDescriptorFamily`
    - 当前 coarse 判定函数对 summary 的可见依赖已进一步缩到最小

- [DefinitionDrivenSidecarCoordinator.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionDrivenSidecarCoordinator.cs)
  `2026-04-29` 新补充确认：
  - `CoarseMainClassObservationCollector.Collect(...)`
    的结果当前只流向：
    - [CoarseMainClassObservationWorkflow.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoarseMainClassObservationWorkflow.cs)
    - `json/md/xlsx` sidecar 工件
  - 未再被：
    - `BodyFamilyDefinitionEvaluator`
    - `BodyProfileResolver`
    - 其它运行时主链
    反向消费
  - 当前说明：
    - coarse observation 不仅内部判定链已基本纯化
    - 输出消费边界当前也已保持 observation-only

- [BodyCandidatePartitioner.cs](/I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs)
  `2026-04-29` 再补充确认并已收口：
  - `IsSingleEndedLocalPart(PartFeature part)`
    这一旧 helper 仍会直接读取：
    - `part.NearMemberStart`
    - `part.NearMemberEnd`
  - 当前虽已不在主体候选主路径被调用，
    但继续保留会增加未来误接回旧端部字段的风险
  - 当前已删除，仅保留：
    - `IsSingleEndedLocalPart(bool nearStart, bool nearEnd)`
    并只服务于当前现场重算出的端部关系
