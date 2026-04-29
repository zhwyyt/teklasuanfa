using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleReadmeBuilder
{
    public static string Build(
        BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleManifest manifest,
        string readmeMarkdown)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Stage-2 Body Candidate Partition Validation Bundle");
        builder.AppendLine();
        builder.Append("生成时间（UTC）：`").Append(manifest.GeneratedAtUtc.ToString("O")).AppendLine("`");
        builder.Append("输出目录：`").Append(manifest.OutputDirectory).AppendLine("`");
        builder.Append("Section 数：`").Append(manifest.Entries.Count).AppendLine("`");
        builder.AppendLine();
        builder.Append(readmeMarkdown.TrimEnd());
        builder.AppendLine();
        return builder.ToString();
    }
}
