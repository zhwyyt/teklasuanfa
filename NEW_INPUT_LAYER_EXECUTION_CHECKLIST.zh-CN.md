# 新输入层执行清单

## 文档定位

本文档用于定义“新输入层”预研的执行清单。

当前定位：

- `V3` 候选主题
- 预研执行面
- 不替代当前 V2 活跃真源

也就是说：

1. 当前活跃真源仍然是
   - [PROJECT_STATUS_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_STATUS_V2.zh-CN.md>)
   - [PROJECT_TASKLIST_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_TASKLIST_V2.zh-CN.md>)
2. 本文档只负责回答：
   - 新输入层要解决什么
   - 先做哪些事
   - 何时允许升格为正式 `V3`
   - 如何保证不把当前主线搞坏

---

## 是否作为 V3

当前建议：

- 先不要立刻把它命名成正式 `V3`
- 先按“`V3` 候选 / 预研”推进

原因：

1. 当前 V2 主题“基础健康检查层”刚刚接上 collector / workflow，仍处于首轮验证阶段。
2. 新输入层会动到更上游的表达方式，影响范围明显大于当前 health-audit sidecar。
3. 在没有冻结：
   - 输入表达目标
   - 对照样本
   - 切换门槛
   - 回退策略
   之前，直接升格成正式 `V3` 太早。

升格条件：

只有当下面四项都完成时，才建议正式升格为 `V3`：

1. 新输入层表达契约冻结
2. 旧链 / 新链 A/B 对照样本通过
3. 关键异常样本在新链上确实更稳
4. 已明确默认入口切换策略与回退策略

---

## 新输入层目标

新输入层的目标不是“再加一层下游判定器”，而是：

1. 把 Tekla 深化模型导出成更强语义、更少失真的中间表达
2. 尽量减少：
   - 主轴劫持
   - 候选集漏主板
   - trace 方向漂移
   - 局部几何把整体关系带歪
3. 为后续：
   - candidate
   - station
   - trace
   - topology
   提供更稳定的输入基座

一句话：

- 当前链路是在“摘要几何”上做翻译
- 新输入层要把“摘要几何”尽量升级成“更接近工程语义的参数表达”

---

## 建议表达方向

新输入层优先统一为两类基础表达：

### 1. 路径 + 截面 sweep

适用对象：

- Beam / PolyBeam 类长向主体
- 可稳定抽取 longitudinal path 的构件

目标：

1. 主轴来自路径本身，而不是靠局部 guide 猜
2. 任意 station 的截面来自参数化 sweep 求交
3. 降低“短附件 / 短腹板 / 局部更直小件”劫持主轴的风险

### 2. 边界 + 厚度 plate

适用对象：

- ContourPlate
- BentPlate
- 其它板件主体

目标：

1. 板件以“边界 + 厚度 + 局部坐标系”表达
2. 不再只靠局部 edge cloud 再反推出代表线
3. 为后续主板 / 腹板 / 翼缘 / 壁板组织关系提供更稳的输入

---

## 当前不做的事

新输入层预研阶段，明确不做：

1. 不直接改 `BOX / H / PRIMARY_PLATE_BODY` 判定门槛
2. 不直接改 proof / family 主链
3. 不让新输入层结果直接覆盖当前默认入口
4. 不在没有 A/B 对照的情况下，把新输入层写成唯一真源

---

## 分阶段执行清单

## 阶段 0：边界冻结

目标：

- 先把新输入层的边界和成功标准说清楚

完成项：

1. 明确新输入层只负责输入表达，不直接判主类
2. 明确两类目标表达：
   - `路径 + 截面 sweep`
   - `边界 + 厚度 plate`
3. 明确旧链与新链并行验证，不直接替换
4. 明确首批对照样本

完成标准：

- 后续讨论不再把“新输入层”误解成“重写整个下游识别器”

---

## 阶段 1：输入表达契约草案

目标：

- 定义新输入层最小输出契约

至少包含：

1. member-level longitudinal path
2. part-level boundary / thickness / local frame
3. part-level axis projection / coverage
4. station-ready section query inputs
5. 明确哪些字段是：
   - 原始导出事实
   - 归一化表达
   - 推导辅助字段

完成标准：

1. 有独立文档
2. 不混用旧缓存字段命名
3. 明确哪些字段允许为空、哪些必须可追溯

当前产物：

- [NEW_INPUT_LAYER_OUTPUT_CONTRACT_DRAFT.zh-CN.md](</I:/autoteklasuanfa/NEW_INPUT_LAYER_OUTPUT_CONTRACT_DRAFT.zh-CN.md>)

---

## 阶段 2：最小归一化原型

目标：

- 不先重写所有导出，只先做一条最小可跑通的归一化原型

完成项：

1. 对单个 member：
   - 抽出统一 path
   - 抽出 part boundary / thickness 表达
