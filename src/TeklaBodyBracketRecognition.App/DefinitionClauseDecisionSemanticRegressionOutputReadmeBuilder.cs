using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionSemanticRegressionOutputReadmeBuilder
{
    public static string Build(DefinitionClauseDecisionSemanticRegressionManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClause Semantic Regression Output");
        builder.AppendLine();
        builder.AppendLine("该目录用于固化 `SYNTHETIC_NONE_TOPOLOGY / SYNTHETIC_TARGET_ONLY_REVIEW` 等语义回归样本，供后续并入 validation bundle / fixture / sidecar 导出链。");
        builder.AppendLine();
        builder.AppendLine("## 文件");
        builder.AppendLine();
        builder.Append("- `").Append(manifest.SnapshotJsonFileName).AppendLine("`：结构化 JSON 快照");
        builder.Append("- `").Append(manifest.SnapshotMarkdownFileName).AppendLine("`：Markdown 语义回归摘要");
        builder.AppendLine("- `manifest.json`：本目录自描述元数据");
        builder.AppendLine("- `README.md`：本说明文件");
        builder.AppendLine();
        builder.Append("当前样本行数：").AppendLine(manifest.RowCount.ToString());
        builder.AppendLine();
        builder.AppendLine("## 下一步");
        builder.AppendLine();
        builder.AppendLine("1. 将本目录作为独立 section 并入现有 validation bundle");
        builder.AppendLine("2. 将 snapshot 行与 mapper / effect-adapter fixtures 对齐");
        builder.AppendLine("3. 在旧 bridge / fixture / sidecar 导出链稳定后，缩并独立 workflow 入口");
        return builder.ToString();
    }
}
