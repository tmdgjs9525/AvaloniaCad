using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Diagnostics;

namespace Avalonia.Container.Region
{
    /// <summary>
    /// ContentControl 컨트롤에 붙이는 어태치 프로퍼티
    /// 해당 클래스로 Region을 등록한다.
    /// (Avalonia 변환 버전)
    /// </summary>
    public class RegionManager // 굳이 ContentControl에서 상속받을 필요가 없습니다.
    {
        // 1. Avalonia AttachedProperty 정의
        // TValue의 제네릭 타입을 사용하는 것이 일반적입니다.
        // WPF와 달리 프로퍼티 이름 뒤에 'Property'를 붙이는 것이 강력한 관례입니다.
        public static readonly AttachedProperty<string?> RegionNameProperty =
             AvaloniaProperty.RegisterAttached<RegionManager, Control, string?>( // <--- 여기를 수정했습니다.
                 "RegionName",           // XAML에서 사용할 이름
                 defaultValue: default,
                 inherits: false         // 값 상속 여부
             );

        // 2. Get/Set 헬퍼 메서드 (필수)
        // XAML 컴파일러가 이 정적 메서드들을 사용합니다.
        // WPF의 UIElement 대신 Control 또는 AvaloniaObject를 사용합니다.
        public static void SetRegionName(Control element, string value)
        {
            element.SetValue(RegionNameProperty, value);
        }

        public static string? GetRegionName(Control element)
        {
            return element.GetValue(RegionNameProperty);
        }

        // 3. 정적 생성자 (중요!)
        // Avalonia에서는 정적 생성자에서 변경 핸들러를 등록합니다.
        static RegionManager()
        {
            // RegionNameProperty의 값이 변경될 때 OnRegionNameChanged를 호출합니다.
            // <ContentControl>로 타입을 한정하면, 콜백 메서드에서 (ContentControl)d 캐스팅이
            // 필요 없어지고 타입 안정성이 높아집니다.
            RegionNameProperty.Changed.AddClassHandler<ContentControl>(OnRegionNameChanged);
        }

        // 4. 변경 콜백 메서드
        // 첫 번째 인자가 DependencyObject d 대신 <T>로 지정한 ContentControl control이 됩니다.
        // EventArgs 타입도 AvaloniaPropertyChangedEventArgs로 변경됩니다.
        private static void OnRegionNameChanged(ContentControl control, AvaloniaPropertyChangedEventArgs e)
        {
            // Avalonia의 디자인 모드 확인
            if (Design.IsDesignMode == false)
            {
                // 런타임
                // Ioc.Default... 코드는 프레임워크와 무관하므로 그대로 작동합니다.
                var navigationService = Ioc.Default.GetRequiredService<IRegionRegister>();

                // 'control'은 이미 ContentControl 타입으로 보장됩니다.
                if ((string?)e.NewValue != null)
                {
                    navigationService.RegisterRegion((string)e.NewValue, control);
                }
                else
                {
                    Debug.WriteLine("RegionName이 null입니다.");
                }
            }
        }
    }
}