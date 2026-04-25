namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditBuilder
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit BuildFromOutputRootDirectory(
        string outputRootDirectory)
    {
        var outputPaths =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPathResolver
                .ResolveFromOutputRootDirectory(outputRootDirectory);

        return BuildFromOutputPaths(outputPaths);
    }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit BuildFromOutputDirectory(
        string outputDirectory)
    {
        var outputPaths =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPathResolver
                .ResolveFromOutputDirectory(outputDirectory);

        return BuildFromOutputPaths(outputPaths);
    }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit BuildFromOutputPaths(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths outputPaths)
    {
        var snapshot =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactInspector
                .Inspect(outputPaths);

        var consoleMessage =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSummaryFormatter
                .BuildConsoleMessage(snapshot);

        var markdownBlock =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSummaryFormatter
                .BuildMarkdownBlock(snapshot);

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit(
            outputPaths,
            snapshot,
            consoleMessage,
            markdownBlock);
    }
}
