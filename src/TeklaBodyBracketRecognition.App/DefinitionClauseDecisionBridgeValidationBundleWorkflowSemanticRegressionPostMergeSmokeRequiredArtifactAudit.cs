namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit
{
    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAudit(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths outputPaths,
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSnapshot snapshot,
        string consoleMessage,
        string markdownBlock)
    {
        OutputPaths = outputPaths;
        Snapshot = snapshot;
        ConsoleMessage = consoleMessage;
        MarkdownBlock = markdownBlock;
    }

    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths OutputPaths { get; }

    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSnapshot Snapshot { get; }

    public string ConsoleMessage { get; }

    public string MarkdownBlock { get; }

    public bool IsComplete => Snapshot.AllRequiredArtifactsPresent;
}
