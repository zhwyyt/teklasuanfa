# 基础几何健康检查输出契约

## 文档目的

本文档冻结 `FoundationGeometryHealthAudit` sidecar 的首版输出契约。

它只回答：

1. 健康检查结果写到哪些文件
2. JSON 顶层结构是什么
3. member-level summary 至少有哪些稳定字段
4. station / candidate detail 将如何扩展
5. 状态枚举、reason code、中文文案如何固定
6. 哪些消费方式明确禁止

本文档不定义新的粗分类规则，也不定义 proof / 家族映射逻辑。

---

## 当前适用范围

首版只落地三类检查：

1. `AxisConsistency`
2. `CandidateSetConsistency`
3. `SampleTraceConsistency`

首版暂不实现但预留字段：

1. `SectionFrameConsistency`
2. `TopologyInputConsistency`

预留字段必须显式输出为 `NOT_EVALUATED`，不能缺列。

---

## 输出工件

所有工件写入当前离线识别 run 的输出目录。

首版固定输出：

1. `foundation-geometry-health-audit.json`
2. `foundation-geometry-health-audit.zh-CN.md`

Excel 导出启用时固定输出：

3. `foundation-geometry-health-audit.xlsx`

后续若补 station / candidate 明细，仍优先并入同一个 JSON；只有当体积或人工审阅需要明确拆分时，才新增：

1. `foundation-geometry-health-audit-stations.json`
2. `foundation-geometry-health-audit-candidates.json`

首版不得新增会被粗分类主链读取的隐藏中间文件。

---

## JSON 顶层结构

顶层对象固定包含：

1. `SchemaVersion`
2. `GeneratedAtUtc`
3. `SourceRunId`
4. `SourceInputDirectory`
5. `SourceOutputDirectory`
6. `EnabledChecks`
7. `DeferredChecks`
8. `Summary`
9. `MemberRows`
10. `StationRows`
11. `CandidateRows`

### SchemaVersion

固定首版值：

- `foundation-geometry-health-audit/v1`

### EnabledChecks

首版固定为：

1. `AxisConsistency`
2. `CandidateSetConsistency`
3. `SampleTraceConsistency`

### DeferredChecks

首版固定为：

1. `SectionFrameConsistency`
2. `TopologyInputConsistency`

### Summary

`Summary` 固定包含：

1. `TotalMemberCount`
2. `PassCount`
3. `WarningCount`
4. `FailCount`
5. `NotEvaluatedCount`
6. `PrimaryReasonBreakdown`
7. `SuggestedInvestigationLayerBreakdown`
8. `CheckStatusBreakdown`

`PrimaryReasonBreakdown`、`SuggestedInvestigationLayerBreakdown`、`CheckStatusBreakdown` 均使用数组，不使用动态对象键，避免 Excel / Markdown 重建脚本对未知 reason code 做特殊解析。

每个 breakdown item 至少包含：

1. `Code`
2. `LabelZh`
3. `Count`
4. `Share`

`Share` 为 `0..1` 的小数。

---

## 状态枚举

所有检查状态字段统一使用：

1. `PASS`
2. `WARNING`
3. `FAIL`
4. `NOT_EVALUATED`

中文文案固定为：

| StatusCode | StatusLabelZh |
|---|---|
| `PASS` | `通过` |
| `WARNING` | `可疑` |
| `FAIL` | `失败` |
| `NOT_EVALUATED` | `未评估` |

解释口径：

1. `PASS`
   - 当前层输入可信，可继续往下看。
2. `WARNING`
   - 有可疑点，但还不足以单独解释粗分类异常。
3. `FAIL`
   - 当前层已经足以解释后续粗分类失真。
4. `NOT_EVALUATED`
   - 当前首版未实现该检查，或输入缺少必要证据。

`NOT_EVALUATED` 只能用于 deferred checks 或缺证据场景；不得拿它掩盖已能判断的异常。

---

