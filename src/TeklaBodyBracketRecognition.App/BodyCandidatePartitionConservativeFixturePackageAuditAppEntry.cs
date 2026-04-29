using System;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditAppEntry
{
    public static int Run(string[] args, string defaultOutputRootDirectory)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (string.IsNullOrWhiteSpace(defaultOutputRootDirectory))
        {
            throw new ArgumentException("Default output root directory must not be empty.", nameof(defaultOutputRootDirectory));
        }

        return BodyCandidatePartitionConservativeFixturePackageAuditCommandHandler.TryHandle(
            args,
            defaultOutputRootDirectory,
            out var result)
            ? result.ExitCode
            : -1;
    }
}
