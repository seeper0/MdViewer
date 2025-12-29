using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using MdViewer.Models;
using MdViewer.Services;

namespace MdViewer
{
    public partial class MainWindow : Window
    {
        // Win32 API for system menu
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        private const int MF_SEPARATOR = 0x800;
        private const int MF_STRING = 0x0;
        private const int WM_SYSCOMMAND = 0x112;
        private const int ABOUT_MENU_ID = 1000;

        private string? _filePath;

        public MainWindow()
        {
            InitializeComponent();
            LoadWindowSettings();
            Closing += MainWindow_Closing;
            SourceInitialized += MainWindow_SourceInitialized;
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

                // MdXaml이 상대 경로 이미지를 찾을 수 있도록 AssetPathRoot 설정
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    MarkdownViewer.AssetPathRoot = directory;
                }

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

            // # 기호로 파일 경로와 시트 정보 분리 (Excel 표준 형식)
            string filePath = uri;
            string? sheetInfo = null;
            if (uri.Contains('#'))
            {
                var parts = uri.Split('#', 2);
                filePath = parts[0];
                sheetInfo = parts.Length > 1 ? parts[1] : null;
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            // 이미지 링크 - 파일 존재 여부 상관없이 Windows에 넘김
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico" };

            if (imageExtensions.Contains(extension))
            {
                var imagePath = ResolvePathWithoutExistenceCheck(filePath);
                if (imagePath != null)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이미지를 열 수 없습니다:\n경로: {imagePath}\n오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"이미지 경로를 찾을 수 없습니다:\n원본 URI: {uri}\n파일 경로: {filePath}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }

            // Excel 파일 링크 - 시트 정보가 있으면 COM Interop 사용
            string[] excelExtensions = { ".xls", ".xlsx" };

            if (excelExtensions.Contains(extension))
            {
                var excelPath = ResolvePathWithoutExistenceCheck(filePath);
                if (excelPath != null)
                {
                    if (sheetInfo != null)
                    {
                        // COM Interop으로 특정 시트 열기
                        OpenExcelWithSheet(excelPath, sheetInfo);
                    }
                    else
                    {
                        // 시트 정보 없으면 기존 방식
                        try
                        {
                            Process.Start(new ProcessStartInfo(excelPath) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Excel 파일을 열 수 없습니다:\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                return;
            }

            // 기타 Office 파일 링크 (Word, PowerPoint)
            string[] otherOfficeExtensions = { ".doc", ".docx", ".ppt", ".pptx" };

            if (otherOfficeExtensions.Contains(extension))
            {
                var officePath = ResolvePathWithoutExistenceCheck(filePath);
                if (officePath != null)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(officePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Office 파일을 열 수 없습니다:\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return;
            }

            // .md 파일 링크
            if (uri.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                var targetPath = ResolveRelativePath(uri);
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
            // Ctrl+E: 폴더 열기
            if (e.Key == Key.E && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                OpenFolder();
                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.F5:
                    ReloadFile();
                    break;
                case Key.F1:
                    ShowAbout();
                    e.Handled = true;
                    break;
            }
        }

        private void OpenFolder()
        {
            if (string.IsNullOrEmpty(_filePath))
                return;

            var directory = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                MessageBox.Show("폴더를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폴더를 열 수 없습니다:\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void OpenExcelWithSheet(string excelPath, string sheetInfo)
        {
            dynamic? excelApp = null;
            dynamic? workbook = null;

            try
            {
                // 시트 정보 파싱: '시트이름!셀주소' 형식
                string sheetName = sheetInfo;
                string? cellAddress = null;

                if (sheetInfo.Contains('!'))
                {
                    var parts = sheetInfo.Split('!', 2);
                    sheetName = parts[0];
                    cellAddress = parts.Length > 1 ? parts[1] : null;
                }

                // 시트 이름에서 작은따옴표 제거 (있을 경우)
                sheetName = sheetName.Trim('\'');

                // Excel 애플리케이션 시작 (dynamic COM 바인딩)
                Type? excelType = Type.GetTypeFromProgID("Excel.Application");
                if (excelType == null)
                {
                    MessageBox.Show("Excel이 설치되어 있지 않습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                excelApp = Activator.CreateInstance(excelType);
                if (excelApp == null)
                {
                    MessageBox.Show("Excel 애플리케이션을 생성할 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                excelApp.Visible = true;

                // 워크북 열기
                workbook = excelApp.Workbooks.Open(excelPath);

                // 시트 활성화
                try
                {
                    dynamic worksheet = workbook.Sheets[sheetName];
                    worksheet.Activate();

                    // 셀 주소가 지정되어 있으면 해당 셀로 이동
                    if (!string.IsNullOrEmpty(cellAddress))
                    {
                        try
                        {
                            dynamic range = worksheet.Range[cellAddress];
                            range.Select();
                        }
                        catch
                        {
                            // 셀 주소가 잘못되어도 시트는 열림
                        }
                    }
                }
                catch
                {
                    // 시트 이름이 잘못되어도 파일은 열림
                    MessageBox.Show($"시트를 찾을 수 없습니다: {sheetName}\n파일은 열렸습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel 파일을 열 수 없습니다:\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);

                // 오류 발생 시 COM 객체 정리
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                }
            }
            // 성공 시에는 COM 객체를 유지 (Excel이 계속 실행되어야 함)
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            IntPtr sysMenu = GetSystemMenu(hwnd, false);

            // 구분선 추가
            AppendMenu(sysMenu, MF_SEPARATOR, 0, string.Empty);
            // About 메뉴 추가 (닫기 바로 위)
            AppendMenu(sysMenu, MF_STRING, ABOUT_MENU_ID, "MdViewer 정보(&A)");

            // 메시지 후킹
            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND && wParam.ToInt32() == ABOUT_MENU_ID)
            {
                ShowAbout();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void ShowAbout()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            var aboutWindow = new Window
            {
                Title = "MdViewer 정보",
                Width = 400,
                Height = 380,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow
            };

            // ESC로 창 닫기
            aboutWindow.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    aboutWindow.Close();
                    e.Handled = true;
                }
            };

            var stackPanel = new System.Windows.Controls.StackPanel
            {
                Margin = new Thickness(20)
            };

            // 버전 정보
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = $"MdViewer v{version?.ToString(3)}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // 설명
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "마크다운 파일 뷰어",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // GitHub 링크 (클릭 가능)
            var linkText = new System.Windows.Controls.TextBlock
            {
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 20)
            };
            linkText.Inlines.Add(new System.Windows.Documents.Run("Home: "));
            var hyperlink = new System.Windows.Documents.Hyperlink(new System.Windows.Documents.Run("https://github.com/seeper0/MdViewer"))
            {
                NavigateUri = new Uri("https://github.com/seeper0/MdViewer")
            };
            hyperlink.RequestNavigate += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = e.Uri.AbsoluteUri,
                        UseShellExecute = true
                    });
                }
                catch { }
            };
            linkText.Inlines.Add(hyperlink);
            stackPanel.Children.Add(linkText);

            // HotKey List
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "HotKey List",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });

            var hotkeyList = new System.Windows.Controls.TextBlock
            {
                FontSize = 12,
                Margin = new Thickness(10, 0, 0, 20)
            };
            hotkeyList.Inlines.Add(new System.Windows.Documents.Run("F1: 정보 보기\n"));
            hotkeyList.Inlines.Add(new System.Windows.Documents.Run("F5: 새로고침\n"));
            hotkeyList.Inlines.Add(new System.Windows.Documents.Run("Ctrl+E: 폴더 열기\n"));
            hotkeyList.Inlines.Add(new System.Windows.Documents.Run("ESC: 닫기"));
            stackPanel.Children.Add(hotkeyList);

            // Copyright
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Copyright © 2025 seeper0",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // 확인 버튼
            var okButton = new System.Windows.Controls.Button
            {
                Content = "확인",
                Width = 80,
                Height = 30,
                IsDefault = true
            };
            okButton.Click += (s, e) => aboutWindow.Close();

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            buttonPanel.Children.Add(okButton);
            stackPanel.Children.Add(buttonPanel);

            aboutWindow.Content = stackPanel;
            aboutWindow.ShowDialog();
        }
    }
}
