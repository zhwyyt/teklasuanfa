using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactWriteResult
{
    public required string JsonPath { get; init; }

    public required string MarkdownPath { get; init; }
}

internal static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactWriteResult Write(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifacts artifacts,
        string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.Combine(outputDirectory, "validation.json");
        var markdownPath = Path.Combine(outputDirectory, "validation.md");
        var json = JsonSerializer.Serialize(artifacts, JsonOptions);
        File.WriteAllText(jsonPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        File.WriteAllText(markdownPath, BuildMarkdown(artifacts), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactWriteResult
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath
        };
    }

    private static string BuildMarkdown(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifacts artifacts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Smoke Validation");
        builder.AppendLine();
        builder.Append("Succeeded: ").AppendLine(artifacts.Succeeded ? "true" : "false");
        builder.AppendLine();
        builder.AppendLine(artifacts.Summary);
        builder.AppendLine();

        if (artifacts.MissingPaths.Count > 0)
        {
            builder.AppendLine("## Missing Paths");
            builder.AppendLine();

            foreach (var path in artifacts.MissingPaths)
            {
                builder.Append("- `").Append(path).AppendLine("`");
            }
        }

        return builder.ToString();
    }
}
