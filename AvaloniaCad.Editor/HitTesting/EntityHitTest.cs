using System.Numerics;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.HitTesting;

internal static class EntityHitTest
{
    public static bool HitTest(Entity entity, Vector2 point, float tolerance)
    {
        return entity switch
        {
            LineEntity line => DistanceToSegment(point, line.Start, line.End) <= tolerance,
            CircleEntity circle => HitTestCircle(point, circle, tolerance),
            RectangleEntity rect => HitTestRectangle(point, rect, tolerance),
            PolylineEntity polyline => HitTestPolyline(point, polyline, tolerance),
            _ => throw new NotSupportedException($"{entity.GetType().Name}의 히트 테스트가 구현되지 않았습니다."),
        };
    }

    private static bool HitTestCircle(Vector2 point, CircleEntity circle, float tolerance)
    {
        var distToCenter = Vector2.Distance(point, circle.Center);
        return MathF.Abs(distToCenter - circle.Radius) <= tolerance;
    }

    private static bool HitTestRectangle(Vector2 point, RectangleEntity rect, float tolerance)
    {
        var (min, max) = (
            new Vector2(MathF.Min(rect.Corner1.X, rect.Corner2.X), MathF.Min(rect.Corner1.Y, rect.Corner2.Y)),
            new Vector2(MathF.Max(rect.Corner1.X, rect.Corner2.X), MathF.Max(rect.Corner1.Y, rect.Corner2.Y)));

        var topLeft = new Vector2(min.X, max.Y);
        var topRight = new Vector2(max.X, max.Y);
        var bottomLeft = new Vector2(min.X, min.Y);
        var bottomRight = new Vector2(max.X, min.Y);

        return DistanceToSegment(point, topLeft, topRight) <= tolerance
            || DistanceToSegment(point, topRight, bottomRight) <= tolerance
            || DistanceToSegment(point, bottomRight, bottomLeft) <= tolerance
            || DistanceToSegment(point, bottomLeft, topLeft) <= tolerance;
    }

    private static bool HitTestPolyline(Vector2 point, PolylineEntity polyline, float tolerance)
    {
        for (var i = 0; i < polyline.Points.Count - 1; i++)
        {
            if (DistanceToSegment(point, polyline.Points[i], polyline.Points[i + 1]) <= tolerance)
                return true;
        }

        return false;
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var lengthSq = ab.LengthSquared();

        if (lengthSq < 0.0001f)
            return Vector2.Distance(p, a);

        var t = Vector2.Dot(p - a, ab) / lengthSq;
        t = Math.Clamp(t, 0f, 1f);

        var closest = a + ab * t;
        return Vector2.Distance(p, closest);
    }
}