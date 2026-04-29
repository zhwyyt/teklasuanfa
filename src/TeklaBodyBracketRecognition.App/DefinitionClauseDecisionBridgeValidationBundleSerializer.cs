using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static void WriteManifest(
        string path,
        DefinitionClauseDecisionBridgeValidationBundleManifest manifest)
    {
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public static void WriteMarkdown(
        string path,
        string markdown)
    {
        File.WriteAllText(path, markdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
