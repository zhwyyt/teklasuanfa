namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifact
{
    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifact(
        string outputDirectory,
        bool isComplete,
        int exitCode,
        string consoleMessage,
        string manifestPath,
        string readmePath,
        string validationJsonPath,
        string validationMarkdownPath,
        string semanticRegressionBundleSectionMarkdownPath,
        string[] missingArtifacts,
        string markdownContent)
    {
        OutputDirectory = outputDirectory;
        IsComplete = isComplete;
        ExitCode = exitCode;
        ConsoleMessage = consoleMessage;
        ManifestPath = manifestPath;
        ReadmePath = readmePath;
        ValidationJsonPath = validationJsonPath;
        ValidationMarkdownPath = validationMarkdownPath;
        SemanticRegressionBundleSectionMarkdownPath = semanticRegressionBundleSectionMarkdownPath;
        MissingArtifacts = missingArtifacts;
        MarkdownContent = markdownContent;
    }

    public string OutputDirectory { get; }

    public bool IsComplete { get; }

    public int ExitCode { get; }

    public string ConsoleMessage { get; }

    public string ManifestPath { get; }

    public string ReadmePath { get; }

    public string ValidationJsonPath { get; }

    public string ValidationMarkdownPath { get; }

    public string SemanticRegressionBundleSectionMarkdownPath { get; }

    public string[] MissingArtifacts { get; }

    public string MarkdownContent { get; }
}
