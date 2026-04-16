using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class AssemblyAnalyzer
{
    private readonly PartFeatureExtractor _featureExtractor;
    private readonly PartGraphBuilder _graphBuilder;
    private readonly BodyRecognizer _bodyRecognizer;
    private readonly BracketRecognizer _bracketRecognizer;

    public AssemblyAnalyzer(RecognitionOptions? options = null)
    {
        var resolvedOptions = options ?? new RecognitionOptions();
        _featureExtractor = new PartFeatureExtractor(resolvedOptions);
        _graphBuilder = new PartGraphBuilder(resolvedOptions);
        _bodyRecognizer = new BodyRecognizer(resolvedOptions);
        _bracketRecognizer = new BracketRecognizer(resolvedOptions);
    }

    public AssemblyRecognitionResult Analyze(AssemblyInput input)
    {
        var features = _featureExtractor.Extract(input.Parts);
        var graph = _graphBuilder.Build(features, input.Relationships);
        var body = _bodyRecognizer.Recognize(graph, input.MainPartId);
        var brackets = _bracketRecognizer.Recognize(graph, body);

        return new AssemblyRecognitionResult
        {
            AssemblyId = input.AssemblyId,
            Body = body,
            Brackets = brackets
        };
    }
}
