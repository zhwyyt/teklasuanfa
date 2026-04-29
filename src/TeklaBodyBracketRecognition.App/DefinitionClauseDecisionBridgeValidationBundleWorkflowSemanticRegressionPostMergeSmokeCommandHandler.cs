using System;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandHandler
{
    private const string CommandName = "--definition-clause-validation-bundle-semantic-regression-post-merge-smoke";

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandResult TryHandle(
        string[] args,
        string defaultOutputRootDirectory)
    {
        if (!ContainsCommand(args))
        {
            return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandResult
            {
                Handled = false,
                ExitCode = 0
            };
        }

        var outputRootDirectory = ResolveOutputRootDirectory(args, defaultOutputRootDirectory);
        var workflowResult =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeExportService.Export(
                outputRootDirectory);
        var validationResult =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidator.Validate(
                workflowResult);
        var validationArtifacts =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactBuilder.Build(
                validationResult);
        var validationArtifactWriteResult =
            DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationArtifactSerializer.Write(
                validationArtifacts,
                workflowResult.OutputDirectory);

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandResult
        {
            Handled = true,
            ExitCode = validationResult.Succeeded ? 0 : 1,
            OutputDirectory = workflowResult.OutputDirectory,
            ManifestPath = workflowResult.ManifestPath,
            ReadmePath = workflowResult.ReadmePath,
            Message = validationResult.Succeeded
                ? $"semantic regression post-merge smoke 已导出并校验通过：{workflowResult.OutputDirectory}"
                : $"{validationResult.Summary} 输出目录：{workflowResult.OutputDirectory}",
            ValidationSucceeded = validationResult.Succeeded,
            ValidationJsonPath = validationArtifactWriteResult.JsonPath,
            ValidationMarkdownPath = validationArtifactWriteResult.MarkdownPath
        };
    }

    private static bool ContainsCommand(string[] args)
    {
        foreach (var arg in args)
        {
            if (string.Equals(arg, CommandName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveOutputRootDirectory(string[] args, string defaultOutputRootDirectory)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], "--output-root", StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return defaultOutputRootDirectory;
    }
}
