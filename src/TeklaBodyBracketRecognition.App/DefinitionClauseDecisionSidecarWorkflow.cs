using System.IO;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionSidecarWorkflow
{
    public const string SummaryJsonFileName = "definition-clause-decision-summary.json";
    public const string SummaryMarkdownFileName = "definition-clause-decision-summary.zh-CN.md";
    public const string FixtureJsonFileName = "definition-clause-decision-fixture-report.json";
    public const string FixtureMarkdownFileName = "definition-clause-decision-fixture-report.md";
    public const string ValidationBundleDirectoryName = "definition-clause-decision-bridge-validation-bundle";

    public static void WriteSummaryArtifacts(
        string outputDirectory,
        DefinitionClauseDecisionArtifacts artifacts)
    {
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.Combine(outputDirectory, SummaryJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, SummaryMarkdownFileName);

        DefinitionClauseDecisionSidecarSerializer.WriteJson(jsonPath, artifacts);
        var markdown = DefinitionClauseDecisionArtifactBuilder.BuildFullMarkdown(artifacts);
        File.WriteAllText(markdownPath, markdown, new System.Text.UTF8Encoding(true));
    }

    public static void WriteFixtureArtifacts(
        string outputDirectory,
        DefinitionClauseDecisionFixtureArtifacts artifacts)
    {
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.Combine(outputDirectory, FixtureJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, FixtureMarkdownFileName);

        DefinitionClauseDecisionFixtureSerializer.WriteJson(jsonPath, artifacts);
        var markdown = DefinitionClauseDecisionBridgeFixtureReportBuilder.BuildMarkdown(artifacts.Results);
        File.WriteAllText(markdownPath, markdown, new System.Text.UTF8Encoding(true));
    }

    public static void WriteDefaultFixtureArtifacts(string outputDirectory)
    {
        WriteFixtureArtifacts(outputDirectory, DefinitionClauseDecisionFixtureArtifactBuilder.BuildDefault());
    }

    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowResult
        WriteDefaultFixtureArtifactsWithValidationBundle(string outputDirectory)
    {
        WriteDefaultFixtureArtifacts(outputDirectory);

        var validationBundleOutputDirectory = Path.Combine(outputDirectory, ValidationBundleDirectoryName);
        return DefinitionClauseDecisionBridgeValidationBundleWorkflow.Export(validationBundleOutputDirectory);
    }
}
