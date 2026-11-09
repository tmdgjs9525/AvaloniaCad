using System.Numerics;
using KoDrawing.Core;
using KoDrawing.Core.Entities;
using Xunit;

namespace KoDrawing.Tests.Document;

public class CadDocumentTests
{
    private static LineEntity NewLine() => new() { Start = Vector2.Zero, End = new Vector2(10, 0) };

    // ───────── 도형 ─────────

    [Fact]
    public void NewDocument_HasSingleDefaultLayer_AsCurrent()
    {
        var document = new CadDocument();

        var layer = Assert.Single(document.Layers);
        Assert.Equal("Default", layer.Name);
        Assert.Same(layer, document.CurrentLayer);
        Assert.Empty(document.Entities);
    }

    [Fact]
    public void Add_RaisesChanged_AndAssignsCurrentLayer()
    {
        var document = new CadDocument();
        var changed = 0;
        document.Changed += () => changed++;

        var line = NewLine();
        document.Add(line);

        Assert.Equal(1, changed);
        Assert.Contains(line, document.Entities);
        Assert.Equal(document.CurrentLayer.Id, line.LayerId);
    }

    [Fact]
    public void Add_KeepsValidLayerId()
    {
        var document = new CadDocument();
        var other = document.AddLayer("Other");

        var line = NewLine();
        line.LayerId = other.Id;
        document.Add(line);

        Assert.Equal(other.Id, line.LayerId);
    }

    [Fact]
    public void Add_WithUnknownLayerId_FallsBackToCurrentLayer()
    {
        var document = new CadDocument();

        var line = NewLine();
        line.LayerId = Guid.NewGuid();   // 문서에 없는 레이어
        document.Add(line);

        Assert.Equal(document.CurrentLayer.Id, line.LayerId);
    }

    [Fact]
    public void Remove_NotContainedEntity_ReturnsFalse_WithoutChanged()
    {
        var document = new CadDocument();
        var changed = 0;
        document.Changed += () => changed++;

        Assert.False(document.Remove(NewLine()));
        Assert.Equal(0, changed);
    }

    [Fact]
    public void Remove_ContainedEntity_ReturnsTrue_AndRaisesChanged()
    {
        var document = new CadDocument();
        var line = NewLine();
        document.Add(line);

        var changed = 0;
        document.Changed += () => changed++;

        Assert.True(document.Remove(line));
        Assert.Empty(document.Entities);
        Assert.Equal(1, changed);
    }

    [Fact]
    public void NotifyChanged_RaisesChanged()
    {
        var document = new CadDocument();
        var changed = 0;
        document.Changed += () => changed++;

        document.NotifyChanged();

        Assert.Equal(1, changed);
    }

    // ───────── 레이어 ─────────

    [Fact]
    public void AddLayer_AddsLayer_AndRaisesChanged()
    {
        var document = new CadDocument();
        var changed = 0;
        document.Changed += () => changed++;

        var layer = document.AddLayer("Annotation");

        Assert.Equal(2, document.Layers.Count);
        Assert.Equal("Annotation", layer.Name);
        Assert.Same(layer, document.FindLayer(layer.Id));
        Assert.Equal(1, changed);
    }

    [Fact]
    public void CurrentLayer_ForeignLayer_Throws()
    {
        var document = new CadDocument();

        Assert.Throws<ArgumentException>(() => document.CurrentLayer = new Layer("외부"));
    }

    [Fact]
    public void NewEntity_GoesToCurrentLayer()
    {
        var document = new CadDocument();
        var annotation = document.AddLayer("Annotation");
        document.CurrentLayer = annotation;

        var line = NewLine();
        document.Add(line);

        Assert.Equal(annotation.Id, line.LayerId);
    }

    [Fact]
    public void RemoveLayer_LastLayer_IsRejected()
    {
        var document = new CadDocument();

        Assert.False(document.RemoveLayer(document.CurrentLayer));
        Assert.Single(document.Layers);
    }

    [Fact]
    public void RemoveLayer_MovesEntitiesToFirstLayer_AndResetsCurrent()
    {
        var document = new CadDocument();
        var defaultLayer = document.CurrentLayer;
        var temp = document.AddLayer("Temp");
        document.CurrentLayer = temp;

        var line = NewLine();
        document.Add(line);

        Assert.True(document.RemoveLayer(temp));

        Assert.Single(document.Layers);
        Assert.Same(defaultLayer, document.CurrentLayer);
        Assert.Equal(defaultLayer.Id, line.LayerId);
        Assert.Contains(line, document.Entities);   // 도형은 지워지지 않는다
    }

    [Fact]
    public void RemoveLayer_NotInDocument_ReturnsFalse()
    {
        var document = new CadDocument();
        document.AddLayer("Other");

        Assert.False(document.RemoveLayer(new Layer("외부")));
        Assert.Equal(2, document.Layers.Count);
    }

    [Theory]
    [InlineData(true, false, true, true)]    // 보임 + 잠기지 않음 → 그리고, 편집 가능
    [InlineData(true, true, true, false)]    // 보임 + 잠김 → 그리지만 편집 불가
    [InlineData(false, false, false, false)] // 숨김 → 안 그리고, 편집 불가
    public void LayerState_ControlsVisibilityAndEditability(
        bool isVisible, bool isLocked, bool expectedVisible, bool expectedEditable)
    {
        var document = new CadDocument();
        var line = NewLine();
        document.Add(line);

        document.CurrentLayer.IsVisible = isVisible;
        document.CurrentLayer.IsLocked = isLocked;

        Assert.Equal(expectedVisible, document.IsEntityVisible(line));
        Assert.Equal(expectedEditable, document.IsEntityEditable(line));
    }
}
