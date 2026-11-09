using System.Numerics;
using KoDrawing.Core.Entities;
using KoDrawing.Core.HitTesting;

namespace KoDrawing.Core.Tools;

public sealed class SelectTool : ITool
{
    public IReadOnlyList<Grip> CurrentGrips => _currentGrips;

    /// <summary>그립을 잡고 끄는 중인지 여부 (드래그 중에는 스냅 좌표를 받아야 함)</summary>
    public bool IsDraggingGrip => _draggingGrip is not null;

    /// <summary>마우스가 올라가 있는 도형 (hover 강조용)</summary>
    public Entity? HoveredEntity { get; private set; }

    /// <summary>마우스가 올라가 있거나 끌고 있는 그립의 위치</summary>
    public Vector2? HoveredGripPosition { get; private set; }

    private Grip? _draggingGrip;
    private IReadOnlyList<Grip> _currentGrips = Array.Empty<Grip>();
    private Entity? _selected;

    // 생성 시점의 문서를 고정하지 않고, 클릭할 때마다 현재 문서를 가져온다 (New/Open 대응)
    private readonly Func<CadDocument?> _getDocument;
    private readonly Action<Entity?> _onSelectionChanged;
    private readonly Func<float> _pixelToleranceInWorld;

    public SelectTool(Func<CadDocument?> getDocument, Action<Entity?> onSelectionChanged, Func<float> pixelToleranceInWorld)
    {
        _getDocument = getDocument;
        _onSelectionChanged = onSelectionChanged;
        _pixelToleranceInWorld = pixelToleranceInWorld;
    }

    public IReadOnlyList<Entity> Preview => Array.Empty<Entity>();

    public void OnPointerPressed(Vector2 world)
    {
        var tolerance = _pixelToleranceInWorld();

        // 1) 이미 선택된 도형이 있으면, 그립부터 먼저 검사 (몸통보다 우선)
        if (FindGrip(world, tolerance) is { } grip)
        {
            _draggingGrip = grip;
            return;   // 그립을 잡았으니 몸통 검사는 안 함
        }

        // 2) 그립에 안 맞았으면 몸통 검사 (빈 곳이면 null → 선택 해제)
        Select(FindEntity(world, tolerance));
    }

    public void OnPointerMoved(Vector2 world)
    {
        if (_draggingGrip is not null)
        {
            _draggingGrip.MoveTo(world);
            RebuildGrips();   // 도형이 바뀌었으니 그립 위치도 다시 계산

            HoveredEntity = null;
            HoveredGripPosition = _draggingGrip.Position;
            return;
        }

        // 드래그 중이 아니면 마우스 아래 그립/도형을 찾아 hover 상태로 기록
        var tolerance = _pixelToleranceInWorld();
        var grip = FindGrip(world, tolerance);

        HoveredGripPosition = grip?.Position;
        HoveredEntity = grip is null ? FindEntity(world, tolerance) : null;
    }

    /// <summary>포인터가 캔버스를 벗어났을 때 hover 표시를 지운다</summary>
    public void ClearHover()
    {
        HoveredEntity = null;
        HoveredGripPosition = null;
    }

    private Grip? FindGrip(Vector2 world, float tolerance)
    {
        foreach (var grip in _currentGrips)
        {
            if (Vector2.Distance(grip.Position, world) <= tolerance)
                return grip;
        }

        return null;
    }

    // 나중에 그린 도형(위에 있는 도형)부터 검사. 숨김/잠금 레이어의 도형은 건너뛴다.
    private Entity? FindEntity(Vector2 world, float tolerance)
    {
        var document = _getDocument();
        if (document is null)
            return null;

        for (var i = document.Entities.Count - 1; i >= 0; i--)
        {
            var entity = document.Entities[i];
            if (document.IsEntityEditable(entity) && EntityHitTest.HitTest(entity, world, tolerance))
                return entity;
        }

        return null;
    }

    public void OnPointerReleased(Vector2 world) => _draggingGrip = null;

    /// <summary>Esc: 드래그 중단 + 선택 해제</summary>
    public void Cancel()
    {
        _draggingGrip = null;
        Select(null);
    }

    /// <summary>
    /// 외부(ViewModel 등)에서 선택이 바뀌었을 때 내부 상태만 맞춘다.
    /// 콜백을 다시 부르지 않으므로 바인딩과 서로 무한 호출되지 않는다.
    /// </summary>
    public void SetSelection(Entity? entity)
    {
        if (ReferenceEquals(entity, _selected))
            return;

        _draggingGrip = null;
        _selected = entity;
        RebuildGrips();
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