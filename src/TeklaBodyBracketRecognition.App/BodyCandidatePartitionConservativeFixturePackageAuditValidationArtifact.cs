using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public sealed record BodyCandidatePartitionConservativeFixturePackageAuditValidationArtifact(
    string OutputDirectory,
    int ExitCode,
    bool IsSuccess,
    int TotalCount,
    int PassedCount,
    int FailedCount,
    bool PackageIsComplete,
    IReadOnlyList<string> MissingFiles,
    IReadOnlyList<string> Issues,
    DateTimeOffset GeneratedAtUtc);
