# 主体候选分层器设计

## 文档目的

本文档定义“主体候选分层器”的职责、输入输出与判定逻辑。

它的目标不是识别 H / BOX / T，而是先回答：

- 哪些真实零件有资格进入主体分析
- 哪些零件应被视为主体附属候选
- 哪些零件更像端部连接件、局部构造件、小件或特殊件

只有这一层稳定之后，后续的主轴、截面与主材证明才有可能稳定。

---

## 一、模块定位

推荐模块名：

- `BodyCandidatePartitioner`

位于：

- 几何特征抽取之后
- 主轴估计之前

输入：

- `AssemblyInput`
- `PartFeature`
- 当前已有 `RelationshipInput`
- 上游缓存提供的角色候选（如有）

输出：

- `BodyCandidatePartitionResult`

---

## 二、输出分层

每个真实零件必须被分配到以下至少一类：

### 1. `body_candidate`

可能属于主体核心或主体主轮廓的零件。

### 2. `body_accessory_candidate`

沿主体长向存在，但更像加劲板、覆板、衬板、补强板的零件。

### 3. `end_connection_candidate`

更像端板、耳板、连接板、连接辅助板的零件。

### 4. `local_stiffener_candidate`

局部性明显的短板、加劲板、局部隔板。

### 5. `tiny_part`

尺度过小、制造语义明显为局部件的对象。

### 6. `special_shape`

曲板、折板、特殊型对象。

### 7. `unknown`

无法稳定归类的对象。

---

## 三、分层原则

## 3.1 进入 `body_candidate` 的必要倾向

零件具备以下多数特征时，才允许进入 `body_candidate`：

1. 沿主轴方向具有显著长度或覆盖率
2. 与主轴近似平行或能稳定参与主截面
3. 非明显端部局部件
4. 非明显小件
5. 与主体区域存在稳定焊接 / 接触 / 贴合关系

## 3.2 不得过早排除的对象

以下对象即便暂时不确定，也应至少进入主体分析候选或附属候选，不可直接删掉：

- 分段腹板
- 分段翼缘板
- 分段壁板
- 长向加劲板
- 长向补强板

## 3.3 应优先下沉到局部候选的对象

以下对象优先下沉，而非直接进入主体候选：

- 端板
- 耳板
- 马板
- 局部补板
- 明显短区段加劲板
- 螺栓主导连接件

---

## 四、判定因子

分层器建议使用以下因子，但输出必须带原因码，而非黑盒总分。

### A. 长向持续性

- 主轴投影长度
- 相对总长度覆盖率
- 是否只位于端部局部区

### B. 截面参与潜力

- 是否为板件
- 板法向是否稳定
- 是否可能构成主轮廓

### C. 局部性

- 是否靠近构件端部
- 是否长度很短
- 是否明显只覆盖少量局部区

### D. 连接语义

- 焊接主导还是螺栓主导
- 是否仅通过少量局部连接附着

### E. 角色先验

若上游缓存已有：

- `web_candidate`
- `flange_candidate`
- `wall_candidate`
- `endplate_candidate`
- `stiffener_candidate`

则可以作为先验，但不可直接替代最终分层。

---

## 五、原因码建议

每个零件的分层结论必须输出原因码。

推荐原因码：

- `LONGITUDINAL_PRIMARY_PLATE`
- `LIKELY_MAIN_SECTION_PART`
- `LIKELY_BODY_ACCESSORY_LONGITUDINAL`
- `LIKELY_END_CONNECTION`
- `LIKELY_LOCAL_STIFFENER`
- `TOO_SMALL_FOR_BODY`
- `SPECIAL_SHAPE_PART`
- `BOLT_DOMINATED_LOCAL_PART`
- `NEAR_END_LOCAL_PART`
- `LOW_CONFIDENCE_PARTITION`

---

## 六、对当前问题样本的预期行为

## 6.1 对普通焊接 H 梁

预期：

- 腹板、翼缘进入 `body_candidate`
- 长向加劲板进入 `body_accessory_candidate`
- 端板进入 `end_connection_candidate`

## 6.2 对普通箱柱

预期：

- 四块主壁板进入 `body_candidate`
- 内部长向加劲板进入 `body_accessory_candidate`
- 局部耳板 / 端板进入 `end_connection_candidate`

## 6.3 对 `T3-1HXZ-1`

预期：

- `29616986` `PL40*360` 至少进入 `body_candidate`
- `29617226/29617238/29617250/29617262` 也可进入主体分析，但需标记为“待截面证明”
- 不能仅凭它们长度较长就直接把它们全部当成主体核心

换句话说：

- 分层器负责“让它们进入正确池子”
- 不负责直接给出 `H / BOX`

---

## 七、输出契约建议

推荐文件：

- `body-candidate-partition.json`

每个零件输出：

- `PartId`
- `PartName`
- `ProfileString`
- `PartitionClass`
- `PartitionConfidence`
- `PartitionReasons`
- `UpstreamRoleHint`
- `NearStableZone`
- `LongitudinalCoverageEstimate`

---

## 八、验收标准

本模块通过验收的标志是：

1. 端板、耳板、马板不再大面积污染主体候选池
2. 长向加劲板不会被粗暴删掉
3. `HXZ` / `GKZ` 这类边界样本中的长板能进入主体分析，但不会被直接强判为主体核心
4. 任意一个零件都能解释为什么分到当前类别

---

## 九、与当前实现的关系

当前 `BodyRecognizer` 中已有一部分候选筛选逻辑，但仍然过于粗糙：

- 更接近“哪些件先拿来投票”
- 还不是严格意义上的主体候选分层器

后续建议：

1. 先单独实现 `BodyCandidatePartitioner`
2. 再让 `BodyRecognizer` 逐步改为消费该分层结果
3. 不再在 `BodyRecognizer` 内部同时承担“候选分层 + 家族判定”两类职责
