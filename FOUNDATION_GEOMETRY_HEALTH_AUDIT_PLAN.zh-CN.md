# 基础几何与数据健康检查方案

## 文档目的

本文档用于把当前项目从“样本驱动找漏洞”推进到“基础层先做健康体检”。

当前目标不是继续扩粗分类规则，而是先建立一层独立的健康检查，
用于系统发现以下几类根因：

1. 上游导出轴线不可信
2. 站位截面坐标系不稳定
3. trace 重建把真实组织关系整理歪了
4. 候选集与几何事实不一致
5. 围合/拓扑观察输入在进入粗分类前已经失真

这层检查只服务于：

- 输入层
- 主体候选层
- 截面拓扑观察层

不服务于：

- proof 条款层
- 最终家族判定层
- 家族映射层

---

## 为什么现在要做这一层

当前粗分类基线已经基本收住，但最近几轮问题也暴露出同一个模式：

1. 样本表面看起来像 `BOX / H` 漏判
2. 真正根因却常常更上游
3. 例如：
   - 导出轴被短腹板劫持
   - 零厚度板型 Beam 没拿到主板语义
   - trace 被最远点对拉成斜线
   - 真实平行翼板在观察层被表达成镜像斜板

如果没有一层独立健康检查，
就只能等粗分类结果异常后，人工再反查几何链哪里坏了。

这会导致：

1. 调试成本高
2. 问题发现滞后
3. 容易误把“输入失真”当成“规则不够宽”
4. 容易重新回到按样本补规则

---

## 核心原则

### 1. 健康检查不直接改判主类

它的职责是暴露输入事实是否可信，
不是替粗分类直接产出 `BOX / H / PRIMARY_PLATE_BODY`。

### 2. 先查几何事实，再查规则结果

粗分类前必须先回答：

1. 主轴是否可信
2. 站位截面是否可信
3. 代表 trace 是否可信
4. 候选集是否可信

### 3. 检查规则必须是普适性 invariant

不允许出现：

- 某个编号例外
- 某类命名习惯例外
- 某个样本专用阈值

### 4. 健康检查输出必须是 sidecar

这一层先只做：

- audit
- 对账
- 诊断

不直接回灌粗分类主判定。

---

## 检查对象分层

健康检查按四个对象层级组织。

### A. Member 级

回答整根构件的基础几何骨架是否可信：

- longitudinal axis
- member span
- station frame

### B. Candidate Set 级

回答当前进入粗分类的主板候选集是否可信：

- 是否漏掉长向主板
- 是否把附件误当主体
- 是否出现“候选集与主跨度脱节”

### C. Station 级

回答每个优先站位的截面表达是否可信：

- 站位是否落在合理里程
- 截面坐标轴是否稳定
- 该站位 trace 是否与样本截面一致

### D. Trace / Topology 级

回答进入 `BOX / H` 观察前的组织关系表达是否可信：

- 平行关系是否被整理歪
- 闭环边界是否真能闭合
- 中间连接板是否被丢成附件

---

## 首批检查主题

### 1. AxisConsistency

目的：

- 防止短附件、短腹板、局部更直的小件劫持整根 member 轴线

重点对比：

1. `AxisSegmentsLength`
2. `Member.MainAxis.Length`
3. 最长连续主板的 `AxisProjection.Length`
4. 候选主板集合的整体跨度

首批 invariant：

1. 导出轴长度不能显著短于 member 主跨度
2. 导出轴长度不能显著短于最长长向主板投影长度
3. 候选主板整体跨度不能与导出轴严重脱节
4. 若 axis 被判为无效，必须能给出具体原因码

典型异常：

- `AXIS_TOO_SHORT_FOR_MEMBER`
- `AXIS_TOO_SHORT_FOR_MAIN_PLATE`
- `AXIS_SPAN_NOT_COVERING_CANDIDATE_SET`
- `AXIS_SOURCE_HIJACKED_BY_LOCAL_PART`

### 2. SectionFrameConsistency

目的：

- 防止 station frame 在折线段切换、轴方向变化、局部异常时翻转或漂移

重点检查：

1. `sectionAxisX / Y / Z` 是否正交
2. 相邻 station 的 frame 是否连续
3. 折线构件跨段时是否正确跟随当前段切向

首批 invariant：

1. frame 三轴必须近似正交
2. 相邻站位不能无故翻面
3. 折线不同段的站位取截面必须保持“对当前段正切”

典型异常：

- `SECTION_FRAME_NOT_ORTHOGONAL`
- `SECTION_FRAME_SUDDEN_FLIP`
- `SECTION_FRAME_NOT_TANGENT_TO_SEGMENT`

### 3. SampleTraceConsistency

目的：

- 防止上游 `Samples` 已经正确，但 trace 整理后把真实板件关系做坏

重点对比：

1. 原始 sample 截面方向
2. 清理后的 trace 方向
3. sample 中的跨度/中心关系
4. trace 中的跨度/中心关系

首批 invariant：

1. sample 已平行的主板，trace 不应变成明显斜交
2. sample 已居中的连接板，trace 不应跑到外侧
3. trace 不应被局部倒角/斜边主导成对角线

典型异常：

- `TRACE_DIRECTION_DRIFT_FROM_SAMPLE`
- `TRACE_CENTER_DRIFT_FROM_SAMPLE`
- `TRACE_DIAGONALIZED_BY_CORNER_GEOMETRY`

### 4. CandidateSetConsistency

目的：

- 防止“看起来像规则没判出来”，其实是主板根本没进入候选集

