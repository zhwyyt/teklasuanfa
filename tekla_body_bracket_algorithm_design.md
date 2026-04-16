---
title: "Tekla 焊接板材主体与牛腿识别算法设计说明"
subtitle: "供 Codex 编程实现的详细技术文档"
author: "OpenAI"
date: "2026-04-15"
lang: "zh-CN"
documentclass: article
numbersections: true
toc: true
toc-depth: 3
papersize: a4
fontsize: 11pt
linkcolor: blue
urlcolor: blue
...

# 文档定位

## 目的

本文档用于指导在 Tekla 生态内开发“焊接板材主体识别 + 牛腿识别 + 难度特征抽取”模块，重点服务于以下业务目标：

1. 自动识别由多块焊接板材组成的构件主体，尽量反推其属于焊接 H、焊接箱、焊接 T、十字型、规则变截面或异形主体。
2. 自动识别主体上的牛腿，并将牛腿与加劲板、隔板、端部连接板等相似附属构件区分开来。
3. 为后续的加工复杂度、工价、工时、班组排产、切割工艺决策提供稳定、可解释、可追溯的输入特征。
4. 使 Codex 可以按模块逐步实现，而不是直接生成一段难以维护的大块代码。

## 适用范围

本文档只覆盖以下算法设计内容：

- 以 `Assembly` 为处理单元的预处理流程。
- 焊接板材主体识别算法。
- 牛腿识别算法。
- 置信度与人工复核机制。
- 推荐的数据结构、模块边界、验收方式。

本文档不覆盖以下内容：

- GUI 交互细节。
- 数据库物理表结构。
- ERP/MES 接口细节。
- 具体编程语言实现细节。
- 任何可以直接复制运行的源代码。

## 读者对象

- 架构设计人员
- Codex 提示工程与实现人员
- Tekla 插件开发人员
- 制造工艺工程师
- 测试与验收人员

# 基本原则

## 总体原则

1. 先识别制造语义，再计算复杂度。不要直接按 Tekla 对象类型给构件打总分。
2. 先把构件拆解为“主体 + 主体附属 + 牛腿 + 其他局部件”，再做加工难度建模。
3. 先做可解释的规则系统，再考虑统计校准或机器学习增强。
4. 先保证主体识别和牛腿识别的稳定性，再叠加工艺与成本逻辑。
5. 低置信度对象不强判，进入人工复核。

## 核心技术路线

建议采用以下技术路线：

- 以 `Assembly` 作为制造单元。
- 以 `Part` 作为几何与工艺特征的最小建模对象。
- 以 `Weld`、几何贴合和 `BooleanPart` 关系共同构建零件关系图。
- 对板件主体采用“主轴估计 -> 虚拟长向板链 -> 横截面抽样 -> 截面家族判定 -> 标准型材/规则拼焊反推”的路线。
- 对牛腿采用“附属簇提取 -> 根部区域识别 -> 外伸特征分析 -> 与加劲板/隔板/连接板区分 -> 牛腿实例分割 -> 牛腿难度特征抽取”的路线。

## 不变约束

以下约束建议在系统设计阶段固定：

1. 单个算法任务以一个 `Assembly` 为输入，不跨 assembly 推理。
2. 默认只处理车间制造相关对象，现场临时对象、安装态辅助件尽量排除。
3. 复杂构件允许进入人工复核，不要求首版算法覆盖全部异形场景。
4. 任何阶段都要输出中间解释结果，避免黑盒式结论。

# 术语定义

## 主体相关术语

- **主体 core body**：构成主受力截面、主长度方向、主体外包络或主体闭合轮廓的核心零件集合。
- **主体附属 body accessory**：沿主体长向存在，但不构成主体最小截面解释的零件，例如纵向加劲板、局部补强板等。
- **焊接板材主体 welded-plate body**：主体由两块及以上板件经焊接形成，而不是单一标准型材对象直接表达。
- **主体主轴 body axis**：构件的主长度方向，也是后续横截面抽样的法向参考方向。
- **主体局部坐标系 body coordinate system**：以主体主轴为 `x` 轴，横截面主方向为 `y` 轴，法向叉乘结果为 `z` 轴的局部坐标系。

## 牛腿相关术语

- **牛腿 bracket**：从主体局部区域向外悬挑并形成支承、托座、局部承压或局部连接承托功能的附属单元。
- **根部区域 root zone**：牛腿与主体发生直接附着、焊接或贴合的局部区域。
- **附属簇 appendage cluster**：除主体核心之外，与主体关联的一组局部附加件连通分量。
- **牛腿实例 bracket instance**：经实例分割后，被视为同一个牛腿的板件集合。

## 易混对象定义

- **加劲板 stiffener**：用于提高局部刚度的板件，通常贴附主体表面，外伸有限，通常不形成明显托座面。
- **隔板 diaphragm**：位于主体内部或横向贯通截面的板件，通常不向主体外部形成悬挑。
- **端部连接板 end connection plate**：位于构件端部，用于与其他构件连接的板件，通常伴随孔群。

