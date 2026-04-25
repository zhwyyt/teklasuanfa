namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionDrivenSidecarCoordinator
{
    public static DefinitionDrivenSidecarCoordinatorResult Run(
        string outputDirectory,
        BatchArtifacts artifacts)
    {
        var definitionClauseDecisionAssemblies =
            DefinitionClauseDecisionFullRunSourceCollector.CollectAssemblies(
                artifacts.BodyMaterialSummaries,
                artifacts.RealInputCoreBodyProof);
        var definitionClauseDecisionRepresentativeParts =
            DefinitionClauseDecisionFullRunSourceCollector.CollectRepresentativeParts(
                artifacts.BodyMaterialSummaries,
                artifacts.RealInputCoreBodyProof);
        var bodyFamilyProofRows =
            BodyFamilyDefinitionEvaluator.Evaluate(
                definitionClauseDecisionAssemblies);
        var bodyProfileResolutionRows =
            BodyProfileResolver.Resolve(
                artifacts.BodyMaterialSummaries,
                bodyFamilyProofRows);

        return new DefinitionDrivenSidecarCoordinatorResult
        {
            DefinitionClauseDecisionFullRunSource = DefinitionClauseDecisionFullRunSourceWorkflow.Run(
                outputDirectory,
                definitionClauseDecisionAssemblies,
                definitionClauseDecisionRepresentativeParts),
            BodyFamilyProof = BodyFamilyProofWorkflow.Run(
                outputDirectory,
                bodyFamilyProofRows),
            BodyProfileResolution = BodyProfileResolutionWorkflow.Run(
                outputDirectory,
                bodyProfileResolutionRows)
        };
    }
}
