using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoOutputValidationSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string JsonFileName => "definition-clause-decision-demo-validation.json";
    public static string MarkdownFileName => DefinitionClauseDecisionDemoOutputValidationReportBuilder.FileName;

    public static string WriteJson(string outputDirectory, DefinitionClauseDecisionDemoOutputValidationResult result)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, JsonFileName);
        var utf8Bom = new UTF8Encoding(true);
        var json = JsonSerializer.Serialize(result ?? new DefinitionClauseDecisionDemoOutputValidationResult(), JsonOptions);
        File.WriteAllText(path, json, utf8Bom);
        return path;
    }

    public static string WriteMarkdown(string outputDirectory, DefinitionClauseDecisionDemoOutputValidationResult result)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, MarkdownFileName);
        var utf8Bom = new UTF8Encoding(true);
        var markdown = DefinitionClauseDecisionDemoOutputValidationReportBuilder.BuildMarkdown(result);
        File.WriteAllText(path, markdown, utf8Bom);
        return path;
    }
}
