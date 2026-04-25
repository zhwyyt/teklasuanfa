using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class SectionTraceExtractor
{
    private readonly RecognitionOptions _options;

    public SectionTraceExtractor(RecognitionOptions options)
    {
        _options = options;
    }

    public SectionTraceExtractionResult Extract(
        BodyCandidatePartitionResult partition,
        SectionStationPlanResult stationPlan,
        PartGraph graph)
    {
        var defaultSectionAxisX = partition.ProvisionalBodyAxis.Normalize();
        var defaultSectionAxisY = ResolveSectionAxisY(partition.Items, graph, defaultSectionAxisX);
        var defaultSectionAxisZ = defaultSectionAxisX.Cross(defaultSectionAxisY).Normalize();

        var partitionLookup = partition.Items.ToDictionary(item => item.PartId);
        var stations = new List<SectionTraceStationResult>(stationPlan.Stations.Count);

        foreach (var station in stationPlan.Stations)
        {
            var stationItems = partition.Items
                .Where(
                    item =>
                        station.AxisPosition >= item.AxisIntervalMin - _options.ContactDistanceToleranceMm &&
                        station.AxisPosition <= item.AxisIntervalMax + _options.ContactDistanceToleranceMm)
                .ToArray();
            var sectionAxisX = ResolveSectionAxisX(partition, station.AxisPosition, defaultSectionAxisX);
            var sectionAxisY = ResolveSectionAxisY(stationItems, graph, sectionAxisX);
            if (sectionAxisY.Length <= 1e-6)
            {
                sectionAxisY = defaultSectionAxisY;
            }

            var sectionAxisZ = sectionAxisX.Cross(sectionAxisY).Normalize();
            if (sectionAxisZ.Length <= 1e-6)
            {
                sectionAxisZ = defaultSectionAxisZ;
            }

            var segments = new List<SectionTraceSegment>();
            foreach (var item in stationItems)
            {
                if (!graph.Parts.TryGetValue(item.PartId, out var part) || !part.IsPlateLike)
                {
                    continue;
                }

                if (!TryResolveTraceVectorAndSpan(part, sectionAxisX, out var traceVector, out var traceLength, out var traceReason))
                {
                    continue;
                }

                var centerY = part.Centroid.Dot(sectionAxisY);
                var centerZ = part.Centroid.Dot(sectionAxisZ);
                var halfSpan = Math.Max(Math.Abs(traceLength) * 0.5, 1.0);
                var traceY = traceVector.Dot(sectionAxisY);
                var traceZ = traceVector.Dot(sectionAxisZ);
                var startY = centerY - (traceY * halfSpan);
                var startZ = centerZ - (traceZ * halfSpan);
                var endY = centerY + (traceY * halfSpan);
                var endZ = centerZ + (traceZ * halfSpan);

                segments.Add(
                    new SectionTraceSegment
                    {
                        TraceId = $"{station.StationId}-part-{part.PartId}",
                        StationId = station.StationId,
                        PartId = part.PartId,
                        PartName = string.IsNullOrWhiteSpace(part.Name) ? "<unnamed>" : part.Name,
                        ProfileString = part.ProfileString,
                        PartitionClass = partitionLookup[part.PartId].PartitionClass,
                        CenterY = Math.Round(centerY, 3),
                        CenterZ = Math.Round(centerZ, 3),
                        StartY = Math.Round(startY, 3),
                        StartZ = Math.Round(startZ, 3),
                        EndY = Math.Round(endY, 3),
                        EndZ = Math.Round(endZ, 3),
                        WidthLength = Math.Round(Math.Sqrt(Math.Pow(endY - startY, 2) + Math.Pow(endZ - startZ, 2)), 3),
                        Reasons = new[]
                        {
                            "SECTION_TRACE_FROM_OBB_WIDTH",
                            traceReason,
                            $"PARTITION_{partitionLookup[part.PartId].PartitionClass}"
                        }
                    });
            }

            stations.Add(
                new SectionTraceStationResult
                {
                    StationId = station.StationId,
                    AxisPosition = station.AxisPosition,
                    StationKind = station.StationKind,
                    Segments = segments
                        .OrderBy(segment => segment.CenterY)
                        .ThenBy(segment => segment.CenterZ)
                        .ToArray()
                });
        }

        return new SectionTraceExtractionResult
        {
            AssemblyId = partition.AssemblyId,
            InputMainPartId = partition.InputMainPartId,
            SectionAxisX = defaultSectionAxisX,
            SectionAxisY = defaultSectionAxisY,
            SectionAxisZ = defaultSectionAxisZ,
            Stations = stations
        };
    }

    private static Vector3 ResolveSectionAxisX(BodyCandidatePartitionResult partition, double axisPosition, Vector3 fallbackAxis)
    {
        if (partition.ProvisionalBodyAxisSegments.Count == 0)
        {
            return fallbackAxis;
        }

        var segment = partition.ProvisionalBodyAxisSegments
            .Where(item => axisPosition >= item.AxisStart - 1e-6 && axisPosition <= item.AxisEnd + 1e-6)
            .OrderBy(item => Math.Abs(((item.AxisStart + item.AxisEnd) * 0.5) - axisPosition))
            .FirstOrDefault();

        if (segment.Direction.Length > 1e-6)
        {
            return segment.Direction;
        }

        segment = partition.ProvisionalBodyAxisSegments
            .OrderBy(item => Math.Min(Math.Abs(item.AxisStart - axisPosition), Math.Abs(item.AxisEnd - axisPosition)))
            .First();
        return segment.Direction.Length > 1e-6 ? segment.Direction : fallbackAxis;
    }

    private static Vector3 ResolveSectionAxisY(IEnumerable<BodyCandidatePartitionItem> items, PartGraph graph, Vector3 sectionAxisX)
    {
        var preferredPartIds = items
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .OrderByDescending(item => item.IsInputMainPart)
            .ThenByDescending(item => item.PartitionConfidence)
            .Select(item => item.PartId)
            .ToArray();

        foreach (var partId in preferredPartIds)
        {
            if (!graph.Parts.TryGetValue(partId, out var part))
            {
                continue;
            }

            var axisY = part.PlateNormal.ProjectToPlane(sectionAxisX).Normalize();
            if (axisY.Length > 1e-6)
            {
                return axisY;
            }

            axisY = part.PlateWidthDirection.ProjectToPlane(sectionAxisX).Normalize();
            if (axisY.Length > 1e-6)
            {
                return axisY;
            }
        }

        var fallback = Math.Abs(sectionAxisX.Dot(Vector3.UnitZ)) < 0.9
            ? Vector3.UnitZ.ProjectToPlane(sectionAxisX).Normalize()
            : Vector3.UnitY.ProjectToPlane(sectionAxisX).Normalize();
        return fallback.Length > 1e-6 ? fallback : Vector3.UnitY;
    }

    private static bool TryResolveTraceVectorAndSpan(
        PartFeature part,
        Vector3 sectionAxisX,
        out Vector3 traceVector,
        out double traceLength,
        out string traceReason)
    {
        var dims = new[] { Math.Abs(part.ObbDims.X), Math.Abs(part.ObbDims.Y), Math.Abs(part.ObbDims.Z) }
            .OrderByDescending(value => value)
            .ToArray();
        var thickness = Math.Max(Math.Abs(part.Thickness ?? dims.LastOrDefault()), 1.0);
        var primaryInPlaneLength = Math.Max(dims.FirstOrDefault(), thickness);
        var secondaryInPlaneLength = Math.Max(dims.Skip(1).FirstOrDefault(), thickness);

        var candidates = new List<(Vector3 Vector, double Span, string Reason)>();

        var longProjection = part.PlateLongDirection.ProjectToPlane(sectionAxisX);
        if (longProjection.Length > 1e-6)
        {
            candidates.Add(
                (
                    longProjection.Normalize(),
                    Math.Max(primaryInPlaneLength * longProjection.Length, thickness),
                    "SECTION_TRACE_USING_LONG_DIRECTION"));
        }

        var widthProjection = part.PlateWidthDirection.ProjectToPlane(sectionAxisX);
        if (widthProjection.Length > 1e-6)
        {
            candidates.Add(
                (
                    widthProjection.Normalize(),
                    Math.Max(secondaryInPlaneLength * widthProjection.Length, thickness),
                    "SECTION_TRACE_USING_WIDTH_DIRECTION"));
        }

        var normalProjection = part.PlateNormal.ProjectToPlane(sectionAxisX);
        if (normalProjection.Length > 1e-6)
        {
            candidates.Add(
                (
                    normalProjection.Normalize(),
                    Math.Max(thickness * normalProjection.Length, 1.0),
                    "SECTION_TRACE_USING_NORMAL_FALLBACK"));
        }

        var selected = candidates
            .OrderByDescending(candidate => candidate.Span)
            .FirstOrDefault();

        if (selected.Vector.Length <= 1e-6 || selected.Span <= 1e-6)
        {
            traceVector = Vector3.Zero;
            traceLength = 0;
            traceReason = string.Empty;
            return false;
        }

        traceVector = selected.Vector;
        traceLength = selected.Span;
        traceReason = selected.Reason;
        return true;
    }
}
