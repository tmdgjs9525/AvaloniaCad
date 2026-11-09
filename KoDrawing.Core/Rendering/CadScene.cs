using System.Numerics;
using KoDrawing.Core.Entities;
using KoDrawing.Core.HitTesting;
using SkiaSharp;

namespace KoDrawing.Core.Rendering;

public sealed class CadScene
{
    private readonly ViewportTransform _viewport;
    private readonly IReadOnlyList<Entity> _entities;
    private readonly IReadOnlyList<Entity> _preview;
    private readonly IReadOnlyList<Grip> _grips;
    private readonly Entity? _selectedEntity;
    private readonly Entity? _hoveredEntity;
    private readonly Vector2? _hoveredGripPosition;
    private readonly Vector2? _snapMarker;

    public CadScene(
        ViewportTransform viewport,
        IReadOnlyList<Entity> entities,
        IReadOnlyList<Entity> preview,
        IReadOnlyList<Grip> grips,
        Entity? selectedEntity = null,
        Entity? hoveredEntity = null,
        Vector2? hoveredGripPosition = null,
        Vector2? snapMarker = null)
    {
        _viewport = viewport;
        _entities = entities;
        _preview = preview;
        _grips = grips;
        _selectedEntity = selectedEntity;
        _hoveredEntity = hoveredEntity;
        _hoveredGripPosition = hoveredGripPosition;
        _snapMarker = snapMarker;
    }

    public void Render(SKCanvas canvas, float w, float h)
    {
        canvas.Save();
        try
        {
            canvas.ClipRect(new SKRect(0, 0, w, h));
            canvas.Clear(SKColors.White);

            // 아래 → 위 순서: 그리드, 축, 도형, hover, 미리보기, 선택 강조, 그립, 스냅 마커
            GridRenderer.Draw(canvas, _viewport, w, h);
            DrawAxes(canvas, w, h);
            EntityRenderer.Draw(canvas, _viewport, _entities);

            if (_hoveredEntity is not null && !ReferenceEquals(_hoveredEntity, _selectedEntity))
                EntityRenderer.DrawHover(canvas, _viewport, _hoveredEntity);

            EntityRenderer.DrawPreview(canvas, _viewport, _preview);

            if (_selectedEntity is not null)
                EntityRenderer.DrawHighlight(canvas, _viewport, _selectedEntity);

            EntityRenderer.DrawGrips(canvas, _viewport, _grips, _hoveredGripPosition);

            if (_snapMarker is { } snap)
                DrawSnapMarker(canvas, snap);
        }
        finally
        {
            canvas.Restore();
        }
    }

    // 스냅된 위치에 작은 사각형 + 십자 표시
    private void DrawSnapMarker(SKCanvas canvas, Vector2 world)
    {
        var p = _viewport.WorldToScreen(world);
        const float half = 5f;

        using var paint = new SKPaint
        {
            Color = SKColors.SeaGreen,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };

        canvas.DrawRect(new SKRect(p.X - half, p.Y - half, p.X + half, p.Y + half), paint);
        canvas.DrawLine(p.X - half * 2, p.Y, p.X + half * 2, p.Y, paint);
        canvas.DrawLine(p.X, p.Y - half * 2, p.X, p.Y + half * 2, paint);
    }

    private void DrawAxes(SKCanvas canvas, float w, float h)
    {
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
}