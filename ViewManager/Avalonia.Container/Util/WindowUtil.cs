using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace Avalonia.Container.Util
{
    internal static class WindowUtil
    {
        public static Window? GetActiveWindow()
        {
            // 1. Avalonia의 UI 스레드 Dispatcher 사용
            if (!Dispatcher.UIThread.CheckAccess())
            {
                // 2. UI 스레드로 동기 호출
                return Dispatcher.UIThread.Invoke(GetActiveWindow);
            }

            // 3. ApplicationLifetime 확인
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // 4. 활성 창 찾기 (desktop.Windows에서)
                // (참고: desktop.Windows는 이미 Window 컬렉션이므로 OfType<Window>() 불필요)
                return desktop.Windows.FirstOrDefault(w => w.IsActive)
                       ?? desktop.MainWindow; // 5. 백업 (desktop.MainWindow)
            }

            // 데스크톱 앱이 아니거나(예: 모바일) Application.Current가 null이면
            return null;
        }
    }
    
}
