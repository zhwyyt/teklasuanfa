# 主材识别基座重构任务清单

> 历史档案说明
>
> 本文档已转为历史累计任务档案，不再作为当前活跃执行真源。
>
> 当前请优先使用：
>
> - [PROJECT_STATUS_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_STATUS_V2.zh-CN.md>)
> - [PROJECT_TASKLIST_V2.zh-CN.md](</I:/autoteklasuanfa/PROJECT_TASKLIST_V2.zh-CN.md>)
> - [PROJECT_REFOCUS_PLAN.zh-CN.md](</I:/autoteklasuanfa/PROJECT_REFOCUS_PLAN.zh-CN.md>)
> - [PROJECT_ARCHIVE_NOTICE.zh-CN.md](</I:/autoteklasuanfa/PROJECT_ARCHIVE_NOTICE.zh-CN.md>)

## 文档目的

本文档把 [MAIN_BODY_FOUNDATION_REBUILD_PLAN.zh-CN.md](</I:/autoteklasuanfa/MAIN_BODY_FOUNDATION_REBUILD_PLAN.zh-CN.md>) 拆成可执行任务清单。

目标不是继续微调当前 `BodyRecognizer`，而是按“工程定义驱动的截面证明器”路线，逐步替换当前启发式主体识别基座。

本文档同时承担“当前执行清单”的职责：

- 基线定义与总路线，看 [MAIN_BODY_FOUNDATION_REBUILD_PLAN.zh-CN.md](</I:/autoteklasuanfa/MAIN_BODY_FOUNDATION_REBUILD_PLAN.zh-CN.md>)
- 最新阶段状态与样本验证，看 [MAIN_BODY_REBUILD_STATUS.md](</I:/autoteklasuanfa/MAIN_BODY_REBUILD_STATUS.md>)
- 阶段 5 已冻结稳定规则总表，看 [STAGE5_STABLE_RULES_FREEZE.zh-CN.md](</I:/autoteklasuanfa/STAGE5_STABLE_RULES_FREEZE.zh-CN.md>)
- 当前应做什么、先做什么，以本文为准

---

## 总体执行原则

1. 先冻结定义，再改代码。
2. 先补输出和证据链，再替换主判定器。
3. 先做最小可验证闭环，再扩展复杂样本。
4. 每个阶段都必须有独立验收，不允许“感觉差不多”。
5. 对主体家族结论，允许 `UNKNOWN / IRREGULAR / REVIEW_REQUIRED`，不强判。

---

## 当前状态

- `2026-04-28` 已把阶段 6 最终家族判定对上游 `SourceMemberMainClassCode` 的运行时依赖摘除：
  - 已确认此前真正参与最终 `BOX / H / 变截面 BOX / deferred H hint` 判定的核心位置在
    [BodyFamilyDefinitionEvaluator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs>)
  - 当前已改为：
    - 上游 `SourceMemberMainClassCode` 只保留为 audit / sidecar 字段
    - 最终家族归属只吃下游工程定义链：
      - `LeadClause`
      - `LeadClauseVerdict / PromotionReadiness`
      - `BodyDescriptorFamily / BodyDescriptorSectionType`
      - `ImportSynthesisKind`
  - 已执行 `dotnet build I:\autoteklasuanfa\TeklaBodyBracketRecognition.sln`
    - `0` warning
    - `0` error
  - 当前下一步建议：
    - 若后续还要继续收口，可再把 output 文案中“上游稳定语义”这类历史措辞继续清成“下游工程定义链”
- `2026-04-28` 已完成一轮上游 `TeklaSectionClassifier` 根因修正，并确认这不是下游临时防守：
  - 已修 `Tekla2017MemberExtractor.BuildGuidePolyline(...)`：
    - 从“按端点投影排序后硬连线”
    - 改为“按纵向边构图 + 主组件筛选 + 平行组件同里程取中点”的真实 centerline 重建
  - 已新增 `LongitudinalAxisResolver.cs`，并把：
    - `DefaultSectionSampler`
    - `DefaultAnomalyDetector`
    - `ApproximateSectionIntersectionService`
    - 统一切到优先消费 `AxisSegments`
  - 已修 `ApproximateSectionIntersectionService.BuildFeatures(...)`：
    - 不再用 `wallCandidates >= 4`
    - 不再用 `2 flange + 2 web => enclosure=0.45`
    - 改为“边界覆盖 + 四角接触 + 横竖向边界资格”闭环证据
  - 已在临时重建缓存
    [run_body_bracket_real_04_upstream_axis_rebuilt_v2](</I:/autoteklasuanfa/.tmpdata/run_body_bracket_real_04_upstream_axis_rebuilt_v2>)
    上用上游 Runner 离线重算确认：
    - `T3-1GKZ-3`：仍稳定 `BOX`
    - `T3-1HXZ-2`：回到 `IRREGULAR + review`
    - `T3-2YPGL-5`：稳定 `BOX`
  - 当前这一步已把上游“轴线导出 / 采样 / enclosure”三层口径拉齐
  - 下一步优先级已收敛为：
    - 用真实 Tekla 模型重新正式导出一份缓存
    - 再跑下游全链路，确认正式导出物与临时重建缓存一致
- `2026-04-28` 已继续用 [run_body_bracket_real_11](</I:/xingcaisuanfa/cache/run_body_bracket_real_11>) 验证上游纵向轴选择层，并补上第二层上游根因：
  - 已确认 `T3-4GZ-7 / T3-4GZ-10` 的真实问题不是下游条款，而是：
    - 主件本体是 `Beam PL16*1000`
    - 但旧上游 guide 候选只认 `PolyBeam`
    - 导致短附件被误选为 longitudinal guide
  - 已把上游 `HasUsableGuideSegments(...)` 扩到：
    - `PolyBeam + Beam`
  - 已用临时重建缓存
    [run_body_bracket_real_11_upstream_axis_rebuilt_v1](</I:/autoteklasuanfa/.tmpdata/run_body_bracket_real_11_upstream_axis_rebuilt_v1>)
    复跑确认：
    - `T3-4GZ-7 / T3-4GZ-10` 已回到 `BOX`
    - `T3-4HXZ-4 / T3-4HXZ-15` 仍保持 `IRREGULAR + review`
  - 当前这一步说明：
    - 上游 longitudinal 相关 root cause 现在已经覆盖到两层：
      - guide 重建
      - guide 候选 part 选择
  - 当前下一步优先级进一步收敛为：
    - 从真实 Tekla 模型重新正式导出一份带最新上游代码的缓存
    - 用正式导出物再复跑 `real_11`
    - 然后再决定是否继续扩展到更多 `GZ / GKZ / YPGL / GL` 组合样本
- `2026-04-28` 已把 `ClosedLoopCandidate` 的粗拓扑判据继续收紧为“双证据闭环”：
  - 当前 [SectionClosedLoopEvidence.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/SectionClosedLoopEvidence.cs>) 已改成两级判定：
    - `endpoint-cycle`
    - `convex-hull boundary coverage`
  - 已对真实 [run_body_bracket_real_04_true_closed_loop_check_v3](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_true_closed_loop_check_v3) 复跑确认：
    - `T3-1GKZ-3` 仍稳定为：
      - `ClosedLoopStationCount = 5`
      - `CoarseMainClass = BOX`
    - `T3-1HXZ-2` 仍稳定为：
      - `ClosedLoopStationCount = 0`
      - `CoarseMainClass = NONE`
    - `T3-2YPGL-5` 当前仍未回到 `BOX`
  - 当前这一步的净效果是：
    - 已把“假围合误报”继续收住
    - 残余问题已收敛到“折线变截面箱体为何在 priority station 上仍取不到可证明围合”
- `2026-04-27` 已完成一轮低风险 `MainClass` 直达映射校正：
  - 根因已确认不是标准角钢识别链失效，而是下游 `DefinitionClauseDecisionFullRunSourceCollector` 的源 `MainClass` 映射表与上游 `TeklaSectionClassifier.MemberClass` 枚举脱节
  - 已确认上游枚举为：
    - `H=1 / Box=2 / T=3 / Cross=4 / Angle=5 / Pipe=6 / Irregular=7`
  - 已修正下游映射：
    - `4 => CROSS`
    - `5 => L`
    - `6 => PIPE`
  - 已补齐粗分类观察层 `CROSS` 中文标签，避免 sidecar/Excel 再把十字类显示成 `NONE`
  - 已对真实 [run_body_bracket_real_04_mainclass_mapping_fix_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_mainclass_mapping_fix_check) 复跑确认：
    - `T3-2GL-78` 已从旧错误的 `PIPE` 改回 `L`
    - `BodyDescriptorSectionType = STANDARD_ANGLE`
    - `run_body_bracket_real_04` 中 `34` 条角钢样本均已稳定显示为 `L`
  - 当前这一步只修“属性直达”映射口径，不改阶段 5/6 规则和证据链
- `2026-04-27` 已把“属性直达构件不走粗分类”落实到观察层：
  - 当前仅对 `BodyDescriptorFamily = StandardSection` 生效
  - 这类构件在粗分类 sidecar 中不再产出粗主类，而是直接落 `ATTRIBUTE_DIRECT_BYPASS`
  - 已对真实 [run_body_bracket_real_04_attribute_direct_bypass_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_attribute_direct_bypass_check) 复跑确认：
    - `T3-2GL-78` 当前为 `L + STANDARD_ANGLE + ATTRIBUTE_DIRECT_BYPASS`
    - `run_body_bracket_real_04` 内 `34` 条 `STANDARD_ANGLE` 样本全部旁路粗分类
  - 当前这一步仍只收口“属性直达 vs 粗分类观察”的边界，不扩展到 built-up H/BOX 主链
- `2026-04-25` 已把仓库内残留的旧启发式主体识别链从运行时主线中清除：
  - [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>)
  - [BodyFamilyDefinitionEvaluator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs>)
  - [BodyProfileResolver.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyProfileResolver.cs>)
  - 已删除：
    - `src/TeklaBodyBracketRecognition.Core/Algorithms/AssemblyAnalyzer.cs`
    - `src/TeklaBodyBracketRecognition.Core/Algorithms/BodyRecognizer.cs`
    - `src/TeklaBodyBracketRecognition.Core/Algorithms/BracketRecognizer.cs`
    - `src/TeklaBodyBracketRecognition.Core/Domain/Results.cs`
  - 当前已确认：
    - 主程序 sample/default 入口均只走阶段 2-7 proof 主链
    - `body-main-material-*` 摘要不再消费旧 `AssemblyRecognitionResult`
    - 阶段 6/7 不再依赖旧启发式 family/section 兜底闸门
  - 已继续完成第一轮输出命名清理：
    - 旧主体描述字段已统一迁移到 `BodyDescriptor*` 口径
    - 已覆盖主线模型、collector、Excel 导出与真实样本复跑
  - 当前主线输出契约、模型、collector 与 Excel 已完成 `Heuristic* -> BodyDescriptor*` 口径切换；后续仅在发现活跃文档残留时再做零星清扫，不再把它作为主线阻塞项
- `2026-04-25` 已对 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 做第一轮低风险结构优化：
  - 已把“单个 assembly 的 proof 流水线”抽成独立 helper
  - 已把批量结果容器抽成 `BatchArtifacts`
  - 已把集中落盘逻辑抽成 `WriteBatchArtifacts`
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 `run_body_bracket_real_08` 复跑结果与重构前保持一致
  - 当前这一步只做编排层降复杂度，不改阶段 2-7 的判定语义
- `2026-04-25` 已完成第二轮低风险结构优化：
  - 已新增 [BodyMaterialSupport.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyMaterialSupport.cs>)
  - 已把 `body-main-material-*` 的 summary/explanation/CSV/Markdown builder 从 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 挪出
  - 已删除 `Program.cs` 中对应重复实现
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_08_bodymaterial_extract_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_08_bodymaterial_extract_check) 复跑结果保持：
      - `BOX`
      - `CLOSED_LOOP_BOX`
      - `BUILTUP_BOX`
- `2026-04-25` 已完成第三轮低风险结构优化：
  - 已新增 [PipelineArtifactSupport.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/PipelineArtifactSupport.cs>)
  - 已把 `view-output` builder 与 `SectionTraceTopologySummaryRow` 聚合 builder 从 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 挪出
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_08_pipeline_artifact_extract_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_08_pipeline_artifact_extract_check) 复跑结果保持：
      - `BOX`
      - `CLOSED_LOOP_BOX`
      - `BUILTUP_BOX`
- `2026-04-25` 已完成第四轮低风险结构优化：
  - 已新增 [PipelineSummarySupport.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/PipelineSummarySupport.cs>)
  - 已把阶段 2-4.5 的四个 Markdown summary builder 从 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 挪出：
    - `BodyCandidatePartition`
    - `StableBodyZone`
    - `SectionTrace`
    - `SectionTopologyAnalysis`
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_08_pipeline_summary_extract_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_08_pipeline_summary_extract_check) 复跑结果保持：
      - `BOX`
      - `CLOSED_LOOP_BOX`
      - `BUILTUP_BOX`
- `2026-04-25` 已完成第五轮低风险结构优化：
  - 已新增 [CoreBodyProofSummarySupport.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/CoreBodyProofSummarySupport.cs>)
  - 已把 `CoreBodyProof / TopologyRewrite` 摘要 builder
  - 以及对应的 `TopologyRewrite*` 聚合 helper / private record 从 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 挪出
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_08_coreproof_summary_extract_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_08_coreproof_summary_extract_check) 复跑结果保持：
      - `BOX`
      - `CLOSED_LOOP_BOX`
      - `BUILTUP_BOX`
  - 随后已继续完成第一轮旧字段命名清理：
    - 已把旧主体描述字段统一切到 `BodyDescriptor*` 口径
    - 已对真实 [run_body_bracket_real_08_body_descriptor_rename_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_08_body_descriptor_rename_check) 复跑确认：
      - `BodyDescriptorFamily = BuiltUpBox`
      - `BodyDescriptorSectionType = BUILTUP_BOX`
      - `Family = BOX`
      - `FamilySubtype = CLOSED_LOOP_BOX`
      - `ProfileCategory/Profile = BUILTUP_BOX`
  - 下一步若继续优化，优先考虑：
    - 再评估 `Program.cs` 里是否还存在只负责模型映射/输出拼装的零散 helper，可继续下沉到独立 support 文件
- `2026-04-25` 已完成第六轮低风险结构优化：
  - 已新增 [BodyMaterialModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyMaterialModels.cs>)
  - 已新增 [PipelineArtifactModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/PipelineArtifactModels.cs>)
  - 已新增 [PipelineRunModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/PipelineRunModels.cs>)
  - 已把 `Program.cs` 尾部纯模型定义整体下沉，入口文件只保留主流程、局部服务容器与批量落盘逻辑
  - 已删除当前 App 主线未使用的 `AnalysisSummary` 与本地 `SanitizeFileName`
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_05_model_extract_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_05_model_extract_check) 复跑结果保持：
      - `AssemblyCount = 119`
      - `AdjudicatedCount = 90`
      - `DeferredCount = 29`
      - `ResolvedCount = 90`
      - `ReviewBypassCount = 29`
      - `H = 76 / STANDARD_SECTION = 41 / BOX = 1 / NONE = 1`
  - 下一步若继续按当前节奏优化，优先考虑：
    - 继续把 `Program.cs` 里仍然偏“工件落盘编排”的逻辑，收口成独立 batch artifact writer/support
- `2026-04-25` 已完成第七轮低风险结构优化：
  - 已新增 [BatchArtifactWriter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BatchArtifactWriter.cs>)
  - 已把 `BatchArtifacts` 与整块批量工件落盘逻辑从 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 下沉到独立 writer
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_05_batchwriter_extract_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_05_batchwriter_extract_check) 复跑结果保持：
      - `AssemblyCount = 119`
      - `AdjudicatedCount = 90`
      - `DeferredCount = 29`
      - `ResolvedCount = 90`
      - `ReviewBypassCount = 29`
      - `H = 76 / STANDARD_SECTION = 41 / BOX = 1 / NONE = 1`
  - 下一步若继续按当前节奏优化，优先考虑：
    - 继续把 `Program.cs` 中 `ProcessJob / RunProofPipeline` 周边仍然只是编排胶水的部分，评估是否还能再做一层 facade/support 收口
- `2026-04-25` 已完成第八轮低风险结构优化：
  - 已新增 [AnalysisPipelineServices.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/AnalysisPipelineServices.cs>)
  - 已新增 [OfflineBatchWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/OfflineBatchWorkflow.cs>)
  - 已把服务装配和离线流程编排从 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 挪出
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_05_workflow_extract_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_05_workflow_extract_check) 复跑结果保持：
      - `AssemblyCount = 119`
      - `AdjudicatedCount = 90`
      - `DeferredCount = 29`
      - `ResolvedCount = 90`
      - `ReviewBypassCount = 29`
      - `H = 76 / STANDARD_SECTION = 41 / BOX = 1 / NONE = 1`
  - 下一步若继续按当前节奏优化，优先考虑：
    - 继续把 `Program.cs` 最后剩下的批量主线调度收口成更薄的 app entry/facade
- `2026-04-25` 已完成第九轮低风险结构优化：
  - 已新增 [OfflineRecognitionApp.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/OfflineRecognitionApp.cs>)
  - 已把 sample / batch 入口调度与输出目录解析从 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 下沉到独立 app 层
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_05_appentry_extract_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_05_appentry_extract_check) 复跑结果保持：
      - `AssemblyCount = 119`
      - `AdjudicatedCount = 90`
      - `DeferredCount = 29`
      - `ResolvedCount = 90`
      - `ReviewBypassCount = 29`
      - `H = 76 / STANDARD_SECTION = 41 / BOX = 1 / NONE = 1`
  - 下一步若继续按当前节奏优化，优先考虑：
    - 评估是否需要把阶段 6/7 sidecar 串接再收口成单独 coordinator，但只有在确实还能降低入口复杂度时才继续
- `2026-04-25` 已完成第十轮低风险结构优化：
  - 已新增 [OfflineRecognitionBatchRunner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/OfflineRecognitionBatchRunner.cs>)
  - 已把 batch 主线串接从 [OfflineRecognitionApp.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/OfflineRecognitionApp.cs>) 下沉到独立 runner
  - 并已同步收口任务清单中已过期的 `Heuristic*` 清理待办表述，避免文档继续背历史包袱
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_05_batchrunner_extract_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_05_batchrunner_extract_check) 复跑结果保持：
      - `AssemblyCount = 119`
      - `AdjudicatedCount = 90`
      - `DeferredCount = 29`
      - `ResolvedCount = 90`
      - `ReviewBypassCount = 29`
      - `H = 76 / STANDARD_SECTION = 41 / BOX = 1 / NONE = 1`
  - 下一步若继续按当前节奏优化，优先考虑：
    - 只在确实还能显著降低复杂度时，再评估是否要把阶段 6/7 sidecar 串接收口成单独 coordinator
    - 否则可以暂停结构抽离，转向更高价值的规则/证据链收紧或活跃文档零星清扫
- `2026-04-25` 已完成第十一轮低风险结构优化：
  - 已新增 [DefinitionDrivenSidecarCoordinator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionDrivenSidecarCoordinator.cs>)
  - 已新增 [DefinitionDrivenSidecarCoordinatorResult.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionDrivenSidecarCoordinatorResult.cs>)
  - 已把阶段 6/7 sidecar 串接从 [OfflineRecognitionBatchRunner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/OfflineRecognitionBatchRunner.cs>) 收口成独立 coordinator
  - 当前已确认：
    - `dotnet build` 通过
    - 真实 [run_body_bracket_real_05_sidecar_coordinator_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_05_sidecar_coordinator_check) 复跑结果保持：
      - `AssemblyCount = 119`
      - `AdjudicatedCount = 90`
      - `DeferredCount = 29`
      - `ResolvedCount = 90`
      - `ReviewBypassCount = 29`
      - `H = 76 / STANDARD_SECTION = 41 / BOX = 1 / NONE = 1`
  - 当前这说明：
    - “再收一个阶段 6/7 sidecar coordinator” 这一步是值得的，且已完成
    - 继续沿这条结构线硬拆的边际收益已经明显下降
  - 下一步若继续按当前节奏优化，优先考虑：
    - 暂停进一步结构抽离，除非又发现新的明显职责边界
    - 保持“不碰规则和证据链”的前提下，只做零星活跃文档清扫或可证明收益明确的编排层收口