## SuggestedInvestigationLayer 枚举

`SuggestedInvestigationLayerCode` 固定允许值：

1. `NONE`
2. `INPUT_LAYER`
3. `BODY_CANDIDATE_LAYER`
4. `SECTION_TOPOLOGY_OBSERVATION_LAYER`
5. `SECTION_FRAME_LAYER`
6. `TOPOLOGY_INPUT_LAYER`
7. `INSUFFICIENT_EVIDENCE`

中文文案固定为：

| Code | LabelZh |
|---|---|
| `NONE` | `暂无明显异常层级` |
| `INPUT_LAYER` | `输入层` |
| `BODY_CANDIDATE_LAYER` | `主体候选层` |
| `SECTION_TOPOLOGY_OBSERVATION_LAYER` | `截面拓扑观察层` |
| `SECTION_FRAME_LAYER` | `截面坐标系层` |
| `TOPOLOGY_INPUT_LAYER` | `拓扑输入层` |
| `INSUFFICIENT_EVIDENCE` | `证据不足` |

首版三项检查的映射原则：

1. `AxisConsistency = FAIL`
   - 优先映射为 `INPUT_LAYER`
2. `CandidateSetConsistency = FAIL`
   - 优先映射为 `BODY_CANDIDATE_LAYER`
3. `SampleTraceConsistency = FAIL`
   - 优先映射为 `SECTION_TOPOLOGY_OBSERVATION_LAYER`

如果多个检查同时 `FAIL`，优先选择更上游的层级：

1. `INPUT_LAYER`
2. `BODY_CANDIDATE_LAYER`
3. `SECTION_TOPOLOGY_OBSERVATION_LAYER`
4. `SECTION_FRAME_LAYER`
5. `TOPOLOGY_INPUT_LAYER`

---

## MemberRows

`MemberRows` 是首版主输出。每个元素对应一个 member / assembly 的健康检查摘要。

### 身份字段

固定包含：

1. `AssemblyId`
2. `AssemblyNumber`
3. `MemberId`
4. `MemberName`
5. `Profile`
6. `SourceFileName`

缺失时使用空字符串或 `null`，不得用 `UNKNOWN` 伪造输入事实。

### 总体状态字段

固定包含：

1. `OverallHealthStatusCode`
2. `OverallHealthStatusLabelZh`
3. `PrimaryReasonCode`
4. `PrimaryReasonLabelZh`
5. `SuggestedInvestigationLayerCode`
6. `SuggestedInvestigationLayerLabelZh`
7. `ReasonCodes`
8. `EvidenceSummaryZh`

`ReasonCodes` 是字符串数组，按诊断优先级排序。

`EvidenceSummaryZh` 是给人工 review 的短句，只能总结本行已有证据，不得引入未输出的隐藏判断。

### 检查状态字段

固定包含：

1. `AxisConsistencyStatusCode`
2. `AxisConsistencyStatusLabelZh`
3. `CandidateSetConsistencyStatusCode`
4. `CandidateSetConsistencyStatusLabelZh`
5. `SampleTraceConsistencyStatusCode`
6. `SampleTraceConsistencyStatusLabelZh`
7. `SectionFrameConsistencyStatusCode`
8. `SectionFrameConsistencyStatusLabelZh`
9. `TopologyInputConsistencyStatusCode`
10. `TopologyInputConsistencyStatusLabelZh`

首版中：

1. `SectionFrameConsistencyStatusCode = NOT_EVALUATED`
2. `TopologyInputConsistencyStatusCode = NOT_EVALUATED`

### Axis evidence 字段

固定包含：

1. `AxisSegmentsLength`
2. `MemberMainAxisLength`
3. `LongestMainPlateAxisProjectionLength`
4. `CandidateSetSpanLength`
5. `AxisLengthToMemberMainAxisRatio`
6. `AxisLengthToLongestMainPlateRatio`
7. `AxisLengthToCandidateSetSpanRatio`
8. `AxisEvidenceReasonCodes`

