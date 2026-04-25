namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceExportService
{
    private static readonly System.Text.UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static DefinitionClauseDecisionFullRunSourceExportResult Export(
        string outputDirectory,
        IReadOnlyList<DefinitionClauseDecisionFullRunAssemblySource> assemblies,
        IReadOnlyList<DefinitionClauseDecisionFullRunRepresentativePartSource> representativeParts)
    {
        Directory.CreateDirectory(outputDirectory);

        var artifact = DefinitionClauseDecisionFullRunSourceArtifactBuilder.Build(assemblies, representativeParts);
        var validation = DefinitionClauseDecisionFullRunSourceValidator.Validate(artifact);

        var jsonPath = Path.Combine(outputDirectory, "definition-clause-decision-fullrun-source.json");
        var markdownPath = Path.Combine(outputDirectory, "definition-clause-decision-fullrun-source.zh-CN.md");
        var validationJsonPath = Path.Combine(outputDirectory, "definition-clause-decision-fullrun-source-validation.json");
        var validationMarkdownPath = Path.Combine(outputDirectory, "definition-clause-decision-fullrun-source-validation.md");
        var manifestJsonPath = Path.Combine(outputDirectory, "definition-clause-decision-fullrun-source-manifest.json");
        var readmePath = Path.Combine(outputDirectory, "README.md");

        DefinitionClauseDecisionFullRunSourceArtifactSerializer.WriteJson(jsonPath, artifact);
        DefinitionClauseDecisionFullRunSourceArtifactSerializer.WriteMarkdown(markdownPath, artifact);
        DefinitionClauseDecisionFullRunSourceValidationSerializer.WriteJson(validationJsonPath, validation);
        DefinitionClauseDecisionFullRunSourceValidationSerializer.WriteMarkdown(validationMarkdownPath, validation);

        var manifest = new DefinitionClauseDecisionFullRunSourceManifest
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath,
            ValidationJsonPath = validationJsonPath,
            ValidationMarkdownPath = validationMarkdownPath
        };

        DefinitionClauseDecisionFullRunSourceManifestSerializer.WriteJson(manifestJsonPath, manifest);
        File.WriteAllText(readmePath, DefinitionClauseDecisionFullRunSourceReadmeBuilder.BuildMarkdown(manifest), Utf8Bom);

        return new DefinitionClauseDecisionFullRunSourceExportResult
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath,
            ValidationJsonPath = validationJsonPath,
            ValidationMarkdownPath = validationMarkdownPath,
            ManifestJsonPath = manifestJsonPath,
            ReadmePath = readmePath
        };
    }
}
