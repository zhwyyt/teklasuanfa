using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSectionMarkdownBuilder
{
    public static string BuildSummaryBlock(
        BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSection section)
    {
        var builder = new StringBuilder();
        builder.AppendLine("## Stage-2 Body Candidate Partition Validation");
        builder.AppendLine();
        builder.Append("总样本数：").AppendLine(section.TotalCount.ToString());
        builder.Append("通过数：").AppendLine(section.PassedCount.ToString());
        builder.Append("失败数：").AppendLine(section.FailedCount.ToString());
        builder.Append("Package 完整：").AppendLine(section.PackageIsComplete ? "是" : "否");
        builder.Append("Validation 成功：").AppendLine(section.ValidationSucceeded ? "是" : "否");
        builder.Append("Validation JSON：`").Append(section.ValidationJsonPath).AppendLine("`");
        builder.Append("Validation Markdown：`").Append(section.ValidationMarkdownPath).AppendLine("`");
        builder.AppendLine();
        builder.AppendLine("该 section 用于把阶段 2 `body-candidate partition conservative fixture` 的 package-audit/validation 结果收口成可被统一 summary 或 bundle 消费的稳定入口。");
        return builder.ToString();
    }
}
