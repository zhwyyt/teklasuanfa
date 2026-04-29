namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceWorkflow
{
    public static DefinitionClauseDecisionFullRunSourceWorkflowResult Run(
        string outputDirectory,
        IReadOnlyList<DefinitionClauseDecisionFullRunAssemblySource> assemblies,
        IReadOnlyList<DefinitionClauseDecisionFullRunRepresentativePartSource> representativeParts)
    {
        var exportResult = DefinitionClauseDecisionFullRunSourceExportService.Export(
            outputDirectory,
            assemblies,
            representativeParts);

        return new DefinitionClauseDecisionFullRunSourceWorkflowResult
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
