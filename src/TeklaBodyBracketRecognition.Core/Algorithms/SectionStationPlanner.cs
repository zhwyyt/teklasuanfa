using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class SectionStationPlanner
{
    private readonly RecognitionOptions _options;

    public SectionStationPlanner(RecognitionOptions options)
    {
        _options = options;
    }

    public SectionStationPlanResult Plan(StableBodyZoneResult stableZones)
    {
        var stable = stableZones.Zones
            .Where(zone => zone.ZoneKind == StableBodyZoneKind.Stable)
            .OrderByDescending(zone => zone.AxisIntervalMax - zone.AxisIntervalMin)
            .ToArray();
        var fallbackZones = stableZones.Zones
            .Where(zone => zone.ZoneKind == StableBodyZoneKind.LocalComplex)
            .OrderByDescending(zone => zone.AxisIntervalMax - zone.AxisIntervalMin)
            .ToArray();

        var stations = new List<SectionStation>();
        var zoneSource = stable.Length > 0 ? stable : fallbackZones;
        var targetCount = Math.Clamp(zoneSource.Length * 3, _options.SectionSampleCountMin, _options.SectionSampleCountMax);

        foreach (var zone in zoneSource)
        {
            var span = Math.Max(zone.AxisIntervalMax - zone.AxisIntervalMin, 1.0);
            var zoneTarget = span switch
            {
                <= 200 => 2,
                <= 1000 => 3,
                _ => 5
            };

            AddZoneStations(stations, zone, zoneTarget, stable.Length == 0);
        }

        if (stations.Count > targetCount)
        {
            stations = stations
                .OrderBy(station => station.StationKind)
                .ThenBy(station => station.AxisPosition)
                .Take(targetCount)
                .ToList();
        }

        if (stations.Count < _options.SectionSampleCountMin && zoneSource.Length > 0)
        {
            var longest = zoneSource[0];
            while (stations.Count < _options.SectionSampleCountMin)
            {
                var ratio = (stations.Count + 1.0) / (_options.SectionSampleCountMin + 1.0);
                stations.Add(
                    new SectionStation
                    {
                        StationId = $"station-{stations.Count + 1:000}",
                        ZoneId = longest.ZoneId,
                        AxisPosition = Math.Round(longest.AxisIntervalMin + ((longest.AxisIntervalMax - longest.AxisIntervalMin) * ratio), 3),
                        StationKind = stable.Length > 0 ? SectionStationKind.LowPriority : SectionStationKind.Transition,
                        StationReasons = stable.Length > 0
                            ? new[] { "LOW_PRIORITY_COMPLEX_ZONE_SAMPLE" }
                            : new[] { "LOW_PRIORITY_COMPLEX_ZONE_SAMPLE", "NEAR_TRANSITION_BOUNDARY" }
                    });
            }
        }

        return new SectionStationPlanResult
        {
            AssemblyId = stableZones.AssemblyId,
            InputMainPartId = stableZones.InputMainPartId,
            Stations = stations
                .GroupBy(station => $"{station.ZoneId}|{station.AxisPosition:0.###}")
                .Select(group => group.First())
                .OrderBy(station => station.AxisPosition)
                .Select(
                    (station, index) => station with
                    {
                        StationId = $"station-{index + 1:000}"
                    })
                .ToArray()
        };
    }

    private static void AddZoneStations(List<SectionStation> stations, StableBodyZone zone, int requestedCount, bool isFallbackZone)
    {
        var count = Math.Max(requestedCount, 1);
        var span = zone.AxisIntervalMax - zone.AxisIntervalMin;

        if (count == 1 || span <= 1e-6)
        {
            stations.Add(
                new SectionStation
                {
                    StationId = $"station-{stations.Count + 1:000}",
                    ZoneId = zone.ZoneId,
                    AxisPosition = Math.Round((zone.AxisIntervalMin + zone.AxisIntervalMax) * 0.5, 3),
                    StationKind = isFallbackZone ? SectionStationKind.LowPriority : SectionStationKind.Core,
                    StationReasons = isFallbackZone
                        ? new[] { "LOW_PRIORITY_COMPLEX_ZONE_SAMPLE", "ZONE_CENTER_PRIORITY" }
                        : new[] { "ZONE_CENTER_PRIORITY" }
                });
            return;
        }

        for (var index = 0; index < count; index++)
        {
            var ratio = (index + 1.0) / (count + 1.0);
            var position = zone.AxisIntervalMin + (span * ratio);
            var kind = isFallbackZone
                ? SectionStationKind.LowPriority
                : (index == (count / 2) ? SectionStationKind.Core : SectionStationKind.Transition);
            var reasons = isFallbackZone
                ? new[] { "LOW_PRIORITY_COMPLEX_ZONE_SAMPLE", "UNIFORM_ZONE_SAMPLE" }
                : kind == SectionStationKind.Core
                    ? new[] { "ZONE_CENTER_PRIORITY", "UNIFORM_ZONE_SAMPLE" }
                    : new[] { "UNIFORM_ZONE_SAMPLE" };

            stations.Add(
                new SectionStation
                {
                    StationId = $"station-{stations.Count + 1:000}",
                    ZoneId = zone.ZoneId,
                    AxisPosition = Math.Round(position, 3),
                    StationKind = kind,
                    StationReasons = reasons
                });
        }
    }
}
