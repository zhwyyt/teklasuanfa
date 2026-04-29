namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyCandidatePartitionConservativeFixtureStage2AggregateWorkflowResult
    : BodyCandidatePartitionConservativeFixtureStage2TopLevelWorkflowResult
{
    public required string PackageOutputDirectory { get; init; }

    public required string PackageJsonPath { get; init; }

    public required string PackageMarkdownPath { get; init; }

    public required string PackageManifestPath { get; init; }

    public required string PackageReadmePath { get; init; }

    public required string ValidationJsonPath { get; init; }

    public required string ValidationMarkdownPath { get; init; }

    public required string BundleOutputDirectory { get; init; }

    public required string BundleSummaryMarkdownPath { get; init; }

    public required string BundleReadmePath { get; init; }

    public required string BundleManifestPath { get; init; }

    public required bool ValidationSucceeded { get; init; }

    public required bool PackageIsComplete { get; init; }

    public required int TotalCount { get; init; }

    public required int PassedCount { get; init; }

    public required int FailedCount { get; init; }
}
