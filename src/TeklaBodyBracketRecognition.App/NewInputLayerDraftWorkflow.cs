namespace TeklaBodyBracketRecognition.App;

internal static class NewInputLayerDraftWorkflow
{
    public static NewInputLayerDraftWorkflowResult Run(
        string outputDirectory,
        IReadOnlyList<NewInputLayerDraftMemberRow> members)
    {
        Directory.CreateDirectory(outputDirectory);

        var artifact = NewInputLayerDraftArtifactBuilder.Build(outputDirectory, members);
        var jsonPath = Path.Combine(outputDirectory, "new-input-layer-draft.json");
        var markdownPath = Path.Combine(outputDirectory, "new-input-layer-draft.zh-CN.md");

        NewInputLayerDraftSerializer.WriteJson(jsonPath, artifact);
        NewInputLayerDraftSerializer.WriteMarkdown(markdownPath, artifact);

        return new NewInputLayerDraftWorkflowResult
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath
        };
    }
}
