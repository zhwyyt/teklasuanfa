using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionSemanticRegressionArtifactSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static DefinitionClauseDecisionSemanticRegressionWorkflowResult Write(
        DefinitionClauseDecisionSemanticRegressionArtifacts artifacts,
        string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.Combine(outputDirectory, "definition-clause-semantic-regression-snapshot.json");
        var markdownPath = Path.Combine(outputDirectory, "definition-clause-semantic-regression-snapshot.md");

        var json = JsonSerializer.Serialize(artifacts, JsonOptions);
        File.WriteAllText(jsonPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        File.WriteAllText(markdownPath, artifacts.MarkdownReport, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        return new DefinitionClauseDecisionSemanticRegressionWorkflowResult
        {
            OutputDirectory = outputDirectory,
            JsonPath = jsonPath,
            MarkdownPath = markdownPath,
            ManifestPath = string.Empty,
            ReadmePath = string.Empty,
            RowCount = artifacts.Rows.Count,
            BundleSectionJsonPath = string.Empty,
            BundleSectionMarkdownPath = string.Empty
        };
    }
}
