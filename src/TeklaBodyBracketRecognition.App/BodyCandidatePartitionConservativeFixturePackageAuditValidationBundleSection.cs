namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSection
{
    public const string SectionKey = "body-candidate-partition-stage2-validation";

    public required string Key { get; init; }

    public required string OutputDirectory { get; init; }

    public required string ValidationJsonPath { get; init; }

    public required string ValidationMarkdownPath { get; init; }

    public required bool ValidationSucceeded { get; init; }

    public required bool PackageIsComplete { get; init; }

    public required int TotalCount { get; init; }

    public required int PassedCount { get; init; }

    public required int FailedCount { get; init; }
}
