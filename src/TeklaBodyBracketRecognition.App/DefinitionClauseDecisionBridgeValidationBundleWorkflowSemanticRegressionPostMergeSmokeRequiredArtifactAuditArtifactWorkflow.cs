namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflow
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflowResult RunFromOutputRootDirectory(
        string outputRootDirectory)
    {
        var decision =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecisionBuilder
                .BuildFromOutputRootDirectory(outputRootDirectory);

        return Run(decision);
    }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflowResult RunFromOutputDirectory(
        string outputDirectory)
    {
        var decision =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecisionBuilder
                .BuildFromOutputDirectory(outputDirectory);

        return Run(decision);
    }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflowResult Run(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecision decision)
    {
        var artifact =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactBuilder
                .Build(decision);

        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactSerializer
            .WriteJson(artifact);

        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactSerializer
            .WriteMarkdown(artifact);

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflowResult(
            outputDirectory: artifact.OutputDirectory,
            isComplete: artifact.IsComplete,
            exitCode: artifact.ExitCode,
            consoleMessage: artifact.ConsoleMessage,
            validationJsonPath: artifact.ValidationJsonPath,
            validationMarkdownPath: artifact.ValidationMarkdownPath);
    }
}
