namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchBridge
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision Evaluate(
        string[] args,
        string defaultOutputRootDirectory)
    {
        if (!DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter.TryRun(
                args,
                defaultOutputRootDirectory,
                out var exitCode,
                out var message,
                out var outputDirectory))
        {
            return DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision.NotHandled();
        }

        return DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision.CreateHandled(
            exitCode,
            message,
            outputDirectory);
    }
}
