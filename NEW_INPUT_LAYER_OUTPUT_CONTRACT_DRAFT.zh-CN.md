# 新输入层输出契约草案

## 文档定位

本文档是“新输入层”阶段 1 的输出契约草案。

当前定位：

1. `V3` 候选主题的预研契约
2. 只用于定义新输入层最小输出面
3. 暂不替代当前：
   - [FOUNDATION_GEOMETRY_HEALTH_AUDIT_OUTPUT_CONTRACT.zh-CN.md](</I:/autoteklasuanfa/FOUNDATION_GEOMETRY_HEALTH_AUDIT_OUTPUT_CONTRACT.zh-CN.md>)
   - [PROJECT_STATUS_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_STATUS_V2.zh-CN.md>)
   - [PROJECT_TASKLIST_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_TASKLIST_V2.zh-CN.md>)

本文档只回答：

1. 新输入层准备输出哪些对象
2. 哪些字段属于原始导出事实
3. 哪些字段属于归一化表达
4. 哪些字段属于推导辅助字段
5. 当前阶段哪些消费方式允许，哪些禁止

本文档不定义：

1. `BOX / H / PRIMARY_PLATE_BODY` 主判定规则
2. proof 条款
3. 最终家族映射

---

## 设计目标

新输入层的目标不是“缓存更多字段”本身，而是：

1. 把 Tekla 深化模型导出成更强语义、更少失真的中间表达
2. 尽量减少：
   - 主轴劫持
   - 候选集漏主板
   - trace 方向漂移
   - 局部 edge 几何压坏整体组织关系
3. 为后续：
   - candidate
   - station
   - trace
   - topology
   提供更稳定的输入基座

一句话：

- 当前缓存偏“摘要几何”
- 新输入层要把它提升成“可追溯的参数化归一化表达”

---

## 当前适用范围

阶段 1 草案只覆盖四类对象：

1. `Member`
2. `LongitudinalPath`
3. `NormalizedPart`
4. `StationQueryContext`

当前阶段不直接定义：

1. `CandidateDecision`
2. `TraceDecision`
3. `TopologyDecision`
4. `ProofDecision`

原因：

这些属于下游解释层，不应混进新输入层基础契约。

---

## 输出原则

## 1. 必须区分三类字段

每个对象的字段都必须标清属于以下哪一类：

1. `RawFact`
   - 直接来自上游导出或 Tekla 事实
2. `NormalizedRepresentation`
   - 为了统一表达而做的归一化结果
3. `DerivedAid`
   - 为后续解释层服务的辅助推导字段

禁止把三类字段混成一个“不知道是事实还是推导”的口袋对象。

## 2. 归一化表达必须可回溯

凡是 `NormalizedRepresentation` 字段，都必须能指出来源是：

1. 哪个原始字段
2. 哪段算法
3. 哪种 fallback

## 3. 当前阶段先并行输出

新输入层输出当前只能：

1. 并行导出
2. 对照验证
3. sidecar 观察

不得：

1. 直接替换当前默认输入链
2. 直接回灌主判定

---

## 顶层输出对象

阶段 1 建议输出单一 JSON 顶层对象：

1. `SchemaVersion`
2. `GeneratedAtUtc`
3. `SourceRunId`
4. `SourceInputDirectory`
5. `NormalizationProfile`
6. `Members`

### SchemaVersion

阶段 1 草案固定建议值：

- `new-input-layer-draft/v0`

当前用 `draft/v0`，目的就是明确：

- 这不是已经冻结的正式 `V3` 契约

### NormalizationProfile

固定包含：

1. `PathModelPolicy`
2. `PlateModelPolicy`
3. `FallbackPolicy`
4. `ConfidencePolicy`

它不保存具体判定结果，只保存当前这份导出采用了什么归一化策略。

---

## Member 对象

每个 `Member` 固定包含：

1. `AssemblyId`
2. `MemberId`
3. `MemberName`
4. `Profile`
5. `Material`
6. `SourceFileName`
7. `RawMember`
8. `NormalizedLongitudinalPath`
9. `NormalizedParts`
10. `StationQueryContexts`
11. `NormalizationWarnings`

### RawMember

`RawMember` 只存原始事实，固定包含：