# 输入、输出与边界

## 输入对象

单次算法运行的最小输入建议为：

- 一个 `Assembly` 标识符。
- 该 assembly 的 main part。
- 该 assembly 的 secondaries。
- 与这些 part 直接关联的 weld、boolean、bolt、坐标系、solid 信息。

## 输出对象

算法主输出建议包含三个层次：

### 主体识别结果

- `body_type`
- `body_family`
- `body_axis`
- `body_cs`
- `core_body_part_ids`
- `body_accessory_part_ids`
- `body_confidence`
- `body_review_required`
- `body_review_reasons`

### 牛腿识别结果

- `bracket_count`
- `bracket_instances`
- `bracket_total_score`
- `bracket_confidence`
- `bracket_review_required`
- `bracket_review_reasons`

### 中间解释层

- `axis_diagnostics`
- `virtual_plate_chains`
- `section_samples`
- `section_family_votes`
- `appendage_clusters`
- `root_zones`
- `classification_reasons`

## 输出 JSON 契约示例

以下字段命名仅用于说明接口契约，不要求最终实现必须完全一致。

```json
{
  "assembly_id": "A-000123",
  "body": {
    "body_type": "WELDED_PLATE_BODY",
    "body_family": "BUILTUP_H",
    "profile_match": {
      "match_type": "BUILTUP",
      "profile_name": null,
      "similarity": 0.91
    },
    "body_axis": [0.998, 0.041, 0.012],
    "body_confidence": 0.88,
    "core_body_part_ids": [101, 102, 103],
    "body_accessory_part_ids": [121, 122],
    "review_required": false,
    "review_reasons": []
  },
  "brackets": {
    "count": 2,
    "instances": [
      {
        "instance_id": "BKT-1",
        "type": "RIBBED_BRACKET",
        "part_ids": [201, 202, 203],
        "score": 7.5,
        "confidence": 0.84
      },
      {
        "instance_id": "BKT-2",
        "type": "RIBBED_BRACKET",
        "part_ids": [204, 205, 206],
        "score": 7.3,
        "confidence": 0.82
      }
    ],
    "total_score": 14.8,
    "review_required": false,
    "review_reasons": []
  }
}
```

# 内部数据结构设计

本节不是代码，而是建议 Codex 实现时采用的中间对象形态。

## `PartFeature`

`PartFeature` 是所有几何推理的基础。不要在识别流程中反复直接读取 Tekla 原始对象，而应先提取到统一结构。

| 字段 | 类型建议 | 说明 |
|---|---|---|
| `part_id` | string/int | 零件标识 |
| `runtime_type` | enum | `ContourPlate` / `Beam` / `PolyBeam` / `BentPlate` / `Other` |
| `is_plate_like` | bool | 是否按板件处理 |
| `is_special_shape` | bool | 是否曲板、折板或其他特殊对象 |
| `material` | string | 材质 |
| `profile_string` | string | Tekla profile 表达 |
| `solid` | geometry handle | 零件实体 |
| `centroid` | vector3 | 质心 |
| `obb_dims` | vector3 | 有向包围盒三轴尺寸 |
| `volume` | float | 体积 |
| `surface_area` | float | 表面积 |
| `thickness` | float | 板厚，型材件可为空 |
| `plate_normal` | vector3 | 板法向 |
| `plate_long_dir` | vector3 | 板长向 |
| `plate_width_dir` | vector3 | 板宽向 |
| `mid_plane` | plane | 板中面 |
| `contour_vertex_count` | int | 轮廓顶点数 |
| `concave_corner_count` | int | 凹角数 |
| `hole_like_feature_count` | int | 孔/槽类特征数 |
| `boolean_cut_count` | int | 切割数量 |
| `boolean_add_count` | int | 加料数量 |
| `shop_weld_degree` | float | 与车间焊关系的强度指标 |
| `site_weld_degree` | float | 与现场焊关系的强度指标 |

## `PartGraph`

`PartGraph` 是零件关系图。

- 节点：`PartFeature`
- 边类型：
  - `WELD_EDGE`
  - `CONTACT_EDGE`
  - `BOOLEAN_EDGE`
  - `BOLT_EDGE`（仅辅助判断，不作为主体识别主边）

每条边至少应包含：

- `edge_type`
- `part_id_a`
- `part_id_b`
- `strength`
- `geom_support`
- `meta`

## `VirtualPlateChain`

虚拟长向板链用于把被拆分成多段的 web、flange、wall 在算法层面合并。

| 字段 | 说明 |
|---|---|
| `chain_id` | 板链标识 |
| `part_ids` | 链包含的板件集合 |
| `mean_normal` | 平均法向 |
| `mean_long_dir` | 平均长向 |
| `mean_transverse_location` | 相对主体横截面的平均位置 |
| `thickness_stats` | 厚度统计 |
| `union_interval_x` | 在主体主轴上的投影并集 |
| `continuity_score` | 连续性评分 |
| `persistence` | 长向持续性 |
| `is_longitudinal_chain` | 是否判为长向板链 |

