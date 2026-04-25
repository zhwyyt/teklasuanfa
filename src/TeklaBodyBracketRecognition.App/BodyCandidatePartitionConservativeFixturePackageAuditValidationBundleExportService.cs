using System;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleExportService
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleCommandResult Export(
        string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
        {
            throw new ArgumentException("Output root directory must not be empty.", nameof(outputRootDirectory));
        }

        var workflowResult = BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleWorkflow.Run(outputRootDirectory);
        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleCommandResult(
            ExitCode: workflowResult.ValidationSucceeded ? 0 : 1,
            OutputDirectory: workflowResult.OutputDirectory,
            SummaryMarkdownPath: workflowResult.SummaryMarkdownPath,
            ReadmePath: workflowResult.ReadmePath,
            ManifestPath: workflowResult.ManifestPath,
            ValidationOutputDirectory: workflowResult.ValidationOutputDirectory,
            ValidationJsonPath: workflowResult.ValidationJsonPath,
            ValidationMarkdownPath: workflowResult.ValidationMarkdownPath,
            ValidationSucceeded: workflowResult.ValidationSucceeded);
    }
}