1. `MainAxis`
2. `BoundingBox`
3. `GuidePolyline`
4. `AxisSegments`
5. `LongitudinalAxisKind`
6. `LongitudinalAxisConfidence`
7. `LongitudinalAxisSource`

这些都属于 `RawFact`。

### NormalizedLongitudinalPath

这是当前阶段最关键的新对象。

固定包含：

1. `PathModelType`
2. `PathSegments`
3. `PathLength`
4. `PathSourceKind`
5. `PathConfidence`
6. `FallbackUsed`
7. `SupportEvidence`

字段解释：

1. `PathModelType`
   - 建议允许值：
     - `RAW_AXIS_SEGMENTS`
     - `GUIDE_POLYLINE_RECONSTRUCTED`
     - `MAIN_AXIS_FALLBACK`
     - `SYNTHETIC_PATH`
2. `PathSegments`
   - 统一后的 longitudinal path 片段
3. `PathLength`
   - 统一 path 总长度
4. `PathSourceKind`
   - 这条 path 实际来自哪一类原始来源
5. `PathConfidence`
   - 对统一 path 自身的可信度评价
6. `FallbackUsed`
   - 是否发生 fallback
7. `SupportEvidence`
   - 支撑这条 path 的原始证据摘要

其中：

- `PathSegments`
- `PathLength`
- `PathSourceKind`
属于 `NormalizedRepresentation`

- `SupportEvidence`
属于 `DerivedAid`

---

## NormalizedPart 对象

每个 `NormalizedPart` 固定包含：

1. `PartId`
2. `PartName`
3. `PartType`
4. `ProfileString`
5. `Material`
6. `RawGeometry`
7. `PartModelType`
8. `NormalizedShape`
9. `LongitudinalProjection`
10. `SemanticHints`
11. `NormalizationWarnings`

### RawGeometry

只存原始事实，固定包含：

1. `BoundingBox`
2. `Centroid`
3. `LocalCoordinateSystem`
4. `Thickness`
5. `AxisProjection`
6. `SolidEdges`
7. `GeometryHints`
8. `EndProximity`

这些都属于 `RawFact`。

### PartModelType

建议允许值：

1. `PATH_SECTION_SWEEP`
2. `PLATE_BOUNDARY_THICKNESS`
3. `UNCLASSIFIED_SOLID_SUMMARY`

解释：

1. `PATH_SECTION_SWEEP`
   - 当前零件已被归一化为“路径 + 截面”表达
2. `PLATE_BOUNDARY_THICKNESS`
   - 当前零件已被归一化为“边界 + 厚度”表达
3. `UNCLASSIFIED_SOLID_SUMMARY`
   - 当前还无法可靠归入上述两类，只保留摘要表达

### NormalizedShape

阶段 1 固定建议包含：

1. `ReferenceFrame`
2. `LongDirection`
3. `NormalDirection`
4. `WidthDirection`
5. `Thickness`
6. `Boundary2D`
7. `SweepProfile`
8. `SweepPathReference`
9. `ModelConfidence`

说明：

1. 对 `PLATE_BOUNDARY_THICKNESS`
   - `Boundary2D + Thickness + ReferenceFrame` 为主
2. 对 `PATH_SECTION_SWEEP`
   - `SweepProfile + SweepPathReference + ReferenceFrame` 为主
3. 某些字段允许为空
   - 但必须显式为空，不能缺列

### LongitudinalProjection

固定包含：

1. `ProjectionStart`
2. `ProjectionEnd`
3. `ProjectionLength`
4. `CoverageRatio`
5. `ProjectionConfidence`

这些字段属于 `DerivedAid`，但必须和 `NormalizedLongitudinalPath` 同源计算。

### SemanticHints

当前阶段允许保留，但必须降格为“提示”：

1. `ImportedRoleHint`
2. `ImportedRoleScore`
3. `SourceMainPartHint`
4. `OuterSideHint`

这些字段不得伪装成几何事实。

---

## StationQueryContext 对象

新输入层当前不直接输出 trace/topology 结果，但应预留 station 查询上下文。

每个 `StationQueryContext` 固定包含：

1. `StationIndex`
2. `StationRatio`
3. `StationDistance`
4. `SectionFrameOrigin`
5. `SectionAxisX`
6. `SectionAxisY`
7. `SectionAxisZ`
8. `SegmentReference`
9. `FrameConfidence`
10. `Warnings`

这组对象的作用是：

