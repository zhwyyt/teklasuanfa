using System;
using System.IO;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixtureStage2TopLevelOutputWorkflow
{
    public static BodyCandidatePartitionConservativeFixtureStage2TopLevelOutputWriteResult<TManifest> Write<TManifest>(
        string outputDirectory,
        string summaryMarkdown,
        Func<string, string, TManifest> buildManifest,
        Func<TManifest, string> buildReadme)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory must not be empty.", nameof(outputDirectory));
        }

        if (summaryMarkdown is null)
        {
            throw new ArgumentNullException(nameof(summaryMarkdown));
        }

        if (buildManifest is null)
        {
            throw new ArgumentNullException(nameof(buildManifest));
        }

        if (buildReadme is null)
        {
            throw new ArgumentNullException(nameof(buildReadme));
        }

        var summaryMarkdownPath =
            BodyCandidatePartitionConservativeFixtureStage2OutputArtifactWriter.WriteMarkdown(
                outputDirectory,
                "summary.md",
                summaryMarkdown);

        var manifest = buildManifest(Path.GetFileName(summaryMarkdownPath), "README.md");
        var readmeMarkdown = buildReadme(manifest);
        var readmePath =
            BodyCandidatePartitionConservativeFixtureStage2OutputArtifactWriter.WriteMarkdown(
                outputDirectory,
                "README.md",
                readmeMarkdown);

        var manifestPath =
            BodyCandidatePartitionConservativeFixtureStage2OutputArtifactWriter.WriteManifest(
                manifest,
                outputDirectory);

        return new BodyCandidatePartitionConservativeFixtureStage2TopLevelOutputWriteResult<TManifest>
        {
            OutputDirectory = outputDirectory,
            SummaryMarkdownPath = summaryMarkdownPath,
            ReadmePath = readmePath,
            ManifestPath = manifestPath,
            Manifest = manifest
        };
    }
}
