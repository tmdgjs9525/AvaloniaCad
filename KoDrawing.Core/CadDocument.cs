using KoDrawing.Core.Entities;

namespace KoDrawing.Core;

public sealed class CadDocument
{
    private readonly List<Entity> _entities = new();
    private readonly List<Layer> _layers = new();
    private Layer _currentLayer;

    public CadDocument()
    {
        _currentLayer = new Layer("Default");
        _layers.Add(_currentLayer);
    }

    public IReadOnlyList<Entity> Entities => _entities;
    public IReadOnlyList<Layer> Layers => _layers;

    /// <summary>새로 그리는 도형이 들어갈 레이어</summary>
    public Layer CurrentLayer
    {
        get => _currentLayer;
        set
        {
            if (!_layers.Contains(value))
                throw new ArgumentException("문서에 없는 레이어입니다.", nameof(value));

            _currentLayer = value;
        }
    }

    /// <summary>도형/레이어가 추가·삭제·변경될 때 발생. 화면 갱신 신호로 사용.</summary>
    public event Action? Changed;

    /// <summary>
    /// 도형 속성을 직접 바꾼 경우(그립 드래그, 속성 패널 입력 등) 호출해서 변경을 알린다.
    /// </summary>
    public void NotifyChanged() => Changed?.Invoke();

    public void Add(Entity entity)
    {
        if (FindLayer(entity.LayerId) is null)
            entity.LayerId = _currentLayer.Id;

        _entities.Add(entity);
        Changed?.Invoke();
    }

    public bool Remove(Entity entity)
    {
        var removed = _entities.Remove(entity);
        if (removed)
            Changed?.Invoke();
        return removed;
    }

    // ───────── 레이어 ─────────

    public Layer AddLayer(string name)
    {
        var layer = new Layer(name);
        _layers.Add(layer);
        Changed?.Invoke();
        return layer;
    }

    /// <summary>
    /// 레이어를 삭제한다. 마지막 남은 레이어는 삭제할 수 없다.
    /// 삭제된 레이어의 도형은 첫 번째 레이어로 옮긴다.
    /// </summary>
    public bool RemoveLayer(Layer layer)
    {
        if (_layers.Count <= 1 || !_layers.Remove(layer))
            return false;

        var fallback = _layers[0];
        foreach (var entity in _entities)
        {
            if (entity.LayerId == layer.Id)
                entity.LayerId = fallback.Id;
        }

        if (ReferenceEquals(_currentLayer, layer))
            _currentLayer = fallback;

        Changed?.Invoke();
        return true;
    }

    public Layer? FindLayer(Guid id) => _layers.Find(l => l.Id == id);

    /// <summary>화면에 그려야 하는 도형인지 (레이어가 보이는 상태)</summary>
    public bool IsEntityVisible(Entity entity) => FindLayer(entity.LayerId)?.IsVisible ?? true;

    /// <summary>선택/편집할 수 있는 도형인지 (레이어가 보이고 잠기지 않은 상태)</summary>
    public bool IsEntityEditable(Entity entity)
    {
        var layer = FindLayer(entity.LayerId);
        return layer is null || (layer.IsVisible && !layer.IsLocked);
    }
}