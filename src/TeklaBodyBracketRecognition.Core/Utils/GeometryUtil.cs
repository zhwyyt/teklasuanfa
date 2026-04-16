using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Utils;

internal static class GeometryUtil
{
    public static double Clamp01(double value) => Math.Max(0.0, Math.Min(1.0, value));

    public static Vector3 AlignDirection(Vector3 vector, Vector3 reference)
    {
        var normalized = vector.Normalize();
        var referenceNormalized = reference.Normalize();
        if (normalized.Length <= 1e-9)
        {
            return referenceNormalized.Length <= 1e-9 ? Vector3.UnitX : referenceNormalized;
        }

        return normalized.Dot(referenceNormalized) < 0 ? -normalized : normalized;
    }

    public static double AngleBetweenDeg(Vector3 left, Vector3 right)
    {
        var l = left.Normalize();
        var r = right.Normalize();
        var dot = Math.Max(-1.0, Math.Min(1.0, l.Dot(r)));
        return Math.Acos(dot) * 180.0 / Math.PI;
    }

    public static double ParallelAngleDeg(Vector3 left, Vector3 right)
    {
        var angle = AngleBetweenDeg(left, right);
        return Math.Min(angle, 180.0 - angle);
    }

    public static double ProjectScalar(Vector3 point, Vector3 axis) => point.Dot(axis.Normalize());

    public static double IntervalOverlap((double Min, double Max) a, (double Min, double Max) b) =>
        Math.Max(0.0, Math.Min(a.Max, b.Max) - Math.Max(a.Min, b.Min));

    public static double IntervalGap((double Min, double Max) a, (double Min, double Max) b)
    {
        if (IntervalOverlap(a, b) > 0)
        {
            return 0;
        }

        if (a.Max < b.Min)
        {
            return b.Min - a.Max;
        }

        return a.Min - b.Max;
    }
}
