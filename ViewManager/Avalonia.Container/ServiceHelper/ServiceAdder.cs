using Avalonia.Container.Dialog;
using Avalonia.Container.Navigate;
using Avalonia.Container.Region;
using Wpf.Container.Extensions;
using Eye.Base.Extensions;
using Container.Core.Interfaces;
using EyeContainer.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Container.ServiceHelper
{
    public static class ServiceAdder
    {
        public static IServiceCollection AddAvaloniaContainerService(this IServiceCollection services)
        {
            ///////////////////////////naviagtion
            NavigationService navigationService = new NavigationService();

            // NavigationService를 싱글톤으로 등록
            services.AddSingleton<INavigationService>(navigationService);

            // 인터페이스들도 같은 인스턴스로 등록
            services.AddSingleton<INavigationService>(navigationService);
            services.AddSingleton<INavigationRegister>(navigationService);
            services.AddSingleton<IRegionRegister>(navigationService);

            NavigationServiceExtensions._navigationRegister = navigationService;




            /////////////////////////Dialog
            DialogService dialogService = new DialogService();
            // DialogService를 싱글톤으로 등록
            services.AddSingleton<IDialogService>(dialogService);

            // 인터페이스들도 같은 인스턴스로 등록
            services.AddSingleton<IDialogService>(dialogService);
            services.AddSingleton<IDialogRegister>(dialogService);

            DialogServiceExtensions._dialogRegister = dialogService;


            return services;
        }
    }
}