- `2026-04-25` 已针对 `T3-5GL-21` 暴露出的折线主体漏洞补上“按分段长度方向处理”的第一轮实现：
  - 已把 `member_*.json` 中的 `SolidEdges` 接入 importer / input / feature 模型
  - 已让阶段 2 的 longitudinal coverage 从“单一轴投影”改成“折线 guide 的累计长度坐标”
  - 已让阶段 3 `stable zone`、`section station` 和 `section trace` 改为沿折线累计坐标取站，并按所在段局部方向取截面
  - 已对 `PolyBeam + SpecialShape` 中高覆盖主体板候选开保守 `BodyCandidate` 入口，避免还没进入 proof 就被单轴误读直接打死
  - 已在真实 [run_body_bracket_real_06_segmented_axis_fix](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_06_segmented_axis_fix) 复跑：
    - `T3-5GL-21` 不再停在“无 stable zone / 无 priority station / 无 proof parts”
    - 当前已出现 `5` 个 priority stations、稳定核心区，以及 `CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE / Mixed`
    - 说明折线长度方向漏洞已被补上，剩余问题已前移为阶段 5/6 的条款升级门槛，而不是阶段 2/3 的几何误读
  - 下一步若继续围绕这个样本收口，优先考虑：
    - 不再重做折线长度方向底座
    - 直接分析 `CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE` 为何仍停在 `HoldEffectOnly`
    - 以及这类“成对翼缘 + 单腹板”的 `H + POLYLINE` 样本，何时可从 `Mixed` 安全上推到 `Satisfied / ReadyForPromotion`
- `2026-04-25` 已冻结折线主体上游导出契约：
  - 新增 [LONGITUDINAL_AXIS_EXPORT_CONTRACT.zh-CN.md](</I:/autoteklasuanfa/LONGITUDINAL_AXIS_EXPORT_CONTRACT.zh-CN.md>)
  - 当前已决定：
    - 不再把“从 `SolidEdges` 猜主体折线”视作长期终态
    - 由上游 `TeklaSectionClassifier` 负责导出 member 级 `GuidePolyline / AxisSegments`
    - 下游 `autoteklasuanfa` 负责消费，不再重复承担主体导向抽取
  - 接下来的执行顺序固定为：
- `2026-04-25` 已完成 `T3-5GL-21 / real_09` 的第一轮 `BOX` 提升收紧：
  - 已确认本轮真实根因：
    - `real_09` 中 priority stations 收缩到 `2`
    - 闭环只出现在折线末端的局部回折段
    - `35349275` 在该局部段从 `outer envelope` 切成 `internal trace`
    - 旧规则会把这类“局部晚段闭环”直接放大成 `PAIRED_PRIMARY_WALL_SYSTEM_RISK -> BOX`
  - 已在 [CoreBodyProofEngine.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/CoreBodyProofEngine.cs>) 落地第一轮窄修：
    - `closed box shell fallback` 现在要求多数/持续闭环证据
    - `PAIRED_PRIMARY_WALL_SYSTEM_RISK` 现在要求 cohort 级持续闭环与强包络支撑
    - 不满足时回退到 `CONTROLLER_REASSIGNMENT_RISK / GENERAL_DEFINITION_REVIEW_CLAUSE`
  - 已用真实 [run_body_bracket_real_09_box_persistence_fix](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_09_box_persistence_fix) 复跑确认：
    - `T3-5GL-21` 不再被推成 `BOX`
    - 当前回到 `Mixed / HoldEffectOnly / Deferred`
  - 已用真实 [run_body_bracket_real_05_box_persistence_fix](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_05_box_persistence_fix) 复跑确认：
    - 稳定 `BOX` 样本 `T2-12GL-75` 仍保留
    - 最终 `body-main-material-summary` 顶层计数相对 [run_body_bracket_real_05_sidecar_coordinator_check](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_05_sidecar_coordinator_check) 未见新增漂移
  - 下一步若继续围绕这个样本收口，优先考虑：
    - 不再回退 polyline longitudinal 支持
    - 直接分析 `T3-5GL-21` 为什么当前停在 `GENERAL_DEFINITION_REVIEW_CLAUSE`
    - 判断如何把“折线 H 主链”从 `Mixed / HoldEffectOnly` 再稳定上推，而不是再次误落 `BOX`
- `2026-04-25` 已完成 `T3-5GL-21 / real_09` 的第二轮折线 `H` 主链接回：
  - 已在 [CoreBodyProofEngine.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/CoreBodyProofEngine.cs>) 新增 `ResolveFoldedThreePlateHPartIds(...)`
  - 当前只对：
    - `3` 块板件主体
    - `2` 块强包络 + `1` 块弱包络
    - 弱单站闭环
    - 仍带显式 topology rewrite pattern
    - 的折线样本触发 `FALLBACK_FOLDED_THREE_PLATE_H_CHAIN`
  - 已在 [DefinitionClauseDecisionBridge.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridge.cs>) 补上
    - `H_WEB_FLANGE_CONTINUITY_CLAUSE`
    - `H_WEB_FLANGE_CONTINUITY_BREAK_EFFECT / REWRITE_EFFECT`
    - 的 bridge 映射
  - 已在真实 [run_body_bracket_real_09_folded_h_fix](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_09_folded_h_fix) 确认：
    - `T3-5GL-21`
    - `LeadClause = H_WEB_FLANGE_CONTINUITY_CLAUSE`
    - `BrokenCandidate`
    - `Break/Rewrite = 2/1`
    - `ReadyForPromotion`
    - `Family = H / FOLDED_FLANGE_H`
  - 已在真实 [run_body_bracket_real_05_folded_h_fix](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_05_folded_h_fix) 确认：
    - `BOX` 样本未丢
    - `FOLDED_FLANGE_H` 新增 `T2-12GL-26 / 27 / 30`
  - 下一步若继续围绕这个方向收口，优先考虑：
    - 先人工复核这 `3` 条新增 `FOLDED_FLANGE_H`
    - 若确认都合理，则把这条窄规则视作稳定增量
    - 若其中存在误吃，再继续压缩 `ResolveFoldedThreePlateHPartIds(...)` 的半径，而不是回退 `T3-5GL-21`
    1. 改 `I:\xingcaisuanfa\TeklaSectionClassifier\Models.cs`
    2. 改 `I:\xingcaisuanfa\TeklaSectionClassifier\Tekla2017MemberExtractor.cs`
    3. 重导一批新缓存做 smoke
    4. 再改 [XingcaiCacheImporter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs>) 与阶段 2-4 消费链
  - 这一步的目标是：
    - 先稳定几何证据层
    - 再判断 `T3-5GL-21` 剩余卡点是否真属于阶段 5/6 条款问题
- `2026-04-25` 已完成第一轮实现，不再停留在契约层：
  - 已改上游：
    - `I:\xingcaisuanfa\TeklaSectionClassifier\Models.cs`
    - `I:\xingcaisuanfa\TeklaSectionClassifier\Tekla2017MemberExtractor.cs`
  - 已改下游：
    - [XingcaiCacheImporter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/XingcaiCacheImporter.cs>)
    - [Inputs.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Domain/Inputs.cs>)
    - [BodyCandidatePartitioner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyCandidatePartitioner.cs>)
    - [OfflineBatchWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/OfflineBatchWorkflow.cs>)
    - [OfflineRecognitionApp.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/OfflineRecognitionApp.cs>)
  - 当前已确认三处编译通过：
    - `TeklaSectionClassifier`
    - `TeklaSectionClassifier.Runner`
    - `TeklaBodyBracketRecognition.sln`
  - 下一步执行顺序进一步收敛为：
    1. 用 `TeklaSectionClassifier.Runner` 重导一批带新字段的 `member_*.json`
    2. 对 `real_06` 先做 smoke，优先验证 `T3-5GL-21`
    3. 再对 `real_05` 做回归
    4. 若几何层稳定，再继续判断阶段 5/6 的 clause promotion 卡点

- `2026-04-24` 已把“先定大类，再定长度方向类型，再细分”的长度方向层正式接入主链，并完成真实 `real_06 / v02` 回归：
  - [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>)
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [BodyFamilyDefinitionEvaluator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs>)
  - [BodyProfileResolver.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyProfileResolver.cs>)
  - [Export-RecognitionExcels.py](</I:/autoteklasuanfa/tools/Export-RecognitionExcels.py>)
  - 当前已新增显式层：
    - `LongitudinalTypeCode = STRAIGHT / ARC / POLYLINE`
    - `LongitudinalTypeLabelZh = 直线主线 / 弧线主线 / 折线主线`
  - 当前保守规则：
    - `PolyBeam / BentPlate / IsSpecialShape -> POLYLINE`
    - 其它先落 `STRAIGHT`
    - `ARC` 先保留输出位，待上游补到可靠圆弧信号后再启用
  - 当前 `real_06 / v02` 已确认：
    - `body-family-proof` 与 `body-profile-resolution` 的 JSON / 中文 Markdown / Excel 都已带出长度方向列
    - 分布为 `279 x STRAIGHT`、`98 x POLYLINE`、`0 x ARC`
  - 随后已继续复核：
    - `real_01 / v01 = 193 x STRAIGHT / 53 x POLYLINE / 0 x ARC`
    - `real_04 / v84 = 173 x STRAIGHT / 32 x POLYLINE / 0 x ARC`
  - 并已对单件：
    - `T2-3GL-179`
    做原始 JSON + 主链双重确认：
    - 原始输入无 `Arc / Radius / Curv / 半径 / 圆弧` 显式字段
    - 仅含 `1 x PolyBeam(RHS300*10)`
    - 当前应按 `POLYLINE / 折线主线` 处理
  - 当前已继续把 `STRAIGHT` 再细分一层，并完成真实 `real_08 / v02` 验证：
    - 当前已新增：
      - `LongitudinalSubtypeCode`
      - `LongitudinalSubtypeLabelZh`
    - 当前首版细分类：
      - `GENERAL_STRAIGHT`
      - `AXIAL_KINKED_STRAIGHT`
    - 当前 `AXIAL_KINKED_STRAIGHT` 专门用于收：
      - 截面本身不必改变
      - 但中心主轴在分段处发生折向/偏折
      - 且整体仍不应并入 `POLYLINE` 的样本
    - 当前代表样本：
      - `T3-4GZ-5 -> STRAIGHT / AXIAL_KINKED_STRAIGHT`
  - 当前这说明长度方向这条主线已经可以先收口成：
    - 真实数据已稳定出现 `STRAIGHT / POLYLINE`
    - 其中 `STRAIGHT` 已可继续细分到 `主轴折向直线`
    - 暂未发现能可靠起出 `ARC` 的真实样本
    - 后续不要反复在现有样本上重复找 `ARC`，除非上游补到可靠圆弧信号
- `2026-04-24` 已在真实 `real_05 / v08` 回归中把剩余一批“`H` 大类已能站稳、但子类仍待收紧”的 deferred 样本，从 `NONE` 家族口径改回 `H` 家族口径：
  - [BodyFamilyDefinitionEvaluator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs>)
  - 当前这一步没有把它们全部硬推成阶段 7 已完成，而是先在阶段 6 `Deferred` 行里保留稳定 `H` 家族与保守子类：
    - `19 x GENERAL_BUILTUP_H`
    - `6 x VARIABLE_SECTION_H`
    - `3 x IRREGULAR_H`
    - `1 x H_MAINLINE_BENT`
  - 当前代表样本：
    - `T2-12GL-75 -> H_MAINLINE_BENT`
    - `T2-12GL-17 / 20 / 77 -> IRREGULAR_H`
    - `T2-12GKL-2 / T2-12GL-10 / 11 / 12 / 13 / 15 -> VARIABLE_SECTION_H`
  - 当前这说明下一步主线不再是“它们是不是 H 大类”，而是：
    - 继续把这些 `H` 保守子类往更稳定的阶段 6/7 细分结果收紧
  - 同时后续细分顺序固定为：
    - 先确定家族大类
    - 再确定长度方向类型 `直线 / 弧线 / 折线`
    - 最后再进入 built-up / 标准型材等细类收紧
  - 当前执行口径再收紧为：
    - 主线先把 `直线 / 折线` 用好
    - `直线` 内部允许继续细分 `一般直线 / 主轴折向直线`
    - `弧线` 保留语义位，但不作为当前批次的阻塞项
- `2026-04-24` 已在真实 `real_05 / v07` 回归中继续收掉一批“阶段 5 已起出 H 条款、但阶段 6 仍停在 deferred”的三板开口 `H` cohort：
  - 阶段 5：
    - [CoreBodyProofEngine.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/CoreBodyProofEngine.cs>)
    - 已把 `2 x BodyCandidate + 1 x SpecialShape`、`纯开口`、`主翼缘为 special-shape` 的窄 `H` 变体纳入 `H_WEB_FLANGE_CONTINUITY_CLAUSE` bootstrap
  - 阶段 6：
    - [BodyFamilyDefinitionEvaluator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs>)
    - 已新增仅针对：
      - `SourceMemberMainClass = H`
      - `BodyDescriptorFamily = BuiltUpT`
      - `H_WEB_FLANGE_CONTINUITY_CLAUSE + BrokenCandidate + B2/R1 + ReadyForReview`
      - 的稳定 `H` family gate
  - 当前命中：
    - `T2-12GKL-5 / 6 / 7 / 10 / 11 / 12 / 16`
    - `T2-12GL-26 / 27 / 30 / 31 / 33 / 39`
  - 当前这 `13` 个样本已统一达到：
    - 阶段 6：`H / GENERAL_BUILTUP_H`
    - 阶段 7：`BUILTUP_H / H_BUILTUP_SERIES`
  - 当前 `real_05 / v07` 汇总：
    - `AdjudicatedCount = 90`
    - `DeferredCount = 29`
    - `ResolvedCount = 90`
- `2026-04-24` 已在真实 `real_05 / v05` 回归中确认一批“稳定 H 主类却被阶段 6 并成单主板主体”的误判共性：
  - `T2-12GKL-1 / 3 / 4`
  - `T2-12GL-23 / 24`
  - 当前已不再按 `PRIMARY_PLATE_BODY / MAJORITY_CONTINUITY` 落桶，而是统一按“上游稳定 H 语义优先”进入：
    - 阶段 6：`H / GENERAL_BUILTUP_H`
    - 阶段 7：`BUILTUP_H / H_BUILTUP_SERIES`
  - 当前这一步不是构件号白名单，而是新增了基于：
    - `SourceMemberMainClass = H`
    - 或 `ImportSynthesisKind = H`
    - 或 `BodyDescriptorFamily = BuiltUpH`
    - 的稳定 `H` 主类优先闸门
- `2026-04-22` 已撤回本地 `worker` 推进分支：
  - 已删除仓库内常驻脚本、运行目录和对应说明文档；
  - 后续执行统一回到当前线程的 Codex 心跳，不再把本地执行器当作任务清单的一部分。
- `2026-04-22` 已新增阶段 5 -> 阶段 6 的判定升级闸门文档：
  - [DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md>)
  - 当前已先冻结 `ClauseVerdict / ClausePromotionReadiness` 的保守定义、升级必要条件和 `GKZ / HXZ / GL / MJ / YPGL` 首轮落点。
- `2026-04-22` 已新增 `DefinitionClause` 证据优先级文档：
  - [DEFINITION_CLAUSE_DECISION_EVIDENCE_PRECEDENCE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_EVIDENCE_PRECEDENCE.zh-CN.md>)
  - 当前已冻结来源优先级、站位稳定性优先级、拓扑信号优先级、证明完整度优先级和冲突降级规则。
- `2026-04-22` 已新增 `DefinitionClause` bridge 映射表：
  - [DEFINITION_CLAUSE_DECISION_BRIDGE_MAPPING_TABLE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_BRIDGE_MAPPING_TABLE.zh-CN.md>)
  - 当前已把 `tier -> CandidateDirection -> ClauseVerdict -> ClausePromotionReadiness` 的表驱动映射固定下来。
- `2026-04-22` 已新增 bridge 邻接层纯函数骨架：
  - [DefinitionClauseDecisionBridgeTierModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeTierModels.cs>)
  - [DefinitionClauseDecisionBridgeMapper.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapper.cs>)
  - 当前已能在独立纯函数层输出 `CandidateDirection / Verdict / PromotionReadiness`，下一轮重点是并回既有 bridge 与 fixture。
- `2026-04-22` 已补上 bridge mapper 的最小 fixture 层：
  - [DefinitionClauseDecisionBridgeMapperFixtures.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapperFixtures.cs>)
  - [DefinitionClauseDecisionBridgeMapperFixtureRunner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapperFixtureRunner.cs>)
  - [DefinitionClauseDecisionBridgeMapperFixtureReportBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapperFixtureReportBuilder.cs>)
  - 当前已能对 `GKZ / HXZ / GL / MJ / YPGL + synthetic broken` 跑最小回归。
- `2026-04-22` 已补上 bridge mapper fixture 的 artifact/workflow 层：
  - [DefinitionClauseDecisionBridgeMapperFixtureArtifactModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeMapperFixtureArtifactModels.cs>)
  - [DefinitionClauseDecisionBridgeMapperFixtureArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeMapperFixtureArtifactBuilder.cs>)
  - [DefinitionClauseDecisionBridgeMapperFixtureArtifactSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeMapperFixtureArtifactSerializer.cs>)
  - [DefinitionClauseDecisionBridgeMapperFixtureWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeMapperFixtureWorkflow.cs>)
  - [DEFINITION_CLAUSE_DECISION_BRIDGE_MAPPER_FIXTURE_WORKFLOW.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_BRIDGE_MAPPER_FIXTURE_WORKFLOW.zh-CN.md>)
  - 当前新 mapper fixture 层已经能独立落 JSON/Markdown 工件。
- `2026-04-22` 已补上 bridge mapper 的 raw-input 归一化层：
  - [DefinitionClauseDecisionBridgeTierModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeTierModels.cs>)
  - [DefinitionClauseDecisionBridgeMapper.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapper.cs>)
  - 当前已能从原始布尔/计数输入统一归一化出 `SourceTier / StationTier / TopologyTier / ProofCompletenessTier / LeadClauseTier`。
- `2026-04-22` 已补上旧 effect -> 新 mapper 的适配层：
  - [DefinitionClauseDecisionBridgeEffectSnapshot.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectSnapshot.cs>)
  - [DefinitionClauseDecisionBridgeEffectAdapter.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectAdapter.cs>)
  - [DEFINITION_CLAUSE_DECISION_BRIDGE_EFFECT_ADAPTER.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_BRIDGE_EFFECT_ADAPTER.zh-CN.md>)
  - 当前下一轮只差把既有旧 bridge/effect 结果字段映射进 `EffectSnapshot`。
- `2026-04-23` 已把真实 full-run 的 `CoreBodyProof` 输出字段接进 `EffectSnapshot -> tier -> verdict -> readiness` 主链：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [DefinitionClauseDecisionFullRunSourceModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceModels.cs>)
  - [DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs>)
  - [DefinitionClauseDecisionFullRunSourceArtifactModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactModels.cs>)
  - 当前真实工件已能直接导出 `tier / candidate-direction / verdict / readiness / conflict-reasons`，并在构件级聚合 `LeadClauseVerdict / LeadClausePromotionReadiness / MixStatus`。
- `2026-04-23` 已继续把真实 `full-run source` 的 `DefinitionClauseEffect` 导出从零件级抬到 assembly 聚合层：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [DefinitionClauseDecisionFullRunSourceModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceModels.cs>)
  - [DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs>)
  - [DefinitionClauseDecisionFullRunSourceArtifactModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactModels.cs>)
  - [run_body_bracket_real_04_definition_clause_v58](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v58)
  - 当前代表零件已能直接导出 `TopologyRewriteDefinitionClauseEffectCode / LabelZh`；
  - 当前 assembly 已能直接导出：
    - `LeadClauseEffectCode / LabelZh / Share / MixStatus`
    - `LeadClauseBreakEffectCount / LeadClauseRewriteEffectCount`
  - 当前 `H_WEB_FLANGE_CONTINUITY_CLAUSE` 折型翼缘 cohort 已在工件里稳定显式为：
    - `2 x H_WEB_FLANGE_CONTINUITY_BREAK_EFFECT`
    - `1 x H_WEB_FLANGE_CONTINUITY_REWRITE_EFFECT`
  - 下一轮主线应从“猜 review prompt 里的 effect 文本”切换为：
    - 基于结构化 `DefinitionClauseEffect` 聚合，决定 `H` 折型翼缘小类是否仍停在 `ReviewRequired / ReadyForReview`
    - 或者把 `DefinitionClauseEffect` 更深接进 verdict gate
