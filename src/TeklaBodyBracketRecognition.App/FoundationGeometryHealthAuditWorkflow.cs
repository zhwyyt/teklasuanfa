namespace TeklaBodyBracketRecognition.App;

internal static class FoundationGeometryHealthAuditWorkflow
{
    public static FoundationGeometryHealthAuditWorkflowResult Run(
        string outputDirectory,
        IReadOnlyList<FoundationGeometryHealthAuditMemberRow> rows)
    {
        Directory.CreateDirectory(outputDirectory);

        var artifact = FoundationGeometryHealthAuditArtifactBuilder.Build(outputDirectory, rows);
        var jsonPath = Path.Combine(outputDirectory, "foundation-geometry-health-audit.json");
        var markdownPath = Path.Combine(outputDirectory, "foundation-geometry-health-audit.zh-CN.md");

        FoundationGeometryHealthAuditSerializer.WriteJson(jsonPath, artifact);
        FoundationGeometryHealthAuditSerializer.WriteMarkdown(markdownPath, artifact);

        return new FoundationGeometryHealthAuditWorkflowResult
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath
        };
    }
}
