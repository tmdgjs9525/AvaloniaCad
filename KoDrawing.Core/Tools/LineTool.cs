using System.Numerics;
using KoDrawing.Core.Entities;

namespace KoDrawing.Core.Tools;

public sealed class LineTool : ITool
{
    private const float MinLength = 1e-4f;      // 이보다 짧은 선은 만들지 않음 (mm)

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
            if (Vector2.Distance(_start.Value, world) < MinLength)
                return;   // 시작점과 같은 곳을 다시 찍으면 무시

            _commit(new LineEntity { Start = _start.Value, End = world });
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

    public void Cancel()
    {
        _start = null;
    }
}