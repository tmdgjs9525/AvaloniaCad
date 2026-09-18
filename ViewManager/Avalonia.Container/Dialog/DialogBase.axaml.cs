using Avalonia.Container.Util;
using Container.Core.Interfaces;
using Avalonia;
using Avalonia.Animation; // Animation, KeyFrame
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input; // PointerPressedEventArgs
using Avalonia.Interactivity;
using Avalonia.Media; // ScaleTransform
using Avalonia.Styling;

namespace Avalonia.Container;

public partial class DialogBase : Window
{
    private readonly StartPosition? _startPosition;
    public DialogBase()
    {
        InitializeComponent();

        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public DialogBase(StartPosition? startPosition)
        : this()
    {
        _startPosition = startPosition;
    }
}