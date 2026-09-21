using KoDrawing.Core;
using KoDrawing.Core.Entities;
using SkiaSharp;

namespace AvaloniaCad.Editor.Rendering;

internal static class EntityRenderer
{
    public static void Draw(SKCanvas canvas, ViewportTransform viewport, IReadOnlyList<Entity> entities)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };
        
        foreach (var entity in entities)   // document.Entities → entities
        {
            switch (entity)
            {
                case LineEntity line:
                    DrawLine(canvas, viewport, line, paint);
                    break;
                case CircleEntity circle:
                    DrawCircle(canvas, viewport, circle, paint);
                    break;
            }
        }
    }

    private static void DrawLine(SKCanvas canvas, ViewportTransform viewport, LineEntity line, SKPaint paint)
    {
        var a = viewport.WorldToScreen(line.Start);
        var b = viewport.WorldToScreen(line.End);
        canvas.DrawLine(a.X, a.Y, b.X, b.Y, paint);
    }

    private static void DrawCircle(SKCanvas canvas, ViewportTransform viewport, CircleEntity circle, SKPaint paint)
    {
        var c = viewport.WorldToScreen(circle.Center);
        var r = circle.Radius * viewport.Zoom;   // mm → 픽셀
        canvas.DrawCircle(c.X, c.Y, r, paint);
    }
}