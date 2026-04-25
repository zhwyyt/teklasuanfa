using System.Text;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoOutputValidationReportBuilder
{
    public static string FileName => "definition-clause-decision-demo-validation.md";

    public static string BuildMarkdown(DefinitionClauseDecisionDemoOutputValidationResult result)
    {
        result ??= new DefinitionClauseDecisionDemoOutputValidationResult();

        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClauseDecision Demo Output Validation");
        builder.AppendLine();
        builder.AppendLine("| Item | Exists |");
        builder.AppendLine("| --- | --- |");
        builder.AppendLine($"| definition-clause-decision-snapshot.json | {ToYesNo(result.SnapshotJsonExists)} |");
        builder.AppendLine($"| definition-clause-decision-snapshot-roundtrip.json | {ToYesNo(result.SnapshotRoundTripJsonExists)} |");
        builder.AppendLine($"| definition-clause-decision-snapshot-roundtrip.md | {ToYesNo(result.SnapshotRoundTripMarkdownExists)} |");
        builder.AppendLine($"| definition-clause-decision-summary.json | {ToYesNo(result.SummaryJsonExists)} |");
        builder.AppendLine($"| definition-clause-decision-summary.zh-CN.md | {ToYesNo(result.SummaryMarkdownExists)} |");
        builder.AppendLine($"| definition-clause-decision-fixture-report.json | {ToYesNo(result.FixtureJsonExists)} |");
        builder.AppendLine($"| definition-clause-decision-fixture-report.md | {ToYesNo(result.FixtureMarkdownExists)} |");
        builder.AppendLine($"| definition-clause-decision-bridge-validation-bundle/definition-clause-decision-bridge-validation-bundle-manifest.json | {ToYesNo(result.BridgeValidationBundleManifestExists)} |");
        builder.AppendLine($"| definition-clause-decision-bridge-validation-bundle/README.md | {ToYesNo(result.BridgeValidationBundleReadmeExists)} |");
        builder.AppendLine($"| definition-clause-decision-bridge-validation-bundle/definition-clause-decision-bridge-validation-summary.md | {ToYesNo(result.BridgeValidationBundleSummaryMarkdownExists)} |");
        builder.AppendLine($"| definition-clause-decision-bridge-validation-bundle/mapper-fixtures/definition-clause-decision-bridge-mapper-fixtures.json | {ToYesNo(result.BridgeValidationBundleMapperFixtureJsonExists)} |");
        builder.AppendLine($"| definition-clause-decision-bridge-validation-bundle/mapper-fixtures/definition-clause-decision-bridge-mapper-fixtures.md | {ToYesNo(result.BridgeValidationBundleMapperFixtureMarkdownExists)} |");
        builder.AppendLine($"| definition-clause-decision-bridge-validation-bundle/effect-adapter-fixtures/definition-clause-decision-bridge-effect-adapter-fixtures.json | {ToYesNo(result.BridgeValidationBundleEffectAdapterFixtureJsonExists)} |");
        builder.AppendLine($"| definition-clause-decision-bridge-validation-bundle/effect-adapter-fixtures/definition-clause-decision-bridge-effect-adapter-fixtures.md | {ToYesNo(result.BridgeValidationBundleEffectAdapterFixtureMarkdownExists)} |");
        builder.AppendLine($"| definition-clause-decision-demo-manifest.json | {ToYesNo(result.ManifestJsonExists)} |");
        builder.AppendLine($"| README.md | {ToYesNo(result.ReadmeExists)} |");
        builder.AppendLine();
        builder.AppendLine($"AllExpectedFilesExist: {ToYesNo(result.AllExpectedFilesExist)}");
        return builder.ToString().TrimEnd();
    }

    private static string ToYesNo(bool value)
    {
        return value ? "Yes" : "No";
    }
}
