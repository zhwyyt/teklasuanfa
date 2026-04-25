using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class StableBodyZoneResolver
{
    private readonly RecognitionOptions _options;

    public StableBodyZoneResolver(RecognitionOptions options)
    {
        _options = options;
    }

    public StableBodyZoneResult Resolve(BodyCandidatePartitionResult partition)
    {
        var contributors = partition.Items
            .Where(item => item.PartitionClass is BodyCandidatePartitionClass.BodyCandidate or BodyCandidatePartitionClass.BodyAccessoryCandidate)
            .ToArray();
        var intervalSource = contributors.Length > 0 ? contributors : partition.Items.ToArray();

        if (intervalSource.Length == 0)
        {
            return new StableBodyZoneResult
            {
                AssemblyId = partition.AssemblyId,
                InputMainPartId = partition.InputMainPartId,
                ProvisionalBodyAxis = partition.ProvisionalBodyAxis,
                ProvisionalBodyAxisSegments = partition.ProvisionalBodyAxisSegments,
                AssemblyAxisIntervalMin = 0,
                AssemblyAxisIntervalMax = 0,
                Zones = Array.Empty<StableBodyZone>()
            };
        }

        var assemblyMin = intervalSource.Min(item => item.AxisIntervalMin);
        var assemblyMax = intervalSource.Max(item => item.AxisIntervalMax);
        var span = Math.Max(assemblyMax - assemblyMin, 1.0);
        var trimLength = Math.Max(span * _options.SectionEndTrimRatio, 1.0);
        var trimMin = assemblyMin + trimLength;
        var trimMax = assemblyMax - trimLength;

        var eventPoints = intervalSource
            .SelectMany(item => new[] { item.AxisIntervalMin, item.AxisIntervalMax })
            .Append(trimMin)
            .Append(trimMax)
            .Distinct()
            .OrderBy(value => value)
            .ToArray();

        var zones = new List<StableBodyZone>();
        var zoneIndex = 1;

        for (var index = 0; index < eventPoints.Length - 1; index++)
        {
            var min = eventPoints[index];
            var max = eventPoints[index + 1];
            if ((max - min) <= 1e-6)
            {
                continue;
            }

            var midpoint = (min + max) * 0.5;
            var overlapping = partition.Items
                .Where(item => item.AxisIntervalMin < max && item.AxisIntervalMax > min)
                .ToArray();

            if (overlapping.Length == 0)
            {
                continue;
            }

            var bodyCount = overlapping.Count(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate);
            var accessoryCount = overlapping.Count(item => item.PartitionClass == BodyCandidatePartitionClass.BodyAccessoryCandidate);
            var endCount = overlapping.Count(item => item.PartitionClass == BodyCandidatePartitionClass.EndConnectionCandidate);
            var localCount = overlapping.Count(item => item.PartitionClass == BodyCandidatePartitionClass.LocalStiffenerCandidate);
            var total = Math.Max(overlapping.Length, 1);
            var endDensity = (double)endCount / total;
            var localDensity = (double)localCount / total;
            var withinTrimmedCore = midpoint >= trimMin && midpoint <= trimMax;

            StableBodyZoneKind zoneKind;
            var reasons = new List<string>();
            double confidence;

            if (!withinTrimmedCore)
            {
                zoneKind = StableBodyZoneKind.EndComplex;
                confidence = 0.90;
                reasons.Add("TRIMMED_BY_END_COMPLEXITY");
                if (endCount == 0)
                {
                    reasons.Add("LOW_STABLE_ZONE_CONFIDENCE");
                }
            }
            else if (bodyCount > 0 && endDensity <= 0.25 && localDensity <= 0.50)
            {
                zoneKind = StableBodyZoneKind.Stable;
                confidence = Math.Min(0.95, 0.65 + (0.15 * bodyCount) + (0.10 * accessoryCount));
                reasons.Add("HIGH_BODY_CANDIDATE_COVERAGE");
                if (endDensity <= 0.10)
                {
                    reasons.Add("LOW_END_CONNECTION_DENSITY");
                }

                if (localDensity <= 0.25)
                {
                    reasons.Add("LOW_LOCAL_COMPLEXITY");
                }
            }
            else if (localCount > 0 || localDensity > 0.50)
            {
                zoneKind = StableBodyZoneKind.LocalComplex;
                confidence = 0.70;
                reasons.Add("TRIMMED_BY_LOCAL_COMPLEXITY");
            }
            else
            {
                zoneKind = StableBodyZoneKind.Unstable;
                confidence = 0.45;
                reasons.Add("LOW_STABLE_ZONE_CONFIDENCE");
            }

            zones.Add(
                new StableBodyZone
                {
                    ZoneId = $"zone-{zoneIndex:000}",
                    ZoneKind = zoneKind,
                    AxisIntervalMin = Math.Round(min, 3),
                    AxisIntervalMax = Math.Round(max, 3),
                    Confidence = Math.Round(confidence, 4),
                    Reasons = reasons.Distinct().ToArray()
                });
            zoneIndex++;
        }

        return new StableBodyZoneResult
        {
            AssemblyId = partition.AssemblyId,
            InputMainPartId = partition.InputMainPartId,
            ProvisionalBodyAxis = partition.ProvisionalBodyAxis,
            ProvisionalBodyAxisSegments = partition.ProvisionalBodyAxisSegments,
            AssemblyAxisIntervalMin = Math.Round(assemblyMin, 3),
            AssemblyAxisIntervalMax = Math.Round(assemblyMax, 3),
            Zones = MergeNeighborZones(zones)
        };
    }

    private static IReadOnlyList<StableBodyZone> MergeNeighborZones(IReadOnlyList<StableBodyZone> zones)
    {
        if (zones.Count == 0)
        {
            return Array.Empty<StableBodyZone>();
        }

        var merged = new List<StableBodyZone>();
        StableBodyZone current = zones[0];

        for (var i = 1; i < zones.Count; i++)
        {
            var next = zones[i];
            if (current.ZoneKind == next.ZoneKind &&
                current.Reasons.SequenceEqual(next.Reasons) &&
                Math.Abs(current.AxisIntervalMax - next.AxisIntervalMin) <= 1e-6)
            {
                current = current with
                {
                    AxisIntervalMax = next.AxisIntervalMax,
                    Confidence = Math.Round((current.Confidence + next.Confidence) * 0.5, 4)
                };
                continue;
            }

            merged.Add(current);
            current = next;
        }

        merged.Add(current);
        return merged;
    }
}
