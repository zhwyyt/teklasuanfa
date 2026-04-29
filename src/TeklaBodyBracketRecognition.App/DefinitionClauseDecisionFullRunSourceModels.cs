namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionFullRunAssemblySource
{
    public string SourceFile { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public string AssemblyId { get; init; } = string.Empty;

    public int InputMainPartId { get; init; }

    public string ViewKind { get; init; } = string.Empty;

    public string BodyDescriptorFamily { get; init; } = string.Empty;

    public string BodyDescriptorSectionType { get; init; } = string.Empty;

    public string ImportSynthesisKind { get; init; } = string.Empty;

    public string LongitudinalTypeCode { get; init; } = "STRAIGHT";

    public string LongitudinalTypeLabelZh { get; init; } = "直线主线";

    public string LongitudinalSubtypeCode { get; init; } = "GENERAL_STRAIGHT";

    public string LongitudinalSubtypeLabelZh { get; init; } = "一般直线主线";

    public string SourceMemberMainClassCode { get; init; } = string.Empty;

    public string SourceSemanticBodyFamily { get; init; } = string.Empty;

    public string SourceSemanticSectionType { get; init; } = string.Empty;

    public bool SourceSemanticPriorityApplied { get; init; }

    public bool HasTopologyRewrite { get; init; }

    public string LeadClauseCode { get; init; } = string.Empty;

    public string LeadClauseLabelZh { get; init; } = string.Empty;

    public double LeadClauseShare { get; init; }

    public string ClauseMix { get; init; } = string.Empty;

    public string LeadClauseEffects { get; init; } = "NONE";

    public string LeadClauseEffectCode { get; init; } = "NONE";

    public string LeadClauseEffectLabelZh { get; init; } = "无条款效果";

    public double LeadClauseEffectShare { get; init; }

    public string LeadClauseEffectMixStatus { get; init; } = "无条款效果";

    public string LeadClauseEffectDirectionCode { get; init; } = "Unknown";

    public string LeadClauseEffectDirectionLabelZh { get; init; } = "候选方向未知";

    public int LeadClauseBreakEffectCount { get; init; }

    public int LeadClauseRewriteEffectCount { get; init; }

    public string ClauseVerdicts { get; init; } = "NONE";

    public string LeadClauseVerdictCode { get; init; } = "InsufficientEvidence";

    public string LeadClauseVerdictLabelZh { get; init; } = "证据不足";

    public double LeadClauseVerdictShare { get; init; }

    public string ClauseVerdictMixStatus { get; init; } = "无条款判定";

    public string ClausePromotionReadinesses { get; init; } = "NONE";

    public string LeadClausePromotionReadinessCode { get; init; } = "HoldEffectOnly";

    public string LeadClausePromotionReadinessLabelZh { get; init; } = "仅保留 effect 提示";

    public double LeadClausePromotionReadinessShare { get; init; }

    public string ClausePromotionReadinessMixStatus { get; init; } = "无提升准备度";
}

internal sealed class DefinitionClauseDecisionFullRunRepresentativePartSource
{
    public string SourceFile { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public string AssemblyId { get; init; } = string.Empty;

    public int InputMainPartId { get; init; }

    public string ViewKind { get; init; } = string.Empty;

    public string SourceSemanticBodyFamily { get; init; } = string.Empty;

    public string SourceSemanticSectionType { get; init; } = string.Empty;

    public bool SourceSemanticPriorityApplied { get; init; }

    public int RepresentativePartId { get; init; }

    public string RepresentativePartName { get; init; } = string.Empty;

    public string RepresentativePartProfile { get; init; } = string.Empty;

    public bool IsInputMainPart { get; init; }

    public bool IsCurrentCorePart { get; init; }

    public bool IsCurrentReviewPart { get; init; }

    public string TopologyRewritePatternCode { get; init; } = string.Empty;

    public string TopologyRewritePatternLabelZh { get; init; } = string.Empty;

    public string TopologyRewriteControllerRoleCode { get; init; } = string.Empty;

    public string TopologyRewriteControllerRoleLabelZh { get; init; } = string.Empty;

    public string TopologyRewriteShapeRoleCode { get; init; } = string.Empty;

    public string TopologyRewriteShapeRoleLabelZh { get; init; } = string.Empty;

    public string TopologyRewriteFamilyRiskCode { get; init; } = string.Empty;

    public string TopologyRewriteFamilyRiskLabelZh { get; init; } = string.Empty;

    public string TopologyRewriteProofTypeCode { get; init; } = string.Empty;

    public string TopologyRewriteProofTypeLabelZh { get; init; } = string.Empty;

    public string TopologyRewriteFamilyProofTargetCode { get; init; } = string.Empty;

    public string TopologyRewriteFamilyProofTargetLabelZh { get; init; } = string.Empty;

    public string TopologyRewriteDefinitionClauseCode { get; init; } = string.Empty;

    public string TopologyRewriteDefinitionClauseLabelZh { get; init; } = string.Empty;

    public string TopologyRewriteDefinitionClauseEffectCode { get; init; } = string.Empty;

    public string TopologyRewriteDefinitionClauseEffectLabelZh { get; init; } = string.Empty;

    public string LeadClauseCode { get; init; } = string.Empty;

    public string LeadClauseLabelZh { get; init; } = string.Empty;

    public double LeadClauseShare { get; init; }

    public string ClauseMix { get; init; } = string.Empty;

    public string ReviewPromptZh { get; init; } = string.Empty;

    public string SourceTierCode { get; init; } = "RecognitionOnly";

    public string SourceTierLabelZh { get; init; } = "仅 recognition_input 证据";

    public string StationTierCode { get; init; } = "Episodic";

    public string StationTierLabelZh { get; init; } = "仅偶发站位命中";

    public string TopologyTierCode { get; init; } = "None";

    public string TopologyTierLabelZh { get; init; } = "无拓扑信号";

    public string ProofCompletenessTierCode { get; init; } = "Unresolved";

    public string ProofCompletenessTierLabelZh { get; init; } = "证明对象未收敛";

    public string LeadClauseTierCode { get; init; } = "Unset";

    public string LeadClauseTierLabelZh { get; init; } = "无主导条款";

    public string CandidateDirectionCode { get; init; } = "Unknown";

    public string CandidateDirectionLabelZh { get; init; } = "候选方向未知";

    public string ClauseVerdictCode { get; init; } = "InsufficientEvidence";

    public string ClauseVerdictLabelZh { get; init; } = "证据不足";

    public string ClausePromotionReadinessCode { get; init; } = "HoldEffectOnly";

    public string ClausePromotionReadinessLabelZh { get; init; } = "仅保留 effect 提示";

    public IReadOnlyList<string> ConflictReasons { get; init; } = Array.Empty<string>();
}