重点检查：

1. 长向非 tiny 板件是否被错误排除
2. `SpecialShape / BentPlate` 是否被合理纳入
3. 候选集是否覆盖了主跨度骨架
4. 候选集是否被附件零件污染

首批 invariant：

1. 高 coverage 的长向板件不能只因身份弱就被排除
2. 候选集整体跨度不能明显小于 member 主跨度
3. 候选集若只剩局部短件，必须明确报错

典型异常：

- `MAIN_PLATE_MISSING_FROM_CANDIDATE_SET`
- `CANDIDATE_SET_SPAN_TOO_SHORT`
- `SPECIAL_SHAPE_BODY_LIKE_PART_EXCLUDED`
- `ACCESSORY_DOMINATES_CANDIDATE_SET`

### 5. TopologyInputConsistency

目的：

- 在进入 `BOX / H` 观察前，先确认组织关系输入本身没有被破坏

重点检查：

1. `H` 所需的平行外板 + 中间连接板是否真实可见
2. `BOX` 所需的多壁围合是否真实可见
3. eligible station 的质量是否足够

首批 invariant：

1. `H` 若失败，要能区分“候选缺失”和“trace 表达失真”
2. `BOX` 若失败，要能区分“闭环证据不足”和“站位输入失真”
3. 同一 member 不应大量出现“station 本身不可用但未被标记”

典型异常：

- `H_TOPOLOGY_INPUT_INCOMPLETE`
- `BOX_TOPOLOGY_INPUT_INCOMPLETE`
- `ELIGIBLE_STATION_GEOMETRY_UNSTABLE`

---

## 输出工件设计

这层先只设计成 sidecar，不直接回灌主判定。

建议输出三类工件：

### 1. member-level summary

建议文件：

- `foundation-geometry-health-audit.json`
- `foundation-geometry-health-audit.zh-CN.md`
- `foundation-geometry-health-audit.xlsx`

每个 member 至少包含：

1. `AssemblyId`
2. `AssemblyNumber`
3. `AxisConsistencyStatus`
4. `SectionFrameConsistencyStatus`
5. `SampleTraceConsistencyStatus`
6. `CandidateSetConsistencyStatus`
7. `TopologyInputConsistencyStatus`
8. `PrimaryReasonCode`
9. `PrimaryReasonLabelZh`
10. `SuggestedInvestigationLayer`

### 2. station-level detail

用于回答：

- 是哪一个站位先坏了
- 坏在 frame、trace 还是 topology input

建议字段：

1. `StationIndex`
2. `StationRatio`
3. `FrameStatus`
4. `TraceStatus`
5. `TopologyInputStatus`
6. `ReasonCodes`

### 3. candidate/part-level detail

用于回答：

- 哪块板本该成为主体候选却被排掉
- 哪块附件反而主导了 axis/span/candidate set

建议字段：

1. `PartId`
2. `PartRole`
3. `Coverage`
4. `AxisProjectionLength`
5. `CandidateDecision`
6. `ReasonCodes`

---

## 状态分级

每类检查都统一使用三档：

1. `PASS`
2. `WARNING`
3. `FAIL`

解释口径：

- `PASS`
  - 当前层输入可信，可继续往下看
- `WARNING`
  - 有可疑点，但还不足以判定这一层失真
- `FAIL`
  - 当前层已经足以解释后续粗分类失真

同时每个 member 再汇总一个：

- `OverallHealthStatus`

汇总原则：

1. 任一关键层 `FAIL`，overall 即 `FAIL`
2. 无 `FAIL` 但有 `WARNING`，overall 为 `WARNING`
3. 全部通过才为 `PASS`

---

## 首批落地顺序

### 第一批

先做最容易形成普适收益的三项：

1. `AxisConsistency`
2. `CandidateSetConsistency`
3. `SampleTraceConsistency`

原因：

这三项已经直接对应最近最典型的共性 root cause：

1. 短腹板劫持主轴
2. 主板候选失守
3. trace 被整理歪

### 第二批

再补：

1. `SectionFrameConsistency`
2. `TopologyInputConsistency`

原因：

这两项更适合在前面三项稳定后，
用来解释折线段切换和 `BOX/H` 观察前输入失真。

---

## 与当前粗分类链的关系

这层与当前粗分类链的关系必须固定为：

1. 健康检查先做诊断
2. 粗分类继续只做粗分类
3. 当前阶段不允许健康检查结果直接抬升或压低主类

也就是：

- 不允许 `Audit says H-like => 直接判 H`
- 不允许 `Audit says suspicious => 直接打回 NONE`

否则这层又会变成新的隐藏判定器。

---

## 验收标准

这层首版完成后，至少要满足：

1. 能对最近已知三类 root cause 给出明确诊断：
   - 轴线劫持
   - 候选集失守
   - trace 几何失真
2. 当粗分类异常时，能先输出“问题所在层级”
3. 不引入新的运行时借力口子
4. 仍保持：
   - 粗分类主链不吃 proof
   - 粗分类主链不吃最终家族
   - 健康检查只做 sidecar

---

## 当前建议的直接下一步

1. 先把本方案挂入 V2 状态板和任务板，作为新的活跃主题。
2. 第一轮实现只落：
   - member-level summary
   - 三个首批检查项
3. 用当前已知样本做对照验证：
   - `T3-2GL-53 / 55`
   - `T2-13GL-9 / 10 / 16 / 21 / 24`
   - `T2-13GL-23`
4. 确认这层能先报出根因，再决定是否继续扩到 station/detail 层。