- `2026-04-23` 已继续把真实 `full-run source` 的 assembly effect 聚合收紧成 `effect direction`：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [DefinitionClauseDecisionFullRunSourceModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceModels.cs>)
  - [DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs>)
  - [DefinitionClauseDecisionFullRunSourceArtifactModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactModels.cs>)
  - [run_body_bracket_real_04_definition_clause_v59](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v59)
  - 当前 assembly 已新增：
    - `LeadClauseEffectDirectionCode / LabelZh`
    - 摘要中的 `EffectDirection` 分布
  - 当前 `H_WEB_FLANGE_CONTINUITY_CLAUSE` 折型翼缘 cohort 已在 `v59` 统一显式为：
    - `LeadClauseEffectDirectionCode = BrokenCandidate`
    - 且仍保持：
      - `LeadClauseVerdictCode = ReviewRequired`
      - `LeadClausePromotionReadinessCode = ReadyForReview`
  - 下一轮主线应进一步从“effect 结构已知”切到：
    - `BrokenCandidate` 是否足够安全进入 verdict gate
    - 还是必须继续补额外结构锚点后，才允许从 `ReviewRequired` 提升到 `Broken`
- `2026-04-23` 已把 `BrokenCandidate + StableFull + B2/R1` 的 `H` 折型翼缘 assembly 组合正式接进 verdict gate：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [run_body_bracket_real_04_definition_clause_v60](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v60)
  - 当前窄 gate 仅针对：
    - `LeadClauseCode = H_WEB_FLANGE_CONTINUITY_CLAUSE`
    - `LeadClauseShare = 100%`
    - `ClauseMix = SINGLE_CLAUSE`
    - `LeadClauseEffectDirectionCode = BrokenCandidate`
    - `LeadClauseBreakEffectCount = 2`
    - `LeadClauseRewriteEffectCount = 1`
    - 且当前 assembly 仍为 `ReviewRequired + ReadyForReview`
  - 当前命中结果：
    - `T3-2GL-5 / 9 / 10 / 49 / 50 / 51 / 52 / 53 / 54 / 55 / 56 / 57`
    - `T3-2GKL-3`
    - 已统一从：
      - `ReviewRequired + ReadyForReview`
    - 收紧到：
      - `Broken + ReadyForReview`
  - 下一轮主线应转向：
    - 是否要把这条窄 gate 固化进 fixture / verdict-gate 文档
    - 以及是否存在其它 `GL / H` effect 结构也能安全按同样方式上推 verdict
- `2026-04-23` 已把这条 `H` 折型翼缘 assembly 窄 gate 正式固化进定义文档：
  - [DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md>)
  - [DEFINITION_CLAUSE_DECISION_EXPECTED_CASES.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_EXPECTED_CASES.zh-CN.md>)
  - 当前已明确：
    - 普通 `GL` 仍按保守主线处理
    - `GL-H` 折型翼缘子类允许作为 `GL` 大类下的小分类例外，稳定落在：
      - `BROKEN`
      - `READY_FOR_REVIEW`
  - 下一轮主线应继续转向：
    - 是否存在其它 `GL / H` effect 组合也具备同等级别的 assembly narrow gate 资格
    - 或者当前应优先离开 `GL-H`，回到 `MJ / YPGL / 普通 GL` 这些仍未稳定分化的主桶
- `2026-04-23` 已在真实 `full-run source` 聚合层补上首个 `GL` lead-clause stabilization 窄规则：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [run_body_bracket_real_04_definition_clause_v48](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v48)
  - 当前已把 `GL` 中 `2 x BOX 对壁条款 + 1 x SINGLE_PRIMARY 直控主板条款` 的 `3` 个装配从伪 `ClauseMix` 收紧到稳定 `BOX` 主条款，并真正落成 `条款满足 / 可进入阶段 6 提升`
  - 当前这条规则仍刻意停在 `full-run source` 聚合层，不直接改写 `CoreBodyProofEngine` 的原始 `DefinitionClause` 赋值；下一轮若要继续扩大覆盖面，应先确认更多真实装配是否也属于同一种 shadow-primary 模式
- `2026-04-23` 已在真实 `full-run source` 复核层补上 `BOUNDARY_REVIEW` 的弱 side-row 拆分：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [run_body_bracket_real_04_definition_clause_v49](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v49)
  - 当前已把 `NONE / NONE / NONE + INSUFFICIENT_PERSISTENT_PROOF` 的 review 行从 `BOUNDARY_REVIEW` 单独拆成 `SIDE_ROW_PERSISTENCE_REVIEW`
  - 这一步没有改变 `MJ / YPGL` 的 assembly 级 verdict/readiness，但把复核语义进一步收紧为：
    - 真正已进入证明链的边界复核
    - 仅旁路件持续性不足的弱 review
- `2026-04-23` 已在真实 `full-run source` 冲突原因层补上 `BROKEN_STRUCTURE_NEEDS_PROOF_CHAIN`：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [run_body_bracket_real_04_definition_clause_v50](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v50)
  - 当前已把“结构破坏已强成立，但 `ProofType / FamilyProofTarget / DefinitionClause` 全空”的样本从 `BROKEN_STRUCTURE_NEEDS_CLAUSE_ANCHOR` 中单独拆出
  - 这一步没有改变 `MJ / YPGL` 的 assembly 级 verdict/readiness，但明确表明下一轮更值得去补 proof-chain 上游，而不是继续在下游 review gate 上硬推
- `2026-04-23` 已在 [run_body_bracket_real_04_definition_clause_v53](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v53) 把 `MJ / MQMJ / YPGL` 的窄 proof-chain bootstrap 直接接回 [CoreBodyProofEngine.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/CoreBodyProofEngine.cs>)：
  - `T3-2MJ-1 / T3-2MQMJ-1` 已不再停在 `BROKEN_STRUCTURE_NEEDS_PROOF_CHAIN`，而是直接起出：
    - `SINGLE_PRIMARY_PLATE_PROOF`
    - `SINGLE_PRIMARY_PLATE_SYSTEM_TARGET`
    - `PRIMARY_PLATE_CONTINUITY_CLAUSE`
  - `T3-2YPGL-12 / 17 / 23` 已不再停在 `BROKEN_STRUCTURE_NEEDS_PROOF_CHAIN`，而是直接起出：
    - `PAIRED_WALL_MAIN_CONTOUR_PROOF`
    - `BOX_WALL_PAIR_PROOF_TARGET`
    - `BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE`
  - 当前 `BOX` 的 proof-chain 启动语义已从“矩形箱”改成更贴近用户定义的“闭合箱壁壳”
- `2026-04-23` 已在 [run_body_bracket_real_04_definition_clause_v54](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v54) 继续吃掉 `T3-2YPGL-5`：
  - 当前已确认这类不是“脱离箱形”的异形，而是：
    - `闭合箱壳 + 折板翼缘` 小类
  - 当前闭合箱壳 bootstrap 已允许把带同等闭环破坏证据的 `SpecialShape` 折板壳件并入 cohort
  - `T3-2YPGL-5` 现已成功起出：
    - `PAIRED_WALL_MAIN_CONTOUR_PROOF`
    - `BOX_WALL_PAIR_PROOF_TARGET`
    - `BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE`
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v61](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v61) 把 `T3-2YPGL-12 / 17 / 23` 重新接回 `BOX` 主链：
  - [CoreBodyProofEngine.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/CoreBodyProofEngine.cs>) 已把 `BOX` fallback 从“每块壳件都必须有 envelope support”收紧成 cohort 级判定：
    - 保留原来的 canonical `BOX` 和 `折板翼缘闭合箱壳`
    - 新增窄小类：
      - `4` 块板全部稳定 `LostClosedLoop`
      - `2` 块承担 envelope support
      - `2` 块不承担 envelope support，但仍属于同一闭合箱壳 cohort
  - 当前这条窄规则只命中：
    - `T3-2YPGL-12 / 17 / 23`
  - 当前 `v61` 真实结果：
    - `T3-2YPGL-12 / 17 / 23 / 5`
    - 已统一达到：
      - `BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE`
      - `Broken`
      - `ReadyForPromotion`
  - 下一轮主线应继续转向：
    - 是否还有其它 `YPGL` 闭合箱壳 cohort 只是被当前 proof-chain 上游漏掉
    - 或者应优先离开 `YPGL`，回到普通 `GL / BuiltUpT / Irregular` 的 `NO_CLAUSE_ROWS + ReviewRequired` 大盘
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v63](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v63) 把 `T3-2GL-12 / 47 / 48` 从 `BuiltUpT + NO_CLAUSE_ROWS` 接回折板翼缘 `H` 主链：
  - [CoreBodyProofEngine.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/CoreBodyProofEngine.cs>) 已新增“单站位弱闭环痕迹”的折板翼缘 `H` 窄规则：
    - 允许最多 `1` 个优先站位出现局部闭环像
    - 但主体语义仍必须稳定表现为：
      - `2 x BodyCandidate + 1 x SpecialShape`
      - `腹板 + 平翼缘 + 折板翼缘`
    - 不允许因为这一个局部闭环像直接掉成 `BOX`
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>) 已把这类 `B2/R1` 的折板翼缘 `H` assembly readiness 统一收紧为：
    - `Broken`
    - `ReadyForReview`
  - 当前 `v63` 真实结果：
    - `T3-2GL-12 / 47 / 48`
    - 已统一达到：
      - `H_WEB_FLANGE_CONTINUITY_CLAUSE`
      - `LeadClauseEffectDirectionCode = BrokenCandidate`
      - `Break/Rewrite = 2/1`
      - `Broken + ReadyForReview`
  - 下一轮主线应继续转向：
    - 普通 `GL / BuiltUpT / Irregular` 剩余 `NO_CLAUSE_ROWS + ReviewRequired` 大盘里，哪些也是“局部弱闭环 + 折板翼缘 H”漏判
    - 以及这条窄规则是否需要补进 `expected cases / verdict gate` 文档冻结
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v65](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v65) 把一批单零件标准截面从 `Irregular` 拉回标准型材主线：
  - [BodyRecognizer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyRecognizer.cs>) 已新增“单主件标准截面” fast path：
    - 优先条件：
      - `1` 个主体件
      - `Beam / PolyBeam`
      - `ProfileString` 非 `PL*`
      - 标准截面字符串直接可判
    - 当前先覆盖：
      - `L / C / U / H / I / T / BOX / PIPE / ROD`
  - 当前 `v65` 真实结果：
    - `T3-2GL-13 / 14 / 77 / 78 / 79 / 80 / 81 / 82 / 83 / 84 / 85 / 86 / 88 / 89 / 90 / 91 / 92 / 93 / 94 / 95`
    - 已统一达到：
      - `BodyDescriptorFamily = StandardSection`
      - `BodyDescriptorSectionType = STANDARD_ANGLE`
      - `BodyDescriptorDerivationType = StandardProfile`
      - `BodyDescriptorReviewRequired = false`
  - 这说明当前这批 `L75*5` 不应继续留在：
    - `Irregular`
    - `NO_CLAUSE_ROWS + ReviewRequired`
  - 下一轮主线应继续转向：
    - 其余单零件标准截面是否也存在同类漏判
    - 以及阶段 5/6 是否需要为 `StandardSection` 单独开一条“非 built-up，不走板链 proof-engine”的家族直达链
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v67](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v67) 继续把 `YC` 这批单零件角钢从 `Unknown` 拉回 `StandardSection`：
  - [BodyRecognizer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BodyRecognizer.cs>) 已把“单主件标准截面” fast path 再收紧一轮：
    - 不再排除 `IsSpecialShape = true` 的单件 `PolyBeam`
    - 当前 profile 前缀已扩大到：
      - `L / C / U / [ / H / I / BH / T / BOX / BK / RHS / SHS / PIPE / CHS / ROD / Dxx`
  - 当前 `v67` 真实结果：
    - `T3-2YC-1 / 2 / 3 / 4 / 5 / 6 / 7 / 8 / 9 / 10 / 11 / 12 / 13`
    - 已统一达到：
      - `BodyDescriptorFamily = StandardSection`
      - `BodyDescriptorSectionType = STANDARD_ANGLE`
      - `BodyDescriptorDerivationType = StandardProfile`
      - `BodyDescriptorReviewRequired = false`
  - 当前这说明“标准截面”入口不能只覆盖普通 `Beam`，也必须覆盖单件 `PolyBeam` 的标准型材
  - 当前下一轮主线应继续收紧成两档：
    - `BodyRecognizer` 侧仍剩下的 unsynthesized 单件标准截面漏判：
      - `T3-2GL-66 / 70 / 103 / 76`（`BH400*150*8*8 / BH400*200*8*10`）
      - `T3-1HMS-1 / 2 / 3`（`D24`）
    - 导入阶段已经先被 `ImportSynthesisKind = H / BOX` 合成为板链的单件标准截面：
      - 是否要在 importer/source-summary 层补“source single standard section”旁路
      - 以及这件事是否会影响当前阶段 5 的 `H / BOX` proof-chain 主线，需要谨慎拆开推进
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v68](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v68) 把 synthesized `BH / BOX` 大盘从 summary 口径上收回 `StandardSection`：
  - [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 已新增“源主件标准截面优先”覆盖：
    - 家族结论优先看 `SourceMainPartProfileString`
    - synthetic `H / BOX` 仅保留为 proof-side 表示，不再覆盖 `body-main-material-summary`
  - 当前 `v68` 真实结果：
    - `80` 个 synthesized `BH` 样本已落成：
      - `StandardSection / STANDARD_IH / StandardProfile / ReviewRequired=false`
    - `13` 个 synthesized `BOX` 样本已落成：
      - `StandardSection / STANDARD_BOX / StandardProfile / ReviewRequired=false`
- `2026-04-24` 已完成阶段 7 首轮 profile-series 真实验证：
  - [BodyProfileResolver.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyProfileResolver.cs>)
  - [BodyProfileResolutionModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyProfileResolutionModels.cs>)
  - [BodyProfileResolutionArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyProfileResolutionArtifactBuilder.cs>)
  - [run_body_bracket_real_04_definition_clause_v82](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v82)
  - [run_body_bracket_real_04_definition_clause_v83](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v83)
  - 当前已先把旧 `REGULAR_BUILT_UP` 主桶稳定拆成：
    - `36 x BUILTUP_BOX`
    - `25 x BUILTUP_PRIMARY_PLATE`
    - `8 x BUILTUP_H`
    - `1 x VARIABLE_SECTION_BUILT_UP`
  - 当前又把 `STANDARD_SECTION` 主桶继续细分成真实系列：
    - `75 x BH_SERIES`
    - `34 x L_SERIES`
    - `3 x D_SERIES`
  - `v83` 当前验证通过：
    - `AssemblyCount = 205`
    - `ResolvedCount = 182`
    - `ReviewBypassCount = 23`
    - `IsValid = true`
  - 这说明阶段 7 当前已经形成：
    - `ProfileCategory / Profile / ProfileSeries` 三层稳定输出
  - 当前已进一步确认阶段 7 收口口径：
    - `BH_SERIES` 维持系列级，不再继续下钻到规格级
    - 阶段 7 可按“主线完成”管理
    - 最终面向人工的主输出文件要求保持中文
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v77](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v77) 把 `SourceStandardSection` 直达链推进到真实 full-run 主链：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>) 已允许：
    - `proof.Result.Parts = 0` 的标准截面空链样本仍按 source semantic 直达；
    - synthesized `STANDARD_IH` 的 `2 x core plate / 0 review` 样本按 source semantic 直达；
    - `STANDARD_ROD` 的“单主杆件 + 附属模板/锚杆”样本按 source 主杆件直达；
  - assembly 聚合已改成：
    - 命中这条 source-standard 直达时，直接落成：
      - `STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE`
      - `Satisfied`
      - `ReadyForPromotion`
  - 当前已真实收掉：
    - `T3-2GL-1 / 16 / 21 / 24 / 37 / 59 / 67 / 69 / 75 / 87 / 96 / 98`
    - `T3-2GKL-2`
    - `T3-1HMS-1 / 2 / 3`
    - `T3-2YC-1..13`
  - 当前保守 keep-out 继续保持不变：
    - `T3-2GL-66 / 70 / 76 / 103`
    - `T3-2MJ-2 / 10 / 12`
  - 下一轮主线应继续转向：
    - `BuiltUpT` 的 `SatisfiedCandidate + 1/2` 折板 `H` review 桶
    - `STANDARD_IH` 最后 `6` 个保守桶为何仍不起直达
    - `MJ-2 / 10 / 12` 继续保持 review，不作为当前主线扩张对象
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v78](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v78) 把 `BuiltUpT` 的 `SatisfiedCandidate + 1/2` 折板 `H` review 桶收掉大头：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>) 已新增普通 `GL / BuiltUpT` 的折板 `H` satisfied gate：
    - `LeadClause = H_WEB_FLANGE_CONTINUITY_CLAUSE`
    - `SatisfiedCandidate`
    - `Break/Rewrite = 1/2`
    - `SINGLE_CLAUSE + Share = 100%`
    - 当前仍是 `ReviewRequired + ReadyForReview`
    - 命中后统一改落：
      - `Satisfied + ReadyForPromotion`
  - 当前已真实收掉：
    - `T3-2GL-5 / 10 / 50 / 51 / 52 / 55 / 56 / 57`
  - 当前 keep-out 继续保持不变：
    - `T3-2GKL-3`
    - `T3-2GL-9`
  - 下一轮主线应继续转向：
    - `T3-2GKL-3` 为何仍应保留人工复核
    - `T3-2GL-9` 这类 `BuiltUpH` satisfied-side 是否需要单独规则
    - `STANDARD_IH` 最后 `6` 个保守桶
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v70](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v70) 把 `T3-2GL-31` 接回折板/拼接翼缘 `H` 主链：
  - [CoreBodyProofEngine.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/CoreBodyProofEngine.cs>) 的 `ResolveBentFlangeHPartIds(...)` 已新增更窄的 review-翼缘变体入口：
    - 只允许 `2 x BodyCandidate + 1 x SpecialShape`
    - 只允许全 cohort 保持开口 `H`，不允许转成 `BOX`
    - 只允许 `SpecialShape` 作为不承担 envelope support 的折板/拼接翼缘 review 件并入
  - 当前 `v70` 已确认：
    - `T3-2GL-31` 起出：
      - `H_MAIN_CONTOUR_PROOF`
      - `H_WEB_FLANGE_PROOF_TARGET`
      - `H_WEB_FLANGE_CONTINUITY_CLAUSE`
    - 其 effect 组合稳定为：
      - `2 x BREAK + 1 x REWRITE`
  - 当前下一步不要把这条规则扩散到 `T3-2GL-66 / 70 / 103 / 76`，仍保持保守
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v71](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v71) 补上 `StandardSection / STANDARD_BOX` 单件直达链：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>) 已新增 `source-standard BOX + 单 core part + 无 review part` 的阶段 5/6 直达旁路
  - 当前 `v71` 已确认把以下 `13` 个 `YPGL` 全部从 `NO_CLAUSE_ROWS + ReviewRequired` 收到：
    - `Satisfied + ReadyForPromotion`
    - `T3-2YPGL-1 / 2 / 4 / 7 / 10 / 13 / 14 / 16 / 19 / 20 / 22 / 24 / 25`
  - 当前这说明：
    - 这批样本早已是明确的 `StandardSection / STANDARD_BOX`
    - 之前真正缺的是 `stage 5/6` 的 `source-standard BOX` 单件直达语义链
  - 当前下一步主线应从 `YPGL` 主桶撤出，转回：
    - 普通 `GL`
    - `MJ`
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v73](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v73) 把 `T3-2GL-11 / 17 / 46` 接回折板 `H` 主链：
  - [CoreBodyProofEngine.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/CoreBodyProofEngine.cs>) 已在 `ResolveBentFlangeHPartIds(...)` 新增两档更窄的折板 `H` 入口：
    - `纯开口 + 对称双主体板`
    - `弱单站位闭环 + review 翼缘不承担 envelope support`
  - 当前 `v73` 已确认：
    - `T3-2GL-11 / 17 / 46`
    - 已统一达到：
      - `H_MAIN_CONTOUR_PROOF`
      - `H_WEB_FLANGE_PROOF_TARGET`
      - `H_WEB_FLANGE_CONTINUITY_CLAUSE`
      - `Broken + ReadyForReview`
  - 当前同时确认：
    - `T3-2GL-66 / 70 / 103 / 76`
    - 仍保持保守，不随本轮放宽进入折板 `H` 主链
  - 当前关于 `MJ` 的执行口径按用户冻结：
    - `T3-2MJ-2 / 10 / 12` 先保留在当前复核状态，不作为本轮主线继续追打
  - 这一步当前只改了：
    - source-summary / explanation 家族语义
  - 这一步当前没有改：
    - recognition console 里 `result.Body.BodyFamily`
    - 阶段 5 synthetic `H / BOX` proof-chain 本体
  - 当前下一轮主线应继续收紧成：
    - 是否要把这层“source standard section”语义继续接入阶段 5/6 的 family lane，而不只是 summary
    - 以及 `GL-31 / 66 / 70 / 103 / 76` 这类 remaining variant/irregular 边界，应归为标准型材变体还是继续保守留在当前 proof 主线
