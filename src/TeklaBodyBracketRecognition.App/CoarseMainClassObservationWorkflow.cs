namespace TeklaBodyBracketRecognition.App;

internal static class CoarseMainClassObservationWorkflow
{
    public static CoarseMainClassObservationWorkflowResult Run(
        string outputDirectory,
        IReadOnlyList<CoarseMainClassObservationRow> rows)
    {
        Directory.CreateDirectory(outputDirectory);

        var artifact = CoarseMainClassObservationArtifactBuilder.Build(rows);
        var jsonPath = Path.Combine(outputDirectory, "coarse-main-class-observation.json");
        var markdownPath = Path.Combine(outputDirectory, "coarse-main-class-observation.zh-CN.md");

        CoarseMainClassObservationSerializer.WriteJson(jsonPath, artifact);
        CoarseMainClassObservationSerializer.WriteMarkdown(markdownPath, artifact);

        return new CoarseMainClassObservationWorkflowResult
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath
        };
    }
}
