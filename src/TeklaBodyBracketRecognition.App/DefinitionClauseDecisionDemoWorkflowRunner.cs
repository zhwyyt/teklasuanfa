using System.IO;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoWorkflowRunner
{
    public static DefinitionClauseDecisionDemoWorkflowResult Run(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        var extractor = new DefinitionClauseDecisionDemoSnapshotExtractor();
        var snapshotResult = DefinitionClauseDecisionSnapshotPipeline.Export<object?>(null, extractor, outputDirectory);
        var validationBundle =
            DefinitionClauseDecisionSidecarWorkflow.WriteDefaultFixtureArtifactsWithValidationBundle(outputDirectory);

        return new DefinitionClauseDecisionDemoWorkflowResult
        {
            Artifacts = snapshotResult.Artifacts,
            Manifest = snapshotResult.Manifest,
            ValidationBundle = validationBundle
        };
    }
}
