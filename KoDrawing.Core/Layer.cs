using System;

namespace KoDrawing.Core;

/// <summary>도형을 묶는 레이어. 숨기면 그리지 않고, 잠그면 선택/편집할 수 없다.</summary>
public sealed class Layer
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; }

    public Layer(string name)
    {
        Name = name;
    }
}
