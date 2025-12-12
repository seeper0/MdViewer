using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MdViewer.Services
{
    public static class WindowManager
    {
        private static readonly Dictionary<string, MainWindow> _openWindows = new(StringComparer.OrdinalIgnoreCase);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        public static bool TryActivateWindow(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);

            if (_openWindows.TryGetValue(fullPath, out var window))
            {
                BringToFront(window);
                return true;
            }

            return false;
        }

        public static MainWindow OpenFile(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);

            if (_openWindows.TryGetValue(fullPath, out var existingWindow))
            {
                BringToFront(existingWindow);
                return existingWindow;
            }

            var window = new MainWindow(fullPath);
            _openWindows[fullPath] = window;
            window.Closed += (s, e) => _openWindows.Remove(fullPath);
            window.Show();
            BringToFront(window);

            return window;
        }

        private static void BringToFront(Window window)
        {
            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            var hwnd = new WindowInteropHelper(window).Handle;

            // 현재 포그라운드 윈도우의 스레드에 연결하여 포커스 권한 획득
            var foregroundHwnd = GetForegroundWindow();
            var foregroundThreadId = GetWindowThreadProcessId(foregroundHwnd, out _);
            var currentThreadId = GetCurrentThreadId();

            if (foregroundThreadId != currentThreadId)
            {
                AttachThreadInput(currentThreadId, foregroundThreadId, true);
                BringWindowToTop(hwnd);
                SetForegroundWindow(hwnd);
                AttachThreadInput(currentThreadId, foregroundThreadId, false);
            }
            else
            {
                BringWindowToTop(hwnd);
                SetForegroundWindow(hwnd);
            }

            window.Activate();
            window.Focus();
        }

        public static bool IsFileOpen(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);
            return _openWindows.ContainsKey(fullPath);
        }
    }
}
