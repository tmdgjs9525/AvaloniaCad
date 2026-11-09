using System.Collections.ObjectModel;
using System.Numerics;
using AvaloniaCad.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoDrawing.Core;
using KoDrawing.Core.Entities;
using KoDrawing.Core.Tools;

namespace AvaloniaCad.Editor.ViewModels;

public partial class EditorViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SnapStatusText))]
    private bool _snapEnabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentToolName), nameof(ToolHint))]
    private ToolKind _currentToolKind = ToolKind.Select;

    [ObservableProperty] private CadDocument _cadDocument = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection), nameof(SelectedEntityTypeName), nameof(SelectionText), nameof(SelectedEntityLayer))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private Entity? _selectedEntity;

    /// <summary>레이어 패널에서 선택한 레이어 = 새 도형이 들어갈 현재 레이어</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveLayerCommand))]
    private LayerItemViewModel? _selectedLayer;

    public ObservableCollection<LayerItemViewModel> Layers { get; } = new();

    /// <summary>속성 패널에 표시할 선택 도형의 수치 항목</summary>
    public ObservableCollection<PropertyFieldViewModel> Fields { get; } = new();

    public EditorViewModel()
    {
        AttachDocument(CadDocument);
    }

    // ───────── 상태 표시줄 ─────────

    public string SnapStatusText => SnapEnabled ? "Snap: ON" : "Snap: OFF";

    public string CurrentToolName => CurrentToolKind switch
    {
        ToolKind.Select => "선택",
        ToolKind.Line => "선",
        ToolKind.Rectangle => "사각형",
        ToolKind.Circle => "원",
        ToolKind.Polyline => "폴리라인",
        ToolKind.Pan => "화면 이동",
        _ => CurrentToolKind.ToString(),
    };

    public string ToolHint => CurrentToolKind switch
    {
        ToolKind.Select => "클릭: 선택 · 그립 드래그: 편집 · Del: 삭제 · Esc: 선택 해제",
        ToolKind.Line => "시작점과 끝점을 클릭 · Esc: 취소",
        ToolKind.Rectangle => "두 코너를 클릭 · Esc: 취소",
        ToolKind.Circle => "중심과 반지름 위치를 클릭 · Esc: 취소",
        ToolKind.Polyline => "점을 연속 클릭 · 우클릭: 완료 · Esc: 취소",
        ToolKind.Pan => "드래그: 화면 이동 · 휠: 확대/축소",
        _ => "",
    };

    // ───────── 선택 / 속성 ─────────

    public bool HasSelection => SelectedEntity is not null;

    public string SelectedEntityTypeName => SelectedEntity switch
    {
        null => "",
        LineEntity => "선",
        CircleEntity => "원",
        RectangleEntity => "사각형",
        PolylineEntity => "폴리라인",
        _ => SelectedEntity.GetType().Name,
    };

    public string SelectionText => SelectedEntity is null
        ? "선택 없음"
        : $"선택: {SelectedEntityTypeName} ({SelectedEntityLayer?.Name})";

    /// <summary>속성 패널의 레이어 콤보박스. 바꾸면 선택 도형을 그 레이어로 옮긴다.</summary>
    public LayerItemViewModel? SelectedEntityLayer
    {
        get => SelectedEntity is { } entity
            ? Layers.FirstOrDefault(l => l.Layer.Id == entity.LayerId)
            : null;
        set
        {
            if (value is null || SelectedEntity is not { } entity || value.Layer.Id == entity.LayerId)
                return;

            entity.LayerId = value.Layer.Id;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectionText));
            CadDocument.NotifyChanged();   // 숨김/잠금 레이어로 옮겼다면 여기서 선택이 해제됨
        }
    }

    partial void OnSelectedEntityChanged(Entity? value) => BuildFields(value);

    partial void OnSelectedLayerChanged(LayerItemViewModel? value)
    {
        if (value is not null)
            CadDocument.CurrentLayer = value.Layer;
    }

    partial void OnCadDocumentChanged(CadDocument? oldValue, CadDocument newValue)
    {
        if (oldValue is not null)
            oldValue.Changed -= OnDocumentContentChanged;

        SelectedEntity = null;
        AttachDocument(newValue);
    }

    private void AttachDocument(CadDocument document)
    {
        document.Changed += OnDocumentContentChanged;
        RebuildLayers();
    }

    private void OnDocumentContentChanged()
    {
        // 선택 도형이 삭제됐거나 숨김/잠금 레이어에 있게 되면 선택 해제
        if (SelectedEntity is { } selected
            && (!CadDocument.Entities.Contains(selected) || !CadDocument.IsEntityEditable(selected)))
        {
            SelectedEntity = null;
            return;
        }

        // 그립 드래그 등으로 값이 바뀌었을 수 있으니 표시 값 갱신
        foreach (var field in Fields)
            field.Refresh();

        OnPropertyChanged(nameof(SelectionText));
    }

    private void BuildFields(Entity? entity)
    {
        Fields.Clear();

        switch (entity)
        {
            case LineEntity line:
                AddPointFields("시작", () => line.Start, p => line.Start = p);
                AddPointFields("끝", () => line.End, p => line.End = p);
                AddReadOnlyField("길이", () => Vector2.Distance(line.Start, line.End));
                break;

            case CircleEntity circle:
                AddPointFields("중심", () => circle.Center, p => circle.Center = p);
                Fields.Add(new PropertyFieldViewModel(
                    "반지름",
                    () => circle.Radius,
                    v => circle.Radius = (float)v,
                    validate: v => v > 0,
                    onEdited: NotifyEdited));
                AddReadOnlyField("지름", () => circle.Radius * 2);
                break;

            case RectangleEntity rect:
                AddPointFields("코너1", () => rect.Corner1, p => rect.Corner1 = p);
                AddPointFields("코너2", () => rect.Corner2, p => rect.Corner2 = p);
                AddReadOnlyField("폭", () => MathF.Abs(rect.Corner2.X - rect.Corner1.X));
                AddReadOnlyField("높이", () => MathF.Abs(rect.Corner2.Y - rect.Corner1.Y));
                break;

            case PolylineEntity polyline:
                AddReadOnlyField("점 개수", () => polyline.Points.Count, unit: "");
                AddReadOnlyField("길이", () => PolylineLength(polyline));
                for (var i = 0; i < polyline.Points.Count; i++)
                {
                    var index = i;   // 클로저 캡처용
                    AddPointFields($"P{index + 1}", () => polyline.Points[index], p => polyline.Points[index] = p);
                }
                break;
        }
    }

    private void AddPointFields(string label, Func<Vector2> get, Action<Vector2> set)
    {
        Fields.Add(new PropertyFieldViewModel(
            $"{label} X", () => get().X, v => set(new Vector2((float)v, get().Y)), onEdited: NotifyEdited));
        Fields.Add(new PropertyFieldViewModel(
            $"{label} Y", () => get().Y, v => set(new Vector2(get().X, (float)v)), onEdited: NotifyEdited));
    }

    private void AddReadOnlyField(string label, Func<double> get, string unit = "mm")
    {
        Fields.Add(new PropertyFieldViewModel(label, get, unit: unit));
    }

    private void NotifyEdited() => CadDocument.NotifyChanged();

    private static double PolylineLength(PolylineEntity polyline)
    {
        var length = 0.0;
        for (var i = 0; i < polyline.Points.Count - 1; i++)
            length += Vector2.Distance(polyline.Points[i], polyline.Points[i + 1]);
        return length;
    }

    // ───────── 레이어 ─────────

    private void RebuildLayers()
    {
        Layers.Clear();
        foreach (var layer in CadDocument.Layers)
            Layers.Add(new LayerItemViewModel(layer, CadDocument));

        SelectedLayer = FindLayerItem(CadDocument.CurrentLayer);
        RemoveLayerCommand.NotifyCanExecuteChanged();
    }

    private LayerItemViewModel? FindLayerItem(Layer layer) =>
        Layers.FirstOrDefault(l => ReferenceEquals(l.Layer, layer));

    [RelayCommand]
    private void AddLayer()
    {
        var layer = CadDocument.AddLayer(NextLayerName());
        var item = new LayerItemViewModel(layer, CadDocument);
        Layers.Add(item);
        SelectedLayer = item;
        RemoveLayerCommand.NotifyCanExecuteChanged();
    }

    private bool CanRemoveLayer() => SelectedLayer is not null && Layers.Count > 1;

    [RelayCommand(CanExecute = nameof(CanRemoveLayer))]
    private void RemoveLayer()
    {
        if (SelectedLayer is not { } item || !CadDocument.RemoveLayer(item.Layer))
            return;

        // 삭제된 레이어의 도형은 Core에서 첫 번째 레이어로 옮겨짐
        Layers.Remove(item);
        SelectedLayer = FindLayerItem(CadDocument.CurrentLayer);
        RemoveLayerCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(SelectedEntityLayer));
        OnPropertyChanged(nameof(SelectionText));
    }

    private string NextLayerName()
    {
        for (var n = 1; ; n++)
        {
            var name = $"Layer {n}";
            if (Layers.All(l => l.Name != name))
                return name;
        }
    }

    // ───────── 명령 ─────────

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DeleteSelected()
    {
        if (SelectedEntity is not { } entity)
            return;

        SelectedEntity = null;
        CadDocument.Remove(entity);
    }

    [RelayCommand]
    private void SelectTool(ToolKind kind)
    {
        CurrentToolKind = kind;
    }

    /// <summary>F3: 스냅 켜기/끄기</summary>
    [RelayCommand]
    private void ToggleSnap()
    {
        SnapEnabled = !SnapEnabled;
    }
}