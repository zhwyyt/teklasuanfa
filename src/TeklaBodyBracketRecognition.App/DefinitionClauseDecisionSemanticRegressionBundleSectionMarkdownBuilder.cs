using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionSemanticRegressionBundleSectionMarkdownBuilder
{
    public static string BuildSummaryBlock(DefinitionClauseDecisionSemanticRegressionBundleSection section)
    {
        var builder = new StringBuilder();
        builder.AppendLine("## Semantic Regression");
        builder.AppendLine();
        builder.Append("样本行数：").AppendLine(section.RowCount.ToString());
        builder.Append("JSON：`").Append(section.JsonPath).AppendLine("`");
        builder.Append("Markdown：`").Append(section.MarkdownPath).AppendLine("`");
        builder.Append("Manifest：`").Append(section.ManifestPath).AppendLine("`");
        builder.Append("README：`").Append(section.ReadmePath).AppendLine("`");
        builder.AppendLine();
        builder.AppendLine("该 section 用于固化 `SYNTHETIC_NONE_TOPOLOGY / SYNTHETIC_TARGET_ONLY_REVIEW` 等语义回归样本，后续应并入统一 validation bundle。");
        return builder.ToString();
    }
}
