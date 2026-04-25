using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceReadmeBuilder
{
    public static string BuildMarkdown(DefinitionClauseDecisionFullRunSourceManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClauseDecision Full-Run Source Output");
        builder.AppendLine();
        builder.AppendLine("本目录保存 `DefinitionClauseDecision` 在真实 full-run 结果接线前的第一层工件。");
        builder.AppendLine();
        builder.AppendLine("## 文件说明");
        builder.AppendLine();
        builder.AppendLine("- `definition-clause-decision-fullrun-source.json`：结构化真实输入工件。");
        builder.AppendLine("- `definition-clause-decision-fullrun-source.zh-CN.md`：面向人工 review 的中文摘要。");
        builder.AppendLine("- `definition-clause-decision-fullrun-source-validation.json`：结构化自校验结果。");
        builder.AppendLine("- `definition-clause-decision-fullrun-source-validation.md`：面向人工 review 的校验摘要。");
        builder.AppendLine("- `definition-clause-decision-fullrun-source-manifest.json`：当前导出文件路径清单。");
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
