using CommunityToolkit.Mvvm.ComponentModel;
using KoDrawing.Core;

namespace AvaloniaCad.Editor.ViewModels;

/// <summary>
/// 레이어 패널의 한 줄. Core의 Layer는 변경 알림이 없으므로 이 클래스가 감싸서
/// UI 변경을 Layer에 반영하고, 문서에 변경을 알려 캔버스를 다시 그리게 한다.
/// </summary>
public partial class LayerItemViewModel : ObservableObject
{
    private readonly CadDocument _document;

    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private bool _isLocked;

    public Layer Layer { get; }

    public LayerItemViewModel(Layer layer, CadDocument document)
    {
        Layer = layer;
        _document = document;

        // 생성 시에는 OnXxxChanged가 돌지 않도록 필드에 직접 넣는다
        _name = layer.Name;
        _isVisible = layer.IsVisible;
        _isLocked = layer.IsLocked;
    }

    partial void OnNameChanged(string value)
    {
        Layer.Name = value;
        _document.NotifyChanged();
    }

    partial void OnIsVisibleChanged(bool value)
    {
        Layer.IsVisible = value;
        _document.NotifyChanged();
    }

    partial void OnIsLockedChanged(bool value)
    {
        Layer.IsLocked = value;
        _document.NotifyChanged();
    }
}