## `BodyRecognitionResult`

- `body_type`
- `body_family`
- `profile_match`
- `axis_confidence`
- `chain_confidence`
- `section_family_confidence`
- `profile_match_confidence`
- `body_confidence`
- `review_required`
- `review_reasons`

## `BracketInstance`

- `instance_id`
- `part_ids`
- `root_zone`
- `type`
- `plate_count`
- `weld_feature`
- `shape_feature`
- `score`
- `confidence`

# 全局配置与容差

以下阈值必须配置化，不得硬编码在实现中。初值可按下表设置，然后在样本验证后调优。

| 配置项 | 建议初值 | 说明 |
|---|---:|---|
| `axis_angle_tol_deg` | 10 | 板长向与候选主轴的夹角阈值 |
| `normal_parallel_tol_deg` | 10 | 两块板法向平行容差 |
| `transverse_pos_tol_mm` | 8 | 横向位置接近容差 |
| `chain_gap_tol_mm` | 20 | 板链拼接端部间隙容差 |
| `strong_persistence_min` | 0.55 | 主体强持续性阈值 |
| `weak_persistence_min` | 0.30 | 主体连接链阈值 |
| `section_end_trim_ratio` | 0.08 | 两端排除比例 |
| `section_sample_count_min` | 7 | 最少站位数 |
| `section_sample_count_max` | 21 | 最多站位数 |
| `bracket_axis_span_ratio_max` | 0.35 | 牛腿沿主轴跨度占比上限 |
| `bracket_overhang_ratio_min` | 1.20 | 牛腿外伸主导阈值 |
| `manual_review_conf_min` | 0.75 | 人工复核触发阈值 |

# 预处理流程

## 装配对象收集

对一个 assembly，建议执行以下预处理：

1. 读取 main part。
2. 读取所有 secondary part。
3. 收集所有与这些 part 直接相关的 weld。
4. 收集所有与这些 part 相关的 boolean 特征。
5. 视需要收集 bolt group，但仅作为辅助特征。

## 几何标准化

1. 所有点、向量统一到 assembly 的局部参考坐标下。
2. 所有距离单位统一到毫米。
3. 对向量统一归一化。
4. 明确向量比较函数均采用带容差版本。

## 对象初分层

在主体识别之前，应先做对象分层：

- `plate_like_parts`
- `profile_like_parts`
- `special_shape_parts`
- `tiny_parts`
- `likely_connection_parts`

其中：

- `tiny_parts` 只用于后续局部附加判断，不参与主体主轴估计。
- `special_shape_parts` 默认直接打特殊标记，后续若参与主体识别则降低置信度。

# 焊接板材主体识别详细算法

本章是整份文档的核心。推荐实现顺序不是“直接猜主体是 H 还是箱”，而是：

1. 从板件中估计主体主轴。
2. 将分段板件合并为虚拟长向板链。
3. 利用长向持续性初选主体核心。
4. 基于横截面抽样重建主截面迹线。
5. 先判家族，再做标准 profile 或 built-up 反推。
6. 再把纵向加劲板从主体核心中剥离。

## 候选板件池构建

### 进入候选池的条件

一个 part 可进入焊接板材主体候选池，当满足以下条件：

1. `is_plate_like = true`
2. 非明显曲板、折板、lofted 等特殊对象
3. 几何尺度不极端微小
4. 非明显吊耳、工装板、临时板

### 初步排除项

以下对象原则上先排除出主体候选池：

- 位于构件端部且孔群高度集中者
- 面积和体积过小、明显仅起局部连接作用者
- 仅通过少量 bolt 关联而无焊接/贴合支撑者

### 不应过早排除的对象

以下对象不应在这一阶段直接排除：

- 纵向加劲板
- 纵向补强板
- 分段翼缘板
- 分段腹板

原因是它们在早期都可能看起来像“局部件”，但在长向持续性分析后才可分离。

## 板件几何特征提取

### 目标

对每个板件建立稳定的几何描述，为后续主轴估计和板链合并提供基础。

### 需要提取的关键特征

1. `thickness`
2. `plate_normal`
3. `plate_long_dir`
4. `plate_width_dir`
5. `mid_plane`
6. `centroid`
7. `obb_dims`
8. `outline complexity`

### `plate_long_dir` 计算建议

对板件中面轮廓投影到板中面后做 2D PCA，第一主方向作为 `plate_long_dir`。若板形高度接近正方，则降级使用有向包围盒长边方向。

### 稳定性要求

`plate_long_dir` 与 `plate_normal` 必须严格正交化，避免后续角度判定抖动。

## 主体主轴估计

### 为什么不能只信 main part

对于焊接板材主体，Tekla main part 有时只是其中一块板，并不可靠地代表整体主轴。因此主轴估计必须基于候选板件整体统计。

### 方向聚类法

对每个候选板件：

