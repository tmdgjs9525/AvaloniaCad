using EyeContainer.Core.Interfaces;
using Avalonia.Controls;

namespace Avalonia.Container.Dialog
{
    public interface IDialogRegister
    {
        public void AddTransientDialog<TView, TViewModel>() where TView : Control
                                                            where TViewModel : IViewModelBase, IDialogAware;
        public void AddSingletonDialog<TView, TViewModel>() where TView : Control
                                                            where TViewModel : IViewModelBase, IDialogAware;
    }
}