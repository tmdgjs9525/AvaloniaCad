using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaCad.Editor.ViewModels;

/// <summary>
/// 속성 패널의 숫자 입력 한 칸. 도형 값을 읽고 쓰는 함수를 받아서 동작한다.
/// 입력하는 즉시(파싱이 되면) 도형에 반영된다.
/// </summary>
public sealed class PropertyFieldViewModel : ObservableObject
{
    private readonly Func<double> _getValue;
    private readonly Action<double>? _setValue;
    private readonly Func<double, bool>? _validate;
    private readonly Action? _onEdited;

    // 내가 값을 쓰는 중에 Refresh가 오면 입력 중인 글자("12." 등)를 덮어쓰지 않도록 건너뛴다
    private bool _isApplying;

    public PropertyFieldViewModel(
        string label,
        Func<double> getValue,
        Action<double>? setValue = null,
        string unit = "mm",
        Func<double, bool>? validate = null,
        Action? onEdited = null)
    {
        Label = label;
        Unit = unit;
        _getValue = getValue;
        _setValue = setValue;
        _validate = validate;
        _onEdited = onEdited;
    }

    public string Label { get; }
    public string Unit { get; }
    public bool IsReadOnly => _setValue is null;

    public string Text
    {
        get => _getValue().ToString("0.###", CultureInfo.InvariantCulture);
        set
        {
            if (_setValue is null)
                return;

            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                || !double.IsFinite(number))
                return;   // 입력 중인 미완성 값("", "-" 등)은 무시

            if (_validate is not null && !_validate(number))
                return;

            _isApplying = true;
            try
            {
                _setValue(number);
                _onEdited?.Invoke();
            }
            finally
            {
                _isApplying = false;
            }
        }
    }

    /// <summary>도형이 바깥에서 바뀌었을 때(그립 드래그 등) 표시 값을 갱신</summary>
    public void Refresh()
    {
        if (!_isApplying)
            OnPropertyChanged(nameof(Text));
    }
}
