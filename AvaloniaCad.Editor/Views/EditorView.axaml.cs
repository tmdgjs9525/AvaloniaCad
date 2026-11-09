using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaCad.Editor.ViewModels;
using KoDrawing.Core.Tools;

namespace AvaloniaCad.Editor.Views;

public partial class EditorView : UserControl
{
    public EditorView()
    {
        InitializeComponent();

        // 시작하자마자 단축키/Esc가 먹도록 캔버스에 포커스
        Loaded += (_, _) => DrawingArea.Focus();
    }

    /// <summary>
    /// 편집기 단축키. 버튼 HotKey는 TextBox에 글자를 칠 때도 반응하므로
    /// 여기서 직접 처리하고, 입력 칸에 포커스가 있을 때는 건너뛴다.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled || e.Source is TextBox || e.KeyModifiers != KeyModifiers.None)
            return;

        if (DataContext is not EditorViewModel vm)
            return;

        ToolKind? tool = e.Key switch
        {
            Key.V => ToolKind.Select,
            Key.L => ToolKind.Line,
            Key.R => ToolKind.Rectangle,
            Key.C => ToolKind.Circle,
            Key.P => ToolKind.Polyline,
            Key.H => ToolKind.Pan,
            _ => null,
        };

        if (tool is { } kind)
        {
            vm.SelectToolCommand.Execute(kind);
            e.Handled = true;
            return;
        }

        switch (e.Key)
        {
            case Key.F3:
                vm.ToggleSnapCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Delete:
                if (vm.DeleteSelectedCommand.CanExecute(null))
                    vm.DeleteSelectedCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}