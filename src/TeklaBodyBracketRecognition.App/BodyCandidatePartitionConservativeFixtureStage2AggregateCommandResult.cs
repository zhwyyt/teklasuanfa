using System;

namespace TeklaBodyBracketRecognition.App;

internal sealed record BodyCandidatePartitionConservativeFixtureStage2AggregateCommandResult(
    int ExitCode,
    string OutputDirectory,
    string SummaryMarkdownPath,
    string ReadmePath,
    string ManifestPath,
    string PackageOutputDirectory,
    string PackageJsonPath,
    string PackageMarkdownPath,
    string PackageManifestPath,
    string PackageReadmePath,
    string ValidationJsonPath,
    string ValidationMarkdownPath,
    string BundleOutputDirectory,
    string BundleSummaryMarkdownPath,
    string BundleReadmePath,
    string BundleManifestPath,
    bool ValidationSucceeded)
    : BodyCandidatePartitionConservativeFixtureStage2TopLevelCommandResult(
        ExitCode,
        OutputDirectory,
        SummaryMarkdownPath,
        ReadmePath,
        ManifestPath)
{
    public static BodyCandidatePartitionConservativeFixtureStage2AggregateCommandResult Failure(
        string outputDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory must not be empty.", nameof(outputDirectory));
        }

        return new BodyCandidatePartitionConservativeFixtureStage2AggregateCommandResult(
            ExitCode: 1,
            OutputDirectory: outputDirectory,
            SummaryMarkdownPath: string.Empty,
            ReadmePath: string.Empty,
            ManifestPath: string.Empty,
            PackageOutputDirectory: string.Empty,
            PackageJsonPath: string.Empty,
            PackageMarkdownPath: string.Empty,
            PackageManifestPath: string.Empty,
            PackageReadmePath: string.Empty,
            ValidationJsonPath: string.Empty,
            ValidationMarkdownPath: string.Empty,
            BundleOutputDirectory: string.Empty,
            BundleSummaryMarkdownPath: string.Empty,
            BundleReadmePath: string.Empty,
            BundleManifestPath: string.Empty,
            ValidationSucceeded: false);
    }
}
