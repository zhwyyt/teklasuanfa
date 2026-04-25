namespace TeklaBodyBracketRecognition.App;

internal static class BodyProfileResolutionExportService
{
    private static readonly System.Text.UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static BodyProfileResolutionExportResult Export(
        string outputDirectory,
        IReadOnlyList<BodyProfileResolutionRow> rows)
    {
        Directory.CreateDirectory(outputDirectory);

        var artifact = BodyProfileResolutionArtifactBuilder.Build(rows);
        var validation = BodyProfileResolutionValidator.Validate(artifact);

        var jsonPath = Path.Combine(outputDirectory, "body-profile-resolution.json");
        var markdownPath = Path.Combine(outputDirectory, "body-profile-resolution.zh-CN.md");
        var validationJsonPath = Path.Combine(outputDirectory, "body-profile-resolution-validation.json");
        var validationMarkdownPath = Path.Combine(outputDirectory, "body-profile-resolution-validation.md");
        var manifestJsonPath = Path.Combine(outputDirectory, "body-profile-resolution-manifest.json");
        var readmePath = Path.Combine(outputDirectory, "body-profile-resolution-README.md");

        BodyProfileResolutionArtifactSerializer.WriteJson(jsonPath, artifact);
        BodyProfileResolutionArtifactSerializer.WriteMarkdown(markdownPath, artifact);
        BodyProfileResolutionValidationSerializer.WriteJson(validationJsonPath, validation);
        BodyProfileResolutionValidationSerializer.WriteMarkdown(validationMarkdownPath, validation);

        var manifest = new BodyProfileResolutionManifest
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath,
            ValidationJsonPath = validationJsonPath,
            ValidationMarkdownPath = validationMarkdownPath
        };

        BodyProfileResolutionManifestSerializer.WriteJson(manifestJsonPath, manifest);
        File.WriteAllText(readmePath, BodyProfileResolutionReadmeBuilder.BuildMarkdown(manifest), Utf8Bom);

        return new BodyProfileResolutionExportResult
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
