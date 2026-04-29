using System.Text;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleReadmeBuilder
{
    public static string BuildMarkdown(DefinitionClauseDecisionBridgeValidationBundleManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClause Bridge Validation Bundle");
        builder.AppendLine();
        builder.AppendLine("This directory contains the current bridge-layer validation artifacts for the new:");
        builder.AppendLine();
        builder.AppendLine("- mapper fixture chain");
        builder.AppendLine("- effect-adapter fixture chain");
        builder.AppendLine();
        builder.AppendLine("Use this bundle as the integration target when merging the new bridge validation path back into the legacy fixture and sidecar export flow.");
        builder.AppendLine();
        builder.AppendLine("## Files");
        builder.AppendLine();
        builder.AppendLine($"- `{manifest.MapperFixtureJsonPath}`");
        builder.AppendLine($"- `{manifest.MapperFixtureMarkdownPath}`");
        builder.AppendLine($"- `{manifest.EffectAdapterFixtureJsonPath}`");
        builder.AppendLine($"- `{manifest.EffectAdapterFixtureMarkdownPath}`");
        builder.AppendLine($"- `{manifest.SummaryMarkdownPath}`");
        builder.AppendLine($"- `definition-clause-decision-bridge-validation-bundle-manifest.json`");
        return builder.ToString();
    }
}
