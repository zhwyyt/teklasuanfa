namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision
{
    private DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision(
        bool handled,
        int exitCode,
        string? message,
        string? outputDirectory)
    {
        Handled = handled;
        ExitCode = exitCode;
        Message = message;
        OutputDirectory = outputDirectory;
    }

    public bool Handled { get; }

    public int ExitCode { get; }

    public string? Message { get; }

    public string? OutputDirectory { get; }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision NotHandled()
    {
        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision(
            handled: false,
            exitCode: 0,
            message: null,
            outputDirectory: null);
    }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision CreateHandled(
        int exitCode,
        string? message,
        string? outputDirectory)
    {
        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatchDecision(
            handled: true,
            exitCode: exitCode,
            message: message,
            outputDirectory: outputDirectory);
    }
}
