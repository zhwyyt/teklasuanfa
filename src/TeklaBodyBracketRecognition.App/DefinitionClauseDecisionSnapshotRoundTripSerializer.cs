using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionSnapshotRoundTripSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string JsonFileName => "definition-clause-decision-snapshot-roundtrip.json";
    public static string MarkdownFileName => "definition-clause-decision-snapshot-roundtrip.md";

    public static string WriteJson(string outputDirectory, DefinitionClauseDecisionSnapshotRoundTripResult result)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, JsonFileName);
        var utf8Bom = new UTF8Encoding(true);
        var json = JsonSerializer.Serialize(result ?? new DefinitionClauseDecisionSnapshotRoundTripResult(), JsonOptions);
        File.WriteAllText(path, json, utf8Bom);
        return path;
    }

    public static string WriteMarkdown(string outputDirectory, DefinitionClauseDecisionSnapshotRoundTripResult result)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, MarkdownFileName);
        var utf8Bom = new UTF8Encoding(true);
        var markdown = DefinitionClauseDecisionSnapshotRoundTripReportBuilder.BuildMarkdown(result);
        File.WriteAllText(path, markdown, utf8Bom);
        return path;
    }
}
