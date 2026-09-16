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
}