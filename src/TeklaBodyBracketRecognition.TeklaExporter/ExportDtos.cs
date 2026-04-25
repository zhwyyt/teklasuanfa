using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TeklaBodyBracketRecognition.TeklaExporter;

[DataContract]
internal sealed class ExportBundle
{
    [DataMember(Name = "schemaVersion", Order = 1)]
    public string SchemaVersion { get; set; } = "tekla-body-bracket-export.v1";

    [DataMember(Name = "exportedAtUtc", Order = 2)]
    public string ExportedAtUtc { get; set; } = string.Empty;

    [DataMember(Name = "source", Order = 3)]
    public ExportSourceInfo Source { get; set; } = new();

    [DataMember(Name = "assemblies", Order = 4)]
    public List<ExportedAssembly> Assemblies { get; set; } = new();
}

[DataContract]
internal sealed class ExportSourceInfo
{
    [DataMember(Name = "modelPath", Order = 1)]
    public string ModelPath { get; set; } = string.Empty;

    [DataMember(Name = "selectionMode", Order = 2)]
    public string SelectionMode { get; set; } = "selected";

    [DataMember(Name = "exporterVersion", Order = 3)]
    public string ExporterVersion { get; set; } = "0.1.0";
}

[DataContract]
internal sealed class ExportedAssembly
{
    [DataMember(Name = "assemblyId", Order = 1)]
    public string AssemblyId { get; set; } = string.Empty;

    [DataMember(Name = "mainPartId", Order = 2)]
    public int MainPartId { get; set; }

    [DataMember(Name = "parts", Order = 3)]
    public List<ExportedPart> Parts { get; set; } = new();

    [DataMember(Name = "relationships", Order = 4)]
    public List<ExportedRelationship> Relationships { get; set; } = new();

    [DataMember(Name = "metadata", Order = 5)]
    public ExportedAssemblyMetadata Metadata { get; set; } = new();
}

[DataContract]
internal sealed class ExportedAssemblyMetadata
{
    [DataMember(Name = "assemblyPosition", EmitDefaultValue = false, Order = 1)]
    public string? AssemblyPosition { get; set; }

    [DataMember(Name = "mainPartName", EmitDefaultValue = false, Order = 2)]
    public string? MainPartName { get; set; }
}

[DataContract]
internal sealed class ExportedPart
{
    [DataMember(Name = "partId", Order = 1)]
    public int PartId { get; set; }

    [DataMember(Name = "runtimeType", Order = 2)]
    public string RuntimeType { get; set; } = string.Empty;

    [DataMember(Name = "name", Order = 3)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "material", Order = 4)]
    public string Material { get; set; } = string.Empty;

    [DataMember(Name = "profileString", Order = 5)]
    public string ProfileString { get; set; } = string.Empty;

    [DataMember(Name = "isPlateLike", Order = 6)]
    public bool IsPlateLike { get; set; }

    [DataMember(Name = "isSpecialShape", Order = 7)]
    public bool IsSpecialShape { get; set; }

    [DataMember(Name = "centroid", Order = 8)]
    public ExportedVector3 Centroid { get; set; } = new();

    [DataMember(Name = "obbDims", Order = 9)]
    public ExportedVector3 ObbDims { get; set; } = new();

    [DataMember(Name = "boundingBox", Order = 10)]
    public ExportedBoundingBox BoundingBox { get; set; } = new();

    [DataMember(Name = "volume", Order = 11)]
    public double Volume { get; set; }

    [DataMember(Name = "surfaceArea", Order = 12)]
    public double SurfaceArea { get; set; }

    [DataMember(Name = "thickness", EmitDefaultValue = false, Order = 13)]
    public double? Thickness { get; set; }

    [DataMember(Name = "plateNormal", Order = 14)]
    public ExportedVector3 PlateNormal { get; set; } = new();

    [DataMember(Name = "plateLongDirection", Order = 15)]
    public ExportedVector3 PlateLongDirection { get; set; } = new();

    [DataMember(Name = "plateWidthDirection", Order = 16)]
    public ExportedVector3 PlateWidthDirection { get; set; } = new();

    [DataMember(Name = "contourVertexCount", Order = 17)]
    public int ContourVertexCount { get; set; }

    [DataMember(Name = "concaveCornerCount", Order = 18)]
    public int ConcaveCornerCount { get; set; }

    [DataMember(Name = "holeLikeFeatureCount", Order = 19)]
    public int HoleLikeFeatureCount { get; set; }

    [DataMember(Name = "booleanCutCount", Order = 20)]
    public int BooleanCutCount { get; set; }

    [DataMember(Name = "booleanAddCount", Order = 21)]
    public int BooleanAddCount { get; set; }

    [DataMember(Name = "shopWeldDegree", Order = 22)]
    public double ShopWeldDegree { get; set; }

    [DataMember(Name = "siteWeldDegree", Order = 23)]
    public double SiteWeldDegree { get; set; }

    [DataMember(Name = "contourPoints", EmitDefaultValue = false, Order = 24)]
    public List<ExportedVector3>? ContourPoints { get; set; }
}

[DataContract]
internal sealed class ExportedRelationship
{
    [DataMember(Name = "partIdA", Order = 1)]
    public int PartIdA { get; set; }

    [DataMember(Name = "partIdB", Order = 2)]
    public int PartIdB { get; set; }

    [DataMember(Name = "edgeType", Order = 3)]
    public string EdgeType { get; set; } = string.Empty;

    [DataMember(Name = "strength", Order = 4)]
    public double Strength { get; set; }

    [DataMember(Name = "geometricSupport", Order = 5)]
    public double GeometricSupport { get; set; }

    [DataMember(Name = "meta", EmitDefaultValue = false, Order = 6)]
    public string? Meta { get; set; }
}

[DataContract]
internal sealed class ExportedBoundingBox
{
    [DataMember(Name = "min", Order = 1)]
    public ExportedVector3 Min { get; set; } = new();

    [DataMember(Name = "max", Order = 2)]
    public ExportedVector3 Max { get; set; } = new();
}

[DataContract]
internal sealed class ExportedVector3
{
    [DataMember(Name = "x", Order = 1)]
    public double X { get; set; }

    [DataMember(Name = "y", Order = 2)]
    public double Y { get; set; }

    [DataMember(Name = "z", Order = 3)]
    public double Z { get; set; }
}

[DataContract]
internal sealed class AssemblyLabelTemplate
{
    [DataMember(Name = "assemblyId", Order = 1)]
    public string AssemblyId { get; set; } = string.Empty;

    [DataMember(Name = "expectedBodyFamily", EmitDefaultValue = false, Order = 2)]
    public string? ExpectedBodyFamily { get; set; }

    [DataMember(Name = "expectedCoreBodyPartIds", Order = 3)]
    public List<int> ExpectedCoreBodyPartIds { get; set; } = new();

    [DataMember(Name = "expectedAccessoryPartIds", Order = 4)]
    public List<int> ExpectedAccessoryPartIds { get; set; } = new();

    [DataMember(Name = "expectedBrackets", Order = 5)]
    public List<ExpectedBracketLabel> ExpectedBrackets { get; set; } = new();

    [DataMember(Name = "notes", EmitDefaultValue = false, Order = 6)]
    public string? Notes { get; set; }
}

[DataContract]
internal sealed class ExpectedBracketLabel
{
    [DataMember(Name = "type", EmitDefaultValue = false, Order = 1)]
    public string? Type { get; set; }

    [DataMember(Name = "partIds", Order = 2)]
    public List<int> PartIds { get; set; } = new();
}
