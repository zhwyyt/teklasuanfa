namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeAppEarlyCommandDispatcherHook
{
    public static bool TryRun(
        string[] args,
        string defaultOutputRootDirectory,
        out int exitCode)
    {
        var decision =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchBridge
                .Evaluate(args, defaultOutputRootDirectory);

        if (!decision.Handled)
        {
            exitCode = 0;
            return false;
        }

        WriteUserFacingOutput(decision);

        exitCode = decision.ExitCode;
        return true;
    }

    private static void WriteUserFacingOutput(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision decision)
    {
        if (!string.IsNullOrWhiteSpace(decision.Message))
        {
            System.Console.WriteLine(decision.Message);
        }

        if (!string.IsNullOrWhiteSpace(decision.OutputDirectory))
        {
            System.Console.WriteLine($"Output directory: {decision.OutputDirectory}");
        }
    }
}