1. 取 `plate_long_dir`，忽略正负号差异。
2. 定义权重 `w = area * log(aspect_ratio + 1)`。
3. 在方向空间做加权聚类。
4. 选取总权重最高的方向簇作为 `axis_candidate_1`。

### PCA 兜底法

若方向聚类不稳定，则对候选板件质心做加权 PCA，第一主方向作为 `axis_candidate_2`。

### 主轴选定策略

推荐策略如下：

1. 若主方向簇权重大于总权重的 55%，优先采用方向聚类结果。
2. 否则若 PCA 第一特征值明显大于第二特征值，采用 PCA 结果。
3. 若两者均不稳定，直接标记 `body_axis_low_confidence`。

### 主轴置信度

建议定义两个指标：

- `axis_cluster_support`
- `axis_pca_support`

综合后得到：

`axis_confidence = max(axis_cluster_support, axis_pca_support) * consistency_factor`

其中 `consistency_factor` 用于惩罚候选板之间角度分散过大的情况。

## 虚拟长向板链构建

### 设计动机

一根焊接 H 构件的腹板和翼缘板常被拆成多段建模。若以单块板作为主体判定单位，则主体会被误判为许多局部板件。因此必须先建立“虚拟长向板链”。

### 两块板属于同一板链的条件

两块板 `p` 与 `q` 可并入同一板链，当大部分以下条件成立：

1. 两者 `plate_long_dir` 与主体主轴夹角均足够小。
2. 两者法向近似平行。
3. 两者厚度相同或接近。
4. 两者在横截面平面内的相对位置接近。
5. 两者在主轴方向上的投影区间相接、相交或间隙很小。
6. 两者在 `PartGraph` 中通过焊缝边或接触边可连通。

### 合并逻辑

推荐采用“先聚类后连通”的两阶段合并：

第一阶段：按法向、厚度、横向位置做粗聚类。

第二阶段：在每个粗聚类内，根据主轴投影区间相邻性和关系图连通性求连通分量，生成一条或多条板链。

### 板链连续性评分

建议：

`continuity_score = coverage_ratio * connectivity_score * direction_consistency * thickness_consistency`

其中：

- `coverage_ratio`：板链覆盖的主轴总长度 / assembly 主体候选总长度
- `connectivity_score`：链内边强度平均值
- `direction_consistency`：各板与链平均方向的一致度
- `thickness_consistency`：厚度离散度的反向评分

## 主体局部坐标系构建

### 构建方法

1. 取构件中部站位。
2. 在该站位上让候选板链与截面平面求交。
3. 将交点投影到垂直主轴的平面中。
4. 对投影点做 2D PCA。
5. 取第一主方向为 `y`，`z = x × y`。

### 兜底策略

若 2D PCA 不稳定，则：

1. 优先用板法向聚类主方向构造 `y`。
2. 再不稳定则借用 main part 坐标系。
3. 最终仍不稳定则设置 `body_cs_low_confidence`。

## 长向持续性分析

### 主体原始长度定义

将所有候选长向板链在主轴上的投影做并集，得到 `L_body_raw`。

### 单条板链持续性

定义：

`persistence = covered_length(chain) / L_body_raw`

### 初选规则

推荐规则：

- `persistence >= 0.55`：主体核心强候选
- `0.30 <= persistence < 0.55`：主体连接链候选
- `< 0.30`：更像局部附加件

### 重要说明

长向持续性只做“初选”，不能直接据此确定主体核心。纵向加劲板也可能具有较高持续性，必须依赖截面抽样进一步判断。

## 横截面抽样与迹线重建

### 抽样站位策略

1. 跳过两端各 5% 到 10% 的长度，以排除端部连接构造干扰。
2. 在中间区间按长度均匀布置 7 到 21 个站位。
3. 长构件采用更多站位，短构件采用较少站位。

### 单站位求交逻辑

对每个站位平面 `P_k`：

1. 让主体候选 part solid 与 `P_k` 求交。
2. 把求交结果投影到 `body_cs` 的 `y-z` 平面。
3. 对每个板件生成一条或多条“截面迹线”。

### 迹线清洗

对站位截面进行清洗：

1. 合并近共线、近重合迹线。
2. 删除长度极短、显然为噪声的迹线。
3. 标记外包络迹线和内部附加迹线。
4. 记录参与该站位主截面的板链来源。

### 为什么采用迹线而非体素化

对于薄板构件，横截面识别的本质是“哪些板在截面上形成主导线段与闭环”，因此以迹线为核心比体素化或体网格方法更高效、更可解释、更容易调试。

## 主体截面家族识别

本节先识别主体属于什么家族，再做 profile 反推。

### 焊接 H / I 识别规则

在多数抽样站位上满足以下条件时，优先判为焊接 H / I：

1. 主截面由三类主迹线解释：一条腹板，两条翼缘。
2. 腹板与翼缘近似正交。
3. 两条翼缘分居腹板两侧。
4. 三类主迹线在大多数站位保持拓扑一致。
5. 去掉局部小加劲板后，截面解释不变。

