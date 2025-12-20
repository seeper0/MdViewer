using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using MdViewer.Models;
using MdViewer.Services;

namespace MdViewer
{
    public partial class MainWindow : Window
    {
        private string? _filePath;

        public MainWindow()
        {
            InitializeComponent();
            LoadWindowSettings();
            Closing += MainWindow_Closing;

            // 링크 클릭 이벤트 핸들링
            CommandBindings.Add(new CommandBinding(
                NavigationCommands.GoToPage,
                OnNavigate));

            // 마크다운 렌더링 후 코드 블록 폰트 적용
            MarkdownViewer.Loaded += (s, e) => ApplyCodeBlockFont();
        }

        private void ApplyCodeBlockFont()
        {
            if (MarkdownViewer.Document == null) return;

            var codeFont = new System.Windows.Media.FontFamily("D2Coding, GulimChe, DotumChe, Consolas, Courier New");

            foreach (var block in MarkdownViewer.Document.Blocks)
            {
                ApplyFontToCodeBlocks(block, codeFont);
            }
        }

        private void ApplyFontToCodeBlocks(System.Windows.Documents.Block block, System.Windows.Media.FontFamily codeFont)
        {
            if (block.Tag?.ToString() == "CodeBlock")
            {
                block.FontFamily = codeFont;
                block.FontSize = 13;
            }

            if (block is Section section)
            {
                if (section.Tag?.ToString() == "CodeBlock")
                {
                    section.FontFamily = codeFont;
                    section.FontSize = 13;
                }
                foreach (var child in section.Blocks)
                {
                    ApplyFontToCodeBlocks(child, codeFont);
                }
            }
        }


        public MainWindow(string filePath) : this()
        {
            LoadFile(filePath);
        }

        public string? FilePath => _filePath;

        private void LoadWindowSettings()
        {
            var settings = SettingsService.Load();
            Width = settings.WindowWidth;
            Height = settings.WindowHeight;
            Left = settings.WindowLeft;
            Top = settings.WindowTop;
        }

        private void SaveWindowSettings()
        {
            var settings = new AppSettings
            {
                WindowWidth = Width,
                WindowHeight = Height,
                WindowLeft = Left,
                WindowTop = Top
            };
            SettingsService.Save(settings);
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings();
        }

        public void LoadFile(string filePath)
        {
            _filePath = Path.GetFullPath(filePath);

            if (!File.Exists(_filePath))
            {
                MessageBox.Show($"파일을 찾을 수 없습니다:\n{_filePath}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            try
            {
                var content = File.ReadAllText(_filePath);
                MarkdownViewer.Markdown = content;
                Title = $"{Path.GetFileName(_filePath)} - MdViewer";
                ApplyCodeBlockFont();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일을 읽을 수 없습니다:\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadFile()
        {
            if (!string.IsNullOrEmpty(_filePath))
            {
                LoadFile(_filePath);
            }
        }

        private void OnNavigate(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is not string uri)
                return;

            // 외부 링크 (http/https)
            if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
                return;
            }

            // 이미지 링크 - 파일 존재 여부 상관없이 Windows에 넘김
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico" };
            var extension = Path.GetExtension(uri).ToLowerInvariant();

            if (imageExtensions.Contains(extension))
            {
                var imagePath = ResolvePathWithoutExistenceCheck(uri);
                if (imagePath != null)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이미지를 열 수 없습니다:\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return;
            }

            // 내부 링크 (.md 파일)
            var targetPath = ResolveRelativePath(uri);

            if (targetPath != null && targetPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                WindowManager.OpenFile(targetPath);
                return;
            }
        }

        private string? ResolvePathWithoutExistenceCheck(string relativePath)
        {
            if (string.IsNullOrEmpty(_filePath))
                return null;

            var baseDir = Path.GetDirectoryName(_filePath);
            if (baseDir == null)
                return null;

            return Path.GetFullPath(Path.Combine(baseDir, relativePath));
        }

        private string? ResolveRelativePath(string relativePath)
        {
            if (string.IsNullOrEmpty(_filePath))
                return null;

            var baseDir = Path.GetDirectoryName(_filePath);
            if (baseDir == null)
                return null;

            var fullPath = Path.GetFullPath(Path.Combine(baseDir, relativePath));
            return File.Exists(fullPath) ? fullPath : null;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.F5:
                    ReloadFile();
                    break;
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && files[0].EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    var filePath = files[0];
                    if (filePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadFile(filePath);
                    }
                }
            }
        }
    }
}
