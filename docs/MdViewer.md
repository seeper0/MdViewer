# MdViewer 프로젝트 명세서

## 개요
마크다운 파일(.md)을 렌더링하여 보여주는 WPF 데스크톱 뷰어 애플리케이션

## 기술 스택
- **프레임워크**: WPF (.NET 8.0)
- **마크다운 렌더링**: MdXaml (NuGet)
- **언어**: C#

## 기능 요구사항

### 1. 마크다운 렌더링
- MdXaml 라이브러리를 사용하여 .md 파일 렌더링
- 코드 블록에 고정폭 폰트 적용 (Consolas 또는 D2Coding)
- 테이블, 리스트, 코드 블록 등 기본 마크다운 문법 지원

### 2. 파일 연결 (File Association)
- 앱 첫 실행 시 .md 확장자를 MdViewer에 자동 등록
- 레지스트리를 통한 파일 연결 구현
- 관리자 권한 필요 시 UAC 요청

### 3. 창 관리
- 파일별로 별도 창 생성
- 동일 파일 열기 시도 시 새 창 생성하지 않고 기존 창 포커스
- 열린 파일 경로를 추적하는 메커니즘 필요 (Named Pipe 또는 Mutex 활용)

### 4. 링크 처리
#### 내부 링크 (md 파일)
- 뷰어 내에서 .md 링크 클릭 시 새 창으로 열기
- 이미 열린 파일이면 해당 창 포커스

#### 외부 링크 (http/https)
- 시스템 기본 브라우저로 열기
- `Process.Start(new ProcessStartInfo(url) { UseShellExecute = true })`

#### 이미지 링크
- 지원 확장자: `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.webp`, `.svg`, `.ico`
- 클릭 시 시스템 기본 이미지 뷰어로 열기
- 상대 경로 지원

#### Office 파일 링크
**지원 파일:**
- Excel: `.xls`, `.xlsx`
- Word: `.doc`, `.docx`
- PowerPoint: `.ppt`, `.pptx`

**Excel 시트 이동 기능:**
- 표준 형식: `파일.xlsx#시트이름!셀주소`
  - 예: `data.xlsx#Sheet2!A1`
  - 예: `report.xlsx#'월간 보고서'!B5`
- `#` 기호가 있으면 Dynamic COM 바인딩으로 특정 시트/셀 열기
- `#` 기호가 없으면 기본 방식으로 파일만 열기
- 시트 이름에 공백/특수문자가 있으면 작은따옴표로 감싸기
- Excel이 설치되어 있어야 함 (Interop DLL 의존성 없음)

**기타 Office 파일:**
- Word, PowerPoint는 기본 방식으로 열기
- 시스템에 설치된 Office 프로그램 자동 실행

### 5. 창 상태 저장
- 창 크기(Width, Height) 저장
- 창 위치(Left, Top) 저장
- 저장 위치: `%AppData%/MdViewer/settings.json`
- 앱 종료 시 저장, 시작 시 복원

### 6. 단축키
| 키 | 동작 |
|----|------|
| Esc | 창 닫기 |
| F5 | 현재 파일 새로고침 |
| Ctrl+E | 현재 파일이 있는 폴더 열기 |
| Alt+F4 | 창 닫기 (기본 동작) |

### 7. Drag & Drop
- .md 파일을 창에 드래그하여 열기
- 드래그 중 파일이 .md인 경우에만 복사 커서 표시
- 드롭 시 해당 파일 로드

## 아키텍처

### 프로젝트 구조
```
MdViewer/
├── App.xaml
├── App.xaml.cs              # 앱 진입점, 단일 인스턴스 관리
├── MainWindow.xaml
├── MainWindow.xaml.cs       # 메인 뷰어 창
├── Services/
│   ├── FileAssociationService.cs   # 파일 연결 등록
│   ├── PipeService.cs              # Named Pipe 통신
│   ├── WindowManager.cs            # 창 관리 (중복 방지)
│   └── SettingsService.cs          # 설정 저장/로드
├── Models/
│   └── AppSettings.cs              # 설정 모델
├── docs/                           # 문서
└── MdViewer.csproj
```

### 핵심 클래스 설명

#### App.xaml.cs
- 앱 시작 시 명령줄 인자로 파일 경로 받기
- Named Pipe 서버 실행하여 다른 인스턴스로부터 파일 열기 요청 수신
- 이미 열린 파일인지 확인 후 새 창 생성 또는 기존 창 포커스

#### WindowManager.cs
- 열린 파일 경로와 Window 인스턴스 매핑 (Dictionary<string, MainWindow>)
- 파일 열기 요청 시 중복 체크
- 창 닫힐 때 매핑에서 제거