可进一步细分为：

- 规则焊接 H / I
- 变截面焊接 H / I
- 非对称焊接 H / I

### 焊接箱识别规则

在多数站位上满足以下条件时，优先判为焊接箱：

1. 主迹线构成闭合或近闭合环。
2. 存在四条主壁迹线或可归约为四壁闭合关系。
3. 去掉内部加劲肋后，闭环仍存在。
4. 大多数站位的闭合拓扑一致。

可进一步细分为：

- 规则焊接箱
- 变截面焊接箱
- 多腔箱体

### 焊接 T 识别规则

在多数站位上满足以下条件时，优先判为焊接 T：

1. 可由一条腹板和一条主翼缘解释。
2. 翼缘只位于腹板一侧。
3. 截面拓扑在长度方向稳定。

### 十字型 / 组合型识别规则

若多数站位上存在两条相互正交且稳定持续的腹板主迹线，且由此构成十字或规则组合截面，则可判为十字型或组合型主体。

### 异形主体判定规则

以下任一情况可直接判为异形主体：

1. 不同站位的主截面拓扑变化过大。
2. 既无法稳定匹配 H、箱、T、十字，也无法归约为规则组合截面。
3. 主体解释严重依赖多个局部小迹线才能勉强成立。
4. 曲板、折板、扭转构件深度参与主体形成。

## 标准型材 / 规则拼焊 / 异形反推

### 家族优先，尺寸其次

反推顺序必须是：

1. 先确定家族。
2. 再提取家族参数。
3. 最后才与标准型材目录比对。

### 家族参数提取

建议在稳定站位上取统计中位数：

- H / I：`h, b, tw, tf`
- 箱：`h, b, t1, t2, t3, t4`
- T：`h, b, tw, tf`
- 十字：两腹板厚度、总高度、总宽度

### profile 比对流程

1. 按家族过滤 profile 目录。
2. 按主尺寸过滤候选项。
3. 按轮廓相似度排序。
4. 取最优匹配和次优匹配。
5. 若最优与次优差距不足，则降低置信度。

### 输出规则

- 家族稳定，尺寸落在标准目录内，且相似度高：输出具体 `profile_name`
- 家族稳定，但尺寸不属于标准目录：输出 `BUILTUP_H / BUILTUP_BOX / BUILTUP_T`
- 家族不稳定：输出 `IRREGULAR`

## 主体附属件剥离

### 为什么必须剥离

纵向加劲板和补强板常与主体一样沿主轴持续很长，若不剥离，将导致主体板数虚高、截面识别复杂化，甚至误判为异形。

### 剔除重算法

对每条高持续性的候选板链执行：

1. 临时从主体候选集合中移除该板链。
2. 重算若干关键站位的截面家族。
3. 比较移除前后：
   - 家族是否变化
   - 尺寸是否显著变化
   - 稳定性是否显著变化
4. 若变化很小，则该板链不属于主体核心，应转入 `body_accessory_parts`。

### 附属件输出

即使被剥离，也必须保留该板链的输出，以供后续复杂度计算、切割工艺判断和加工计划使用。

## 主体识别置信度与人工复核

### 置信度构成

建议拆分为：

- `axis_confidence`
- `chain_confidence`
- `section_family_confidence`
- `profile_match_confidence`

综合可采用加权平均：

`body_confidence = 0.25 * axis + 0.20 * chain + 0.35 * section + 0.20 * profile`

### 触发人工复核的情形

以下情况建议直接进入人工复核：

1. 主轴不稳定。
2. 不同站位的家族投票不一致。
3. 同时接近两个家族，例如既像箱又像组合异形。
4. 标准 profile 存在多个近似候选。
5. 特殊板件深度参与主体构成。
6. 核心板链数量异常多。

# 牛腿识别详细算法

牛腿识别的重点不是“有一块板伸出来就算牛腿”，而是“局部附着 + 向外悬挑 + 具备承托意义”。

## 牛腿识别输入域

牛腿识别只在以下对象上执行：

`appendage_candidates = all_parts - core_body_parts - irrelevant_parts`

其中 `irrelevant_parts` 可包括：

- 工装件
- tiny 零件
- 明显只属于端部连接系统的局部件

## 附属簇提取

### 连通分量分组

在 `appendage_candidates` 上构建子图，边优先使用：

1. 车间焊边
2. 几何贴合边
3. 必要时少量使用布尔关联边

对该子图求连通分量，每个连通分量定义为一个 `appendage_cluster`。

### 附属簇的基本统计

每个附属簇至少输出：

- `cluster_id`
- `part_ids`
- `connected_body_part_ids`
- `cluster_bbox`
- `part_count`
- `total_plate_area`
- `total_weld_length_to_body`

## 根部区域识别

### 根部区域定义

根部区域是附属簇中直接与主体接触、焊接或贴合的局部区域。它是识别牛腿是否“挂”在主体上以及挂在哪里的关键。

### 根部识别步骤

对每个附属簇：

