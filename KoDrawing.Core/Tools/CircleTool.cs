using System.Numerics;
using KoDrawing.Core.Entities;

namespace KoDrawing.Core.Tools;

public sealed class CircleTool : ITool
{
    private const float MinRadius = 1e-4f;   // 이보다 작은 원은 만들지 않음 (mm)

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
            // 마지막 Move 값이 아니라 실제 클릭한 위치로 반지름을 계산 (LineTool과 동일한 방식)
            var radius = Vector2.Distance(_center.Value, world);
            if (radius < MinRadius)
                return;   // 중심을 다시 찍으면 무시

            _commit(new CircleEntity { Center = _center.Value, Radius = radius });
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