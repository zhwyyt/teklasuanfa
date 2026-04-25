using System;

namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionSemanticRegressionManifest
{
    public required DateTime GeneratedAtUtc { get; init; }

    public required string OutputDirectory { get; init; }

    public required string SnapshotJsonFileName { get; init; }

    public required string SnapshotMarkdownFileName { get; init; }

    public required int RowCount { get; init; }
}