1. 找出直接与主体相连的 part，记为 `direct_attach_parts`。
2. 对每个 `direct_attach_part`，寻找与主体接触或焊接最强的面组。
3. 将这些面组投影到主体局部坐标系中。
4. 合并空间上相近的附着 patch，形成一个或多个 `root_zone`。

### 根部区域的输出字段

- `root_centroid`
- `root_x_interval`
- `root_contact_area`
- `root_weld_length`
- `root_surface_normal`
- `attached_body_face_type`，例如 `WEB`、`FLANGE_TOP`、`FLANGE_BOTTOM`、`BOX_WALL`、`CORNER`

## 外伸特征计算

### 计算目的

用于区分“局部外挑的牛腿”和“贴附主体表面的加劲板”。

### 关键指标

对每个附属簇或候选牛腿实例，计算：

- `span_x`
- `span_y`
- `span_z`
- `span_perp = sqrt(span_y^2 + span_z^2)`
- `centroid_offset_from_body_surface`
- `cantilever_ratio = span_perp / max(span_x, root_width, body_wall_ref)`

### 直观判别逻辑

- 牛腿：通常外伸主导，且沿主轴长度有限
- 加劲板：通常贴附主体现长，外伸有限
- 隔板：通常横向贯通主体，不形成外部悬挑

## 牛腿与相似对象的区分规则

## 判定为牛腿的必要条件

建议同时满足以下多数条件：

1. 与主体存在局部根部附着，而不是长边整体贴附。
2. 在主体外法向方向上存在明显外伸。
3. 沿主轴方向长度相对有限。
4. 不贯穿主体截面内部。
5. 不属于明显端部连接系统。
6. 几何上形成承托、支座或局部悬臂语义。

## 判定为加劲板的规则

更像加劲板时，通常具有以下特征：

- 外伸量小
- 与主体连接边很长
- 主要贴附在 web 或 wall 上
- 不存在明显托座面
- 轮廓简单且薄高比偏大

## 判定为隔板的规则

更像隔板时，通常满足：

- 位于主体内部或横向贯通
- 与主体两侧或多侧同时接触
- 重心位于主体包络内或附近
- 不形成外部悬挑

## 判定为端部连接板的规则

更像端部连接板时，通常满足：

- 位于主体端部排除区附近
- 存在明显孔群、长圆孔或多孔连接模式
- 几何功能更像连接其他构件，而不是承托本体

## 牛腿实例分割

一个附属簇不一定只对应一个牛腿实例。例如左右对称牛腿、成对牛腿或一个牛腿加局部连接板，都可能先落在同一个附属簇内。

### 根部种子分组

先按以下维度对 `direct_attach_parts` 做根部种子分组：

- 根部在主轴上的站位接近
- 外法向方向接近
- 根部重心距离接近

### 从种子向外扩张

以每个根部种子组为起点：

1. 向外沿焊缝边和接触边扩张。
2. 优先吸收外伸方向一致的板件。
3. 若扩张路径跨越到另一个明显独立根部，则停止扩张。

### 合并条件

两个局部组可合并为同一牛腿实例，当且仅当：

1. 根部站位几乎相同。
2. 根部法向一致。
3. 组间连接强。
4. 合并后更符合“一个承托单元”的几何语义。

## 牛腿子类型识别

### 简单板式牛腿

典型特征：

- 一块主板为主
- 最多附带少量小肋
- 轮廓简单
- 焊接量较低

### 带肋牛腿

典型特征：

- 一块托座板或侧板
- 配 1 到多块肋板、三角板或支撑板
- 焊接边数量明显增加

### 箱式牛腿

典型特征：

- 多块板形成半闭合或闭合单元
- 可能存在封板
- 焊接可达性更差
- 制造与组装难度显著提高

### 斜撑 / 异形牛腿

典型特征：

- 轮廓为斜边、梯形、三角或多折线
- 常伴随异形切割
- 受力和制造都更复杂

## 牛腿特征抽取

每个牛腿实例都应提取一组稳定特征，用于后续难度评分、切割路径决策和班组排产。

### 构成特征

- `plate_count`
- `thickness_set`
- `is_closed_box`
- `root_count`
- `symmetry_group_id`

### 尺寸特征

- `bracket_span_out`
- `bracket_span_x`
- `bracket_span_y`
- `bracket_span_z`
- `root_area`
- `support_face_area`

### 轮廓特征

- `outer_contour_vertex_count`
- `concave_corner_count`
- `slot_count`
- `arc_edge_count`
- `irregular_outline_flag`

### 孔槽与布尔特征

- `hole_group_count`
- `special_hole_count`
- `boolean_cut_count`
- `boolean_add_count`

### 焊接特征

- `shop_weld_count`
- `shop_weld_total_length`
- `around_weld_flag`
- `large_weld_flag`
- `restricted_access_flag`

## 牛腿难度评分建议

牛腿难度不应只按数量，而应按实例累计。

### 基础分

按子类型设基础分：

