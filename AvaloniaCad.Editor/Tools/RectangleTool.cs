using System.Numerics;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.Tools;

public sealed class RectangleTool : ITool
{
    private readonly Action<Entity> _commit;
    private Vector2? _start;
    private Vector2 _end;

    public RectangleTool(Action<Entity> commit)
    {
        _commit = commit;
    }

    public IReadOnlyList<Entity> Preview =>
        _start is { } start
            ? new Entity[] { new RectangleEntity { Corner1 = start, Corner2 = _end } }
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
            _commit(new RectangleEntity { Corner1 = _start.Value, Corner2 = world });
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