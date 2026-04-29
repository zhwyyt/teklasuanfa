using System.IO;
using System.Text;
using System.Text.Json;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static void WriteJson(
        string path,
        DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifact artifact)
    {
        var json = JsonSerializer.Serialize(artifact, JsonOptions);
        File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public static void WriteMarkdown(
        string path,
        DefinitionClauseDecisionBridgeEffectAdapterFixtureRunResult runResult)
    {
        var markdown = DefinitionClauseDecisionBridgeEffectAdapterFixtureReportBuilder.BuildMarkdown(runResult);
        File.WriteAllText(path, markdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
