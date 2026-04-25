using System;
using System.IO;
using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixtureStage2AggregateWorkflow
{
    public static BodyCandidatePartitionConservativeFixtureStage2AggregateWorkflowResult Run(
        string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
        {
            throw new ArgumentException("Output root directory must not be empty.", nameof(outputRootDirectory));
        }

        var packageResult = BodyCandidatePartitionConservativeFixturePackageAuditExportService.Export(outputRootDirectory);
        var bundleResult = BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleWorkflow.Run(packageResult);

        var aggregateOutputDirectory = Path.Combine(
            outputRootDirectory,
            "body-candidate-partition-conservative-fixture-stage2-aggregate");

        var summaryMarkdown = BuildSummaryMarkdown(packageResult, bundleResult);
        var topLevelOutput = BodyCandidatePartitionConservativeFixtureStage2TopLevelOutputWorkflow.Write(
            aggregateOutputDirectory,
            summaryMarkdown,
            (summaryFileName, readmeFileName) => new BodyCandidatePartitionConservativeFixtureStage2AggregateManifest
            {
                GeneratedAtUtc = DateTime.UtcNow,
                OutputDirectory = aggregateOutputDirectory,
                SummaryMarkdownFileName = summaryFileName,
                ReadmeFileName = readmeFileName,
                PackageOutputDirectory = packageResult.OutputDirectory,
                PackageJsonPath = packageResult.JsonPath,
                PackageMarkdownPath = packageResult.MarkdownPath,
                PackageManifestPath = packageResult.ManifestPath,
                PackageReadmePath = packageResult.ReadmePath,
                ValidationJsonPath = packageResult.ValidationJsonPath,
                ValidationMarkdownPath = packageResult.ValidationMarkdownPath,
                BundleOutputDirectory = bundleResult.OutputDirectory,
                BundleSummaryMarkdownPath = bundleResult.SummaryMarkdownPath,
                BundleReadmePath = bundleResult.ReadmePath,
                BundleManifestPath = bundleResult.ManifestPath,
                ValidationSucceeded = packageResult.ValidationSucceeded,
                PackageIsComplete = packageResult.PackageIsComplete,
                TotalCount = packageResult.TotalCount,
                PassedCount = packageResult.PassedCount,
                FailedCount = packageResult.FailedCount
            },
            manifest => BodyCandidatePartitionConservativeFixtureStage2AggregateReadmeBuilder.Build(
                manifest,
                summaryMarkdown));

        return new BodyCandidatePartitionConservativeFixtureStage2AggregateWorkflowResult
        {
            OutputDirectory = topLevelOutput.OutputDirectory,
            SummaryMarkdownPath = topLevelOutput.SummaryMarkdownPath,
            ReadmePath = topLevelOutput.ReadmePath,
            ManifestPath = topLevelOutput.ManifestPath,
            PackageOutputDirectory = packageResult.OutputDirectory,
            PackageJsonPath = packageResult.JsonPath,
            PackageMarkdownPath = packageResult.MarkdownPath,
            PackageManifestPath = packageResult.ManifestPath,
            PackageReadmePath = packageResult.ReadmePath,
            ValidationJsonPath = packageResult.ValidationJsonPath,
            ValidationMarkdownPath = packageResult.ValidationMarkdownPath,
            BundleOutputDirectory = bundleResult.OutputDirectory,
            BundleSummaryMarkdownPath = bundleResult.SummaryMarkdownPath,
            BundleReadmePath = bundleResult.ReadmePath,
            BundleManifestPath = bundleResult.ManifestPath,
            ValidationSucceeded = packageResult.ValidationSucceeded,
            PackageIsComplete = packageResult.PackageIsComplete,
            TotalCount = packageResult.TotalCount,
            PassedCount = packageResult.PassedCount,
            FailedCount = packageResult.FailedCount
        };
    }

    private static string BuildSummaryMarkdown(
        BodyCandidatePartitionConservativeFixturePackageAuditCommandResult packageResult,
        BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleWorkflowResult bundleResult)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Stage-2 Body Candidate Partition Aggregate");
        builder.AppendLine();
        builder.Append("总样本数：").AppendLine(packageResult.TotalCount.ToString());
        builder.Append("通过数：").AppendLine(packageResult.PassedCount.ToString());
        builder.Append("失败数：").AppendLine(packageResult.FailedCount.ToString());
        builder.Append("Package 完整：").AppendLine(packageResult.PackageIsComplete ? "是" : "否");
        builder.Append("Validation 成功：").AppendLine(packageResult.ValidationSucceeded ? "是" : "否");
        builder.AppendLine();
        builder.AppendLine("## Package Output");
        builder.AppendLine();
        builder.Append("目录：`").Append(packageResult.OutputDirectory).AppendLine("`");
        builder.Append("JSON：`").Append(packageResult.JsonPath).AppendLine("`");
        builder.Append("Markdown：`").Append(packageResult.MarkdownPath).AppendLine("`");
        builder.Append("Manifest：`").Append(packageResult.ManifestPath).AppendLine("`");
        builder.Append("README：`").Append(packageResult.ReadmePath).AppendLine("`");
        builder.Append("Validation JSON：`").Append(packageResult.ValidationJsonPath).AppendLine("`");
        builder.Append("Validation Markdown：`").Append(packageResult.ValidationMarkdownPath).AppendLine("`");
        builder.AppendLine();
        builder.AppendLine("## Validation Bundle");
        builder.AppendLine();
        builder.Append("目录：`").Append(bundleResult.OutputDirectory).AppendLine("`");
        builder.Append("Summary：`").Append(bundleResult.SummaryMarkdownPath).AppendLine("`");
        builder.Append("README：`").Append(bundleResult.ReadmePath).AppendLine("`");
        builder.Append("Manifest：`").Append(bundleResult.ManifestPath).AppendLine("`");
        return builder.ToString();
    }
}
