using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

internal sealed class DefinitionClauseDecisionSemanticRegressionCase
{
    public required string Name { get; init; }

    public required string Intent { get; init; }

    public required DefinitionClauseDecisionBridgeSourceTier SourceTier { get; init; }

    public required DefinitionClauseDecisionBridgeStationTier StationTier { get; init; }

    public required DefinitionClauseDecisionBridgeTopologyTier TopologyTier { get; init; }

    public required DefinitionClauseDecisionBridgeProofCompletenessTier ProofCompletenessTier { get; init; }

    public required DefinitionClauseDecisionBridgeLeadClauseTier LeadClauseTier { get; init; }

    public required DefinitionClauseDecisionBridgeCandidateDirection ExpectedCandidateDirection { get; init; }

    public required DefinitionClauseDecisionBridgeVerdict ExpectedVerdict { get; init; }

    public required DefinitionClauseDecisionBridgePromotionReadiness ExpectedPromotionReadiness { get; init; }
}

internal static class DefinitionClauseDecisionSemanticRegressionCatalog
{
    public static IReadOnlyList<DefinitionClauseDecisionSemanticRegressionCase> CreateDefault()
    {
        return new[]
        {
            new DefinitionClauseDecisionSemanticRegressionCase
            {
                Name = "SYNTHETIC_NONE_TOPOLOGY",
                Intent = "锁定 TopologyTier=None 不会被默认抬成 WeakChange，也不会越级形成 definition verdict。",
                SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.None,
                ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Complete,
                LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.Unknown,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.InsufficientEvidence,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly
            },
            new DefinitionClauseDecisionSemanticRegressionCase
            {
                Name = "SYNTHETIC_TARGET_ONLY_REVIEW",
                Intent = "锁定 TargetOnly 与 TypeOnly 分层，不允许在对象类型未闭合时直接 promotion。",
                SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.TargetOnly,
                LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.ReviewRequired,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview
            }
        };
    }
}
