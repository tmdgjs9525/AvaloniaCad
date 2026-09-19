using System.Numerics;

namespace KoDrawing.Core;

public sealed class ViewportTransform
{
    /// <summary>화면 중앙에 오는 월드 좌표 (mm)</summary>
    public Vector2 CameraPosition { get; set; }

    private float _zoom = 1.0f;

    /// <summary>1mm당 픽셀 수. 유한한 양수여야 함</summary>
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

    /// <summary>컨트롤 로컬 좌표 기준 뷰포트 중심 (보통 W/2, H/2)</summary>
    public Vector2 ViewportCenter { get; set; }

    // CAD 관례: 월드 Y는 위로 증가, 스크린 Y는 아래로 증가하므로 Y만 뒤집는다.
    public Vector2 WorldToScreen(Vector2 world)
    {
        var d = world - CameraPosition;

        return new Vector2(
            d.X * Zoom + ViewportCenter.X,
            -d.Y * Zoom + ViewportCenter.Y);
    }

    public Vector2 ScreenToWorld(Vector2 screen)
    {
        var d = screen - ViewportCenter;

        return new Vector2(
            d.X / Zoom + CameraPosition.X,
            -d.Y / Zoom + CameraPosition.Y);
    }

    /// <summary>렌더 스레드로 넘길 스냅샷 복사본</summary>
    public ViewportTransform Clone() => new()
    {
        CameraPosition = CameraPosition,
        Zoom = Zoom,
        ViewportCenter = ViewportCenter
    };
}