#### MainWindow.xaml.cs
- MdXaml MarkdownScrollViewer 컨트롤 사용
- 파일 로드 및 렌더링
- **MdXaml 이미지 경로 설정**: `MarkdownViewer.AssetPathRoot`를 마크다운 파일 디렉터리로 설정하여 상대 경로 이미지 로드
- 링크 클릭 이벤트 핸들링 (HTTP, 이미지, Office, MD)
- Excel 시트 이동 기능 (Dynamic COM)
- 단축키 바인딩 (Esc, F5, Ctrl+E)
- Drag & Drop 지원 (.md 파일)
- Ctrl+E로 현재 파일 폴더 열기 (`OpenFolder()` 메서드)

#### FileAssociationService.cs
- 레지스트리에 .md 확장자 등록
- HKEY_CURRENT_USER\Software\Classes 사용 (관리자 권한 불필요)

#### SettingsService.cs
- JSON 형식으로 설정 저장/로드
- 창 크기, 위치 정보 관리

## 구현 세부사항

### NuGet 패키지
```xml
<PackageReference Include="MdXaml" Version="1.27.0" />
<PackageReference Include="MdXaml.Plugins" Version="1.27.0" />
<PackageReference Include="System.Text.Json" Version="8.0.5" />
```

### Named Pipe 통신
- Pipe 이름: `MdViewer_Pipe`
- 새 인스턴스 시작 시:
  1. 기존 Pipe 서버에 연결 시도
  2. 연결 성공 → 파일 경로 전송 후 종료
  3. 연결 실패 → 새 Pipe 서버 시작, 정상 실행

### 파일 연결 레지스트리
```
HKEY_CURRENT_USER\Software\Classes\.md
  (Default) = "MdViewer.md"

HKEY_CURRENT_USER\Software\Classes\MdViewer.md
  (Default) = "Markdown File"

HKEY_CURRENT_USER\Software\Classes\MdViewer.md\shell\open\command
  (Default) = "\"C:\path\to\MdViewer.exe\" \"%1\""
```

### MdXaml 스타일 커스터마이징

본문 스타일:
- 폰트: Malgun Gothic, Segoe UI
- 크기: 18px

헤딩 크기:
| 레벨 | 크기 |
|------|------|
| H1 | 28px |
| H2 | 26px |
| H3 | 24px |
| H4 | 22px |
| H5 | 20px |
| H6 | 18px (회색) |

코드 블록 스타일:
- 폰트: D2Coding, GulimChe, DotumChe, Consolas, Courier New
- 크기: 13px
- 배경: #F5F5F5

구현 방식: XAML Tag 트리거 + Document.Blocks 후처리

### 링크 클릭 이벤트
마크다운 렌더링 후 FlowDocument 내 Hyperlink를 직접 찾아서 Click 이벤트 연결:
- MdXaml의 `ClickAction` 사용하지 않음 (자체 처리 방지)
- `CommandParameter`에서 URL 추출

**링크 처리 순서:**
1. `http://`, `https://` → 기본 브라우저로 열기
2. `#` 기호로 파일 경로와 시트 정보 분리 (Excel 표준 형식)
3. 이미지 확장자 (`.jpg`, `.png` 등) → 기본 이미지 뷰어로 열기
4. Excel 확장자 (`.xls`, `.xlsx`)
   - `#` 있음 → `OpenExcelWithSheet()` 메서드로 특정 시트 열기
   - `#` 없음 → 기본 방식으로 파일 열기
5. 기타 Office 파일 (`.doc`, `.docx`, `.ppt`, `.pptx`) → 기본 방식으로 열기
6. `.md` 확장자 → WindowManager를 통해 새 창 열기

**OpenExcelWithSheet() 메서드:**
- Dynamic COM 바인딩 사용 (`Type.GetTypeFromProgID("Excel.Application")`)
- Excel 애플리케이션 생성 및 Visible = true 설정
- 워크북 열기 → 시트 활성화 → 셀 선택
- COM 객체는 성공 시 유지 (Excel이 계속 실행)
- 실패 시 COM 객체 정리 (`Marshal.ReleaseComObject`)

### 창 생성 및 파일 로드 순서
1. `new MainWindow(filePath)` → 창 생성만
2. `window.Show()` → 창 표시
3. `Loaded` 이벤트 → `LoadFile()` 호출
4. 파일 없으면 → MessageBox (새 창이 owner) → `Close()`

## 예외 처리
- 파일 없음: 오류 메시지 표시 후 창 닫기
- 파일 읽기 실패: 오류 메시지 표시
- 파일 연결 등록 실패: 경고 표시 후 계속 실행

## 향후 확장 가능
- 다크 모드
- 폰트 크기 조절
- 파일 탐색기 사이드바
- 인쇄 기능
- 북마크

## 개발 교훈

### 1. MdXaml 이미지 경로 처리
**문제**: 상대 경로 이미지(`![](test.png)`)가 "resource not found" 오류로 표시되지 않음

