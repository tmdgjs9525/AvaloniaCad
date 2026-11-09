using System.Numerics;
using KoDrawing.Core.Entities;

namespace KoDrawing.Core.Tools;

public sealed class RectangleTool : ITool
{
    private const float MinSize = 1e-4f;   // 폭/높이가 이보다 작으면 만들지 않음 (mm)

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
            var start = _start.Value;
            if (MathF.Abs(world.X - start.X) < MinSize || MathF.Abs(world.Y - start.Y) < MinSize)
                return;   // 선처럼 납작한 사각형은 무시

            _commit(new RectangleEntity { Corner1 = _start.Value, Corner2 = world });
            _start = null;
        }
    }

    public void OnPointerMoved(Vector2 world)
    {
        if (_start is not null)
            _end = world;
    }

    public void OnPointerReleased(Vector2 world)
    {
        
    }

    public void Cancel() => _start = null;
}