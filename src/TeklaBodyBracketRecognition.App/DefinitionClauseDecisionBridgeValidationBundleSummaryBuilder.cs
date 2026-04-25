using System.Text;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleSummaryBuilder
{
    public static string BuildMarkdown(
        DefinitionClauseDecisionBridgeMapperFixtureRunResult mapperRunResult,
        DefinitionClauseDecisionBridgeEffectAdapterFixtureRunResult effectAdapterRunResult,
        DefinitionClauseDecisionBridgeValidationBundleManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClause Bridge Validation Bundle");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- GeneratedAtUtc: `{manifest.GeneratedAtUtc:O}`");
        builder.AppendLine($"- MapperFixtures: `{mapperRunResult.MatchedCount}/{mapperRunResult.TotalCount}` matched");
        builder.AppendLine($"- EffectAdapterFixtures: `{effectAdapterRunResult.MatchedCount}/{effectAdapterRunResult.TotalCount}` matched");
        builder.AppendLine($"- OverallSucceeded: `{mapperRunResult.Succeeded && effectAdapterRunResult.Succeeded}`");
        builder.AppendLine();
        builder.AppendLine("## Artifacts");
        builder.AppendLine();
        builder.AppendLine($"- Mapper JSON: `{manifest.MapperFixtureJsonPath}`");
        builder.AppendLine($"- Mapper Markdown: `{manifest.MapperFixtureMarkdownPath}`");
        builder.AppendLine($"- EffectAdapter JSON: `{manifest.EffectAdapterFixtureJsonPath}`");
        builder.AppendLine($"- EffectAdapter Markdown: `{manifest.EffectAdapterFixtureMarkdownPath}`");
        builder.AppendLine($"- Summary Markdown: `{manifest.SummaryMarkdownPath}`");
        builder.AppendLine($"- Manifest JSON: `{PathCombine(manifest.OutputDirectory, "definition-clause-decision-bridge-validation-bundle-manifest.json")}`");
        builder.AppendLine();
        builder.AppendLine("## Next Step");
        builder.AppendLine();
        builder.AppendLine("- Merge this bundle workflow into the legacy `DefinitionClauseDecisionBridge` fixture/sidecar export chain.");
        builder.AppendLine("- Replace duplicated entry points so mapper fixtures and effect-adapter fixtures stop drifting independently.");
        return builder.ToString();
    }

    private static string PathCombine(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left))
        {
            return right;
        }

        if (left.EndsWith("\\") || left.EndsWith("/"))
        {
            return left + right;
        }

        return left + "\\" + right;
    }
}
