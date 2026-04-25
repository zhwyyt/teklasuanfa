using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class SectionTraceCleaner
{
    private readonly RecognitionOptions _options;

    public SectionTraceCleaner(RecognitionOptions options)
    {
        _options = options;
    }

    public SectionTraceCleaningResult Clean(SectionTraceExtractionResult extraction)
    {
        var stations = extraction.Stations
            .Select(CleanStation)
            .ToArray();

        return new SectionTraceCleaningResult
        {
            AssemblyId = extraction.AssemblyId,
            InputMainPartId = extraction.InputMainPartId,
            Stations = stations
        };
    }

    private SectionTraceCleanStationResult CleanStation(SectionTraceStationResult station)
    {
        var lengthReference = ResolveLengthReference(station.Segments);
        var bodyCandidateFloor = Math.Max(_options.ContactDistanceToleranceMm * 1.5, 6.0);
        var accessoryFloor = Math.Max(_options.ContactDistanceToleranceMm * 2.0, Math.Max(lengthReference * 0.12, 10.0));
        var duplicateTolerance = Math.Max(_options.TransversePositionToleranceMm, _options.ContactDistanceToleranceMm * 2.0);

        var retained = new List<SectionTraceSegment>();
        var cleaned = new List<SectionTraceCleanSegment>(station.Segments.Count);
        foreach (var segment in station.Segments
                     .OrderByDescending(item => item.WidthLength)
                     .ThenBy(item => item.PartId)
                     .ThenBy(item => item.TraceId, StringComparer.Ordinal))
        {
            var cleanReasons = new List<string>();
            var minLength = segment.PartitionClass == BodyCandidatePartitionClass.BodyCandidate
                ? bodyCandidateFloor
                : accessoryFloor;

            if (segment.WidthLength < minLength)
            {
                cleanReasons.Add("SHORT_TRACE");
            }

            var duplicate = retained.FirstOrDefault(existing => IsNearDuplicate(existing, segment, duplicateTolerance));
            if (duplicate is not null)
            {
                cleanReasons.Add($"DUPLICATE_OF_{duplicate.TraceId}");
            }

            var isSuppressed = cleanReasons.Count > 0;
            if (!isSuppressed)
            {
                retained.Add(segment);
                cleanReasons.Add("RETAINED");
            }

            cleaned.Add(
                new SectionTraceCleanSegment
                {
                    TraceId = segment.TraceId,
                    StationId = segment.StationId,
                    PartId = segment.PartId,
                    PartName = segment.PartName,
                    ProfileString = segment.ProfileString,
                    PartitionClass = segment.PartitionClass,
                    CenterY = segment.CenterY,
                    CenterZ = segment.CenterZ,
                    StartY = segment.StartY,
                    StartZ = segment.StartZ,
                    EndY = segment.EndY,
                    EndZ = segment.EndZ,
                    WidthLength = segment.WidthLength,
                    IsSuppressed = isSuppressed,
                    CleanReasons = cleanReasons,
                    SourceReasons = segment.Reasons
                });
        }

        return new SectionTraceCleanStationResult
        {
            StationId = station.StationId,
            AxisPosition = station.AxisPosition,
            StationKind = station.StationKind,
            Segments = cleaned
                .OrderBy(item => item.IsSuppressed)
                .ThenBy(item => item.CenterY)
                .ThenBy(item => item.CenterZ)
                .ToArray()
        };
    }

    private static double ResolveLengthReference(IReadOnlyList<SectionTraceSegment> segments)
    {
        var candidates = segments
            .Where(item => item.PartitionClass is BodyCandidatePartitionClass.BodyCandidate or BodyCandidatePartitionClass.BodyAccessoryCandidate)
            .Select(item => item.WidthLength)
            .OrderBy(item => item)
            .ToArray();

        if (candidates.Length == 0)
        {
            candidates = segments.Select(item => item.WidthLength).OrderBy(item => item).ToArray();
        }

        if (candidates.Length == 0)
        {
            return 0.0;
        }

        var mid = candidates.Length / 2;
        return candidates.Length % 2 == 0
            ? (candidates[mid - 1] + candidates[mid]) / 2.0
            : candidates[mid];
    }

    private static bool IsNearDuplicate(SectionTraceSegment left, SectionTraceSegment right, double tolerance)
    {
        if (left.PartId == right.PartId)
        {
            return true;
        }

        if (left.PartitionClass != right.PartitionClass)
        {
            return false;
        }

        var centerDistance = Math.Sqrt(Math.Pow(left.CenterY - right.CenterY, 2) + Math.Pow(left.CenterZ - right.CenterZ, 2));
        if (centerDistance > tolerance)
        {
            return false;
        }

        var sameDirectionDistance = EndpointDistance(left.StartY, left.StartZ, right.StartY, right.StartZ) +
                                    EndpointDistance(left.EndY, left.EndZ, right.EndY, right.EndZ);
        var flippedDirectionDistance = EndpointDistance(left.StartY, left.StartZ, right.EndY, right.EndZ) +
                                       EndpointDistance(left.EndY, left.EndZ, right.StartY, right.StartZ);
        return Math.Min(sameDirectionDistance, flippedDirectionDistance) <= tolerance * 2.0;
    }

    private static double EndpointDistance(double leftY, double leftZ, double rightY, double rightZ)
    {
        return Math.Sqrt(Math.Pow(leftY - rightY, 2) + Math.Pow(leftZ - rightZ, 2));
    }
}
