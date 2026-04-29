using System;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoCommandHandler
{
    public const string OutputArgumentName = "--definition-clause-decision-demo-output";

    public static DefinitionClauseDecisionDemoCommandResult TryHandle(string[]? args)
    {
        if (args is null || args.Length == 0)
        {
            return new DefinitionClauseDecisionDemoCommandResult
            {
                Handled = false,
                Succeeded = false,
                Message = "未命中 DefinitionClauseDecision demo 命令。"
            };
        }

        var outputIndex = FindArgumentIndex(args, OutputArgumentName);
        if (outputIndex < 0)
        {
            return new DefinitionClauseDecisionDemoCommandResult
            {
                Handled = false,
                Succeeded = false,
                Message = "未命中 DefinitionClauseDecision demo 命令。"
            };
        }

        if (outputIndex + 1 >= args.Length)
        {
            return new DefinitionClauseDecisionDemoCommandResult
            {
                Handled = true,
                Succeeded = false,
                Message = $"命中 {OutputArgumentName}，但缺少输出目录参数。"
            };
        }

        var outputDirectory = args[outputIndex + 1];
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            return new DefinitionClauseDecisionDemoCommandResult
            {
                Handled = true,
                Succeeded = false,
                Message = $"{OutputArgumentName} 的输出目录不能为空。"
            };
        }

        var exportResult = DefinitionClauseDecisionDemoExportService.Export(outputDirectory);

        return new DefinitionClauseDecisionDemoCommandResult
        {
            Handled = true,
            Succeeded = true,
            Message = "DefinitionClauseDecision demo sidecar 导出完成。",
            OutputDirectory = outputDirectory,
            SnapshotJsonPath = exportResult.SnapshotJsonPath,
            SnapshotValidationJsonPath = exportResult.SnapshotValidationJsonPath,
            SnapshotValidationMarkdownPath = exportResult.SnapshotValidationMarkdownPath,
            SnapshotRoundTripJsonPath = exportResult.SnapshotRoundTripJsonPath,
            SnapshotRoundTripMarkdownPath = exportResult.SnapshotRoundTripMarkdownPath,
            SummaryJsonPath = exportResult.SummaryJsonPath,
            SummaryMarkdownPath = exportResult.SummaryMarkdownPath,
            FixtureJsonPath = exportResult.FixtureJsonPath,
            FixtureMarkdownPath = exportResult.FixtureMarkdownPath,
            BridgeValidationBundleOutputDirectory = exportResult.BridgeValidationBundleOutputDirectory,
            BridgeValidationBundleManifestPath = exportResult.BridgeValidationBundleManifestPath,
            BridgeValidationBundleReadmePath = exportResult.BridgeValidationBundleReadmePath,
            BridgeValidationBundleSummaryMarkdownPath = exportResult.BridgeValidationBundleSummaryMarkdownPath,
            BridgeValidationBundleMapperFixtureJsonPath = exportResult.BridgeValidationBundleMapperFixtureJsonPath,
            BridgeValidationBundleMapperFixtureMarkdownPath = exportResult.BridgeValidationBundleMapperFixtureMarkdownPath,
            BridgeValidationBundleEffectAdapterFixtureJsonPath = exportResult.BridgeValidationBundleEffectAdapterFixtureJsonPath,
            BridgeValidationBundleEffectAdapterFixtureMarkdownPath = exportResult.BridgeValidationBundleEffectAdapterFixtureMarkdownPath,
            ManifestJsonPath = exportResult.ManifestJsonPath,
            ReadmePath = exportResult.ReadmePath,
            ValidationJsonPath = exportResult.ValidationJsonPath,
            ValidationMarkdownPath = exportResult.ValidationMarkdownPath
        };
    }

    private static int FindArgumentIndex(string[] args, string argumentName)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], argumentName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}
