using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditCommandHandler
{
    public const string CommandName = "--body-candidate-partition-conservative-fixture-package-audit";
    public const string OutputRootOption = "--output-root";

    public static bool TryHandle(
        IReadOnlyList<string> args,
        string defaultOutputRootDirectory,
        out BodyCandidatePartitionConservativeFixturePackageAuditCommandResult result)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (string.IsNullOrWhiteSpace(defaultOutputRootDirectory))
        {
            throw new ArgumentException("Default output root directory must not be empty.", nameof(defaultOutputRootDirectory));
        }

        result = default!;
        if (!BodyCandidatePartitionConservativeFixtureCommandHandlerSupport.ContainsCommand(args, CommandName))
        {
            return false;
        }

        var outputRootDirectory =
            BodyCandidatePartitionConservativeFixtureCommandHandlerSupport.ResolveOutputRootDirectory(
                args,
                OutputRootOption,
                defaultOutputRootDirectory);
        result = BodyCandidatePartitionConservativeFixturePackageAuditExportService.Export(outputRootDirectory);
        return true;
    }
}
