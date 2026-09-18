using System.Numerics;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaCad.Editor.Rendering;
using KoDrawing.Core;

namespace AvaloniaCad.Editor.Controls;

public class DrawingCanvas : Control
{
    private ViewportTransform Viewport { get; } = new();

    public DrawingCanvas()
    {
        SizeChanged += OnSizeChanged;
    }

    public override void Render(DrawingContext context)
    {
        Console.WriteLine("DrawingCanvas.Render");

        context.Custom(
            new SkiaDrawOperation(
                Bounds,
                Viewport));
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Viewport.ViewportCenter = new Vector2(
            (float)(e.NewSize.Width / 2),
            (float)(e.NewSize.Height / 2));

        InvalidateVisual();

        Console.WriteLine($"Size: {e.NewSize}");
    }
}