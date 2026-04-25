namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class BodyCandidatePartitionConservativeFixtureResult
{
    public BodyCandidatePartitionConservativeFixtureResult(
        string fixtureName,
        BodyCandidatePartitionDecision decision,
        BodyCandidatePartitionLane expectedLane,
        bool laneMatches,
        bool reviewMatches,
        BodyCandidatePartitionEvidenceCode[] missingEvidenceCodes,
        string[] missingReviewReasonSubstrings)
    {
        FixtureName = fixtureName;
        Decision = decision;
        ExpectedLane = expectedLane;
        LaneMatches = laneMatches;
        ReviewMatches = reviewMatches;
        MissingEvidenceCodes = missingEvidenceCodes;
        MissingReviewReasonSubstrings = missingReviewReasonSubstrings;
    }

    public string FixtureName { get; }

    public BodyCandidatePartitionDecision Decision { get; }

    public BodyCandidatePartitionLane ExpectedLane { get; }

    public bool LaneMatches { get; }

    public bool ReviewMatches { get; }

    public BodyCandidatePartitionEvidenceCode[] MissingEvidenceCodes { get; }

    public string[] MissingReviewReasonSubstrings { get; }

    public bool IsSuccess => LaneMatches
                             && ReviewMatches
                             && MissingEvidenceCodes.Length == 0
                             && MissingReviewReasonSubstrings.Length == 0;
}
