using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.HitTesting;

internal static class EntityGrips
{
    public static IReadOnlyList<Grip> GetGrips(Entity entity)
    {
        return entity switch
        {
            LineEntity line => new[]
            {
                new Grip(line.Start, p => line.Start = p),
                new Grip(line.End,   p => line.End = p),
            },
            CircleEntity circle => new[]
            {
                new Grip(circle.Center, p => circle.Center = p),
            },
            RectangleEntity rect => new[]
            {
                new Grip(rect.Corner1, p => rect.Corner1 = p),
                new Grip(rect.Corner2, p => rect.Corner2 = p),
            },
            PolylineEntity polyline => GetPolylineGrips(polyline),
            _ => throw new NotSupportedException($"{entity.GetType().Name}의 그립이 구현되지 않았습니다."),
        };
    }

    private static IReadOnlyList<Grip> GetPolylineGrips(PolylineEntity polyline)
    {
        var grips = new Grip[polyline.Points.Count];

        for (var i = 0; i < polyline.Points.Count; i++)
        {
            var index = i;   // 클로저 캡처용 지역 변수 (i를 그대로 캡처하면 항상 마지막 값으로 고정됨)
            grips[i] = new Grip(polyline.Points[index], p => polyline.Points[index] = p);
        }

        return grips;
    }
}