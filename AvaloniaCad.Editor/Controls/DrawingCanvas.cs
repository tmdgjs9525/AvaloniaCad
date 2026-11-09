using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaCad.Editor.Rendering;
using KoDrawing.Core;
using KoDrawing.Core.Entities;
using KoDrawing.Core.HitTesting;
using KoDrawing.Core.Rendering;
using KoDrawing.Core.Tools;

namespace AvaloniaCad.Editor.Controls;

public class DrawingCanvas : Control
{
    private ITool _tool = new NullTool();
    // 휠 한 칸당 10% 확대/축소
    private const double ZoomStep = 1.1;

    static DrawingCanvas()
    {
        DocumentProperty.Changed.AddClassHandler<DrawingCanvas>((c, e) => c.OnDocumentChanged(e));
        ToolKindProperty.Changed.AddClassHandler<DrawingCanvas>((c, _) => c.RebuildTool()); 
        SelectedEntityProperty.Changed.AddClassHandler<DrawingCanvas>((c, e) => c.OnSelectedEntityChanged(e));
    }

    // 상태 표시줄 바인딩용. Vector2의 X/Y는 필드라서 Avalonia 바인딩이 안 되므로 Point 사용.
    public static readonly StyledProperty<Point> CursorWorldProperty =
        AvaloniaProperty.Register<DrawingCanvas, Point>(nameof(CursorWorld));

    public static readonly StyledProperty<double> ZoomFactorProperty =
        AvaloniaProperty.Register<DrawingCanvas, double>(nameof(ZoomFactor), 1.0);

    public static readonly StyledProperty<CadDocument?> DocumentProperty =
        AvaloniaProperty.Register<DrawingCanvas, CadDocument?>(nameof(Document));
    
    public static readonly StyledProperty<bool> SnapEnabledProperty = AvaloniaProperty.Register<DrawingCanvas, bool>(
        nameof(SnapEnabled));

    public static readonly StyledProperty<ToolKind> ToolKindProperty =
        AvaloniaProperty.Register<DrawingCanvas, ToolKind>(nameof(ToolKind));

    public static readonly StyledProperty<Entity?> SelectedEntityProperty =
        AvaloniaProperty.Register<DrawingCanvas, Entity?>(nameof(SelectedEntity), defaultBindingMode: BindingMode.TwoWay);

    public ToolKind ToolKind
    {
        get => GetValue(ToolKindProperty);
        set => SetValue(ToolKindProperty, value);
    }

    public Entity? SelectedEntity
    {
        get => GetValue(SelectedEntityProperty);
        set => SetValue(SelectedEntityProperty, value);
    }

    
    public bool SnapEnabled
    {
        get => GetValue(SnapEnabledProperty);
        set => SetValue(SnapEnabledProperty, value);
    }
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

    public CadDocument? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    private ViewportTransform Viewport { get; } = new();

    private bool _isPanning;
    private Point _lastPointer;
    private Point _lastCursorPos;   // 포인터 캡처를 잃었을 때 드래그를 마무리할 위치

    public DrawingCanvas()
    {
        ClipToBounds = true;
        Focusable = true;          // Esc 키를 받으려면 필요
        SizeChanged += OnSizeChanged;

        // ToolKind 기본값(Select)과 바인딩 값이 같으면 Changed가 발생하지 않으므로
        // 생성 시점에 한 번 직접 도구를 만들어 둔다.
        RebuildTool();
    }

    public override void Render(DrawingContext context)
    {
        var localBounds = new Rect(Bounds.Size);

        var entities = Document?.Entities ?? Array.Empty<Entity>();
        var preview = _tool.Preview ?? Array.Empty<Entity>();
        var grips = _tool is SelectTool select ? select.CurrentGrips : Array.Empty<Grip>();
        
        var scene = new CadScene(
            Viewport.Clone(), entities.ToArray(), preview, grips, SelectedEntity);

        context.Custom(new SkiaDrawOperation(localBounds, scene));
    }

    // ───────── 입력 ─────────

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetCurrentPoint(this);
        Focus();   // 어떤 버튼이든 캔버스를 누르면 포커스를 가져와 Esc 등을 받는다

