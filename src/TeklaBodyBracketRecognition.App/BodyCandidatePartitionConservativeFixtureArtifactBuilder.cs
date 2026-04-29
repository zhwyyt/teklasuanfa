using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureArtifactBuilder
{
    public static BodyCandidatePartitionConservativeFixtureArtifact Build(
        BodyCandidatePartitionConservativeFixtureResult[] results)
    {
        var rows = results
            .Select(result => new BodyCandidatePartitionConservativeFixtureArtifactRow(
                fixtureName: result.FixtureName,
                expectedLane: result.ExpectedLane,
                actualLane: result.Decision.Lane,
                needsReview: result.Decision.NeedsReview,
                isSuccess: result.IsSuccess,
                evidenceCodes: result.Decision.EvidenceCodes,
                reviewReasons: result.Decision.ReviewReasons,
                missingEvidenceCodes: result.MissingEvidenceCodes,
                missingReviewReasonSubstrings: result.MissingReviewReasonSubstrings))
            .ToArray();

        var totalCount = rows.Length;
        var passedCount = rows.Count(static row => row.IsSuccess);
        var failedCount = totalCount - passedCount;
        var markdownReport = BodyCandidatePartitionConservativeFixtureReportBuilder.BuildMarkdown(results);

        return new BodyCandidatePartitionConservativeFixtureArtifact(
            rows,
            totalCount,
            passedCount,
            failedCount,
            markdownReport);
    }
}
