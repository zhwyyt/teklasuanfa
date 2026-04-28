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

在已确认正确的上游输入基线之上，重新收拢下游 proof 与家族映射推进节奏，避免继续在旧分支、旧语义、旧样本补丁上发散。

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

选择这个主题的原因：

1. 能直接检验上游轴线修正是否真实传导到下游
2. 能同时覆盖折线/变截面/多切片组织关系
3. 能逼出 proof 条款和家族映射的真实边界

---

## 当前执行分层

所有问题统一按以下层级排查：

1. 输入层
2. 主体候选层
3. 截面拓扑观察层
4. proof 条款层
5. 家族映射层

不允许再跳过层级，直接围绕最终结果补规则。

---

## 最近一次重要结论

1. 上游真实中线重建算法已确认正确。
2. `PolyBeam + Beam` guide 候选修正已确认正确。
3. 下游最终家族判定已摘除对上游 `SourceMemberMainClassCode` 的运行时依赖。
4. 当前最需要的不是继续加规则，而是先重建项目控制面。

---

## 当前下一步

1. 用新的 V2 任务板重建活跃问题台账。
2. 把当前未收敛样本按层级分类。
3. 只选“变截面 `H / BOX`”这一类继续推进。
4. 确定后续是否需要再单独切一个更干净的正式工作分支。

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
