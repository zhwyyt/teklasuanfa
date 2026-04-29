namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactBuilder
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifact Build(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecision decision)
    {
        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifact(
            outputDirectory: decision.Audit.OutputPaths.OutputDirectory,
            isComplete: decision.IsComplete,
            exitCode: decision.ExitCode,
            consoleMessage: decision.ConsoleMessage,
            manifestPath: decision.Audit.OutputPaths.ManifestPath,
            readmePath: decision.Audit.OutputPaths.ReadmePath,
            validationJsonPath: decision.Audit.OutputPaths.ValidationJsonPath,
            validationMarkdownPath: decision.Audit.OutputPaths.ValidationMarkdownPath,
            semanticRegressionBundleSectionMarkdownPath:
                decision.Audit.OutputPaths.SemanticRegressionBundleSectionMarkdownPath,
            missingArtifacts: decision.Audit.Snapshot.MissingArtifacts,
            markdownContent: decision.Audit.MarkdownBlock);
    }
}