**시도한 방법들**:
```csharp
// ❌ 작동하지 않음 - 작업 디렉터리 변경
Directory.SetCurrentDirectory(directory);

// ❌ 작동하지 않음 - DocumentPath 속성 없음
MarkdownViewer.DocumentPath = filePath;
```

**올바른 해결책**:
```csharp
// ✅ AssetPathRoot 속성 사용
var directory = Path.GetDirectoryName(_filePath);
if (!string.IsNullOrEmpty(directory))
{
    MarkdownViewer.AssetPathRoot = directory;
}
MarkdownViewer.Markdown = content;
```

**핵심**: MdXaml은 `AssetPathRoot` 속성을 기준으로 상대 경로를 해석합니다. 이 속성을 마크다운 파일의 디렉터리로 설정하면 같은 폴더의 이미지를 올바르게 로드할 수 있습니다.

### 2. 마크다운 이미지 vs 링크 구문
개발 중 혼동하기 쉬운 부분:

```markdown
# 이미지 삽입 구문 (표시만 됨)
![설명 텍스트](image.png)
→ 이미지를 문서에 렌더링
→ 클릭 불가 (이미지 표시가 목적)

# 링크 구문 (클릭 가능)
[이미지 열기](image.png)
→ 하이퍼링크로 렌더링
→ 클릭하면 파일을 열거나 페이지 이동
```

**결론**:
- `![]()` = 이미지 삽입 (display)
- `[]()` = 하이퍼링크 (clickable)

사용자가 "이미지 클릭이 안 된다"고 하면, `![]()` 구문은 클릭이 불가능한 것이 정상입니다.

### 3. Excel COM Interop 의존성 문제
**문제**: NuGet `Microsoft.Office.Interop.Excel` 사용 시 런타임 오류
```
Could not load file or assembly 'office, Version=16.0.0.0'
```

**원인**: Static Interop은 Interop DLL을 런타임에 요구하며, `EmbedInteropTypes=false`로도 해결 안 됨

**해결책**: Dynamic COM binding 사용
```csharp
// ❌ Static Interop (DLL 의존성)
using Excel = Microsoft.Office.Interop.Excel;
Excel.Application excel = new Excel.Application();

// ✅ Dynamic COM (의존성 없음)
Type? excelType = Type.GetTypeFromProgID("Excel.Application");
if (excelType == null)
{
    MessageBox.Show("Excel이 설치되어 있지 않습니다.");
    return;
}
dynamic excelApp = Activator.CreateInstance(excelType);
excelApp.Visible = true;
dynamic workbook = excelApp.Workbooks.Open(excelPath);
```

**장점**:
- Interop DLL 불필요
- 패키지 크기 감소
- 배포 단순화

**단점**:
- 컴파일 타임 타입 체크 없음
- IntelliSense 지원 제한

### 4. 프레임워크 종속 vs 독립 실행형 배포
**비교**:
| 구분 | 프레임워크 종속 | 독립 실행형 |
|------|----------------|------------|
| 크기 | ~1MB | ~156MB |
| .NET Runtime 필요 | ✅ 필요 | ❌ 불필요 |
| 빌드 명령 | `dotnet publish` | `dotnet publish --self-contained` |
| 사용자 편의성 | 낮음 (Runtime 설치) | 높음 (바로 실행) |

**선택 기준**:
- **일반 사용자**: 독립 실행형 (다운로드 후 바로 실행)
- **개발자/전문가**: 프레임워크 종속 (작은 크기)

v0.2.0에서는 두 버전 모두 제공하여 사용자가 선택하도록 했습니다.

### 5. WPF Trimming 지원 제한
**문제**: WPF는 .NET Trimming을 지원하지 않음
```
NETSDK1168: 트리밍을 사용하도록 설정하면 WPF가 지원되지 않거나 권장되지 않습니다
```

**해결**: Trimming 대신 프레임워크 종속 배포로 크기 축소
- Self-contained (156MB) → Framework-dependent (1MB)

### 6. Excel 하이퍼링크 표준 형식
**질문**: Excel 시트 이동을 마크다운에서 어떻게 표현할까?

**조사 결과**: Excel 하이퍼링크 표준 형식
```
파일경로#시트이름!셀주소
예: data.xlsx#Sheet2!A1
예: report.xlsx#'월간 보고서'!B5
```

**구현**:
```csharp
// # 기호로 파일 경로와 시트 정보 분리
if (uri.Contains('#'))
{
    var parts = uri.Split('#', 2);
    filePath = parts[0];
    sheetInfo = parts.Length > 1 ? parts[1] : null;
}

// 시트 정보 파싱: '시트이름!셀주소'
if (sheetInfo.Contains('!'))
{
    var parts = sheetInfo.Split('!', 2);
    sheetName = parts[0].Trim('\'');
    cellAddress = parts[1];
}
```

**주의**: 시트 이름에 공백/특수문자가 있으면 작은따옴표로 감싸야 함
