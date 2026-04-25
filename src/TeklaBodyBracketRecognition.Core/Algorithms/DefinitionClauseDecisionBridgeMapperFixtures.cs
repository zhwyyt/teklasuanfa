using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionBridgeMapperFixture
{
    public string SampleCode { get; set; } = string.Empty;

    public string LabelZh { get; set; } = string.Empty;

    public bool IsSynthetic { get; set; }

    public DefinitionClauseDecisionBridgeContext Context { get; set; } = new();

    public DefinitionClauseDecisionBridgeCandidateDirection ExpectedCandidateDirection { get; set; }

    public DefinitionClauseDecisionBridgeVerdict ExpectedVerdict { get; set; }

    public DefinitionClauseDecisionBridgePromotionReadiness ExpectedPromotionReadiness { get; set; }
}

public static class DefinitionClauseDecisionBridgeMapperFixtures
{
    public static IReadOnlyList<DefinitionClauseDecisionBridgeMapperFixture> CreateDefault()
    {
        return
        [
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "GKZ",
                LabelZh = "GKZ 对壁闭合正例",
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Complete,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                    HasConflict = false,
                    HasReviewConflict = false,
                    RemovalBreaksClause = false,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Satisfied,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForPromotion,
            },
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "HXZ",
                LabelZh = "HXZ 单主板持续性正例",
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Complete,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                    HasConflict = false,
                    HasReviewConflict = false,
                    RemovalBreaksClause = false,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Satisfied,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForPromotion,
            },
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "GL",
                LabelZh = "GL 混合改写保守样本",
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.TypeOnly,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.Dominant,
                    HasConflict = true,
                    HasReviewConflict = false,
                    RemovalBreaksClause = false,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Mixed,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly,
            },
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "MJ",
                LabelZh = "MJ 条款稳定但仍需复核",
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.TypeOnly,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                    HasConflict = false,
                    HasReviewConflict = true,
                    RemovalBreaksClause = false,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.ReviewRequired,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview,
            },
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "YPGL",
                LabelZh = "YPGL 混合样本",
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMinor,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Unresolved,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.Dominant,
                    HasConflict = true,
                    HasReviewConflict = false,
                    RemovalBreaksClause = false,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Mixed,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly,
            },
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "SYNTHETIC_BROKEN_CLOSED_LOOP",
                LabelZh = "合成闭环破坏探针",
                IsSynthetic = true,
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Complete,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                    HasConflict = false,
                    HasReviewConflict = false,
                    RemovalBreaksClause = true,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Broken,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForPromotion,
            },
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "SYNTHETIC_BROKEN_NEEDS_REVIEW",
                LabelZh = "合成结构破坏但条款锚点不足样本",
                IsSynthetic = true,
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Unresolved,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.Unset,
                    HasConflict = false,
                    HasReviewConflict = false,
                    RemovalBreaksClause = true,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.ReviewRequired,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview,
            },
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "SYNTHETIC_BROKEN_BORROWABLE_ANCHOR",
                LabelZh = "合成结构破坏且可借 assembly 主条款锚点样本",
                IsSynthetic = true,
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Unresolved,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                    HasConflict = true,
                    HasReviewConflict = false,
                    RemovalBreaksClause = true,
                    CanBorrowLeadClauseAnchor = true,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Broken,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview,
            },
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "SYNTHETIC_NONE_TOPOLOGY",
                LabelZh = "合成无拓扑信号兜底样本",
                IsSynthetic = true,
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.None,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Complete,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                    HasConflict = false,
                    HasReviewConflict = false,
                    RemovalBreaksClause = false,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.Unknown,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.InsufficientEvidence,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly,
            },
            new DefinitionClauseDecisionBridgeMapperFixture
            {
                SampleCode = "SYNTHETIC_TARGET_ONLY_REVIEW",
                LabelZh = "合成仅目标半完整证明样本",
                IsSynthetic = true,
                Context = new DefinitionClauseDecisionBridgeContext
                {
                    SourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                    StationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                    TopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                    ProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.TargetOnly,
                    LeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                    HasConflict = false,
                    HasReviewConflict = false,
                    RemovalBreaksClause = false,
                },
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.ReviewRequired,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview,
            },
        ];
    }
}
