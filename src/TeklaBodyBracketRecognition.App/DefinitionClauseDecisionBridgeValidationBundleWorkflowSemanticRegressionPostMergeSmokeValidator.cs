using System.Collections.Generic;
using System.IO;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidator
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationResult Validate(
        DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeWorkflowResult workflowResult)
    {
        var missingPaths = new List<string>();
        var postMergeResult = workflowResult.PostMergeResult;
        var semanticRegressionMerge = postMergeResult.SemanticRegressionMerge;
        var attachment = semanticRegressionMerge.Contribution.Attachment;

        RequireExistingFile(workflowResult.ManifestPath, missingPaths);
        RequireExistingFile(workflowResult.ReadmePath, missingPaths);
        RequireExistingFile(postMergeResult.SummaryPath, missingPaths);
        RequireExistingFile(postMergeResult.ReadmePath, missingPaths);
        RequireExistingFile(postMergeResult.ManifestPath, missingPaths);
        RequireExistingFile(attachment.WorkflowResult.JsonPath, missingPaths);
        RequireExistingFile(attachment.WorkflowResult.MarkdownPath, missingPaths);
        RequireExistingFile(attachment.WorkflowResult.ManifestPath, missingPaths);
        RequireExistingFile(attachment.WorkflowResult.ReadmePath, missingPaths);
        RequireExistingFile(attachment.WorkflowResult.BundleSectionJsonPath, missingPaths);
        RequireExistingFile(attachment.WorkflowResult.BundleSectionMarkdownPath, missingPaths);

        var succeeded = missingPaths.Count == 0;
        var summary = succeeded
            ? "post-merge smoke 输出校验通过：关键 manifest / README / summary / bundle-section 文件均已落盘。"
            : "post-merge smoke 输出校验失败：存在缺失文件。";

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeValidationResult
        {
            Succeeded = succeeded,
            MissingPaths = missingPaths,
            Summary = summary
        };
    }

    private static void RequireExistingFile(string path, ICollection<string> missingPaths)
    {
        if (!File.Exists(path))
        {
            missingPaths.Add(path);
        }
    }
}
