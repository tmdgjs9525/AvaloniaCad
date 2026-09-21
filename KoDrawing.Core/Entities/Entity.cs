namespace KoDrawing.Core.Entities;

public abstract class Entity
{
    public Guid Id { get; } = Guid.NewGuid();
}