using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionSemanticRegressionBundleSectionSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string WriteJson(
        DefinitionClauseDecisionSemanticRegressionBundleSection section,
        string outputDirectory)
    {
        var path = Path.Combine(outputDirectory, "bundle-section.json");
        var json = JsonSerializer.Serialize(section, JsonOptions);
        File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        return path;
    }

    public static string WriteMarkdownSummary(
        DefinitionClauseDecisionSemanticRegressionBundleSection section,
        string outputDirectory)
    {
        var path = Path.Combine(outputDirectory, "bundle-section-summary.md");
        var markdown = DefinitionClauseDecisionSemanticRegressionBundleSectionMarkdownBuilder.BuildSummaryBlock(section);
        File.WriteAllText(path, markdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        return path;
    }
}
