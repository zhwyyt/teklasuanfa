namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactSerializer
{
    public static void WriteJson(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifact artifact)
    {
        System.IO.Directory.CreateDirectory(artifact.OutputDirectory);

        var payload = new
        {
            artifact.IsComplete,
            artifact.ExitCode,
            artifact.ConsoleMessage,
            artifact.ManifestPath,
            artifact.ReadmePath,
            artifact.ValidationJsonPath,
            artifact.ValidationMarkdownPath,
            artifact.SemanticRegressionBundleSectionMarkdownPath,
            artifact.MissingArtifacts,
        };

        var json = System.Text.Json.JsonSerializer.Serialize(
            payload,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
            });

        System.IO.File.WriteAllText(
            artifact.ValidationJsonPath,
            json,
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public static void WriteMarkdown(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifact artifact)
    {
        System.IO.Directory.CreateDirectory(artifact.OutputDirectory);

        var builder = new System.Text.StringBuilder();
        builder.AppendLine("# Post-Merge Smoke Validation");
        builder.AppendLine();
        builder.AppendLine($"ExitCode: {artifact.ExitCode}");
        builder.AppendLine();
        builder.AppendLine(artifact.MarkdownContent);

        System.IO.File.WriteAllText(
            artifact.ValidationMarkdownPath,
            builder.ToString().TrimEnd() + System.Environment.NewLine,
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
