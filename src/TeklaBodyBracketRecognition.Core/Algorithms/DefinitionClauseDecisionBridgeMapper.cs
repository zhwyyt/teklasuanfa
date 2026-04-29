namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class DefinitionClauseDecisionBridgeMapper
{
    public static DefinitionClauseDecisionBridgeContext Normalize(DefinitionClauseDecisionBridgeRawInputs rawInputs)
    {
        return new DefinitionClauseDecisionBridgeContext
        {
            SourceTier = ResolveSourceTier(rawInputs),
            StationTier = ResolveStationTier(rawInputs),
            TopologyTier = ResolveTopologyTier(rawInputs),
            ProofCompletenessTier = ResolveProofCompletenessTier(rawInputs),
            LeadClauseTier = ResolveLeadClauseTier(rawInputs),
            HasConflict = rawInputs.HasConflict,
            HasReviewConflict = rawInputs.HasReviewConflict,
            RemovalBreaksClause = rawInputs.RemovalBreaksClause,
            CanBorrowLeadClauseAnchor = rawInputs.CanBorrowLeadClauseAnchor,
        };
    }

    public static DefinitionClauseDecisionBridgeMapperResult Evaluate(DefinitionClauseDecisionBridgeContext context)
    {
        var candidateDirection = ResolveCandidateDirection(context);
        var verdict = ResolveVerdict(context, candidateDirection);
        var promotionReadiness = ResolvePromotionReadiness(context, verdict);

        return new DefinitionClauseDecisionBridgeMapperResult
        {
            CandidateDirection = candidateDirection,
            Verdict = verdict,
            PromotionReadiness = promotionReadiness,
        };
    }

    public static DefinitionClauseDecisionBridgeSourceTier ResolveSourceTier(
        DefinitionClauseDecisionBridgeRawInputs rawInputs)
    {
        if (rawInputs.HasRealInputEvidence && rawInputs.HasRecognitionInputEvidence)
        {
            return DefinitionClauseDecisionBridgeSourceTier.DualConsistent;
        }

        if (rawInputs.HasRealInputEvidence)
        {
            return DefinitionClauseDecisionBridgeSourceTier.RealPrimary;
        }

        return DefinitionClauseDecisionBridgeSourceTier.RecognitionOnly;
    }

    public static DefinitionClauseDecisionBridgeStationTier ResolveStationTier(
        DefinitionClauseDecisionBridgeRawInputs rawInputs)
    {
        if (rawInputs.PriorityStationCount > 0 &&
            rawInputs.PriorityStationHitCount >= System.Math.Ceiling(rawInputs.PriorityStationCount / 2.0))
        {
            return DefinitionClauseDecisionBridgeStationTier.PriorityMajority;
        }

        if (rawInputs.PriorityStationHitCount > 0)
        {
            return DefinitionClauseDecisionBridgeStationTier.PriorityMinor;
        }

        if (rawInputs.HasLocalOnlyEvidence)
        {
            return DefinitionClauseDecisionBridgeStationTier.LocalOnly;
        }

        return DefinitionClauseDecisionBridgeStationTier.Episodic;
    }

    public static DefinitionClauseDecisionBridgeTopologyTier ResolveTopologyTier(
        DefinitionClauseDecisionBridgeRawInputs rawInputs)
    {
        if (rawInputs.HasLostClosedLoop)
        {
            return DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop;
        }

        if (rawInputs.HasLostEnvelopeSupport)
        {
            return DefinitionClauseDecisionBridgeTopologyTier.LostEnvelope;
        }

        if (rawInputs.HasMainContourBreak)
        {
            return DefinitionClauseDecisionBridgeTopologyTier.MainContourBreak;
        }

        if (rawInputs.HasStructuralTopologyRewrite)
        {
            return DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural;
        }

        if (rawInputs.HasDominantDimensionSwitch)
        {
            return DefinitionClauseDecisionBridgeTopologyTier.DimensionSwitch;
        }

        if (rawInputs.HasWeakTopologyChange)
        {
            return DefinitionClauseDecisionBridgeTopologyTier.WeakChange;
        }

        return DefinitionClauseDecisionBridgeTopologyTier.None;
    }

    public static DefinitionClauseDecisionBridgeProofCompletenessTier ResolveProofCompletenessTier(
        DefinitionClauseDecisionBridgeRawInputs rawInputs)
    {
        if (rawInputs.HasProofType && rawInputs.HasFamilyProofTarget)
        {
            return DefinitionClauseDecisionBridgeProofCompletenessTier.Complete;
        }

        if (rawInputs.HasProofType)
        {
            return DefinitionClauseDecisionBridgeProofCompletenessTier.TypeOnly;
        }

        if (rawInputs.HasFamilyProofTarget)
        {
            return DefinitionClauseDecisionBridgeProofCompletenessTier.TargetOnly;
        }

        return DefinitionClauseDecisionBridgeProofCompletenessTier.Unresolved;
    }

    public static DefinitionClauseDecisionBridgeLeadClauseTier ResolveLeadClauseTier(
        DefinitionClauseDecisionBridgeRawInputs rawInputs)
    {
        if (!rawInputs.HasLeadClause)
        {
            return DefinitionClauseDecisionBridgeLeadClauseTier.Unset;
        }

        if (rawInputs.LeadClauseShare >= 0.999)
        {
            return DefinitionClauseDecisionBridgeLeadClauseTier.StableFull;
        }

        if (rawInputs.LeadClauseShare >= 0.67)
        {
            return DefinitionClauseDecisionBridgeLeadClauseTier.Dominant;
        }

        return DefinitionClauseDecisionBridgeLeadClauseTier.Weak;
    }

    public static DefinitionClauseDecisionBridgeCandidateDirection ResolveCandidateDirection(
        DefinitionClauseDecisionBridgeContext context)
    {
        if (context.SourceTier == DefinitionClauseDecisionBridgeSourceTier.RecognitionOnly)
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.Unknown;
        }

        if (context.StationTier < DefinitionClauseDecisionBridgeStationTier.PriorityMinor)
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.Unknown;
        }

        if (context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop)
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate;
        }

        if (context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.LostEnvelope)
        {
            return context.RemovalBreaksClause
                ? DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate
                : DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate;
        }

        if (context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.MainContourBreak)
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate;
        }

        if (context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural &&
            context.ProofCompletenessTier != DefinitionClauseDecisionBridgeProofCompletenessTier.Unresolved &&
            context.LeadClauseTier >= DefinitionClauseDecisionBridgeLeadClauseTier.Dominant)
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate;
        }

        if (context.LeadClauseTier >= DefinitionClauseDecisionBridgeLeadClauseTier.Dominant &&
            (context.HasConflict || context.TopologyTier >= DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural))
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate;
        }

        if (context.LeadClauseTier == DefinitionClauseDecisionBridgeLeadClauseTier.Weak &&
            context.TopologyTier > DefinitionClauseDecisionBridgeTopologyTier.WeakChange)
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate;
        }

        return DefinitionClauseDecisionBridgeCandidateDirection.Unknown;
    }

    public static DefinitionClauseDecisionBridgeVerdict ResolveVerdict(
        DefinitionClauseDecisionBridgeContext context,
        DefinitionClauseDecisionBridgeCandidateDirection candidateDirection)
    {
        if (candidateDirection == DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate &&
            context.SourceTier == DefinitionClauseDecisionBridgeSourceTier.RealPrimary &&
            context.StationTier == DefinitionClauseDecisionBridgeStationTier.PriorityMajority &&
            (context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop ||
             context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.LostEnvelope ||
             context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.MainContourBreak) &&
            context.CanBorrowLeadClauseAnchor &&
            context.LeadClauseTier == DefinitionClauseDecisionBridgeLeadClauseTier.StableFull &&
            !context.HasReviewConflict)
        {
            // A sibling row in the same assembly already anchored the clause at 100%,
            // so this row can inherit the broken-direction clause context while still
            // staying review-gated at the readiness layer.
            return DefinitionClauseDecisionBridgeVerdict.Broken;
        }

        if (candidateDirection == DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate &&
            context.SourceTier != DefinitionClauseDecisionBridgeSourceTier.RecognitionOnly &&
            context.StationTier == DefinitionClauseDecisionBridgeStationTier.PriorityMajority &&
            (context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop ||
             context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.LostEnvelope ||
             context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.MainContourBreak) &&
            (context.HasConflict ||
             context.HasReviewConflict ||
             context.LeadClauseTier != DefinitionClauseDecisionBridgeLeadClauseTier.StableFull ||
             context.ProofCompletenessTier != DefinitionClauseDecisionBridgeProofCompletenessTier.Complete))
        {
            // Structural break is already strong enough to leave the effect-only layer,
            // but incomplete clause anchoring or boundary conflicts should still stop
            // short of automatic broken/promotion and require manual review.
            return DefinitionClauseDecisionBridgeVerdict.ReviewRequired;
        }

        if ((candidateDirection == DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate ||
             candidateDirection == DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate) &&
            (context.HasReviewConflict ||
             (context.LeadClauseTier == DefinitionClauseDecisionBridgeLeadClauseTier.StableFull &&
              context.ProofCompletenessTier != DefinitionClauseDecisionBridgeProofCompletenessTier.Complete)))
        {
            return DefinitionClauseDecisionBridgeVerdict.ReviewRequired;
        }

        if (candidateDirection == DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate &&
            context.SourceTier != DefinitionClauseDecisionBridgeSourceTier.RecognitionOnly &&
            context.StationTier == DefinitionClauseDecisionBridgeStationTier.PriorityMajority &&
            context.LeadClauseTier == DefinitionClauseDecisionBridgeLeadClauseTier.StableFull &&
            context.ProofCompletenessTier == DefinitionClauseDecisionBridgeProofCompletenessTier.Complete &&
            !context.HasConflict &&
            !context.HasReviewConflict)
        {
            return DefinitionClauseDecisionBridgeVerdict.Satisfied;
        }

        if (candidateDirection == DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate &&
            context.SourceTier == DefinitionClauseDecisionBridgeSourceTier.RealPrimary &&
            context.StationTier == DefinitionClauseDecisionBridgeStationTier.PriorityMajority &&
            (context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop ||
             context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.LostEnvelope ||
             context.TopologyTier == DefinitionClauseDecisionBridgeTopologyTier.MainContourBreak) &&
            !context.HasConflict &&
            !context.HasReviewConflict)
        {
            return DefinitionClauseDecisionBridgeVerdict.Broken;
        }

        if (candidateDirection == DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate ||
            (context.LeadClauseTier >= DefinitionClauseDecisionBridgeLeadClauseTier.Dominant &&
             (context.HasConflict ||
              context.ProofCompletenessTier != DefinitionClauseDecisionBridgeProofCompletenessTier.Complete)))
        {
            return DefinitionClauseDecisionBridgeVerdict.Mixed;
        }

        return DefinitionClauseDecisionBridgeVerdict.InsufficientEvidence;
    }

    public static DefinitionClauseDecisionBridgePromotionReadiness ResolvePromotionReadiness(
        DefinitionClauseDecisionBridgeContext context,
        DefinitionClauseDecisionBridgeVerdict verdict)
    {
        if ((verdict == DefinitionClauseDecisionBridgeVerdict.Satisfied ||
             verdict == DefinitionClauseDecisionBridgeVerdict.Broken) &&
            context.SourceTier != DefinitionClauseDecisionBridgeSourceTier.RecognitionOnly &&
            context.StationTier == DefinitionClauseDecisionBridgeStationTier.PriorityMajority &&
            context.ProofCompletenessTier == DefinitionClauseDecisionBridgeProofCompletenessTier.Complete &&
            context.LeadClauseTier == DefinitionClauseDecisionBridgeLeadClauseTier.StableFull &&
            !context.HasConflict &&
            !context.HasReviewConflict)
        {
            return DefinitionClauseDecisionBridgePromotionReadiness.ReadyForPromotion;
        }

        if (verdict == DefinitionClauseDecisionBridgeVerdict.ReviewRequired ||
            verdict == DefinitionClauseDecisionBridgeVerdict.Broken ||
            (verdict == DefinitionClauseDecisionBridgeVerdict.Satisfied && context.HasReviewConflict))
        {
            return DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview;
        }

        return DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly;
    }
}
