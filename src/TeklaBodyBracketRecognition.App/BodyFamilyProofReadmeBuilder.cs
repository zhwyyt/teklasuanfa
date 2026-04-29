using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyFamilyProofReadmeBuilder
{
    public static string BuildMarkdown(BodyFamilyProofManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Body Family Proof Output");
        builder.AppendLine();
        builder.AppendLine("本目录保存阶段 6 首版 `body-family-proof` sidecar。");
        builder.AppendLine();
        builder.AppendLine("## 文件说明");
        builder.AppendLine();
        builder.AppendLine("- `body-family-proof.json`：构件级家族判定结果。");
        builder.AppendLine("- `body-family-proof.zh-CN.md`：面向人工 review 的中文摘要。");
        builder.AppendLine("- `body-family-proof-validation.json`：结构化自校验结果。");
        builder.AppendLine("- `body-family-proof-validation.md`：校验摘要。");
        builder.AppendLine("- `body-family-proof-manifest.json`：当前导出文件路径清单。");
        builder.AppendLine("- `README.md`：本说明。");
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
