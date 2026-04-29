namespace TeklaBodyBracketRecognition.Core.Algorithms;

public enum BodyCandidatePartitionEvidenceCode
{
    StableZoneSpan = 0,
    MainContourControl = 1,
    ClosedLoopControl = 2,
    SingleMainPlateContinuity = 3,
    BodyAccessoryTopology = 4,
    ExternalContactOnly = 5,
    ConflictingTopology = 6,
    InsufficientTrace = 7,
    BodySupportAdjacency = 8,
}
