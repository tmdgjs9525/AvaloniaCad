using System.Numerics;
using Avalonia.Controls;
using KoDrawing.Core;

namespace AvaloniaCad.Editor.Controls;

public partial class DrawingCanvas : UserControl
{
    public ViewportTransform Viewport { get; } = new();
    
    public DrawingCanvas()
    {
        InitializeComponent();
        
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Viewport.ViewportCenter = new Vector2(
            (float)(e.NewSize.Width / 2),
            (float)(e.NewSize.Height / 2));
        Console.WriteLine($"{Viewport.ViewportCenter}");
    }
}