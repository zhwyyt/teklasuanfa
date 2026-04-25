# DefinitionClause Bridge Effect Adapter

## 文档目的

本文档固定“旧 bridge/effect 结果如何喂给新 mapper 主链”的最薄适配面。

在当前环境下，我们已经把：

- `RawInputs`
- `tier`
- `CandidateDirection`
- `ClauseVerdict`
- `ClausePromotionReadiness`

都拆成了稳定纯函数。

下一步真正需要的，不是再长新规则，而是把既有 `DefinitionClause effect` 结果喂进这条纯函数主链。

---

## 当前新增

- [DefinitionClauseDecisionBridgeEffectSnapshot.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectSnapshot.cs>)
- [DefinitionClauseDecisionBridgeEffectAdapter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectAdapter.cs>)

---

## 当前职责

### `DefinitionClauseDecisionBridgeEffectSnapshot`

它不是最终业务模型，而是“旧 bridge/effect 结果并回 mapper 前”的最薄中间快照。

当前只保留 mapper 真正需要的字段：

- 真实/识别视图存在性
- 优先站位命中数
- 闭环/包络/拓扑改写
- 证明对象完整度
- `LeadClauseCode / LeadClauseShare`
- 冲突标记

### `DefinitionClauseDecisionBridgeEffectAdapter`

它负责三步：

1. `snapshot -> RawInputs`
2. `RawInputs -> Context`
3. `Context -> Result`

也就是说，下一轮如果能拿到旧 bridge/effect 结果，只需要再写一层：

- `OldEffectResult -> DefinitionClauseDecisionBridgeEffectSnapshot`

就能直接接进新 mapper 主链。

---

## 当前作用

这层 adapter 的价值是把“接回旧 bridge 文件”的动作收薄成：

1. 从旧 effect 结果里取字段
2. 填到 `DefinitionClauseDecisionBridgeEffectSnapshot`
3. 调 `DefinitionClauseDecisionBridgeEffectAdapter.Adapt(...)`

这样可以避免在旧 bridge 文件里直接重复写 tier 解析、candidate direction 判断和 verdict/promotion 逻辑。

---

## 下一步

下一轮优先做：

1. 识别现有 `DefinitionClauseDecisionBridge.cs` / `DefinitionClauseDecisionBridgeFixtures.cs` 的真实输出字段
2. 增加 `OldEffectResult -> DefinitionClauseDecisionBridgeEffectSnapshot` 的薄映射
3. 把现有 fixture / sidecar 入口改为走 `EffectAdapter -> Mapper`

当前这层已经把“怎么接回旧桥”从大面积改写，收薄成了一个中间快照映射问题。
