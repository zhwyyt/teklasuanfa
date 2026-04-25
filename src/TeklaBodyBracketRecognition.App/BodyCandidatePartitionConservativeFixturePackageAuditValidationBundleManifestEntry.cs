namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleManifestEntry
{
    public required string SectionKey { get; init; }

    public required string OutputDirectory { get; init; }

    public required string ValidationJsonPath { get; init; }

    public required string ValidationMarkdownPath { get; init; }

    public required bool ValidationSucceeded { get; init; }

    public required bool PackageIsComplete { get; init; }

    public required int TotalCount { get; init; }

    public required int PassedCount { get; init; }

    public required int FailedCount { get; init; }
}
