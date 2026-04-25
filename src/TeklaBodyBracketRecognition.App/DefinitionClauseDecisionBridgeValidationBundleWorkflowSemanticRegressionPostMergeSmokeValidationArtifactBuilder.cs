namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactBuilder
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifacts Build(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationResult validationResult)
    {
        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifacts
        {
            Succeeded = validationResult.Succeeded,
            Summary = validationResult.Summary,
            MissingPaths = validationResult.MissingPaths
        };
    }
}
