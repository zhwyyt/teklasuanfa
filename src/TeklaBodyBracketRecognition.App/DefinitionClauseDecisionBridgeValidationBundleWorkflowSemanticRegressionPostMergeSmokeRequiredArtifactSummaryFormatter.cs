namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSummaryFormatter
{
    public static string BuildConsoleMessage(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSnapshot snapshot)
    {
        if (snapshot.AllRequiredArtifactsPresent)
        {
            return "Post-merge smoke required artifacts are complete.";
        }

        return $"Post-merge smoke is missing required artifacts: {string.Join(", ", snapshot.MissingArtifacts)}";
    }

    public static string BuildMarkdownBlock(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactSnapshot snapshot)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("## Required Artifacts");
        builder.AppendLine();

        if (snapshot.AllRequiredArtifactsPresent)
        {
            builder.AppendLine("Status: complete");
            builder.AppendLine();
            builder.AppendLine("All shared required artifacts are present.");
            return builder.ToString().TrimEnd();
        }

        builder.AppendLine("Status: incomplete");
        builder.AppendLine();
        builder.AppendLine("Missing artifacts:");

        foreach (var artifact in snapshot.MissingArtifacts)
        {
            builder.AppendLine($"- {artifact}");
        }

        return builder.ToString().TrimEnd();
    }
}
