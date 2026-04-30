using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class NewInputLayerDraftSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static void WriteJson(string path, NewInputLayerDraftArtifact artifact)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(artifact, JsonOptions), Utf8Bom);
    }

    public static void WriteMarkdown(string path, NewInputLayerDraftArtifact artifact)
    {
        File.WriteAllText(path, NewInputLayerDraftArtifactBuilder.BuildMarkdown(artifact), Utf8Bom);
    }
}
