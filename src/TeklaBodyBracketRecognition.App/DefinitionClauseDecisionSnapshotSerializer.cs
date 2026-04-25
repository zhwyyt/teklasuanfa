using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionSnapshotSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FileName => "definition-clause-decision-snapshot.json";

    public static string ToJson(DefinitionClauseDecisionSnapshot snapshot)
    {
        snapshot ??= new DefinitionClauseDecisionSnapshot();
        return JsonSerializer.Serialize(snapshot, JsonOptions);
    }

    public static string Write(string outputDirectory, DefinitionClauseDecisionSnapshot snapshot)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, FileName);
        var utf8Bom = new UTF8Encoding(true);
        File.WriteAllText(path, ToJson(snapshot), utf8Bom);
        return path;
    }
}
