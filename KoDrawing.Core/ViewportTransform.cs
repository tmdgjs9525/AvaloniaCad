using System.Numerics;

namespace KoDrawing.Core;

public sealed class ViewportTransform
{
    public Vector2 CameraPosition { get; set; }

    private float _zoom = 1.0f;

    public float Zoom
    {
        get => _zoom;
        set
        {
            if (!float.IsFinite(value) || value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            _zoom = value;
        }
    }

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
}