- 简单板式：低基础分
- 带肋：中基础分
- 箱式：高基础分
- 异形：高基础分

### 修正项

对每个牛腿实例按以下项叠加：

- 板件数修正
- 异形轮廓修正
- 孔槽修正
- 特殊切割修正
- 焊缝长度修正
- 焊接可达性修正
- 角部安装位置修正
- 与相邻牛腿干涉风险修正

### 总分

建议：

`bracket_total_score = sum(score(instance_i))`

同时输出：

- `bracket_count`
- `dominant_bracket_type`
- `bracket_difficulty_grade`

## 牛腿识别置信度与人工复核

### 置信度建议分项

- `root_confidence`
- `overhang_confidence`
- `classification_confidence`
- `instance_split_confidence`

### 触发人工复核的典型场景

1. 同一附属簇既像牛腿又像端部连接系统。
2. 根部区域存在多个强附着点，实例分割不稳定。
3. 牛腿与大尺寸加劲板边界模糊。
4. 箱式牛腿与局部箱形封板结构难以区分。
5. 模型缺少焊缝关系，需要几何贴合兜底过多。

# 主体与牛腿之间的边界规则

这是实现中最容易争议的部分，建议固化成明确规则。

## 必须归入主体核心的对象

- 构成稳定主截面的 web、flange、wall 板链
- 去掉后主体家族改变的板件
- 构成主体外包络或闭合箱壁的板件

## 必须归入主体附属的对象

- 纵向加劲板
- 长向补强板
- 贴附主体表面的覆板或衬板

## 优先归入牛腿的对象

- 在主体外侧形成局部悬挑并有承托意义的板件簇
- 由托板、肋板、封板构成的局部承托单元
- 外挑箱式局部支座

## 必须优先排除为非牛腿的对象

- 端部连接板
- 隔板、横隔板
- 单纯局部加劲板
- 吊耳、耳板、工装板

# 推荐给 Codex 的模块拆分

为降低一次生成失败的风险，建议严格按模块递进实现。

## 模块 M1：几何特征抽取器

输入：Tekla 原始对象集合。

输出：`PartFeature[]`。

验收标准：

- 能稳定得到板厚、法向、长向、质心、包围盒。
- 板件分段模型不会导致长向识别崩溃。

## 模块 M2：关系图构建器

输入：`PartFeature[]` 与 Tekla 关系对象。

输出：`PartGraph`。

验收标准：

- weld 边和 contact 边可同时存在。
- 缺少 weld 时，几何贴合仍可补出合理连接。

## 模块 M3：主体主轴与板链模块

输入：候选板件池。

输出：`body_axis`、`VirtualPlateChain[]`。

验收标准：

- 分段腹板能被合并为一条虚拟板链。
- 与主体无关的短板不被误并入长向板链。

## 模块 M4：主体截面分类模块

输入：`body_axis`、`VirtualPlateChain[]`。

输出：主体家族及截面诊断数据。

验收标准：

- 焊接 H、焊接箱的识别率明显高于随机基线。
- 小加劲板不会改变主体家族判断。

## 模块 M5：profile / built-up 反推模块

输入：主体家族与稳定站位参数。

输出：标准 profile / built-up / irregular。

验收标准：

- 标准截面能匹配到具体 profile。
- 非标准规则主体能落到 built-up 分类。

## 模块 M6：牛腿聚类与分类模块

输入：`appendage_candidates`。

输出：`BracketInstance[]`。

验收标准：

- 牛腿与加劲板、隔板、端部连接板可分开。
- 多板组合牛腿能被识别为一个实例。

## 模块 M7：置信度与复核模块

输入：主体与牛腿识别中间结果。

输出：置信度和人工复核原因码。

验收标准：

- 低质量模型不会被强行给出高置信度结论。
- 原因码能直接指导人工修正。

# 测试样本与验收方案

## 最小测试集

建议至少准备以下 8 类样本，每类 10 个以上：

1. 三板焊接 H，无牛腿。
2. 三板焊接 H，腹板或翼缘分段。
3. 焊接箱，含纵向加劲板。
4. 变截面焊接 H。
5. 焊接箱柱，带两个带肋牛腿。
6. 焊接 H 柱，带单板牛腿与若干加劲板。
7. 端部连接板复杂但无牛腿。
8. 明显异形主体。

## 建议指标

### 主体识别指标

- `body_family_accuracy`
- `core_body_part_precision`
- `core_body_part_recall`
- `profile_match_accuracy`
- `manual_review_rate`

### 牛腿识别指标

- `bracket_count_accuracy`
- `bracket_instance_f1`
- `bracket_type_accuracy`
- `false_bracket_rate`
- `review_trigger_precision`

## 分阶段验收目标

- 第 1 阶段：标准焊接 H / 箱主体识别稳定。
- 第 2 阶段：纵向加劲板与主体核心剥离稳定。
- 第 3 阶段：牛腿与加劲板区分稳定。
- 第 4 阶段：built-up 与 irregular 反推稳定。

