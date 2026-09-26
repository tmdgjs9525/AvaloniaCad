using System.Numerics;
using AvaloniaCad.Editor.HitTesting;
using KoDrawing.Core;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.Tools;

public sealed class SelectTool : ITool
{
    public IReadOnlyList<Grip> CurrentGrips => _currentGrips;

    private Grip? _draggingGrip;
    private IReadOnlyList<Grip> _currentGrips = Array.Empty<Grip>();
    private Entity? _selected;

    private readonly CadDocument _document;
    private readonly Action<Entity?> _onSelectionChanged;
    private readonly Func<float> _pixelToleranceInWorld;

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

        // 1) 이미 선택된 도형이 있으면, 그립부터 먼저 검사 (몸통보다 우선)
        foreach (var grip in _currentGrips)
        {
            if (Vector2.Distance(grip.Position, world) <= tolerance)
            {
                _draggingGrip = grip;
                return;   // 그립을 잡았으니 몸통 검사는 안 함
            }
        }

        // 2) 그립에 안 맞았으면 기존처럼 몸통 검사
        for (var i = _document.Entities.Count - 1; i >= 0; i--)
        {
            var entity = _document.Entities[i];
            if (EntityHitTest.HitTest(entity, world, tolerance))
            {
                Select(entity);
                return;
            }
        }

        Select(null);   // 빈 곳 클릭 → 선택 해제
    }

    public void OnPointerMoved(Vector2 world)
    {
        if (_draggingGrip is not null)
        {
            _draggingGrip.MoveTo(world);
            RebuildGrips();   // 도형이 바뀌었으니 그립 위치도 다시 계산
        }
    }

    public void OnPointerReleased(Vector2 world) => _draggingGrip = null;

    public void Cancel()
    {
        _draggingGrip = null;
    }

    private void Select(Entity? entity)
    {
        _selected = entity;
        _onSelectionChanged(entity);
        RebuildGrips();
    }

    private void RebuildGrips()
    {
        _currentGrips = _selected is not null
            ? EntityGrips.GetGrips(_selected)
            : Array.Empty<Grip>();
    }
}