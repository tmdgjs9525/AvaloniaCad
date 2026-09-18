using Avalonia.Container.Navigate;
using Container.Core.Interfaces;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Wpf.Container.Extensions
{
    public static class NavigationServiceExtensions
    {
        internal static INavigationRegister _navigationRegister = null!;

        // View와 ViewModel을 Transient로 등록하는 확장 메서드
        public static IServiceCollection AddTransientNavigation<TView, TViewModel>(this IServiceCollection services)
            where TView : Control
            where TViewModel : class, IViewModelBase
        {
            services.AddTransient<TView>();
            services.AddTransient<TViewModel>();

            _navigationRegister.AddTransientNavigation<TView, TViewModel>();

            return services;
        }

        // View와 ViewModel을 Singleton으로 등록하는 확장 메서드
        public static IServiceCollection AddSingletonNavigation<TView, TViewModel>(this IServiceCollection services)
            where TView : Control
            where TViewModel : class, IViewModelBase
        {
            services.AddSingleton<TView>();
            services.AddSingleton<TViewModel>();

            _navigationRegister.AddSingletonNavigation<TView, TViewModel>();

            return services;
        }

        // 인터페이스를 통해 View와 ViewModel을 Singleton으로 등록하는 확장 메서드
        public static IServiceCollection AddSingletonNavigation<TInterface, TImplementationView, TViewModel>(this IServiceCollection services)
            where TInterface : class
            where TImplementationView : Control, TInterface
            where TViewModel : class, IViewModelBase
        {
            services.AddSingleton<TInterface, TImplementationView>();
            services.AddSingleton<TViewModel>();

            _navigationRegister.AddSingletonNavigation<TInterface, TImplementationView, TViewModel>();

            return services;
        }

        // 인터페이스를 통해 View와 ViewModel을 Transient로 등록하는 확장 메서드
        public static IServiceCollection AddTransientNavigation<TInterface, TImplementationView, TViewModel>(this IServiceCollection services)
            where TInterface : class
            where TImplementationView : Control, TInterface
            where TViewModel : class, IViewModelBase
        {
            services.AddTransient<TInterface, TImplementationView>();
            services.AddTransient<TViewModel>();

            _navigationRegister.AddTransientNavigation<TInterface, TImplementationView, TViewModel>();

            return services;
        }

        public static IServiceCollection AddTransientNavigation<TView>(this IServiceCollection services, IViewModelBase viewModelInstance)
            where TView : Control
        {
            services.AddTransient<TView>();

            _navigationRegister.AddTransientNavigation<TView>(viewModelInstance);

            return services;
        }

        public static IServiceCollection AddSingletonNavigation<TView>(this IServiceCollection services, IViewModelBase viewModelInstance)
            where TView : Control
        {
            services.AddSingleton<TView>();

            _navigationRegister.AddSingletonNavigation<TView>(viewModelInstance);

            return services;
        }
    }
}
