using System.Numerics;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaCad.Editor.Rendering;
using KoDrawing.Core;

namespace AvaloniaCad.Editor.Controls;

public partial class DrawingCanvas : Control
{
    private ViewportTransform Viewport { get; } = new();
    
    public DrawingCanvas()
    {
        InitializeComponent();
        
        SizeChanged += OnSizeChanged;
    }
    
    public override void Render(DrawingContext context)
    {
        context.Custom(new SkiaDrawOperation(
            Bounds,
            Viewport));
    }
    
    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Viewport.ViewportCenter = new Vector2(
            (float)(e.NewSize.Width / 2),
            (float)(e.NewSize.Height / 2));
        Console.WriteLine($"{Viewport.ViewportCenter}");
    }
}