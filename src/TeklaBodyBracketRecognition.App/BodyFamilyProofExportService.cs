namespace TeklaBodyBracketRecognition.App;

internal static class BodyFamilyProofExportService
{
    private static readonly System.Text.UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static BodyFamilyProofExportResult Export(
        string outputDirectory,
        IReadOnlyList<BodyFamilyProofRow> rows)
    {
        Directory.CreateDirectory(outputDirectory);

        var artifact = BodyFamilyProofArtifactBuilder.Build(rows);
        var validation = BodyFamilyProofValidator.Validate(artifact);

        var jsonPath = Path.Combine(outputDirectory, "body-family-proof.json");
        var markdownPath = Path.Combine(outputDirectory, "body-family-proof.zh-CN.md");
        var validationJsonPath = Path.Combine(outputDirectory, "body-family-proof-validation.json");
        var validationMarkdownPath = Path.Combine(outputDirectory, "body-family-proof-validation.md");
        var manifestJsonPath = Path.Combine(outputDirectory, "body-family-proof-manifest.json");
        var readmePath = Path.Combine(outputDirectory, "body-family-proof-README.md");

        BodyFamilyProofArtifactSerializer.WriteJson(jsonPath, artifact);
        BodyFamilyProofArtifactSerializer.WriteMarkdown(markdownPath, artifact);
        BodyFamilyProofValidationSerializer.WriteJson(validationJsonPath, validation);
        BodyFamilyProofValidationSerializer.WriteMarkdown(validationMarkdownPath, validation);

        var manifest = new BodyFamilyProofManifest
        {
            JsonPath = jsonPath,
            MarkdownPath = markdownPath,
            ValidationJsonPath = validationJsonPath,
            ValidationMarkdownPath = validationMarkdownPath
        };

        BodyFamilyProofManifestSerializer.WriteJson(manifestJsonPath, manifest);
        File.WriteAllText(readmePath, BodyFamilyProofReadmeBuilder.BuildMarkdown(manifest), Utf8Bom);

        return new BodyFamilyProofExportResult
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
