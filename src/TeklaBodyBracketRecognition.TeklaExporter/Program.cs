using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
namespace TeklaBodyBracketRecognition.TeklaExporter;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            return new ExporterApplication().Run(args);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "Tekla Body/Bracket Exporter",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return 1;
        }
    }
}

internal sealed class ExporterApplication
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TeklaBodyBracketRecognition",
        "ExporterLogs");

    private static readonly string LogPath = Path.Combine(LogDirectory, "last-run.log");

    public int Run(string[] args)
    {
        Directory.CreateDirectory(LogDirectory);
        Log("exporter-start");

        var model = new Model();
        if (!model.GetConnectionStatus())
        {
            Log("model-not-connected");
            MessageBox.Show(
                "没有检测到正在运行并已打开模型的 Tekla Structures。请先打开模型，再运行导出器。",
                "Tekla Body/Bracket Exporter",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return 2;
        }

        var outputDirectory = ResolveOutputDirectory(args);
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            Log("output-directory-empty");
            return 3;
        }

        Log("output-directory=" + outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var collectedAssemblies = CollectTargetAssemblies(model);
        var assemblies = collectedAssemblies.Assemblies;
        Log(
            string.Format(
                CultureInfo.InvariantCulture,
                "collected-assemblies mode={0} count={1}",
                collectedAssemblies.SelectionMode,
                assemblies.Count));
        if (assemblies.Count == 0)
        {
            MessageBox.Show(
                "没有找到可导出的 assembly。\n\n请先选中零件或装配后重试；或者在提示时选择回退导出当前模型中的全部 assembly。",
                "Tekla Body/Bracket Exporter",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return 4;
        }

        var bundle = new ExportBundle
        {
            ExportedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Source = new ExportSourceInfo
            {
                ModelPath = TryGetModelPath(model),
                SelectionMode = collectedAssemblies.SelectionMode,
                ExporterVersion = "0.2.0"
            },
            Assemblies = assemblies
                .OrderBy(assembly => assembly.Identifier.ID)
                .Select(assembly => TryBuildAssemblyExport(model, assembly))
                .Where(assembly => assembly != null)
                .Cast<ExportedAssembly>()
                .ToList()
        };

        var exportPath = Path.Combine(outputDirectory, "tekla-body-bracket-export.bundle.json");
        WriteJson(bundle, exportPath);
        Log("bundle-written=" + exportPath);

        foreach (var assembly in bundle.Assemblies)
        {
            var labels = new AssemblyLabelTemplate
            {
                AssemblyId = assembly.AssemblyId
            };
            var labelPath = Path.Combine(outputDirectory, $"assembly-{SanitizeFileName(assembly.AssemblyId)}.labels.template.json");
            WriteJson(labels, labelPath);
            Log("label-written=" + labelPath);
        }

        var summary = new StringBuilder();
        summary.AppendLine("Tekla body/bracket export completed.");
        summary.AppendLine($"Assemblies: {bundle.Assemblies.Count}");
        summary.AppendLine($"Output: {exportPath}");
        summary.AppendLine();
        summary.AppendLine("Files:");
        summary.AppendLine($"- {Path.GetFileName(exportPath)}");
        summary.AppendLine($"- {bundle.Assemblies.Count} label template files");

        File.WriteAllText(Path.Combine(outputDirectory, "README-export.txt"), summary.ToString(), Encoding.UTF8);
        Log("summary-written");

        MessageBox.Show(
            $"已导出 {bundle.Assemblies.Count} 个 assembly。\n输出目录：\n{outputDirectory}",
            "Tekla Body/Bracket Exporter",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        return 0;
    }

    private static string? ResolveOutputDirectory(string[] args)
    {
        var explicitPath = args.FirstOrDefault(arg => !arg.StartsWith("--", StringComparison.Ordinal));
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            return Path.GetFullPath(explicitPath);
        }

        var pickFolder = args.Any(arg => arg.Equals("--pick-folder", StringComparison.OrdinalIgnoreCase));
        if (!pickFolder)
        {
            return BuildDefaultOutputDirectory();
        }

        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = "选择 Tekla 导出 JSON 的输出目录";
            dialog.ShowNewFolderButton = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var datedFolder = Path.Combine(
                    dialog.SelectedPath,
                    $"tekla-body-bracket-export-{DateTime.Now:yyyyMMdd-HHmmss}");
                return datedFolder;
            }
        }

        return null;
    }

    private static string BuildDefaultOutputDirectory()
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(desktop) || !Directory.Exists(desktop))
        {
            desktop = Path.GetTempPath();
        }

        return Path.Combine(
            desktop,
            "TeklaBodyBracketExports",
            $"tekla-body-bracket-export-{DateTime.Now:yyyyMMdd-HHmmss}");
    }

    private static CollectedAssemblies CollectTargetAssemblies(Model model)
    {
        var selected = CollectSelectedAssemblies();
        if (selected.Count > 0)
        {
            return new CollectedAssemblies(selected, "selected");
        }

        Log("selected-empty");
        var fallback = MessageBox.Show(
            "没有从当前选择集中解析出 assembly。\n\n是否回退为导出当前模型中的全部 assembly？",
            "Tekla Body/Bracket Exporter",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (fallback != DialogResult.Yes)
        {
            return new CollectedAssemblies(new List<Assembly>(), "selected");
        }

        return new CollectedAssemblies(CollectAllAssemblies(model), "all-assemblies");
    }

    private static List<Assembly> CollectSelectedAssemblies()
    {
        var selectedObjects = new Tekla.Structures.Model.UI.ModelObjectSelector().GetSelectedObjects();
        var assemblies = new Dictionary<int, Assembly>();

        while (true)
        {
            ModelObject? currentObject;
            try
            {
                if (!selectedObjects.MoveNext())
                {
                    break;
                }

                currentObject = selectedObjects.Current as ModelObject;
            }
            catch (Exception ex)
            {
                Log("selected-enumerator-error=" + ex.Message);
                break;
            }

            try
            {
                var assembly = ResolveAssembly(currentObject);
                if (assembly == null)
                {
                    Log("skip-selected-type=" + (currentObject?.GetType().FullName ?? "<null>"));
                    continue;
                }

                var assemblyId = SafeGetIdentifier(assembly);
                if (assemblyId != 0)
                {
                    assemblies[assemblyId] = assembly;
                }
            }
            catch (Exception ex)
            {
                Log(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "selection-resolve-skip objectId={0} objectType={1} reason={2}",
                        SafeGetIdentifier(currentObject),
                        currentObject?.GetType().FullName ?? "<null>",
                        ex.Message));
            }
        }

        return assemblies.Values.ToList();
    }

    private static List<Assembly> CollectAllAssemblies(Model model)
    {
        var assemblies = new Dictionary<int, Assembly>();
        try
        {
            var selector = model.GetModelObjectSelector();
            var enumerator = selector.GetAllObjectsWithType(ModelObject.ModelObjectEnum.ASSEMBLY);
            while (enumerator != null && enumerator.MoveNext())
            {
                if (!(enumerator.Current is Assembly assembly))
                {
                    continue;
                }

                var assemblyId = SafeGetIdentifier(assembly);
                if (assemblyId != 0)
                {
                    assemblies[assemblyId] = assembly;
                }
            }
        }
        catch (Exception ex)
        {
            Log("collect-all-assemblies-error=" + ex);
        }

        return assemblies.Values.ToList();
    }

    private static Assembly? ResolveAssembly(ModelObject? modelObject)
    {
        if (modelObject is Assembly assembly)
        {
            return assembly;
        }

        if (modelObject is Part part)
        {
            return part.GetAssembly();
        }

        return null;
    }

    private static ExportedAssembly? TryBuildAssemblyExport(Model model, Assembly assembly)
    {
        try
        {
            return BuildAssemblyExport(model, assembly);
        }
        catch (Exception ex)
        {
            Log(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "assembly-export-skip assemblyId={0} reason={1}",
                    SafeGetIdentifier(assembly),
                    ex));
            return null;
        }
    }

    private static ExportedAssembly BuildAssemblyExport(Model model, Assembly assembly)
    {
        var mainPart = assembly.GetMainPart() as Part;
        if (mainPart == null)
        {
            throw new InvalidOperationException($"Assembly {assembly.Identifier.ID} does not have a main part.");
        }

        var parts = GetAssemblyParts(assembly)
            .GroupBy(part => part.Identifier.ID)
            .ToDictionary(group => group.Key, group => group.First());

        var exportedParts = parts.Values
            .OrderBy(part => part.Identifier.ID)
            .Select(part => TryBuildPartExport(model, part))
            .Where(part => part != null)
            .Cast<ExportedPart>()
            .ToList();

        var relationships = BuildRelationships(parts.Values.ToList(), exportedParts);
        var assemblyPosition = TryGetStringReportProperty(mainPart, "ASSEMBLY_POS");

        return new ExportedAssembly
        {
            AssemblyId = assembly.Identifier.ID.ToString(CultureInfo.InvariantCulture),
            MainPartId = mainPart.Identifier.ID,
            Parts = exportedParts,
            Relationships = relationships,
            Metadata = new ExportedAssemblyMetadata
            {
                AssemblyPosition = assemblyPosition,
                MainPartName = mainPart.Name
            }
        };
    }

    private static IEnumerable<Part> GetAssemblyParts(Assembly assembly)
    {
        var parts = new List<Part>();
        var mainPart = assembly.GetMainPart() as Part;
        if (mainPart != null)
        {
            parts.Add(mainPart);
        }

        var secondaries = assembly.GetSecondaries();
        foreach (var secondary in secondaries.OfType<Part>())
        {
            parts.Add(secondary);
        }

        return parts
            .Where(part => part != null)
            .GroupBy(part => part.Identifier.ID)
            .Select(group => group.First());
    }

    private static ExportedPart? TryBuildPartExport(Model model, Part part)
    {
        try
        {
            return BuildPartExport(model, part);
        }
        catch (Exception ex)
        {
            Log(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "part-export-skip partId={0} reason={1}",
                    SafeGetIdentifier(part),
                    ex));
            return null;
        }
    }

    private static ExportedPart BuildPartExport(Model model, Part part)
    {
        var runtimeType = MapRuntimeType(part);
        var profileString = part.Profile != null ? part.Profile.ProfileString ?? string.Empty : string.Empty;
        var isPlateLike = part is ContourPlate || IsPlateProfile(profileString);
        var isSpecialShape = runtimeType.IndexOf("bent", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             runtimeType.IndexOf("lofted", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             runtimeType.Equals("PolyBeam", StringComparison.OrdinalIgnoreCase);

        var coordinateSystem = part.GetCoordinateSystem();
        var axisX = Normalize(ToVector(coordinateSystem.AxisX));
        var axisY = Normalize(ToVector(coordinateSystem.AxisY));
        var axisZ = Normalize(Cross(axisX, axisY));

        var globalBox = GetGlobalBoundingBox(part);
        var localBox = GetLocalBoundingBox(model, part, coordinateSystem);
        var thickness = ResolveThickness(part, profileString, localBox);

        ExportedVector3 plateNormal;
        ExportedVector3 plateLongDirection;
        ExportedVector3 plateWidthDirection;
        if (isPlateLike)
        {
            var localLengthX = Math.Abs(localBox.Max.X - localBox.Min.X);
            var localLengthY = Math.Abs(localBox.Max.Y - localBox.Min.Y);
            var useXAsLong = localLengthX >= localLengthY;
            plateNormal = ToExportedVector3(axisZ);
            plateLongDirection = ToExportedVector3(useXAsLong ? axisX : axisY);
            plateWidthDirection = ToExportedVector3(useXAsLong ? axisY : axisX);
        }
        else
        {
            plateNormal = ToExportedVector3(axisZ);
            plateLongDirection = ToExportedVector3(axisX);
            plateWidthDirection = ToExportedVector3(axisY);
        }

        var contourPoints = GetContourPoints(part, coordinateSystem);
        var weldStats = GetWeldStrengths(part);
        var booleanCounts = GetBooleanCounts(part);
        var holeLikeCount = GetHoleLikeCount(part, booleanCounts.Cuts);

        return new ExportedPart
        {
            PartId = part.Identifier.ID,
            RuntimeType = runtimeType,
            Name = part.Name ?? string.Empty,
            Material = part.Material != null ? part.Material.MaterialString ?? string.Empty : string.Empty,
            ProfileString = profileString,
            IsPlateLike = isPlateLike,
            IsSpecialShape = isSpecialShape,
            Centroid = ToExportedVector3(GetCenter(globalBox)),
            ObbDims = ToExportedVector3(new Vector(localBox.Max.X - localBox.Min.X, localBox.Max.Y - localBox.Min.Y, localBox.Max.Z - localBox.Min.Z)),
            BoundingBox = new ExportedBoundingBox
            {
                Min = ToExportedVector3(globalBox.Min),
                Max = ToExportedVector3(globalBox.Max)
            },
            Volume = TryGetDoubleReportProperty(part, "VOLUME"),
            SurfaceArea = TryGetDoubleReportProperty(part, "AREA"),
            Thickness = thickness,
            PlateNormal = plateNormal,
            PlateLongDirection = plateLongDirection,
            PlateWidthDirection = plateWidthDirection,
            ContourVertexCount = contourPoints?.Count ?? 0,
            ConcaveCornerCount = ComputeConcaveCornerCount(contourPoints, coordinateSystem),
            HoleLikeFeatureCount = holeLikeCount,
            BooleanCutCount = booleanCounts.Cuts,
            BooleanAddCount = booleanCounts.Adds,
            ShopWeldDegree = weldStats.Shop,
            SiteWeldDegree = weldStats.Site,
            ContourPoints = contourPoints
        };
    }

    private static List<ExportedRelationship> BuildRelationships(IReadOnlyList<Part> parts, IReadOnlyList<ExportedPart> exportedParts)
    {
        var partIds = new HashSet<int>(parts.Select(part => part.Identifier.ID));
        var relationships = new List<ExportedRelationship>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var part in parts)
        {
            var welds = part.GetWelds();
            while (welds != null && welds.MoveNext())
            {
                if (!(welds.Current is BaseWeld weld) ||
                    !(weld.MainObject is Part main) ||
                    !(weld.SecondaryObject is Part secondary))
                {
                    continue;
                }

                if (!partIds.Contains(main.Identifier.ID) || !partIds.Contains(secondary.Identifier.ID))
                {
                    continue;
                }

                var strength = 0.5 + (Math.Max(weld.SizeAbove, weld.SizeBelow) / 20.0);
                var meta = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}|shop={1}|sizeAbove={2:0.###}|sizeBelow={3:0.###}|around={4}",
                    weld.GetType().Name,
                    weld.ShopWeld,
                    weld.SizeAbove,
                    weld.SizeBelow,
                    weld.AroundWeld);
                AddRelationship(seen, relationships, main.Identifier.ID, secondary.Identifier.ID, "Weld", strength, 1.0, meta);
            }

            var booleans = part.GetBooleans();
            while (booleans != null && booleans.MoveNext())
            {
                if (!(booleans.Current is BooleanPart booleanPart))
                {
                    continue;
                }

                var operativePart = booleanPart.OperativePart;
                if (operativePart == null)
                {
                    continue;
                }

                if (!partIds.Contains(part.Identifier.ID) || !partIds.Contains(operativePart.Identifier.ID))
                {
                    continue;
                }

                AddRelationship(
                    seen,
                    relationships,
                    part.Identifier.ID,
                    operativePart.Identifier.ID,
                    "Boolean",
                    0.8,
                    1.0,
                    booleanPart.Type.ToString());
            }

            var bolts = part.GetBolts();
            while (bolts != null && bolts.MoveNext())
            {
                if (!(bolts.Current is BoltGroup bolt))
                {
                    continue;
                }

                var partToBoltTo = bolt.PartToBoltTo;
                var partToBeBolted = bolt.PartToBeBolted;
                if (partToBoltTo != null && partToBeBolted != null &&
                    partIds.Contains(partToBoltTo.Identifier.ID) &&
                    partIds.Contains(partToBeBolted.Identifier.ID))
                {
                    AddRelationship(
                        seen,
                        relationships,
                        partToBoltTo.Identifier.ID,
                        partToBeBolted.Identifier.ID,
                        "Bolt",
                        0.45,
                        0.9,
                        string.Format(CultureInfo.InvariantCulture, "bolt={0}|size={1:0.###}", bolt.Bolt, bolt.BoltSize));
                }
            }
        }

        AddContactRelationships(exportedParts, seen, relationships);

        return relationships
            .OrderBy(item => item.PartIdA)
            .ThenBy(item => item.PartIdB)
            .ThenBy(item => item.EdgeType)
            .ToList();
    }

    private static void AddContactRelationships(
        IReadOnlyList<ExportedPart> exportedParts,
        HashSet<string> seen,
        List<ExportedRelationship> relationships)
    {
        const double contactMarginMm = 3.0;

        for (var i = 0; i < exportedParts.Count; i++)
        {
            for (var j = i + 1; j < exportedParts.Count; j++)
            {
                var left = exportedParts[i];
                var right = exportedParts[j];
                if (!BoxesIntersect(left.BoundingBox, right.BoundingBox, contactMarginMm))
                {
                    continue;
                }

                AddRelationship(
                    seen,
                    relationships,
                    left.PartId,
                    right.PartId,
                    "Contact",
                    0.35,
                    0.75,
                    $"bbox-contact|margin={contactMarginMm:0.###}");
            }
        }
    }

    private static bool BoxesIntersect(ExportedBoundingBox left, ExportedBoundingBox right, double margin)
    {
        return (left.Min.X - margin) <= right.Max.X &&
               (left.Max.X + margin) >= right.Min.X &&
               (left.Min.Y - margin) <= right.Max.Y &&
               (left.Max.Y + margin) >= right.Min.Y &&
               (left.Min.Z - margin) <= right.Max.Z &&
               (left.Max.Z + margin) >= right.Min.Z;
    }

    private static void AddRelationship(
        HashSet<string> seen,
        List<ExportedRelationship> relationships,
        int partIdA,
        int partIdB,
        string edgeType,
        double strength,
        double geometricSupport,
        string? meta)
    {
        var left = Math.Min(partIdA, partIdB);
        var right = Math.Max(partIdA, partIdB);
        var key = string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", edgeType, left, right);
        if (!seen.Add(key))
        {
            return;
        }

        relationships.Add(new ExportedRelationship
        {
            PartIdA = left,
            PartIdB = right,
            EdgeType = edgeType,
            Strength = Math.Round(strength, 4),
            GeometricSupport = Math.Round(geometricSupport, 4),
            Meta = meta
        });
    }

    private static (double Shop, double Site) GetWeldStrengths(Part part)
    {
        var seen = new HashSet<int>();
        double shop = 0.0;
        double site = 0.0;

        var welds = part.GetWelds();
        while (welds != null && welds.MoveNext())
        {
            if (!(welds.Current is BaseWeld weld))
            {
                continue;
            }

            if (!seen.Add(weld.Identifier.ID))
            {
                continue;
            }

            var magnitude = 1.0 + (Math.Max(weld.SizeAbove, weld.SizeBelow) / 10.0);
            if (weld.ShopWeld)
            {
                shop += magnitude;
            }
            else
            {
                site += magnitude;
            }
        }

        return (Math.Round(shop, 4), Math.Round(site, 4));
    }

    private static (int Cuts, int Adds) GetBooleanCounts(Part part)
    {
        var seen = new HashSet<int>();
        var cuts = 0;
        var adds = 0;

        var booleans = part.GetBooleans();
        while (booleans != null && booleans.MoveNext())
        {
            if (!(booleans.Current is BooleanPart booleanPart))
            {
                continue;
            }

            if (!seen.Add(booleanPart.Identifier.ID))
            {
                continue;
            }

            var typeText = booleanPart.Type.ToString();
            if (typeText.IndexOf("cut", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                cuts++;
            }
            else
            {
                adds++;
            }
        }

        return (cuts, adds);
    }

    private static int GetHoleLikeCount(Part part, int booleanCutCount)
    {
        var seen = new HashSet<int>();
        var bolts = part.GetBolts();
        while (bolts != null && bolts.MoveNext())
        {
            if (bolts.Current is BoltGroup bolt)
            {
                seen.Add(bolt.Identifier.ID);
            }
        }

        return seen.Count + booleanCutCount;
    }

    private static List<ExportedVector3>? GetContourPoints(Part part, CoordinateSystem coordinateSystem)
    {
        if (!(part is ContourPlate contourPlate) || contourPlate.Contour == null)
        {
            return null;
        }

        var points = new List<ExportedVector3>();
        foreach (var item in contourPlate.Contour.ContourPoints.Cast<object>())
        {
            if (item is ContourPoint contourPoint)
            {
                points.Add(new ExportedVector3
                {
                    X = Math.Round(contourPoint.X, 6),
                    Y = Math.Round(contourPoint.Y, 6),
                    Z = Math.Round(contourPoint.Z, 6)
                });
            }
        }

        return points.Count == 0 ? null : points;
    }

    private static int ComputeConcaveCornerCount(List<ExportedVector3>? contourPoints, CoordinateSystem coordinateSystem)
    {
        if (contourPoints == null || contourPoints.Count < 4)
        {
            return 0;
        }

        var local = MatrixFactory.ToCoordinateSystem(coordinateSystem);
        var polygon = contourPoints
            .Select(point => local.Transform(new Point(point.X, point.Y, point.Z)))
            .ToList();

        var signedArea = 0.0;
        for (var i = 0; i < polygon.Count; i++)
        {
            var current = polygon[i];
            var next = polygon[(i + 1) % polygon.Count];
            signedArea += (current.X * next.Y) - (next.X * current.Y);
        }

        var winding = signedArea >= 0 ? 1.0 : -1.0;
        var concave = 0;
        for (var i = 0; i < polygon.Count; i++)
        {
            var prev = polygon[(i - 1 + polygon.Count) % polygon.Count];
            var current = polygon[i];
            var next = polygon[(i + 1) % polygon.Count];
            var cross = ((current.X - prev.X) * (next.Y - current.Y)) - ((current.Y - prev.Y) * (next.X - current.X));
            if ((cross * winding) < -1e-6)
            {
                concave++;
            }
        }

        return concave;
    }

    private static double? ResolveThickness(Part part, string profileString, LocalBoundingBox localBox)
    {
        var reportThickness = TryGetNullableDoubleReportProperty(part, "DIM_A")
                              ?? TryGetNullableDoubleReportProperty(part, "PROFILE.WIDTH")
                              ?? TryGetNullableDoubleReportProperty(part, "PLATE_THICKNESS");
        if (reportThickness.HasValue && reportThickness.Value > 0)
        {
            return reportThickness.Value;
        }

        var parsed = ParsePlateThickness(profileString);
        if (parsed.HasValue && parsed.Value > 0)
        {
            return parsed.Value;
        }

        var dims = new[]
        {
            Math.Abs(localBox.Max.X - localBox.Min.X),
            Math.Abs(localBox.Max.Y - localBox.Min.Y),
            Math.Abs(localBox.Max.Z - localBox.Min.Z)
        }
        .Where(value => value > 0)
        .OrderBy(value => value)
        .ToArray();

        return dims.Length == 0 ? null : dims[0];
    }

    private static double? ParsePlateThickness(string profileString)
    {
        if (string.IsNullOrWhiteSpace(profileString))
        {
            return null;
        }

        var match = Regex.Match(profileString, @"PL\s*([0-9]+(?:\.[0-9]+)?)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        if (!double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return null;
        }

        return value;
    }

    private static bool IsPlateProfile(string profileString)
    {
        if (string.IsNullOrWhiteSpace(profileString))
        {
            return false;
        }

        return profileString.StartsWith("PL", StringComparison.OrdinalIgnoreCase) ||
               profileString.StartsWith("FLAT", StringComparison.OrdinalIgnoreCase);
    }

    private static string MapRuntimeType(Part part)
    {
        if (part is ContourPlate)
        {
            return "ContourPlate";
        }

        if (part is Beam)
        {
            return "Beam";
        }

        if (part is PolyBeam)
        {
            return "PolyBeam";
        }

        return part.GetType().Name;
    }

    private static GlobalBoundingBox GetGlobalBoundingBox(Part part)
    {
        var solid = part.GetSolid();
        return new GlobalBoundingBox(solid.MinimumPoint, solid.MaximumPoint);
    }

    private static LocalBoundingBox GetLocalBoundingBox(Model model, Part part, CoordinateSystem coordinateSystem)
    {
        var handler = model.GetWorkPlaneHandler();
        var current = handler.GetCurrentTransformationPlane();
        try
        {
            handler.SetCurrentTransformationPlane(new TransformationPlane(coordinateSystem));
            var solid = part.GetSolid();
            return new LocalBoundingBox(solid.MinimumPoint, solid.MaximumPoint);
        }
        finally
        {
            handler.SetCurrentTransformationPlane(current);
        }
    }

    private static string TryGetModelPath(Model model)
    {
        try
        {
            return model.GetInfo().ModelPath ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string? TryGetStringReportProperty(ModelObject modelObject, string propertyName)
    {
        string value = string.Empty;
        return modelObject.GetReportProperty(propertyName, ref value) ? value : null;
    }

    private static double TryGetDoubleReportProperty(ModelObject modelObject, string propertyName)
    {
        double value = 0.0;
        return modelObject.GetReportProperty(propertyName, ref value) ? value : 0.0;
    }

    private static double? TryGetNullableDoubleReportProperty(ModelObject modelObject, string propertyName)
    {
        double value = 0.0;
        if (!modelObject.GetReportProperty(propertyName, ref value))
        {
            return null;
        }

        return value;
    }

    private static void WriteJson<T>(T value, string path)
    {
        var serializer = new DataContractJsonSerializer(typeof(T));
        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, value);
            var compactJson = Encoding.UTF8.GetString(stream.ToArray());
            var prettyJson = PrettyPrintJson(compactJson);
            File.WriteAllText(path, prettyJson, new UTF8Encoding(false));
        }
    }

    private static void Log(string message)
    {
        try
        {
            File.AppendAllText(
                LogPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}",
                new UTF8Encoding(false));
        }
        catch
        {
        }
    }

    private static string PrettyPrintJson(string json)
    {
        var output = new StringBuilder();
        var quoted = false;
        var escaped = false;
        var indent = 0;

        foreach (var ch in json)
        {
            if (escaped)
            {
                output.Append(ch);
                escaped = false;
                continue;
            }

            switch (ch)
            {
                case '\\':
                    output.Append(ch);
                    escaped = true;
                    break;
                case '"':
                    output.Append(ch);
                    quoted = !quoted;
                    break;
                case '{':
                case '[':
                    output.Append(ch);
                    if (!quoted)
                    {
                        output.AppendLine();
                        indent++;
                        output.Append(new string(' ', indent * 2));
                    }
                    break;
                case '}':
                case ']':
                    if (quoted)
                    {
                        output.Append(ch);
                    }
                    else
                    {
                        output.AppendLine();
                        indent--;
                        output.Append(new string(' ', indent * 2));
                        output.Append(ch);
                    }
                    break;
                case ',':
                    output.Append(ch);
                    if (!quoted)
                    {
                        output.AppendLine();
                        output.Append(new string(' ', indent * 2));
                    }
                    break;
                case ':':
                    output.Append(ch);
                    if (!quoted)
                    {
                        output.Append(' ');
                    }
                    break;
                default:
                    output.Append(ch);
                    break;
            }
        }

        return output.ToString();
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(invalid.Contains(ch) ? '_' : ch);
        }

        return builder.ToString();
    }

    private static int SafeGetIdentifier(ModelObject? modelObject)
    {
        try
        {
            return modelObject?.Identifier?.ID ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private static ExportedVector3 ToExportedVector3(Point point)
    {
        return new ExportedVector3
        {
            X = Math.Round(point.X, 6),
            Y = Math.Round(point.Y, 6),
            Z = Math.Round(point.Z, 6)
        };
    }

    private static ExportedVector3 ToExportedVector3(Vector vector)
    {
        return new ExportedVector3
        {
            X = Math.Round(vector.X, 6),
            Y = Math.Round(vector.Y, 6),
            Z = Math.Round(vector.Z, 6)
        };
    }

    private static Point GetCenter(GlobalBoundingBox box)
    {
        return new Point(
            (box.Min.X + box.Max.X) / 2.0,
            (box.Min.Y + box.Max.Y) / 2.0,
            (box.Min.Z + box.Max.Z) / 2.0);
    }

    private static Vector ToVector(Vector vector)
    {
        return new Vector(vector.X, vector.Y, vector.Z);
    }

    private static Vector Normalize(Vector vector)
    {
        var length = Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
        return length <= 1e-9
            ? new Vector(0, 0, 0)
            : new Vector(vector.X / length, vector.Y / length, vector.Z / length);
    }

    private static Vector Cross(Vector left, Vector right)
    {
        return new Vector(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X));
    }

    private sealed class GlobalBoundingBox
    {
        public GlobalBoundingBox(Point min, Point max)
        {
            Min = min;
            Max = max;
        }

        public Point Min { get; }

        public Point Max { get; }
    }

    private sealed class LocalBoundingBox
    {
        public LocalBoundingBox(Point min, Point max)
        {
            Min = min;
            Max = max;
        }

        public Point Min { get; }

        public Point Max { get; }
    }

    private sealed class CollectedAssemblies
    {
        public CollectedAssemblies(IReadOnlyList<Assembly> assemblies, string selectionMode)
        {
            Assemblies = assemblies;
            SelectionMode = selectionMode;
        }

        public IReadOnlyList<Assembly> Assemblies { get; }

        public string SelectionMode { get; }
    }
}
