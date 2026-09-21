using System.Numerics;

namespace KoDrawing.Core.Entities;

public sealed class LineEntity : Entity
{
    public Vector2 Start { get; set; }
    public Vector2 End { get; set; }
}