using System.Numerics;
using KoDrawing.Core.Entities;
using KoDrawing.Core.HitTesting;
using Xunit;

namespace KoDrawing.Tests.HitTesting;

public class EntityGripsTests
{
    [Fact]
    public void Line_HasStartAndEndGrips()
    {
        var line = new LineEntity { Start = new Vector2(1, 2), End = new Vector2(3, 4) };

        var grips = EntityGrips.GetGrips(line);

        Assert.Equal(2, grips.Count);
        Assert.Equal(line.Start, grips[0].Position);
        Assert.Equal(line.End, grips[1].Position);
    }

    [Fact]
    public void MovingLineEndGrip_ChangesOnlyEnd()
    {
        var line = new LineEntity { Start = new Vector2(1, 2), End = new Vector2(3, 4) };
        var grips = EntityGrips.GetGrips(line);

        grips[1].MoveTo(new Vector2(20, 30));

        Assert.Equal(new Vector2(1, 2), line.Start);
        Assert.Equal(new Vector2(20, 30), line.End);
        Assert.Equal(new Vector2(20, 30), grips[1].Position);
    }

    [Fact]
    public void Circle_CenterGrip_MovesCenterAndKeepsRadius()
    {
        var circle = new CircleEntity { Center = Vector2.Zero, Radius = 5 };
        var grips = EntityGrips.GetGrips(circle);

        Assert.Single(grips);

        grips[0].MoveTo(new Vector2(7, 8));

        Assert.Equal(new Vector2(7, 8), circle.Center);
        Assert.Equal(5f, circle.Radius);
    }

    [Fact]
    public void Rectangle_HasTwoCornerGrips()
    {
        var rect = new RectangleEntity { Corner1 = new Vector2(0, 0), Corner2 = new Vector2(10, 5) };
        var grips = EntityGrips.GetGrips(rect);

        Assert.Equal(2, grips.Count);

        grips[0].MoveTo(new Vector2(-1, -1));

        Assert.Equal(new Vector2(-1, -1), rect.Corner1);
        Assert.Equal(new Vector2(10, 5), rect.Corner2);
    }

    [Fact]
    public void Polyline_EachGrip_MovesItsOwnPoint()
    {
        // 클로저가 반복 변수를 잘못 캡처하면 모든 그립이 마지막 점을 움직이게 된다
        var polyline = new PolylineEntity
        {
            Points = { new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 10) }
        };
        var grips = EntityGrips.GetGrips(polyline);

        Assert.Equal(3, grips.Count);

        grips[1].MoveTo(new Vector2(5, 5));

        Assert.Equal(new Vector2(0, 0), polyline.Points[0]);
        Assert.Equal(new Vector2(5, 5), polyline.Points[1]);
        Assert.Equal(new Vector2(10, 10), polyline.Points[2]);
    }

    [Fact]
    public void UnknownEntity_Throws()
    {
        Assert.Throws<NotSupportedException>(() => EntityGrips.GetGrips(new UnknownEntity()));
    }
}
