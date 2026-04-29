namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionFullRunAssemblySnapshotSeed
{
    public string SourceFile { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public string AssemblyId { get; init; } = string.Empty;

    public int InputMainPartId { get; init; }

    public string ViewKind { get; init; } = string.Empty;

    public string BodyDescriptorFamily { get; init; } = string.Empty;

    public string BodyDescriptorSectionType { get; init; } = string.Empty;

    public bool HasTopologyRewrite { get; init; }

    public string LeadClauseCode { get; init; } = string.Empty;

    public string LeadClauseLabelZh { get; init; } = string.Empty;

    public double LeadClauseShare { get; init; }

    public string ClauseMix { get; init; } = string.Empty;
}

internal sealed class DefinitionClauseDecisionFullRunRepresentativePartSnapshotSeed
{
    public string SourceFile { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public string AssemblyId { get; init; } = string.Empty;

    public int InputMainPartId { get; init; }

    public string ViewKind { get; init; } = string.Empty;

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

    public string LeadClauseCode { get; init; } = string.Empty;

    public string LeadClauseLabelZh { get; init; } = string.Empty;

    public double LeadClauseShare { get; init; }

    public string ClauseMix { get; init; } = string.Empty;

    public string ReviewPromptZh { get; init; } = string.Empty;
}
