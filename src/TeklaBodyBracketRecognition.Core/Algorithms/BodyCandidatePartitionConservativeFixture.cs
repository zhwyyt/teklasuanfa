namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class BodyCandidatePartitionConservativeFixture
{
    public BodyCandidatePartitionConservativeFixture(
        string name,
        BodyCandidatePartitionSignalSnapshot snapshot,
        BodyCandidatePartitionLane expectedLane,
        BodyCandidatePartitionEvidenceCode[] expectedEvidenceCodes,
        bool expectedNeedsReview,
        string[] expectedReviewReasonSubstrings)
    {
        Name = name;
        Snapshot = snapshot;
        ExpectedLane = expectedLane;
        ExpectedEvidenceCodes = expectedEvidenceCodes;
        ExpectedNeedsReview = expectedNeedsReview;
        ExpectedReviewReasonSubstrings = expectedReviewReasonSubstrings;
    }

    public string Name { get; }

    public BodyCandidatePartitionSignalSnapshot Snapshot { get; }

    public BodyCandidatePartitionLane ExpectedLane { get; }

    public BodyCandidatePartitionEvidenceCode[] ExpectedEvidenceCodes { get; }

    public bool ExpectedNeedsReview { get; }

    public string[] ExpectedReviewReasonSubstrings { get; }
}
