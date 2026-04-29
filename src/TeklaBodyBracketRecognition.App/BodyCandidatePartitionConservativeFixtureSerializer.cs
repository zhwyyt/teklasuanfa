namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureSerializer
{
    public static void WriteJson(
        string path,
        BodyCandidatePartitionConservativeFixtureArtifact artifact)
    {
        var directory = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(
            artifact,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
            });

        System.IO.File.WriteAllText(
            path,
            json,
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public static void WriteMarkdown(
        string path,
        BodyCandidatePartitionConservativeFixtureArtifact artifact)
    {
        var directory = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        System.IO.File.WriteAllText(
            path,
            artifact.MarkdownReport.TrimEnd() + System.Environment.NewLine,
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
