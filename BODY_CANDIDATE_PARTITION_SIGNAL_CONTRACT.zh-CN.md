# Body Candidate Partition Signal Contract

## 目标

在已经冻结 `lane / evidence code / decision` 的基础上，再补一层最小可执行输入契约和保守纯映射骨架，便于后续把现有 `body-candidate partitioner` 或中间结果先适配到统一语言，而不是继续直接输出启发式标签。

## 当前代码入口

- `BodyCandidatePartitionSignalSnapshot`
- `BodyCandidatePartitionConservativeMapper`

## 输入信号

### 主体核心控制信号

- `HasStableZoneSpanControl`
- `HasMainContourControl`
- `HasClosedLoopControl`

三者任一命中，默认认为该候选已经具备“主体核心候选”级别的主控信号。

### 主体支撑信号

- `HasSingleMainPlateContinuity`
- `SupportsBodyCoreAdjacency`

当没有主体核心控制信号，但具备这些支撑信号时，保守落到 `BodySupportCandidate`。

### 附件 / 外部上下文信号

- `MatchesBodyAccessoryTopology`
- `HasExternalContactOnly`

### 直接 review 信号

- `HasConflictingTopology`
- `HasInsufficientTrace`
- `ReviewReasons`

## 当前保守映射规则

### 1. 先处理冲突

以下情况直接 `ReviewRequired`：

- `MatchesBodyAccessoryTopology` 与主体核心/主体支撑信号同时出现
- `HasExternalContactOnly` 与主体核心/主体支撑/附件信号同时出现
- `HasConflictingTopology == true`
- `HasInsufficientTrace == true`
- 已显式携带 `ReviewReasons`

### 2. 再处理稳定落点

- 有主体核心控制信号：`BodyCoreCandidate`
- 无主体核心控制信号，但有主体支撑信号：`BodySupportCandidate`
- 明显附件拓扑：`BodyAccessory`
- 仅外部接触：`ExternalContext`
- 其余无法解释：`ReviewRequired`

## 新增 evidence code

- `BodySupportAdjacency`

用于表示“该候选依附主体核心、但自身不控制主体主跨度/主轮廓/闭环”的支撑型证据。

## 当前价值

- 为阶段 2 提供一份先于算法并回的保守输入契约
- 给后续旧 partitioner / 中间结果的适配层提供最小纯函数目标
- 降低“支撑候选”被误塞进核心控制证据或附件证据的风险
