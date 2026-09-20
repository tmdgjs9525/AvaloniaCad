using System.Numerics;
using KoDrawing.Core;
using SkiaSharp;

namespace AvaloniaCad.Editor.Rendering;

internal static class GridRenderer
{
    private const float MinPixelSpacing = 12f;
    private const int MajorEvery = 10;

    public static void Draw(SKCanvas canvas, ViewportTransform viewport, float width, float height)
    {
        var spacing = ChooseSpacing(viewport.Zoom);

        // 화면 네 모서리에 해당하는 월드 범위 (Y가 뒤집혀 있어서 topLeft.Y가 최댓값)
        var topLeft = viewport.ScreenToWorld(new Vector2(0, 0));
        var bottomRight = viewport.ScreenToWorld(new Vector2(width, height));

        using var minorPaint = CreatePaint(new SKColor(0xEE, 0xEE, 0xEE));
        using var majorPaint = CreatePaint(new SKColor(0xD4, 0xD4, 0xD4));

        // 세로선
        var i0 = (int)MathF.Floor(topLeft.X / spacing);
        var i1 = (int)MathF.Ceiling(bottomRight.X / spacing);

        for (var i = i0; i <= i1; i++)
        {
            var sx = viewport.WorldToScreen(new Vector2(i * spacing, 0)).X;
            var x = MathF.Floor(sx) + 0.5f;   // 1px 선이 또렷하게 보이도록 픽셀 중앙에 맞춤
            canvas.DrawLine(x, 0, x, height, i % MajorEvery == 0 ? majorPaint : minorPaint);
        }

        // 가로선
        var j0 = (int)MathF.Floor(bottomRight.Y / spacing);
        var j1 = (int)MathF.Ceiling(topLeft.Y / spacing);

        for (var j = j0; j <= j1; j++)
        {
            var sy = viewport.WorldToScreen(new Vector2(0, j * spacing)).Y;
            var y = MathF.Floor(sy) + 0.5f;
            canvas.DrawLine(0, y, width, y, j % MajorEvery == 0 ? majorPaint : minorPaint);
        }
    }

    // 화면상 간격이 MinPixelSpacing 이상이 되는 가장 작은 10의 거듭제곱(mm)
    private static float ChooseSpacing(float zoom)
    {
        var targetMm = MinPixelSpacing / zoom;
        return MathF.Pow(10f, MathF.Ceiling(MathF.Log10(targetMm)));
    }

    private static SKPaint CreatePaint(SKColor color) => new()
    {
        Color = color,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1,
        IsAntialias = false
    };
}