using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using KoDrawing.Core;
using KoDrawing.Core.Entities;
using SkiaSharp;

namespace AvaloniaCad.Editor.Rendering;

public sealed class SkiaDrawOperation : ICustomDrawOperation
{
    private readonly ViewportTransform _viewport;

    public Rect Bounds { get; }
    private readonly IReadOnlyList<Entity> _entities;      // 필드 추가

    public SkiaDrawOperation(Rect bounds, ViewportTransform viewport, IReadOnlyList<Entity> entities)
    {
        Bounds = bounds;
        _viewport = viewport;
        _entities = entities;
    }
    
    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null)
            return;

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;

        var w = (float)Bounds.Width;
        var h = (float)Bounds.Height;

        canvas.Save();
        try
        {
            canvas.ClipRect(new SKRect(0, 0, w, h));
            canvas.Clear(SKColors.White);

            GridRenderer.Draw(canvas, _viewport, w, h);
            EntityRenderer.Draw(canvas, _viewport, _entities);
            // 월드 원점 축 (X: 빨강, Y: 초록)
            var origin = _viewport.WorldToScreen(Vector2.Zero);

            using var axisPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            axisPaint.Color = SKColors.IndianRed;
            canvas.DrawLine(0, origin.Y, w, origin.Y, axisPaint);

            axisPaint.Color = SKColors.SeaGreen;
            canvas.DrawLine(origin.X, 0, origin.X, h, axisPaint);
        }
        finally
        {
            canvas.Restore();
        }
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