字段口径：

1. `AxisSegmentsLength`
   - 当前导出/导入链认为的 longitudinal axis segments 总长度。
2. `MemberMainAxisLength`
   - 原始 member main axis 长度。
3. `LongestMainPlateAxisProjectionLength`
   - 最长长向主板沿当前轴投影长度。
4. `CandidateSetSpanLength`
   - 当前主体候选集合沿当前轴覆盖的整体跨度。

比值字段在分母缺失或为 `0` 时输出 `null`。

### Candidate evidence 字段

固定包含：

1. `TotalPartCount`
2. `CandidatePartCount`
3. `BodyCandidatePartCount`
4. `CandidatePartIds`
5. `BodyCandidatePartIds`
6. `SuspectMissingMainPartIds`
7. `SuspectAccessoryDominantPartIds`
8. `CandidateSetCoverageRatio`
9. `CandidateEvidenceReasonCodes`

数组字段为空时输出空数组，不输出 `null`。

`CandidateSetCoverageRatio` 表示候选集合跨度相对 member 主跨度或可用 assembly span 的覆盖比例；具体分母必须在实现代码注释和 Markdown 摘要中说明。

### Sample trace evidence 字段

固定包含：

1. `EvaluatedStationCount`
2. `EvaluatedTraceCount`
3. `TraceDirectionDriftCount`
4. `TraceCenterDriftCount`
5. `TraceDiagonalizedCount`
6. `TraceParallelRelationLostCount`
7. `TraceConnectorRelationLostCount`
8. `MaxTraceDirectionDeviationDegrees`
9. `SampleTraceEvidenceReasonCodes`

首版若暂时只能做 member-level 聚合，以上字段仍必须输出；未能计算时使用 `0` 或 `null`，并追加 `EVIDENCE_INCOMPLETE`。

---

## StationRows

首版允许为空数组。

后续启用时，每个元素固定包含：

1. `AssemblyId`
2. `AssemblyNumber`
3. `MemberId`
4. `StationIndex`
5. `StationRatio`
6. `StationDistance`
7. `FrameStatusCode`
8. `FrameStatusLabelZh`
9. `TraceStatusCode`
10. `TraceStatusLabelZh`
11. `TopologyInputStatusCode`
12. `TopologyInputStatusLabelZh`
13. `ReasonCodes`
14. `EvidenceSummaryZh`

StationRows 不得反向决定 member 的 `OverallHealthStatus`，除非对应检查项已经在 member aggregation 中明确实现。

---

## CandidateRows

首版允许为空数组。

后续启用时，每个元素固定包含：

1. `AssemblyId`
2. `AssemblyNumber`
3. `MemberId`
4. `PartId`
5. `PartName`
6. `PartRole`
7. `PartitionClass`
8. `LongitudinalCoverageEstimate`
9. `AxisProjectionLength`
10. `CandidateDecision`
11. `CandidateDecisionLabelZh`
12. `ReasonCodes`
13. `EvidenceSummaryZh`

`CandidateDecision` 允许值：

1. `INCLUDED_BODY_CANDIDATE`
2. `INCLUDED_BODY_LIKE_SPECIAL_SHAPE`
3. `EXCLUDED_ACCESSORY`
4. `EXCLUDED_LOCAL_STIFFENER`
5. `SUSPECT_MISSING_MAIN_PLATE`
6. `SUSPECT_ACCESSORY_DOMINATES`
7. `NOT_EVALUATED`

---

## Reason code 注册表

### 通用 reason code

| Code | LabelZh |
|---|---|
| `NONE` | `无明显异常` |
| `CHECK_NOT_EVALUATED` | `检查项未评估` |
| `INPUT_DATA_MISSING` | `输入数据缺失` |
| `EVIDENCE_INCOMPLETE` | `证据不完整` |

### AxisConsistency reason code

