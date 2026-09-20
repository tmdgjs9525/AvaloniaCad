using System.Numerics;
using KoDrawing.Core;
using Xunit;

namespace KoDrawing.Tests.Viewport;

public class ViewportTransformTests
{
    private static ViewportTransform CreateTransform() => new()
    {
        CameraPosition = new Vector2(100, 50),
        Zoom = 2.0f,
        ViewportCenter = new Vector2(500, 300)
    };

    [Fact]
    public void WorldToScreen_And_ScreenToWorld_ReturnsOriginalPoint()
    {
        var transform = CreateTransform();

        var world = new Vector2(150, 100);

        var screen = transform.WorldToScreen(world);
        var result = transform.ScreenToWorld(screen);

        Assert.Equal(world, result);
    }

    [Fact]
    public void WorldToScreen_Should_ConvertCorrectly()
    {
        var transform = CreateTransform();

        var result = transform.WorldToScreen(new Vector2(150, 100));

        // X: (150-100)*2 + 500 = 600
        // Y: -(100-50)*2 + 300 = 200  (Y축 뒤집힘)
        Assert.Equal(new Vector2(600, 200), result);
    }

    [Fact]
    public void ScreenToWorld_Should_ConvertCorrectly()
    {
        var transform = CreateTransform();

        var result = transform.ScreenToWorld(new Vector2(600, 200));

        Assert.Equal(new Vector2(150, 100), result);
    }

    [Fact]
    public void WorldPoint_AtCameraPosition_IsAtViewportCenter()
    {
        var transform = CreateTransform();

        var result = transform.WorldToScreen(transform.CameraPosition);

        Assert.Equal(transform.ViewportCenter, result);
    }

    [Fact]
    public void PositiveWorldY_GoesUpOnScreen()
    {
        var transform = new ViewportTransform
        {
            Zoom = 2.0f,
            ViewportCenter = new Vector2(400, 225)
        };

        var result = transform.WorldToScreen(new Vector2(10, 10));

        Assert.Equal(420f, result.X);   // 오른쪽으로 이동
        Assert.Equal(205f, result.Y);   // 위쪽으로 이동 (스크린 Y 감소)
    }

    [Fact]
    public void Clone_CopiesValues_AndIsIndependent()
    {
        var original = CreateTransform();

        var clone = original.Clone();
        original.CameraPosition = new Vector2(999, 999);
        original.Zoom = 10f;

        Assert.Equal(new Vector2(100, 50), clone.CameraPosition);
        Assert.Equal(2.0f, clone.Zoom);
        Assert.Equal(new Vector2(500, 300), clone.ViewportCenter);
    }

    [Fact]
    public void Zoom_Zero_Throws()
    {
        var transform = new ViewportTransform();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            transform.Zoom = 0);
    }

    [Fact]
    public void Zoom_Negative_Throws()
    {
        var transform = new ViewportTransform();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            transform.Zoom = -1);
    }

    [Fact]
    public void Zoom_NaN_Throws()
    {
        var transform = new ViewportTransform();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            transform.Zoom = float.NaN);
    }

    [Fact]
    public void Zoom_Infinity_Throws()
    {
        var transform = new ViewportTransform();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            transform.Zoom = float.PositiveInfinity);
    }

    [Fact]
    public void Zoom_ValidValue_IsAccepted()
    {
        var transform = new ViewportTransform
        {
            Zoom = 0.5f
        };

        Assert.Equal(0.5f, transform.Zoom);
    }
    
    [Fact]
    public void PanByScreenDelta_MovesContentWithPointer()
    {
        var transform = CreateTransform();
        var world = new Vector2(150, 100);   // 현재 스크린 (600, 200)

        transform.PanByScreenDelta(new Vector2(30, -20));

        // 포인터가 (+30, -20) 움직였으니 도면 점도 같은 만큼 움직여야 한다
        Assert.Equal(new Vector2(630, 180), transform.WorldToScreen(world));
    }

    [Fact]
    public void ZoomAt_KeepsWorldPointUnderCursor()
    {
        var transform = CreateTransform();
        var anchor = new Vector2(620, 180);
        var before = transform.ScreenToWorld(anchor);

        transform.ZoomAt(anchor, 1.5f);

        Assert.Equal(3.0f, transform.Zoom);
        AssertNear(before, transform.ScreenToWorld(anchor));
    }

    [Fact]
    public void ZoomAt_ClampsToMaxZoom_AndKeepsAnchor()
    {
        var transform = CreateTransform();
        var anchor = new Vector2(620, 180);
        var before = transform.ScreenToWorld(anchor);

        transform.ZoomAt(anchor, 1_000_000f);

        Assert.Equal(ViewportTransform.MaxZoom, transform.Zoom);
        AssertNear(before, transform.ScreenToWorld(anchor));
    }

    [Fact]
    public void ZoomAt_ClampsToMinZoom_AndKeepsAnchor()
    {
        var transform = CreateTransform();
        var anchor = new Vector2(620, 180);
        var before = transform.ScreenToWorld(anchor);

        transform.ZoomAt(anchor, 0.000001f);

        Assert.Equal(ViewportTransform.MinZoom, transform.Zoom);
        AssertNear(before, transform.ScreenToWorld(anchor));
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void ZoomAt_InvalidFactor_Throws(float factor)
    {
        var transform = CreateTransform();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            transform.ZoomAt(Vector2.Zero, factor));
    }
    
    private static void AssertNear(Vector2 expected, Vector2 actual, float tolerance = 1e-3f)
    {
        Assert.True(
            Vector2.Distance(expected, actual) < tolerance,
            $"Expected {expected}, actual {actual}");
    }
}