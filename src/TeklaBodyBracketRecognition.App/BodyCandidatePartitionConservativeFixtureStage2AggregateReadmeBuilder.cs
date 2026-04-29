using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixtureStage2AggregateReadmeBuilder
{
    public static string Build(
        BodyCandidatePartitionConservativeFixtureStage2AggregateManifest manifest,
        string summaryMarkdown)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Stage-2 Body Candidate Partition Aggregate");
        builder.AppendLine();
        builder.Append("生成时间（UTC）：`").Append(manifest.GeneratedAtUtc.ToString("O")).AppendLine("`");
        builder.Append("输出目录：`").Append(manifest.OutputDirectory).AppendLine("`");
        builder.Append("总样本数：`").Append(manifest.TotalCount).AppendLine("`");
        builder.Append("通过数：`").Append(manifest.PassedCount).AppendLine("`");
        builder.Append("失败数：`").Append(manifest.FailedCount).AppendLine("`");
        builder.Append("Package 完整：`").Append(manifest.PackageIsComplete).AppendLine("`");
        builder.Append("Validation 成功：`").Append(manifest.ValidationSucceeded).AppendLine("`");
        builder.AppendLine();
        builder.Append(summaryMarkdown.TrimEnd());
        builder.AppendLine();
        return builder.ToString();
    }
}
