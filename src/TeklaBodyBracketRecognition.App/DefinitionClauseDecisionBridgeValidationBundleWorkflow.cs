using System;
using System.IO;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeValidationBundleWorkflow
{
    public static DefinitionClauseDecisionBridgeValidationBundleWorkflowResult Export(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        var mapperRunResult = DefinitionClauseDecisionBridgeMapperFixtureRunner.RunDefault();
        var effectAdapterRunResult = DefinitionClauseDecisionBridgeEffectAdapterFixtureRunner.RunDefault();

        var mapperExport = DefinitionClauseDecisionBridgeMapperFixtureWorkflow.Export(
            Path.Combine(outputDirectory, "mapper-fixtures"),
            mapperRunResult);
        var effectAdapterExport = DefinitionClauseDecisionBridgeEffectAdapterFixtureWorkflow.Export(
            Path.Combine(outputDirectory, "effect-adapter-fixtures"),
            effectAdapterRunResult);

        var manifest = new DefinitionClauseDecisionBridgeValidationBundleManifest
        {
            GeneratedAtUtc = DateTime.UtcNow,
            OutputDirectory = outputDirectory,
            MapperFixtureJsonPath = mapperExport.JsonPath,
            MapperFixtureMarkdownPath = mapperExport.MarkdownPath,
            EffectAdapterFixtureJsonPath = effectAdapterExport.JsonPath,
            EffectAdapterFixtureMarkdownPath = effectAdapterExport.MarkdownPath,
            SummaryMarkdownPath = Path.Combine(outputDirectory, "definition-clause-decision-bridge-validation-summary.md"),
            ReadmePath = Path.Combine(outputDirectory, "README.md"),
        };

        var manifestPath = Path.Combine(outputDirectory, "definition-clause-decision-bridge-validation-bundle-manifest.json");
        var summaryMarkdown = DefinitionClauseDecisionBridgeValidationBundleSummaryBuilder.BuildMarkdown(
            mapperRunResult,
            effectAdapterRunResult,
            manifest);
        var readmeMarkdown = DefinitionClauseDecisionBridgeValidationBundleReadmeBuilder.BuildMarkdown(manifest);

        DefinitionClauseDecisionBridgeValidationBundleSerializer.WriteManifest(manifestPath, manifest);
        DefinitionClauseDecisionBridgeValidationBundleSerializer.WriteMarkdown(manifest.SummaryMarkdownPath, summaryMarkdown);
        DefinitionClauseDecisionBridgeValidationBundleSerializer.WriteMarkdown(manifest.ReadmePath, readmeMarkdown);

        return new DefinitionClauseDecisionBridgeValidationBundleWorkflowResult
        {
            OutputDirectory = outputDirectory,
            ManifestPath = manifestPath,
            ReadmePath = manifest.ReadmePath,
            SummaryMarkdownPath = manifest.SummaryMarkdownPath,
            MapperFixtureExport = mapperExport,
            EffectAdapterFixtureExport = effectAdapterExport,
        };
    }
}
