using Avalonia;
using Avalonia.Container.ServiceHelper;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaCad.Editor.ViewModels;
using AvaloniaCad.Editor.Views;
using AvaloniaCad.ViewModels;
using AvaloniaCad.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Container.Extensions;

namespace AvaloniaCad;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        var collection = new ServiceCollection();
        collection.AddAvaloniaContainerService().ConfigureViews();

        var services = collection.BuildServiceProvider();
        Ioc.Default.ConfigureServices(services);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var v = Ioc.Default.GetRequiredService<MainWindow>();
        var vm = Ioc.Default.GetRequiredService<MainViewModel>();

        v.DataContext = vm;
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = v;
        }

        base.OnFrameworkInitializationCompleted();
    }
    
}

internal static class Configure
{
    public static IServiceCollection ConfigureViews(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();

        services.AddSingletonNavigation<EditorView, EditorViewModel>();
        return services;  
    }
}