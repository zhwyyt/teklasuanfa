namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths
{
    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths(
        string outputRootDirectory,
        string outputDirectory,
        string manifestPath,
        string readmePath,
        string validationJsonPath,
        string validationMarkdownPath,
        string semanticRegressionBundleSectionMarkdownPath)
    {
        OutputRootDirectory = outputRootDirectory;
        OutputDirectory = outputDirectory;
        ManifestPath = manifestPath;
        ReadmePath = readmePath;
        ValidationJsonPath = validationJsonPath;
        ValidationMarkdownPath = validationMarkdownPath;
        SemanticRegressionBundleSectionMarkdownPath = semanticRegressionBundleSectionMarkdownPath;
    }

    public string OutputRootDirectory { get; }

    public string OutputDirectory { get; }

    public string ManifestPath { get; }

    public string ReadmePath { get; }

    public string ValidationJsonPath { get; }

    public string ValidationMarkdownPath { get; }

    public string SemanticRegressionBundleSectionMarkdownPath { get; }
}
