using System.Numerics;
using KoDrawing.Core;
using KoDrawing.Core.Entities;
using KoDrawing.Core.Tools;
using Xunit;

namespace KoDrawing.Tests.Tools;

public class SelectToolTests
{
    private const float Tolerance = 1f;

    private readonly CadDocument _document = new();
    private readonly List<Entity?> _selectionEvents = new();   // 선택 변경 콜백 기록

    private SelectTool CreateTool(CadDocument? document = null) =>
        new(() => document ?? _document, _selectionEvents.Add, () => Tolerance);

    private LineEntity AddLine(Vector2 start, Vector2 end)
    {
        var line = new LineEntity { Start = start, End = end };
        _document.Add(line);
        return line;
    }

    // ───────── 선택 ─────────

    [Fact]
    public void ClickOnEntity_SelectsIt_AndCreatesGrips()
    {
        var line = AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var tool = CreateTool();

        tool.OnPointerPressed(new Vector2(5, 0.5f));

        Assert.Same(line, Assert.Single(_selectionEvents));
        Assert.Equal(2, tool.CurrentGrips.Count);
    }

    [Fact]
    public void ClickOnEmptySpace_ClearsSelection()
    {
        AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var tool = CreateTool();

        tool.OnPointerPressed(new Vector2(5, 0));
        tool.OnPointerPressed(new Vector2(50, 50));

        Assert.Null(_selectionEvents[^1]);
        Assert.Empty(tool.CurrentGrips);
    }

    [Fact]
    public void OverlappingEntities_TopmostIsSelected()
    {
        AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var top = AddLine(new Vector2(0, 0), new Vector2(10, 0));   // 나중에 그린 도형이 위
        var tool = CreateTool();

        tool.OnPointerPressed(new Vector2(5, 0));

        Assert.Same(top, _selectionEvents[^1]);
    }

    [Fact]
    public void LockedLayerEntity_IsNotSelectable()
    {
        AddLine(new Vector2(0, 0), new Vector2(10, 0));
        _document.CurrentLayer.IsLocked = true;
        var tool = CreateTool();

        tool.OnPointerPressed(new Vector2(5, 0));

        Assert.Null(_selectionEvents[^1]);
    }

    [Fact]
    public void HiddenLayerEntity_IsNotSelectable()
    {
        AddLine(new Vector2(0, 0), new Vector2(10, 0));
        _document.CurrentLayer.IsVisible = false;
        var tool = CreateTool();

        tool.OnPointerPressed(new Vector2(5, 0));

        Assert.Null(_selectionEvents[^1]);
    }

    [Fact]
    public void UsesCurrentDocument_NotDocumentAtCreation()
    {
        // 도구를 만든 뒤에 문서가 바뀌어도 새 문서를 기준으로 선택해야 한다
        var current = new CadDocument();
        var tool = new SelectTool(() => current, _selectionEvents.Add, () => Tolerance);

        var replaced = new CadDocument();
        var line = new LineEntity { Start = Vector2.Zero, End = new Vector2(10, 0) };
        replaced.Add(line);
        current = replaced;

        tool.OnPointerPressed(new Vector2(5, 0));

        Assert.Same(line, _selectionEvents[^1]);
    }

    [Fact]
    public void NullDocument_ClearsSelection_WithoutThrowing()
    {
        var tool = new SelectTool(() => null, _selectionEvents.Add, () => Tolerance);

        tool.OnPointerPressed(new Vector2(5, 0));

        Assert.Null(Assert.Single(_selectionEvents));
    }

    [Fact]
    public void Cancel_ClearsSelection()
    {
        AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var tool = CreateTool();
        tool.OnPointerPressed(new Vector2(5, 0));

        tool.Cancel();

        Assert.Null(_selectionEvents[^1]);
        Assert.Empty(tool.CurrentGrips);
    }

    [Fact]
    public void SetSelection_UpdatesGrips_WithoutCallback()
    {
        var line = AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var tool = CreateTool();

        tool.SetSelection(line);

        Assert.Equal(2, tool.CurrentGrips.Count);
        Assert.Empty(_selectionEvents);   // 바인딩과 무한 호출되지 않도록 콜백을 부르지 않는다

        tool.SetSelection(null);

        Assert.Empty(tool.CurrentGrips);
        Assert.Empty(_selectionEvents);
    }

    // ───────── 그립 드래그 ─────────

    [Fact]
    public void DraggingGrip_MovesEntityPoint()
    {
        var line = AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var tool = CreateTool();
        tool.SetSelection(line);

        tool.OnPointerPressed(new Vector2(10.3f, 0));   // End 그립 근처
        Assert.True(tool.IsDraggingGrip);

        tool.OnPointerMoved(new Vector2(20, 5));

        Assert.Equal(new Vector2(20, 5), line.End);
        Assert.Equal(new Vector2(0, 0), line.Start);
        Assert.Contains(tool.CurrentGrips, g => g.Position == new Vector2(20, 5));   // 그립 위치도 갱신
        Assert.Equal(new Vector2(20, 5), tool.HoveredGripPosition);

        tool.OnPointerReleased(new Vector2(20, 5));

        Assert.False(tool.IsDraggingGrip);
    }

    [Fact]
    public void GripHasPriorityOverEntityBody()
    {
        // 그립 위치에 다른 도형이 겹쳐 있어도 그립을 먼저 잡는다
        var selected = AddLine(new Vector2(0, 0), new Vector2(10, 0));
        AddLine(new Vector2(10, -5), new Vector2(10, 5));   // 위에 그려진 세로선
        var tool = CreateTool();
        tool.SetSelection(selected);

        tool.OnPointerPressed(new Vector2(10, 0));

        Assert.True(tool.IsDraggingGrip);
        Assert.Empty(_selectionEvents);   // 선택은 바뀌지 않음
    }

    [Fact]
    public void Cancel_StopsDragging()
    {
        var line = AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var tool = CreateTool();
        tool.SetSelection(line);
        tool.OnPointerPressed(new Vector2(10, 0));

        tool.Cancel();

        Assert.False(tool.IsDraggingGrip);
    }

    // ───────── Hover ─────────

    [Fact]
    public void Hover_OverEntity_SetsHoveredEntity()
    {
        var line = AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var tool = CreateTool();

        tool.OnPointerMoved(new Vector2(5, 0.5f));

        Assert.Same(line, tool.HoveredEntity);
        Assert.Null(tool.HoveredGripPosition);

        tool.OnPointerMoved(new Vector2(50, 50));

        Assert.Null(tool.HoveredEntity);
    }

    [Fact]
    public void Hover_OverGrip_SetsGripPosition_InsteadOfEntity()
    {
        var line = AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var tool = CreateTool();
        tool.SetSelection(line);

        tool.OnPointerMoved(new Vector2(0.2f, 0));

        Assert.Equal(new Vector2(0, 0), tool.HoveredGripPosition);
        Assert.Null(tool.HoveredEntity);
    }

    [Fact]
    public void ClearHover_ResetsHoverState()
    {
        AddLine(new Vector2(0, 0), new Vector2(10, 0));
        var tool = CreateTool();
        tool.OnPointerMoved(new Vector2(5, 0));

        tool.ClearHover();

        Assert.Null(tool.HoveredEntity);
        Assert.Null(tool.HoveredGripPosition);
    }
}
