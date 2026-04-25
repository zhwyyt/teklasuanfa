namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionDemoWorkflowResult
{
    public DefinitionClauseDecisionArtifacts Artifacts { get; init; } = new();
    public DefinitionClauseDecisionSidecarManifest Manifest { get; init; } = new();
    public DefinitionClauseDecisionBridgeValidationBundleWorkflowResult ValidationBundle { get; init; } = new();
}