        // 휠 클릭 드래그 = Pan, Pan 도구일 때는 왼쪽 드래그도 Pan
        if (point.Properties.IsMiddleButtonPressed
            || (point.Properties.IsLeftButtonPressed && ToolKind == ToolKind.Pan))
        {
            _isPanning = true;
            _lastPointer = point.Position;
            e.Pointer.Capture(this);
            Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Handled = true;
        }
        else if (point.Properties.IsLeftButtonPressed)
        {
            _tool.OnPointerPressed(WorldPositionFor(point.Position)); 
            InvalidateVisual();
            e.Handled = true;
        }
        else if (point.Properties.IsRightButtonPressed)
        {
            if (_tool is PolylineTool polylineTool)
                polylineTool.Finish();
            else
                _tool.Cancel();

            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var pos = e.GetPosition(this);
        _lastCursorPos = pos;

        if (_isPanning)
        {
            var delta = pos - _lastPointer;
            Viewport.PanByScreenDelta(new Vector2((float)delta.X, (float)delta.Y));
            _lastPointer = pos;
        }

        _tool.OnPointerMoved(WorldPositionFor(pos));  

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

        if (_isPanning)
        {
            e.Pointer.Capture(null);
            EndPan();
        }
        else if (e.InitialPressMouseButton == MouseButton.Left)
        {
            // 왼쪽 버튼을 놓았을 때만 도구에 전달 (우클릭/휠클릭 해제는 무시)
            _tool.OnPointerReleased(WorldPositionFor(e.GetPosition(this)));
            InvalidateVisual();
        }

        e.Handled = true;
    }

    // 창 밖에서 놓거나 포커스를 잃는 경우에도 Pan/그립 드래그 상태가 남지 않게 한다
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        EndPan();
        _tool.OnPointerReleased(WorldPositionFor(_lastCursorPos));
        InvalidateVisual();
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

    private void OnDocumentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is CadDocument oldDoc)
            oldDoc.Changed -= InvalidateVisual;

        if (e.NewValue is CadDocument newDoc)
            newDoc.Changed += InvalidateVisual;

        // 문서가 바뀌면 그리던 도형과 선택은 의미가 없으므로 정리
        _tool.Cancel();
        SelectedEntity = null;

        InvalidateVisual();
    }

    // ViewModel 쪽에서 선택이 바뀐 경우(예: 삭제 후 null) SelectTool의 그립도 맞춰 준다
    private void OnSelectedEntityChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_tool is SelectTool selectTool)
            selectTool.SetSelection(e.NewValue as Entity);

        InvalidateVisual();
    }
    
    private void RebuildTool()
    {
        // 이전 도구의 작업(그리던 도형, 선택)을 정리하고 바꾼다
        _tool.Cancel();
        SelectedEntity = null;

        _tool = ToolKind switch
        {
            ToolKind.Select => new SelectTool(
                () => Document,               // 항상 현재 문서를 보도록 함수로 전달
                entity => SelectedEntity = entity,
                () => 10f / Viewport.Zoom),   // 화면 10px를 현재 줌 기준 mm로 환산
            ToolKind.Line => new LineTool(entity => Document?.Add(entity)),
            ToolKind.Rectangle => new RectangleTool(entity => Document?.Add(entity)),
            ToolKind.Circle => new CircleTool(entity => Document?.Add(entity)),
            ToolKind.Polyline => new PolylineTool(entity => Document?.Add(entity)),
            _ => new NullTool(),   // Pan은 OnPointerPressed에서 직접 처리
        };

        InvalidateVisual();
    }

    private void EndPan()
    {
        _isPanning = false;
        Cursor = null;
    }

    // 상태 표시줄에는 실제로 찍힐 좌표(스냅 적용 후)를 보여 준다
    private void UpdateCursorWorld(Point screenPos)
    {
        var world = WorldPositionFor(screenPos);
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
    
    private Vector2 ScreenToSnappedWorld(Point screenPos)
    {
        var world = Viewport.ScreenToWorld(ToVector2(screenPos));

        if (!SnapEnabled)
            return world;

        var spacing = GridRenderer.ChooseSpacing(Viewport.Zoom);
        return new Vector2(
            MathF.Round(world.X / spacing) * spacing,
            MathF.Round(world.Y / spacing) * spacing);
    }
    
    private Vector2 WorldPositionFor(Point screenPos)
    {
        // Select 도구의 클릭 판정은 실제 위치로 해야 하므로 스냅을 적용하지 않는다.
        // 단, 그립을 잡고 끄는 중에는 정확한 좌표에 놓이도록 스냅을 적용한다.
        var useRawPosition = (ToolKind is ToolKind.Select or ToolKind.Pan)
                             && _tool is not SelectTool { IsDraggingGrip: true };

        return useRawPosition
            ? Viewport.ScreenToWorld(ToVector2(screenPos))
            : ScreenToSnappedWorld(screenPos);
    }
}