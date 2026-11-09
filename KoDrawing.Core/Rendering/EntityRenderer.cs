using System.Diagnostics;
using System.Numerics;
using KoDrawing.Core.Entities;
using KoDrawing.Core.HitTesting;
using SkiaSharp;

namespace KoDrawing.Core.Rendering;

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
            StrokeWidth = 3f,
            IsAntialias = true
        };

        DrawOne(canvas, viewport, entity, paint);
    }

    public static void DrawHover(SKCanvas canvas, ViewportTransform viewport, Entity entity)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.DodgerBlue.WithAlpha(150),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3f,
            IsAntialias = true
        };

        DrawOne(canvas, viewport, entity, paint);
    }

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

    /// <summary>그립을 그린다. hoveredPosition과 같은 그립은 크게 채워서 강조.</summary>
    public static void DrawGrips(SKCanvas canvas, ViewportTransform viewport, IReadOnlyList<Grip> grips, Vector2? hoveredPosition = null)
    {
        using var fill = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill, IsAntialias = true };
        using var hotFill = new SKPaint { Color = SKColors.OrangeRed, Style = SKPaintStyle.Fill, IsAntialias = true };
        using var border = new SKPaint { Color = SKColors.OrangeRed, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };

        foreach (var grip in grips)
        {
            var isHot = hoveredPosition is { } hp && hp == grip.Position;
            var half = isHot ? 5f : 4f;

            var p = viewport.WorldToScreen(grip.Position);
            var rect = new SKRect(p.X - half, p.Y - half, p.X + half, p.Y + half);
            canvas.DrawRect(rect, isHot ? hotFill : fill);
            canvas.DrawRect(rect, border);
        }
    }

    private static void DrawAll(SKCanvas canvas, ViewportTransform viewport, IReadOnlyList<Entity> entities, SKPaint paint)
    {
        foreach (var entity in entities)
        {
            try
            {
                DrawOne(canvas, viewport, entity, paint);
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

    private static void DrawOne(SKCanvas canvas, ViewportTransform viewport, Entity entity, SKPaint paint)
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
            case PolylineEntity polyline:
                DrawPolyline(canvas, viewport, polyline, paint);
                break;
            default:
                throw new NotSupportedException($"{entity.GetType().Name} 렌더링 미구현");
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

        var left = MathF.Min(a.X, b.X);
        var right = MathF.Max(a.X, b.X);
        var top = MathF.Min(a.Y, b.Y);
        var bottom = MathF.Max(a.Y, b.Y);

        canvas.DrawRect(new SKRect(left, top, right, bottom), paint);
    }

    private static void DrawPolyline(SKCanvas canvas, ViewportTransform viewport, PolylineEntity polyline, SKPaint paint)
    {
        if (polyline.Points.Count < 2)
            return;

        // 하나의 경로로 그려야 꺾이는 부분이 자연스럽게 이어지고 점선 패턴도 끊기지 않는다
        // SkiaSharp 4부터 SKPath는 불변 객체라서 SKPathBuilder로 만든 뒤 Detach()로 꺼낸다
        using var builder = new SKPathBuilder();

        var first = viewport.WorldToScreen(polyline.Points[0]);
        builder.MoveTo(first.X, first.Y);

        for (var i = 1; i < polyline.Points.Count; i++)
        {
            var p = viewport.WorldToScreen(polyline.Points[i]);
            builder.LineTo(p.X, p.Y);
        }

        using var path = builder.Detach();
        canvas.DrawPath(path, paint);
    }
}