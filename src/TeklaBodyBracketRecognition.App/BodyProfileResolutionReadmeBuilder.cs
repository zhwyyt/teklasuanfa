using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyProfileResolutionReadmeBuilder
{
    public static string BuildMarkdown(BodyProfileResolutionManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Body Profile Resolution Output");
        builder.AppendLine();
        builder.AppendLine("本目录保存阶段 7 首版 `body-profile-resolution` sidecar。");
        builder.AppendLine();
        builder.AppendLine("## 文件说明");
        builder.AppendLine();
        builder.AppendLine("- `body-profile-resolution.json`：构件级细分结果。");
        builder.AppendLine("- `body-profile-resolution.zh-CN.md`：面向人工 review 的中文摘要。");
        builder.AppendLine("- `body-profile-resolution-validation.json`：结构化自校验结果。");
        builder.AppendLine("- `body-profile-resolution-validation.md`：校验摘要。");
        builder.AppendLine("- `body-profile-resolution-manifest.json`：当前导出文件路径清单。");
        builder.AppendLine("- `body-profile-resolution-README.md`：本说明。");
        builder.AppendLine();
        builder.AppendLine("## 当前路径");
        builder.AppendLine();
        builder.AppendLine($"- JSON: `{manifest.JsonPath}`");
        builder.AppendLine($"- Markdown: `{manifest.MarkdownPath}`");
        builder.AppendLine($"- Validation JSON: `{manifest.ValidationJsonPath}`");
        builder.AppendLine($"- Validation Markdown: `{manifest.ValidationMarkdownPath}`");
        return builder.ToString();
    }
}
