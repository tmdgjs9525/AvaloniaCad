using System.Numerics;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.Tools;

public interface ITool
{
    /// <summary>그리는 중인 도형 (화면에 미리보기로 표시)</summary>
    IReadOnlyList<Entity> Preview { get; }

    void OnPointerPressed(Vector2 world);
    void OnPointerMoved(Vector2 world);
    void OnPointerReleased(Vector2 world);
    void Cancel();
}