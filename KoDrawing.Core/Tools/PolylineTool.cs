using System.Numerics;
using KoDrawing.Core.Entities;

namespace KoDrawing.Core.Tools;

public sealed class PolylineTool : ITool
{
    private const float MinSegment = 1e-4f;   // 직전 점과 이보다 가까우면 점을 추가하지 않음 (mm)

    private readonly Action<Entity> _commit;
    private readonly List<Vector2> _points = new();
    private Vector2 _cursor;

    public PolylineTool(Action<Entity> commit)
    {
        _commit = commit;
    }

    public IReadOnlyList<Entity> Preview
    {
        get
        {
            if (_points.Count == 0)
                return Array.Empty<Entity>();

            // 확정된 점들 + 현재 마우스 위치를 한 개의 폴리라인으로 미리보기
            // (선분을 따로 그리면 점선 패턴이 마디마다 끊기므로)
            var points = new List<Vector2>(_points) { _cursor };
            return new Entity[] { new PolylineEntity { Points = points } };
        }
    }

    public void OnPointerPressed(Vector2 world)
    {
        if (_points.Count > 0 && Vector2.Distance(_points[^1], world) < MinSegment)
            return;   // 같은 점 연속 클릭 무시

        _points.Add(world);
        _cursor = world;
    }

    public void OnPointerMoved(Vector2 world) => _cursor = world;
    public void OnPointerReleased(Vector2 world)
    {
        
    }

    public void Finish()
    {
        if (_points.Count >= 2)
            _commit(new PolylineEntity { Points = new List<Vector2>(_points) });

        _points.Clear();
    }

    public void Cancel() => _points.Clear();
}