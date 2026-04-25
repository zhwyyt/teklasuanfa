using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyFamilyProofManifestSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    private static readonly UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static void WriteJson(string path, BodyFamilyProofManifest manifest)
    {
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        File.WriteAllText(path, json, Utf8Bom);
    }
}
