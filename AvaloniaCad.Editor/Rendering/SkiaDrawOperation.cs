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

        var center = _viewport.WorldToScreen(Vector2.Zero);

        using var paint = new SKPaint
        {
            Color = SKColors.Gray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };

        canvas.DrawLine(
            0,
            center.Y,
            (float)Bounds.Width,
            center.Y,
            paint);

        canvas.DrawLine(
            center.X,
            0,
            center.X,
            (float)Bounds.Height,
            paint);
        
        // 그려지는지 테스트용 코드 (빨간 동그라미)
        using var pointPaint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Fill
        };

        canvas.DrawCircle(
            center.X,
            center.Y,
            20,
            pointPaint);
        
        Console.WriteLine(_viewport.WorldToScreen(Vector2.Zero));
    }

    public bool HitTest(Point p)
    {
        return Bounds.Contains(p);
    }

    public bool Equals(ICustomDrawOperation? other)
    {
        return false;
    }

    public void Dispose()
    {
    }
}