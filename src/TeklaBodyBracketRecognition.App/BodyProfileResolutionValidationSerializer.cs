using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyProfileResolutionValidationSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    private static readonly UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static void WriteJson(string path, BodyProfileResolutionValidationResult result)
    {
        var json = JsonSerializer.Serialize(result, JsonOptions);
        File.WriteAllText(path, json, Utf8Bom);
    }

    public static void WriteMarkdown(string path, BodyProfileResolutionValidationResult result)
    {
        File.WriteAllText(path, BodyProfileResolutionValidationReportBuilder.BuildMarkdown(result), Utf8Bom);
    }
}
