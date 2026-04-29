using System;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixtureStage2AggregateExportService
{
    public static BodyCandidatePartitionConservativeFixtureStage2AggregateCommandResult Export(
        string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
        {
            throw new ArgumentException("Output root directory must not be empty.", nameof(outputRootDirectory));
        }

        var workflowResult = BodyCandidatePartitionConservativeFixtureStage2AggregateWorkflow.Run(outputRootDirectory);
        return new BodyCandidatePartitionConservativeFixtureStage2AggregateCommandResult(
            ExitCode: workflowResult.ValidationSucceeded ? 0 : 1,
            OutputDirectory: workflowResult.OutputDirectory,
            SummaryMarkdownPath: workflowResult.SummaryMarkdownPath,
            ReadmePath: workflowResult.ReadmePath,
            ManifestPath: workflowResult.ManifestPath,
            PackageOutputDirectory: workflowResult.PackageOutputDirectory,
            PackageJsonPath: workflowResult.PackageJsonPath,
            PackageMarkdownPath: workflowResult.PackageMarkdownPath,
            PackageManifestPath: workflowResult.PackageManifestPath,
            PackageReadmePath: workflowResult.PackageReadmePath,
            ValidationJsonPath: workflowResult.ValidationJsonPath,
            ValidationMarkdownPath: workflowResult.ValidationMarkdownPath,
            BundleOutputDirectory: workflowResult.BundleOutputDirectory,
            BundleSummaryMarkdownPath: workflowResult.BundleSummaryMarkdownPath,
            BundleReadmePath: workflowResult.BundleReadmePath,
            BundleManifestPath: workflowResult.BundleManifestPath,
            ValidationSucceeded: workflowResult.ValidationSucceeded);
    }
}
