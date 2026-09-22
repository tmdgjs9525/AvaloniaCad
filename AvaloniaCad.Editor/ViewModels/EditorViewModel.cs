using AvaloniaCad.Core;
using AvaloniaCad.Editor.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoDrawing.Core;

namespace AvaloniaCad.Editor.ViewModels;

public partial class EditorViewModel : ViewModelBase
{
    [ObservableProperty] private bool _snapEnabled = true;
    [ObservableProperty] private ITool _currentTool;
    [ObservableProperty] private CadDocument _cadDocument = new();

    public EditorViewModel()
    {
        
    }

    [RelayCommand]
    private void SelectTool(ToolKind kind)
    {
        CurrentTool = kind switch
        {
            ToolKind.Line => new LineTool(CadDocument.Add),
            ToolKind.Rectangle => new RectangleTool(CadDocument.Add),
            ToolKind.Circle => new CircleTool(CadDocument.Add),
            ToolKind.Polyline => new PolylineTool(CadDocument.Add),
            _ => new NullTool(),   // Select, Pan은 아직 미구현
        };
    }
}