using System;
using System.IO;
using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionSemanticRegressionWorkflow
{
    public static DefinitionClauseDecisionSemanticRegressionWorkflowResult Run(string outputRootDirectory)
    {
        var outputDirectory = Path.Combine(outputRootDirectory, "definition-clause-semantic-regression");
        var artifacts = DefinitionClauseDecisionSemanticRegressionArtifactBuilder.BuildDefault();
        var workflowResult = DefinitionClauseDecisionSemanticRegressionArtifactSerializer.Write(artifacts, outputDirectory);

        var manifest = new DefinitionClauseDecisionSemanticRegressionManifest
        {
            GeneratedAtUtc = DateTime.UtcNow,
            OutputDirectory = outputDirectory,
            SnapshotJsonFileName = Path.GetFileName(workflowResult.JsonPath),
            SnapshotMarkdownFileName = Path.GetFileName(workflowResult.MarkdownPath),
            RowCount = artifacts.Rows.Count
        };

        var manifestPath = DefinitionClauseDecisionSemanticRegressionManifestSerializer.Write(manifest, outputDirectory);
        var readmePath = Path.Combine(outputDirectory, "README.md");
        var readme = DefinitionClauseDecisionSemanticRegressionOutputReadmeBuilder.Build(manifest);
        File.WriteAllText(readmePath, readme, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        var bundleSection = DefinitionClauseDecisionSemanticRegressionBundleSectionBuilder.Build(
            new DefinitionClauseDecisionSemanticRegressionWorkflowResult
            {
                OutputDirectory = workflowResult.OutputDirectory,
                JsonPath = workflowResult.JsonPath,
                MarkdownPath = workflowResult.MarkdownPath,
                ManifestPath = manifestPath,
                ReadmePath = readmePath,
                RowCount = artifacts.Rows.Count,
                BundleSectionJsonPath = string.Empty,
                BundleSectionMarkdownPath = string.Empty
            });
        var bundleSectionJsonPath = DefinitionClauseDecisionSemanticRegressionBundleSectionSerializer.WriteJson(bundleSection, outputDirectory);
        var bundleSectionMarkdownPath = DefinitionClauseDecisionSemanticRegressionBundleSectionSerializer.WriteMarkdownSummary(bundleSection, outputDirectory);

        return new DefinitionClauseDecisionSemanticRegressionWorkflowResult
        {
            OutputDirectory = workflowResult.OutputDirectory,
            JsonPath = workflowResult.JsonPath,
            MarkdownPath = workflowResult.MarkdownPath,
            ManifestPath = manifestPath,
            ReadmePath = readmePath,
            RowCount = artifacts.Rows.Count,
            BundleSectionJsonPath = bundleSectionJsonPath,
            BundleSectionMarkdownPath = bundleSectionMarkdownPath
        };
    }
}
