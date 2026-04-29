using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixtureCommandHandlerSupport
{
    public static bool ContainsCommand(IReadOnlyList<string> args, string commandName)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (string.IsNullOrWhiteSpace(commandName))
        {
            throw new ArgumentException("Command name must not be empty.", nameof(commandName));
        }

        for (var index = 0; index < args.Count; index++)
        {
            if (string.Equals(args[index], commandName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static string ResolveOutputRootDirectory(
        IReadOnlyList<string> args,
        string optionName,
        string defaultOutputRootDirectory)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (string.IsNullOrWhiteSpace(optionName))
        {
            throw new ArgumentException("Option name must not be empty.", nameof(optionName));
        }

        if (string.IsNullOrWhiteSpace(defaultOutputRootDirectory))
        {
            throw new ArgumentException(
                "Default output root directory must not be empty.",
                nameof(defaultOutputRootDirectory));
        }

        for (var index = 0; index < args.Count - 1; index++)
        {
            if (!string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var candidate = args[index + 1];
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        return defaultOutputRootDirectory;
    }
}
