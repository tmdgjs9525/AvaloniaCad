using System.Numerics;
using AvaloniaCad.Editor.HitTesting;
using KoDrawing.Core;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.Tools;

public sealed class SelectTool : ITool
{
    private readonly CadDocument _document;
    private readonly Action<Entity?> _onSelectionChanged;
    private readonly Func<float> _pixelToleranceInWorld;   // 화면 5px를 현재 줌 기준 mm로 환산

    public SelectTool(CadDocument document, Action<Entity?> onSelectionChanged, Func<float> pixelToleranceInWorld)
    {
        _document = document;
        _onSelectionChanged = onSelectionChanged;
        _pixelToleranceInWorld = pixelToleranceInWorld;
    }

    public IReadOnlyList<Entity> Preview => Array.Empty<Entity>();

    public void OnPointerPressed(Vector2 world)
    {
        var tolerance = _pixelToleranceInWorld();

        // 뒤에 그려진 것부터가 아니라, 나중에 그려진(위에 있는) 도형을 먼저 찾도록 역순 탐색
        for (var i = _document.Entities.Count - 1; i >= 0; i--)
        {
            var entity = _document.Entities[i];
            if (EntityHitTest.HitTest(entity, world, tolerance))
            {
                _onSelectionChanged(entity);
                return;
            }
        }

        _onSelectionChanged(null);   // 빈 곳 클릭 → 선택 해제
    }

    public void OnPointerMoved(Vector2 world) { }
    public void Cancel() { }
}