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

    public CadScene(
        ViewportTransform viewport,
        IReadOnlyList<Entity> entities,
        IReadOnlyList<Entity> preview,
        IReadOnlyList<Grip> grips,
        Entity? selectedEntity = null)
    {
        _viewport = viewport;
        _entities = entities;
        _preview = preview;
        _grips = grips;
        _selectedEntity = selectedEntity;
    }

    public void Render(SKCanvas canvas, float w, float h)
    {
        canvas.Save();
        try
        {
            canvas.ClipRect(new SKRect(0, 0, w, h));
            canvas.Clear(SKColors.White);

            GridRenderer.Draw(canvas, _viewport, w, h);
            EntityRenderer.Draw(canvas, _viewport, _entities);
            EntityRenderer.DrawPreview(canvas, _viewport, _preview);
            EntityRenderer.DrawGrips(canvas, _viewport, _grips);

            if (_selectedEntity is not null)
                EntityRenderer.DrawHighlight(canvas, _viewport, _selectedEntity);

            DrawAxes(canvas, w, h);
        }
        finally
        {
            canvas.Restore();
        }
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