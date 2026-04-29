using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditValidationSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string SerializeJson(
        BodyCandidatePartitionConservativeFixturePackageAuditValidationArtifact artifact)
    {
        if (artifact is null)
        {
            throw new ArgumentNullException(nameof(artifact));
        }

        return JsonSerializer.Serialize(artifact, JsonOptions);
    }

    public static string SerializeMarkdown(
        BodyCandidatePartitionConservativeFixturePackageAuditValidationArtifact artifact)
    {
        if (artifact is null)
        {
            throw new ArgumentNullException(nameof(artifact));
        }

        var builder = new StringBuilder();
        builder.AppendLine("# Body Candidate Partition Conservative Fixture Package Audit Validation");
        builder.AppendLine();
        builder.AppendLine($"- GeneratedAtUtc: `{artifact.GeneratedAtUtc:O}`");
        builder.AppendLine($"- OutputDirectory: `{artifact.OutputDirectory}`");
        builder.AppendLine($"- ExitCode: `{artifact.ExitCode}`");
        builder.AppendLine($"- IsSuccess: `{artifact.IsSuccess}`");
        builder.AppendLine($"- TotalCount: `{artifact.TotalCount}`");
        builder.AppendLine($"- PassedCount: `{artifact.PassedCount}`");
        builder.AppendLine($"- FailedCount: `{artifact.FailedCount}`");
        builder.AppendLine($"- PackageIsComplete: `{artifact.PackageIsComplete}`");
        builder.AppendLine();
        builder.AppendLine("## Missing Files");
        builder.AppendLine();

        if (artifact.MissingFiles.Count == 0)
        {
            builder.AppendLine("- None");
        }
        else
        {
            foreach (var missingFile in artifact.MissingFiles)
            {
                builder.AppendLine($"- `{missingFile}`");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Issues");
        builder.AppendLine();

        if (artifact.Issues.Count == 0)
        {
            builder.AppendLine("- None");
        }
        else
        {
            foreach (var issue in artifact.Issues.Where(issue => !string.IsNullOrWhiteSpace(issue)))
            {
                builder.AppendLine($"- {issue}");
            }
        }

        return builder.ToString();
    }
}
