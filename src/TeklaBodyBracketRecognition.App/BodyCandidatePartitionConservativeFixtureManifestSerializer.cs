namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureManifestSerializer
{
    public static void Write(
        string path,
        BodyCandidatePartitionConservativeFixtureManifest manifest)
    {
        var directory = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(
            manifest,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
            });

        System.IO.File.WriteAllText(
            path,
            json,
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
