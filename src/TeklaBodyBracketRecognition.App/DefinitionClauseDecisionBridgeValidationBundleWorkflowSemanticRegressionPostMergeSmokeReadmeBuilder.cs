using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeReadmeBuilder
{
    public static string Build(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Validation Bundle Semantic Regression Post-Merge Smoke");
        builder.AppendLine();
        builder.AppendLine("该 smoke 输出用于验证：");
        builder.AppendLine();
        builder.AppendLine("1. 既有 validation bundle workflow 可被 post-merge 包装层成功调用");
        builder.AppendLine("2. semantic regression 的 summary / README / manifest 可被稳定并回");
        builder.AppendLine("3. bundle section 相关旁路文件路径在实际输出中可被定位");
        builder.AppendLine();
        builder.AppendLine("## 输出路径");
        builder.AppendLine();
        builder.Append("- validation summary：`").Append(manifest.ValidationSummaryPath).AppendLine("`");
        builder.Append("- validation README：`").Append(manifest.ValidationReadmePath).AppendLine("`");
        builder.Append("- validation manifest：`").Append(manifest.ValidationManifestPath).AppendLine("`");
        builder.Append("- semantic regression summary：`").Append(manifest.SemanticRegressionSummaryPath).AppendLine("`");
        builder.Append("- semantic regression README：`").Append(manifest.SemanticRegressionReadmePath).AppendLine("`");
        builder.Append("- semantic regression manifest：`").Append(manifest.SemanticRegressionManifestPath).AppendLine("`");
        builder.Append("- semantic regression bundle-section.json：`").Append(manifest.SemanticRegressionBundleSectionJsonPath).AppendLine("`");
        builder.Append("- semantic regression bundle-section-summary.md：`").Append(manifest.SemanticRegressionBundleSectionMarkdownPath).AppendLine("`");
        builder.AppendLine();
        builder.AppendLine("## 下一步");
        builder.AppendLine();
        builder.AppendLine("1. 在最小 smoke 入口上实跑该 workflow");
        builder.AppendLine("2. 确认后再把 post-merge 包装层直接并进旧 validation bundle 主入口或保留为旁路包装");
        return builder.ToString();
    }
}
