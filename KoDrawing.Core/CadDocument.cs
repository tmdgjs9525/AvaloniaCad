using KoDrawing.Core.Entities;

namespace KoDrawing.Core;

public sealed class CadDocument
{
    private readonly List<Entity> _entities = new();

    public IReadOnlyList<Entity> Entities => _entities;

    public void Add(Entity entity) => _entities.Add(entity);
    public bool Remove(Entity entity) => _entities.Remove(entity);
}