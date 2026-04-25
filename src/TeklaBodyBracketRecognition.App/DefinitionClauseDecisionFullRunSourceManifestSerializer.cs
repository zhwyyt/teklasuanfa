using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceManifestSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    private static readonly UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static void WriteJson(string path, DefinitionClauseDecisionFullRunSourceManifest manifest)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(manifest, JsonOptions), Utf8Bom);
    }
}
