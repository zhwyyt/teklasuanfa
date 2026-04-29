# Body Candidate Partition Conservative Fixtures

## 目标

为阶段 2 的保守分层契约补上代码级夹具与最小 runner，让：

- `signal snapshot -> conservative mapper -> decision`

不只是定义上的约定，而是已经有最小回归样本可跑。

## 当前代码入口

- `BodyCandidatePartitionConservativeFixture`
- `BodyCandidatePartitionConservativeFixtures`
- `BodyCandidatePartitionConservativeFixtureResult`
- `BodyCandidatePartitionConservativeFixtureRunner`

## 当前默认夹具

### `CORE_STABLE_SPAN_AND_MAIN_CONTOUR`

验证主体核心候选在主跨度 + 主轮廓控制信号下稳定落到：

- `BodyCoreCandidate`

### `SUPPORT_CONTINUITY_AND_ADJACENCY`

验证主体支撑候选在持续性 + 邻接支撑信号下稳定落到：

- `BodySupportCandidate`

并显式要求出现：

- `SingleMainPlateContinuity`
- `BodySupportAdjacency`

### `ACCESSORY_TOPOLOGY_ONLY`

验证明确附件拓扑稳定落到：

- `BodyAccessory`

### `EXTERNAL_CONTACT_ONLY`

验证仅外部接触稳定落到：

- `ExternalContext`

### `REVIEW_ACCESSORY_CORE_CONFLICT`

验证附件拓扑与主体核心控制信号冲突时强制进入：

- `ReviewRequired`

并要求 review reason 至少包含：

- `Accessory topology conflicts`

### `REVIEW_INSUFFICIENT_TRACE`

验证 trace 证据不足时强制进入：

- `ReviewRequired`

## 当前价值

- 为阶段 2 提供最小回归面
- 后续即使还没碰旧 partitioner，也能先守住保守映射契约
- 为未来把现有 partitioner 或中间结果适配到统一 lane/evidence/decision 输出提供直接校验基座
