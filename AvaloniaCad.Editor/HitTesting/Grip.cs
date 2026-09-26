using System.Numerics;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.HitTesting;

public sealed class Grip
{
    public Vector2 Position { get; private set; }
    private readonly Action<Vector2> _moveTo;

    public Grip(Vector2 position, Action<Vector2> moveTo)
    {
        Position = position;
        _moveTo = moveTo;
    }

    public void MoveTo(Vector2 newPosition)
    {
        Position = newPosition;
        _moveTo(newPosition);
    }
}