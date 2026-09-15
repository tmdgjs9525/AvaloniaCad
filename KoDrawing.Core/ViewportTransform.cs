using System.Numerics;

namespace KoDrawing.Core;

public sealed class ViewportTransform
{
    public Vector2 CameraPosition { get; set; }

    public float Zoom { get; set; } = 1.0f;

    public Vector2 ViewportCenter { get; set; }

    public Vector2 WorldToScreen(Vector2 world)
    {
        return (world - CameraPosition) * Zoom
               + ViewportCenter;
    }

    public Vector2 ScreenToWorld(Vector2 screen)
    {
        return (screen - ViewportCenter) / Zoom
               + CameraPosition;
    }