# 性能与鲁棒性要求

## 性能建议目标

- 对典型装配单元，平均分析时间不高于数秒级。
- 主体截面抽样数量可动态调节，避免过密采样。
- 中间几何结果应支持缓存，避免重复求交。

## 鲁棒性要求

- 对缺失 weld 的模型，允许用几何贴合兜底，但必须降低置信度。
- 对端部局部构造极复杂的模型，允许只识别中部稳定截面。
- 对特殊件参与较多的模型，应优先触发人工复核。

# 建议的原因码体系

建议统一输出机器可读原因码，便于日志、复核界面和统计分析。

## 主体原因码

- `BODY_AXIS_LOW_CONF`
- `BODY_CHAIN_FRAGMENTED`
- `BODY_SECTION_INCONSISTENT`
- `BODY_PROFILE_AMBIGUOUS`
- `BODY_SPECIAL_SHAPE_IN_CORE`
- `BODY_IRREGULAR_TOPOLOGY`

## 牛腿原因码

- `BRACKET_ROOT_AMBIGUOUS`
- `BRACKET_OVERHANG_WEAK`
- `BRACKET_VS_STIFFENER_AMBIGUOUS`
- `BRACKET_VS_ENDPLATE_AMBIGUOUS`
- `BRACKET_INSTANCE_SPLIT_UNSTABLE`
- `BRACKET_GEOMETRY_INCOMPLETE`

# 实施建议

## 首版实现建议

首版不要追求覆盖所有异形件，而应优先做准以下场景：

1. 焊接 H
2. 焊接箱
3. 简单牛腿
4. 带肋牛腿
5. 加劲板与牛腿的基础区分

## 第二阶段建议

在首版稳定后，再加入：

- 变截面主体
- 多腔箱体
- 箱式牛腿
- 端部复杂连接系统排除
- 不同工厂规则的阈值配置化

## 中间解释层不可省略

实现时必须落盘或缓存以下中间结果：

- 主轴估计过程数据
- 虚拟板链列表
- 每个站位的截面迹线
- 主体家族投票过程
- 牛腿附属簇、根部区域和实例分割过程

没有解释层，后续调试和规则修正成本会非常高。

# 对 Codex 的使用建议

给 Codex 提需求时，不要直接要求“一次写完所有算法”。建议按模块下发，例如：

1. 先写 `PartFeature` 提取与 `PartGraph` 构建。
2. 再写主轴估计和 `VirtualPlateChain` 逻辑。
3. 再写截面抽样与主体家族分类。
4. 再写牛腿附属簇、根部区域和实例分割。
5. 最后写置信度与原因码输出。

每一轮都要求 Codex：

- 输出模块边界
- 输出输入输出契约
- 输出单元测试样例设计
- 不要跳过异常路径

# 附录：与 Tekla API 的映射建议

本算法文档在实现层建议主要依赖以下对象族：

- `Assembly`：作为制造单元入口。
- `Part`：作为零件、实体和用户属性的统一入口。
- `ContourPlate`：板件主体的主要来源对象。
- `Weld` / `BaseWeld` / `PolygonWeld`：用于建立装配关系和焊接特征。
- `BooleanPart`：用于识别切割与加料特征。
- `Solid`：用于求实体、包围盒和截面求交。
- `CatalogHandler` / `ProfileItem`：用于 profile 目录比对。

建议优先把 API 调用隔离在“数据访问层”，不要在主体算法中到处直接依赖 Tekla 原始对象。这样可以显著降低调试复杂度，并利于后续离线回放测试。

# 附录：官方 API 参考（用于实现时核对）

- Assembly.GetMainPart: https://developer.tekla.com/doc/tekla-structures/2026/get-main-part-method-70725
- Assembly.GetSecondaries: https://developer.tekla.com/doc/tekla-structures/2026/get-secondaries-method-70726
- Part.GetAssembly: https://developer.tekla.com/doc/tekla-structures/2026/get-assembly-method-72109
- Part.GetWelds: https://developer.tekla.com/doc/tekla-structures/2026/get-welds-method-72125
- Part.GetSolid: https://developer.tekla.com/doc/tekla-structures/2026/get-solid-method-forming-states-72121
- ContourPlate Class: https://developer.tekla.com/doc/tekla-structures/2025/contour-plate-class-52321
- Solid.GetAllIntersectionPoints: https://developer.tekla.com/doc/tekla-structures/2025/get-all-intersection-points-method-53813
- Solid Class: https://developer.tekla.com/doc/tekla-structures/2025/solid-class-53808
- CatalogHandler.GetProfileItems: https://developer.tekla.com/doc/tekla-structures/2026/get-profile-items-method-65388
- BooleanPart.Type: https://developer.tekla.com/doc/tekla-structures/2026/type-property-71079
- BooleanPart.OperativePart: https://developer.tekla.com/doc/tekla-structures/2026/operative-part-property-71078
- Weld Properties: https://developer.tekla.com/doc/tekla-structures/2026/weld-properties-73014

