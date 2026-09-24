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

        DrawAll(canvas, viewport, entities, paint);
    }
    
    public static void DrawHighlight(SKCanvas canvas, ViewportTransform viewport, Entity entity)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.OrangeRed,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3f,      // 일반 선(1.5f)보다 굵게
            IsAntialias = true
        };

        switch (entity)
        {
            case LineEntity line:
                DrawLine(canvas, viewport, line, paint);
                break;
            case CircleEntity circle:
                DrawCircle(canvas, viewport, circle, paint);
                break;
            case RectangleEntity rect:
                DrawRectangle(canvas, viewport, rect, paint);
                break;
        }
    }
    
    // 그리는 중인 도형: 파란 점선
    public static void DrawPreview(SKCanvas canvas, ViewportTransform viewport, IReadOnlyList<Entity> entities)
    {
        if (entities.Count == 0)
            return;

        using var dash = SKPathEffect.CreateDash(new[] { 6f, 4f }, 0f);
        using var paint = new SKPaint
        {
            Color = SKColors.DodgerBlue,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true,
            PathEffect = dash
        };

        DrawAll(canvas, viewport, entities, paint);
    }

    private static void DrawAll(SKCanvas canvas, ViewportTransform viewport, IReadOnlyList<Entity> entities, SKPaint paint)
    {
        foreach (var entity in entities)
        {
            switch (entity)
            {
                case LineEntity line:
                    DrawLine(canvas, viewport, line, paint);
                    break;
                case CircleEntity circle:
                    DrawCircle(canvas, viewport, circle, paint);
                    break;
                case RectangleEntity rect:
                    DrawRectangle(canvas, viewport, rect, paint);
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
        var r = circle.Radius * viewport.Zoom;
        canvas.DrawCircle(c.X, c.Y, r, paint);
    }
    
    private static void DrawRectangle(SKCanvas canvas, ViewportTransform viewport, RectangleEntity rect, SKPaint paint)
    {
        var a = viewport.WorldToScreen(rect.Corner1);
        var b = viewport.WorldToScreen(rect.Corner2);

        // 화면 좌표는 Y가 뒤집혀 있을 수 있으므로, 어느 점이 위/아래인지 따지지 않고
        // Min/Max로 항상 올바른 사각형을 만든다.
        var left = MathF.Min(a.X, b.X);
        var right = MathF.Max(a.X, b.X);
        var top = MathF.Min(a.Y, b.Y);
        var bottom = MathF.Max(a.Y, b.Y);

        canvas.DrawRect(new SKRect(left, top, right, bottom), paint);
    }
}