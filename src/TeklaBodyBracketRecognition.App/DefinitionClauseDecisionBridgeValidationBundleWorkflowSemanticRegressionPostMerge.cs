using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMerge
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeResult Run(
        string outputRootDirectory)
    {
        var baseWorkflowResult = RunBaseWorkflow(outputRootDirectory);
        var outputDirectory = ResolveOutputDirectory(baseWorkflowResult, outputRootDirectory);
        var summaryPath = ResolveMarkdownPath(
            baseWorkflowResult,
            outputDirectory,
            "SummaryPath",
            "SummaryMarkdownPath",
            "ValidationSummaryPath");
        var readmePath = ResolveMarkdownPath(
            baseWorkflowResult,
            outputDirectory,
            "ReadmePath",
            "ReadmeMarkdownPath",
            "ValidationReadmePath");
        var manifestPath = ResolveJsonPath(
            baseWorkflowResult,
            outputDirectory,
            "ManifestPath",
            "ManifestJsonPath",
            "ValidationManifestPath");

        var existingSummaryMarkdown = File.Exists(summaryPath) ? File.ReadAllText(summaryPath) : string.Empty;
        var existingReadmeMarkdown = File.Exists(readmePath) ? File.ReadAllText(readmePath) : string.Empty;

        var mergeResult = DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger.Merge(
            outputRootDirectory,
            existingSummaryMarkdown,
            existingReadmeMarkdown);

        WriteUtf8Bom(summaryPath, mergeResult.SummaryMarkdown);
        WriteUtf8Bom(readmePath, mergeResult.ReadmeMarkdown);
        MergeManifest(manifestPath, mergeResult.ManifestEntry);

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeResult
        {
            OutputDirectory = outputDirectory,
            SummaryPath = summaryPath,
            ReadmePath = readmePath,
            ManifestPath = manifestPath,
            SemanticRegressionMerge = mergeResult
        };
    }

    private static object RunBaseWorkflow(string outputRootDirectory)
    {
        var workflowType = typeof(DefinitionClauseDecisionBridgeValidationBundleWorkflow);
        var runMethod = workflowType.GetMethod(
            "Run",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types: new[] { typeof(string) },
            modifiers: null);

        if (runMethod is null)
        {
            throw new InvalidOperationException(
                "DefinitionClauseDecisionBridgeValidationBundleWorkflow 缺少可反射调用的静态 Run(string) 入口。");
        }

        return runMethod.Invoke(null, new object[] { outputRootDirectory })
               ?? throw new InvalidOperationException(
                   "DefinitionClauseDecisionBridgeValidationBundleWorkflow.Run(...) 返回了空结果。");
    }

    private static string ResolveOutputDirectory(object workflowResult, string fallbackRootDirectory)
    {
        return TryGetStringProperty(workflowResult, "OutputDirectory", "BundleOutputDirectory", "Directory")
               ?? fallbackRootDirectory;
    }

    private static string ResolveMarkdownPath(object workflowResult, string outputDirectory, params string[] propertyNames)
    {
        var path = TryGetStringProperty(workflowResult, propertyNames);
        if (!string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        var file = Directory.Exists(outputDirectory)
            ? Directory.GetFiles(outputDirectory, "*.md", SearchOption.AllDirectories)
                .FirstOrDefault(candidate => ContainsAllKeywords(candidate, "validation", "summary"))
            : null;

        return file ?? Path.Combine(outputDirectory, "definition-clause-decision-bridge-validation-summary.md");
    }

    private static string ResolveJsonPath(object workflowResult, string outputDirectory, params string[] propertyNames)
    {
        var path = TryGetStringProperty(workflowResult, propertyNames);
        if (!string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        var file = Directory.Exists(outputDirectory)
            ? Directory.GetFiles(outputDirectory, "*.json", SearchOption.AllDirectories)
                .FirstOrDefault(candidate => ContainsAllKeywords(candidate, "validation", "manifest"))
            : null;

        return file ?? Path.Combine(outputDirectory, "definition-clause-decision-bridge-validation-manifest.json");
    }

    private static string? TryGetStringProperty(object target, params string[] names)
    {
        foreach (var name in names)
        {
            var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property?.PropertyType == typeof(string))
            {
                var value = property.GetValue(target) as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return null;
    }

    private static bool ContainsAllKeywords(string path, params string[] keywords)
    {
        var normalized = path.Replace('\\', '/').ToLowerInvariant();
        return keywords.All(keyword => normalized.Contains(keyword.ToLowerInvariant()));
    }

    private static void WriteUtf8Bom(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static void MergeManifest(
        string manifestPath,
        DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionManifestEntry entry)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath) ?? ".");

        JsonObject rootObject;
        if (File.Exists(manifestPath))
        {
            var text = File.ReadAllText(manifestPath);
            rootObject = JsonNode.Parse(text) as JsonObject ?? new JsonObject();
        }
        else
        {
            rootObject = new JsonObject();
        }

        rootObject["semanticRegression"] = JsonSerializer.SerializeToNode(entry);

        if (rootObject["sections"] is JsonArray sections)
        {
            sections.Add(JsonSerializer.SerializeToNode(entry));
        }

        var json = rootObject.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(manifestPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
