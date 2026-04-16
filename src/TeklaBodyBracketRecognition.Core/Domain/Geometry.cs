using System.Globalization;

namespace TeklaBodyBracketRecognition.Core.Domain;

public readonly record struct Vector3(double X, double Y, double Z)
{
    public static readonly Vector3 Zero = new(0, 0, 0);
    public static readonly Vector3 UnitX = new(1, 0, 0);
    public static readonly Vector3 UnitY = new(0, 1, 0);
    public static readonly Vector3 UnitZ = new(0, 0, 1);

    public double Length => Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

    public Vector3 Normalize()
    {
        var length = Length;
        return length <= 1e-9 ? Zero : this / length;
    }

    public double Dot(in Vector3 other) => (X * other.X) + (Y * other.Y) + (Z * other.Z);

    public Vector3 Cross(in Vector3 other) =>
        new(
            (Y * other.Z) - (Z * other.Y),
            (Z * other.X) - (X * other.Z),
            (X * other.Y) - (Y * other.X));

    public Vector3 ProjectToPlane(in Vector3 normal)
    {
        var n = normal.Normalize();
        return this - (n * Dot(n));
    }

    public static Vector3 operator +(Vector3 left, Vector3 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static Vector3 operator -(Vector3 left, Vector3 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    public static Vector3 operator -(Vector3 value) => new(-value.X, -value.Y, -value.Z);
    public static Vector3 operator *(Vector3 value, double scalar) => new(value.X * scalar, value.Y * scalar, value.Z * scalar);
    public static Vector3 operator /(Vector3 value, double scalar) => scalar == 0 ? Zero : new(value.X / scalar, value.Y / scalar, value.Z / scalar);

    public override string ToString() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"[{X:0.###}, {Y:0.###}, {Z:0.###}]");
}

public readonly record struct Plane(Vector3 Origin, Vector3 Normal);

public readonly record struct BoundingBox(Vector3 Min, Vector3 Max)
{
    public Vector3 Size => new(Max.X - Min.X, Max.Y - Min.Y, Max.Z - Min.Z);

    public Vector3 Center => new((Min.X + Max.X) / 2.0, (Min.Y + Max.Y) / 2.0, (Min.Z + Max.Z) / 2.0);

    public BoundingBox Expand(double margin) =>
        new(
            new Vector3(Min.X - margin, Min.Y - margin, Min.Z - margin),
            new Vector3(Max.X + margin, Max.Y + margin, Max.Z + margin));

    public bool Intersects(BoundingBox other)
    {
        return Min.X <= other.Max.X &&
               Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y &&
               Max.Y >= other.Min.Y &&
               Min.Z <= other.Max.Z &&
               Max.Z >= other.Min.Z;
    }

    public static BoundingBox FromPoints(IEnumerable<Vector3> points)
    {
        var materialized = points.ToArray();
        if (materialized.Length == 0)
        {
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }

        var minX = materialized.Min(p => p.X);
        var minY = materialized.Min(p => p.Y);
        var minZ = materialized.Min(p => p.Z);
        var maxX = materialized.Max(p => p.X);
        var maxY = materialized.Max(p => p.Y);
        var maxZ = materialized.Max(p => p.Z);
        return new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }
}
