using System.Text.Json;
using TeklaBodyBracketRecognition.App;
using TeklaBodyBracketRecognition.Core.Algorithms;

var analyzer = new AssemblyAnalyzer();
var input = SampleAssemblyFactory.CreateBuiltUpHWithBracket();
var result = analyzer.Analyze(input);

var json = JsonSerializer.Serialize(
    result,
    new JsonSerializerOptions
    {
        WriteIndented = true
    });

Console.WriteLine(json);
