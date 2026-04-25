# DefinitionClauseDecision 预期样本清单

## 用途

这份清单用于约束 `DefinitionClauseDecisionBridge` 接线后的首轮回归，确保 `ClauseVerdict / ClausePromotionReadiness` 不会偏离当前阶段 5 已经观察到的稳定模式。

## 目标样本组

### GKZ

当前观察：

- `TopologyRewriteFamilyProofTarget = 箱型对壁闭合证明目标`
- `DefinitionClause = 箱型：闭合/对边稳定条款`
- `DefinitionClauseEffect = 箱型：移除此件会改写对边稳定控制`

预期：

- `ClauseVerdict = CLAUSE_SATISFIED_WITH_REWRITE`
- `ClausePromotionReadiness = READY_WITH_REWRITE_NOTE`

说明：

- 不应直接升级成“条款被破坏”
- 也不应退化成“完全无结论”

### HXZ

当前观察：

- `TopologyRewriteProofType = 单主板主轮廓证明`
- `DefinitionClause = 主板：多数站位持续性条款`
- `DefinitionClauseEffect = 主板：移除此件会改写多数站位持续性`

预期：

- `ClauseVerdict = CLAUSE_SATISFIED_WITH_REWRITE`
- `ClausePromotionReadiness = READY_WITH_REWRITE_NOTE`

说明：

- 重点是保住“主板持续性仍可追踪，但路径被改写”的保守结论

### MJ

当前观察：

- `DefinitionClause = 多板簇：直接控制排除条款`
- `DefinitionClauseEffect = 多板簇：此件更像直接控制件，不能直接排除`

预期：

- `ClauseVerdict = CLAUSE_BROKEN_DIRECT`
- `ClausePromotionReadiness = READY_FOR_DEFINITION_CHECK`

说明：

- 这类样本应优先稳定成“不能直接排除”的定义层结论

### GL

当前观察：

- 混合出现 `主板持续性改写` 与 `箱型对边稳定改写`
- 当前仍属于混合样本

预期：

- `ClauseVerdict = CLAUSE_REVIEW_REQUIRED`
- `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`

说明：

- 不应过早落成单一条款满足/破坏

### GL-H 折型翼缘子类

当前观察：

- `LeadClause = H型：腹板/翼缘连续性条款`
- `LeadClauseShare = 100%`
- assembly effect 已稳定为：
  - `2 x H型：移除此件会破坏腹板/翼缘连续性`
  - `1 x H型：移除此件会改写腹板/翼缘连续性`
- assembly effect direction 已稳定为：
  - `BrokenCandidate`

预期：

- `ClauseVerdict = CLAUSE_BROKEN_DIRECT`
- `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`

说明：

- 这是 `GL` 大类里的窄子类例外，不等于把所有 `GL` 都提升为 `BROKEN`
- 重点是保住：
  - 仍属 `H` 主家族下的折型翼缘小类
  - 但当前 assembly 级腹板/翼缘连续性破坏已足够稳定进入 `BROKEN`
  - readiness 仍保守停在 review
- 当前已明确纳入该子类的真实边界样本：
  - `T3-2GL-31`
    - 语义应解释为“折板/拼接翼缘 H 变体”
    - 不是“必须单整块下翼缘板”的特例
    - 预期同样稳定落在：
      - `ClauseVerdict = CLAUSE_BROKEN_DIRECT`
      - `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`
  - `T3-2GL-11 / 17`
    - 语义应解释为“纯开口 + 对称双主体板”的折板 `H` 变体
    - 预期同样稳定落在：
      - `ClauseVerdict = CLAUSE_BROKEN_DIRECT`
      - `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`
  - `T3-2GL-46`
    - 语义应解释为“弱单站位闭环 + review 翼缘不承担 envelope support”的折板 `H` 变体
    - 预期同样稳定落在：
      - `ClauseVerdict = CLAUSE_BROKEN_DIRECT`
      - `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`

### YPGL

当前观察：

- `DefinitionClauseEffect = 包络控制：移除此件会触发控制重分配`

预期：

- `ClauseVerdict = CLAUSE_REVIEW_REQUIRED`
- `ClausePromotionReadiness = NOT_READY_NEEDS_REVIEW`

说明：

- 仍以控制重分配复核为主，不直接晋升为家族定义判定

### YPGL-BOX 单件标准截面子类

当前观察：

- `SourceSemanticBodyFamily = StandardSection`
- `SourceSemanticSectionType = STANDARD_BOX`
- `SynthesizedBody = BOX`
- `core-body-proof` 只有 `1` 个 core part
- 原先虽已识别成标准型材，但阶段 5/6 仍停在：
  - `NO_CLAUSE_ROWS`
  - `ReviewRequired`

预期：

- `LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE`
- `ClauseVerdict = CLAUSE_SATISFIED_WITH_DIRECT_SOURCE`
- `ClausePromotionReadiness = READY_FOR_DEFINITION_CHECK`

说明：

- 这是 `YPGL` 大类里的窄子类例外，不等于把全部 `YPGL` 都直接提升
- 重点是保住：
  - “已经明确是单件标准箱型材”的 source 语义
  - 阶段 5/6 不再强制它先回到 built-up BOX proof-chain 再收口
- 当前已验证样本：
  - `T3-2YPGL-1 / 2 / 4 / 7 / 10 / 13 / 14 / 16 / 19 / 20 / 22 / 24 / 25`

## 首轮回归检查点

接入引擎后，至少检查以下四件事：

1. `GKZ / HXZ` 是否都稳定落在 `CLAUSE_SATISFIED_WITH_REWRITE`
2. `MJ` 是否稳定落在 `CLAUSE_BROKEN_DIRECT`
3. 普通 `GL / YPGL` 是否仍保守维持 `CLAUSE_REVIEW_REQUIRED`
4. `GL-H` 折型翼缘子类是否稳定落在 `CLAUSE_BROKEN_DIRECT + NOT_READY_NEEDS_REVIEW`
5. 摘要层是否能正确聚合 `LeadClauseVerdict / LeadClausePromotionReadiness`
