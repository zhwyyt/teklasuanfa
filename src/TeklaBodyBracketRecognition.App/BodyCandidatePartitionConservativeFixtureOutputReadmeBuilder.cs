namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureOutputReadmeBuilder
{
    public static string Build(
        BodyCandidatePartitionConservativeFixtureManifest manifest)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("# Body Candidate Partition Conservative Fixture Output");
        builder.AppendLine();
        builder.AppendLine($"Total: {manifest.TotalCount}");
        builder.AppendLine($"Passed: {manifest.PassedCount}");
        builder.AppendLine($"Failed: {manifest.FailedCount}");
        builder.AppendLine();
        builder.AppendLine("Files:");
        builder.AppendLine();
        builder.AppendLine($"- JSON: `{manifest.JsonPath}`");
        builder.AppendLine($"- Markdown: `{manifest.MarkdownPath}`");
        builder.AppendLine($"- Manifest: `manifest.json`");
        builder.AppendLine($"- README: `README.md`");
        return builder.ToString().TrimEnd();
    }
}
