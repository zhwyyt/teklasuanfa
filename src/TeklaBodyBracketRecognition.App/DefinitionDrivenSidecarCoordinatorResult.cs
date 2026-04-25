namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionDrivenSidecarCoordinatorResult
{
    public required DefinitionClauseDecisionFullRunSourceWorkflowResult DefinitionClauseDecisionFullRunSource { get; init; }

    public required BodyFamilyProofWorkflowResult BodyFamilyProof { get; init; }

    public required BodyProfileResolutionWorkflowResult BodyProfileResolution { get; init; }
}
