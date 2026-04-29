namespace TeklaBodyBracketRecognition.Core.Domain;

public enum CoreBodyProofClass
{
    Unknown,
    CoreBodyPart,
    BodyAccessoryPart,
    ReviewRequired
}

public sealed record CoreBodyProofPartResult
{
    public required int PartId { get; init; }
    public required string PartName { get; init; }
    public required string ProfileString { get; init; }
    public required bool IsInputMainPart { get; init; }
    public required BodyCandidatePartitionClass PartitionClass { get; init; }
    public required int PriorityStationPresenceCount { get; init; }
    public required double PriorityStationPresenceRatio { get; init; }
    public required int EnvelopeSupportStationCount { get; init; }
    public required double EnvelopeSupportStationRatio { get; init; }
    public required int PriorityStationsWithBodyCandidateAfterRemoval { get; init; }
    public required double PriorityBodyCoverageAfterRemovalRatio { get; init; }
    public required int PriorityStationsWithEnvelopeBodyCandidateAfterRemoval { get; init; }
    public required double EnvelopeBodyCoverageAfterRemovalRatio { get; init; }
    public required int ClosedLoopStationCountBeforeRemoval { get; init; }
    public required int ClosedLoopStationCountAfterRemoval { get; init; }
    public required double AverageBodyWidthBeforeRemoval { get; init; }
    public required double AverageBodyWidthAfterRemoval { get; init; }
    public required double BodyWidthRetentionRatio { get; init; }
    public required double RemovalImpactScore { get; init; }
    public required IReadOnlyList<string> MissingPriorityStationIds { get; init; }
    public required IReadOnlyList<string> LostBodyCoverageStationIds { get; init; }
    public required IReadOnlyList<string> LostEnvelopeSupportStationIds { get; init; }
    public required IReadOnlyList<string> LostClosedLoopStationIds { get; init; }
    public required IReadOnlyList<string> TopologyRewriteStationIds { get; init; }
    public required IReadOnlyList<string> TopologyRewriteSpanYStationIds { get; init; }
    public required IReadOnlyList<string> TopologyRewriteSpanZStationIds { get; init; }
    public required IReadOnlyList<string> TopologyRewriteEnvelopeControllerSwitchStationIds { get; init; }
    public required IReadOnlyList<string> TopologyRewriteDirectControllerStationIds { get; init; }
    public required IReadOnlyList<string> TopologyRewriteIndirectControllerStationIds { get; init; }
    public required string TopologyRewriteControllerRoleCode { get; init; }
    public required string TopologyRewriteControllerRoleLabelZh { get; init; }
    public required string TopologyRewritePatternCode { get; init; }
    public required string TopologyRewritePatternLabelZh { get; init; }
    public required IReadOnlyList<int> TopologyRewriteCohortPartIds { get; init; }
    public required int TopologyRewriteCohortPartCount { get; init; }
    public required string TopologyRewriteShapeRoleCode { get; init; }
    public required string TopologyRewriteShapeRoleLabelZh { get; init; }
    public required string TopologyRewriteFamilyRiskCode { get; init; }
    public required string TopologyRewriteFamilyRiskLabelZh { get; init; }
    public required string TopologyRewriteProofTypeCode { get; init; }
    public required string TopologyRewriteProofTypeLabelZh { get; init; }
    public required string TopologyRewriteFamilyProofTargetCode { get; init; }
    public required string TopologyRewriteFamilyProofTargetLabelZh { get; init; }
    public required string TopologyRewriteDefinitionClauseCode { get; init; }
    public required string TopologyRewriteDefinitionClauseLabelZh { get; init; }
    public required string TopologyRewriteDefinitionClauseEffectCode { get; init; }
    public required string TopologyRewriteDefinitionClauseEffectLabelZh { get; init; }
    public required IReadOnlyList<string> DominantDimensionSwitchStationIds { get; init; }
    public required CoreBodyProofClass ProofClass { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
}

public sealed record CoreBodyProofResult
{
    public required string AssemblyId { get; init; }
    public required int InputMainPartId { get; init; }
    public required int PriorityStationCount { get; init; }
    public required bool BootstrapOnly { get; init; }
    public required IReadOnlyList<int> CoreBodyPartIds { get; init; }
    public required IReadOnlyList<int> BodyAccessoryPartIds { get; init; }
    public required IReadOnlyList<int> ReviewPartIds { get; init; }
    public required IReadOnlyList<CoreBodyProofPartResult> Parts { get; init; }
}
