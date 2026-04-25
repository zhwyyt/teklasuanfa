# 稳定区与截面站位求解设计

## 文档目的

本文档定义阶段 3 的两个核心模块：

- `StableBodyZoneResolver`
- `SectionStationPlanner`

目标不是直接判 `H / BOX / T`，而是先回答：

- 主体沿主轴的哪些长度区间适合用来判主截面
- 哪些端部或局部复杂区应被剔除
- 后续横截面重建应在哪些站位取样

---

## 一、设计背景

当前旧主体识别器的主要问题之一，是把端板、耳板、局部连接件和端部复杂构造，与真正的主截面稳定区混在一起。

阶段 2 已经把“主体候选分层”从“识别输入”里拆出第一版，但还没有解决一个更底层的问题：

- 即使某零件进入了 `body_candidate`
- 也不代表它在整个长度方向上都适合参与主截面判定

因此阶段 3 的职责是：

1. 在长度方向上建立“主体稳定区”
2. 把端部复杂区、局部构造密集区从主截面判定里切出去
3. 为阶段 4 的横截面迹线重建提供可解释的站位集合

---

## 二、输入与输出

## 2.1 输入

- `BodyCandidatePartitionResult`
- `PartFeature`
- `PartGraph`
- `AssemblyInput`

需要同时支持两种视图：

- `real_input`
- `recognition_input`

其中：

- `real_input` 用于工程解释和后续定义驱动证明
- `recognition_input` 只作为兼容旧启发式的旁路参考

## 2.2 输出

推荐输出两个文件：

- `stable-body-zone.json`
- `section-stations.json`

---

## 三、稳定区定义

稳定区不是“零件最多的区间”，而是满足以下条件的长度区段：

1. `body_candidate` 在该区段具有稳定持续性
2. `end_connection_candidate / local_stiffener_candidate` 密度不高
3. 主体候选的横截面构成在该区段变化较小
4. 不被构件两端的端板、耳板、底板、连接辅助板主导

换句话说：

- 稳定区是“适合证明主截面”的区间
- 不是“任何存在零件的区间”

---

## 四、求解步骤

## 4.1 先求主轴投影总区间

对 `body_candidate + body_accessory_candidate` 做主轴投影，得到：

- `assembly_axis_interval = [xmin, xmax]`

这里的主轴优先取阶段 2 分层结果中与 `inputMainPartId` 一致、且置信度最高的候选轴；若不稳定，则退回到当前已有的主体临时主轴。

## 4.2 构造长度方向事件点

每个参与长度分析的零件都投影为一个区间：

- `part_interval = [pi_min, pi_max]`

对以下对象分别生成事件点：

- `body_candidate`
- `body_accessory_candidate`
- `end_connection_candidate`
- `local_stiffener_candidate`

## 4.3 形成初始分段

按所有事件点切分主轴区间，得到若干原子小区段。

对每个小区段统计：

- `body_candidate_coverage`
- `body_accessory_density`
- `end_connection_density`
- `local_stiffener_density`
- `tiny_part_density`

## 4.4 剔除端部复杂区

以下情况优先标记为 `end_complex_zone`：

1. 小区段接近主轴两端
2. `end_connection_candidate` 密度高
3. 端板 / 耳板 / 底板 / 垫板占优
4. 局部螺栓主导连接件集中

注意：

- “靠近端部”不能仅按固定毫米数判断
- 应使用相对长度比例 + 构造复杂度双条件

推荐先验：

- 端部初始候选修剪比例：`5% - 12%`
- 再由局部复杂度进行二次扩张

## 4.5 标记局部构造密集区

对非端部区段，如果出现以下情况，也要标记为 `local_complex_zone`：

1. `local_stiffener_candidate` 密度高
2. 主体候选数量剧烈波动
3. 相邻区段横向构成差异很大

这些区段不一定全部剔除，但需要降低其站位优先级。

## 4.6 合并稳定区

保留同时满足下列条件的区段：

1. `body_candidate_coverage` 达标
2. `end_connection_density` 低
3. `local_complexity_score` 低或中

将相邻稳定小区段合并为若干 `stable_body_zones`。

---

## 五、截面站位规划

## 5.1 站位目标

站位不求多，而要求：

1. 覆盖稳定区首、中、尾
2. 避免落在构造突变点上
3. 足够支撑“多数站位满足定义”的判断

## 5.2 布置原则

对每个 `stable_body_zone`：

1. 若区段很短，至少放 `1-2` 个站位
2. 若区段中等，放 `3-5` 个站位
3. 若区段较长，按长度自适应放更多站位，但总数需受上限约束

推荐规则：

- 全装配总站位不小于 `7`
- 不大于 `21`
- 优先均匀分布在最长稳定区

## 5.3 站位过滤

以下点位不应被保留：

1. 紧贴事件点边界
2. 紧贴端部复杂区边界
3. 落在局部构造密集峰值附近

需要为每个站位输出：

- `StationId`
- `AxisPosition`
- `ZoneId`
- `StationKind` (`core / transition / low_priority`)
- `StationReasons`

---

## 六、原因码建议

稳定区原因码建议：

- `HIGH_BODY_CANDIDATE_COVERAGE`
- `LOW_END_CONNECTION_DENSITY`
- `LOW_LOCAL_COMPLEXITY`
- `TRIMMED_BY_END_COMPLEXITY`
- `TRIMMED_BY_LOCAL_COMPLEXITY`
- `LOW_STABLE_ZONE_CONFIDENCE`

站位原因码建议：

- `UNIFORM_ZONE_SAMPLE`
- `ZONE_CENTER_PRIORITY`
- `NEAR_TRANSITION_BOUNDARY`
- `LOW_PRIORITY_COMPLEX_ZONE_SAMPLE`
- `FILTERED_NEAR_EVENT_BOUNDARY`

---

## 七、对典型样本的预期

## 7.1 普通焊接 H 梁

预期：

- 中部大段形成单一稳定区
- 两端端板、连接板主导区被剔除
- 站位主要落在中部

## 7.2 普通箱柱

预期：

- 柱中段形成稳定区
- 底板、柱脚耳板附近被剔除
- 站位覆盖柱中段多个位置

## 7.3 `T3-1HXZ-1`

预期：

- `real_input` 中的 `29616986` 至少能参与稳定区分析
- 耳板、底板、垫块所在端部区被剔除
- `PL30*40` 短板不应主导稳定区

也就是说：

- 阶段 3 要先把“哪里适合判主截面”说清楚
- 而不是继续让端部复杂构造主导主体家族判断

---

## 八、与当前阶段 2 的衔接

阶段 2 已经证明：

- `recognition_input` 与 `real_input` 必须拆开看

因此阶段 3 的默认主路径必须是：

- 先基于 `real_input` 求稳定区与站位

`recognition_input` 只作为：

- 兼容旧识别器的对照视图
- 或用于解释“为什么旧启发式会偏”

---

## 九、阶段 3 的最小可实现闭环

第一版不必追求完美，只要做到：

1. 能输出主轴总区间
2. 能输出端部复杂修剪后的稳定区
3. 能输出 `7-21` 个可解释站位
4. 能在 `T3-1HXZ-1` 上明确看出端部复杂区被剔除

做到这一步，就可以进入阶段 4 的横截面迹线重建原型。
