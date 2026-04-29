using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionBridgeEffectAdapterFixture
{
    public string SampleCode { get; set; } = string.Empty;

    public string LabelZh { get; set; } = string.Empty;

    public bool IsSynthetic { get; set; }

    public DefinitionClauseDecisionBridgeEffectSnapshot Snapshot { get; set; } = new();

    public DefinitionClauseDecisionBridgeSourceTier ExpectedSourceTier { get; set; }

    public DefinitionClauseDecisionBridgeStationTier ExpectedStationTier { get; set; }

    public DefinitionClauseDecisionBridgeTopologyTier ExpectedTopologyTier { get; set; }

    public DefinitionClauseDecisionBridgeProofCompletenessTier ExpectedProofCompletenessTier { get; set; }

    public DefinitionClauseDecisionBridgeLeadClauseTier ExpectedLeadClauseTier { get; set; }

    public DefinitionClauseDecisionBridgeCandidateDirection ExpectedCandidateDirection { get; set; }

    public DefinitionClauseDecisionBridgeVerdict ExpectedVerdict { get; set; }

    public DefinitionClauseDecisionBridgePromotionReadiness ExpectedPromotionReadiness { get; set; }
}

public static class DefinitionClauseDecisionBridgeEffectAdapterFixtures
{
    public static IReadOnlyList<DefinitionClauseDecisionBridgeEffectAdapterFixture> CreateDefault()
    {
        return
        [
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "GKZ",
                LabelZh = "GKZ 对壁闭合正例",
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    HasRecognitionInputEvidence = false,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 6,
                    HasStructuralTopologyRewrite = true,
                    HasProofType = true,
                    HasFamilyProofTarget = true,
                    LeadClauseCode = "BOX_CLOSED_OPPOSED",
                    LeadClauseShare = 1.0d,
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Complete,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Satisfied,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForPromotion,
            },
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "HXZ",
                LabelZh = "HXZ 单主板持续性正例",
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    HasRecognitionInputEvidence = false,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 6,
                    HasStructuralTopologyRewrite = true,
                    HasProofType = true,
                    HasFamilyProofTarget = true,
                    LeadClauseCode = "MAIN_PLATE_PERSISTENCE",
                    LeadClauseShare = 1.0d,
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Complete,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Satisfied,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForPromotion,
            },
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "GL",
                LabelZh = "GL 混合改写保守样本",
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 6,
                    HasStructuralTopologyRewrite = true,
                    HasProofType = true,
                    HasFamilyProofTarget = false,
                    LeadClauseCode = "MAIN_PLATE_PERSISTENCE",
                    LeadClauseShare = 0.67d,
                    HasConflict = true,
                    ConflictReasons = ["CLAUSE_MIX"],
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.TypeOnly,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.Dominant,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Mixed,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly,
            },
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "MJ",
                LabelZh = "MJ 需复核样本",
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 6,
                    HasStructuralTopologyRewrite = true,
                    HasProofType = true,
                    LeadClauseCode = "MAIN_PLATE_PERSISTENCE",
                    LeadClauseShare = 1.0d,
                    HasReviewConflict = true,
                    ConflictReasons = ["BOUNDARY_REVIEW"],
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.TypeOnly,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.ReviewRequired,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview,
            },
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "YPGL",
                LabelZh = "YPGL 混合样本",
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 2,
                    HasStructuralTopologyRewrite = true,
                    LeadClauseCode = "MAIN_PLATE_PERSISTENCE",
                    LeadClauseShare = 0.67d,
                    HasConflict = true,
                    ConflictReasons = ["MIXED_PROOF_TARGET"],
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMinor,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Unresolved,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.Dominant,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Mixed,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly,
            },
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "SYNTHETIC_BROKEN_CLOSED_LOOP",
                LabelZh = "合成闭环破坏探针",
                IsSynthetic = true,
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 6,
                    HasLostClosedLoop = true,
                    HasProofType = true,
                    HasFamilyProofTarget = true,
                    LeadClauseCode = "BOX_CLOSED_OPPOSED",
                    LeadClauseShare = 1.0d,
                    RemovalBreaksClause = true,
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Complete,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Broken,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForPromotion,
            },
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "SYNTHETIC_BROKEN_NEEDS_REVIEW",
                LabelZh = "合成结构破坏但条款锚点不足样本",
                IsSynthetic = true,
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 6,
                    HasLostClosedLoop = true,
                    RemovalBreaksClause = true,
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Unresolved,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.Unset,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.ReviewRequired,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview,
            },
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "SYNTHETIC_BROKEN_BORROWABLE_ANCHOR",
                LabelZh = "合成结构破坏且可借 assembly 主条款锚点样本",
                IsSynthetic = true,
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 6,
                    HasLostClosedLoop = true,
                    LeadClauseCode = "PRIMARY_PLATE_CONTINUITY_CLAUSE",
                    LeadClauseShare = 1.0d,
                    RemovalBreaksClause = true,
                    HasConflict = true,
                    CanBorrowLeadClauseAnchor = true,
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Unresolved,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.Broken,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview,
            },
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "SYNTHETIC_NONE_TOPOLOGY",
                LabelZh = "合成无拓扑信号兜底样本",
                IsSynthetic = true,
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 6,
                    HasProofType = true,
                    HasFamilyProofTarget = true,
                    LeadClauseCode = "MAIN_PLATE_PERSISTENCE",
                    LeadClauseShare = 1.0d,
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.None,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.Complete,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.Unknown,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.InsufficientEvidence,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly,
            },
            new DefinitionClauseDecisionBridgeEffectAdapterFixture
            {
                SampleCode = "SYNTHETIC_TARGET_ONLY_REVIEW",
                LabelZh = "合成仅目标半完整证明样本",
                IsSynthetic = true,
                Snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
                {
                    HasRealInputEvidence = true,
                    PriorityStationCount = 6,
                    PriorityStationHitCount = 6,
                    HasStructuralTopologyRewrite = true,
                    HasFamilyProofTarget = true,
                    LeadClauseCode = "BOX_CLOSED_OPPOSED",
                    LeadClauseShare = 1.0d,
                },
                ExpectedSourceTier = DefinitionClauseDecisionBridgeSourceTier.RealPrimary,
                ExpectedStationTier = DefinitionClauseDecisionBridgeStationTier.PriorityMajority,
                ExpectedTopologyTier = DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural,
                ExpectedProofCompletenessTier = DefinitionClauseDecisionBridgeProofCompletenessTier.TargetOnly,
                ExpectedLeadClauseTier = DefinitionClauseDecisionBridgeLeadClauseTier.StableFull,
                ExpectedCandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
                ExpectedVerdict = DefinitionClauseDecisionBridgeVerdict.ReviewRequired,
                ExpectedPromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview,
            },
        ];
    }
}
