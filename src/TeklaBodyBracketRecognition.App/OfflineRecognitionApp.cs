using System.Text.Json;
using TeklaBodyBracketRecognition.Core.Config;

namespace TeklaBodyBracketRecognition.App;

internal static class OfflineRecognitionApp
{
    public static int Run(string[] args, JsonSerializerOptions jsonOptions, TextWriter output, TextWriter error)
    {
        var services = AnalysisPipelineServices.CreateDefault(new RecognitionOptions());

        if (args.Length == 0)
        {
            var sample = SampleAssemblyFactory.CreateBuiltUpHWithBracket();
            var sampleProof = OfflineBatchWorkflow.RunProofPipeline(
                sample.AssemblyId,
                sample.MainPartId,
                sample.Parts,
                sample.Relationships,
                sample.ImportedLongitudinalAxisSegments,
                services);
            output.WriteLine(JsonSerializer.Serialize(sampleProof, jsonOptions));
            return 0;
        }

        return OfflineRecognitionBatchRunner.Run(
            args[0],
            args.Skip(1).FirstOrDefault(),
            services,
            jsonOptions,
            output,
            error);
    }
}
