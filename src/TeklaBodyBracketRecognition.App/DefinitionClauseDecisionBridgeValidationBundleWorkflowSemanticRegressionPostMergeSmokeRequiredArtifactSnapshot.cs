namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSnapshot
{
    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSnapshot(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths outputPaths,
        bool manifestExists,
        bool readmeExists,
        bool validationJsonExists,
        bool validationMarkdownExists,
        bool semanticRegressionBundleSectionMarkdownExists,
        string[] missingArtifacts)
    {
        OutputPaths = outputPaths;
        ManifestExists = manifestExists;
        ReadmeExists = readmeExists;
        ValidationJsonExists = validationJsonExists;
        ValidationMarkdownExists = validationMarkdownExists;
        SemanticRegressionBundleSectionMarkdownExists = semanticRegressionBundleSectionMarkdownExists;
        MissingArtifacts = missingArtifacts;
    }

    public DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths OutputPaths { get; }

    public bool ManifestExists { get; }

    public bool ReadmeExists { get; }

    public bool ValidationJsonExists { get; }

    public bool ValidationMarkdownExists { get; }

    public bool SemanticRegressionBundleSectionMarkdownExists { get; }

    public string[] MissingArtifacts { get; }

    public bool AllRequiredArtifactsPresent => MissingArtifacts.Length == 0;
}
