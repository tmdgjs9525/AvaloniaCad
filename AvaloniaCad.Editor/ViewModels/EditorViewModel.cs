using AvaloniaCad.Core;
using AvaloniaCad.Editor.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoDrawing.Core;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.ViewModels;

public partial class EditorViewModel : ViewModelBase
{
    [ObservableProperty] private bool _snapEnabled = true;
    [ObservableProperty] private ToolKind _currentToolKind = ToolKind.Select;
    [ObservableProperty] private CadDocument _cadDocument = new();

    [ObservableProperty] private Entity? _selectedEntity;
    
    
    
    public EditorViewModel()
    {
        
    }

    [RelayCommand]
    private void SelectTool(ToolKind kind)
    {
        CurrentToolKind = kind;
    }
}