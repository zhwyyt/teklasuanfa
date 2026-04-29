using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyFamilyProofValidationReportBuilder
{
    public static string BuildMarkdown(BodyFamilyProofValidationResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Body Family Proof Validation");
        builder.AppendLine();
        builder.AppendLine($"- IsValid: `{result.IsValid}`");
        builder.AppendLine($"- AssemblyCount: `{result.AssemblyCount}`");
        builder.AppendLine($"- AdjudicatedCount: `{result.AdjudicatedCount}`");
        builder.AppendLine($"- DeferredCount: `{result.DeferredCount}`");
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
