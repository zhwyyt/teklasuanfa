using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditValidator
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationResult Validate(
        BodyCandidatePartitionConservativeFixturePackageAuditCommandResult commandResult)
    {
        if (commandResult is null)
        {
            throw new ArgumentNullException(nameof(commandResult));
        }

        var issues = new List<string>();
        if (commandResult.TotalCount <= 0)
        {
            issues.Add("No conservative partition fixtures were evaluated.");
        }

        if (commandResult.FailedCount > 0)
        {
            issues.Add($"Conservative partition fixture failures detected: {commandResult.FailedCount}.");
        }

        if (!commandResult.PackageIsComplete)
        {
            issues.Add("Package audit is incomplete.");
        }

        foreach (var missingFile in commandResult.MissingFiles)
        {
            issues.Add($"Missing required artifact: {missingFile}.");
        }

        var isSuccess = issues.Count == 0 && commandResult.ExitCode == 0;
        var normalizedExitCode = isSuccess ? 0 : 1;

        return BodyCandidatePartitionConservativeFixturePackageAuditValidationResult.Create(
            outputDirectory: commandResult.OutputDirectory,
            exitCode: normalizedExitCode,
            isSuccess: isSuccess,
            totalCount: commandResult.TotalCount,
            passedCount: commandResult.PassedCount,
            failedCount: commandResult.FailedCount,
            packageIsComplete: commandResult.PackageIsComplete,
            missingFiles: commandResult.MissingFiles,
            issues: issues);
    }
}
