using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoManifestSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FileName => "definition-clause-decision-demo-manifest.json";

    public static string ToJson(DefinitionClauseDecisionSidecarManifest manifest)
    {
        manifest ??= new DefinitionClauseDecisionSidecarManifest();
        return JsonSerializer.Serialize(manifest, JsonOptions);
    }

    public static string Write(string outputDirectory, DefinitionClauseDecisionSidecarManifest manifest)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, FileName);
        var utf8Bom = new UTF8Encoding(true);
        File.WriteAllText(path, ToJson(manifest), utf8Bom);
        return path;
    }
}
