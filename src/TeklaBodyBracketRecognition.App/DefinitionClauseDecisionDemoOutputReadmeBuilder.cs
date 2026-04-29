using System.IO;
using System.Text;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoOutputReadmeBuilder
{
    public static string FileName => "README.md";

    public static string BuildMarkdown(DefinitionClauseDecisionSidecarManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClauseDecision Demo Output");
        builder.AppendLine();
        builder.AppendLine("这个目录是 `DefinitionClauseDecision` demo 最小闭环的输出目录。");
        builder.AppendLine();
        builder.AppendLine("固定包含以下文件：");
        builder.AppendLine();
        builder.AppendLine("- `definition-clause-decision-snapshot.json`");
        builder.AppendLine("- `definition-clause-decision-snapshot-validation.json`");
        builder.AppendLine("- `definition-clause-decision-snapshot-validation.md`");
        builder.AppendLine("- `definition-clause-decision-snapshot-roundtrip.json`");
        builder.AppendLine("- `definition-clause-decision-snapshot-roundtrip.md`");
        builder.AppendLine("- `definition-clause-decision-summary.json`");
        builder.AppendLine("- `definition-clause-decision-summary.zh-CN.md`");
        builder.AppendLine("- `definition-clause-decision-fixture-report.json`");
        builder.AppendLine("- `definition-clause-decision-fixture-report.md`");
        builder.AppendLine("- `definition-clause-decision-bridge-validation-bundle/definition-clause-decision-bridge-validation-bundle-manifest.json`");
        builder.AppendLine("- `definition-clause-decision-bridge-validation-bundle/definition-clause-decision-bridge-validation-summary.md`");
        builder.AppendLine("- `definition-clause-decision-bridge-validation-bundle/README.md`");
        builder.AppendLine("- `definition-clause-decision-bridge-validation-bundle/mapper-fixtures/definition-clause-decision-bridge-mapper-fixtures.json`");
        builder.AppendLine("- `definition-clause-decision-bridge-validation-bundle/mapper-fixtures/definition-clause-decision-bridge-mapper-fixtures.md`");
        builder.AppendLine("- `definition-clause-decision-bridge-validation-bundle/effect-adapter-fixtures/definition-clause-decision-bridge-effect-adapter-fixtures.json`");
        builder.AppendLine("- `definition-clause-decision-bridge-validation-bundle/effect-adapter-fixtures/definition-clause-decision-bridge-effect-adapter-fixtures.md`");
        builder.AppendLine("- `definition-clause-decision-demo-manifest.json`");
        builder.AppendLine("- `definition-clause-decision-demo-validation.json`");
        builder.AppendLine("- `definition-clause-decision-demo-validation.md`");
        builder.AppendLine("- `README.md`");
        builder.AppendLine();
        builder.AppendLine("当前路径：");
        builder.AppendLine();
        builder.AppendLine($"- SnapshotJsonPath: `{manifest.SnapshotJsonPath}`");
        builder.AppendLine($"- SnapshotValidationJsonPath: `{manifest.SnapshotValidationJsonPath}`");
        builder.AppendLine($"- SnapshotValidationMarkdownPath: `{manifest.SnapshotValidationMarkdownPath}`");
        builder.AppendLine($"- SnapshotRoundTripJsonPath: `{manifest.SnapshotRoundTripJsonPath}`");
        builder.AppendLine($"- SnapshotRoundTripMarkdownPath: `{manifest.SnapshotRoundTripMarkdownPath}`");
        builder.AppendLine($"- ValidationJsonPath: `{manifest.ValidationJsonPath}`");
        builder.AppendLine($"- SummaryJsonPath: `{manifest.SummaryJsonPath}`");
        builder.AppendLine($"- SummaryMarkdownPath: `{manifest.SummaryMarkdownPath}`");
        builder.AppendLine($"- FixtureJsonPath: `{manifest.FixtureJsonPath}`");
        builder.AppendLine($"- FixtureMarkdownPath: `{manifest.FixtureMarkdownPath}`");
        builder.AppendLine($"- BridgeValidationBundleOutputDirectory: `{manifest.BridgeValidationBundleOutputDirectory}`");
        builder.AppendLine($"- BridgeValidationBundleManifestPath: `{manifest.BridgeValidationBundleManifestPath}`");
        builder.AppendLine($"- BridgeValidationBundleReadmePath: `{manifest.BridgeValidationBundleReadmePath}`");
        builder.AppendLine($"- BridgeValidationBundleSummaryMarkdownPath: `{manifest.BridgeValidationBundleSummaryMarkdownPath}`");
        builder.AppendLine($"- BridgeValidationBundleMapperFixtureJsonPath: `{manifest.BridgeValidationBundleMapperFixtureJsonPath}`");
        builder.AppendLine($"- BridgeValidationBundleMapperFixtureMarkdownPath: `{manifest.BridgeValidationBundleMapperFixtureMarkdownPath}`");
        builder.AppendLine($"- BridgeValidationBundleEffectAdapterFixtureJsonPath: `{manifest.BridgeValidationBundleEffectAdapterFixtureJsonPath}`");
        builder.AppendLine($"- BridgeValidationBundleEffectAdapterFixtureMarkdownPath: `{manifest.BridgeValidationBundleEffectAdapterFixtureMarkdownPath}`");
        return builder.ToString().TrimEnd();
    }

    public static string Write(string outputDirectory, DefinitionClauseDecisionSidecarManifest manifest)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, FileName);
        var utf8Bom = new UTF8Encoding(true);
        File.WriteAllText(path, BuildMarkdown(manifest), utf8Bom);
        return path;
    }
}