2. 不求一开始覆盖全部零件类型
3. 优先覆盖：
   - `Beam`
   - `PolyBeam`
   - `ContourPlate`
   - `BentPlate`

完成标准：

1. 能对单 member 输出统一归一化结果
2. 输出结果不依赖下游主判定
3. 可以和旧缓存同样本对照

---

## 阶段 3：A/B 对照验证

目标：

- 用真实样本比较旧输入链和新输入层

首批样本建议：

1. 轴线类
   - `T2-13GL-9`
   - `T2-13GL-10`
   - `T2-13GL-16`
   - `T2-13GL-21`
   - `T2-13GL-24`
2. 候选类
   - `T2-13GL-23`
3. trace 类
   - `T3-2GL-53`
   - `T3-2GL-55`

对照维度：

1. 主轴是否稳定
2. candidate set 是否完整
3. station 输入是否稳定
4. trace 方向是否回正
5. topology 关系是否更接近人工观察

完成标准：

1. 至少在一组真实异常样本上，新输入层明显优于旧链
2. 对原本已正确样本，不得大面积回退

---

## 阶段 4：独立 sidecar 验证

目标：

- 先把新输入层作为独立 sidecar 输出，而不是默认入口

完成项：

1. 新输入层输出独立目录
2. 与旧链结果并列
3. 至少可导出：
   - member summary
   - part normalized summary
   - 基本差分摘要

完成标准：

1. 用户可以直接对照旧链与新链
2. 不需要手工翻日志才能看差异

---

## 阶段 5：切换门槛评估

目标：

- 判断新输入层是否已经具备替换旧输入链的资格

必须满足：

1. 关键异常样本稳定改善
2. 正常样本无明显回退
3. 健康检查层显示基础失真显著减少
4. 现有 candidate / trace / topology 主链无需大改即可获益

完成标准：

- 形成“是否升格为正式 `V3`”的结论

---

## 安全推进规则

## 1. 新输入层必须并行输出

禁止：

- 直接替换当前默认输入链
- 没有对照就改默认入口

要求：

- 旧链和新链必须能并行跑同一批样本

## 2. 新输入层不得直接回灌主判定

禁止：

- `新输入层看起来像 H => 直接判 H`
- `新输入层看起来更像 BOX => 直接抬主类`

要求：

- 先只作为对照输入和 sidecar

## 3. 每轮改动都必须带固定样本回归

至少包含：

1. 已知异常样本
2. 已知正确样本
3. 边界样本

## 4. 任一轮若出现明显回退，优先回到并行验证

禁止：

- 为了推进节奏，带着明显回退继续扩面

---

## 验收口径

新输入层预研阶段的验收，不看“是否已经重写全部逻辑”，而看：

1. 是否从根上减少基础失真
2. 是否让后续 candidate / trace / topology 更稳定
3. 是否减少人工倒查成本
4. 是否没有破坏当前主线稳定性

---

## 当前建议的直接下一步

1. 先新增“新输入层输出契约草案”文档
2. 再做最小归一化原型，只覆盖少量代表样本
3. 与当前 V2 health-audit sidecar 结合做 A/B 对照
4. 等对照结果稳定后，再决定是否正式升格为 `V3`

---

## 当前结论

当前结论固定为：

1. 新输入层值得做
2. 它大概率是下一阶段主方向
3. 但现在更适合作为：
   - `V3` 候选
   - 而不是已经正式切换完成的 `V3`

等输入契约、对照样本和切换门槛冻结后，再正式命名为 `V3` 更稳。

---

## 2026-04-30 当前补充

1. 新输入层已不再只输出 `PartModelType + WarningCodes`。
2. 当前每个零件已显式补出：
   - `RepresentationKind`
   - `RepresentationLevel`
   - `DistortionRiskCode`
   - `DistortionRiskReasons`
   - `DegradationReasonCodes`
3. 当前首版固定口径为：
   - 能形成 `路径 + 截面` 的，直接落 `PATH_SECTION_SWEEP`
   - 能形成 `边界 + 厚度` 的，直接落 `PLATE_BOUNDARY_THICKNESS`
   - 只在语义拿不全时，才显式落：
     - `APPROX_PATH_SECTION_SWEEP`
     - `APPROX_BOUNDARY_THICKNESS`
     - `TRACE_ONLY_FALLBACK`
4. 已在
   [new-input-layer-draft-run18-representation-v1](</I:/autoteklasuanfa/.tmpresults/new-input-layer-draft-run18-representation-v1>)
   对 `run_body_bracket_real_18` 批量验证通过。
5. 当前最明确的下一步已收窄为：
   - 不是继续扩 importer 猜测
   - 而是回到 exporter
   - 专攻 `Beam / PolyBeam` richer `boundary / section` 语义补层