1. 后续 station / trace / topology 都从这里取统一截面查询前提
2. 不再在多个下游模块里各自重建 station frame

---

## 路径片段对象

`PathSegments` 每个元素固定包含：

1. `SegmentIndex`
2. `StartPoint`
3. `EndPoint`
4. `Direction`
5. `PathStart`
6. `PathEnd`
7. `Length`
8. `SourceKind`
9. `SourceReference`

这里的重点是：

1. 统一 path 仍然必须保留片段级信息
2. 不允许再次压回单一方向向量

---

## 边界对象

对 `PLATE_BOUNDARY_THICKNESS`，`Boundary2D` 建议固定包含：

1. `BoundaryLoopKind`
2. `Points`
3. `Closed`
4. `BoundaryConfidence`
5. `SourceKind`

建议允许值：

1. `OUTER_LOOP`
2. `APPROX_OUTER_LOOP`
3. `UNRESOLVED`

当前阶段允许 `Boundary2D` 为空，但如果为空，必须写：

- `BoundaryLoopKind = UNRESOLVED`

---

## 警告与异常字段

新输入层不直接输出“主判定失败”，但允许输出归一化警告。

`NormalizationWarnings` / `Warnings` 当前建议使用注册表式 code。

阶段 1 草案先建议以下 code：

### 通用

1. `NONE`
2. `RAW_INPUT_MISSING`
3. `FALLBACK_USED`
4. `MULTIPLE_SOURCE_CONFLICT`
5. `LOW_CONFIDENCE_NORMALIZATION`

### 路径类

1. `PATH_TOO_SHORT_FOR_MEMBER`
2. `PATH_SOURCE_CONFLICT`
3. `PATH_FALLBACK_TO_MAIN_AXIS`
4. `PATH_SEGMENTS_GAPPED`

### 板件类

1. `THICKNESS_INFERRED_FROM_SIZE`
2. `BOUNDARY_UNRESOLVED`
3. `PART_MODEL_TYPE_UNCLASSIFIED`
4. `LOCAL_FRAME_REPAIRED`

当前阶段这些 warning 只服务：

1. A/B 对照
2. health-audit 辅助定位
3. 预研期异常排查

不得直接驱动主类。

---

## 与当前缓存的关系

当前缓存已存在的原始字段，原则上只分两类处理：

### 1. 保留为 RawFact

例如：

1. `AxisSegments`
2. `GuidePolyline`
3. `MainAxis`
4. `BoundingBox`
5. `Centroid`
6. `LocalCoordinateSystem`
7. `AxisProjection`
8. `SolidEdges`

### 2. 降格为 Hint

例如：

1. `PartRoles`
2. `MainClass`
3. `OuterSideCandidate`
4. `EndProximity`

原则：

- 不允许再把 hint 混成归一化几何真相

---

## 消费边界

当前允许消费方：

1. 新输入层 sidecar
2. A/B 对照脚本
3. 基础健康检查层
4. 人工审阅与差分分析

当前禁止消费方：

1. `CoarseMainClassObservationCollector`
2. `BodyFamilyDefinitionEvaluator`
3. `BodyProfileResolver`
4. 任何最终家族判定器

禁止用法：

1. `PartModelType = PATH_SECTION_SWEEP => 直接判 H`
2. `Boundary2D 看起来闭合 => 直接判 BOX`
3. `PathConfidence 高 => 直接覆盖下游结果`

---

## 阶段 1 完成标准

阶段 1 只要求做到：

1. 有独立契约草案
2. 明确对象边界
3. 明确字段分类：
   - `RawFact`
   - `NormalizedRepresentation`
   - `DerivedAid`
4. 明确禁止回灌主判定

当前不要求：

1. 已完成全部代码实现
2. 已覆盖全部零件类型
3. 已切换默认输入链

---

## 阶段 2 直接下一步

基于本草案，下一步应做：

1. 新增最小原型输出对象
2. 只覆盖少量代表样本
3. 优先输出：
   - `NormalizedLongitudinalPath`
   - `NormalizedPart`
4. 先不碰默认入口

---

## 当前结论

当前结论固定为：

1. 新输入层应该先走“契约先行”
2. 这份文档是阶段 1 草案，不是正式冻结版
3. 只有当最小原型和 A/B 对照稳定后，才适合把它升级成正式 `V3` 契约
