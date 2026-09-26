using System.Numerics;

namespace KoDrawing.Core.Entities;

public sealed class PolylineEntity : Entity
{
    public List<Vector2> Points { get; set; } = new();
}