- `2026-04-24` 已在 [run_body_bracket_real_04_definition_clause_v69](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v69) 把这层 source 语义继续接进阶段 5/6 的 full-run source：
  - [DefinitionClauseDecisionFullRunSourceModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceModels.cs>) 已新增：
    - `SourceSemanticBodyFamily`
    - `SourceSemanticSectionType`
    - `SourceSemanticPriorityApplied`
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>) 已把 `SourceMainPartProfileString` 的标准截面语义直接映射到 assembly / representative-part lane
  - 当前 `v69` 真实结果：
    - stage 5/6 assembly lane 已明确覆盖：
      - `84` 个 `STANDARD_IH`
      - `34` 个 `STANDARD_ANGLE`
      - `13` 个 `STANDARD_BOX`
      - `7` 个 `STANDARD_ROD`
    - representative-part lane 也已同步带出同一组 source semantic 字段
  - 当前下一轮主线应继续收紧成：
    - 这些 `SourceSemantic*` 字段是否进一步接入 verdict/readiness gate 或阶段 6 family classifier
    - 以及 remaining variant 边界样本：
      - `T3-2GL-31`
      - `T3-2GL-66`
      - `T3-2GL-70`
      - `T3-2GL-103`
      - `T3-2GL-76`
      - 应归入标准型材变体、source semantic + review，还是继续停在当前 built-up / irregular lane
- `2026-04-23` 已在 [run_body_bracket_real_04_definition_clause_v55](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v55) 把 `T3-2GL-51` 接进“折型翼缘 H 截面”小类 proof-chain：
  - 当前已确认这类不是 `BOX`，也不应掉成纯 `SpecialShape` review
  - 当前窄 fallback 会把这类样本统一起到：
    - `H_MAIN_CONTOUR_PROOF`
    - `H_WEB_FLANGE_PROOF_TARGET`
    - `H_WEB_FLANGE_CONTINUITY_CLAUSE`
  - `T3-2GL-51` 当前已成功起链，并已出现：
    - 腹板件 `H_WEB_FLANGE_CONTINUITY_BREAK_EFFECT`
    - 折型翼缘件 `H_WEB_FLANGE_CONTINUITY_REWRITE_EFFECT`
- `2026-04-23` 已继续把真实 `full-run source` 的构件级聚合口径收紧到“优先 own-clause rows”：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [run_body_bracket_real_01_definition_clause_v41](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_definition_clause_v41)
  - 当前 `GKZ` 在真实工件里已重新分化成 `条款满足 / 可进入阶段 6 提升` 与 `仍需复核 / 可进入定义复核`，不再被无条款旁路件整体拖回伪 `Mixed`。
  - 当前 `GL` 在这批 `real_01` 样本里仍主要停在 `证据不足 / 仅保留 effect 提示`，少量进入 `条款混合` 或 `仍需复核`，说明下一轮重点应放在真实弱信号门槛而不是聚合噪声。
- `2026-04-22` 已补上 effect adapter 的最小夹具链：
  - [DefinitionClauseDecisionBridgeEffectAdapterFixtures.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectAdapterFixtures.cs>)
  - [DefinitionClauseDecisionBridgeEffectAdapterFixtureRunner.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectAdapterFixtureRunner.cs>)
  - [DefinitionClauseDecisionBridgeEffectAdapterFixtureReportBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectAdapterFixtureReportBuilder.cs>)
  - 当前 `EffectSnapshot -> tier -> verdict -> readiness` 主链也已经有最小回归入口。
- `2026-04-22` 已补上 effect adapter 的 artifact/workflow 层：
  - [DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactModels.cs>)
  - [DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactBuilder.cs>)
  - [DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactSerializer.cs>)
  - [DefinitionClauseDecisionBridgeEffectAdapterFixtureWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeEffectAdapterFixtureWorkflow.cs>)
  - [DEFINITION_CLAUSE_DECISION_BRIDGE_EFFECT_ADAPTER_FIXTURE_WORKFLOW.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_BRIDGE_EFFECT_ADAPTER_FIXTURE_WORKFLOW.zh-CN.md>)
  - 当前 effect adapter 主链已经能独立落 JSON/Markdown 工件。
- `2026-04-22` 已新增统一 bridge validation bundle：
  - [DefinitionClauseDecisionBridgeValidationBundleWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionBridgeValidationBundleWorkflow.cs>)
  - [DEFINITION_CLAUSE_DECISION_BRIDGE_VALIDATION_BUNDLE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_BRIDGE_VALIDATION_BUNDLE.zh-CN.md>)
  - 当前 mapper fixtures 与 effect-adapter fixtures 已能先统一导出到一个 bundle 目录。
- `2026-04-22` 已收紧 bridge mapper 的 tier 语义：
  - [DefinitionClauseDecisionBridgeTierModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeTierModels.cs>)
  - [DefinitionClauseDecisionBridgeMapper.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapper.cs>)
  - 当前已补上 `TopologyTier=None`，并把 `ProofCompletenessTier` 的 `TypeOnly / TargetOnly` 拆成独立值，避免半完整证明在工件层被混淆。
- `2026-04-22` 已把语义收紧后的关键回归点落进夹具：
  - [DefinitionClauseDecisionBridgeMapperFixtures.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeMapperFixtures.cs>)
  - [DefinitionClauseDecisionBridgeEffectAdapterFixtures.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeEffectAdapterFixtures.cs>)
  - 当前已新增 `SYNTHETIC_NONE_TOPOLOGY` 与 `SYNTHETIC_TARGET_ONLY_REVIEW` 两组样本。

### 已完成

- 主材识别基座问题已重新分析。
- 已形成重构方案文档：[MAIN_BODY_FOUNDATION_REBUILD_PLAN.zh-CN.md](</I:/autoteklasuanfa/MAIN_BODY_FOUNDATION_REBUILD_PLAN.zh-CN.md>)
- 已完成阶段 0 定义冻结。
- 已完成阶段 1 输出契约首版落地。
- 已完成阶段 3 稳定区与站位求解首版原型。
- 已完成阶段 4 横截面迹线重建首版最小原型。
- 已完成阶段 4.5 `trace cleaning + topology` 首版旁路。
- 已启动阶段 5 主体核心证明器 bootstrap。
- 已完成阶段 5 bootstrap 的“扰动指标”扩展。
- 已完成 `GKZ` 主壁板 peer-group promotion 首版收敛。
- 已完成阶段 5 的 `remove-and-recompute` 站位退化首版过渡版。
- 当前离线结果已能额外导出：
  - 构件编号
  - 装配编号
  - 真实主材种子零件编号
  - 当前启发式核心件
  - 双视图主体候选分层
  - 双视图稳定区 / 站位
  - 双视图横截面迹线
  - 双视图迹线清洗结果
  - 双视图拓扑分析结果
  - 双视图主体核心 bootstrap 证明结果
  - `section-topology-summary.json`
- 已在真实样本目录验证：
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v7`
  - `I:\autoteklasuanfa\.tmpresults\section-trace-stage4-smoke`
  - `I:\autoteklasuanfa\.tmpresults\section-trace-stage4-smoke-v6`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v10`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v11`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v14`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v16`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v17`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v18`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v19`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v20`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v21`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v22`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v24`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v25`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v26`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v27`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v28`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v29`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v30`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v31`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v32`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v33`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v34`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v35`
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_04_contract_v36`

### 当前不可靠点

- `H / BOX / T` 仍然来自启发式板链投票。
- 合成主体仍深度参与主体判定。
- 主体核心仍未完成真正的“去除重算”证明，目前只进入 bootstrap。
- 阶段 4 / 4.5 虽已落地最小旁路，但 `closed-loop candidate` 误报半径与 `internal trace` 边界仍需继续收敛。
- `recognition_input` 视图下，合成主体样本仍可能由虚拟件吞掉真实主导迹线。
- 阶段 2 / 3 对边界样本的覆盖率虽然已在 `GL / HXZ / GKZ` 上完成多轮复核，但 `GL / MJ / YPGL` 这类混合改写样本仍需继续确认“稳定主体覆盖”与“控制结构改写提示”的边界。

### 当前执行快照

- 阶段 2：
  - `BodyCandidatePartitioner` 已支持双视图输出。
  - 已修正：
    - 零几何 `PolyBeam` 不再污染主体长度估计；
    - `inputMainPart` 所在连通分量之外的远端同 profile 长板，不再混入当前构件主体候选。
- 阶段 3：
  - `StableBodyZoneResolver` 与 `SectionStationPlanner` 已跑通首版。
  - `T2-3GL-22 / T2-3GL-53` 的 `real_input` 视图下，优先站位 `Priority Body Coverage` 已从 `0.00` 回到 `1.00`。
- 阶段 4：
  - `SectionTraceExtractor` 已落地并接入离线批处理。
  - `SectionTraceCleaner` 与 `SectionTopologyAnalyzer` 已完成首版旁路。
  - `GKZ / HXZ / GL` 样本已能输出 `retained / suppressed / outer envelope / internal trace / closed-loop candidate` 这几类证据。
- 阶段 5：
  - `CoreBodyProofEngine` 已启动 bootstrap。
  - 当前已能基于 `priority station 持续性 + 外包络支撑` 提取首版 `core / accessory / review` 零件集合。
  - 当前已新增扰动指标：
    - `PriorityBodyCoverageAfterRemovalRatio`
    - `ClosedLoopStationCountAfterRemoval`
    - `BodyWidthRetentionRatio`
    - `RemovalImpactScore`
  - `T3-1HXZ-1` 已出现更合理的结构化拆分：
    - `29616986` 因“输入主件 + 扰动证据命中”回到 `CoreBodyPartIds`
    - 四块 `PL30*40` 暂留在 `ReviewPartIds`
  - `GKZ` 柱当前已从“只有输入主件进 core”收敛到“四块主壁板一起进 core”，说明 `trace` 长边修正和 grouped peer promotion 已打通。
  - 当前 `remove-and-recompute` 已能输出：
    - `PriorityStationsWithEnvelopeBodyCandidateAfterRemoval`
    - `EnvelopeBodyCoverageAfterRemovalRatio`
    - `LostBodyCoverageStationIds`
    - `LostEnvelopeSupportStationIds`
    - `LostClosedLoopStationIds`
    - `TopologyRewriteStationIds`
    - `DominantDimensionSwitchStationIds`
  - `v18` 已把阶段 5 摘要收敛到更干净的 review 基线，不再被“单主件去掉后当然全空”的样本刷屏。
  - `v20` 已把 `DominantDimensionSwitchStationIds` 收紧成“剩余结构内部主导关系改写”信号，不再把“删除当前主导板导致主导自然轮换”计作切换。
  - `v21` 已把阶段 5 摘要改成优先展示 `LostClosedLoop / TopologyRewrite` 命中样本。
  - `v22` 已把 `TopologyRewriteStationIds` 从“任何包络变化”收紧到“未被 lost-coverage / lost-envelope / lost-closed-loop 覆盖的补充证据”。
  - `v24` 已把 `TopologyRewriteStationIds` 再收紧到“工程意义门槛后”的版本：只有基线站位本身具备 `closed-loop` 或至少 `3` 条主体控制迹线时，才会记成主截面改写；命中零件数已从 `v22` 的 `187` 降到 `v24` 的 `66`。
  - `v25` 已把“主截面改写样本”单独挂到 `core-body-proof-summary.zh-CN.md` 里，当前 `GKZ / HXZ` 的命中样本可以不经 `LostClosedLoop` 大表直接 review。
  - `v26` 已把 `TopologyRewrite` 拆成三类结构化原因：`SpanYRewrite / SpanZRewrite / EnvelopeControllerSwitch`。
  - `v27` 已新增独立摘要 `topology-rewrite-summary.zh-CN.md`，可以直接查看构件级聚合和代表零件的改写原因。
  - `v28` 已把 `TopologyRewritePatternCode / TopologyRewritePatternLabelZh` 固化进结果模型，当前 `GKZ` 已稳定收敛成 `SPAN_Y_PLUS_CONTROLLER`，`HXZ` 已稳定收敛成 `SPAN_Z_PLUS_CONTROLLER`。
  - `v29` 已把 `TopologyRewrite` 摘要正式接进工作流：`Run-OfflineRecognition.ps1 / Build-ReviewArtifacts.ps1 / Publish-TestablePackage.ps1` 现在都能带上 `topology-rewrite-summary.zh-CN.md`。
  - [Build-TopologyRewriteSummary.ps1](</I:/autoteklasuanfa/tools/Build-TopologyRewriteSummary.ps1>) 已重写成 WinPS 兼容版本，修掉了 `UTF-8 / Markdown 反引号 / Sort-Object / Measure-Object` 这几类脚本链问题。
  - `v29 / v30` 已把“空 `priority-bracket-answers.p1.csv` 直接打断 review/打包链”的问题改成“空则跳过 P1 / BuiltUpT 小包”，因此新模型结果也能稳定走完整条 review 与交付流程。
  - `v31` 已把 `ControllerRole` 固化进阶段 5 结果模型：`TopologyRewriteDirectControllerStationIds / IndirectControllerStationIds / ControllerRoleCode / LabelZh` 已能稳定回答“被删零件是不是主包络控制件”。
  - `v32` 已继续补上“主体板组角色”层：`TopologyRewriteCohortPartIds / CohortPartCount / ShapeRoleCode / ShapeRoleLabelZh` 已能把 `GKZ` 稳定解释成“成对主体板改写”，把 `HXZ` 稳定解释成“单主板改写”。
  - `v32` 的 `topology-rewrite-summary.zh-CN.md` 与 `core-body-proof-summary.zh-CN.md` 现在都能直接展开 `ShapeRole + CohortParts`，不再只停在 `Y/Z` 轴向模式标签。
  - `v33` 已继续补上“家族证明风险提示”层：`TopologyRewriteFamilyRiskCode / TopologyRewriteFamilyRiskLabelZh` 已能把 `GKZ` 稳定解释成“成对主壁板系统改写风险”，把 `HXZ` 稳定解释成“单主板系统改写风险”。
  - `v33` 的 `topology-rewrite-summary.zh-CN.md` 与 `core-body-proof-summary.zh-CN.md` 现在都已能同时展示 `Pattern + ControllerRole + ShapeRole + FamilyRisk`，阶段 5 已开始从“控制结构变化”进入“更接近家族证明风险”的解释层。
  - `v33` 结果已同步进入交付包链：`tekla-body-bracket-testable-v33-familyrisk` 现已能在包内直接复核 `FamilyRisk`，说明这层不是只停留在工作区实验结果里。
  - `v34` 已继续补上“主轮廓证明对象”层：`TopologyRewriteProofTypeCode / TopologyRewriteProofTypeLabelZh` 已能把 `GKZ` 稳定解释成“成对主壁板主轮廓证明”，把 `HXZ` 稳定解释成“单主板主轮廓证明”。
  - `v34` 的 `topology-rewrite-summary.zh-CN.md` 与 `core-body-proof-summary.zh-CN.md` 现在都已能同时展示 `Pattern + ControllerRole + ShapeRole + FamilyRisk + ProofType`，阶段 5 已开始从“风险提示”继续推进到“更像哪一类主轮廓证明对象被改写”的解释层。
  - `v35` 已继续补上“家族证明目标”层：`TopologyRewriteFamilyProofTargetCode / TopologyRewriteFamilyProofTargetLabelZh` 已能把 `GKZ` 稳定解释成“箱型对壁闭合证明目标”，把 `HXZ` 保守解释成“单主板体系待定目标”。
  - `v35` 的 `topology-rewrite-summary.zh-CN.md` 与 `core-body-proof-summary.zh-CN.md` 现在都已能同时展示 `Pattern + ControllerRole + ShapeRole + FamilyRisk + ProofType + FamilyProofTarget`，阶段 5 已开始从“证明对象”继续推进到“下一步最该拿哪类家族定义去验证”的解释层。
  - `DefinitionClause` 已正式接入阶段 5 结果模型与摘要链：`TopologyRewriteDefinitionClauseCode / TopologyRewriteDefinitionClauseLabelZh` 现在会把改写证据继续压到“更像哪条定义条款正在被触发 / 破坏”的提示层。
  - 已在 `section-trace-stage5-definition-clause-smoke-v2` 验证：`topology-rewrite-summary.zh-CN.md` 与 `core-body-proof-summary.zh-CN.md` 现在都已能同时展示 `Pattern + ControllerRole + ShapeRole + FamilyRisk + ProofType + FamilyProofTarget + DefinitionClause`。
  - 当前 smoke 里：
    - `GKZ` 已稳定命中 `箱型：闭合/对边稳定条款`
    - `GL` 已出现 `主体板：多数站位持续性条款 / 主板：多数站位持续性条款 / 箱型：闭合/对边稳定条款` 的分化提示
    - `HXZ` 当时仍待下一轮 full-run 复核
  - `v36` 已完成 `DefinitionClause` 的 full-run 复核：
    - `v35 -> v36` 在 `205` 个 assembly 上，`bracket total / positive assemblies / max bracket count / body family changes` 全部保持不变
    - `run-comparison.md / body-change-summary.md / review-queue.csv / priority-review-pack.md` 均已正常产出，且 `body-change-summary.md` 仍为 `0` 组变化
    - 当前命中 `TopologyRewrite` 的 `GKZ` 样本已稳定落在 `箱型：闭合/对边稳定条款`
    - 当前命中 `TopologyRewrite` 的 `HXZ` 样本也已在 full-run 下稳定落在 `主板：多数站位持续性条款`
    - `review-queue.csv` 仍只保留 `6` 个 `BuiltUpH` 正例，全部来自 `GKZ`，说明阶段 5 的解释层扩展没有打坏现有 review 链
  - 已在 `v36` 摘要链补上“主条款稳定度 / 混合度”：
    - `Program.cs` 与 `Build-TopologyRewriteSummary.ps1` 现在都会额外输出 `LeadClause / LeadClauseShare / ClauseMix`
    - `MJ` 已能直接表现为 `24/24 (100%) / 单条款稳定`
    - `GL` 已能直接表现为 `4/6 (67%) / 主条款占优，但仍属混合改写`
    - `YPGL` 已能直接表现为 `4/6 (67%) / 主条款占优，但仍有未定零件`
    - `HXZ / GKZ` 这类已落稳样本则会继续表现为 `100% / 单条款稳定`

### 脚本链稳定性补充

- 已继续收敛 `v36` 的脚本链与包链稳定性：
  - [Build-TopologyRewriteSummary.ps1](</I:/autoteklasuanfa/tools/Build-TopologyRewriteSummary.ps1>) 已切到 WinPS 可稳定解析的带 BOM UTF-8 版本，不再因为 `DefinitionClause` 中文文案触发 `powershell.exe -File` 解析失败。
  - [Smoke-TestReviewFlows.ps1](</I:/autoteklasuanfa/tools/Smoke-TestReviewFlows.ps1>) 已改成“按实际存在且非空的答案表逐个跑”，不再把缺失或空的 `priority-bracket-answers.p1.csv / priority-bracket-answers.p1.builtupt-first.csv` 误判成整包失败。
- 这一步不改变 `v36` 的主体 / 牛腿结果，但会让后续 `real_04 / v36` 一类没有旧 `P1 / BuiltUpT` 子流输入的包，也能继续把 `DefinitionClause` 结果当作主 review 入口，而不会被历史 smoke gate 卡住。

### 当前下一步

