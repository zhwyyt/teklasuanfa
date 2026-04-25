namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class DefinitionClauseDecisionBridgeEffectAdapter
{
    public static DefinitionClauseDecisionBridgeRawInputs ToRawInputs(
        DefinitionClauseDecisionBridgeEffectSnapshot snapshot)
    {
        return new DefinitionClauseDecisionBridgeRawInputs
        {
            HasRealInputEvidence = snapshot.HasRealInputEvidence,
            HasRecognitionInputEvidence = snapshot.HasRecognitionInputEvidence,
            PriorityStationCount = snapshot.PriorityStationCount,
            PriorityStationHitCount = snapshot.PriorityStationHitCount,
            HasLocalOnlyEvidence = snapshot.HasLocalOnlyEvidence,
            HasLostClosedLoop = snapshot.HasLostClosedLoop,
            HasLostEnvelopeSupport = snapshot.HasLostEnvelopeSupport,
            HasStructuralTopologyRewrite = snapshot.HasStructuralTopologyRewrite,
            HasDominantDimensionSwitch = snapshot.HasDominantDimensionSwitch,
            HasWeakTopologyChange = snapshot.HasWeakTopologyChange,
            HasMainContourBreak = snapshot.HasMainContourBreak,
            HasProofType = snapshot.HasProofType,
            HasFamilyProofTarget = snapshot.HasFamilyProofTarget,
            HasLeadClause = !string.IsNullOrWhiteSpace(snapshot.LeadClauseCode),
            LeadClauseShare = NormalizeLeadClauseShare(snapshot.LeadClauseShare),
            HasConflict = snapshot.HasConflict,
            HasReviewConflict = snapshot.HasReviewConflict,
            RemovalBreaksClause = snapshot.RemovalBreaksClause,
            CanBorrowLeadClauseAnchor = snapshot.CanBorrowLeadClauseAnchor,
        };
    }

    public static DefinitionClauseDecisionBridgeAdaptedResult Adapt(
        DefinitionClauseDecisionBridgeEffectSnapshot snapshot)
    {
        var rawInputs = ToRawInputs(snapshot);
        var context = DefinitionClauseDecisionBridgeMapper.Normalize(rawInputs);
        var result = DefinitionClauseDecisionBridgeMapper.Evaluate(context);

        return new DefinitionClauseDecisionBridgeAdaptedResult
        {
            RawInputs = rawInputs,
            Context = context,
            Result = result,
        };
    }

    private static double NormalizeLeadClauseShare(double share)
    {
        if (double.IsNaN(share) || double.IsInfinity(share))
        {
            return 0d;
        }

        if (share < 0d)
        {
            return 0d;
        }

        if (share > 1d)
        {
            return 1d;
        }

        return share;
    }
}
