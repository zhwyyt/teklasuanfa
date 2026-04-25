using System;
using System.IO;

namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeWorkflowResult
{
    public required string OutputDirectory { get; init; }

    public required string ManifestPath { get; init; }

    public required string ReadmePath { get; init; }

    public required DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeResult PostMergeResult { get; init; }
}

internal static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeWorkflow
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeWorkflowResult Run(
        string outputRootDirectory)
    {
        var smokeOutputDirectory = Path.Combine(
            outputRootDirectory,
            "definition-clause-decision-bridge-validation-bundle-semantic-regression-post-merge-smoke");

        var postMergeResult =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMerge.Run(outputRootDirectory);
        var semanticRegressionSection = postMergeResult.SemanticRegressionMerge.Contribution.Attachment.Section;

        var manifest = new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeManifest
        {
            GeneratedAtUtc = DateTime.UtcNow,
            SmokeOutputDirectory = smokeOutputDirectory,
            PostMergeOutputDirectory = postMergeResult.OutputDirectory,
            ValidationSummaryPath = postMergeResult.SummaryPath,
            ValidationReadmePath = postMergeResult.ReadmePath,
            ValidationManifestPath = postMergeResult.ManifestPath,
            SemanticRegressionSummaryPath = semanticRegressionSection.MarkdownPath,
            SemanticRegressionReadmePath = semanticRegressionSection.ReadmePath,
            SemanticRegressionManifestPath = semanticRegressionSection.ManifestPath,
            SemanticRegressionBundleSectionJsonPath =
                postMergeResult.SemanticRegressionMerge.Contribution.Attachment.WorkflowResult.BundleSectionJsonPath,
            SemanticRegressionBundleSectionMarkdownPath =
                postMergeResult.SemanticRegressionMerge.Contribution.Attachment.WorkflowResult.BundleSectionMarkdownPath
        };

        var manifestPath =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeSerializer.WriteManifest(
                manifest,
                smokeOutputDirectory);
        var readme =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeReadmeBuilder.Build(
                manifest);
        var readmePath =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeSerializer.WriteReadme(
                readme,
                smokeOutputDirectory);

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeWorkflowResult
        {
            OutputDirectory = smokeOutputDirectory,
            ManifestPath = manifestPath,
            ReadmePath = readmePath,
            PostMergeResult = postMergeResult
        };
    }
}