1. 深化阶段 5：
   - 已在 [run_body_bracket_real_04_definition_clause_v42](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v42) 确认当前真实输入确实覆盖 `MJ / YPGL`，下一步不再花时间重复找样本，而是直接围绕已落出来的真实 `tier / verdict / readiness / conflict-reasons` 继续收紧判据
   - 已在 [run_body_bracket_real_04_definition_clause_v43](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v43) 把“结构破坏已成立但条款锚点不足”的样本从 `HoldEffectOnly` 抬到 `ReadyForReview`；下一步继续判断这些 `ReviewRequired` 中哪些已足够再升级成 `Broken`
   - 已在 [run_body_bracket_real_04_definition_clause_v44](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v44) 把 `ReviewRequired` 进一步拆成“缺条款锚点 / 缺完整证明 / 纯边界复核”三类解释原因，下一步直接按桶推进，不再把所有复核样本混为一类
   - 已在 [run_body_bracket_real_04_definition_clause_v45](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v45) 再把“缺条款锚点”细分成 `BROKEN_STRUCTURE_NEEDS_CLAUSE_ANCHOR` 与 `BROKEN_STRUCTURE_CAN_BORROW_LEAD_CLAUSE_ANCHOR`；下一步优先只围绕后者这 `7` 条候选判断是否可安全上推到 `Broken`
   - 已在 [run_body_bracket_real_04_definition_clause_v47](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v47) 把这 `7` 条 `CAN_BORROW_LEAD_CLAUSE_ANCHOR` 候选正式上推到 `Broken`，但 readiness 仍保守保持 `ReadyForReview`
   - 已确认 `GL` 剩余主桶当前没有更多可安全借用 assembly 主条款的候选；下一步不要再在 `GL` 上重复试同一类 borrow-anchor gate，而应转向“如何先稳定 `LeadClause` 本身”
   - 已在 [run_body_bracket_real_04_definition_clause_v48](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v48) 吃掉第一批真实 `GL` lead-clause 不稳定装配：`T3-2GL-15 / 20 / 97` 已从 `条款混合 / 仅保留 effect 提示` 收紧到 `条款满足 / 可进入阶段 6 提升`
   - 已在 [run_body_bracket_real_04_definition_clause_v49](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v49) 把 `MJ / YPGL` 中误混入 `BOUNDARY_REVIEW` 的弱 side-row 旁路件拆成 `SIDE_ROW_PERSISTENCE_REVIEW`，assembly 级落点保持不变
   - 已在 [run_body_bracket_real_04_definition_clause_v50](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v50) 把最大非 `GL` 主桶进一步从 `BROKEN_STRUCTURE_NEEDS_CLAUSE_ANCHOR` 收紧到 `BROKEN_STRUCTURE_NEEDS_PROOF_CHAIN`
   - 下一轮 `GL` 主线应继续区分两类剩余样本：
     - `ProofType / FamilyProofTarget / DefinitionClause` 全空的 `NO_CLAUSE_ROWS` 装配
     - 仍可能存在但尚未被当前窄规则覆盖的其它 `shadow-primary / clause-mix` 装配
   - 继续围绕 `GL` 这类当前主要停在 `证据不足 / effect-only` 的样本，收紧真实弱信号门槛，区分“应继续保守忽略”和“已经足够进入 mixed/review”的边界
   - 继续围绕 `MJ / YPGL` 这两类当前已出现 `证据不足 / 复核 / 破坏 / 满足` 真实分化的样本，优先解释为什么有些构件会进入 `ReadyForPromotion`，而更多构件仍停在 `HoldEffectOnly / ReadyForReview`
   - 先按 [DEFINITION_CLAUSE_DECISION_EVIDENCE_PRECEDENCE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_EVIDENCE_PRECEDENCE.zh-CN.md>) 继续收紧真实 `EffectSnapshot` 的冲突判据，避免 `GL / MJ / YPGL` 只停在宽泛 mixed/review 标签而没有明确原因
   - 优先把 `CLAUSE_MIX + LEAD_CLAUSE_NOT_STABLE_FULL`、`BOUNDARY_REVIEW`、`PART_CLAUSE_DIFFERS_FROM_LEAD` 这三类在 `real_04 / v42` 已稳定暴露出来的真实冲突，进一步压成更可解释的阶段 5 -> 阶段 6 升级门槛
   - 先按 [DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_VERDICT_GATE.zh-CN.md>) 继续判断哪些 `v37` effect 结果已经足够升级成定义判定、哪些仍应保守停在 `HoldEffectOnly / ReadyForReview`
   - 优先拆清 `real_04 / v43` 里这三类最典型升级边界：
     - `GL` 中为什么有一大批从 `HoldEffectOnly` 抬到了 `ReadyForReview`
     - `MJ / YPGL` 中哪些 `ReviewRequired` 已具备 `Broken` 所需的稳定 `LeadClause / ProofCompleteness`
     - 哪些样本虽然已有真实结构破坏，但仍只能停在“结构强、定义弱”的复核层
   - 当前优先按 `v44` 新暴露出的两大主桶继续推进：
     - `BROKEN_STRUCTURE_NEEDS_CLAUSE_ANCHOR`
     - `BOUNDARY_REVIEW`
- 当前关于 `BROKEN_STRUCTURE_NEEDS_CLAUSE_ANCHOR` 的最新更细结论是：
  - `MJ / YPGL` 里最大的非 `GL` 主桶其实更准确是 `BROKEN_STRUCTURE_NEEDS_PROOF_CHAIN`
  - 也就是结构破坏已经成立，但 upstream 还没有把 `ProofType / FamilyProofTarget / DefinitionClause` 真正挂起来
  - 所以下一轮优先级应继续前移到 proof-chain 的生成与收敛，而不是继续在下游 mapper/readiness gate 上硬拆
- 当前关于这一步的最新更细结论是：
  - `MJ / MQMJ` 的“单主板 + 锚筋/附属杆件”典型埋件类已经被窄 bootstrap 吃到
  - `YPGL` 中：
    - `BodyCandidate` 形态清晰的闭合箱壁壳已被窄 bootstrap 吃到
    - `闭合箱壳 + 折板翼缘` 小类也已被窄 bootstrap 吃到
  - `GL` 中：
    - `折型翼缘 H 截面` 小类也已被窄 bootstrap 吃到
  - 下一轮若继续推进 `YPGL / BOX`，重点不再是 proof-chain 起不来，而应转向：
    - 这些 `BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE` 行何时足够升级成 `Broken / Satisfied / ReviewRequired`
    - 以及折板翼缘小类是否需要单独的 `review hint / subtype` 命名，而不只是共享 `BOX` 主链
  - 下一轮若继续推进 `GL / H`，重点也不再是 proof-chain 起不来，而应转向：
    - `H_WEB_FLANGE_CONTINUITY_CLAUSE` 怎样接入 `ClauseVerdict / PromotionReadiness`
    - 以及 `H` 主链是否需要把“腹板破坏”与“折型翼缘仅改写”进一步分成更稳定的 assembly 级聚合规则
   - 当前关于 `BOUNDARY_REVIEW` 的最新更细结论是：
     - `MJ-10 / MJ-12` 这类 `CLUSTER_DIRECT_CONTROL_EXCLUSION_CLAUSE` 行仍属于真正的定义边界复核
     - `MJ-13 / YPGL-12 / 17 / 23 / 5` 这类 `NONE / NONE / NONE + INSUFFICIENT_PERSISTENT_PROOF` 的旁路件不再应与前者混为同一桶
   - 当前新增更小、更安全的优先候选桶：
     - `BROKEN_STRUCTURE_CAN_BORROW_LEAD_CLAUSE_ANCHOR`
   - 先判断 `BROKEN_STRUCTURE_NEEDS_CLAUSE_ANCHOR` 能否继续拆成“可从 assembly lead-clause 借锚”与“必须继续停在复核”的两档，再决定是否把其中一部分从 `ReviewRequired` 推到 `Broken`
   - 现已完成这一步验证：当 assembly 内已存在 `LeadClause=100% + sibling broken row + 当前 row 仅缺 own-clause` 时，允许把该 row 从 `ReviewRequired` 推到 `Broken`，但 readiness 仍保守保持 `ReadyForReview`
   - 下一轮优先转向剩余大盘：
     - `BROKEN_STRUCTURE_NEEDS_CLAUSE_ANCHOR`
     - `BOUNDARY_REVIEW`
   - 重点不再是这 `7` 条窄候选，而是判断剩余 `GL / MJ / YPGL` 大盘里，哪些还可以继续从 assembly 主条款借锚，哪些必须长期保守停在复核层
  - 当前关于这一步的最新结论是：
     - `MJ / YPGL` 的 borrow-anchor 窄候选已基本吃干净
     - `GL` 暂时没有更多 borrow-anchor 候选
     - 所以下一轮更值得把时间投到 `GL` 的 lead-clause 稳定化，而不是继续扩张 broken-promotion gate
  - 当前关于 `GL` 折型翼缘 `H` 小类的最新更细结论是：
    - `T3-2GL-50 / 51 / 52 / 53 / 54 / 55 / 56 / 57` 已经进入：
      - `H_MAIN_CONTOUR_PROOF`
      - `H_WEB_FLANGE_PROOF_TARGET`
      - `H_WEB_FLANGE_CONTINUITY_CLAUSE`
    - `T3-2GL-53 / 54 / 55` 已在 `v56` 真实 full-run 中确认完成接线，不再属于“proof-chain 尚未挂起”的待验证桶
    - `T3-2GL-5 / 9 / 10 / 49 / 50 / 51 / 52 / 53 / 54 / 55 / 56 / 57` 与 `T3-2GKL-3` 已在 `v57` 真实 full-run 中继续从：
      - `InsufficientEvidence + HoldEffectOnly`
    - 收紧到：
      - `ReviewRequired + ReadyForReview`
    - 这说明当前 `H` 折型翼缘小类的 proof-chain 与定义复核闸门都已跑通；下一轮优先级不再是“能否进入 review”，而是继续判断：
      - 哪些样本已足够从 `ReviewRequired` 上推到 `Broken`
      - 哪些样本只是“折型翼缘改写但不破坏 H 主截面连续性”，更接近 `SatisfiedCandidate`
    - 当前 `H` 主链的最优先问题已进一步收紧为：
      - `H_WEB_FLANGE_CONTINUITY_BREAK_EFFECT`
      - 与 `H_WEB_FLANGE_CONTINUITY_REWRITE_EFFECT`
      - 在 assembly 聚合层何时可分化成更稳定的 `Broken / Satisfied / Mixed`
   - 在 `CoreBodyProofEngine` 上继续把 `TopologyRewrite` 从“定义条款提示”收紧到更接近“哪一种定义条款正在被满足 / 破坏”
   - 优先围绕 `GKZ / HXZ / GL / MJ / YPGL` 这几类当前仍命中 `TopologyRewrite` 的样本，补“对壁闭合 / 单主板持续性 / 主轮廓控制边 / 主导跨度 / 闭环控制件集合 / 条款满足门槛 / 证明对象边界 / 何时可从提示升级为定义判定”解释
   - 现阶段重点观察：`GKZ` 与 `HXZ` 的条款结果已经在真实 full-run 落稳，当前重点正式转向 `GL / MJ / YPGL` 为什么分别卡在 `LeadClauseNotStableFull`、`BoundaryReview`、`PartClauseDiffersFromLead`，以及哪些 `ReviewRequired` 已足够再上推为 `Broken / ReadyForPromotion`
2. 回补阶段 4.5 的误差控制：
   - 收紧 `closed-loop candidate` 的误报半径
   - 让 `internal trace` 与 `body_accessory` 的边界更稳
3. 当阶段 5 的证明输入稳定后，再推进阶段 6 定义驱动家族判定器

---

## 阶段 0：定义冻结

### 目标

把“主材 / 主体核心 / 主体附属 / H / BOX / T / CROSS / IRREGULAR”的定义从讨论稿变成可执行规范。

### 输入

- [MAIN_BODY_FOUNDATION_REBUILD_PLAN.zh-CN.md](</I:/autoteklasuanfa/MAIN_BODY_FOUNDATION_REBUILD_PLAN.zh-CN.md>)
- [tekla_body_bracket_algorithm_design.md](</I:/autoteklasuanfa/tekla_body_bracket_algorithm_design.md>)
- 当前典型误判样本

### 任务

1. 固化主材定义。
2. 固化主体核心零件定义。
3. 固化主体附属件定义。
4. 固化 H / BOX / T / CROSS / IRREGULAR 的必要条件与禁止条件。
5. 固化 `UNKNOWN / REVIEW_REQUIRED` 的触发条件。

### 产出

- `MAIN_BODY_DEFINITIONS.zh-CN.md`

### 验收

- 每种家族都能回答：
  - 必须满足什么
  - 不能出现什么
  - 证据来自什么
- 对 `T3-1HXZ-1`、`GKZ` 柱、普通焊接 H 梁，能用定义解释为什么判或不判。

### 风险

- 若定义过宽，会回到启发式。
- 若定义过窄，会导致大量 `UNKNOWN`。

---

## 最近增量

- `2026-04-22` 已新增真实结果接线契约与中间模型：
  - [DEFINITION_CLAUSE_DECISION_REAL_SOURCE_CONTRACT.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_REAL_SOURCE_CONTRACT.zh-CN.md>)
  - [DefinitionClauseDecisionFullRunSourceModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceModels.cs>)
- 该增量的目的不是再补 demo 脚手架，而是固定：
  - full-run 真实结果进入 `DefinitionClauseDecision` 旁路前，至少要保留哪些字段；
  - 哪些字段属于稳定证据，哪些字段只允许作为 review 辅助；
  - `AssemblySnapshot` 与 `RepresentativePartSnapshot` 的最小边界。
- 当前最近一步：
  - 优先把 `DefinitionClauseDecisionFullRunSourceModels` 接入 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 的 full-run 结果落盘点；
  - 先生成一版真实 `representative-part source`；
  - 再把它接进既有 `snapshot -> sidecar` 统一链。
- `2026-04-22` 已补齐真实 source artifact 层：
  - [DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_ARTIFACT_CONTRACT.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_ARTIFACT_CONTRACT.zh-CN.md>)
  - [DefinitionClauseDecisionFullRunSourceArtifactModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactModels.cs>)
  - [DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs>)
- 当前最近一步更新为：
  - 把 `DefinitionClauseDecisionFullRunSourceModels` 与 `DefinitionClauseDecisionFullRunSourceArtifactBuilder` 一起接到 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 的 full-run 落盘点；
  - 优先落出第一版真实：
  - `definition-clause-decision-fullrun-source.json`
  - `definition-clause-decision-fullrun-source.zh-CN.md`
  - 然后再把它接入既有 `snapshot -> sidecar` 统一链。
- `2026-04-22` 已补上 fullrun-source serializer 与 workflow：
  - [DefinitionClauseDecisionFullRunSourceArtifactSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactSerializer.cs>)
  - [DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_WORKFLOW.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_WORKFLOW.zh-CN.md>)
- 当前最近一步进一步收敛为：
  - 直接把 `DefinitionClauseDecisionFullRunSourceModels + ArtifactBuilder + ArtifactSerializer` 接到 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 的 full-run 结果落盘点；
  - 先产出第一版真实 `definition-clause-decision-fullrun-source.json/.md`；
  - 再沿既有 `snapshot -> sidecar` 统一链往下走。
- `2026-04-22` 已补上 fullrun-source 的统一导出服务：
  - [DefinitionClauseDecisionFullRunSourceExportResult.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceExportResult.cs>)
  - [DefinitionClauseDecisionFullRunSourceExportService.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceExportService.cs>)
  - [DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_EXPORT_QUICKSTART.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_EXPORT_QUICKSTART.zh-CN.md>)
- 当前最近一步进一步收敛为：
  - 把 `DefinitionClauseDecisionFullRunAssemblySource / RepresentativePartSource` 收集逻辑直接接到 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 的 full-run 主循环；
  - 主循环结束后调用 `DefinitionClauseDecisionFullRunSourceExportService.Export(...)`；
  - 先稳定产出第一版真实 `definition-clause-decision-fullrun-source.json/.md`，再接入既有 `snapshot -> sidecar` 统一链。
- `2026-04-22` 已补上 fullrun-source 的 validation 层：
  - [DefinitionClauseDecisionFullRunSourceValidationResult.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceValidationResult.cs>)
  - [DefinitionClauseDecisionFullRunSourceValidator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceValidator.cs>)
  - [DefinitionClauseDecisionFullRunSourceValidationReportBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceValidationReportBuilder.cs>)
  - [DefinitionClauseDecisionFullRunSourceValidationSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceValidationSerializer.cs>)
  - [DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_VALIDATION.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_VALIDATION.zh-CN.md>)
- 当前最近一步进一步收敛为：
  - 把 `DefinitionClauseDecisionFullRunAssemblySource / RepresentativePartSource` 收集逻辑接到 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 的 full-run 主循环；
  - 主循环结束后调用 `DefinitionClauseDecisionFullRunSourceExportService.Export(...)`；
  - 让第一版真实 `fullrun-source` 一次性产出：
  - `definition-clause-decision-fullrun-source.json/.md`
  - `definition-clause-decision-fullrun-source-validation.json/.md`
  - 再沿既有 `snapshot -> sidecar` 统一链继续推进。
- `2026-04-22` 已补上 fullrun-source 的 manifest / README 层：
  - [DefinitionClauseDecisionFullRunSourceManifest.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceManifest.cs>)
  - [DefinitionClauseDecisionFullRunSourceManifestSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceManifestSerializer.cs>)
  - [DefinitionClauseDecisionFullRunSourceReadmeBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceReadmeBuilder.cs>)
  - [DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_OUTPUT_README.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_OUTPUT_README.zh-CN.md>)
- 当前最近一步进一步收敛为：
  - 把 `DefinitionClauseDecisionFullRunAssemblySource / RepresentativePartSource` 收集逻辑接到 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 的 full-run 主循环；
  - 主循环结束后调用 `DefinitionClauseDecisionFullRunSourceExportService.Export(...)`；
  - 让第一版真实 `fullrun-source` 一次性产出：
    - `definition-clause-decision-fullrun-source.json/.md`
    - `definition-clause-decision-fullrun-source-validation.json/.md`
    - `definition-clause-decision-fullrun-source-manifest.json`
    - `README.md`
- `2026-04-22` 已把 fullrun-source 收成统一 workflow：
  - [DefinitionClauseDecisionFullRunSourceWorkflowResult.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceWorkflowResult.cs>)
  - [DefinitionClauseDecisionFullRunSourceWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceWorkflow.cs>)
  - [DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_WORKFLOW_ENTRY.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_WORKFLOW_ENTRY.zh-CN.md>)
- 当前最近一步进一步收敛为：
  - 在 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 主循环里收集 `DefinitionClauseDecisionFullRunAssemblySource / RepresentativePartSource`
  - 主循环结束后调一次 `DefinitionClauseDecisionFullRunSourceWorkflow.Run(...)`
  - 先稳定落出第一版真实 fullrun-source 全套工件，再把同一批 representative parts 投给既有 `snapshot -> sidecar` 统一链。
- `2026-04-22` 已固定 fullrun-source collector：
  - [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)
  - [DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_COLLECTOR.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_FULLRUN_SOURCE_COLLECTOR.zh-CN.md>)
- 当前最近一步进一步收敛为：
  - 在 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) full-run 主循环结束后，
    - 调 `CollectAssemblies(...)`
    - 调 `CollectRepresentativeParts(...)`
    - 调 `DefinitionClauseDecisionFullRunSourceWorkflow.Run(...)`
  - 先稳定落出第一版真实 fullrun-source 全套工件，再把同一批 representative parts 投给既有 `snapshot -> sidecar` 统一链。
