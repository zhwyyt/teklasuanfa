# DefinitionClauseDecision Full-Run Source Validation

## 目标

在真实 `fullrun-source` 工件刚落盘时，立刻做一层最小自校验，避免后面进入 `snapshot / sidecar` 后才发现：

- `Assemblies` 为空
- `RepresentativeParts` 为空
- `Summary` 计数不一致
- 代表零件缺少 `MemberId / AssemblyId / RepresentativePartId`

---

## 当前输出

`DefinitionClauseDecisionFullRunSourceExportService.Export(...)` 现在会额外输出：

- `definition-clause-decision-fullrun-source-validation.json`
- `definition-clause-decision-fullrun-source-validation.md`

---

## 当前校验范围

目前先覆盖最小一致性校验：

1. `Assemblies` 非空
2. `RepresentativeParts` 非空
3. `Summary.AssemblyCount` 与实际数量一致
4. `Summary.RepresentativePartCount` 与实际数量一致
5. 代表零件必须具备：
   - `MemberId`
   - `AssemblyId`
   - `RepresentativePartId != 0`

这一步还不是“业务正确性判定”，只是确保真实 `fullrun-source` 工件自身结构完整。
