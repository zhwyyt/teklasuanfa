using System.IO;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixtureStage2OutputArtifactWriter
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string WriteMarkdown(
        string outputDirectory,
        string fileName,
        string markdown)
    {
        Directory.CreateDirectory(outputDirectory);

        var path = Path.Combine(outputDirectory, fileName);
        File.WriteAllText(path, markdown, Utf8WithBom);
        return path;
    }

    public static string WriteManifest<TManifest>(
        TManifest manifest,
        string outputDirectory,
        string fileName = "manifest.json")
    {
        Directory.CreateDirectory(outputDirectory);

        var path = Path.Combine(outputDirectory, fileName);
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        File.WriteAllText(path, json, Utf8WithBom);
        return path;
    }
}