- `2026-04-22` 已完成上述最薄接线：
  - [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 现在已在结果落盘前直接调用：
    - `DefinitionClauseDecisionFullRunSourceCollector.CollectAssemblies(...)`
    - `DefinitionClauseDecisionFullRunSourceCollector.CollectRepresentativeParts(...)`
    - `DefinitionClauseDecisionFullRunSourceWorkflow.Run(...)`
- 当前最近一步更新为：
  - 补一层真实 `fullrun-source -> snapshot` 适配，不再只停在导出 `fullrun-source` 工件；
  - 优先让已有真实 representative-part source 直接进入既有 `snapshot -> sidecar` 统一链。
- `2026-04-22` 已开始固定 `fullrun-source -> snapshot` 适配面：
  - [DEFINITION_CLAUSE_DECISION_FULLRUN_SNAPSHOT_BRIDGE.zh-CN.md](</I:/autoteklasuanfa/DEFINITION_CLAUSE_DECISION_FULLRUN_SNAPSHOT_BRIDGE.zh-CN.md>)
  - [DefinitionClauseDecisionFullRunSnapshotSeedModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSnapshotSeedModels.cs>)
- 当前最近一步进一步收敛为：
  - 先补 `fullrun-source -> snapshot seed` 的 bridge builder；
  - 再让真实 `fullrun-source representative parts` 进入既有 `snapshot -> sidecar` 统一链；
  - 暂不在这一步提前写死最终 `ClauseVerdict / ClausePromotionReadiness`。

---

## 阶段 1：输出契约重构

### 目标

先让系统输出足够的真实证据，保证后续重构可验证、可对照、可复核。

### 任务

1. 为主体识别新增“真实主材种子零件”输出。
2. 为主体识别新增“当前核心零件 vs 真实来源零件”映射输出。
3. 为每个样本新增“主体判定解释摘要”。
4. 区分：
   - `ImportSynthesisKind`
   - `BodyDescriptorFamily`
   - `DefinitionBodyFamily`（预留）

### 产出

- `body-main-material-source-seeds.csv`
- `body-main-material-explanation.json`
- 中文复核摘要文档

### 验收

- 任一主体样本都能直接看到：
  - 构件编号
  - 装配编号
  - 真实零件编号
  - 当前启发式核心件
  - 为什么进入主体候选

### 风险

- 如果仍保留大量只有虚拟件编号的输出，会继续误导分析。

---

## 阶段 2：主体候选零件分层器

### 目标

在进入主轴与截面分析前，先把零件分层，而不是一股脑投票。

### 任务

1. 新建“主体候选分层器”模块。
2. 对真实零件划分：
   - body_candidate
   - body_accessory_candidate
   - end_connection_candidate
   - local_stiffener_candidate
   - tiny_part
   - special_shape
3. 对每个零件输出进入该层的原因码。

### 推荐模块

- `BodyCandidatePartitioner`

### 产出

- `body-candidate-partition.json`

### 验收

- `T3-1HXZ-1` 这类样本里，耳板、端板、短局部件不再直接参与主体核心投票。
- `GKZ` 柱上的长向主板仍能进入主体候选池。

### 风险

- 过早排除会漏掉真实主材。
- 不分层会继续污染后续截面重建。

---

## 阶段 3：稳定区与截面站位求解器

### 目标

确定哪些长度区间可以用来判主体，不让端部复杂构造主导主材识别。

### 任务

1. 基于主轴投影确定总长度区间。
2. 自动剔除两端复杂区。
3. 标记局部构造密集区。
4. 在稳定区布置截面站位。

### 推荐模块

- `StableBodyZoneResolver`
- `SectionStationPlanner`

### 产出

- `stable-body-zone.json`
- `section-stations.json`

### 验收

- 端板、耳板、局部连接件密集区不进入主截面判定。
- 站位数量和位置可解释。

### 风险

- 稳定区过短会导致证据不足。
- 稳定区过宽会引入端部污染。

---

## 阶段 4：横截面迹线重建原型

### 目标

从“板链投票”转向“截面证据”。

### 当前进度

- 已完成首版最小原型：
  - `SectionTraceExtractor`
  - `section-traces-recognition-input.json`
  - `section-traces-real-input.json`
  - `section-topology-summary.json`
  - `section-trace-summary.zh-CN.md`
- 已完成阶段 4.5 首版旁路：
  - `SectionTraceCleaner`
  - `SectionTopologyAnalyzer`
  - `section-trace-cleaning-recognition-input.json`
  - `section-trace-cleaning-real-input.json`
  - `section-topology-recognition-input.json`
  - `section-topology-real-input.json`
  - `section-topology-analysis-summary.zh-CN.md`
- 当前未完成：
  - 真正的实体求交版迹线
  - 更稳的 `closed-loop` 规则
  - `SectionEnvelopeAnalyzer` 独立化

### 任务

1. 实现真实零件与截面平面的求交。
2. 生成每站位的零件迹线。
3. 清洗重合线、短噪声线、局部小线段。
4. 标记：
   - 外包络迹线
   - 内部迹线
   - 闭合候选关系
5. 保留“迹线 -> 真实零件”映射。

### 推荐模块

- `SectionTraceExtractor`
- `SectionTraceCleaner`
- `SectionEnvelopeAnalyzer`

### 产出

- `section-traces/*.json`
- `section-topology-summary.json`

### 验收

- 对普通焊接 H，能重建出 `腹板 + 上翼缘 + 下翼缘` 的主迹线关系。
- 对普通箱型，能看出 `四壁闭合或准闭合`。
- 对 `T3-1HXZ-1` 这类边界样本，能明确看出其主截面是否真的满足 H/BOX 定义。

### 风险

- 若迹线映射不到真实零件，后续全部失效。

---

## 阶段 5：主体核心证明器

### 目标

不再用“前 3/4 条强链”近似主体核心，而是通过“去除重算”证明主体核心。

### 任务

1. 为每个高持续性候选零件做去除试验。
2. 比较去除前后：
   - 主截面家族是否变化
   - 主轮廓是否变化
   - 闭合关系是否消失
   - 关键尺寸是否显著变化
3. 形成：
   - `core_body_parts`
   - `body_accessory_parts`

### 推荐模块

- `CoreBodyProofEngine`

### 产出

- `core-body-proof.json`

### 当前进度

- 已完成 bootstrap 版：
  - `CoreBodyProofEngine`
  - `core-body-proof-recognition-input.json`
  - `core-body-proof-real-input.json`
  - `core-body-proof-summary.zh-CN.md`
- 当前 bootstrap 只使用：
  - `priority station 持续性`
  - `外包络支撑`
  - `移除后的扰动指标`
- 当前未完成：
  - 真正的 `去除重算`
  - 去除前后主轮廓 / 闭合关系 / 关键尺寸对比

### 验收

- 任一核心零件都能回答“为什么它必须是主材”。
- 任一附属件都能回答“为什么去掉它主体家族不变”。

### 风险

- 计算量明显上升，需要先做典型站位抽样版。

---

## 阶段 6：定义驱动的家族判定器

### 目标

将 `H / BOX / T / CROSS / IRREGULAR` 从投票器迁移到定义判定器。

### 任务

1. 基于阶段 0 的严格定义，逐类实现判定器。
2. 每类判定器必须输出：
   - 满足了哪些条件
   - 哪些条件不满足
3. 支持 `UNKNOWN` 与 `REVIEW_REQUIRED`。

### 推荐模块

- `BodyFamilyDefinitionEvaluator`

### 产出

- `body-family-proof.json`

### 当前进展

- `2026-04-24` 已完成阶段 6 首版 `BodyFamilyDefinitionEvaluator` 与 `body-family-proof` sidecar：
  - 已新增 [BodyFamilyDefinitionEvaluator.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyDefinitionEvaluator.cs>)，按现有 `LeadClause / LeadClauseVerdict / LeadClausePromotionReadiness / SourceSemantic*` 做保守家族判定
  - 已新增 [BodyFamilyProofWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyFamilyProofWorkflow.cs>) 与整套 artifact/validation/export 链
  - 已在 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 接入真实 full-run 输出
  - 已在 [run_body_bracket_real_04_definition_clause_v79](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v79) 验证通过
  - 当前 `body-family-proof-validation.json` 为 `IsValid = true`
- 当前 `v79` 首版覆盖面：
  - `112 x STANDARD_SECTION`
  - `36 x BOX`
  - `25 x PRIMARY_PLATE_BODY`
  - `8 x H`
  - `24 x Deferred`
- `2026-04-24` 已完成阶段 6 首版后的第一条窄语义收口：
  - [run_body_bracket_real_04_definition_clause_v80](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v80)
  - 当前已确认：
    - `T3-2YPGL-3` 按“变截面闭合箱壳”进入 `BOX / VARIABLE_SECTION_BOX`
    - `T3-2MJ-11` 继续保守停在人工复核，不进入自动家族判定
  - 当前 `v80` 分布更新为：
    - `182 x Adjudicated`
    - `23 x Deferred`
    - `1 x VARIABLE_SECTION_BOX`
- 当前阶段 6 的下一步应继续收紧为：
  - 暂不补 `MJ` 的 `CLUSTER_DIRECT_CONTROL_EXCLUSION_CLAUSE` 自动映射，继续保守人工复核
  - 优先检查 `YPGL` 是否还有其它真实样本应并入 `VARIABLE_SECTION_BOX`
  - 明确保留 `H_WEB_FLANGE_CONTINUITY_CLAUSE + Broken + ReadyForReview` 这批折型翼缘 `H` 在 deferred，不把 review 桶误推成最终家族
- 当前阶段 6 主线收口口径已明确：
  - `182 x Adjudicated` 直接进入阶段 7 自动细分
  - `23 x Deferred` 直接人工复核旁路
  - 阶段 6 不再阻塞主线阶段 7

### 验收

- H 必须由多数稳定站位上的 `1 腹板 + 2 翼缘` 证明。
- BOX 必须由多数稳定站位上的 `四壁闭合/准闭合` 证明。
- 不满足定义时不能强判。

### 风险

- 若直接套定义、没有稳定区和截面证据支撑，会变成形式主义。

---

## 阶段 7：标准型材 / built-up / 变截面细分

### 目标

在家族确定后，再做更细的截面反推。

### 任务

1. 区分：
   - 标准型材
   - 规则 built-up
   - 变截面 built-up
   - 异形
2. 输出尺寸来源与相似度依据。

### 推荐模块

- `BodyProfileResolver`

### 产出

- `body-profile-resolution.json`

### 当前进展

- `2026-04-24` 已完成阶段 7 首版 `BodyProfileResolver` 与 `body-profile-resolution` sidecar：
  - 已新增 [BodyProfileResolver.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyProfileResolver.cs>)，按阶段 6 已 adjudicated 家族结果做自动细分
  - 已新增 [BodyProfileResolutionWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/BodyProfileResolutionWorkflow.cs>) 与整套 artifact/validation/export 链
  - 已在 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 接入真实 full-run 输出
  - 已在 [run_body_bracket_real_04_definition_clause_v81](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v81) 验证通过
  - 当前 [body-profile-resolution-validation.json](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_04_definition_clause_v81/body-profile-resolution-validation.json) 为 `IsValid = true`
- 当前 `v81` 首版分布：
  - `182 x Resolved`
  - `23 x ReviewBypass`
  - `112 x STANDARD_SECTION`
  - `69 x REGULAR_BUILT_UP`
  - `1 x VARIABLE_SECTION_BUILT_UP`
- 当前阶段 7 的下一步应继续收紧为：
  - 优先把 `REGULAR_BUILT_UP` 继续拆成更细的 `H / BOX / PRIMARY_PLATE / T` 细分编码
  - 优先给 `STANDARD_SECTION` 增补更细的尺寸簇/系列输出
  - 继续保持 `23 x ReviewBypass` 不回流干扰自动主线

### 验收

- 这一层不再反向干扰家族定义。

---

## 阶段 8：回归测试与基线替换

### 目标

把新基座接入现有离线工作流，逐步替换旧启发式主体识别。

### 任务

1. 选取代表样本集：
   - 标准焊接 H
   - 标准箱型
   - 焊接 T
   - `GKZ` 柱
   - `HXZ` 边界样本
   - 复杂异形
2. 为每类样本准备最小真值。
3. 新旧结果对比。
4. 若新基座通过最小验收，再切为默认主体识别路径。

### 产出

- `body-foundation-regression-report.md`

### 验收

- 主材核心识别比当前版更稳定、更可解释。
- `HXZ` 这类样本不再被草率判成 H 或 BOX。

---

## 优先级排序

### P0：必须先做

1. 阶段 0：定义冻结
2. 阶段 1：输出契约重构
3. 阶段 2：主体候选分层器

### P1：基座核心

4. 阶段 3：稳定区与截面站位求解器
5. 阶段 4：横截面迹线重建原型
6. 阶段 5：主体核心证明器

### P2：替换现有主体识别器

7. 阶段 6：定义驱动家族判定器
8. 阶段 7：标准型材 / built-up / 变截面细分
9. 阶段 8：回归测试与切换

---

## 当前自动迭代建议顺序

下一轮自动迭代，建议严格按以下顺序执行：

1. 先完成 `MAIN_BODY_DEFINITIONS.zh-CN.md`
2. 再补齐主体输出契约与真实零件映射
3. 再实现主体候选分层器
4. 在这三步稳定前，不再继续调 `BodyRecognizer` 的 H / BOX 阈值

---

## 停止条件

以下情况应暂停自动迭代，回到人工确认：

1. 对主材定义本身仍存在业务分歧。
2. 样本显示同一类构件在工程语义上并非单一家族。
3. 横截面重建无法稳定回到真实零件。
4. 输出证据链仍无法解释“为什么这块板是主材”。

---

## 最终目标

最终要从当前的：

- `启发式板链投票器`

切换到：

- `工程定义驱动的主体截面证明器`

只有完成这一步，主材识别、主材零件输出、主体附属剥离，才真正能作为后续牛腿识别、复杂度评估和工艺决策的可靠基座。
## 2026-04-21 ClauseVerdict 桥接补充

- 已新增桥接设计文档 [DEFINITION_CLAUSE_DECISION_BRIDGE_DESIGN.zh-CN.md](./DEFINITION_CLAUSE_DECISION_BRIDGE_DESIGN.zh-CN.md)，用于把 `DefinitionClauseEffect` 推进到 `ClauseVerdict / ClausePromotionReadiness`。
- 已新增独立 helper [DefinitionClauseDecisionBridge.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridge.cs)，先把桥接映射做成可接线的纯函数入口。
- 已新增预期样本清单 [DEFINITION_CLAUSE_DECISION_EXPECTED_CASES.zh-CN.md](./DEFINITION_CLAUSE_DECISION_EXPECTED_CASES.zh-CN.md)，用于约束 `GKZ / HXZ / MJ / GL / YPGL` 的首轮回归落点。
- 已新增聚合 helper [DefinitionClauseDecisionAggregate.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionAggregate.cs)，后续摘要层直接复用它聚合 `LeadClauseVerdict / LeadClausePromotionReadiness`。
- 已新增接线计划 [DEFINITION_CLAUSE_DECISION_WIRING_PLAN.zh-CN.md](./DEFINITION_CLAUSE_DECISION_WIRING_PLAN.zh-CN.md)，下一轮按“引擎 -> 结果模型 -> 摘要 -> 脚本 -> full-run”顺序接线。
- 已新增输出契约 [DEFINITION_CLAUSE_DECISION_OUTPUT_CONTRACT.zh-CN.md](./DEFINITION_CLAUSE_DECISION_OUTPUT_CONTRACT.zh-CN.md)，固定 `ClauseVerdict / ClausePromotionReadiness` 的字段名与中文文案。
- 已新增摘要模型 [DefinitionClauseDecisionSummaryModels.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSummaryModels.cs)，后续 `Program.cs` 直接按这组模型接线。
- 已新增呈现 helper [DefinitionClauseDecisionPresentation.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionPresentation.cs)，统一生成 review 提示和代表零件 Markdown 表。
- 已新增夹具集 [DefinitionClauseDecisionBridgeFixtures.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeFixtures.cs)，把 `GKZ / HXZ / MJ / GL / YPGL` 的预期落点固化成代码级样例。
- 已新增最小 runner [DefinitionClauseDecisionBridgeFixtureRunner.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeFixtureRunner.cs)，下一轮接线前后都先跑 fixture，再做 full-run。
- 已新增 sidecar 结果模型 [DefinitionClauseDecisionArtifactModels.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionArtifactModels.cs) 与构建器 [DefinitionClauseDecisionArtifactBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionArtifactBuilder.cs)，后续可先独立落 `definition-clause-decision-summary.zh-CN.md`，再逐步并回主摘要。
- 已新增 sidecar 摘要脚本 [Build-DefinitionClauseDecisionSummary.ps1](./tools/Build-DefinitionClauseDecisionSummary.ps1)，后续只要先落 JSON 就能独立重建 Markdown 摘要。
- 已新增 sidecar 契约 [DEFINITION_CLAUSE_DECISION_SIDECAR_CONTRACT.zh-CN.md](./DEFINITION_CLAUSE_DECISION_SIDECAR_CONTRACT.zh-CN.md)，固定 `RepresentativeRows / AggregateRows / ReviewRows` 三层结构。
- 已新增 sidecar serializer [DefinitionClauseDecisionSidecarSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSidecarSerializer.cs)，后续 sidecar JSON 可直接按统一编码落盘。
- 已新增 fixture 报告构建器 [DefinitionClauseDecisionBridgeFixtureReportBuilder.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeFixtureReportBuilder.cs)，下一轮接线前后都可先旁路产出 fixture Markdown 报告。
- 已新增 fixture artifact builder [DefinitionClauseDecisionFixtureArtifactBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFixtureArtifactBuilder.cs) 与 serializer [DefinitionClauseDecisionFixtureSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFixtureSerializer.cs)，后续 bridge fixture 可先独立落成 JSON。
- 已新增 fixture Markdown 重建脚本 [Build-DefinitionClauseDecisionFixtureReport.ps1](./tools/Build-DefinitionClauseDecisionFixtureReport.ps1) 与旁路契约 [DEFINITION_CLAUSE_DECISION_FIXTURE_CONTRACT.zh-CN.md](./DEFINITION_CLAUSE_DECISION_FIXTURE_CONTRACT.zh-CN.md)。
- 已新增统一落盘入口 [DefinitionClauseDecisionSidecarWorkflow.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSidecarWorkflow.cs)，后续 `summary / fixture` 两条 sidecar 一律走同一工作流落盘。
- 已新增 workflow 文档 [DEFINITION_CLAUSE_DECISION_SIDECAR_WORKFLOW.zh-CN.md](./DEFINITION_CLAUSE_DECISION_SIDECAR_WORKFLOW.zh-CN.md)，下一轮接线优先走这条统一 sidecar 路径。
- 已新增 source row 模型 [DefinitionClauseDecisionSourceModels.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSourceModels.cs) 与 mapper [DefinitionClauseDecisionMapper.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionMapper.cs)，后续真实结果一旦有 `ClauseVerdict` 字段，就先投到 `SourceRow`，再统一映射成 sidecar artifacts。
- 已新增 source 契约 [DEFINITION_CLAUSE_DECISION_SOURCE_CONTRACT.zh-CN.md](./DEFINITION_CLAUSE_DECISION_SOURCE_CONTRACT.zh-CN.md)，下一轮接线遵守“判定 / 适配 / 输出”三层分工。
- 已新增 demo source builder [DefinitionClauseDecisionDemoSourceBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoSourceBuilder.cs)，可先用默认 fixtures 构造 sidecar 的最小输入。
- 已新增 demo workflow 文档 [DEFINITION_CLAUSE_DECISION_DEMO_WORKFLOW.zh-CN.md](./DEFINITION_CLAUSE_DECISION_DEMO_WORKFLOW.zh-CN.md)，下一轮可先走 `Bridge -> SourceRow -> Mapper -> Sidecar` 最小闭环，再接真实结果链。
- 已新增 demo workflow runner [DefinitionClauseDecisionDemoWorkflowRunner.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoWorkflowRunner.cs) 与 manifest [DefinitionClauseDecisionSidecarManifest.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSidecarManifest.cs)，后续 demo 路径可直接拿到四个 sidecar 文件路径。
- 已新增导出服务 [DefinitionClauseDecisionDemoExportService.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoExportService.cs) 与最短说明 [DEFINITION_CLAUSE_DECISION_DEMO_EXPORT_QUICKSTART.zh-CN.md](./DEFINITION_CLAUSE_DECISION_DEMO_EXPORT_QUICKSTART.zh-CN.md)，下一轮若继续沿 demo 路推进，只需在任一入口挂一个调用点即可。
- 已新增命令处理器 [DefinitionClauseDecisionDemoCommandHandler.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoCommandHandler.cs)、结果对象 [DefinitionClauseDecisionDemoCommandResult.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoCommandResult.cs) 与命令行契约 [DEFINITION_CLAUSE_DECISION_DEMO_COMMANDLINE_CONTRACT.zh-CN.md](./DEFINITION_CLAUSE_DECISION_DEMO_COMMANDLINE_CONTRACT.zh-CN.md)，下一轮如果继续沿 demo 路推进，可以直接在 `Program.cs` 挂一个薄入口。
- 已新增应用入口 helper [DefinitionClauseDecisionDemoAppEntry.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoAppEntry.cs) 与接线说明 [DEFINITION_CLAUSE_DECISION_DEMO_PROGRAM_INTEGRATION.zh-CN.md](./DEFINITION_CLAUSE_DECISION_DEMO_PROGRAM_INTEGRATION.zh-CN.md)，后续主程序只需插一层很薄的 `TryRun(...)` 调用。
- 已新增统一早期命令分发器 [AppEarlyCommandDispatcher.cs](./src/TeklaBodyBracketRecognition.App/AppEarlyCommandDispatcher.cs) 与契约 [APP_EARLY_COMMAND_DISPATCHER_CONTRACT.zh-CN.md](./APP_EARLY_COMMAND_DISPATCHER_CONTRACT.zh-CN.md)，下一轮接主程序时优先只接 dispatcher。
- 已新增主程序接线计划 [PROGRAM_EARLY_COMMAND_HOOK_PLAN.zh-CN.md](./PROGRAM_EARLY_COMMAND_HOOK_PLAN.zh-CN.md)，下一轮如果继续沿 demo 路推进，直接按这份 hook plan 去接 `Program.cs`。
- 已将 dispatcher 薄入口正式接入 [Program.cs](./src/TeklaBodyBracketRecognition.App/Program.cs)；下一轮重点从“接入口”转成“做 smoke 验证并实际导出 demo sidecar 文件”。
- 已把导出目录自描述化接进 demo 导出链：新增 manifest serializer [DefinitionClauseDecisionDemoManifestSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoManifestSerializer.cs) 与目录 README builder [DefinitionClauseDecisionDemoOutputReadmeBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoOutputReadmeBuilder.cs)，并让导出服务/命令结果/应用入口一并回传 `manifest.json + README.md` 路径。
- 下一实现优先顺序：
  - `GKZ` 箱型闭合/对边稳定条款
  - `HXZ` 主板持续性条款
  - `MJ` 多板簇排除条款
  - `GL / YPGL` 保守复核条款
## 2026-04-22 Source Pipeline 补充

- 已新增统一 source pipeline：provider 接口 [IDefinitionClauseDecisionSourceProvider.cs](./src/TeklaBodyBracketRecognition.App/IDefinitionClauseDecisionSourceProvider.cs)、pipeline [DefinitionClauseDecisionSourcePipeline.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSourcePipeline.cs)、demo provider [DefinitionClauseDecisionDemoSourceProvider.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoSourceProvider.cs)。
- demo runner 已切到 source pipeline，后续真实结果链优先补 provider，不复制 demo 路径。
- 已新增说明 [DEFINITION_CLAUSE_DECISION_SOURCE_PIPELINE.zh-CN.md](./DEFINITION_CLAUSE_DECISION_SOURCE_PIPELINE.zh-CN.md)。
- 已新增 snapshot 契约与导出骨架：模型 [DefinitionClauseDecisionSnapshotModels.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotModels.cs)、provider [DefinitionClauseDecisionSnapshotSourceProvider.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotSourceProvider.cs)、导出服务 [DefinitionClauseDecisionSnapshotExportService.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotExportService.cs)。
- 已新增说明 [DEFINITION_CLAUSE_DECISION_SNAPSHOT_CONTRACT.zh-CN.md](./DEFINITION_CLAUSE_DECISION_SNAPSHOT_CONTRACT.zh-CN.md)，后续真实结果链优先走“真实结果 -> snapshot -> source pipeline -> sidecar”。
- 已新增 [DefinitionClauseDecisionDemoSnapshotBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoSnapshotBuilder.cs)，并把 demo runner 正式切到 snapshot export service；现在 demo 闭环已经实际走过 snapshot 这一层。
- 已新增 snapshot extractor/pipeline：接口 [IDefinitionClauseDecisionSnapshotExtractor.cs](./src/TeklaBodyBracketRecognition.App/IDefinitionClauseDecisionSnapshotExtractor.cs)、pipeline [DefinitionClauseDecisionSnapshotPipeline.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotPipeline.cs)、demo extractor [DefinitionClauseDecisionDemoSnapshotExtractor.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoSnapshotExtractor.cs)。
- 已新增说明 [DEFINITION_CLAUSE_DECISION_SNAPSHOT_PIPELINE.zh-CN.md](./DEFINITION_CLAUSE_DECISION_SNAPSHOT_PIPELINE.zh-CN.md)，后续真实结果链接入时优先补 extractor。
- snapshot 现已直接落盘到 demo 导出目录：新增 [DefinitionClauseDecisionSnapshotSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotSerializer.cs)，后续真实 extractor 接入时优先先看 `definition-clause-decision-snapshot.json` 是否正确，再看 summary/fixture sidecar。
- 已新增 snapshot round-trip 自检链：模型 [DefinitionClauseDecisionSnapshotRoundTripModels.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotRoundTripModels.cs)、分析服务 [DefinitionClauseDecisionSnapshotRoundTripService.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotRoundTripService.cs)、报告构建器 [DefinitionClauseDecisionSnapshotRoundTripReportBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotRoundTripReportBuilder.cs)、serializer [DefinitionClauseDecisionSnapshotRoundTripSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotRoundTripSerializer.cs)。
- 后续真实 extractor 接入时，优先检查 `definition-clause-decision-snapshot-roundtrip.json/.md` 是否合理，再看 summary/fixture。
- 已把 snapshot / snapshot-roundtrip 路径继续贯通到 `SnapshotExportResult / DemoExportResult / DemoCommandResult / DemoAppEntry`，后续真实入口一旦接上，终端会直接回显 snapshot 与 roundtrip 文件路径。
- 已新增 snapshot validation 自检链：结果 [DefinitionClauseDecisionSnapshotValidationResult.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotValidationResult.cs)、校验器 [DefinitionClauseDecisionSnapshotValidator.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotValidator.cs)、报告构建器 [DefinitionClauseDecisionSnapshotValidationReportBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotValidationReportBuilder.cs)、serializer [DefinitionClauseDecisionSnapshotValidationSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionSnapshotValidationSerializer.cs)。
- 后续真实 extractor 接入时，优先先看 `definition-clause-decision-snapshot-validation.json/.md` 是否通过，再看 snapshot roundtrip。
- 已把 snapshot validation 路径继续贯通到 `SnapshotExportResult / DemoExportResult / DemoCommandResult / DemoAppEntry`，后续真实入口一旦接上，终端会直接给出 snapshot validation / roundtrip / summary / fixture 的完整路径集合。
- 已新增 demo 输出自校验链：模型 [DefinitionClauseDecisionDemoOutputValidationResult.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoOutputValidationResult.cs)、校验器 [DefinitionClauseDecisionDemoOutputValidator.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoOutputValidator.cs)、报告构建器 [DefinitionClauseDecisionDemoOutputValidationReportBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoOutputValidationReportBuilder.cs)。
- 已新增 validation serializer [DefinitionClauseDecisionDemoOutputValidationSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoOutputValidationSerializer.cs)，demo 导出服务现在会额外落出 `definition-clause-decision-demo-validation.json/.md`，并把路径贯通到 `DemoExportResult / DemoCommandResult / DemoAppEntry`；后续入口一旦接上，导出目录即具备 manifest + README + validation(json+md) 三层自描述/自校验能力。
> 2026-04-22 23:xx 任务推进：已补语义回归契约 [DEFINITION_CLAUSE_DECISION_SEMANTIC_REGRESSION_CASES.zh-CN.md](./DEFINITION_CLAUSE_DECISION_SEMANTIC_REGRESSION_CASES.zh-CN.md)。当前最先执行项：把 `SYNTHETIC_NONE_TOPOLOGY / SYNTHETIC_TARGET_ONLY_REVIEW` 接进 mapper / effect-adapter fixtures，复核 `TopologyTier=None` 与 `TargetOnly` 没有被重新收平。
> 2026-04-22 23:50 任务推进：已落独立回归 catalog [DefinitionClauseDecisionSemanticRegressionCatalog.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionSemanticRegressionCatalog.cs)。当前最先执行项：把 catalog 接到 `DefinitionClauseDecisionBridgeMapperFixtures` 与 `DefinitionClauseDecisionBridgeEffectAdapterFixtures`，再复核 `TopologyTier=None` / `TargetOnly` 的 verdict 与 readiness。
> 2026-04-23 00:03 任务推进：已落回归 snapshot 层 [DefinitionClauseDecisionSemanticRegressionSnapshot.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionSemanticRegressionSnapshot.cs)。当前最先执行项不变：把 semantic regression catalog 接进 mapper/effect-adapter fixtures；如旧 fixture 入口暂时不易修改，则先用 snapshot 接到现有 runner / artifact builder 做并行回归比对。
> 2026-04-23 00:05 任务推进：已补 snapshot Markdown builder [DefinitionClauseDecisionSemanticRegressionSnapshotReportBuilder.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionSemanticRegressionSnapshotReportBuilder.cs)。如果旧 fixture 入口仍暂时不便改动，下一轮先把 snapshot 报告并进现有 artifact / bundle，再回接真正 fixtures。
> 2026-04-23 00:16 任务推进：已落 semantic regression sidecar workflow 与说明文档。当前最先执行项调整为：先把 `DefinitionClauseDecisionSemanticRegressionWorkflow` 的输出并进现有 validation bundle / artifact 导出链，再回接 mapper/effect-adapter fixtures，避免三套入口长期并行。
> 2026-04-23 00:28 任务推进：semantic regression workflow 已补 `manifest.json + README.md`，当前最先执行项进一步收敛为：把这条 bundle-friendly workflow 的输出正式并入现有 validation bundle summary/readme/manifest 链，再回接旧 fixtures。
> 2026-04-23 00:40 任务推进：semantic regression 已补 bundle section / section builder / section markdown builder。当前最先执行项继续收敛为：在既有 validation bundle workflow 中追加 `semantic-regression` section，再统一 summary / README / manifest 输出。
> 2026-04-23 00:52 任务推进：semantic regression workflow 已能直接输出 `bundle-section.json + bundle-section-summary.md`。当前最先执行项进一步收敛为：把这两份现成 section 文件接入既有 validation bundle workflow 的 summary/readme/manifest 汇总，而不是再在主入口临时构造 section。
> 2026-04-23 01:04 任务推进：semantic regression 已补单调用 attachment helper。当前最先执行项进一步收敛为：在既有 validation bundle workflow 中直接调用 `DefinitionClauseDecisionSemanticRegressionBundleAttachmentBuilder.Run(...)`，把返回的 `Section / SummaryMarkdownBlock` 并进 summary、README、manifest 链。
> 2026-04-23 01:16 任务推进：已补 validation bundle 侧专用 contribution helper。当前最先执行项继续收敛为：在既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 中直接调用 `DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionBuilder.Build(...)`，再把返回的 summary/readme/section 并入主 bundle 链。
> 2026-04-23 01:27 任务推进：semantic regression 的 manifest entry / markdown composer 已补齐。当前最先执行项继续收敛为：在既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 中调用 contribution builder + composer，把 summary/readme/manifest 三条链一次性接上。
> 2026-04-23 01:17 任务推进：semantic regression 已补 merge helper。当前最先执行项继续收敛为：在既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 中直接调用 `DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger.Merge(...)`，把 summary/readme/manifest 三条链一次性接上。
> 2026-04-23 01:29 任务推进：semantic regression 已补单次 `Apply(...)` helper。当前最先执行项进一步收敛为：直接修改既有 `DefinitionClauseDecisionBridgeValidationBundleWorkflow`，插入一次 `DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeApplicator.Apply(...)`，完成 summary/readme/manifest 三条链接线。
> 2026-04-23 01:29 任务推进：已落 validation bundle semantic regression post-merge workflow。当前最先执行项调整为：优先把这条 post-merge workflow 接到最小 smoke/demo/sidecar 入口验证路径解析与后合并稳定性，再决定是否直接内建进旧 `DefinitionClauseDecisionBridgeValidationBundleWorkflow` 本体。
> 2026-04-23 01:43 任务推进：已落 validation bundle semantic regression post-merge smoke workflow。当前最先执行项继续收敛为：把这条 smoke workflow 接到最小 smoke/demo/sidecar 入口，优先验证路径解析和 summary/readme/manifest 后合并稳定性，再决定是否直接改旧 validation bundle 主入口。
> 2026-04-23 01:43 任务推进：已补 post-merge smoke workflow 的 command handler / app entry / commandline contract。当前最先执行项继续收敛为：把该 smoke 入口并入 `Program.cs` 或既有 `AppEarlyCommandDispatcher`，先实跑一次 post-merge smoke，再决定是否直接改旧 validation bundle 主入口。
> 2026-04-23 01:57 任务推进：已补 `AppEarlyCommandDispatcher` 可直接消费的 smoke adapter。当前最先执行项继续收敛为：在既有 `AppEarlyCommandDispatcher` 中插入一次 `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter.TryRun(...)`，再进行最小实跑验证。
> 2026-04-23 02:09 任务推进：post-merge smoke 路线已补最小输出校验，命令命中后可直接返回通过/失败。当前最先执行项继续收敛为：把 smoke adapter 接进 `AppEarlyCommandDispatcher` 并进行最小实跑，优先验证路径解析与自校验稳定性。
> 2026-04-23 02:21 任务推进：已冻结 dispatcher hook contract。当前最先执行项继续收敛为：按该契约在 `AppEarlyCommandDispatcher.cs` 中插入一次 smoke adapter `TryRun(...)`，然后进行最小实跑验证。
> 2026-04-23 02:22 任务推进：post-merge smoke 路线已补独立 validation artifacts。当前最先执行项继续收敛为：把 smoke adapter 真正接入 `AppEarlyCommandDispatcher` 并做最小实跑，优先查看 `validation.md` 判断路径解析与工件落盘是否稳定。
> 2026-04-23 02:35 任务推进：已冻结 dispatcher 最小 patch snippet。当前最先执行项继续收敛为：按该 snippet 真正修改 `AppEarlyCommandDispatcher.cs`，然后执行最小 smoke 命令并优先查看 `validation.md`。
> 2026-04-23 02:38 已完成一层 dispatcher-facing bridge facade：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchBridge.Evaluate(...)`。下一步优先事项不变：一旦能安全读取旧入口源码，就在 `AppEarlyCommandDispatcher.cs` 中插入一次 `Evaluate(...)` 调用并执行最小 smoke 实跑，检查 `validation.md` / `manifest.json` / `README.md` / `bundle-section-summary.md` 是否齐全。
> 2026-04-23 02:50 已完成最终 one-line hook 收口：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeAppEarlyCommandDispatcherHook.TryRun(...)`。下一步优先事项继续保持不变：一旦能安全读取旧入口源码，就在 `AppEarlyCommandDispatcher.cs` 中插入一次该 hook 调用并执行最小 smoke 实跑，核对 `validation.json`、`validation.md`、`manifest.json`、`README.md` 与 `bundle-section-summary.md`。
> 2026-04-23 03:07 已完成 post-merge smoke 输出契约冻结：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.cs`。下一步优先顺序为：1）一旦能安全读取旧入口源码，在 `AppEarlyCommandDispatcher.cs` 插入 one-line hook 并做最小 smoke 实跑；2）其余相关 validator / smoke workflow / 文档逐步改为引用这份 output contract，减少硬编码文件名漂移。
> 2026-04-23 03:18 已完成 post-merge smoke 输出路径解析收口：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPathResolver`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）随后将 smoke workflow / validator / dispatcher 最小回查逻辑逐步切到 `OutputContract + OutputPathResolver`，消除路径硬编码漂移。
> 2026-04-23 03:30 已完成 post-merge smoke 共享必需工件检查层：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactInspector`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）随后将 smoke validator / dispatcher 回查逻辑逐步切到 `OutputContract + OutputPathResolver + RequiredArtifactInspector`，消除散落的文件存在判断。
> 2026-04-23 03:42 已完成 post-merge smoke 共享工件检查结果 formatter：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSummaryFormatter`。下一步优先顺序仍保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）随后将 smoke validator / dispatcher 回查逐步切到 `OutputContract + OutputPathResolver + RequiredArtifactInspector + RequiredArtifactSummaryFormatter`，彻底消除散落的文件检查与文案拼接。
> 2026-04-23 03:54 已完成 post-merge smoke 单次 audit 收口：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditBuilder`。下一步优先顺序不变：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）随后将 smoke validator / dispatcher 回查逐步切到 `OutputContract + OutputPathResolver + RequiredArtifactInspector + RequiredArtifactSummaryFormatter + RequiredArtifactAuditBuilder`，把输出完成态检查彻底统一为单次 audit 调用。
> 2026-04-23 04:07 已完成 post-merge smoke audit decision 收口：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecisionBuilder`。下一步优先顺序继续保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）随后将 smoke validator / dispatcher 回查逐步切到共享 audit decision，避免散落的退出码和 message 规则。
> 2026-04-23 04:20 已完成 post-merge smoke 统一 validation 工件层：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactBuilder` 与 `...Serializer`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）随后优先把现有 smoke validator 的 `validation.json / validation.md` 输出切到共享 audit artifact 生成链，减少分散的 validation 输出逻辑。
> 2026-04-23 04:31 已完成 post-merge smoke 统一 validation artifact workflow：`DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflow`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）随后优先把现有 smoke validator 的 validation 输出切到共享 workflow，而不是继续维护分散的 audit / artifact 串接逻辑。
> 2026-04-23 04:43 已完成阶段 2 的一层保守分层契约冻结：`BodyCandidatePartitionLane / BodyCandidatePartitionEvidenceCode / BodyCandidatePartitionDecision`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）并行地，后续可把现有 body-candidate partitioner 或中间结果先映射到这套 lane/evidence/decision 契约，再继续细化分层算法。
> 2026-04-23 04:55 已完成阶段 2 的一层可执行保守映射：`BodyCandidatePartitionSignalSnapshot` + `BodyCandidatePartitionConservativeMapper`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）并行地，可把现有 body-candidate partitioner 或中间结果先适配到 `signal snapshot -> conservative mapper -> decision` 这套阶段 2 契约，再继续替换具体 heuristics。
> 2026-04-23 05:07 已完成阶段 2 默认夹具与最小 runner：`BodyCandidatePartitionConservativeFixtures` + `BodyCandidatePartitionConservativeFixtureRunner`。下一步优先顺序继续保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）并行地，可把现有 body-candidate partitioner 或中间结果先适配到 `signal snapshot -> conservative mapper -> decision`，再用默认夹具守住 core/support/accessory/external/review 的保守分层落点。
> 2026-04-23 05:20 已完成阶段 2 的 Markdown 报告层：`BodyCandidatePartitionConservativeFixtureReportBuilder`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）并行地，可继续把阶段 2 的默认夹具链推进到 artifact/serializer/workflow，或先把现有 body-candidate partitioner / 中间结果适配进 `signal snapshot -> conservative mapper -> fixture runner -> report` 这条闭环。
> 2026-04-23 05:33 已完成阶段 2 默认 artifact/workflow：`BodyCandidatePartitionConservativeFixtureWorkflow.RunDefault(outputRootDirectory)`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）并行地，可将现有 body-candidate partitioner / 中间结果先适配到阶段 2 的 `signal snapshot -> conservative mapper -> fixture runner -> report -> workflow` 闭环，再继续替换具体 heuristics。
> 2026-04-23 05:45 已完成阶段 2 默认 package workflow：`BodyCandidatePartitionConservativeFixturePackageWorkflow.RunDefault(outputRootDirectory)`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）并行地，可将现有 body-candidate partitioner / 中间结果先适配到阶段 2 的 package workflow 闭环，再继续收敛真实分层算法。
> 2026-04-23 05:57 已完成阶段 2 最小 command/export/app-entry 契约：`--body-candidate-partition-conservative-fixture`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，插入 one-line hook 并做最小 smoke 实跑；2）并行地，可在条件允许时把阶段 2 的最小导出命令接入早期命令分发器，或先把真实 partitioner / 中间结果适配到阶段 2 的 package workflow 闭环。
> 2026-04-23 06:09 已完成阶段 2 dispatcher-facing 适配层：`BodyCandidatePartitionConservativeFixtureAppEarlyCommandDispatcherHook.TryRun(...)`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，优先插入 smoke 的 one-line hook 并做最小 smoke 实跑；2）并行地，也可在条件允许时把阶段 2 的默认导出命令接入早期命令分发器，或继续把真实 partitioner / 中间结果适配到阶段 2 的 package workflow 闭环。
> 2026-04-23 06:21 已完成阶段 2 输出契约清理一层：`BodyCandidatePartitionConservativeFixtureOutputContract` + `BodyCandidatePartitionConservativeFixtureOutputPathResolver`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，优先插入 smoke 的 one-line hook 并做最小 smoke 实跑；2）并行地，可逐步让阶段 2 的 command/package/dispatcher 入口切到统一 output contract/path resolver，再把真实 partitioner / 中间结果适配进这条闭环。
> 2026-04-23 06:33 已完成阶段 2 共享 output audit：`BodyCandidatePartitionConservativeFixtureOutputAuditBuilder`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，优先插入 smoke 的 one-line hook 并做最小 smoke 实跑；2）并行地，可逐步让阶段 2 的 command/package/dispatcher 入口切到统一 output audit，并继续把真实 partitioner / 中间结果适配进阶段 2 的 package workflow 闭环。
> 2026-04-23 06:45 已完成阶段 2 package audit workflow：`BodyCandidatePartitionConservativeFixturePackageAuditWorkflow.RunDefault(outputRootDirectory)`。下一步优先顺序保持：1）一旦能安全读取 `AppEarlyCommandDispatcher.cs`，优先插入 smoke 的 one-line hook 并做最小 smoke 实跑；2）并行地，可逐步让阶段 2 的 command/package/dispatcher 入口切到 package audit workflow，或把真实 partitioner / 中间结果适配进这条“导出并自校验”的默认闭环。
> 2026-04-23 05:44 已补齐阶段 2 `body-candidate partition conservative fixture package audit` 的 command/export/app-entry/hook 独立链，下一步优先顺序保持不变：若旧入口源码仍不可读，则继续把阶段 2 package-audit 往 unified validation / audit artifact 方向收口；若入口可读，则直接在 `AppEarlyCommandDispatcher.cs` 插入 `BodyCandidatePartitionConservativeFixturePackageAuditAppEarlyCommandDispatcherHook.TryRun(...)` 做最小实跑验证。
> 2026-04-23 08:41 已把阶段 2 `body-candidate partition conservative fixture package audit` 推进成统一 validation artifact 链；下一步优先顺序保持不变：若旧入口源码仍不可读，则继续把阶段 2 package-audit validation 往 command/result/audit artifact 复用层收口；若入口可读，则优先在 `AppEarlyCommandDispatcher.cs` 插入 `BodyCandidatePartitionConservativeFixturePackageAuditAppEarlyCommandDispatcherHook.TryRun(...)` 做最小实跑验证。
> 2026-04-23 09:03 旧入口源码现已可读，且已完成阶段 2 `BodyCandidatePartitionConservativeFixturePackageAuditAppEarlyCommandDispatcherHook.TryRun(...)` 的 first integration；下一步优先执行最小 `dotnet build` 与 `--body-candidate-partition-conservative-fixture-package-audit` smoke，若编译/运行暴露接口不匹配，再回头收敛 command/result/validation 链，不再继续只加外围壳。
> 2026-04-23 09:11 最小编译已从“只加外围壳”推进到真实阻塞清理：当前已修正 `DefinitionClauseDecisionBridgeResult` 的重复定义冲突，保留 `DefinitionClauseDecisionBridge.cs` 的 verdict-code 结果类型不动，并把 mapper 纯函数结果改为 `DefinitionClauseDecisionBridgeMapperResult`；下一步立即复跑 `dotnet build`，不再回避真实编译面。
> 2026-04-23 09:16 已确认 `DefinitionClauseDecisionBridgeMapperResult` 的 rename 只剩 adapter/fixture 连锁收尾，当前下一步仍是立即复跑 `dotnet build` 直到编译栈真正落到阶段 2 package-audit 入口或新的真实阻塞点，不再回退到纯外围建设。
> 2026-04-23 09:21 当前编译已不再被 Core 重名冲突拦住，而是进入 App 层历史语法清理；`DefinitionClauseDecisionBridgeValidationBundleSummaryBuilder.cs` 的未终止字符串已修复，下一步继续立即复跑 `dotnet build`，沿真实错误栈逐个清理，直到项目可编译或暴露新的入口级阻塞。
> 2026-04-23 09:25 semantic-regression 的跨层 API 边界已收口：`SnapshotRow / Snapshot / SnapshotReportBuilder` 已改为 public，目的是消除 App artifact/workflow 对 Core internal 类型的直接阻塞；下一步继续立即复跑 `dotnet build`，沿真实编译面推进，不再回退到只写文档/壳层。
> 2026-04-23 09:34 已把 `DefinitionClauseDecisionFullRunSourceCollector` 对 `CoreBodyProofViewOutput` 的旧字段依赖改为基于 `Result.Parts` 现算，同时补齐 semantic-regression serializer 的 required 字段；下一步仍是立即复跑 `dotnet build`，沿真实编译栈继续推进到阶段 2 package-audit 入口或新的最小阻塞点。
> 2026-04-23 09:38 当前编译错误已收缩到单个命名空间引用缺失，`DefinitionClauseDecisionFullRunSourceCollector.cs` 已补 `using TeklaBodyBracketRecognition.Core.Algorithms;`；下一步继续立即复跑 `dotnet build`，避免在已经接近通过时中断真实编译链。
> 2026-04-23 09:42 `DefinitionClauseDecisionFullRunSourceCollector` 的剩余单点错误已定位为错误命名空间引用，当前已切到 `TeklaBodyBracketRecognition.Core.Domain`；下一步继续立即复跑 `dotnet build`，避免在已逼近通过时停下。
> 2026-04-23 09:46 full-run collector 与新 proof 模型的对齐继续推进：旧便捷字段 `HasTopologyRewrite / IsCorePart / IsReviewPart` 已改为显式推导；下一步继续立即复跑 `dotnet build`，不让编译链停在这最后几层 App 侧历史适配噪声上。
> 2026-04-23 09:50 full-run collector 对 proof 模型的显性断点已继续缩到极小：`ReviewPromptZh` 现改由 `Reasons` 拼接生成；下一步继续立即复跑 `dotnet build`，争取把编译栈彻底推过 full-run collector 这一层。
> 2026-04-23 09:58 阶段 2 的 `--body-candidate-partition-conservative-fixture-package-audit` 已完成真实入口接线与最小 smoke 验证：应用已可通过 `AppEarlyCommandDispatcher` 直接触发该命令且输出成功；下一步优先把现有 package-audit validation workflow 继续上提成独立 command/hook，或在现有 command 结果里补出 `validation.json / validation.md` 路径，避免 validation 工件仍停留在旁路层。
> 2026-04-23 10:09 阶段 2 `--body-candidate-partition-conservative-fixture-package-audit` 已不再只是 package-audit 旁路，而是同一次命令链直接产出 `body-candidate-partition-conservative-fixture.json/.md + manifest.json + README.md + validation.json + validation.md` 六件套并通过真实 smoke；下一步优先把这条成功链进一步抽成更统一的 validation/audit command 或 bundle contribution，而不是再回到只补旁路 workflow。
> 2026-04-23 10:22 阶段 2 `package-audit validation` 已不再只是 workflow 旁路，而是拥有独立 `--body-candidate-partition-conservative-fixture-package-audit-validation` 命令和 dispatcher hook，并已通过真实 smoke；下一步优先把这条 validation 入口继续抽成更统一的 validation/audit contribution 或汇总 command，而不是让 stage-2 长期保留多条彼此平行但不汇总的命令链。
> 2026-04-23 10:31 阶段 2 validation 主线已继续收口：在真实跑通 `--body-candidate-partition-conservative-fixture-package-audit-validation` 之后，又补出最小 `BundleSection / Attachment / Contribution` 层；下一步优先考虑把这条 contribution 真正并进统一 summary/bundle 汇总入口，或继续为 stage-2 再补一个更高层的 aggregate command，而不是让 contribution 只停留在可调用未并线状态。
> 2026-04-23 10:42 阶段 2 现已拥有三级稳定入口：`package-audit` 命令、独立 `validation` 命令、以及新的 `validation-bundle` 命令；下一步优先把这条 bundle 入口再向更高层统一 summary/aggregate command 推进，或开始让多个 stage-2 入口共享同一 manifest/readme/smoke 汇总骨架，避免命令数量继续增加却没有再汇流。
> 2026-04-23 11:08 阶段 2 `--body-candidate-partition-conservative-fixture-stage2-aggregate` 已完成真实入口接线与最小 smoke：当前 `AppEarlyCommandDispatcher.cs` 已接入 `BodyCandidatePartitionConservativeFixtureStage2AggregateAppEarlyCommandDispatcherHook.TryRun(...)`，且 aggregate workflow 已按“先跑一次 `package-audit` 主链，再复用 command result 生成 `validation-bundle` 与 aggregate summary”收口；最小实跑 `dotnet run --project .\src\TeklaBodyBracketRecognition.App\TeklaBodyBracketRecognition.App.csproj -- --body-candidate-partition-conservative-fixture-stage2-aggregate --output-root .\.tmpresults\body-candidate-partition-conservative-fixture-stage2-aggregate-smoke-20260423` 已真实通过，并在 `I:\autoteklasuanfa\.tmpresults\body-candidate-partition-conservative-fixture-stage2-aggregate-smoke-20260423` 下同时落出 aggregate / package / bundle 三层结果。下一步优先把 `package-audit`、`validation`、`validation-bundle`、`stage2-aggregate` 这四条阶段 2 入口继续收敛到共享 manifest/readme/summary 输出骨架，避免稳定入口增加后重复维护同类目录拼装逻辑。
> 2026-04-23 11:24 阶段 2 的共享输出骨架已开始落地：新增 `BodyCandidatePartitionConservativeFixtureStage2OutputArtifactWriter`，并把 `validation-bundle` 与 `stage2-aggregate` 两条链的 `summary.md / README.md / manifest.json` 落盘逻辑切到统一 helper；最小 `dotnet build`、`--body-candidate-partition-conservative-fixture-package-audit-validation-bundle` 与 `--body-candidate-partition-conservative-fixture-stage2-aggregate` smoke 均继续通过。下一步优先把这套共享骨架继续往更高复用层推进：优先识别 `package-audit` / `validation` / `validation-bundle` / `stage2-aggregate` 四条链中仍重复的输出目录命名、结果摘要字段与 manifest 顶层元数据，再决定是抽共享 stage-2 output contract，还是先收敛共享 summary/readme builder 输入模型。
> 2026-04-23 11:36 阶段 2 已继续把共享骨架从“artifact writer”推进到“顶层 workflow 壳”：新增 `BodyCandidatePartitionConservativeFixtureStage2TopLevelOutputWorkflow` 与 `...TopLevelOutputWriteResult`，并让 `validation-bundle` / `stage2-aggregate` 两条链共用“写 summary -> 构造 manifest 顶层字段 -> 写 README -> 写 manifest”这一整段流程；最小 `dotnet build`、`--body-candidate-partition-conservative-fixture-package-audit-validation-bundle` 与 `--body-candidate-partition-conservative-fixture-stage2-aggregate` smoke 均继续通过。下一步优先把还未并入这层共享壳的 `package-audit` / `validation` 两条入口也纳入统一结果模型视角，重点识别是否先抽共享 top-level output result/metadata contract，再决定要不要继续下沉到共享 summary/readme builder 输入。
> 2026-04-23 11:48 阶段 2 已继续把共享范围从 workflow 壳推进到顶层对象 contract：新增 `BodyCandidatePartitionConservativeFixtureStage2TopLevelManifest` 与 `...TopLevelWorkflowResult`，并让 `validation-bundle` / `stage2-aggregate` 两条链共用顶层 manifest 元数据与 workflow result 路径字段基类；最小 `dotnet build`、`--body-candidate-partition-conservative-fixture-package-audit-validation-bundle` 与 `--body-candidate-partition-conservative-fixture-stage2-aggregate` smoke 均继续通过。下一步优先检查 `package-audit` / `validation` / `validation-bundle` / `stage2-aggregate` 四条链的 command result 是否也适合收敛到共享 top-level output result contract，还是先只统一 manifest / workflow result 而保留 command result 各自独立。
> 2026-04-23 12:06 阶段 2 已继续把共享范围推进到顶层 command result contract：新增 `BodyCandidatePartitionConservativeFixtureStage2TopLevelCommandResult`，并让 `validation-bundle` / `stage2-aggregate` 两条链共用 `ExitCode / OutputDirectory / SummaryMarkdownPath / ReadmePath / ManifestPath` 这一组顶层 command-result 字段基类；最小 `dotnet build`、`--body-candidate-partition-conservative-fixture-package-audit-validation-bundle` 与 `--body-candidate-partition-conservative-fixture-stage2-aggregate` smoke 均继续通过。下一步优先判断 `package-audit` / `validation` 是否值得继续并入同一 top-level command result contract，还是保持目前“bundle / aggregate 共享、package / validation 独立”的保守边界，以免为减少样板而扭曲不同产物链的语义。
> 2026-04-23 12:22 阶段 2 已继续把共享范围推进到 command handler 解析层：新增 `BodyCandidatePartitionConservativeFixtureCommandHandlerSupport`，并让 `package-audit`、`validation`、`validation-bundle`、`stage2-aggregate` 四条入口共用命令命中判断与 `--output-root` 解析逻辑；最小 `dotnet build` 与四条 smoke 均继续通过。下一步优先评估是否还值得继续把四个 dispatcher hook 的 console 输出层也收成共享 formatter，还是停在当前边界，把更多精力留给真正还未共享的业务语义层。
> 2026-04-23 12:33 阶段 2 已继续把共享范围推进到 dispatcher console 输出层：新增 `BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter`，并让 `package-audit`、`validation`、`validation-bundle`、`stage2-aggregate` 四个 dispatcher hook 共用 header、键值行与可选字段输出逻辑；最小 `dotnet build` 与四条 smoke 均继续通过。当前这组入口的壳层重复已经基本收干净，下一步更建议停在这里，把后续时间转回业务语义层，除非你明确希望继续压缩 hook/handler 的结构样板。
> 2026-04-23 12:49 `DefinitionClause` 主线已真正推进到“validation bundle 并回旧 demo/sidecar 导出链”：当前 `DefinitionClauseDecisionSidecarWorkflow` 已新增 `WriteDefaultFixtureArtifactsWithValidationBundle(...)`，`DefinitionClauseDecisionDemoWorkflowRunner` 会在旧 sidecar 基线之上同步导出 `definition-clause-decision-bridge-validation-bundle/`；`DefinitionClauseDecisionSidecarManifest`、`DefinitionClauseDecisionDemoExportService`、`DefinitionClauseDecisionDemoCommandHandler`、`DefinitionClauseDecisionDemoAppEntry`、`DefinitionClauseDecisionDemoOutputReadmeBuilder`、`DefinitionClauseDecisionDemoOutputValidator` 与 validation report 也已补入 bundle 路径和存在性检查。真实 smoke `definition-clause-decision-demo-smoke-20260423-v2` 下，旧 fixture sidecar 与新增 bundle 顶层/mapper/effect-adapter 工件已全部通过验证，`AllExpectedFilesExist=True`。下一步优先把同样的并线思路继续推进到“旧 bridge fixtures / runner / reportBuilder 是否要直接消费新 mapper/effect-adapter 结果”，而不是再让 `DefinitionClauseDecisionBridge.cs` 这条旧直判骨架长期停留在完全独立的测试入口。

## 2026-04-27 纯粗拓扑观察层

- 已完成第一轮独立重建，当前边界固定为：
  - 属性直达层保持独立
  - 粗分类层独立重建，只做观察
  - 最终 `body-family-proof` 不消费这层结果
- 当前粗分类规则主线：
  - 小板粗排除
  - 主板候选集合
  - 多切片拓扑聚合
  - 输出 `BOX / H / PRIMARY_PLATE_BODY / NONE`
- 当前已完成：
  - 新 coarse observation workflow 接入 sidecar coordinator
  - Excel 导出新增 `coarse-main-class-observation.xlsx`
  - `run_body_bracket_real_06_coarse_topology_rebuild_v2` 已复跑
  - `run_body_bracket_real_04_coarse_topology_rebuild_v2` 已复跑
  - `T3-2YPGL-18` 在 `real_04` 中已稳定回到 `BOX`
- 当前下一步优先级：
  - 先围绕 `T3-2YPGL-5` 继续拆清当前漏报层级：
    - 是 `priority station` 选取过窄
    - 还是折线局部截面在当前截取方式下确实没有形成可证明围合
    - 还是需要单独的“斜围合 / 变截面 box”粗拓扑证据
  - 再扫 `real_06` 中剩余 `SourceMemberMainClass = BOX` 但粗分类未落 `BOX` 的样本
  - 重点区分：
    - `NO_ELIGIBLE_CANDIDATE_SET`
    - `TOPOLOGY_CONSENSUS_NOT_REACHED`
  - 只有在确认还能明显提升普适性时，再收第三轮候选集/切片聚合规则
# 2026-04-29 当前任务补充

- 已完成：
  - 新增旧派生字段运行时依赖排查表：
    [OLD_DERIVED_RUNTIME_DEPENDENCY_AUDIT.zh-CN.md](</I:/autoteklasuanfa/OLD_DERIVED_RUNTIME_DEPENDENCY_AUDIT.zh-CN.md>)
  - 已完成第一轮字段分级：
    - 高优先级继续收口：
      - `ImportSynthesisKind`
      - `EndProximity / NearMemberStart / NearMemberEnd` 的残余运行时读取
    - 中优先级评估是否彻底 observation 化：
      - `SourceMemberMainClassCode`
      - `BodyDescriptorFamily / BodyDescriptorSectionType` 在 coarse direct signal / bypass 中的角色
    - 暂不去依赖，但明确属于外部先验：
      - `SemanticRole / SemanticRoleScore`
- 当前下一步：
  - 先专攻 `ImportSynthesisKind`
  - 逐点梳理：
    - 哪些是必要桥接
    - 哪些应改成 `LeadClause / BodyDescriptor` 驱动
    - 哪些应纯降级成审计字段

- 已完成：
  - 删除粗分类观察层旧 H 借力分支 `DIRECT_H_WITH_NARROW_CANDIDATE_SET`
  - 复跑 [run_body_bracket_real_13_no_h_fallback_v1](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_13_no_h_fallback_v1)
    - `FallbackCount = 0`
    - `T2-12MJ-1 / T2-12MJ-4 / T2-13GL-20 / T2-13MJ-2 / T2-13MJ-4`
      全部回落为 `PRIMARY_PLATE_BODY`
  - 复跑 [run_body_bracket_real_14_no_h_fallback_v1](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_14_no_h_fallback_v1)
    - `FallbackCount = 0`
    - `T2-13GL-9 / 10 / 16 / 21 / 23 / 24`
      仍稳定为 `H + WEB_FLANGE_SECTION_CONSENSUS`
- 当前明确冻结：
  - 粗分类观察层禁止再借 `SourceMemberMainClassCode = H` 直接抬主类
  - `H` 只能来自真实 `web + flange` 多切片拓扑共识
- 当前下一步：
  - 继续围绕粗分类主线，专攻“为什么 `T2-13GL-20` 这类样本当前只剩双主板候选”
  - 但修复口径必须是上游/候选集层面的普适性修复，不能恢复任何旧借力

- 已完成：
  - `T2-13GL-20` 一类“长腹板被收缩成附件候选”的根因已拆清并修复
  - 已确认根因是：
    - `BodyCandidatePartitioner` 用当前 provisional axis 重算 `coverage / projectedInterval`
    - 却继续直接使用缓存导入的旧 `EndProximity`
    - 导致同一层分区逻辑内部出现纵向位置信号打架
  - 已把 `nearStableZone` 改为基于当前 `projectedInterval + assemblySpan` 同源重算
  - 已复跑 [run_body_bracket_real_13_gl20_interval_consistency_v1](/I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_13_gl20_interval_consistency_v1)
    - `T2-13GL-20` 已回到：
      - `CandidatePartCount = 3`
      - `CoarseMainClassCode = H`
      - `ReasonCode = WEB_FLANGE_SECTION_CONSENSUS`
    - `T2-12MJ-1 / T2-12MJ-4 / T2-13MJ-2 / T2-13MJ-4`
      仍保持 `PRIMARY_PLATE_BODY`
- 当前下一步：
  - 继续横向排查 `GL / YPGL / HXZ` 中是否还有同类“旧 EndProximity 与当前重算 interval 打架”的样本
  - 若有，再统一收敛到“候选集/稳定区只认当前 provisional axis 重算口径”
