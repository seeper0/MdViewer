using Microsoft.Win32;

namespace MdViewer.Services
{
    public static class FileAssociationService
    {
        private const string Extension = ".md";
        private const string ProgId = "MdViewer.md";
        private const string FileDescription = "Markdown File";

        public static bool IsAssociated()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{Extension}");
                if (key == null) return false;

                var value = key.GetValue("") as string;
                return value == ProgId;
            }
            catch
            {
                return false;
            }
        }

        public static bool Associate()
        {
            try
            {
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath))
                    return false;

                // .md 확장자 등록
                using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{Extension}"))
                {
                    key.SetValue("", ProgId);
                }

                // ProgId 등록
                using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}"))
                {
                    key.SetValue("", FileDescription);
                }

                // 아이콘 등록 (shell32.dll 노트 아이콘 - 인덱스 70)
                using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}\DefaultIcon"))
                {
                    key.SetValue("", @"%SystemRoot%\System32\shell32.dll,70");
                }

                // 열기 명령 등록
                using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}\shell\open\command"))
                {
                    key.SetValue("", $"\"{exePath}\" \"%1\"");
                }

                // 탐색기에 변경 알림
                NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        public static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
