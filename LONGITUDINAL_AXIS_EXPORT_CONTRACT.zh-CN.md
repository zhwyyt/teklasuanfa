# 折线主体长度方向导出契约

## 文档目的

本文档冻结 `TeklaSectionClassifier -> member_*.json -> autoteklasuanfa` 之间的长度方向结构化导出契约。

目标不是继续让下游从原始 `SolidEdges` 临时反推主体折线，而是把“主体沿长度方向如何展开”前移为上游明确导出的稳定几何证据。

---

## 适用范围

- 上游提取：
  - `I:\xingcaisuanfa\TeklaSectionClassifier\Models.cs`
  - `I:\xingcaisuanfa\TeklaSectionClassifier\Tekla2017MemberExtractor.cs`
- 下游消费：
  - [XingcaiCacheImporter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs>)
  - 阶段 2-4 的 longitudinal 相关算法

---

## 设计原则

1. `SolidEdges` 保留，但不再作为长期唯一主路径。
2. 上游负责整理“主体导向折线”，下游负责消费，不重复猜。
3. 对直构件和折线构件使用同一套累计里程语义。
4. 新字段必须允许渐进接线，不要求一次性打断旧缓存。
5. 规则判定不能依赖手工名单、样本兜底或名称特判。

---

## 当前问题

当前 `member_*.json` 已包含 `SolidEdges`，下游也已能基于它恢复折线 guide 并修补“单一直轴误读”的漏洞。

但这仍存在三个结构性问题：

1. 上下游职责混杂：
   - 上游导出的是原始边集
   - 下游承担了主体折线抽取
2. 语义容易漂移：
   - 不同阶段可能用不同方式从 `SolidEdges` 反推 guide
3. 兼容成本偏高：
   - 每一层算法都要背一份“边集转长度轴”的复杂度

因此，后续应把“主体长度方向结构化证据”正式上移到导出层。

---

## 目标契约

### 1. Member 级新增字段

建议在 `MemberSnapshot.Member` 或 `MemberSnapshot` 上新增一组 member 级长度方向字段，用于表达整个构件的主体导向。

推荐字段：

- `LongitudinalAxisKind`
  - 值域：
    - `STRAIGHT`
    - `POLYLINE`
- `GuidePolyline`
  - 点列，表达主体导向折线
- `AxisSegments`
  - 分段表达，每段都带累计里程
- `LongitudinalAxisConfidence`
  - `0.0 - 1.0`
- `LongitudinalAxisSource`
  - 建议值：
    - `MAIN_AXIS`
    - `SOLID_EDGE_GUIDE`
    - `DERIVED_FALLBACK`

说明：

- 这组字段是下游阶段 2-4 的主消费对象。
- `GuidePolyline` 与 `AxisSegments` 属于结构化证据；`MainAxis` 继续保留为基础兼容字段。

### 2. Segment 结构

建议新增 `LongitudinalAxisSegment`：

- `Start`
- `End`
- `Direction`
- `Length`
- `CumulativeStart`
- `CumulativeEnd`

约束：

1. `Direction = Normalize(End - Start)`
2. `Length > 0`
3. 首段 `CumulativeStart = 0`
4. 后一段 `CumulativeStart = 前一段 CumulativeEnd`
5. 末段 `CumulativeEnd = member longitudinal total length`

### 3. Point 级约束

`GuidePolyline` 约束：

1. 至少 2 个点
2. 相邻点不能重合
3. `AxisSegments.Count = GuidePolyline.Count - 1`
4. `STRAIGHT` 类型允许只有 1 个 segment
5. `POLYLINE` 类型要求至少 2 个 segment

---

## 语义定义

### `LongitudinalAxisKind`

- `STRAIGHT`
  - 主体长度方向可由单一直线段稳定表达
- `POLYLINE`
  - 主体长度方向存在稳定折线展开，需要分段累计里程

注意：

- `POLYLINE` 不是“形状复杂”的同义词
- 只有当主体的主导长度方向确实发生分段转折时才标记为 `POLYLINE`

### `LongitudinalAxisConfidence`

用途：

- 防止下游把弱 guide 当强证据
- 为后续“是否允许进入 proof 主链”提供几何可信度参考

建议解释：

- `>= 0.90`：可直接作为主路径消费
- `0.60 - 0.89`：可消费，但应在摘要中保留导向来源
- `< 0.60`：仅允许保守消费，必要时进入 review

说明：

- 当前阶段先冻结字段和语义，不急着在阶段 5/6 直接把它接成判定条款

### `LongitudinalAxisSource`

用于表达 guide 的生成来源，避免后续排查时只看到结果看不到来源。

