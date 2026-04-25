using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceValidationReportBuilder
{
    public static string BuildMarkdown(DefinitionClauseDecisionFullRunSourceValidationResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClauseDecision Full-Run Source Validation");
        builder.AppendLine();
        builder.AppendLine($"- IsValid: `{result.IsValid}`");
        builder.AppendLine($"- AssemblyCount: `{result.AssemblyCount}`");
        builder.AppendLine($"- RepresentativePartCount: `{result.RepresentativePartCount}`");
        builder.AppendLine($"- AssembliesWithTopologyRewrite: `{result.AssembliesWithTopologyRewrite}`");
        builder.AppendLine($"- CoreRepresentativePartCount: `{result.CoreRepresentativePartCount}`");
        builder.AppendLine($"- ReviewRepresentativePartCount: `{result.ReviewRepresentativePartCount}`");
        builder.AppendLine();
        builder.AppendLine("## Issues");
        builder.AppendLine();

        if (result.Issues.Count == 0)
        {
            builder.AppendLine("- 无");
        }
        else
        {
            foreach (var issue in result.Issues)
            {
                builder.Append("- ").AppendLine(issue);
            }
        }

        return builder.ToString();
    }
}
