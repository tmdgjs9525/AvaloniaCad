using Container.Core.Interfaces;
using Avalonia.Controls;

namespace Avalonia.Container.Navigate
{

    public interface INavigationRegister
    {
        public void AddTransientNavigation<TView, TViewModel>() where TView : Control
                                                                where TViewModel : IViewModelBase;
        public void AddSingletonNavigation<TView, TViewModel>() where TView : Control
                                                                where TViewModel : IViewModelBase;

        public void AddSingletonNavigation<TInterface, TImplementation, TViewModel>()
                    where TInterface : class
                    where TImplementation : Control, TInterface
                    where TViewModel : IViewModelBase;

        public void AddTransientNavigation<TInterface, TImplementation, TViewModel>()
                   where TInterface : class
                   where TImplementation : Control, TInterface
                   where TViewModel : IViewModelBase;
        void AddTransientNavigation<TView>(IViewModelBase viewModelInstance) where TView : Control;
        void AddSingletonNavigation<TView>(IViewModelBase viewModelInstance) where TView : Control;
    }


}
