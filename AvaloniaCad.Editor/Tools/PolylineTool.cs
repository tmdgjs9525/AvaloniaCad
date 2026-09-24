using System.Numerics;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.Tools;

public sealed class PolylineTool : ITool
{
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

            var segments = new List<Entity>();
            for (var i = 0; i < _points.Count - 1; i++)
                segments.Add(new LineEntity { Start = _points[i], End = _points[i + 1] });

            // 마지막 점에서 현재 마우스 위치까지 미리보기
            segments.Add(new LineEntity { Start = _points[^1], End = _cursor });
            return segments;
        }
    }

    public void OnPointerPressed(Vector2 world)
    {
        _points.Add(world);
        _cursor = world;
    }

    public void OnPointerMoved(Vector2 world) => _cursor = world;

    /// <summary>우클릭으로 폴리라인을 마무리할 때 호출 (DrawingCanvas에서 연결 필요)</summary>
    public void Finish()
    {
        for (var i = 0; i < _points.Count - 1; i++)
            _commit(new LineEntity { Start = _points[i], End = _points[i + 1] });
        
        _points.Clear();
    }

    public void Cancel() => _points.Clear();
}