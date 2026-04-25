namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflowResult
{
    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflowResult(
        string outputDirectory,
        bool isComplete,
        int exitCode,
        string consoleMessage,
        string validationJsonPath,
        string validationMarkdownPath)
    {
        OutputDirectory = outputDirectory;
        IsComplete = isComplete;
        ExitCode = exitCode;
        ConsoleMessage = consoleMessage;
        ValidationJsonPath = validationJsonPath;
        ValidationMarkdownPath = validationMarkdownPath;
    }

    public string OutputDirectory { get; }

    public bool IsComplete { get; }

    public int ExitCode { get; }

    public string ConsoleMessage { get; }

    public string ValidationJsonPath { get; }

    public string ValidationMarkdownPath { get; }
}
