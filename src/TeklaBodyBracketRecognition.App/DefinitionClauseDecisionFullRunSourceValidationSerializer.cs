using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceValidationSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    private static readonly UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static void WriteJson(string path, DefinitionClauseDecisionFullRunSourceValidationResult result)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(result, JsonOptions), Utf8Bom);
    }

    public static void WriteMarkdown(string path, DefinitionClauseDecisionFullRunSourceValidationResult result)
    {
        File.WriteAllText(path, DefinitionClauseDecisionFullRunSourceValidationReportBuilder.BuildMarkdown(result), Utf8Bom);
    }
}
