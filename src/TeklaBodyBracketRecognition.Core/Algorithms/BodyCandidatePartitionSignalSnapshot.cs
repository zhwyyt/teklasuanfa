namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class BodyCandidatePartitionSignalSnapshot
{
    public BodyCandidatePartitionSignalSnapshot(
        string candidateId,
        bool hasStableZoneSpanControl,
        bool hasMainContourControl,
        bool hasClosedLoopControl,
        bool hasSingleMainPlateContinuity,
        bool supportsBodyCoreAdjacency,
        bool matchesBodyAccessoryTopology,
        bool hasExternalContactOnly,
        bool hasConflictingTopology,
        bool hasInsufficientTrace,
        string[] reviewReasons)
    {
        CandidateId = candidateId;
        HasStableZoneSpanControl = hasStableZoneSpanControl;
        HasMainContourControl = hasMainContourControl;
        HasClosedLoopControl = hasClosedLoopControl;
        HasSingleMainPlateContinuity = hasSingleMainPlateContinuity;
        SupportsBodyCoreAdjacency = supportsBodyCoreAdjacency;
        MatchesBodyAccessoryTopology = matchesBodyAccessoryTopology;
        HasExternalContactOnly = hasExternalContactOnly;
        HasConflictingTopology = hasConflictingTopology;
        HasInsufficientTrace = hasInsufficientTrace;
        ReviewReasons = reviewReasons;
    }

    public string CandidateId { get; }

    public bool HasStableZoneSpanControl { get; }

    public bool HasMainContourControl { get; }

    public bool HasClosedLoopControl { get; }

    public bool HasSingleMainPlateContinuity { get; }

    public bool SupportsBodyCoreAdjacency { get; }

    public bool MatchesBodyAccessoryTopology { get; }

    public bool HasExternalContactOnly { get; }

    public bool HasConflictingTopology { get; }

    public bool HasInsufficientTrace { get; }

    public string[] ReviewReasons { get; }
}
