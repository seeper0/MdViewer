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
        }

        public MainWindow(string filePath) : this()
        {
            // Show() 후 Loaded 이벤트에서 파일 로드
            var path = filePath;
            Loaded += (s, e) => LoadFile(path);
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
                MessageBox.Show(this, $"파일을 찾을 수 없습니다:\n{_filePath}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            try
            {
                var content = File.ReadAllText(_filePath);
                MarkdownViewer.Markdown = content;
                Title = $"{Path.GetFileName(_filePath)} - MdViewer";

                // 렌더링 완료 후 처리
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
                {
                    ApplyCodeBlockFont();
                    SetupHyperlinks();
                });
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

        private void ApplyCodeBlockFont()
        {
            if (MarkdownViewer.Document == null) return;

            var codeFont = new System.Windows.Media.FontFamily("D2Coding, GulimChe, DotumChe, Consolas, Courier New");

            foreach (var block in MarkdownViewer.Document.Blocks)
            {
                ApplyFontToCodeBlocks(block, codeFont);
            }
        }

        private void ApplyFontToCodeBlocks(Block block, System.Windows.Media.FontFamily codeFont)
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

        private void SetupHyperlinks()
        {
            if (MarkdownViewer.Document == null) return;

            foreach (var block in MarkdownViewer.Document.Blocks)
            {
                SetupHyperlinksInBlock(block);
            }
        }

        private void SetupHyperlinksInBlock(Block block)
        {
            if (block is Paragraph paragraph)
            {
                foreach (var inline in paragraph.Inlines)
                {
                    SetupHyperlinksInInline(inline);
                }
            }
            else if (block is Section section)
            {
                foreach (var child in section.Blocks)
                {
                    SetupHyperlinksInBlock(child);
                }
            }
            else if (block is List list)
            {
                foreach (var item in list.ListItems)
                {
                    foreach (var child in item.Blocks)
                    {
                        SetupHyperlinksInBlock(child);
                    }
                }
            }
            else if (block is Table table)
            {
                foreach (var rowGroup in table.RowGroups)
                {
                    foreach (var row in rowGroup.Rows)
                    {
                        foreach (var cell in row.Cells)
                        {
                            foreach (var child in cell.Blocks)
                            {
                                SetupHyperlinksInBlock(child);
                            }
                        }
                    }
                }
            }
        }

        private void SetupHyperlinksInInline(Inline inline)
        {
            if (inline is Hyperlink hyperlink)
            {
                // MdXaml이 CommandParameter에 URL을 저장함
                var uri = hyperlink.CommandParameter?.ToString()
                          ?? hyperlink.NavigateUri?.ToString();

                if (!string.IsNullOrEmpty(uri))
                {
                    // 기존 Command 제거하고 Click 이벤트로 처리
                    hyperlink.Command = null;
                    hyperlink.Click += (s, e) => OnHyperlinkClick(uri);
                }
            }
            else if (inline is Span span)
            {
                foreach (var child in span.Inlines)
                {
                    SetupHyperlinksInInline(child);
                }
            }
        }

        private void OnHyperlinkClick(string uri)
        {
            // http/https 링크
            if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
                return;
            }

            // .md 파일 링크
            if (uri.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                var targetPath = ResolveRelativePath(uri);
                WindowManager.OpenFile(targetPath);
            }
        }

        private string ResolveRelativePath(string relativePath)
        {
            if (string.IsNullOrEmpty(_filePath))
                return relativePath;

            var baseDir = Path.GetDirectoryName(_filePath);
            if (baseDir == null)
                return relativePath;

            return Path.GetFullPath(Path.Combine(baseDir, relativePath));
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
    }
}
