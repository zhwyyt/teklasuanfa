using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;
using TeklaBodyBracketRecognition.Core.Utils;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class BodyRecognizer
{
    private readonly RecognitionOptions _options;

    public BodyRecognizer(RecognitionOptions options)
    {
        _options = options;
    }

    public BodyRecognitionResult Recognize(PartGraph graph, int mainPartId)
    {
        var diagnostics = new List<string>();
        var candidates = graph.Parts.Values
            .Where(part => part.IsPlateLike && !part.IsSpecialShape && !part.IsTinyPart)
            .Where(part => !part.LikelyConnectionPart)
            .ToArray();

        if (candidates.Length == 0)
        {
            return EmptyResult();
        }

        var (axis, axisConfidence) = EstimateAxis(candidates, diagnostics);
        var chains = BuildChains(candidates, graph, axis);
        var strongChains = chains.Where(chain => chain.Persistence >= _options.StrongPersistenceMin).ToArray();
        var weakChains = chains.Where(chain => chain.Persistence >= _options.WeakPersistenceMin).ToArray();
        var bodyCs = BuildCoordinateSystem(axis, strongChains, candidates);
        var sectionVotes = VoteBodyFamily(strongChains, candidates, bodyCs.Y, diagnostics);
        var family = SelectFamily(sectionVotes);
        var (corePartIds, accessoryPartIds) = SplitCoreAndAccessories(strongChains, weakChains, family);
        var profile = BuildProfileMatch(family, corePartIds.Count);
        var chainConfidence = GeometryUtil.Clamp01(strongChains.Sum(chain => chain.ContinuityScore) / Math.Max(1, strongChains.Length));
        var sectionConfidence = GeometryUtil.Clamp01(sectionVotes.Values.DefaultIfEmpty(0).Max());
        var profileConfidence = profile.Similarity;
        var bodyConfidence = (0.25 * axisConfidence) + (0.20 * chainConfidence) + (0.35 * sectionConfidence) + (0.20 * profileConfidence);
        var reviewReasons = new List<string>();

        if (axisConfidence < _options.ManualReviewConfidenceMin)
        {
            reviewReasons.Add("BODY_AXIS_LOW_CONF");
        }

        if (sectionConfidence < _options.ManualReviewConfidenceMin)
        {
            reviewReasons.Add("BODY_SECTION_INCONSISTENT");
        }

        if (family == BodyFamily.Irregular)
        {
            reviewReasons.Add("BODY_IRREGULAR_TOPOLOGY");
        }

        if (strongChains.Length > 6)
        {
            reviewReasons.Add("BODY_CHAIN_FRAGMENTED");
        }

        diagnostics.Add($"strong_chain_count={strongChains.Length}");
        diagnostics.Add($"weak_chain_count={weakChains.Length}");

        return new BodyRecognitionResult
        {
            BodyType = BodyType.WeldedPlateBody,
            BodyFamily = family,
            ProfileMatch = profile,
            BodyAxis = axis,
            BodyCoordinateSystem = bodyCs,
            CoreBodyPartIds = corePartIds,
            BodyAccessoryPartIds = accessoryPartIds,
            AxisConfidence = axisConfidence,
            ChainConfidence = chainConfidence,
            SectionFamilyConfidence = sectionConfidence,
            ProfileMatchConfidence = profileConfidence,
            BodyConfidence = bodyConfidence,
            ReviewRequired = reviewReasons.Count > 0 || bodyConfidence < _options.ManualReviewConfidenceMin,
            ReviewReasons = reviewReasons,
            SectionFamilyVotes = sectionVotes,
            Diagnostics = diagnostics,
            VirtualPlateChains = chains
        };
    }

    private (Vector3 Axis, double Confidence) EstimateAxis(IReadOnlyList<PartFeature> candidates, List<string> diagnostics)
    {
        var weightedDirections = candidates
            .Select(part => new
            {
                Weight = part.SurfaceArea * Math.Log(part.AspectRatio + 1.0),
                Direction = part.PlateLongDirection
            })
            .OrderByDescending(item => item.Weight)
            .ToArray();

        var seed = weightedDirections.First().Direction;
        var clusterWeight = weightedDirections
            .Where(item => GeometryUtil.ParallelAngleDeg(item.Direction, seed) <= _options.AxisAngleToleranceDeg)
            .Sum(item => item.Weight);
        var totalWeight = weightedDirections.Sum(item => item.Weight);
        var clusterSupport = totalWeight <= 1e-9 ? 0 : clusterWeight / totalWeight;

        var centroidMean = new Vector3(
            candidates.Average(part => part.Centroid.X),
            candidates.Average(part => part.Centroid.Y),
            candidates.Average(part => part.Centroid.Z));
        var varianceByAxis = new[]
        {
            (Axis: Vector3.UnitX, Variance: candidates.Sum(part => Math.Pow(part.Centroid.X - centroidMean.X, 2))),
            (Axis: Vector3.UnitY, Variance: candidates.Sum(part => Math.Pow(part.Centroid.Y - centroidMean.Y, 2))),
            (Axis: Vector3.UnitZ, Variance: candidates.Sum(part => Math.Pow(part.Centroid.Z - centroidMean.Z, 2)))
        };
        var pcaAxis = varianceByAxis.OrderByDescending(item => item.Variance).First().Axis;
        var topVariance = varianceByAxis.Max(item => item.Variance);
        var secondVariance = varianceByAxis.OrderByDescending(item => item.Variance).Skip(1).First().Variance;
        var pcaSupport = topVariance <= 1e-9 ? 0 : 1.0 - (secondVariance / topVariance);

        var axis = clusterSupport >= 0.55 ? seed.Normalize() : pcaAxis;
        var confidence = GeometryUtil.Clamp01(Math.Max(clusterSupport, pcaSupport));
        diagnostics.Add($"axis_cluster_support={clusterSupport:0.###}");
        diagnostics.Add($"axis_pca_support={pcaSupport:0.###}");
        return (axis, confidence);
    }

    private IReadOnlyList<VirtualPlateChain> BuildChains(IReadOnlyList<PartFeature> candidates, PartGraph graph, Vector3 axis)
    {
        var longitudinal = candidates
            .Where(part => GeometryUtil.ParallelAngleDeg(part.PlateLongDirection, axis) <= _options.AxisAngleToleranceDeg)
            .ToArray();

        var rawLength = ComputeBodyLength(longitudinal, axis);
        var groups = new List<List<PartFeature>>();

        foreach (var part in longitudinal)
        {
            var group = groups.FirstOrDefault(existing => CanJoinGroup(part, existing, axis));
            if (group is null)
            {
                groups.Add(new List<PartFeature> { part });
            }
            else
            {
                group.Add(part);
            }
        }

        var chains = new List<VirtualPlateChain>();
        var chainIndex = 1;
        foreach (var group in groups)
        {
            var components = SplitIntoConnectedComponents(group, graph);
            foreach (var component in components)
            {
                var intervals = component.Select(part => ProjectPartInterval(part, axis)).ToArray();
                var min = intervals.Min(interval => interval.Min);
                var max = intervals.Max(interval => interval.Max);
                var coverage = rawLength <= 1e-9 ? 0 : (max - min) / rawLength;
                var thicknesses = component.Select(part => part.Thickness ?? 0).ToArray();
                var thicknessAverage = thicknesses.Length == 0 ? 1.0 : Math.Max(1.0, thicknesses.Average());
                var continuity = GeometryUtil.Clamp01(
                    coverage *
                    component.Average(part => 1.0 - (GeometryUtil.ParallelAngleDeg(part.PlateLongDirection, axis) / 90.0)) *
                    (1.0 - StandardDeviation(thicknesses) / thicknessAverage));

                chains.Add(new VirtualPlateChain
                {
                    ChainId = $"CHAIN-{chainIndex++}",
                    PartIds = component.Select(part => part.PartId).OrderBy(id => id).ToArray(),
                    MeanNormal = AverageVector(component.Select(part => part.PlateNormal)),
                    MeanLongDirection = AverageVector(component.Select(part => part.PlateLongDirection)),
                    MeanTransverseLocation = component.Average(part => part.Centroid.Dot(axis.Cross(part.PlateNormal).Normalize())),
                    ThicknessStats = (thicknesses.Min(), thicknesses.Max()),
                    UnionIntervalX = (min, max),
                    ContinuityScore = continuity,
                    Persistence = coverage,
                    IsLongitudinalChain = coverage >= _options.WeakPersistenceMin
                });
            }
        }

        return chains.OrderByDescending(chain => chain.Persistence).ToArray();
    }

    private bool CanJoinGroup(PartFeature part, List<PartFeature> group, Vector3 axis)
    {
        return group.Any(candidate =>
        {
            var normalClose = GeometryUtil.ParallelAngleDeg(part.PlateNormal, candidate.PlateNormal) <= _options.NormalParallelToleranceDeg;
            var thicknessClose = Math.Abs((part.Thickness ?? 0) - (candidate.Thickness ?? 0)) <= 2.0;
            var gap = GeometryUtil.IntervalGap(ProjectPartInterval(part, axis), ProjectPartInterval(candidate, axis));
            var transverseGap = Math.Abs(part.Centroid.Dot(candidate.PlateNormal) - candidate.Centroid.Dot(candidate.PlateNormal));
            return normalClose && thicknessClose && gap <= _options.ChainGapToleranceMm && transverseGap <= 50.0;
        });
    }

    private static IReadOnlyList<IReadOnlyList<PartFeature>> SplitIntoConnectedComponents(IReadOnlyList<PartFeature> group, PartGraph graph)
    {
        var remaining = group.ToDictionary(part => part.PartId);
        var components = new List<IReadOnlyList<PartFeature>>();

        while (remaining.Count > 0)
        {
            var seed = remaining.Keys.First();
            var queue = new Queue<int>();
            var component = new List<PartFeature>();
            queue.Enqueue(seed);
            remaining.Remove(seed);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var feature = group.First(part => part.PartId == current);
                component.Add(feature);
                foreach (var neighbor in graph.GetNeighbors(current, EdgeType.Weld, EdgeType.Contact, EdgeType.Boolean))
                {
                    if (remaining.Remove(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            components.Add(component);
        }

        return components;
    }

    private static (Vector3 X, Vector3 Y, Vector3 Z) BuildCoordinateSystem(Vector3 axis, IReadOnlyList<VirtualPlateChain> strongChains, IReadOnlyList<PartFeature> candidates)
    {
        var y = strongChains.Count > 0
            ? AverageVector(strongChains.Select(chain => chain.MeanNormal)).ProjectToPlane(axis).Normalize()
            : AverageVector(candidates.Select(part => part.PlateNormal)).ProjectToPlane(axis).Normalize();
        if (y.Length <= 1e-9)
        {
            y = Math.Abs(axis.Dot(Vector3.UnitY)) < 0.9 ? Vector3.UnitY : Vector3.UnitZ;
            y = y.ProjectToPlane(axis).Normalize();
        }

        var z = axis.Cross(y).Normalize();
        return (axis.Normalize(), y, z);
    }

    private IReadOnlyDictionary<string, double> VoteBodyFamily(IReadOnlyList<VirtualPlateChain> strongChains, IReadOnlyList<PartFeature> candidates, Vector3 transverseAxis, List<string> diagnostics)
    {
        var source = strongChains.Count > 0
            ? strongChains
            : candidates.Select(part => new VirtualPlateChain
            {
                ChainId = $"PART-{part.PartId}",
                PartIds = new[] { part.PartId },
                MeanNormal = part.PlateNormal,
                MeanLongDirection = part.PlateLongDirection,
                MeanTransverseLocation = part.Centroid.Dot(transverseAxis),
                ThicknessStats = (part.Thickness ?? 0, part.Thickness ?? 0),
                UnionIntervalX = (0, 0),
                ContinuityScore = 0.3,
                Persistence = 0.1,
                IsLongitudinalChain = false
            }).ToArray();

        var normalGroups = GroupParallelNormals(source.Select(chain => chain.MeanNormal));
        var groupCounts = normalGroups
            .Select(group => new
            {
                Count = group.Count,
                TotalPersistence = source.Where(chain => group.Any(normal => GeometryUtil.ParallelAngleDeg(chain.MeanNormal, normal) <= _options.NormalParallelToleranceDeg)).Sum(chain => chain.Persistence)
            })
            .OrderByDescending(item => item.Count)
            .ThenByDescending(item => item.TotalPersistence)
            .ToArray();

        var majorGroupCount = groupCounts.ElementAtOrDefault(0)?.Count ?? 0;
        var secondaryGroupCount = groupCounts.ElementAtOrDefault(1)?.Count ?? 0;
        var tertiaryGroupCount = groupCounts.ElementAtOrDefault(2)?.Count ?? 0;

        var voteH = majorGroupCount >= 2 && secondaryGroupCount >= 1 ? 0.90 : 0.20;
        var voteBox = majorGroupCount >= 2 && secondaryGroupCount >= 2 ? 0.82 : 0.15;
        var voteT = majorGroupCount >= 1 && secondaryGroupCount == 1 && source.Count <= 3 ? 0.72 : 0.10;
        var voteCross = majorGroupCount >= 2 && secondaryGroupCount >= 2 && tertiaryGroupCount == 0 ? 0.60 : 0.05;
        if (source.Count >= 4 && voteH > 0.8)
        {
            voteBox -= 0.12;
        }

        var votes = new Dictionary<string, double>
        {
            [BodyFamily.BuiltUpH.ToString()] = voteH,
            [BodyFamily.BuiltUpBox.ToString()] = voteBox,
            [BodyFamily.BuiltUpT.ToString()] = voteT,
            [BodyFamily.BuiltUpCross.ToString()] = voteCross,
            [BodyFamily.Irregular.ToString()] = 1.0 - new[] { voteH, voteBox, voteT, voteCross }.Max()
        };
        diagnostics.Add($"family_votes={string.Join(", ", votes.Select(kv => $"{kv.Key}:{kv.Value:0.##}"))}");
        return votes;
    }

    private IReadOnlyList<IReadOnlyList<Vector3>> GroupParallelNormals(IEnumerable<Vector3> normals)
    {
        var groups = new List<List<Vector3>>();
        foreach (var normal in normals)
        {
            var group = groups.FirstOrDefault(existing => existing.Any(seed => GeometryUtil.ParallelAngleDeg(seed, normal) <= _options.NormalParallelToleranceDeg));
            if (group is null)
            {
                groups.Add(new List<Vector3> { normal });
            }
            else
            {
                group.Add(normal);
            }
        }

        return groups;
    }

    private static BodyFamily SelectFamily(IReadOnlyDictionary<string, double> votes)
    {
        var winner = votes.OrderByDescending(kv => kv.Value).First().Key;
        return Enum.TryParse<BodyFamily>(winner, out var family) ? family : BodyFamily.Unknown;
    }

    private static (IReadOnlyList<int> CorePartIds, IReadOnlyList<int> AccessoryPartIds) SplitCoreAndAccessories(IReadOnlyList<VirtualPlateChain> strongChains, IReadOnlyList<VirtualPlateChain> weakChains, BodyFamily family)
    {
        var takeStrong = family switch
        {
            BodyFamily.BuiltUpH => 3,
            BodyFamily.BuiltUpBox => 4,
            BodyFamily.BuiltUpT => 2,
            BodyFamily.BuiltUpCross => 4,
            _ => Math.Min(4, strongChains.Count)
        };

        var core = strongChains
            .OrderByDescending(chain => chain.Persistence)
            .ThenByDescending(chain => chain.ContinuityScore)
            .Take(takeStrong)
            .SelectMany(chain => chain.PartIds)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        var accessory = weakChains
            .SelectMany(chain => chain.PartIds)
            .Distinct()
            .Except(core)
            .OrderBy(id => id)
            .ToArray();

        return (core, accessory);
    }

    private static ProfileMatchResult BuildProfileMatch(BodyFamily family, int coreCount)
    {
        return family switch
        {
            BodyFamily.BuiltUpH => new ProfileMatchResult(Domain.MatchType.BuiltUp, coreCount == 3 ? "BUILTUP_H" : "BUILTUP_H_VARIANT", 0.88),
            BodyFamily.BuiltUpBox => new ProfileMatchResult(Domain.MatchType.BuiltUp, coreCount >= 4 ? "BUILTUP_BOX" : "BUILTUP_BOX_VARIANT", 0.84),
            BodyFamily.BuiltUpT => new ProfileMatchResult(Domain.MatchType.BuiltUp, "BUILTUP_T", 0.82),
            BodyFamily.BuiltUpCross => new ProfileMatchResult(Domain.MatchType.BuiltUp, "BUILTUP_CROSS", 0.78),
            BodyFamily.Irregular => new ProfileMatchResult(Domain.MatchType.Irregular, null, 0.45),
            _ => new ProfileMatchResult(Domain.MatchType.None, null, 0.10)
        };
    }

    private static (double Min, double Max) ProjectPartInterval(PartFeature part, Vector3 axis)
    {
        var center = GeometryUtil.ProjectScalar(part.Centroid, axis);
        var span = Math.Abs(axis.Normalize().Dot(part.PlateLongDirection.Normalize())) * Math.Max(part.ObbDims.X, Math.Max(part.ObbDims.Y, part.ObbDims.Z));
        var half = span / 2.0;
        return (center - half, center + half);
    }

    private static double ComputeBodyLength(IReadOnlyList<PartFeature> parts, Vector3 axis)
    {
        if (parts.Count == 0)
        {
            return 0;
        }

        var intervals = parts.Select(part => ProjectPartInterval(part, axis)).ToArray();
        return intervals.Max(item => item.Max) - intervals.Min(item => item.Min);
    }

    private static Vector3 AverageVector(IEnumerable<Vector3> vectors)
    {
        var materialized = vectors.ToArray();
        if (materialized.Length == 0)
        {
            return Vector3.Zero;
        }

        var seed = materialized[0];
        var sum = Vector3.Zero;
        foreach (var vector in materialized)
        {
            sum += GeometryUtil.AlignDirection(vector, seed);
        }

        return sum.Normalize();
    }

    private static double StandardDeviation(IReadOnlyList<double> values)
    {
        if (values.Count <= 1)
        {
            return 0;
        }

        var average = values.Average();
        var variance = values.Average(value => Math.Pow(value - average, 2));
        return Math.Sqrt(variance);
    }

    private static BodyRecognitionResult EmptyResult() =>
        new()
        {
            BodyType = BodyType.Unknown,
            BodyFamily = BodyFamily.Unknown,
            ProfileMatch = new ProfileMatchResult(Domain.MatchType.None, null, 0.0),
            BodyAxis = Vector3.UnitX,
            BodyCoordinateSystem = (Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ),
            CoreBodyPartIds = Array.Empty<int>(),
            BodyAccessoryPartIds = Array.Empty<int>(),
            AxisConfidence = 0,
            ChainConfidence = 0,
            SectionFamilyConfidence = 0,
            ProfileMatchConfidence = 0,
            BodyConfidence = 0,
            ReviewRequired = true,
            ReviewReasons = new[] { "BODY_AXIS_LOW_CONF", "BODY_SECTION_INCONSISTENT" },
            SectionFamilyVotes = new Dictionary<string, double>(),
            Diagnostics = new[] { "no-eligible-plate-like-parts" },
            VirtualPlateChains = Array.Empty<VirtualPlateChain>()
        };
}
