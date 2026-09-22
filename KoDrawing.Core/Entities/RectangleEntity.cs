using System.Numerics;

namespace KoDrawing.Core.Entities;

public sealed class RectangleEntity : Entity
{
    /// <summary>드래그 시작점 (마우스로 찍은 첫 번째 코너)</summary>
    public Vector2 Corner1 { get; set; }

    /// <summary>드래그 끝점 (반대쪽 코너)</summary>
    public Vector2 Corner2 { get; set; }
}