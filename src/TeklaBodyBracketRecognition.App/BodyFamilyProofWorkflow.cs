namespace TeklaBodyBracketRecognition.App;

internal static class BodyFamilyProofWorkflow
{
    public static BodyFamilyProofWorkflowResult Run(
        string outputDirectory,
        IReadOnlyList<BodyFamilyProofRow> rows)
    {
        var exportResult = BodyFamilyProofExportService.Export(outputDirectory, rows);

        return new BodyFamilyProofWorkflowResult
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
