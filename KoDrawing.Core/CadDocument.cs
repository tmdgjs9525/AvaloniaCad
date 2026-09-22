using KoDrawing.Core.Entities;

namespace KoDrawing.Core;

public sealed class CadDocument
{
    private readonly List<Entity> _entities = new();

    public IReadOnlyList<Entity> Entities => _entities;

    /// <summary>도형이 추가/삭제될 때 발생. 화면 갱신 신호로 사용.</summary>
    public event Action? Changed;

    public void Add(Entity entity)
    {
        _entities.Add(entity);
        Changed?.Invoke();
    }

    public bool Remove(Entity entity)
    {
        var removed = _entities.Remove(entity);
        if (removed)
            Changed?.Invoke();
        return removed;
    }
}