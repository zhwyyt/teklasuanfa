using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditExportService
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditCommandResult Export(string outputRootDirectory)
    {
        var packageAuditResult = ExportPackageAuditOnly(outputRootDirectory);
        var validationResult = BodyCandidatePartitionConservativeFixturePackageAuditValidationWorkflow.Run(packageAuditResult);

        return packageAuditResult with
        {
            ExitCode = validationResult.ExitCode,
            ValidationJsonPath = validationResult.ValidationJsonPath,
            ValidationMarkdownPath = validationResult.ValidationMarkdownPath,
            ValidationSucceeded = validationResult.IsSuccess
        };
    }

    internal static BodyCandidatePartitionConservativeFixturePackageAuditCommandResult ExportPackageAuditOnly(
        string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
        {
            throw new ArgumentException("Output root directory must not be empty.", nameof(outputRootDirectory));
        }

        var workflowMethod = ResolveWorkflowMethod();
        var workflowResult = workflowMethod.Invoke(null, new object[] { outputRootDirectory });
        if (workflowResult is null)
        {
            throw new InvalidOperationException(
                $"{nameof(BodyCandidatePartitionConservativeFixturePackageAuditWorkflow)} returned null.");
        }

        return BuildCommandResult(workflowResult, outputRootDirectory);
    }

    private static MethodInfo ResolveWorkflowMethod()
    {
        var workflowType = typeof(BodyCandidatePartitionConservativeFixturePackageAuditWorkflow);
        var method = workflowType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(candidate => candidate.GetParameters().Length == 1)
            .Where(candidate => candidate.GetParameters()[0].ParameterType == typeof(string))
            .FirstOrDefault(candidate =>
                string.Equals(
                    candidate.ReturnType.Name,
                    nameof(BodyCandidatePartitionConservativeFixturePackageAuditResult),
                    StringComparison.Ordinal));

        return method
            ?? throw new InvalidOperationException(
                $"No public static single-string runner was found on {workflowType.FullName}.");
    }

    private static BodyCandidatePartitionConservativeFixturePackageAuditCommandResult BuildCommandResult(
        object workflowResult,
        string fallbackOutputRootDirectory)
    {
        var outputDirectory = ReadStringProperty(
            workflowResult,
            "OutputDirectory",
            "PackageDirectory",
            "OutputRootDirectory",
            "OutputRootPath")
            ?? fallbackOutputRootDirectory;

        var totalCount = ReadIntProperty(workflowResult, "TotalCount");
        var passedCount = ReadIntProperty(workflowResult, "PassedCount");
        var failedCount = ReadIntProperty(workflowResult, "FailedCount");
        var packageIsComplete = ReadBoolProperty(workflowResult, "PackageIsComplete");
        var missingFiles = ReadStringListProperty(workflowResult, "MissingFiles");
        var exitCode = ReadNullableIntProperty(workflowResult, "ExitCode")
            ?? ((failedCount == 0 && packageIsComplete) ? 0 : 1);

        return new BodyCandidatePartitionConservativeFixturePackageAuditCommandResult(
            ExitCode: exitCode,
            OutputDirectory: outputDirectory,
            JsonPath: ReadStringProperty(workflowResult, "JsonPath") ?? string.Empty,
            MarkdownPath: ReadStringProperty(workflowResult, "MarkdownPath") ?? string.Empty,
            ManifestPath: ReadStringProperty(workflowResult, "ManifestPath") ?? string.Empty,
            ReadmePath: ReadStringProperty(workflowResult, "ReadmePath") ?? string.Empty,
            ValidationJsonPath: string.Empty,
            ValidationMarkdownPath: string.Empty,
            ValidationSucceeded: false,
            TotalCount: totalCount,
            PassedCount: passedCount,
            FailedCount: failedCount,
            PackageIsComplete: packageIsComplete,
            MissingFiles: missingFiles);
    }

    private static string? ReadStringProperty(object instance, params string[] candidateNames)
    {
        foreach (var candidateName in candidateNames)
        {
            var property = instance.GetType().GetProperty(candidateName, BindingFlags.Public | BindingFlags.Instance);
            if (property is null || property.PropertyType != typeof(string))
            {
                continue;
            }

            return property.GetValue(instance) as string;
        }

        return null;
    }

    private static int ReadIntProperty(object instance, params string[] candidateNames)
    {
        return ReadNullableIntProperty(instance, candidateNames) ?? 0;
    }

    private static int? ReadNullableIntProperty(object instance, params string[] candidateNames)
    {
        foreach (var candidateName in candidateNames)
        {
            var property = instance.GetType().GetProperty(candidateName, BindingFlags.Public | BindingFlags.Instance);
            if (property is null)
            {
                continue;
            }

            var propertyValue = property.GetValue(instance);
            switch (propertyValue)
            {
                case int intValue:
                    return intValue;
                case long longValue when longValue <= int.MaxValue && longValue >= int.MinValue:
                    return (int)longValue;
            }
        }

        return null;
    }

    private static bool ReadBoolProperty(object instance, params string[] candidateNames)
    {
        foreach (var candidateName in candidateNames)
        {
            var property = instance.GetType().GetProperty(candidateName, BindingFlags.Public | BindingFlags.Instance);
            if (property is null || property.PropertyType != typeof(bool))
            {
                continue;
            }

            return (bool)(property.GetValue(instance) ?? false);
        }

        return false;
    }

    private static IReadOnlyList<string> ReadStringListProperty(object instance, params string[] candidateNames)
    {
        foreach (var candidateName in candidateNames)
        {
            var property = instance.GetType().GetProperty(candidateName, BindingFlags.Public | BindingFlags.Instance);
            if (property is null)
            {
                continue;
            }

            if (property.GetValue(instance) is IEnumerable<string> values)
            {
                return values.ToArray();
            }
        }

        return Array.Empty<string>();
    }
}
