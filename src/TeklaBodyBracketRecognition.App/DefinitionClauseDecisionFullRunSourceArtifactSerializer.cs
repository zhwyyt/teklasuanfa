using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceArtifactSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    private static readonly UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static void WriteJson(string path, DefinitionClauseDecisionFullRunSourceArtifact artifact)
    {
        var json = JsonSerializer.Serialize(artifact, JsonOptions);
        File.WriteAllText(path, json, Utf8Bom);
    }

    public static void WriteMarkdown(string path, DefinitionClauseDecisionFullRunSourceArtifact artifact)
    {
        var markdown = DefinitionClauseDecisionFullRunSourceArtifactBuilder.BuildMarkdown(artifact);
        File.WriteAllText(path, markdown, Utf8Bom);
    }
}
