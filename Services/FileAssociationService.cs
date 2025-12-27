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
                // .md 확장자가 MdViewer.md와 연결되어 있는지 확인
                using var extKey = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{Extension}");
                if (extKey == null) return false;

                var progIdValue = extKey.GetValue("") as string;
                if (progIdValue != ProgId) return false;

                // 등록된 실행 파일 경로가 현재 실행 중인 경로와 일치하는지 확인
                using var cmdKey = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ProgId}\shell\open\command");
                if (cmdKey == null) return false;

                var registeredCommand = cmdKey.GetValue("") as string;
                var currentExePath = Environment.ProcessPath;

                if (string.IsNullOrEmpty(registeredCommand) || string.IsNullOrEmpty(currentExePath))
                    return false;

                // 등록된 명령에서 실행 파일 경로 추출 ("경로" "%1" 형식)
                var registeredExePath = registeredCommand.Split('"')[1];

                return string.Equals(registeredExePath, currentExePath, StringComparison.OrdinalIgnoreCase);
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
