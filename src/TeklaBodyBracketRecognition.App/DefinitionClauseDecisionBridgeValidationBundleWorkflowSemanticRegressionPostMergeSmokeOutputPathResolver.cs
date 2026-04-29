namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPathResolver
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths ResolveFromOutputRootDirectory(
        string outputRootDirectory)
    {
        var outputDirectory = System.IO.Path.Combine(
            outputRootDirectory,
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.OutputDirectoryName);

        return ResolveFromOutputDirectory(outputRootDirectory, outputDirectory);
    }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths ResolveFromOutputDirectory(
        string outputDirectory)
    {
        var outputRootDirectory =
            System.IO.Directory.GetParent(outputDirectory)?.FullName
            ?? outputDirectory;

        return ResolveFromOutputDirectory(outputRootDirectory, outputDirectory);
    }

    private static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths ResolveFromOutputDirectory(
        string outputRootDirectory,
        string outputDirectory)
    {
        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths(
            outputRootDirectory: outputRootDirectory,
            outputDirectory: outputDirectory,
            manifestPath: System.IO.Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.ManifestFileName),
            readmePath: System.IO.Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.ReadmeFileName),
            validationJsonPath: System.IO.Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.ValidationJsonFileName),
            validationMarkdownPath: System.IO.Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.ValidationMarkdownFileName),
            semanticRegressionBundleSectionMarkdownPath: System.IO.Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputContract.SemanticRegressionBundleSectionMarkdownFileName));
    }
}
