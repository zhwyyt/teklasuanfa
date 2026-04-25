namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecisionBuilder
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecision Build(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit audit)
    {
        var exitCode = audit.IsComplete ? 0 : 1;

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecision(
            audit,
            exitCode,
            audit.ConsoleMessage);
    }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecision BuildFromOutputRootDirectory(
        string outputRootDirectory)
    {
        var audit =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditBuilder
                .BuildFromOutputRootDirectory(outputRootDirectory);

        return Build(audit);
    }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecision BuildFromOutputDirectory(
        string outputDirectory)
    {
        var audit =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditBuilder
                .BuildFromOutputDirectory(outputDirectory);

        return Build(audit);
    }
}
