using System.Numerics;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.Tools;

public sealed class RectangleTool : ITool
{
    // TODO : RectnagleEntity 구현 후 LineEntity 제거 필요
    private readonly Action<Entity> _commit;
    private Vector2? _start;
    private Vector2 _end;

    public RectangleTool(Action<Entity> commit)
    {
        _commit = commit;
    }

    public IReadOnlyList<Entity> Preview =>
        _start is { } start
            ? new Entity[]
            {
                new LineEntity { Start = new Vector2(start.X, start.Y), End = new Vector2(_end.X, start.Y) },
                new LineEntity { Start = new Vector2(_end.X, start.Y), End = new Vector2(_end.X, _end.Y) },
                new LineEntity { Start = new Vector2(_end.X, _end.Y), End = new Vector2(start.X, _end.Y) },
                new LineEntity { Start = new Vector2(start.X, _end.Y), End = new Vector2(start.X, start.Y) },
            }
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
            var start = _start.Value;
            var end = world;

            _commit(new LineEntity { Start = new(start.X, start.Y), End = new(end.X, start.Y) });
            _commit(new LineEntity { Start = new(end.X, start.Y), End = new(end.X, end.Y) });
            _commit(new LineEntity { Start = new(end.X, end.Y), End = new(start.X, end.Y) });
            _commit(new LineEntity { Start = new(start.X, end.Y), End = new(start.X, start.Y) });

            _start = null;
        }
    }

    public void OnPointerMoved(Vector2 world)
    {
        if (_start is not null)
            _end = world;
    }

    public void Cancel() => _start = null;
}