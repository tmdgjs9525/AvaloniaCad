using AvaloniaCad.Core;
using AvaloniaCad.Editor.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Container.Core.Interfaces;

namespace AvaloniaCad.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        
        _navigationService.NavigateTo(RegionNames.MainRegion, nameof(EditorView));
    }
}