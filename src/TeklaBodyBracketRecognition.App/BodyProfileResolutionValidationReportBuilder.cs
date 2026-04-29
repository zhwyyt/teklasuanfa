using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyProfileResolutionValidationReportBuilder
{
    public static string BuildMarkdown(BodyProfileResolutionValidationResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Body Profile Resolution Validation");
        builder.AppendLine();
        builder.AppendLine($"- IsValid: `{result.IsValid}`");
        builder.AppendLine($"- AssemblyCount: `{result.AssemblyCount}`");
        builder.AppendLine($"- ResolvedCount: `{result.ResolvedCount}`");
        builder.AppendLine($"- ReviewBypassCount: `{result.ReviewBypassCount}`");
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