| Code | LabelZh |
|---|---|
| `AXIS_OK` | `轴线一致性通过` |
| `AXIS_TOO_SHORT_FOR_MEMBER` | `导出轴线明显短于构件主跨度` |
| `AXIS_TOO_SHORT_FOR_MAIN_PLATE` | `导出轴线明显短于最长长向主板` |
| `AXIS_SPAN_NOT_COVERING_CANDIDATE_SET` | `导出轴线未覆盖主体候选集合跨度` |
| `AXIS_SOURCE_HIJACKED_BY_LOCAL_PART` | `轴线疑似被局部短件劫持` |
| `AXIS_SEGMENTS_MISSING_BUT_MAIN_AXIS_AVAILABLE` | `导出轴线缺失但原始主轴可用` |

### CandidateSetConsistency reason code

| Code | LabelZh |
|---|---|
| `CANDIDATE_SET_OK` | `主体候选集合一致性通过` |
| `CANDIDATE_SET_EMPTY` | `主体候选集合为空` |
| `MAIN_PLATE_MISSING_FROM_CANDIDATE_SET` | `疑似长向主板未进入候选集合` |
| `CANDIDATE_SET_SPAN_TOO_SHORT` | `主体候选集合跨度过短` |
| `SPECIAL_SHAPE_BODY_LIKE_PART_EXCLUDED` | `类似主体的 SpecialShape/BentPlate 被排除` |
| `ACCESSORY_DOMINATES_CANDIDATE_SET` | `附件疑似主导候选集合` |

### SampleTraceConsistency reason code

| Code | LabelZh |
|---|---|
| `SAMPLE_TRACE_OK` | `sample 与 trace 一致性通过` |
| `TRACE_DIRECTION_DRIFT_FROM_SAMPLE` | `trace 方向相对 sample 明显漂移` |
| `TRACE_CENTER_DRIFT_FROM_SAMPLE` | `trace 中心相对 sample 明显漂移` |
| `TRACE_DIAGONALIZED_BY_CORNER_GEOMETRY` | `trace 疑似被倒角或角部几何拉成对角线` |
| `TRACE_PARALLEL_RELATION_LOST` | `sample 中平行关系在 trace 中丢失` |
| `TRACE_CONNECTOR_RELATION_LOST` | `sample 中连接板关系在 trace 中丢失` |

首版实现不得输出未登记的 reason code。若新增 reason code，必须先更新本文档。

---

## OverallHealthStatus 聚合规则

`OverallHealthStatusCode` 由已启用检查聚合得到。

聚合规则：

1. 任一启用检查为 `FAIL`
   - `OverallHealthStatusCode = FAIL`
2. 无 `FAIL` 且任一启用检查为 `WARNING`
   - `OverallHealthStatusCode = WARNING`
3. 所有启用检查均为 `PASS`
   - `OverallHealthStatusCode = PASS`
4. 所有启用检查均无法评估
   - `OverallHealthStatusCode = NOT_EVALUATED`

首版 deferred checks 不参与 overall 聚合。

---

## PrimaryReasonCode 选择规则

若存在 `FAIL`，优先选择最上游 `FAIL` 对应的首个 reason code：

1. `AxisConsistency`
2. `CandidateSetConsistency`
3. `SampleTraceConsistency`

若无 `FAIL` 但存在 `WARNING`，按同样顺序选择首个 warning reason code。

若全部 `PASS`，固定：

- `PrimaryReasonCode = NONE`
- `PrimaryReasonLabelZh = 无明显异常`
- `SuggestedInvestigationLayerCode = NONE`

若全部 `NOT_EVALUATED`，固定：

- `PrimaryReasonCode = CHECK_NOT_EVALUATED`
- `SuggestedInvestigationLayerCode = INSUFFICIENT_EVIDENCE`

---

## Markdown 输出契约

`foundation-geometry-health-audit.zh-CN.md` 至少包含：

