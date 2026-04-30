using System.Text.Json;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class NewInputLayerDraftCollector
{
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string RepresentationKindPathSectionSweep = "PATH_SECTION_SWEEP";
    private const string RepresentationKindPlateBoundaryThickness = "PLATE_BOUNDARY_THICKNESS";
    private const string RepresentationKindApproxPathSectionSweep = "APPROX_PATH_SECTION_SWEEP";
    private const string RepresentationKindApproxBoundaryThickness = "APPROX_BOUNDARY_THICKNESS";
    private const string RepresentationKindTraceOnlyFallback = "TRACE_ONLY_FALLBACK";

    public static List<NewInputLayerDraftMemberRow> Collect(BatchArtifacts artifacts)
    {
        var cache = new Dictionary<string, XingcaiMemberSnapshot?>(StringComparer.OrdinalIgnoreCase);
        var sidecarCache = new Dictionary<string, XingcaiInputLayerDraftMemberSnapshot?>(StringComparer.OrdinalIgnoreCase);
        var rows = new List<NewInputLayerDraftMemberRow>();

        foreach (var summary in artifacts.BodyMaterialSummaries.OrderBy(item => item.MemberId, StringComparer.Ordinal))
        {
            var snapshot = LoadSnapshot(summary.SourceFile, cache);
            if (snapshot is null)
            {
                continue;
            }

            var draftSnapshot = LoadDraftSnapshot(summary.SourceFile, sidecarCache);
            rows.Add(BuildMemberRow(summary, snapshot, draftSnapshot));
        }

        return rows;
    }

    private static NewInputLayerDraftMemberRow BuildMemberRow(
        BodyMaterialSummary summary,
        XingcaiMemberSnapshot snapshot,
        XingcaiInputLayerDraftMemberSnapshot? draftSnapshot)
    {
        var path = BuildLongitudinalPath(snapshot);
        var draftPartLookup = BuildDraftPartLookup(draftSnapshot);
        var parts = snapshot.Parts
            .Select(part => BuildPartRow(part, snapshot, summary, ResolveDraftPart(part, draftPartLookup)))
            .OrderBy(item => item.PartId)
            .ToList();
        var warnings = path.WarningCodes
            .Concat(parts.SelectMany(item => item.NormalizationWarnings))
            .Concat(draftSnapshot?.ExportDiagnostics ?? Enumerable.Empty<string>())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new NewInputLayerDraftMemberRow
        {
            AssemblyId = summary.AssemblyId,
            MemberId = summary.MemberId,
            MemberName = snapshot.Member.MemberId,
            Profile = summary.MemberProfileString,
            Material = snapshot.Member.Material,
            SourceFileName = summary.SourceFile,
            RawMember = new NewInputLayerDraftRawMember
            {
                LongitudinalAxisKind = snapshot.LongitudinalAxisKind ?? string.Empty,
                LongitudinalAxisConfidence = snapshot.LongitudinalAxisConfidence,
                LongitudinalAxisSource = snapshot.LongitudinalAxisSource ?? string.Empty,
                GuidePolylinePointCount = snapshot.GuidePolyline?.Count ?? 0,
                AxisSegmentCount = snapshot.AxisSegments?.Count ?? 0,
                MainAxisLength = snapshot.Member.MainAxis?.Length,
                BoundingBox = ToBoundingBox(snapshot.Member.BoundingBox)
            },
            NormalizedLongitudinalPath = path,
            NormalizedParts = parts,
            StationQueryContexts = BuildStationQueryContexts(path),
            NormalizationWarnings = warnings
        };
    }

    private static NewInputLayerDraftLongitudinalPath BuildLongitudinalPath(XingcaiMemberSnapshot snapshot)
    {
        var warningCodes = new List<string>();
        var supportEvidence = new List<string>();

        if (snapshot.AxisSegments is { Count: > 0 })
        {
            var segments = snapshot.AxisSegments
                .Select(
                    item => new NewInputLayerDraftPathSegment
                    {
                        SegmentIndex = item.Index ?? 0,
                        StartPoint = ToVector3(item.Start),
                        EndPoint = ToVector3(item.End),
                        Direction = ToVector3(item.Direction).Normalize(),
                        PathStart = item.CumulativeStart,
                        PathEnd = item.CumulativeEnd,
                        Length = Math.Max(0d, item.CumulativeEnd - item.CumulativeStart),
                        SourceKind = "AXIS_SEGMENT",
                        SourceReference = $"segment:{item.Index ?? 0}"
                    })
                .Where(item => item.Length > 1e-6)
                .OrderBy(item => item.SegmentIndex)
                .ToList();

            if (HasSegmentGap(segments))
            {
                warningCodes.Add("PATH_SEGMENTS_GAPPED");
            }

            supportEvidence.Add("IMPORTED_AXIS_SEGMENTS");
            return new NewInputLayerDraftLongitudinalPath
            {
                PathModelType = "RAW_AXIS_SEGMENTS",
                PathSegments = segments,
                PathLength = segments.Sum(item => item.Length),
                PathSourceKind = snapshot.LongitudinalAxisSource ?? "IMPORTED_AXIS_SEGMENTS",
                PathConfidence = snapshot.LongitudinalAxisConfidence,
                FallbackUsed = false,
                SupportEvidence = supportEvidence,
                WarningCodes = warningCodes
            };
        }

        if (snapshot.GuidePolyline is { Count: > 1 })
        {
            var points = snapshot.GuidePolyline.Select(ToVector3).ToArray();
            var segments = new List<NewInputLayerDraftPathSegment>();
            var cumulative = 0d;
            for (var i = 0; i < points.Length - 1; i++)
            {
                var start = points[i];
                var end = points[i + 1];
                var length = (end - start).Length;
                if (length <= 1e-6)
                {
                    continue;
                }

                segments.Add(
                    new NewInputLayerDraftPathSegment
                    {
                        SegmentIndex = i,
                        StartPoint = start,
                        EndPoint = end,
                        Direction = (end - start).Normalize(),
                        PathStart = cumulative,
                        PathEnd = cumulative + length,
                        Length = length,
                        SourceKind = "GUIDE_POLYLINE",
                        SourceReference = $"polyline:{i}"
                    });
                cumulative += length;
            }

            supportEvidence.Add("GUIDE_POLYLINE");
            warningCodes.Add("FALLBACK_USED");
            return new NewInputLayerDraftLongitudinalPath
            {
                PathModelType = "GUIDE_POLYLINE_RECONSTRUCTED",
                PathSegments = segments,
                PathLength = segments.Sum(item => item.Length),
                PathSourceKind = "GUIDE_POLYLINE",
                PathConfidence = snapshot.LongitudinalAxisConfidence,
                FallbackUsed = true,
                SupportEvidence = supportEvidence,
                WarningCodes = warningCodes
            };
        }

        if (snapshot.Member.MainAxis?.Direction is not null && snapshot.Member.MainAxis.Length > 1e-6)
        {
            var origin = ToBoundingBox(snapshot.Member.BoundingBox).Center;
            var direction = ToVector3(snapshot.Member.MainAxis.Direction).Normalize();
            var length = snapshot.Member.MainAxis.Length;
            warningCodes.Add("FALLBACK_USED");
            warningCodes.Add("PATH_FALLBACK_TO_MAIN_AXIS");
            supportEvidence.Add("MEMBER_MAIN_AXIS");
            return new NewInputLayerDraftLongitudinalPath
            {
                PathModelType = "MAIN_AXIS_FALLBACK",
                PathSegments = new List<NewInputLayerDraftPathSegment>
                {
                    new()
                    {
                        SegmentIndex = 0,
                        StartPoint = origin,
                        EndPoint = origin + (direction * length),
                        Direction = direction,
                        PathStart = 0d,
                        PathEnd = length,
                        Length = length,
                        SourceKind = "MAIN_AXIS",
                        SourceReference = "member.main-axis"
                    }
                },
                PathLength = length,
                PathSourceKind = "MAIN_AXIS",
                PathConfidence = 0.50d,
                FallbackUsed = true,
                SupportEvidence = supportEvidence,
                WarningCodes = warningCodes
            };
        }

        return new NewInputLayerDraftLongitudinalPath
        {
            PathModelType = "SYNTHETIC_PATH",
            PathSourceKind = "NONE",
            PathConfidence = 0d,
            FallbackUsed = true,
            SupportEvidence = new List<string> { "NO_PATH_SOURCE" },
            WarningCodes = new List<string> { "RAW_INPUT_MISSING", "LOW_CONFIDENCE_NORMALIZATION" }
        };
    }

    private static NewInputLayerDraftPartRow BuildPartRow(
        XingcaiPartSnapshot part,
        XingcaiMemberSnapshot snapshot,
        BodyMaterialSummary summary,
        XingcaiInputLayerDraftPartSnapshot? draftPart)
    {
        var partType = part.PartType ?? string.Empty;
        var modelType = draftPart?.PartModelType ?? ResolvePartModelType(part);
        var warnings = new List<string>(draftPart?.WarningCodes ?? Enumerable.Empty<string>());
        var rawThickness = draftPart?.RawGeometry?.LegacyThickness
            ?? (part.Thickness > 1e-6 ? part.Thickness : (double?)null);
        var inferredThickness = draftPart?.RawGeometry?.NormalizedThickness
            ?? rawThickness
            ?? InferThickness(ToBoundingBox(part.BoundingBox).Size);
        if (draftPart is null && !rawThickness.HasValue && inferredThickness.HasValue)
        {
            warnings.Add("THICKNESS_INFERRED_FROM_SIZE");
        }

        if (modelType == "UNCLASSIFIED_SOLID_SUMMARY")
        {
            warnings.Add("PART_MODEL_TYPE_UNCLASSIFIED");
        }

        var localX = ToVector3(part.LocalCoordinateSystem?.X).Normalize();
        var localY = ToVector3(part.LocalCoordinateSystem?.Y).Normalize();
        var localZ = ToVector3(part.LocalCoordinateSystem?.Z).Normalize();
        if (localZ.Length <= 1e-9 && localX.Length > 1e-9 && localY.Length > 1e-9)
        {
            localZ = localX.Cross(localY).Normalize();
            warnings.Add("LOCAL_FRAME_REPAIRED");
        }

        var boundaryLoopKind = draftPart?.NormalizedShape?.BoundaryLoopKind
            ?? (string.Equals(modelType, "PLATE_BOUNDARY_THICKNESS", StringComparison.Ordinal) ? "APPROX_OUTER_LOOP" : "UNRESOLVED");
        var boundaryResolved = !string.IsNullOrWhiteSpace(boundaryLoopKind) &&
            !string.Equals(boundaryLoopKind, "UNRESOLVED", StringComparison.OrdinalIgnoreCase);
        if (draftPart is null && !boundaryResolved)
        {
            warnings.Add("BOUNDARY_UNRESOLVED");
        }

        var representation = ResolveRepresentation(part, summary, draftPart, modelType, snapshot, warnings, boundaryResolved, inferredThickness);

        var partId = ParseInt(part.PartId);
        var roleHint = snapshot.Classification?.PartRoles
            .Where(item => ParseInt(item.PartId) == partId)
            .OrderByDescending(item => item.Score)
            .FirstOrDefault();

        return new NewInputLayerDraftPartRow
        {
            PartId = partId,
            PartName = part.Name ?? string.Empty,
            PartType = partType,
            ProfileString = part.ProfileString ?? string.Empty,
            Material = part.Material ?? string.Empty,
            RawGeometry = new NewInputLayerDraftRawGeometry
            {
                BoundingBox = ToBoundingBox(part.BoundingBox),
                Centroid = ToVector3(part.Centroid),
                LocalAxisX = localX,
                LocalAxisY = localY,
                LocalAxisZ = localZ,
                Thickness = rawThickness,
                ThicknessSource = draftPart?.RawGeometry?.ThicknessSource ?? string.Empty,
                ThicknessSourceDetail = draftPart?.RawGeometry?.ThicknessSourceDetail ?? string.Empty,
                AxisProjectionLength = draftPart?.LongitudinalProjection?.Length ?? part.AxisProjection?.Length,
                SolidEdgeCount = part.SolidEdges?.Count ?? 0,
                NearMemberStart = part.EndProximity?.NearStart == true,
                NearMemberEnd = part.EndProximity?.NearEnd == true,
                OuterSideCandidate = part.GeometryHints?.OuterSideCandidate == true,
                Planarity = part.GeometryHints?.Planarity
            },
            PartModelType = modelType,
            Representation = representation,
            NormalizedShape = new NewInputLayerDraftNormalizedShape
            {
                ReferenceOrigin = ToVector3(part.Centroid),
                LongDirection = ResolveLongDirection(part, snapshot),
                NormalDirection = ResolveNormalDirection(part, localZ),
                WidthDirection = ResolveWidthDirection(part, snapshot),
                Thickness = inferredThickness,
                BoundaryLoopKind = boundaryLoopKind,
                BoundarySource = draftPart?.NormalizedShape?.BoundarySource ?? (boundaryResolved ? "LEGACY_INFERRED" : string.Empty),
                BoundaryPointCount = draftPart?.NormalizedShape?.BoundaryPoints?.Count
                    ?? (boundaryResolved ? Math.Max(4, part.SolidEdges?.Count ?? 0) : 0),
                BoundaryClosed = draftPart?.NormalizedShape?.BoundaryClosed ?? boundaryResolved,
                SweepProfile = modelType == "PATH_SECTION_SWEEP" ? ResolveSweepProfile(part, summary) : string.Empty,
                SweepPathReference = modelType == "PATH_SECTION_SWEEP" ? "member.normalized-path" : string.Empty,
                SectionSource = modelType == "PATH_SECTION_SWEEP" ? ResolveSectionSource(part, draftPart, summary) : string.Empty,
                HasExplicitBoundary = boundaryResolved,
                HasExplicitThickness = inferredThickness.HasValue,
                HasExplicitSection = modelType == "PATH_SECTION_SWEEP" && !string.IsNullOrWhiteSpace(ResolveSweepProfile(part, summary)),
                ModelConfidence = ResolveModelConfidence(modelType, warnings)
            },
            LongitudinalProjection = new NewInputLayerDraftLongitudinalProjection
            {
                ProjectionStart = draftPart?.LongitudinalProjection?.Start ?? part.AxisProjection?.Start,
                ProjectionEnd = draftPart?.LongitudinalProjection?.End ?? part.AxisProjection?.End,
                ProjectionLength = draftPart?.LongitudinalProjection?.Length ?? part.AxisProjection?.Length,
                CoverageRatio = draftPart?.LongitudinalProjection?.CoverageRatio ?? part.AxisProjection?.CoverageRatio,
                ProjectionConfidence = (draftPart?.LongitudinalProjection?.Length ?? part.AxisProjection?.Length) > 1e-6 ? 0.85d : 0.35d
            },
            SemanticHints = new NewInputLayerDraftSemanticHints
            {
                ImportedRoleHint = roleHint?.Role ?? string.Empty,
                ImportedRoleScore = roleHint?.Score ?? 0d,
                SourceMainPartHint = partId == summary.SourceMainPartId,
                OuterSideHint = part.GeometryHints?.OuterSideCandidate == true
            },
            NormalizationWarnings = warnings.Distinct(StringComparer.Ordinal).ToList()
        };
    }

    private static NewInputLayerDraftRepresentation ResolveRepresentation(
        XingcaiPartSnapshot part,
        BodyMaterialSummary summary,
        XingcaiInputLayerDraftPartSnapshot? draftPart,
        string modelType,
        XingcaiMemberSnapshot snapshot,
        IReadOnlyCollection<string> warnings,
        bool boundaryResolved,
        double? inferredThickness)
    {
        var draftKind = NormalizeCode(draftPart?.RepresentationKind);
        var draftLevel = NormalizeCode(draftPart?.RepresentationLevel);
        var draftRisk = NormalizeCode(draftPart?.DistortionRiskCode);
        var degradationReasons = new List<string>(draftPart?.DegradationReasonCodes ?? Enumerable.Empty<string>());
        var riskReasons = new List<string>(draftPart?.DistortionRiskReasons ?? Enumerable.Empty<string>());
        var pathFallbackUsed = snapshot.AxisSegments is null || snapshot.AxisSegments.Count == 0;
        var hasSweepSection = !string.IsNullOrWhiteSpace(ResolveSweepProfile(part, summary));
        var hasThickness = inferredThickness.HasValue;
        var explicitBoundary = boundaryResolved;
        var pathModel = string.Equals(modelType, "PATH_SECTION_SWEEP", StringComparison.Ordinal);
        var plateModel = string.Equals(modelType, "PLATE_BOUNDARY_THICKNESS", StringComparison.Ordinal);

        string representationKind;
        string representationLevel;
        string distortionRiskCode;

        if (!string.IsNullOrWhiteSpace(draftKind))
        {
            representationKind = draftKind;
            representationLevel = string.IsNullOrWhiteSpace(draftLevel) ? InferRepresentationLevelFromKind(draftKind) : draftLevel;
            distortionRiskCode = string.IsNullOrWhiteSpace(draftRisk) ? InferRiskFromLevel(representationLevel) : draftRisk;
        }
        else if (pathModel)
        {
            if (hasSweepSection && !pathFallbackUsed)
            {
                representationKind = RepresentationKindPathSectionSweep;
                representationLevel = "EXACT";
                distortionRiskCode = "LOW";
            }
            else if (hasSweepSection)
            {
                representationKind = RepresentationKindApproxPathSectionSweep;
                representationLevel = "APPROXIMATE";
                distortionRiskCode = "MEDIUM";
                degradationReasons.Add("PATH_FALLBACK_USED");
            }
            else
            {
                representationKind = RepresentationKindTraceOnlyFallback;
                representationLevel = "FALLBACK";
                distortionRiskCode = "HIGH";
                degradationReasons.Add("SECTION_PROFILE_MISSING");
            }
        }
        else if (plateModel)
        {
            if (explicitBoundary && hasThickness)
            {
                representationKind = RepresentationKindPlateBoundaryThickness;
                representationLevel = warnings.Contains("THICKNESS_INFERRED_FROM_SIZE", StringComparer.Ordinal) ? "NEAR_EXACT" : "EXACT";
                distortionRiskCode = warnings.Contains("THICKNESS_INFERRED_FROM_SIZE", StringComparer.Ordinal) ? "MEDIUM" : "LOW";
            }
            else if (explicitBoundary || hasThickness)
            {
                representationKind = RepresentationKindApproxBoundaryThickness;
                representationLevel = "APPROXIMATE";
                distortionRiskCode = "MEDIUM";
                if (!explicitBoundary)
                {
                    degradationReasons.Add("BOUNDARY_MISSING");
                }

                if (!hasThickness)
                {
                    degradationReasons.Add("THICKNESS_MISSING");
                }
            }
            else
            {
                representationKind = RepresentationKindTraceOnlyFallback;
                representationLevel = "FALLBACK";
                distortionRiskCode = "HIGH";
                degradationReasons.Add("BOUNDARY_AND_THICKNESS_MISSING");
            }
        }
        else
        {
            representationKind = RepresentationKindTraceOnlyFallback;
            representationLevel = "FALLBACK";
            distortionRiskCode = "HIGH";
            degradationReasons.Add("UNCLASSIFIED_SOLID_ONLY");
        }

        foreach (var warning in warnings)
        {
            switch (warning)
            {
                case "BOUNDARY_UNRESOLVED":
                    AddReason(riskReasons, "BOUNDARY_UNRESOLVED");
                    AddReason(degradationReasons, "BOUNDARY_UNRESOLVED");
                    distortionRiskCode = PromoteRisk(distortionRiskCode, "HIGH");
                    break;
                case "THICKNESS_INFERRED_FROM_SIZE":
                    AddReason(riskReasons, "THICKNESS_INFERRED");
                    distortionRiskCode = PromoteRisk(distortionRiskCode, "MEDIUM");
                    break;
                case "PATH_FALLBACK_TO_MAIN_AXIS":
                case "FALLBACK_USED":
                    AddReason(riskReasons, "PATH_FALLBACK_USED");
                    distortionRiskCode = PromoteRisk(distortionRiskCode, "MEDIUM");
                    break;
                case "PART_MODEL_TYPE_UNCLASSIFIED":
                    AddReason(riskReasons, "MODEL_UNCLASSIFIED");
                    distortionRiskCode = PromoteRisk(distortionRiskCode, "HIGH");
                    break;
            }
        }

        return new NewInputLayerDraftRepresentation
        {
            RepresentationKind = representationKind,
            RepresentationKindLabelZh = ToRepresentationKindLabelZh(representationKind),
            RepresentationLevel = representationLevel,
            RepresentationLevelLabelZh = ToRepresentationLevelLabelZh(representationLevel),
            DistortionRiskCode = distortionRiskCode,
            DistortionRiskLabelZh = ToDistortionRiskLabelZh(distortionRiskCode),
            ExactGeometryAvailable = string.Equals(representationLevel, "EXACT", StringComparison.Ordinal) ||
                                     string.Equals(representationLevel, "NEAR_EXACT", StringComparison.Ordinal),
            ApproximationUsed = string.Equals(representationLevel, "APPROXIMATE", StringComparison.Ordinal) ||
                                string.Equals(representationLevel, "FALLBACK", StringComparison.Ordinal),
            DistortionRiskReasons = riskReasons.Distinct(StringComparer.Ordinal).ToList(),
            DegradationReasonCodes = degradationReasons.Distinct(StringComparer.Ordinal).ToList()
        };
    }

    private static List<NewInputLayerDraftStationQueryContext> BuildStationQueryContexts(
        NewInputLayerDraftLongitudinalPath path)
    {
        if (path.PathLength <= 1e-6 || path.PathSegments.Count == 0)
        {
            return new List<NewInputLayerDraftStationQueryContext>();
        }

        var ratios = new[] { 0.1d, 0.3d, 0.5d, 0.7d, 0.9d };
        return ratios
            .Select(
                (ratio, index) =>
                {
                    var distance = path.PathLength * ratio;
                    var segment = path.PathSegments
                        .LastOrDefault(item => distance >= item.PathStart && distance <= item.PathEnd)
                        ?? path.PathSegments.Last();
                    var x = segment.Direction.Normalize();
                    var y = Math.Abs(x.Dot(Vector3.UnitZ)) < 0.9
                        ? Vector3.UnitZ.ProjectToPlane(x).Normalize()
                        : Vector3.UnitY.ProjectToPlane(x).Normalize();
                    var z = x.Cross(y).Normalize();
                    var t = segment.Length <= 1e-6 ? 0d : Math.Clamp((distance - segment.PathStart) / segment.Length, 0d, 1d);
                    var origin = segment.StartPoint + ((segment.EndPoint - segment.StartPoint) * t);
                    return new NewInputLayerDraftStationQueryContext
                    {
                        StationIndex = index,
                        StationRatio = ratio,
                        StationDistance = distance,
                        SectionFrameOrigin = origin,
                        SectionAxisX = x,
                        SectionAxisY = y,
                        SectionAxisZ = z,
                        SegmentReference = segment.SegmentIndex,
                        FrameConfidence = 0.70d,
                        Warnings = new List<string>()
                    };
                })
            .ToList();
    }

    private static string ResolvePartModelType(XingcaiPartSnapshot part)
    {
        var profile = part.ProfileString ?? string.Empty;
        var type = part.PartType ?? string.Empty;
        if (type.Equals("Beam", StringComparison.OrdinalIgnoreCase) ||
            type.Equals("PolyBeam", StringComparison.OrdinalIgnoreCase))
        {
            return "PATH_SECTION_SWEEP";
        }

        if (type.Equals("ContourPlate", StringComparison.OrdinalIgnoreCase) ||
            type.Equals("BentPlate", StringComparison.OrdinalIgnoreCase) ||
            profile.StartsWith("PL", StringComparison.OrdinalIgnoreCase))
        {
            return "PLATE_BOUNDARY_THICKNESS";
        }

        return "UNCLASSIFIED_SOLID_SUMMARY";
    }

    private static string ResolveSectionSource(
        XingcaiPartSnapshot part,
        XingcaiInputLayerDraftPartSnapshot? draftPart,
        BodyMaterialSummary summary)
    {
        if (!string.IsNullOrWhiteSpace(draftPart?.NormalizedShape?.SectionSource))
        {
            return draftPart.NormalizedShape.SectionSource;
        }

        if (!string.IsNullOrWhiteSpace(part.ProfileString))
        {
            return "PROFILE_STRING";
        }

        return string.IsNullOrWhiteSpace(summary.MemberProfileString) ? string.Empty : "MEMBER_PROFILE_STRING";
    }

    private static string ResolveSweepProfile(XingcaiPartSnapshot part, BodyMaterialSummary summary)
    {
        if (!string.IsNullOrWhiteSpace(part.ProfileString))
        {
            return part.ProfileString;
        }

        return summary.MemberProfileString;
    }

    private static double ResolveModelConfidence(string modelType, IReadOnlyCollection<string> warnings)
    {
        var baseConfidence = modelType switch
        {
            "PATH_SECTION_SWEEP" => 0.82d,
            "PLATE_BOUNDARY_THICKNESS" => 0.78d,
            _ => 0.45d
        };

        return Math.Max(0.10d, baseConfidence - (warnings.Count * 0.08d));
    }

    private static string InferRepresentationLevelFromKind(string representationKind) => representationKind switch
    {
        RepresentationKindPathSectionSweep => "EXACT",
        RepresentationKindPlateBoundaryThickness => "EXACT",
        RepresentationKindApproxPathSectionSweep => "APPROXIMATE",
        RepresentationKindApproxBoundaryThickness => "APPROXIMATE",
        RepresentationKindTraceOnlyFallback => "FALLBACK",
        _ => "APPROXIMATE"
    };

    private static string InferRiskFromLevel(string representationLevel) => representationLevel switch
    {
        "EXACT" => "LOW",
        "NEAR_EXACT" => "LOW",
        "APPROXIMATE" => "MEDIUM",
        "FALLBACK" => "HIGH",
        _ => "MEDIUM"
    };

    private static string PromoteRisk(string current, string incoming)
    {
        var currentRank = RiskRank(current);
        var incomingRank = RiskRank(incoming);
        return incomingRank > currentRank ? incoming : current;
    }

    private static int RiskRank(string code) => code switch
    {
        "HIGH" => 3,
        "MEDIUM" => 2,
        "LOW" => 1,
        _ => 0
    };

    private static void AddReason(List<string> target, string code)
    {
        if (!target.Contains(code, StringComparer.Ordinal))
        {
            target.Add(code);
        }
    }

    private static string NormalizeCode(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    private static string ToRepresentationKindLabelZh(string code) => code switch
    {
        RepresentationKindPathSectionSweep => "路径+截面",
        RepresentationKindPlateBoundaryThickness => "边界+厚度",
        RepresentationKindApproxPathSectionSweep => "近似路径+截面",
        RepresentationKindApproxBoundaryThickness => "近似边界+厚度",
        RepresentationKindTraceOnlyFallback => "退化近似表达",
        _ => NormalizeCode(code)
    };

    private static string ToRepresentationLevelLabelZh(string code) => code switch
    {
        "EXACT" => "精确表达",
        "NEAR_EXACT" => "近精确表达",
        "APPROXIMATE" => "近似表达",
        "FALLBACK" => "退化表达",
        _ => NormalizeCode(code)
    };

    private static string ToDistortionRiskLabelZh(string code) => code switch
    {
        "LOW" => "低风险",
        "MEDIUM" => "中风险",
        "HIGH" => "高风险",
        _ => NormalizeCode(code)
    };

    private static Vector3 ResolveLongDirection(XingcaiPartSnapshot part, XingcaiMemberSnapshot snapshot)
    {
        var x = ToVector3(part.LocalCoordinateSystem?.X).Normalize();
        var y = ToVector3(part.LocalCoordinateSystem?.Y).Normalize();
        var memberAxis = snapshot.Member.MainAxis?.Direction is null
            ? Vector3.UnitX
            : ToVector3(snapshot.Member.MainAxis.Direction).Normalize();
        return Math.Abs(x.Dot(memberAxis)) >= Math.Abs(y.Dot(memberAxis)) ? x : y;
    }

    private static Vector3 ResolveNormalDirection(XingcaiPartSnapshot part, Vector3 localZ)
    {
        if (localZ.Length > 1e-9)
        {
            return localZ;
        }

        return ToVector3(part.LocalCoordinateSystem?.X)
            .Cross(ToVector3(part.LocalCoordinateSystem?.Y))
            .Normalize();
    }

    private static Vector3 ResolveWidthDirection(XingcaiPartSnapshot part, XingcaiMemberSnapshot snapshot)
    {
        var longDirection = ResolveLongDirection(part, snapshot);
        var x = ToVector3(part.LocalCoordinateSystem?.X).Normalize();
        var y = ToVector3(part.LocalCoordinateSystem?.Y).Normalize();
        return Math.Abs(x.Dot(longDirection)) < Math.Abs(y.Dot(longDirection)) ? x : y;
    }

    private static bool HasSegmentGap(IReadOnlyList<NewInputLayerDraftPathSegment> segments)
    {
        for (var i = 1; i < segments.Count; i++)
        {
            if (Math.Abs(segments[i].PathStart - segments[i - 1].PathEnd) > 1e-3)
            {
                return true;
            }
        }

        return false;
    }

    private static XingcaiMemberSnapshot? LoadSnapshot(
        string sourceFile,
        IDictionary<string, XingcaiMemberSnapshot?> cache)
    {
        if (cache.TryGetValue(sourceFile, out var cached))
        {
            return cached;
        }

        XingcaiMemberSnapshot? snapshot = null;
        if (File.Exists(sourceFile))
        {
            using var stream = File.OpenRead(sourceFile);
            snapshot = JsonSerializer.Deserialize<XingcaiMemberSnapshot>(stream, SnapshotJsonOptions);
        }

        cache[sourceFile] = snapshot;
        return snapshot;
    }

    private static XingcaiInputLayerDraftMemberSnapshot? LoadDraftSnapshot(
        string sourceFile,
        IDictionary<string, XingcaiInputLayerDraftMemberSnapshot?> cache)
    {
        var draftPath = ResolveDraftPath(sourceFile);
        if (cache.TryGetValue(draftPath, out var cached))
        {
            return cached;
        }

        XingcaiInputLayerDraftMemberSnapshot? snapshot = null;
        if (File.Exists(draftPath))
        {
            using var stream = File.OpenRead(draftPath);
            snapshot = JsonSerializer.Deserialize<XingcaiInputLayerDraftMemberSnapshot>(stream, SnapshotJsonOptions);
        }

        cache[draftPath] = snapshot;
        return snapshot;
    }

    private static string ResolveDraftPath(string sourceFile)
    {
        const string suffix = ".json";
        return sourceFile.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? sourceFile[..^suffix.Length] + ".input-layer-draft.json"
            : sourceFile + ".input-layer-draft.json";
    }

    private static Dictionary<int, XingcaiInputLayerDraftPartSnapshot> BuildDraftPartLookup(
        XingcaiInputLayerDraftMemberSnapshot? snapshot)
    {
        if (snapshot?.Parts is null || snapshot.Parts.Count == 0)
        {
            return new Dictionary<int, XingcaiInputLayerDraftPartSnapshot>();
        }

        return snapshot.Parts
            .GroupBy(item => ParseInt(item.PartId))
            .Where(group => group.Key != 0)
            .ToDictionary(group => group.Key, group => group.First());
    }

    private static XingcaiInputLayerDraftPartSnapshot? ResolveDraftPart(
        XingcaiPartSnapshot part,
        IReadOnlyDictionary<int, XingcaiInputLayerDraftPartSnapshot> lookup)
    {
        var partId = ParseInt(part.PartId);
        return lookup.TryGetValue(partId, out var draftPart) ? draftPart : null;
    }

    private static BoundingBox ToBoundingBox(XingcaiBoundingBox? boundingBox)
    {
        if (boundingBox is null)
        {
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }

        return new BoundingBox(ToVector3(boundingBox.Min), ToVector3(boundingBox.Max));
    }

    private static Vector3 ToVector3(XingcaiPoint3D? point)
    {
        if (point is null)
        {
            return Vector3.Zero;
        }

        return new Vector3(point.X, point.Y, point.Z);
    }

    private static int ParseInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : 0;
    }

    private static double? InferThickness(Vector3 size)
    {
        var dims = new[] { size.X, size.Y, size.Z }
            .Where(item => item > 1e-6)
            .OrderBy(item => item)
            .ToArray();
        return dims.Length == 0 ? null : dims[0];
    }
}

