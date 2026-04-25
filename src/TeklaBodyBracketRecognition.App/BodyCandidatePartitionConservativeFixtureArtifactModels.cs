using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public sealed class BodyCandidatePartitionConservativeFixtureArtifactRow
{
    public BodyCandidatePartitionConservativeFixtureArtifactRow(
        string fixtureName,
        BodyCandidatePartitionLane expectedLane,
        BodyCandidatePartitionLane actualLane,
        bool needsReview,
        bool isSuccess,
        BodyCandidatePartitionEvidenceCode[] evidenceCodes,
        string[] reviewReasons,
        BodyCandidatePartitionEvidenceCode[] missingEvidenceCodes,
        string[] missingReviewReasonSubstrings)
    {
        FixtureName = fixtureName;
        ExpectedLane = expectedLane;
        ActualLane = actualLane;
        NeedsReview = needsReview;
        IsSuccess = isSuccess;
        EvidenceCodes = evidenceCodes;
        ReviewReasons = reviewReasons;
        MissingEvidenceCodes = missingEvidenceCodes;
        MissingReviewReasonSubstrings = missingReviewReasonSubstrings;
    }

    public string FixtureName { get; }

    public BodyCandidatePartitionLane ExpectedLane { get; }

    public BodyCandidatePartitionLane ActualLane { get; }

    public bool NeedsReview { get; }

    public bool IsSuccess { get; }

    public BodyCandidatePartitionEvidenceCode[] EvidenceCodes { get; }

    public string[] ReviewReasons { get; }

    public BodyCandidatePartitionEvidenceCode[] MissingEvidenceCodes { get; }

    public string[] MissingReviewReasonSubstrings { get; }
}

public sealed class BodyCandidatePartitionConservativeFixtureArtifact
{
    public BodyCandidatePartitionConservativeFixtureArtifact(
        BodyCandidatePartitionConservativeFixtureArtifactRow[] rows,
        int totalCount,
        int passedCount,
        int failedCount,
        string markdownReport)
    {
        Rows = rows;
        TotalCount = totalCount;
        PassedCount = passedCount;
        FailedCount = failedCount;
        MarkdownReport = markdownReport;
    }

    public BodyCandidatePartitionConservativeFixtureArtifactRow[] Rows { get; }

    public int TotalCount { get; }

    public int PassedCount { get; }

    public int FailedCount { get; }

    public string MarkdownReport { get; }
}
