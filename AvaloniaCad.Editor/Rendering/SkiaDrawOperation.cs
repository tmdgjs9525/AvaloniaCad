using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using KoDrawing.Core.Entities;
using KoDrawing.Core.HitTesting;
using KoDrawing.Core.Rendering;
using SkiaSharp;

namespace AvaloniaCad.Editor.Rendering;

public sealed class SkiaDrawOperation : ICustomDrawOperation
{
    private readonly CadScene _scene;
    public Rect Bounds { get; }

    public SkiaDrawOperation(Rect bounds, CadScene scene)
    {
        Bounds = bounds;
        _scene = scene;
    }

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null)
            return;

        using var lease = leaseFeature.Lease();
        _scene.Render(lease.SkCanvas, (float)Bounds.Width, (float)Bounds.Height);
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