using AvaloniaCad.Core;
using AvaloniaCad.Editor.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoDrawing.Core;

namespace AvaloniaCad.Editor.ViewModels;

public partial class EditorViewModel : ViewModelBase
{
    [ObservableProperty] private ITool _currentTool;
    [ObservableProperty] private CadDocument _cadDocument = new();

    public EditorViewModel()
    {
        
    }

    [RelayCommand]
    private void SelectTool(string toolName)
    {
        CurrentTool = toolName switch
        {
            "Line" => new LineTool(entity => CadDocument.Add(entity)),
            _ => new NullTool(),
        };
    }
}