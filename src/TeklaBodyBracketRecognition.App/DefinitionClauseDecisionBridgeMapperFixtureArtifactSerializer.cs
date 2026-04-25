using System.IO;
using System.Text;
using System.Text.Json;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeMapperFixtureArtifactSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static void WriteJson(
        string path,
        DefinitionClauseDecisionBridgeMapperFixtureArtifact artifact)
    {
        var json = JsonSerializer.Serialize(artifact, JsonOptions);
        File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public static void WriteMarkdown(
        string path,
        DefinitionClauseDecisionBridgeMapperFixtureRunResult runResult)
    {
        var markdown = DefinitionClauseDecisionBridgeMapperFixtureReportBuilder.BuildMarkdown(runResult);
        File.WriteAllText(path, markdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
