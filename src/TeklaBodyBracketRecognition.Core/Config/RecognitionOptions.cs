namespace TeklaBodyBracketRecognition.Core.Config;

public sealed record RecognitionOptions
{
    public double AxisAngleToleranceDeg { get; init; } = 10.0;
    public double NormalParallelToleranceDeg { get; init; } = 10.0;
    public double TransversePositionToleranceMm { get; init; } = 8.0;
    public double ChainGapToleranceMm { get; init; } = 20.0;
    public double StrongPersistenceMin { get; init; } = 0.55;
    public double WeakPersistenceMin { get; init; } = 0.30;
    public double SectionEndTrimRatio { get; init; } = 0.08;
    public int SectionSampleCountMin { get; init; } = 7;
    public int SectionSampleCountMax { get; init; } = 21;
    public double BracketAxisSpanRatioMax { get; init; } = 0.35;
    public double BracketOverhangRatioMin { get; init; } = 1.20;
    public double ManualReviewConfidenceMin { get; init; } = 0.75;
    public double TinyPartVolumeThreshold { get; init; } = 50_000.0;
    public double ContactDistanceToleranceMm { get; init; } = 5.0;
}
