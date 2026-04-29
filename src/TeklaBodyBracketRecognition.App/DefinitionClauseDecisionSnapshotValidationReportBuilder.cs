using System.Text;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionSnapshotValidationReportBuilder
{
    public static string BuildMarkdown(DefinitionClauseDecisionSnapshotValidationResult result)
    {
        result ??= new DefinitionClauseDecisionSnapshotValidationResult();

        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClauseDecision Snapshot Validation");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("| --- | --- |");
        builder.AppendLine($"| MemberCount | {result.MemberCount} |");
        builder.AppendLine($"| PartCount | {result.PartCount} |");
        builder.AppendLine($"| MissingMemberIdCount | {result.MissingMemberIdCount} |");
        builder.AppendLine($"| MissingAssemblyIdCount | {result.MissingAssemblyIdCount} |");
        builder.AppendLine($"| MissingPartIdCount | {result.MissingPartIdCount} |");
        builder.AppendLine($"| MissingPartNameCount | {result.MissingPartNameCount} |");
        builder.AppendLine($"| MissingDefinitionClauseCodeCount | {result.MissingDefinitionClauseCodeCount} |");
        builder.AppendLine($"| MissingClauseVerdictCodeCount | {result.MissingClauseVerdictCodeCount} |");
        builder.AppendLine($"| MissingClausePromotionReadinessCodeCount | {result.MissingClausePromotionReadinessCodeCount} |");
        builder.AppendLine();
        builder.AppendLine($"IsStructurallyValid: {(result.IsStructurallyValid ? "Yes" : "No")}");
        return builder.ToString().TrimEnd();
    }
}
