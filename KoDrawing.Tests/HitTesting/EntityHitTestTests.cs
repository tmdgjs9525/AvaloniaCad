using System.Numerics;
using KoDrawing.Core.Entities;
using KoDrawing.Core.HitTesting;
using Xunit;

namespace KoDrawing.Tests.HitTesting;

public class EntityHitTestTests
{
    private const float Tolerance = 1f;

    // ───────── Line ─────────

    [Theory]
    [InlineData(5f, 0.5f, true)]    // 선 위 근처
    [InlineData(5f, 2f, false)]     // 허용 오차 밖
    [InlineData(10.5f, 0f, true)]   // 끝점 바로 바깥 (끝점까지 거리 0.5)
    [InlineData(12f, 0f, false)]    // 선분 연장선 위지만 끝점에서 멀다
    [InlineData(-0.8f, 0f, true)]   // 시작점 바로 바깥
    public void Line_HitTest(float x, float y, bool expected)
    {
        var line = new LineEntity { Start = new Vector2(0, 0), End = new Vector2(10, 0) };

        Assert.Equal(expected, EntityHitTest.HitTest(line, new Vector2(x, y), Tolerance));
    }

    [Fact]
    public void Line_WithZeroLength_UsesDistanceToPoint()
    {
        var line = new LineEntity { Start = new Vector2(3, 3), End = new Vector2(3, 3) };

        Assert.True(EntityHitTest.HitTest(line, new Vector2(3, 3.5f), Tolerance));
        Assert.False(EntityHitTest.HitTest(line, new Vector2(3, 5), Tolerance));
    }

    // ───────── Circle ─────────

    [Theory]
    [InlineData(10f, 0f, true)]     // 원 둘레
    [InlineData(0f, -10.5f, true)]  // 둘레 바로 바깥
    [InlineData(0f, 0f, false)]     // 중심 (원 안쪽은 선택되지 않음)
    [InlineData(5f, 0f, false)]     // 원 안쪽
    [InlineData(12f, 0f, false)]    // 원 바깥
    public void Circle_HitTest_OnlyNearCircumference(float x, float y, bool expected)
    {
        var circle = new CircleEntity { Center = Vector2.Zero, Radius = 10 };

        Assert.Equal(expected, EntityHitTest.HitTest(circle, new Vector2(x, y), Tolerance));
    }

    // ───────── Rectangle ─────────

    [Theory]
    [InlineData(5f, 0f, true)]      // 아래 변
    [InlineData(5f, 5f, true)]      // 위 변
    [InlineData(0f, 2.5f, true)]    // 왼쪽 변
    [InlineData(10.5f, 2.5f, true)] // 오른쪽 변 바로 바깥
    [InlineData(5f, 2.5f, false)]   // 사각형 안쪽
    [InlineData(15f, 2.5f, false)]  // 바깥
    public void Rectangle_HitTest_OnlyNearEdges(float x, float y, bool expected)
    {
        var rect = new RectangleEntity { Corner1 = new Vector2(0, 0), Corner2 = new Vector2(10, 5) };

        Assert.Equal(expected, EntityHitTest.HitTest(rect, new Vector2(x, y), Tolerance));
    }

    [Fact]
    public void Rectangle_CornerOrder_DoesNotMatter()
    {
        var normal = new RectangleEntity { Corner1 = new Vector2(0, 0), Corner2 = new Vector2(10, 5) };
        var reversed = new RectangleEntity { Corner1 = new Vector2(10, 5), Corner2 = new Vector2(0, 0) };

        foreach (var point in new[] { new Vector2(5, 0), new Vector2(10, 2.5f), new Vector2(5, 2.5f) })
        {
            Assert.Equal(
                EntityHitTest.HitTest(normal, point, Tolerance),
                EntityHitTest.HitTest(reversed, point, Tolerance));
        }
    }

    // ───────── Polyline ─────────

    [Theory]
    [InlineData(5f, 0f, true)]      // 첫 번째 선분
    [InlineData(10f, 5f, true)]     // 두 번째 선분
    [InlineData(5f, 5f, false)]     // 두 선분 사이 빈 곳 (닫힌 도형이 아님)
    public void Polyline_HitTest_ChecksEverySegment(float x, float y, bool expected)
    {
        var polyline = new PolylineEntity
        {
            Points = { new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 10) }
        };

        Assert.Equal(expected, EntityHitTest.HitTest(polyline, new Vector2(x, y), Tolerance));
    }

    [Fact]
    public void Polyline_WithSinglePoint_IsNeverHit()
    {
        var polyline = new PolylineEntity { Points = { new Vector2(0, 0) } };

        Assert.False(EntityHitTest.HitTest(polyline, new Vector2(0, 0), Tolerance));
    }

    // ───────── 기타 ─────────

    [Fact]
    public void UnknownEntity_Throws()
    {
        Assert.Throws<NotSupportedException>(
            () => EntityHitTest.HitTest(new UnknownEntity(), Vector2.Zero, Tolerance));
    }
}
