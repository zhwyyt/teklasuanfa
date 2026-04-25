namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter
{
    public static bool TryRun(
        string[] args,
        string defaultOutputRootDirectory,
        out int exitCode,
        out string? message,
        out string? outputDirectory)
    {
        var result =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeAppEntry.TryRun(
                args,
                defaultOutputRootDirectory);

        exitCode = result.ExitCode;
        message = result.Message;
        outputDirectory = result.OutputDirectory;

        return result.Handled;
    }
}
