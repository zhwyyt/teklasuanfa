using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationResult
{
    public required bool Succeeded { get; init; }

    public required IReadOnlyList<string> MissingPaths { get; init; }

    public required string Summary { get; init; }
}
