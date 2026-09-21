using System.Numerics;

namespace KoDrawing.Core.Entities;

public sealed class CircleEntity : Entity
{
    public Vector2 Center { get; set; }
    public float Radius { get; set; }
}