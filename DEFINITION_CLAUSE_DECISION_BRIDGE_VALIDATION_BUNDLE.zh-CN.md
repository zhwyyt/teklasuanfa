# DefinitionClause Bridge Validation Bundle

## 文档目的

本文档固定新的 bridge 验证 bundle 入口。

这个 bundle 的目标不是替代最终 `sidecar/full-run`，而是先把当前两条正在演化的验证链：

- mapper fixtures
- effect-adapter fixtures

收束到一个统一导出目录里。

---

## 当前入口

- [DefinitionClauseDecisionBridgeValidationBundleWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflow.cs>)

---

## 当前输出结构

- `mapper-fixtures/`
- `effect-adapter-fixtures/`
- `definition-clause-decision-bridge-validation-bundle-manifest.json`
- `definition-clause-decision-bridge-validation-summary.md`
- `README.md`

---

## 当前作用

这一步的核心价值是把两条新验证链先并到一个统一 bundle 里，减少后续并回旧 `bridge/fixture/sidecar` 工作流时的入口数量。

下一轮的理想动作就是：

1. 识别旧 bridge/fixture 工作流的现有导出点
2. 用当前 bundle workflow 替换多个分散入口
3. 再把 `OldEffectResult -> EffectSnapshot` 薄映射接进来

---

## 当前结论

新的 bridge 验证链现在已经不只是：

- 规则文档
- 纯函数
- 单独 fixture

而是已经拥有一个统一 bundle 出口。  
下一轮并回旧链时，可以优先围绕这个 bundle 入口做整合，而不是继续扩散更多平行 workflow。
