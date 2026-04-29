namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class BodyCandidatePartitionConservativeMapper
{
    public static BodyCandidatePartitionDecision Decide(BodyCandidatePartitionSignalSnapshot snapshot)
    {
        var evidenceCodes = BuildEvidenceCodes(snapshot);
        var reviewReasons = new System.Collections.Generic.List<string>(snapshot.ReviewReasons);

        var hasCoreSignals = HasCoreSignals(snapshot);
        var hasSupportSignals = HasSupportSignals(snapshot);

        if (snapshot.MatchesBodyAccessoryTopology && (hasCoreSignals || hasSupportSignals))
        {
            reviewReasons.Add("Accessory topology conflicts with body-core/body-support signals.");
            return CreateReviewDecision(snapshot, evidenceCodes, reviewReasons);
        }

        if (snapshot.HasExternalContactOnly && (hasCoreSignals || hasSupportSignals || snapshot.MatchesBodyAccessoryTopology))
        {
            reviewReasons.Add("External-contact-only signal conflicts with internal body candidate signals.");
            return CreateReviewDecision(snapshot, evidenceCodes, reviewReasons);
        }

        if (snapshot.HasConflictingTopology || snapshot.HasInsufficientTrace || reviewReasons.Count > 0)
        {
            return CreateReviewDecision(snapshot, evidenceCodes, reviewReasons);
        }

        if (hasCoreSignals)
        {
            return new BodyCandidatePartitionDecision(
                snapshot.CandidateId,
                BodyCandidatePartitionLane.BodyCoreCandidate,
                evidenceCodes.ToArray(),
                System.Array.Empty<string>());
        }

        if (hasSupportSignals)
        {
            return new BodyCandidatePartitionDecision(
                snapshot.CandidateId,
                BodyCandidatePartitionLane.BodySupportCandidate,
                evidenceCodes.ToArray(),
                System.Array.Empty<string>());
        }

        if (snapshot.MatchesBodyAccessoryTopology)
        {
            return new BodyCandidatePartitionDecision(
                snapshot.CandidateId,
                BodyCandidatePartitionLane.BodyAccessory,
                evidenceCodes.ToArray(),
                System.Array.Empty<string>());
        }

        if (snapshot.HasExternalContactOnly)
        {
            return new BodyCandidatePartitionDecision(
                snapshot.CandidateId,
                BodyCandidatePartitionLane.ExternalContext,
                evidenceCodes.ToArray(),
                System.Array.Empty<string>());
        }

        reviewReasons.Add("No conservative body-candidate partition lane could be established.");
        return CreateReviewDecision(snapshot, evidenceCodes, reviewReasons);
    }

    private static bool HasCoreSignals(BodyCandidatePartitionSignalSnapshot snapshot)
    {
        return snapshot.HasStableZoneSpanControl
               || snapshot.HasMainContourControl
               || snapshot.HasClosedLoopControl;
    }

    private static bool HasSupportSignals(BodyCandidatePartitionSignalSnapshot snapshot)
    {
        return snapshot.HasSingleMainPlateContinuity
               || snapshot.SupportsBodyCoreAdjacency;
    }

    private static System.Collections.Generic.List<BodyCandidatePartitionEvidenceCode> BuildEvidenceCodes(
        BodyCandidatePartitionSignalSnapshot snapshot)
    {
        var evidenceCodes = new System.Collections.Generic.List<BodyCandidatePartitionEvidenceCode>();

        if (snapshot.HasStableZoneSpanControl)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.StableZoneSpan);
        }

        if (snapshot.HasMainContourControl)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.MainContourControl);
        }

        if (snapshot.HasClosedLoopControl)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.ClosedLoopControl);
        }

        if (snapshot.HasSingleMainPlateContinuity)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.SingleMainPlateContinuity);
        }

        if (snapshot.SupportsBodyCoreAdjacency)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.BodySupportAdjacency);
        }

        if (snapshot.MatchesBodyAccessoryTopology)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.BodyAccessoryTopology);
        }

        if (snapshot.HasExternalContactOnly)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.ExternalContactOnly);
        }

        if (snapshot.HasConflictingTopology)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.ConflictingTopology);
        }

        if (snapshot.HasInsufficientTrace)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.InsufficientTrace);
        }

        return evidenceCodes;
    }

    private static BodyCandidatePartitionDecision CreateReviewDecision(
        BodyCandidatePartitionSignalSnapshot snapshot,
        System.Collections.Generic.List<BodyCandidatePartitionEvidenceCode> evidenceCodes,
        System.Collections.Generic.List<string> reviewReasons)
    {
        if (!evidenceCodes.Contains(BodyCandidatePartitionEvidenceCode.ConflictingTopology)
            && reviewReasons.Count > 0)
        {
            evidenceCodes.Add(BodyCandidatePartitionEvidenceCode.ConflictingTopology);
        }

        return new BodyCandidatePartitionDecision(
            snapshot.CandidateId,
            BodyCandidatePartitionLane.ReviewRequired,
            evidenceCodes.ToArray(),
            reviewReasons.ToArray());
    }
}
