using System.IO;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeMapperFixtureWorkflow
{
    public static DefinitionClauseDecisionBridgeMapperFixtureExportResult Export(
        string outputDirectory,
        DefinitionClauseDecisionBridgeMapperFixtureRunResult runResult)
    {
        Directory.CreateDirectory(outputDirectory);

        var artifact = DefinitionClauseDecisionBridgeMapperFixtureArtifactBuilder.Build(runResult);
        var jsonPath = Path.Combine(outputDirectory, "definition-clause-decision-bridge-mapper-fixtures.json");
        var markdownPath = Path.Combine(outputDirectory, "definition-clause-decision-bridge-mapper-fixtures.md");

        DefinitionClauseDecisionBridgeMapperFixtureArtifactSerializer.WriteJson(jsonPath, artifact);
        DefinitionClauseDecisionBridgeMapperFixtureArtifactSerializer.WriteMarkdown(markdownPath, runResult);

        return new DefinitionClauseDecisionBridgeMapperFixtureExportResult
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath,
        };
    }
}
