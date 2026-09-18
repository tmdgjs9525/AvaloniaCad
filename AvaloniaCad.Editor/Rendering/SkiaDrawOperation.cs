using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using KoDrawing.Core;
using SkiaSharp;

namespace AvaloniaCad.Editor.Rendering;

public sealed class SkiaDrawOperation : ICustomDrawOperation
{
    private readonly ViewportTransform _viewport;

    public Rect Bounds { get; }

    public SkiaDrawOperation(
        Rect bounds,
        ViewportTransform viewport)
    {
        Bounds = bounds;
        _viewport = viewport;
    }

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature =
            context.TryGetFeature<ISkiaSharpApiLeaseFeature>();

        if (leaseFeature is null)
            return;

        using var lease = leaseFeature.Lease();

        var canvas = lease.SkCanvas;

        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Fill
        };

        var screen = _viewport.WorldToScreen(Vector2.Zero);

        canvas.DrawCircle(
            screen.X,
            screen.Y,
            5,
            paint);
    }

    public bool HitTest(Point p)
    {
        return Bounds.Contains(p);
    }

    public bool Equals(ICustomDrawOperation? other)
    {
        return other is SkiaDrawOperation;
    }

    public void Dispose()
    {
    }
}