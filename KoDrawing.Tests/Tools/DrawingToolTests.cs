using System.Numerics;
using KoDrawing.Core.Entities;
using KoDrawing.Core.Tools;
using Xunit;

namespace KoDrawing.Tests.Tools;

/// <summary>Line / Rectangle / Circle / Polyline 그리기 도구</summary>
public class DrawingToolTests
{
    // 도구가 문서에 넣으려는 도형을 모아 두는 가짜 commit
    private readonly List<Entity> _committed = new();

    // ───────── LineTool ─────────

    [Fact]
    public void LineTool_TwoClicks_CommitsLine()
    {
        var tool = new LineTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerPressed(new Vector2(10, 5));

        var line = Assert.IsType<LineEntity>(Assert.Single(_committed));
        Assert.Equal(new Vector2(0, 0), line.Start);
        Assert.Equal(new Vector2(10, 5), line.End);
        Assert.Empty(tool.Preview);
    }

    [Fact]
    public void LineTool_Preview_FollowsPointer()
    {
        var tool = new LineTool(_committed.Add);

        Assert.Empty(tool.Preview);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerMoved(new Vector2(3, 4));

        var preview = Assert.IsType<LineEntity>(Assert.Single(tool.Preview));
        Assert.Equal(new Vector2(3, 4), preview.End);
        Assert.Empty(_committed);
    }

    [Fact]
    public void LineTool_ClickingStartPointAgain_IsIgnored()
    {
        var tool = new LineTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(2, 2));
        tool.OnPointerPressed(new Vector2(2, 2));

        Assert.Empty(_committed);
        Assert.Single(tool.Preview);   // 아직 그리는 중
    }

    [Fact]
    public void LineTool_Cancel_DiscardsInProgressLine()
    {
        var tool = new LineTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.Cancel();

        Assert.Empty(tool.Preview);

        // 취소 후 클릭은 새 선의 시작점이 된다
        tool.OnPointerPressed(new Vector2(5, 5));
        Assert.Empty(_committed);
    }

    // ───────── RectangleTool ─────────

    [Fact]
    public void RectangleTool_TwoClicks_CommitsRectangle()
    {
        var tool = new RectangleTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerPressed(new Vector2(10, 5));

        var rect = Assert.IsType<RectangleEntity>(Assert.Single(_committed));
        Assert.Equal(new Vector2(0, 0), rect.Corner1);
        Assert.Equal(new Vector2(10, 5), rect.Corner2);
    }

    [Theory]
    [InlineData(10f, 0f)]   // 높이 0
    [InlineData(0f, 10f)]   // 폭 0
    [InlineData(0f, 0f)]    // 같은 점
    public void RectangleTool_FlatRectangle_IsIgnored(float x, float y)
    {
        var tool = new RectangleTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerPressed(new Vector2(x, y));

        Assert.Empty(_committed);
    }

    // ───────── CircleTool ─────────

    [Fact]
    public void CircleTool_RadiusComesFromClickPosition_NotLastMove()
    {
        var tool = new CircleTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerMoved(new Vector2(3, 0));    // 미리보기 반지름 3
        tool.OnPointerPressed(new Vector2(0, 5));  // 실제 클릭은 반지름 5

        var circle = Assert.IsType<CircleEntity>(Assert.Single(_committed));
        Assert.Equal(Vector2.Zero, circle.Center);
        Assert.Equal(5f, circle.Radius, 4);
    }

    [Fact]
    public void CircleTool_Preview_ShowsRadiusToPointer()
    {
        var tool = new CircleTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(1, 1));
        tool.OnPointerMoved(new Vector2(4, 5));

        var preview = Assert.IsType<CircleEntity>(Assert.Single(tool.Preview));
        Assert.Equal(5f, preview.Radius, 4);
    }

    [Fact]
    public void CircleTool_ClickingCenterAgain_IsIgnored()
    {
        var tool = new CircleTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(1, 1));
        tool.OnPointerPressed(new Vector2(1, 1));

        Assert.Empty(_committed);
    }

    // ───────── PolylineTool ─────────

    [Fact]
    public void PolylineTool_Finish_CommitsAllPoints()
    {
        var tool = new PolylineTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerPressed(new Vector2(10, 0));
        tool.OnPointerPressed(new Vector2(10, 10));
        tool.Finish();

        var polyline = Assert.IsType<PolylineEntity>(Assert.Single(_committed));
        Assert.Equal(
            new[] { new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 10) },
            polyline.Points);
        Assert.Empty(tool.Preview);
    }

    [Fact]
    public void PolylineTool_DuplicateClick_IsIgnored()
    {
        var tool = new PolylineTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerPressed(new Vector2(5, 0));
        tool.Finish();

        var polyline = Assert.IsType<PolylineEntity>(Assert.Single(_committed));
        Assert.Equal(2, polyline.Points.Count);
    }

    [Fact]
    public void PolylineTool_FinishWithOnePoint_CommitsNothing()
    {
        var tool = new PolylineTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.Finish();

        Assert.Empty(_committed);
        Assert.Empty(tool.Preview);
    }

    [Fact]
    public void PolylineTool_Preview_IsSinglePolylineEndingAtCursor()
    {
        var tool = new PolylineTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerPressed(new Vector2(10, 0));
        tool.OnPointerMoved(new Vector2(10, 7));

        var preview = Assert.IsType<PolylineEntity>(Assert.Single(tool.Preview));
        Assert.Equal(
            new[] { new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 7) },
            preview.Points);
    }

    [Fact]
    public void PolylineTool_Cancel_DiscardsPoints()
    {
        var tool = new PolylineTool(_committed.Add);

        tool.OnPointerPressed(new Vector2(0, 0));
        tool.OnPointerPressed(new Vector2(10, 0));
        tool.Cancel();
        tool.Finish();

        Assert.Empty(_committed);
        Assert.Empty(tool.Preview);
    }
}
