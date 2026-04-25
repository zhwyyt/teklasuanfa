namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeAppEntry
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandResult TryRun(
        string[] args,
        string defaultOutputRootDirectory)
    {
        return DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandHandler.TryHandle(
            args,
            defaultOutputRootDirectory);
    }
}
