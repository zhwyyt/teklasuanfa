using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class SectionTopologyAnalyzer
{
    private readonly RecognitionOptions _options;

    public SectionTopologyAnalyzer(RecognitionOptions options)
    {
        _options = options;
    }

    public SectionTopologyAnalysisResult Analyze(SectionTraceCleaningResult cleaned)
    {
        var stations = cleaned.Stations
            .Select(AnalyzeStation)
            .ToArray();

        return new SectionTopologyAnalysisResult
        {
            AssemblyId = cleaned.AssemblyId,
            InputMainPartId = cleaned.InputMainPartId,
            Stations = stations
        };
    }

    private SectionTopologyStationResult AnalyzeStation(SectionTraceCleanStationResult station)
    {
        var retained = station.Segments
            .Where(item => !item.IsSuppressed)
            .ToArray();
        var suppressed = station.Segments
            .Where(item => item.IsSuppressed)
            .ToArray();

        var flags = new List<string>();
        if (retained.Length == 0)
        {
            flags.Add("NO_RETAINED_TRACES");
            return new SectionTopologyStationResult
            {
                StationId = station.StationId,
                AxisPosition = station.AxisPosition,
                StationKind = station.StationKind,
                RetainedTraceIds = Array.Empty<string>(),
                SuppressedTraceIds = suppressed.Select(item => item.TraceId).ToArray(),
                OuterEnvelopeTraceIds = Array.Empty<string>(),
                InternalTraceIds = Array.Empty<string>(),
                RetainedBodyCandidateTraceCount = 0,
                RetainedBodyAccessoryTraceCount = 0,
                RetainedEndConnectionTraceCount = 0,
                RetainedLocalStiffenerTraceCount = 0,
                ClosedLoopCandidate = false,
                EnvelopeMinY = 0,
                EnvelopeMaxY = 0,
                EnvelopeMinZ = 0,
                EnvelopeMaxZ = 0,
                StationFlags = flags
            };
        }

        var minY = retained.Min(item => Math.Min(item.StartY, item.EndY));
        var maxY = retained.Max(item => Math.Max(item.StartY, item.EndY));
        var minZ = retained.Min(item => Math.Min(item.StartZ, item.EndZ));
        var maxZ = retained.Max(item => Math.Max(item.StartZ, item.EndZ));
        var envelopeTolerance = Math.Max(_options.ContactDistanceToleranceMm * 2.0, 8.0);

        var outerEnvelope = retained
            .Where(item => TouchesEnvelope(item, minY, maxY, minZ, maxZ, envelopeTolerance))
            .Select(item => item.TraceId)
            .ToArray();
        var outerLookup = outerEnvelope.ToHashSet(StringComparer.Ordinal);
        var internalTraceIds = retained
            .Where(item => !outerLookup.Contains(item.TraceId))
            .Select(item => item.TraceId)
            .ToArray();

        var retainedBodyCandidateTraceCount = retained.Count(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate);
        var retainedBodyAccessoryTraceCount = retained.Count(item => item.PartitionClass == BodyCandidatePartitionClass.BodyAccessoryCandidate);
        var retainedEndConnectionTraceCount = retained.Count(item => item.PartitionClass == BodyCandidatePartitionClass.EndConnectionCandidate);
        var retainedLocalStiffenerTraceCount = retained.Count(item => item.PartitionClass == BodyCandidatePartitionClass.LocalStiffenerCandidate);

        if (retainedBodyCandidateTraceCount == 0)
        {
            flags.Add("BODY_CANDIDATE_MISSING_AFTER_CLEANING");
        }

        if (internalTraceIds.Length > 0)
        {
            flags.Add("HAS_INTERNAL_TRACES");
        }

        var closedLoopCandidate = SectionClosedLoopEvidence.HasTrueClosedLoop(
            retained,
            minY,
            maxY,
            minZ,
            maxZ,
            envelopeTolerance);
        if (closedLoopCandidate)
        {
            flags.Add("CLOSED_LOOP_CANDIDATE");
        }

        return new SectionTopologyStationResult
        {
            StationId = station.StationId,
            AxisPosition = station.AxisPosition,
            StationKind = station.StationKind,
            RetainedTraceIds = retained.Select(item => item.TraceId).ToArray(),
            SuppressedTraceIds = suppressed.Select(item => item.TraceId).ToArray(),
            OuterEnvelopeTraceIds = outerEnvelope,
            InternalTraceIds = internalTraceIds,
            RetainedBodyCandidateTraceCount = retainedBodyCandidateTraceCount,
            RetainedBodyAccessoryTraceCount = retainedBodyAccessoryTraceCount,
            RetainedEndConnectionTraceCount = retainedEndConnectionTraceCount,
            RetainedLocalStiffenerTraceCount = retainedLocalStiffenerTraceCount,
            ClosedLoopCandidate = closedLoopCandidate,
            EnvelopeMinY = Math.Round(minY, 3),
            EnvelopeMaxY = Math.Round(maxY, 3),
            EnvelopeMinZ = Math.Round(minZ, 3),
            EnvelopeMaxZ = Math.Round(maxZ, 3),
            StationFlags = flags
        };
    }

    private static bool TouchesEnvelope(
        SectionTraceCleanSegment segment,
        double minY,
        double maxY,
        double minZ,
        double maxZ,
        double tolerance)
    {
        return SectionClosedLoopEvidence.TouchesEnvelope(segment, minY, maxY, minZ, maxZ, tolerance);
    }
}
