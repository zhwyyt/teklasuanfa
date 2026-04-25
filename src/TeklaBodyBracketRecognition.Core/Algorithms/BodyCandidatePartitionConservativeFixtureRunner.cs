namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class BodyCandidatePartitionConservativeFixtureRunner
{
    public static BodyCandidatePartitionConservativeFixtureResult[] RunDefault()
    {
        return Run(BodyCandidatePartitionConservativeFixtures.CreateDefault());
    }

    public static BodyCandidatePartitionConservativeFixtureResult[] Run(
        BodyCandidatePartitionConservativeFixture[] fixtures)
    {
        var results = new BodyCandidatePartitionConservativeFixtureResult[fixtures.Length];

        for (var i = 0; i < fixtures.Length; i++)
        {
            results[i] = Run(fixtures[i]);
        }

        return results;
    }

    public static BodyCandidatePartitionConservativeFixtureResult Run(
        BodyCandidatePartitionConservativeFixture fixture)
    {
        var decision = BodyCandidatePartitionConservativeMapper.Decide(fixture.Snapshot);

        var missingEvidenceCodes = fixture.ExpectedEvidenceCodes
            .Where(code => !decision.EvidenceCodes.Contains(code))
            .ToArray();

        var missingReviewReasonSubstrings = fixture.ExpectedReviewReasonSubstrings
            .Where(expectedSubstring =>
                !decision.ReviewReasons.Any(reason =>
                    reason.Contains(expectedSubstring, System.StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        return new BodyCandidatePartitionConservativeFixtureResult(
            fixtureName: fixture.Name,
            decision: decision,
            expectedLane: fixture.ExpectedLane,
            laneMatches: decision.Lane == fixture.ExpectedLane,
            reviewMatches: decision.NeedsReview == fixture.ExpectedNeedsReview,
            missingEvidenceCodes: missingEvidenceCodes,
            missingReviewReasonSubstrings: missingReviewReasonSubstrings);
    }
}
