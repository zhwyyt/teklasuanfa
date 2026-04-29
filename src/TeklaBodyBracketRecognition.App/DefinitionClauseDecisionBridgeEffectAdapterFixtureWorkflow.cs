using System.IO;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeEffectAdapterFixtureWorkflow
{
    public static DefinitionClauseDecisionBridgeEffectAdapterFixtureExportResult Export(
        string outputDirectory,
        DefinitionClauseDecisionBridgeEffectAdapterFixtureRunResult runResult)
    {
        Directory.CreateDirectory(outputDirectory);

        var artifact = DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactBuilder.Build(runResult);
        var jsonPath = Path.Combine(outputDirectory, "definition-clause-decision-bridge-effect-adapter-fixtures.json");
        var markdownPath = Path.Combine(outputDirectory, "definition-clause-decision-bridge-effect-adapter-fixtures.md");

        DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactSerializer.WriteJson(jsonPath, artifact);
        DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactSerializer.WriteMarkdown(markdownPath, runResult);

        return new DefinitionClauseDecisionBridgeEffectAdapterFixtureExportResult
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath,
        };
    }
}
