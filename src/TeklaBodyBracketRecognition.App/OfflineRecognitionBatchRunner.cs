using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class OfflineRecognitionBatchRunner
{
    public static int Run(
        string inputPath,
        string? explicitOutputDirectory,
        AnalysisPipelineServices services,
        JsonSerializerOptions jsonOptions,
        TextWriter output,
        TextWriter error)
    {
        var resolvedInputPath = Path.GetFullPath(inputPath);
        var outputDirectory = ResolveOutputDirectory(resolvedInputPath, explicitOutputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jobs = XingcaiCacheImporter.LoadJobs(resolvedInputPath);
        if (jobs.Count == 0)
        {
            error.WriteLine($"未找到可分析的 xingcai cache 文件: {resolvedInputPath}");
            return 2;
        }

        var artifacts = new BatchArtifacts();
        foreach (var job in jobs)
        {
            OfflineBatchWorkflow.ProcessJob(job, services, artifacts, output);
        }

        _ = DefinitionDrivenSidecarCoordinator.Run(outputDirectory, artifacts);

        BatchArtifactWriter.Write(outputDirectory, artifacts, jsonOptions);

        output.WriteLine($"已完成 {jobs.Count} 个 assembly 的离线分析。输出目录: {outputDirectory}");
        return 0;
    }

    private static string ResolveOutputDirectory(string inputPath, string? explicitOutputDirectory)
    {
        if (!string.IsNullOrWhiteSpace(explicitOutputDirectory))
        {
            return Path.GetFullPath(explicitOutputDirectory);
        }

        if (File.Exists(inputPath))
        {
            var directory = Path.GetDirectoryName(inputPath) ?? Directory.GetCurrentDirectory();
            return Path.Combine(directory, "body-bracket-results");
        }

        return Path.Combine(inputPath, "body-bracket-results");
    }
}
