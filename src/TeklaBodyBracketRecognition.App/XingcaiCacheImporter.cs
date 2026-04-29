using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class XingcaiCacheImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Regex NumberRegex = new(@"-?\d+(?:\.\d+)?", RegexOptions.Compiled);

    public static IReadOnlyList<ImportedAssemblyJob> LoadJobs(string inputPath)
    {
        var files = ResolveMemberFiles(inputPath).ToArray();
        return files.Select(LoadJob).ToArray();
    }

    private static ImportedAssemblyJob LoadJob(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var snapshot = JsonSerializer.Deserialize<XingcaiMemberSnapshot>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"无法读取 xingcai cache: {filePath}");

        var mainPart = ResolveMainPart(snapshot);
        var memberAxis = ResolveMemberAxis(snapshot, mainPart);
        var importedAxisSegments = ToLongitudinalAxisSegments(snapshot.AxisSegments);
        var useSyntheticBody = ShouldSynthesizeBody(snapshot, mainPart);
        var roleLookup = BuildRoleLookup(snapshot);
        var sourceMainPartId = ParseInt(mainPart.PartId);
        var sourceBodySeedParts = CollectSourceBodySeedParts(snapshot, mainPart, roleLookup);

        var recognitionInputParts = new List<PartInput>();
        var recognitionInputRelationships = new List<RelationshipInput>();
        var realInputParts = new List<PartInput>(snapshot.Parts.Count);
        var realInputMainPartId = sourceMainPartId;
        var recognitionInputMainPartId = sourceMainPartId;
        string? synthesisKind = null;

        if (useSyntheticBody)
        {
            synthesisKind = ResolveSynthesisKind(snapshot);
            var syntheticParts = BuildSyntheticBodyParts(snapshot, mainPart, memberAxis, synthesisKind, out var syntheticRelationships);
            recognitionInputParts.AddRange(syntheticParts);
            recognitionInputRelationships.AddRange(syntheticRelationships);
            recognitionInputMainPartId = syntheticParts[0].PartId;
        }

        foreach (var part in snapshot.Parts)
        {
            var convertedPart = ConvertRealPart(snapshot, part, memberAxis, roleLookup);
            realInputParts.Add(convertedPart);

            if (useSyntheticBody && string.Equals(part.Guid, mainPart.Guid, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            recognitionInputParts.Add(convertedPart);
        }

        return new ImportedAssemblyJob(
            filePath,
            snapshot.Member.MemberId,
            snapshot.Member.ProfileString,
            sourceMainPartId,
            string.IsNullOrWhiteSpace(mainPart.Name) ? "<unnamed>" : mainPart.Name,
            mainPart.ProfileString,
            sourceBodySeedParts,
            new AssemblyInput(
                snapshot.Member.AssemblyId,
                recognitionInputMainPartId,
                recognitionInputParts,
                recognitionInputRelationships,
                importedAxisSegments,
                snapshot.LongitudinalAxisKind,
                snapshot.LongitudinalAxisConfidence,
                snapshot.LongitudinalAxisSource),
            new AssemblyInput(
                snapshot.Member.AssemblyId,
                realInputMainPartId,
                realInputParts,
                Array.Empty<RelationshipInput>(),
                importedAxisSegments,
                snapshot.LongitudinalAxisKind,
                snapshot.LongitudinalAxisConfidence,
                snapshot.LongitudinalAxisSource),
            useSyntheticBody,
            synthesisKind);
    }

    private static IEnumerable<string> ResolveMemberFiles(string inputPath)
    {
        if (File.Exists(inputPath))
        {
            yield return inputPath;
            yield break;
        }

        var directory = inputPath;
        if (!Directory.Exists(directory))
        {
            yield break;
        }

        var directFiles = Directory
            .GetFiles(directory, "member_*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (directFiles.Length > 0)
        {
            foreach (var file in directFiles)
            {
                yield return file;
            }

            yield break;
        }

        var membersDirectory = Path.Combine(directory, "members");
        if (!Directory.Exists(membersDirectory))
        {
            yield break;
        }

        foreach (var file in Directory.GetFiles(membersDirectory, "member_*.json", SearchOption.TopDirectoryOnly).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            yield return file;
        }
    }

    private static XingcaiPartSnapshot ResolveMainPart(XingcaiMemberSnapshot snapshot)
    {
        var byGuid = snapshot.Parts.FirstOrDefault(part => string.Equals(part.Guid, snapshot.Member.Guid, StringComparison.OrdinalIgnoreCase));
        if (byGuid is not null)
        {
            return byGuid;
        }

        return snapshot.Parts
            .OrderByDescending(part => part.AxisProjection?.Length ?? 0.0)
            .ThenByDescending(part => part.Volume)
            .First();
    }

    private static Vector3 ResolveMemberAxis(XingcaiMemberSnapshot snapshot, XingcaiPartSnapshot mainPart)
    {
        var importedSegments = ToLongitudinalAxisSegments(snapshot.AxisSegments);
        if (importedSegments.Count > 0)
        {
            var guideAxis = importedSegments[0].Direction.Normalize();
            if (guideAxis.Length > 1e-9)
            {
                return guideAxis;
            }
        }

        var axis = ToVector3(snapshot.Member.MainAxis?.Direction).Normalize();
        if (axis.Length > 1e-9)
        {
            return axis;
        }

        axis = ToVector3(mainPart.LocalCoordinateSystem?.X).Normalize();
        return axis.Length > 1e-9 ? axis : Vector3.UnitX;
    }

    private static bool ShouldSynthesizeBody(XingcaiMemberSnapshot snapshot, XingcaiPartSnapshot mainPart)
    {
        if (!string.Equals(mainPart.PartType, "Beam", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ResolveSynthesisKind(snapshot) is not null;
    }

    private static string? ResolveSynthesisKind(XingcaiMemberSnapshot snapshot)
    {
        var profile = snapshot.Member.ProfileString ?? string.Empty;
        var normalized = profile.Replace(" ", string.Empty).ToUpperInvariant();

        if (normalized.StartsWith("BH", StringComparison.Ordinal) ||
            normalized.StartsWith("H", StringComparison.Ordinal))
        {
            return "H";
        }

        if (normalized.StartsWith("BK", StringComparison.Ordinal) ||
            normalized.StartsWith("BOX", StringComparison.Ordinal) ||
            normalized.Contains("BOX", StringComparison.Ordinal))
        {
            return "BOX";
        }

        return snapshot.Classification?.MainClass switch
        {
            1 => "H",
            2 => "BOX",
            3 => "T",
            4 => "CROSS",
            _ => null
        };
    }

    private static IReadOnlyList<PartInput> BuildSyntheticBodyParts(
        XingcaiMemberSnapshot snapshot,
        XingcaiPartSnapshot mainPart,
        Vector3 axis,
        string? synthesisKind,
        out IReadOnlyList<RelationshipInput> relationships)
    {
        var bbox = ToBoundingBox(snapshot.Member.BoundingBox);
        var center = bbox.Center;
        var localY = ToVector3(mainPart.LocalCoordinateSystem?.Y).ProjectToPlane(axis).Normalize();
        if (localY.Length <= 1e-9)
        {
            localY = Math.Abs(axis.Dot(Vector3.UnitZ)) < 0.9 ? Vector3.UnitZ.ProjectToPlane(axis).Normalize() : Vector3.UnitY.ProjectToPlane(axis).Normalize();
        }

        var localZ = axis.Cross(localY).Normalize();
        if (localZ.Length <= 1e-9)
        {
            localZ = Math.Abs(axis.Dot(Vector3.UnitY)) < 0.9 ? axis.Cross(Vector3.UnitY).Normalize() : axis.Cross(Vector3.UnitZ).Normalize();
        }

        var length = snapshot.Member.MainAxis?.Length > 1e-6
            ? snapshot.Member.MainAxis.Length
            : Math.Max(mainPart.AxisProjection?.Length ?? 0.0, MaxDimension(bbox.Size));

        var dimensions = ResolveBodyDimensions(snapshot.Member.ProfileString, synthesisKind, bbox.Size);
        var material = string.IsNullOrWhiteSpace(mainPart.Material) ? snapshot.Member.Material : mainPart.Material;
        var parts = new List<PartInput>();
        var edges = new List<RelationshipInput>();
        var baseId = -1000;

        if (string.Equals(synthesisKind, "H", StringComparison.OrdinalIgnoreCase))
        {
            var webId = baseId - 1;
            var topId = baseId - 2;
            var bottomId = baseId - 3;
            var webHeight = Math.Max(1.0, dimensions.Height - (2.0 * dimensions.FlangeThickness));

            parts.Add(CreateSyntheticPlate(webId, "SyntheticWeb", material, center, axis, length, localY, webHeight, localZ, dimensions.WebThickness, PartSemanticRole.WebCandidate));
            parts.Add(CreateSyntheticPlate(topId, "SyntheticTopFlange", material, center + (localY * ((dimensions.Height * 0.5) - (dimensions.FlangeThickness * 0.5))), axis, length, localZ, dimensions.Width, localY, dimensions.FlangeThickness, PartSemanticRole.FlangeCandidate));
            parts.Add(CreateSyntheticPlate(bottomId, "SyntheticBottomFlange", material, center - (localY * ((dimensions.Height * 0.5) - (dimensions.FlangeThickness * 0.5))), axis, length, localZ, dimensions.Width, -localY, dimensions.FlangeThickness, PartSemanticRole.FlangeCandidate));

            edges.Add(new RelationshipInput(webId, topId, EdgeType.Weld, 0.98, 0.98, "synthetic-h-web-top"));
            edges.Add(new RelationshipInput(webId, bottomId, EdgeType.Weld, 0.98, 0.98, "synthetic-h-web-bottom"));
        }
        else if (string.Equals(synthesisKind, "BOX", StringComparison.OrdinalIgnoreCase))
        {
            var leftId = baseId - 1;
            var rightId = baseId - 2;
            var topId = baseId - 3;
            var bottomId = baseId - 4;
            var wallHeight = Math.Max(1.0, dimensions.Height - (2.0 * dimensions.FlangeThickness));

            parts.Add(CreateSyntheticPlate(leftId, "SyntheticLeftWall", material, center - (localZ * ((dimensions.Width * 0.5) - (dimensions.WebThickness * 0.5))), axis, length, localY, wallHeight, localZ, dimensions.WebThickness, PartSemanticRole.WallCandidate));
            parts.Add(CreateSyntheticPlate(rightId, "SyntheticRightWall", material, center + (localZ * ((dimensions.Width * 0.5) - (dimensions.WebThickness * 0.5))), axis, length, localY, wallHeight, -localZ, dimensions.WebThickness, PartSemanticRole.WallCandidate));
            parts.Add(CreateSyntheticPlate(topId, "SyntheticTopWall", material, center + (localY * ((dimensions.Height * 0.5) - (dimensions.FlangeThickness * 0.5))), axis, length, localZ, dimensions.Width, localY, dimensions.FlangeThickness, PartSemanticRole.WallCandidate));
            parts.Add(CreateSyntheticPlate(bottomId, "SyntheticBottomWall", material, center - (localY * ((dimensions.Height * 0.5) - (dimensions.FlangeThickness * 0.5))), axis, length, localZ, dimensions.Width, -localY, dimensions.FlangeThickness, PartSemanticRole.WallCandidate));

            edges.Add(new RelationshipInput(leftId, topId, EdgeType.Weld, 0.95, 0.95, "synthetic-box-left-top"));
            edges.Add(new RelationshipInput(leftId, bottomId, EdgeType.Weld, 0.95, 0.95, "synthetic-box-left-bottom"));
            edges.Add(new RelationshipInput(rightId, topId, EdgeType.Weld, 0.95, 0.95, "synthetic-box-right-top"));
            edges.Add(new RelationshipInput(rightId, bottomId, EdgeType.Weld, 0.95, 0.95, "synthetic-box-right-bottom"));
        }
        else if (string.Equals(synthesisKind, "T", StringComparison.OrdinalIgnoreCase))
        {
            var webId = baseId - 1;
            var flangeId = baseId - 2;
            var webHeight = Math.Max(1.0, dimensions.Height - dimensions.FlangeThickness);

            parts.Add(CreateSyntheticPlate(webId, "SyntheticStem", material, center - (localY * (dimensions.FlangeThickness * 0.5)), axis, length, localY, webHeight, localZ, dimensions.WebThickness, PartSemanticRole.WebCandidate));
            parts.Add(CreateSyntheticPlate(flangeId, "SyntheticFlange", material, center + (localY * ((dimensions.Height * 0.5) - (dimensions.FlangeThickness * 0.5))), axis, length, localZ, dimensions.Width, localY, dimensions.FlangeThickness, PartSemanticRole.FlangeCandidate));

            edges.Add(new RelationshipInput(webId, flangeId, EdgeType.Weld, 0.96, 0.96, "synthetic-t"));
        }
        else
        {
            var verticalId = baseId - 1;
            var horizontalId = baseId - 2;

            parts.Add(CreateSyntheticPlate(verticalId, "SyntheticVerticalWeb", material, center, axis, length, localY, dimensions.Height, localZ, dimensions.WebThickness, PartSemanticRole.WebCandidate));
            parts.Add(CreateSyntheticPlate(horizontalId, "SyntheticHorizontalWeb", material, center, axis, length, localZ, dimensions.Width, localY, dimensions.FlangeThickness, PartSemanticRole.WebCandidate));

            edges.Add(new RelationshipInput(verticalId, horizontalId, EdgeType.Weld, 0.94, 0.94, "synthetic-cross"));
        }

        relationships = edges;
        return parts;
    }

    private static BodyDimensions ResolveBodyDimensions(string? profileString, string? synthesisKind, Vector3 bboxSize)
    {
        var values = NumberRegex
            .Matches(profileString ?? string.Empty)
            .Select(match => double.Parse(match.Value, CultureInfo.InvariantCulture))
            .Where(value => value > 0)
            .ToArray();

        var sortedCrossDims = new[] { bboxSize.X, bboxSize.Y, bboxSize.Z }
            .OrderByDescending(value => value)
            .Skip(1)
            .OrderByDescending(value => value)
            .ToArray();

        var height = values.ElementAtOrDefault(0);
        var width = values.ElementAtOrDefault(1);
        var webThickness = values.ElementAtOrDefault(2);
        var flangeThickness = values.ElementAtOrDefault(3);

        if (height <= 0)
        {
            height = sortedCrossDims.ElementAtOrDefault(0);
        }

        if (width <= 0)
        {
            width = sortedCrossDims.ElementAtOrDefault(1);
        }

        var fallbackThickness = Math.Max(8.0, Math.Min(height, width) * 0.03);
        if (webThickness <= 0)
        {
            webThickness = fallbackThickness;
        }

        if (flangeThickness <= 0)
        {
            flangeThickness = fallbackThickness;
        }

        if (string.Equals(synthesisKind, "BOX", StringComparison.OrdinalIgnoreCase))
        {
            flangeThickness = Math.Max(flangeThickness, webThickness);
        }

        return new BodyDimensions(height, width, webThickness, flangeThickness);
    }

    private static PartInput CreateSyntheticPlate(
        int partId,
        string name,
        string material,
        Vector3 center,
        Vector3 longDir,
        double longLength,
        Vector3 widthDir,
        double widthLength,
        Vector3 normal,
        double thickness,
        PartSemanticRole semanticRole)
    {
        var normalizedLong = longDir.Normalize();
        var normalizedWidth = widthDir.ProjectToPlane(normal).Normalize();
        if (normalizedWidth.Length <= 1e-9)
        {
            normalizedWidth = normal.Cross(normalizedLong).Normalize();
        }

        var normalizedNormal = normal.Normalize();
        var bbox = BuildOrientedBoundingBox(center, normalizedLong, longLength, normalizedWidth, widthLength, normalizedNormal, thickness);
        var surfaceArea = 2.0 * ((longLength * widthLength) + (longLength * thickness) + (widthLength * thickness));
        var volume = longLength * widthLength * thickness;

        return new PartInput(
            partId,
            RuntimePartType.ContourPlate,
            name,
            material,
            $"PL{thickness:0.###}",
            true,
            false,
            center,
            new Vector3(longLength, widthLength, thickness),
            bbox,
            volume,
            surfaceArea,
            thickness,
            normalizedNormal,
            normalizedLong,
            normalizedWidth,
            4,
            0,
            0,
            0,
            0,
            0.0,
            0.0,
            semanticRole,
            1.0,
            false,
            false,
            false,
            1.0,
            Array.Empty<LineSegment3>());
    }

    private static PartInput ConvertRealPart(
        XingcaiMemberSnapshot snapshot,
        XingcaiPartSnapshot part,
        Vector3 memberAxis,
        IReadOnlyDictionary<int, XingcaiPartRoleAssignment> roleLookup)
    {
        var partId = ParseInt(part.PartId);
        var runtimeType = MapRuntimeType(part.PartType);
        var boundingBox = ToBoundingBox(part.BoundingBox);
        var size = boundingBox.Size;
        var thickness = part.Thickness > 1e-6 ? part.Thickness : InferThickness(size);
        roleLookup.TryGetValue(partId, out var roleHint);
        var isPlateLike =
            runtimeType == RuntimePartType.ContourPlate ||
            runtimeType == RuntimePartType.BentPlate ||
            (thickness > 0.0 && thickness <= Math.Max(size.X, Math.Max(size.Y, size.Z)) * 0.2) ||
            (part.ProfileString?.StartsWith("PL", StringComparison.OrdinalIgnoreCase) ?? false);

        var isSpecialShape =
            runtimeType == RuntimePartType.BentPlate ||
            runtimeType == RuntimePartType.PolyBeam ||
            part.GeometryHints?.IsBentLike == true ||
            part.GeometryHints?.IsFoldedLike == true;

        var axisX = ToVector3(part.LocalCoordinateSystem?.X).Normalize();
        var axisY = ToVector3(part.LocalCoordinateSystem?.Y).Normalize();
        var axisZ = axisX.Cross(axisY).Normalize();
        if (axisZ.Length <= 1e-9)
        {
            axisZ = ToVector3(part.LocalCoordinateSystem?.Z).Normalize();
        }

        if (axisX.Length <= 1e-9)
        {
            axisX = memberAxis;
        }

        if (axisY.Length <= 1e-9 || Math.Abs(axisY.Dot(axisX)) > 0.95)
        {
            axisY = Math.Abs(axisX.Dot(Vector3.UnitZ)) < 0.9 ? Vector3.UnitZ.ProjectToPlane(axisX).Normalize() : Vector3.UnitY.ProjectToPlane(axisX).Normalize();
        }

        if (axisZ.Length <= 1e-9)
        {
            axisZ = axisX.Cross(axisY).Normalize();
        }

        Vector3 plateNormal;
        Vector3 plateLongDirection;
        Vector3 plateWidthDirection;

        if (isPlateLike)
        {
            plateNormal = axisZ.Length > 1e-9 ? axisZ : Vector3.UnitZ;
            var xAlignment = Math.Abs(axisX.Dot(memberAxis));
            var yAlignment = Math.Abs(axisY.Dot(memberAxis));
            plateLongDirection = (xAlignment >= yAlignment ? axisX : axisY).ProjectToPlane(plateNormal).Normalize();
            plateWidthDirection = (xAlignment >= yAlignment ? axisY : axisX).ProjectToPlane(plateNormal).Normalize();
            if (plateWidthDirection.Length <= 1e-9)
            {
                plateWidthDirection = plateNormal.Cross(plateLongDirection).Normalize();
            }
        }
        else
        {
            plateLongDirection = axisX.Length > 1e-9 ? axisX : memberAxis;
            plateNormal = axisZ.Length > 1e-9 ? axisZ : Vector3.UnitZ;
            plateWidthDirection = plateNormal.Cross(plateLongDirection).Normalize();
        }

        var surfaceArea = part.Area > 1e-6 ? part.Area : EstimateSurfaceArea(size);
        var volume = part.Volume > 1e-6 ? part.Volume : EstimateVolume(size, thickness, isPlateLike);

        return new PartInput(
            partId,
            runtimeType,
            part.Name ?? string.Empty,
            part.Material ?? string.Empty,
            part.ProfileString ?? string.Empty,
            isPlateLike,
            isSpecialShape,
            ToVector3(part.Centroid),
            size,
            boundingBox,
            volume,
            surfaceArea,
            thickness > 1e-6 ? thickness : null,
            plateNormal,
            plateLongDirection,
            plateWidthDirection,
            isPlateLike ? 4 : 0,
            0,
            0,
            0,
            0,
            0.0,
            0.0,
            MapSemanticRole(roleHint?.Role),
            roleHint?.Score ?? 0.0,
            part.EndProximity?.NearStart == true,
            part.EndProximity?.NearEnd == true,
            part.GeometryHints?.OuterSideCandidate == true,
            part.GeometryHints?.Planarity ?? (isPlateLike ? 1.0 : 0.5),
            ToLineSegments(part.SolidEdges));
    }

    private static IReadOnlyDictionary<int, XingcaiPartRoleAssignment> BuildRoleLookup(XingcaiMemberSnapshot snapshot)
    {
        return (snapshot.Classification?.PartRoles ?? Enumerable.Empty<XingcaiPartRoleAssignment>())
            .Where(role => !string.IsNullOrWhiteSpace(role.PartId))
            .GroupBy(role => ParseInt(role.PartId))
            .Where(group => group.Key != 0)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(role => role.Score)
                    .ThenByDescending(role => role.Role, StringComparer.OrdinalIgnoreCase)
                    .First());
    }

    private static IReadOnlyList<SourceBodySeedPart> CollectSourceBodySeedParts(
        XingcaiMemberSnapshot snapshot,
        XingcaiPartSnapshot mainPart,
        IReadOnlyDictionary<int, XingcaiPartRoleAssignment> roleLookup)
    {
        var partsById = snapshot.Parts
            .Where(part => !string.IsNullOrWhiteSpace(part.PartId))
            .GroupBy(part => ParseInt(part.PartId))
            .Where(group => group.Key != 0)
            .ToDictionary(group => group.Key, group => group.First());

        var orderedIds = new List<int>();
        var mainPartId = ParseInt(mainPart.PartId);
        if (mainPartId != 0)
        {
            orderedIds.Add(mainPartId);
        }

        foreach (var pair in roleLookup
                     .Where(pair => IsPrimaryBodyRole(MapSemanticRole(pair.Value.Role)))
                     .OrderByDescending(pair => pair.Value.Score)
                     .ThenBy(pair => pair.Key))
        {
            if (!orderedIds.Contains(pair.Key))
            {
                orderedIds.Add(pair.Key);
            }
        }

        return orderedIds
            .Where(partsById.ContainsKey)
            .Select(
                (partId, index) =>
                {
                    var part = partsById[partId];
                    roleLookup.TryGetValue(partId, out var role);
                    return new SourceBodySeedPart(
                        index + 1,
                        partId,
                        string.IsNullOrWhiteSpace(part.Name) ? "<unnamed>" : part.Name,
                        part.ProfileString,
                        part.Material,
                        part.Thickness > 1e-6 ? part.Thickness : null,
                        MapSemanticRole(role?.Role).ToString(),
                        role?.Score ?? 0.0,
                        partId == mainPartId);
                })
            .ToArray();
    }

    private static bool IsPrimaryBodyRole(PartSemanticRole role) =>
        role is PartSemanticRole.WebCandidate or PartSemanticRole.FlangeCandidate or PartSemanticRole.WallCandidate;

    private static PartSemanticRole MapSemanticRole(string? role)
    {
        return role?.Trim().ToUpperInvariant() switch
        {
            "OTHER" => PartSemanticRole.Other,
            "WEB_CANDIDATE" => PartSemanticRole.WebCandidate,
            "FLANGE_CANDIDATE" => PartSemanticRole.FlangeCandidate,
            "WALL_CANDIDATE" => PartSemanticRole.WallCandidate,
            "ENDPLATE_CANDIDATE" => PartSemanticRole.EndPlateCandidate,
            "STIFFENER_CANDIDATE" => PartSemanticRole.StiffenerCandidate,
            "BRACKET_CANDIDATE" => PartSemanticRole.BracketCandidate,
            _ => PartSemanticRole.Unknown
        };
    }

    private static RuntimePartType MapRuntimeType(string? partType)
    {
        return partType?.ToUpperInvariant() switch
        {
            "CONTOURPLATE" => RuntimePartType.ContourPlate,
            "BENTPLATE" => RuntimePartType.BentPlate,
            "POLYBEAM" => RuntimePartType.PolyBeam,
            "BEAM" => RuntimePartType.Beam,
            _ => RuntimePartType.Other
        };
    }

    private static BoundingBox BuildOrientedBoundingBox(
        Vector3 center,
        Vector3 longDir,
        double longLength,
        Vector3 widthDir,
        double widthLength,
        Vector3 normal,
        double thickness)
    {
        var halfLong = longDir.Normalize() * (longLength * 0.5);
        var halfWidth = widthDir.Normalize() * (widthLength * 0.5);
        var halfThickness = normal.Normalize() * (thickness * 0.5);
        var points = new List<Vector3>(8);

        foreach (var longSign in new[] { -1.0, 1.0 })
        {
            foreach (var widthSign in new[] { -1.0, 1.0 })
            {
                foreach (var thicknessSign in new[] { -1.0, 1.0 })
                {
                    points.Add(center + (halfLong * longSign) + (halfWidth * widthSign) + (halfThickness * thicknessSign));
                }
            }
        }

        return BoundingBox.FromPoints(points);
    }

    private static double InferThickness(Vector3 size)
    {
        return new[] { size.X, size.Y, size.Z }
            .Where(value => value > 1e-6)
            .OrderBy(value => value)
            .FirstOrDefault();
    }

    private static double EstimateSurfaceArea(Vector3 size)
    {
        return 2.0 * ((size.X * size.Y) + (size.X * size.Z) + (size.Y * size.Z));
    }

    private static double EstimateVolume(Vector3 size, double thickness, bool isPlateLike)
    {
        if (isPlateLike && thickness > 1e-6)
        {
            var sorted = new[] { size.X, size.Y, size.Z }.OrderByDescending(value => value).ToArray();
            return sorted[0] * sorted[1] * thickness;
        }

        return size.X * size.Y * size.Z;
    }

    private static double MaxDimension(Vector3 size) => Math.Max(size.X, Math.Max(size.Y, size.Z));

    private static int ParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
    }

    private static Vector3 ToVector3(XingcaiPoint3D? point)
    {
        if (point is null)
        {
            return Vector3.Zero;
        }

        return new Vector3(point.X, point.Y, point.Z);
    }

    private static IReadOnlyList<LineSegment3> ToLineSegments(IEnumerable<XingcaiSolidEdge>? edges)
    {
        if (edges is null)
        {
            return Array.Empty<LineSegment3>();
        }

        return edges
            .Select(edge => new LineSegment3(ToVector3(edge.Start), ToVector3(edge.End)))
            .Where(segment => segment.Length > 1e-3)
            .ToArray();
    }

    private static IReadOnlyList<LongitudinalAxisSegment> ToLongitudinalAxisSegments(IEnumerable<XingcaiLongitudinalAxisSegment>? segments)
    {
        if (segments is null)
        {
            return Array.Empty<LongitudinalAxisSegment>();
        }

        return segments
            .Select(
                (segment, index) => new LongitudinalAxisSegment(
                    segment.Index ?? index,
                    ToVector3(segment.Start),
                    ToVector3(segment.End),
                    ToVector3(segment.Direction).Normalize(),
                    segment.CumulativeStart,
                    segment.CumulativeEnd))
            .Where(segment => segment.Length > 1e-6 && segment.Direction.Length > 1e-6)
            .OrderBy(segment => segment.SegmentIndex)
            .ToArray();
    }

    private static BoundingBox ToBoundingBox(XingcaiBoundingBox? boundingBox)
    {
        if (boundingBox is null)
        {
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }

        return new BoundingBox(ToVector3(boundingBox.Min), ToVector3(boundingBox.Max));
    }

    private sealed record BodyDimensions(double Height, double Width, double WebThickness, double FlangeThickness);
}

internal sealed record ImportedAssemblyJob(
    string SourceFile,
    string MemberId,
    string MemberProfileString,
    int SourceMainPartId,
    string SourceMainPartName,
    string SourceMainPartProfileString,
    IReadOnlyList<SourceBodySeedPart> SourceBodySeedParts,
    AssemblyInput Input,
    AssemblyInput RealInput,
    bool SynthesizedBody,
    string? SynthesisKind);

internal sealed record SourceBodySeedPart(
    int SeedOrder,
    int PartId,
    string PartName,
    string ProfileString,
    string Material,
    double? Thickness,
    string SemanticRole,
    double SemanticRoleScore,
    bool IsSourceMainPart);

internal sealed class XingcaiMemberSnapshot
{
    public XingcaiMemberIdentity Member { get; set; } = new();

    public string? LongitudinalAxisKind { get; set; }

    public List<XingcaiPoint3D>? GuidePolyline { get; set; }

    public List<XingcaiLongitudinalAxisSegment>? AxisSegments { get; set; }

    public double? LongitudinalAxisConfidence { get; set; }

    public string? LongitudinalAxisSource { get; set; }

    public List<XingcaiPartSnapshot> Parts { get; set; } = new();

    public XingcaiClassification? Classification { get; set; }
}

internal sealed class XingcaiClassification
{
    public int MainClass { get; set; }

    public List<XingcaiPartRoleAssignment> PartRoles { get; set; } = new();
}

internal sealed class XingcaiPartRoleAssignment
{
    public string PartId { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public double Score { get; set; }

    public string Reason { get; set; } = string.Empty;
}

internal sealed class XingcaiMemberIdentity
{
    public string AssemblyId { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string Guid { get; set; } = string.Empty;

    public string Material { get; set; } = string.Empty;

    public string ProfileString { get; set; } = string.Empty;

    public XingcaiAxisDefinition? MainAxis { get; set; }

    public XingcaiBoundingBox? BoundingBox { get; set; }
}

internal sealed class XingcaiAxisDefinition
{
    public XingcaiPoint3D? Direction { get; set; }

    public double Length { get; set; }
}

internal sealed class XingcaiPartSnapshot
{
    public string PartId { get; set; } = string.Empty;

    public string Guid { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string PartType { get; set; } = string.Empty;

    public string ProfileString { get; set; } = string.Empty;

    public string Material { get; set; } = string.Empty;

    public double Thickness { get; set; }

    public double Area { get; set; }

    public double Volume { get; set; }

    public XingcaiPoint3D? Centroid { get; set; }

    public XingcaiCoordinateSystem? LocalCoordinateSystem { get; set; }

    public XingcaiBoundingBox? BoundingBox { get; set; }

    public XingcaiAxisProjection? AxisProjection { get; set; }

    public XingcaiEndProximity? EndProximity { get; set; }

    public XingcaiGeometryHints? GeometryHints { get; set; }

    public List<XingcaiSolidEdge>? SolidEdges { get; set; }
}

internal sealed class XingcaiLongitudinalAxisSegment
{
    public int? Index { get; set; }

    public XingcaiPoint3D? Start { get; set; }

    public XingcaiPoint3D? End { get; set; }

    public XingcaiPoint3D? Direction { get; set; }

    public double CumulativeStart { get; set; }

    public double CumulativeEnd { get; set; }
}

internal sealed class XingcaiAxisProjection
{
    public double Length { get; set; }
}

internal sealed class XingcaiGeometryHints
{
    public bool IsBentLike { get; set; }

    public bool IsFoldedLike { get; set; }

    public bool OuterSideCandidate { get; set; }

    public double Planarity { get; set; }
}

internal sealed class XingcaiEndProximity
{
    public bool NearStart { get; set; }

    public bool NearEnd { get; set; }
}

internal sealed class XingcaiCoordinateSystem
{
    public XingcaiPoint3D? X { get; set; }

    public XingcaiPoint3D? Y { get; set; }

    public XingcaiPoint3D? Z { get; set; }
}

internal sealed class XingcaiBoundingBox
{
    public XingcaiPoint3D? Min { get; set; }

    public XingcaiPoint3D? Max { get; set; }
}

internal sealed class XingcaiPoint3D
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }
}

internal sealed class XingcaiSolidEdge
{
    public XingcaiPoint3D? Start { get; set; }

    public XingcaiPoint3D? End { get; set; }
}
