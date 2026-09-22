using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaCad.Editor.Rendering;
using AvaloniaCad.Editor.Tools;
using KoDrawing.Core;
using KoDrawing.Core.Entities;

namespace AvaloniaCad.Editor.Controls;

public class DrawingCanvas : Control
{
    // 휠 한 칸당 10% 확대/축소
    private const double ZoomStep = 1.1;

    // 상태 표시줄 바인딩용. Vector2의 X/Y는 필드라서 Avalonia 바인딩이 안 되므로 Point 사용.
    public static readonly StyledProperty<Point> CursorWorldProperty =
        AvaloniaProperty.Register<DrawingCanvas, Point>(nameof(CursorWorld));

    public static readonly StyledProperty<double> ZoomFactorProperty =
        AvaloniaProperty.Register<DrawingCanvas, double>(nameof(ZoomFactor), 1.0);

    /// <summary>마우스 커서 아래의 월드 좌표 (mm)</summary>
    public Point CursorWorld
    {
        get => GetValue(CursorWorldProperty);
        private set => SetValue(CursorWorldProperty, value);
    }

    /// <summary>1.0 = 100%</summary>
    public double ZoomFactor
    {
        get => GetValue(ZoomFactorProperty);
        private set => SetValue(ZoomFactorProperty, value);
    }

    private ViewportTransform Viewport { get; } = new();

    private CadDocument _document = new();

    public CadDocument Document
    {
        get => _document;
        set { _document = value; InvalidateVisual(); }
    }
    
    private readonly ITool _tool;
    private bool _isPanning;
    private Point _lastPointer;

    public DrawingCanvas()
    {
        ClipToBounds = true;
        Focusable = true;          // Esc 키를 받으려면 필요
        SizeChanged += OnSizeChanged;

        _tool = new LineTool(entity =>
        {
            Document.Add(entity);
            InvalidateVisual();
        });
    }

    public override void Render(DrawingContext context)
    {
        var localBounds = new Rect(Bounds.Size);

        context.Custom(new SkiaDrawOperation(
            localBounds,
            Viewport.Clone(),
            Document.Entities.ToArray(),
            _tool.Preview));
    }

    // ───────── 입력 ─────────

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetCurrentPoint(this);

        // 휠 클릭 드래그 = Pan 
        if (point.Properties.IsMiddleButtonPressed)
        {
            _isPanning = true;
            _lastPointer = point.Position;
            e.Pointer.Capture(this);
            Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Handled = true;
        }
        else if (point.Properties.IsLeftButtonPressed)
        {
            Focus();
            _tool.OnPointerPressed(Viewport.ScreenToWorld(ToVector2(point.Position)));
            InvalidateVisual();
            e.Handled = true;
        }
        else if (point.Properties.IsRightButtonPressed)
        {
            _tool.Cancel();
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var pos = e.GetPosition(this);

        if (_isPanning)
        {
            var delta = pos - _lastPointer;
            Viewport.PanByScreenDelta(new Vector2((float)delta.X, (float)delta.Y));
            _lastPointer = pos;
        }

        _tool.OnPointerMoved(Viewport.ScreenToWorld(ToVector2(pos)));

        UpdateCursorWorld(pos);
        InvalidateVisual();
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape)
        {
            _tool.Cancel();
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isPanning && e.InitialPressMouseButton == MouseButton.Middle)
        {
            e.Pointer.Capture(null);
            EndPan();
            e.Handled = true;
        }
    }

    // 창 밖에서 놓거나 포커스를 잃는 경우에도 Pan 상태가 남지 않게 한다
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        EndPan();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var pos = e.GetPosition(this);
        var factor = (float)Math.Pow(ZoomStep, e.Delta.Y);

        Viewport.ZoomAt(ToVector2(pos), factor);

        ZoomFactor = Viewport.Zoom;
        UpdateCursorWorld(pos);
        InvalidateVisual();

        e.Handled = true;
    }

    // ───────── 내부 ─────────

    private void EndPan()
    {
        _isPanning = false;
        Cursor = null;
    }

    private void UpdateCursorWorld(Point screenPos)
    {
        var world = Viewport.ScreenToWorld(ToVector2(screenPos));
        CursorWorld = new Point(world.X, world.Y);
    }

    private static Vector2 ToVector2(Point p) => new((float)p.X, (float)p.Y);

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Viewport.ViewportCenter = new Vector2(
            (float)(e.NewSize.Width / 2),
            (float)(e.NewSize.Height / 2));

        InvalidateVisual();
    }
}