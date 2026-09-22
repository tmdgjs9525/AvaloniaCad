using System.Numerics;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.Tools;

internal sealed class LineTool : ITool
{
    private readonly Action<Entity> _commit;   // 완성된 도형을 문서에 넣어주는 함수
    private Vector2? _start;
    private Vector2 _end;

    public LineTool(Action<Entity> commit)
    {
        _commit = commit;
    }

    public IReadOnlyList<Entity> Preview =>
        _start is { } start
            ? new Entity[] { new LineEntity { Start = start, End = _end } }
            : Array.Empty<Entity>();

    public void OnPointerPressed(Vector2 world)
    {
        if (_start is null)
        {
            _start = world;
            _end = world;
        }
        else
        {
            _commit(new LineEntity { Start = _start.Value, End = world });
            _start = null;
        }
    }

    public void OnPointerMoved(Vector2 world)
    {
        if (_start is not null)
            _end = world;
    }

    public void Cancel()
    {
        _start = null;
    }
}