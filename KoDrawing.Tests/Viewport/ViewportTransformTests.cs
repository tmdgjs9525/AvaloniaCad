using System.Numerics;
using KoDrawing.Core;
using Xunit;

namespace KoDrawing.Tests.Viewport;

public class ViewportTransformTests
{
    [Fact]
    public void WorldToScreen_And_ScreenToWorld_ReturnsOriginalPoint()
    {
        var transform = new ViewportTransform
        {
            CameraPosition = new Vector2(100, 50),
            Zoom = 2.0f,
            ViewportCenter = new Vector2(500, 300)
        };

        var world = new Vector2(150, 100);

        var screen = transform.WorldToScreen(world);
        var result = transform.ScreenToWorld(screen);

        Assert.Equal(world, result);
    }
    
    [Fact]
    public void WorldToScreen_Should_ConvertCorrectly()
    {
        var transform = new ViewportTransform
        {
            CameraPosition = new Vector2(100, 50),
            Zoom = 2.0f,
            ViewportCenter = new Vector2(500, 300)
        };

        var result = transform.WorldToScreen(new Vector2(150, 100));

        Assert.Equal(new Vector2(600, 400), result);
    }
    
    [Fact]
    public void ScreenToWorld_Should_ConvertCorrectly()
    {
        var transform = new ViewportTransform
        {
            CameraPosition = new Vector2(100, 50),
            Zoom = 2.0f,
            ViewportCenter = new Vector2(500, 300)
        };

        var result = transform.ScreenToWorld(new Vector2(600, 400));

        Assert.Equal(new Vector2(150, 100), result);
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
}