internal sealed class XingcaiInputLayerDraftMemberSnapshot
{
    public List<string> ExportDiagnostics { get; set; } = new();

    public List<XingcaiInputLayerDraftPartSnapshot> Parts { get; set; } = new();
}

internal sealed class XingcaiInputLayerDraftPartSnapshot
{
    public string PartId { get; set; } = string.Empty;

    public string PartModelType { get; set; } = string.Empty;

    public string RepresentationKind { get; set; } = string.Empty;

    public string RepresentationLevel { get; set; } = string.Empty;

    public string DistortionRiskCode { get; set; } = string.Empty;

    public List<string> DistortionRiskReasons { get; set; } = new();

    public List<string> DegradationReasonCodes { get; set; } = new();

    public XingcaiInputLayerDraftRawGeometry? RawGeometry { get; set; }

    public XingcaiInputLayerDraftNormalizedShape? NormalizedShape { get; set; }

    public XingcaiInputLayerDraftLongitudinalProjection? LongitudinalProjection { get; set; }

    public List<string> WarningCodes { get; set; } = new();
}

internal sealed class XingcaiInputLayerDraftRawGeometry
{
    public double? LegacyThickness { get; set; }

    public double? NormalizedThickness { get; set; }

    public string ThicknessSource { get; set; } = string.Empty;

    public string ThicknessSourceDetail { get; set; } = string.Empty;
}

internal sealed class XingcaiInputLayerDraftNormalizedShape
{
    public string BoundaryLoopKind { get; set; } = string.Empty;

    public string BoundarySource { get; set; } = string.Empty;

    public bool BoundaryClosed { get; set; }

    public List<XingcaiPoint3D> BoundaryPoints { get; set; } = new();

    public string SectionSource { get; set; } = string.Empty;
}

internal sealed class XingcaiInputLayerDraftLongitudinalProjection
{
    public double? Start { get; set; }

    public double? End { get; set; }

    public double? Length { get; set; }

    public double? CoverageRatio { get; set; }
}