1. 标题
2. 生成时间
3. 启用检查项
4. 暂缓检查项
5. 总体状态统计
6. 主要 reason code 分布
7. 建议排查层级分布
8. `FAIL / WARNING` member 表
9. 全量 member summary 表
10. 首版未落地 detail 的说明

`FAIL / WARNING` member 表固定列顺序：

1. `AssemblyNumber`
2. `OverallHealthStatus`
3. `PrimaryReason`
4. `SuggestedInvestigationLayer`
5. `AxisConsistency`
6. `CandidateSetConsistency`
7. `SampleTraceConsistency`
8. `EvidenceSummary`

全量表可以复用同一列顺序。

Markdown 不得只输出自然语言摘要，必须保留可对账表格。

---

## Excel 输出契约

`foundation-geometry-health-audit.xlsx` 固定 sheet：

1. `MemberSummary`
2. `ReasonBreakdown`
3. `LayerBreakdown`
4. `CheckBreakdown`

后续启用明细时追加：

1. `StationDetail`
2. `CandidateDetail`

Excel 列名必须与 JSON 字段名一致，除非已有 Excel 导出层明确要求中文列名；若使用中文列名，必须同时保留字段名列或 README 映射。

---

## 消费边界

允许消费方：

1. 人工 review
2. Markdown / Excel 对账
3. 后续测试断言
4. 后续定位异常样本根因

禁止消费方：

1. `CoarseMainClassObservationCollector`
2. `BodyFamilyDefinitionEvaluator`
3. `BodyProfileResolver`
4. 任何最终家族判定器

禁止用法：

1. `Audit says H-like => 直接判 H`
2. `Audit says suspicious => 直接打回 NONE`
3. 用 `SourceMemberMainClassCode / ImportSynthesisKind / BodyDescriptor*` 替代当前检查证据
4. 用 proof / family 输出反向补健康检查结论

---

## 首版验收样本

首版至少要能覆盖以下对照样本：

### AxisConsistency

样本：

- `T2-13GL-9`
- `T2-13GL-10`
- `T2-13GL-16`
- `T2-13GL-21`
- `T2-13GL-24`

期望：

- 对旧短轴缓存能给出 `AXIS_TOO_SHORT_FOR_MEMBER` 或 `AXIS_SOURCE_HIJACKED_BY_LOCAL_PART`
- 对新修正缓存不应继续报同类 `FAIL`

### CandidateSetConsistency

样本：

- `T2-13GL-23`

期望：

- 对零厚度板型 Beam 未进入 wall/main candidate 的旧输入，能给出 `MAIN_PLATE_MISSING_FROM_CANDIDATE_SET`
- 对修正后输入不应继续报同类 `FAIL`

### SampleTraceConsistency

样本：

- `T3-2GL-53`
- `T3-2GL-55`

期望：

- 对 trace 被最远点对拉斜的旧表达，能给出 `TRACE_DIRECTION_DRIFT_FROM_SAMPLE` 或 `TRACE_DIAGONALIZED_BY_CORNER_GEOMETRY`
- 对修正后 trace 不应继续报同类 `FAIL`

---

## 首版实现前置条件

开始写模型 / collector / workflow 前，必须先满足：

1. 本文档已被 V2 状态板和任务板引用为输出契约真源
2. 首版实现只写 sidecar，不改粗分类主判定
3. 首版 JSON 即使没有 station / candidate 明细，也必须输出空数组
4. 首版不得恢复 stash 草稿里的实现后直接落地；只能选择性参考字段和结构

---

## 后续变更规则

以下变更必须先更新本文档：

1. 新增输出文件
2. 删除或重命名 JSON 字段
3. 新增状态枚举
4. 新增 reason code
5. 改变 overall 聚合规则
6. 让 station / candidate detail 参与 member-level 聚合

以下变更不需要更新本文档：

1. 调整 Markdown 表格排序
2. 增加非契约性说明段落
3. 修正文案错别字
4. 增加内部实现 helper
