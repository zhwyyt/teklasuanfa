using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionFixtureSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string ToJson(DefinitionClauseDecisionFixtureArtifacts artifacts)
    {
        artifacts ??= new DefinitionClauseDecisionFixtureArtifacts();
        return JsonSerializer.Serialize(artifacts, JsonOptions);
    }

    public static void WriteJson(string outputJsonPath, DefinitionClauseDecisionFixtureArtifacts artifacts)
    {
        var directory = Path.GetDirectoryName(outputJsonPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var utf8Bom = new UTF8Encoding(true);
        File.WriteAllText(outputJsonPath, ToJson(artifacts), utf8Bom);
    }
}
