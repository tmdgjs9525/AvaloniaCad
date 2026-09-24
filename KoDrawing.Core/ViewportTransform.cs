using System.Numerics;

namespace KoDrawing.Core;

public sealed class ViewportTransform
{
    /// <summary>화면 중앙에 오는 월드 좌표 (mm)</summary>
    public Vector2 CameraPosition { get; set; }

    public const float MinZoom = 0.01f;   // 1%
    public const float MaxZoom = 1000f;   // 100000%
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

    /// <summary>
    /// 화면에서 포인터가 screenDelta(픽셀)만큼 움직였을 때,
    /// 도면이 포인터에 붙어 따라오도록 카메라를 이동한다.
    /// </summary>
    public void PanByScreenDelta(Vector2 screenDelta)
    {
        // 화면 Y는 아래로, 월드 Y는 위로 증가하므로 Y 부호가 반대
        CameraPosition -= new Vector2(
            screenDelta.X / Zoom,
            -screenDelta.Y / Zoom);
    }

    /// <summary>
    /// screenPoint 아래의 월드 좌표를 고정한 채 factor배로 확대/축소한다.
    /// 결과 Zoom은 [MinZoom, MaxZoom]으로 제한된다.
    /// </summary>
    public void ZoomAt(Vector2 screenPoint, float factor)
    {
        if (!float.IsFinite(factor) || factor <= 0)
            throw new ArgumentOutOfRangeException(nameof(factor));

        var before = ScreenToWorld(screenPoint);

        Zoom = Math.Clamp(Zoom * factor, MinZoom, MaxZoom);

        // Zoom 변경 후 같은 화면 위치가 가리키는 월드 좌표가 달라졌으므로
        // 그 차이만큼 카메라를 보정해 원래 월드 점을 다시 그 위치로 되돌린다.
        var after = ScreenToWorld(screenPoint);
        CameraPosition += before - after;
    }
    
    /// <summary>렌더 스레드로 넘길 스냅샷 복사본</summary>
    public ViewportTransform Clone() => new()
    {
        CameraPosition = CameraPosition,
        Zoom = Zoom,
        ViewportCenter = ViewportCenter
    };
}