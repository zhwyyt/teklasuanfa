namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecision
{
    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecision(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit audit,
        int exitCode,
        string consoleMessage)
    {
        Audit = audit;
        ExitCode = exitCode;
        ConsoleMessage = consoleMessage;
    }

    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit Audit { get; }

    public int ExitCode { get; }

    public string ConsoleMessage { get; }

    public bool IsComplete => Audit.IsComplete;
}
