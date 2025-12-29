using System.Windows;
using MdViewer.Services;

namespace MdViewer
{
    public partial class App : Application
    {
        private PipeService? _pipeService;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var filePath = e.Args.Length > 0 ? e.Args[0] : null;

            // 기존 인스턴스에 파일 열기 요청 시도
            if (filePath != null && PipeService.TrySendToExistingInstance(filePath))
            {
                Shutdown();
                return;
            }

            // 파일 연결 등록 (첫 실행 시)
            if (!FileAssociationService.IsAssociated())
            {
                if (!FileAssociationService.Associate())
                {
                    MessageBox.Show(
                        ".md 파일 연결 등록에 실패했습니다.\n프로그램은 계속 실행됩니다.",
                        "경고",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }

            // Pipe 서버 시작
            _pipeService = new PipeService();
            _pipeService.FileRequested += OnFileRequested;
            _pipeService.StartServer();

            // 창 열기
            if (filePath != null)
            {
                WindowManager.OpenFile(filePath);
            }
            else
            {
                // 기본으로 실행 파일 위치의 README.md 열기
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                var defaultFile = System.IO.Path.Combine(exeDir, "README.md");
                if (System.IO.File.Exists(defaultFile))
                {
                    WindowManager.OpenFile(defaultFile);
                }
                else
                {
                    var window = new MainWindow();
                    window.Show();
                }
            }
        }

        private void OnFileRequested(string filePath)
        {
            Dispatcher.Invoke(() =>
            {
                WindowManager.OpenFile(filePath);
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _pipeService?.Dispose();
            base.OnExit(e);
        }
    }
}
