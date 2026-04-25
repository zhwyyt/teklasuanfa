namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactInspector
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSnapshot Inspect(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths outputPaths)
    {
        var manifestExists = System.IO.File.Exists(outputPaths.ManifestPath);
        var readmeExists = System.IO.File.Exists(outputPaths.ReadmePath);
        var validationJsonExists = System.IO.File.Exists(outputPaths.ValidationJsonPath);
        var validationMarkdownExists = System.IO.File.Exists(outputPaths.ValidationMarkdownPath);
        var semanticRegressionBundleSectionMarkdownExists =
            System.IO.File.Exists(outputPaths.SemanticRegressionBundleSectionMarkdownPath);

        var missingArtifacts = new System.Collections.Generic.List<string>();

        if (!manifestExists)
        {
            missingArtifacts.Add(
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.ManifestFileName);
        }

        if (!readmeExists)
        {
            missingArtifacts.Add(
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.ReadmeFileName);
        }

        if (!validationJsonExists)
        {
            missingArtifacts.Add(
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.ValidationJsonFileName);
        }

        if (!validationMarkdownExists)
        {
            missingArtifacts.Add(
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.ValidationMarkdownFileName);
        }

        if (!semanticRegressionBundleSectionMarkdownExists)
        {
            missingArtifacts.Add(
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.SemanticRegressionBundleSectionMarkdownFileName);
        }

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSnapshot(
            outputPaths,
            manifestExists,
            readmeExists,
            validationJsonExists,
            validationMarkdownExists,
            semanticRegressionBundleSectionMarkdownExists,
            missingArtifacts.ToArray());
    }
}
