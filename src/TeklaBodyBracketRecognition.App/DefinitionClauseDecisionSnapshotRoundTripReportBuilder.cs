using System.Text;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionSnapshotRoundTripReportBuilder
{
    public static string BuildMarkdown(DefinitionClauseDecisionSnapshotRoundTripResult result)
    {
        result ??= new DefinitionClauseDecisionSnapshotRoundTripResult();

        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClauseDecision Snapshot Round-Trip 报告");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("| --- | --- |");
        builder.AppendLine($"| MemberCount | {result.MemberCount} |");
        builder.AppendLine($"| PartCount | {result.PartCount} |");
        builder.AppendLine($"| SourceRowCount | {result.SourceRowCount} |");
        builder.AppendLine($"| RepresentativeRowCount | {result.RepresentativeRowCount} |");
        builder.AppendLine($"| AggregateRowCount | {result.AggregateRowCount} |");
        builder.AppendLine($"| ReviewRowCount | {result.ReviewRowCount} |");
        builder.AppendLine();
        builder.AppendLine("这份报告用于快速检查：snapshot -> source rows -> representative rows -> aggregate rows -> review rows 这条链是否闭合。");
        return builder.ToString().TrimEnd();
    }
}
