using System;
using System.IO;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleWorkflow
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleWorkflowResult Run(
        string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
        {
            throw new ArgumentException("Output root directory must not be empty.", nameof(outputRootDirectory));
        }

        var commandResult = BodyCandidatePartitionConservativeFixturePackageAuditExportService.Export(outputRootDirectory);
        return Run(commandResult);
    }

    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleWorkflowResult Run(
        BodyCandidatePartitionConservativeFixturePackageAuditCommandResult commandResult)
    {
        if (commandResult is null)
        {
            throw new ArgumentNullException(nameof(commandResult));
        }

        var contribution =
            BodyCandidatePartitionConservativeFixturePackageAuditValidationContributionBuilder.Build(commandResult);
        var manifestEntry =
            BodyCandidatePartitionConservativeFixturePackageAuditValidationContributionComposer.BuildManifestEntry(
                contribution);

        var outputRootDirectory = Path.GetDirectoryName(contribution.Attachment.CommandResult.OutputDirectory)
            ?? contribution.Attachment.CommandResult.OutputDirectory;
        var bundleOutputDirectory = Path.Combine(
            outputRootDirectory,
            "body-candidate-partition-conservative-fixture-package-audit-validation-bundle");

        var summaryMarkdown =
            BodyCandidatePartitionConservativeFixturePackageAuditValidationContributionComposer.AppendSummaryMarkdown(
                string.Empty,
                contribution);
        var readmeMarkdown =
            BodyCandidatePartitionConservativeFixturePackageAuditValidationContributionComposer.AppendReadmeMarkdown(
                string.Empty,
                contribution);
        var topLevelOutput = BodyCandidatePartitionConservativeFixtureStage2TopLevelOutputWorkflow.Write(
            bundleOutputDirectory,
            summaryMarkdown,
            (summaryFileName, readmeFileName) => new BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleManifest
            {
                GeneratedAtUtc = DateTime.UtcNow,
                OutputDirectory = bundleOutputDirectory,
                SummaryMarkdownFileName = summaryFileName,
                ReadmeFileName = readmeFileName,
                Entries = new[] { manifestEntry }
            },
            manifest => BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleReadmeBuilder.Build(
                manifest,
                readmeMarkdown));

        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleWorkflowResult
        {
            OutputDirectory = topLevelOutput.OutputDirectory,
            SummaryMarkdownPath = topLevelOutput.SummaryMarkdownPath,
            ReadmePath = topLevelOutput.ReadmePath,
            ManifestPath = topLevelOutput.ManifestPath,
            ValidationOutputDirectory = contribution.Attachment.CommandResult.OutputDirectory,
            ValidationJsonPath = contribution.Attachment.CommandResult.ValidationJsonPath,
            ValidationMarkdownPath = contribution.Attachment.CommandResult.ValidationMarkdownPath,
            ValidationSucceeded = contribution.Attachment.CommandResult.ValidationSucceeded
        };
    }
}
