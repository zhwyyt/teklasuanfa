# Body Candidate Partition Lanes And Evidence

## 目标

为阶段 2“主体候选分层器”先冻结一份保守的分层契约，避免后续继续以启发式口头描述来区分：

- 哪些零件属于主体核心候选
- 哪些零件只是主体支撑候选
- 哪些零件应稳定落到 `body_accessory`
- 哪些零件只是外部上下文
- 哪些零件必须停在 review

## 当前代码入口

- `BodyCandidatePartitionLane`
- `BodyCandidatePartitionEvidenceCode`
- `BodyCandidatePartitionDecision`

## Lane 定义

### 1. `BodyCoreCandidate`

用于已经具备主体核心候选资格的零件，典型信号包括：

- 控制稳定区主跨度
- 参与主轮廓控制边
- 形成多数站位的闭合或持续主板证据

### 2. `BodySupportCandidate`

用于明显服务于主体核心、但自身还不足以单独成为主体核心的零件，典型情形包括：

- 与主体核心连续，但只承担局部支撑
- 对闭合/持续性有辅助作用，但不是主控件

### 3. `BodyAccessory`

用于明确属于主体附件链的零件，后续应优先流向 `body_accessory` 或 review 摘要，而不是进入主体核心证明对象。

### 4. `ExternalContext`

用于只提供外部接触、邻接约束或装配上下文的零件。这类零件不进入主体证明对象，只保留作上下文参照。

### 5. `ReviewRequired`

用于证据不足或信号冲突，暂时不能安全落入以上 4 类的零件。

## EvidenceCode 定义

### `StableZoneSpan`

表示该零件主导或显著参与稳定区主跨度。

### `MainContourControl`

表示该零件参与主轮廓控制边，而不是只提供局部附属边。

### `ClosedLoopControl`

表示该零件对闭合控制件集合或闭环控制具有核心作用。

### `SingleMainPlateContinuity`

表示该零件在多数站位里体现单主板持续性，是主体连续性的重要来源。

### `BodySupportAdjacency`

表示该零件依附主体核心、服务于主体连续性或局部支撑，但并不主控稳定区主跨度、主轮廓或闭环控制。

### `BodyAccessoryTopology`

表示该零件的拓扑结构更符合附件链，而不是主体核心或主体支撑。

### `ExternalContactOnly`

表示该零件只提供外部接触/邻接上下文，不应进入主体证明对象。

### `ConflictingTopology`

表示该零件同时触发多组互相冲突的拓扑信号，需要 review。

### `InsufficientTrace`

表示当前 internal trace / section trace 证据不足，不能稳定分层。

## 当前保守落点规则

- `BodyCoreCandidate`
  至少应伴随 `StableZoneSpan`、`MainContourControl`、`ClosedLoopControl`、`SingleMainPlateContinuity` 之一
- `BodySupportCandidate`
  通常伴随主体相关证据，但不以单独主控证据为主
- `BodyAccessory`
  优先由 `BodyAccessoryTopology` 驱动
- `ExternalContext`
  优先由 `ExternalContactOnly` 驱动
- `ReviewRequired`
  优先由 `ConflictingTopology`、`InsufficientTrace` 或 review reason 驱动

## 当前价值

- 为阶段 2 提供一份先于算法实现的保守分层契约
- 给后续 body-candidate partitioner、summary 输出和 review 结果提供共同语言
- 减少以后把 `body_accessory / external context / review` 混成一类的风险
