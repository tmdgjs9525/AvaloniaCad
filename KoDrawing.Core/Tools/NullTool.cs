using System.Numerics;
using KoDrawing.Core.Entities;

namespace KoDrawing.Core.Tools;

public sealed class NullTool : ITool
{
    public IReadOnlyList<Entity> Preview => Array.Empty<Entity>();

    public void OnPointerPressed(Vector2 world) { }
    public void OnPointerMoved(Vector2 world) { }
    public void OnPointerReleased(Vector2 world)
    {
        
    }

    public void Cancel() { }
}