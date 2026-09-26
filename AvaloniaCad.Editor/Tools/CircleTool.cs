using System.Numerics;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.Tools;

public sealed class CircleTool : ITool
{
    private readonly Action<Entity> _commit;
    private Vector2? _center;
    private float _radius;

    public CircleTool(Action<Entity> commit)
    {
        _commit = commit;
    }

    public IReadOnlyList<Entity> Preview =>
        _center is { } center
            ? new Entity[] { new CircleEntity { Center = center, Radius = _radius } }
            : Array.Empty<Entity>();

    public void OnPointerPressed(Vector2 world)
    {
        if (_center is null)
        {
            _center = world;
            _radius = 0f;
        }
        else
        {
            _commit(new CircleEntity { Center = _center.Value, Radius = _radius });
            _center = null;
        }
    }

    public void OnPointerMoved(Vector2 world)
    {
        if (_center is { } center)
            _radius = Vector2.Distance(center, world);
    }

    public void OnPointerReleased(Vector2 world)
    {
        
    }

    public void Cancel() => _center = null;
}