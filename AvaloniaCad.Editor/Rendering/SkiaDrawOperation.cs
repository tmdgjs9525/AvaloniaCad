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
    private readonly IReadOnlyList<Entity> _entities;
    private readonly IReadOnlyList<Entity> _preview;
    private readonly Entity? _selectedEntity;

    public Rect Bounds { get; }

    public SkiaDrawOperation(
        Rect bounds,
        ViewportTransform viewport,
        IReadOnlyList<Entity> entities,
        IReadOnlyList<Entity> preview,
        Entity? selectedEntity = null)
    {
        Bounds = bounds;
        _viewport = viewport;
        _entities = entities;
        _preview = preview;
        _selectedEntity = selectedEntity;
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
            EntityRenderer.DrawPreview(canvas, _viewport, _preview);
            if (_selectedEntity is not null)
                EntityRenderer.DrawHighlight(canvas, _viewport, _selectedEntity);
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