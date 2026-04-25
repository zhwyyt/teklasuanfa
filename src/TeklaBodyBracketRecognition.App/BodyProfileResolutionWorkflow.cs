namespace TeklaBodyBracketRecognition.App;

internal static class BodyProfileResolutionWorkflow
{
    public static BodyProfileResolutionWorkflowResult Run(
        string outputDirectory,
        IReadOnlyList<BodyProfileResolutionRow> rows)
    {
        var exportResult = BodyProfileResolutionExportService.Export(outputDirectory, rows);

        return new BodyProfileResolutionWorkflowResult
        {
            JsonPath = exportResult.JsonPath,
            MarkdownPath = exportResult.MarkdownPath,
            ValidationJsonPath = exportResult.ValidationJsonPath,
            ValidationMarkdownPath = exportResult.ValidationMarkdownPath,
            ManifestJsonPath = exportResult.ManifestJsonPath,
            ReadmePath = exportResult.ReadmePath
        };
    }
}
