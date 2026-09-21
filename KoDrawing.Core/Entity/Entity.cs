namespace KoDrawing.Core.Entity;

public abstract class Entity
{
    public Guid Id { get; } = Guid.NewGuid();
}