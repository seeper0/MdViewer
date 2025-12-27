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
| Alt+F4 | 창 닫기 (기본 동작) |

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
- 링크 클릭 이벤트 핸들링 (HTTP, 이미지, Office, MD)
- Excel 시트 이동 기능 (Dynamic COM)
- 단축키 바인딩
- Drag & Drop 지원 (.md 파일)

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