推荐含义：

- `MAIN_AXIS`
  - 直接使用上游 reference/main part 的直轴
- `SOLID_EDGE_GUIDE`
  - 由主体相关 `SolidEdges` 整理出的折线 guide
- `DERIVED_FALLBACK`
  - 仅在证据不足时使用的保守推导结果

---

## 与现有字段的关系

### 保留字段

- `MainAxis`
- `SolidEdges`
- `AxisProjection`

这些字段暂时都保留，不做破坏性删除。

### 职责调整

- `MainAxis`
  - 继续作为兼容基础字段
  - 不再假定足以表达所有折线主体
- `SolidEdges`
  - 继续作为原始几何证据
  - 不再作为长期唯一主消费字段
- `GuidePolyline / AxisSegments`
  - 成为阶段 2-4 的优先消费字段

---

## 上游提取策略

### 第一原则

不要给每个零件各自导一套“长度方向折线”，而应优先导出 member 级主体导向。

原因：

1. 下游阶段 2-4 的站位与稳定区语义本来就是 assembly/member 级
2. 每个零件各自一套 guide 会放大歧义
3. `T3-5GL-21` 这类问题本质上是“主体整体长度方向”被单轴误读

### 建议实现顺序

1. 先基于 reference/main body 候选与其 `SolidEdges` 构建 member 级 `GuidePolyline`
2. 再从 `GuidePolyline` 派生 `AxisSegments`
3. 最后写回 `MemberSnapshot`

### 不建议的实现方式

1. 不建议只给 `PartSnapshot` 加 guide 而没有 member 级 guide
2. 不建议让下游继续按阶段各自重建 guide
3. 不建议用名称、编号或样本名单决定 `POLYLINE`

---

## 下游消费策略

### 第一阶段

在 [XingcaiCacheImporter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs>) 中：

1. 若新字段存在：
   - 直接导入 `GuidePolyline / AxisSegments`
2. 若新字段不存在：
   - 继续使用当前基于 `SolidEdges` 的恢复逻辑

目的：

- 先完成平滑迁移
- 不立即打断现有缓存与回归样本

### 第二阶段

待新缓存重导稳定后：

1. 下游优先依赖结构化 axis
2. `SolidEdges` 推断逻辑逐步退化为诊断或过渡路径
3. 后续再评估是否完全移除主链 fallback

---

## 阶段 2-4 的统一语义

接入新契约后，以下概念都必须统一为“累计里程 + 当前 segment 局部方向”：

- `BodyCandidatePartition` 中的 longitudinal coverage
- `StableBodyZone` 中的 `AxisIntervalMin / AxisIntervalMax`
- `SectionStation` 的站位坐标
- `SectionTrace` 的局部截面方向

约束：

1. 不允许一部分仍按全局直轴投影，另一部分按分段里程
2. 不允许 `POLYLINE` 只作为标签而不参与站位/截面方向计算

---

## 验收口径

### 上游验收

对同一批样本重新导出后，应满足：

1. 直构件稳定得到：
   - `LongitudinalAxisKind = STRAIGHT`
2. 折线主体样本稳定得到：
   - `LongitudinalAxisKind = POLYLINE`
   - `AxisSegments.Count >= 2`
3. `GuidePolyline` 与 `AxisSegments` 的累计长度自洽

### 下游验收

至少对以下真实目录回归：

- `I:\xingcaisuanfa\cache\run_body_bracket_real_06`
- `I:\xingcaisuanfa\cache\run_body_bracket_real_05`

重点观察：

1. `T3-5GL-21` 不再退回“单轴误读”类问题
2. 阶段 2 出现合理 `BodyCandidate`
3. 阶段 3 出现稳定 `StableBodyZone`
4. 阶段 4 `SectionTrace` 按分段方向工作
5. 若仍未 adjudicate，为阶段 5/6 条款问题，而非长度方向几何问题

---

## 本阶段不做的事

当前文档只冻结 longitudinal 结构化导出契约，不在本阶段直接决定：

1. `CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE` 是否要放宽
2. `Mixed / HoldEffectOnly` 是否要改 gate
3. 阶段 5/6 是否直接消费 `LongitudinalAxisConfidence`

这些都应放在几何证据层稳定之后再评估。

---

## 当前执行顺序

1. 先按本文档修改 `TeklaSectionClassifier` 上游模型与提取
2. 导出一批新缓存做 smoke
3. 再接 [XingcaiCacheImporter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs>)
4. 再统一阶段 2-4 longitudinal 语义
5. 最后才回头分析阶段 5/6 条款升级门槛
