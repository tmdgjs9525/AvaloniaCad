using System;
using System.Threading.Tasks;
using Container.Core.Parameter;

namespace Container.Core.Interfaces
{
    public interface IDialogService
    {
        void ShowDialog(string viewName, Parameters? parameters = null, Action<IDialogResult?>? callback = null, StartPosition? startPosition = null);

        Task<IDialogResult?> ShowDialogAsync(string viewName, Parameters? parameters = null, StartPosition? startPosition = null);
        
        void Show(string viewName, Parameters? parameters = null,  StartPosition? startPosition = null);
    }


    public interface IDialogResult
    {
        bool Success { get; set; }
        Parameters Parameters { get; set; }
    }

    public interface IDialogAware
    {
        bool CanCloseDialog();
        void OnDialogClosed();
        void OnDialogOpened(Parameters parameters);
        string? Title { get; set; }

        event Action<IDialogResult?>? RequestClose;
    }

    public enum StartPosition
    {
        Center,
        Left,
        Top,
        Right,
        Bottom,
        LeftUp,
        RightUp,
        LeftDown,
        RightDown,
    }
}
