# MdViewer

깔끔하고 방해 없는 인터페이스로 마크다운 파일을 보는 가벼운 데스크톱 애플리케이션입니다.

## 개요

**MdViewer**는 WPF와 .NET 8.0으로 제작된 Windows 데스크톱 애플리케이션으로 마크다운(.md) 파일 전용 뷰어를 제공합니다. 첫 실행 시 `.md` 파일 확장자를 자동으로 연결하여 Windows 탐색기에서 마크다운 파일을 더블클릭하여 바로 볼 수 있습니다.

## 기능

### 핵심 기능
- **마크다운 렌더링**: 아름다운 서식으로 표준 마크다운 문법을 완벽하게 지원
- **파일 연결**: 첫 실행 시 `.md` 파일 확장자 자동 등록
- **멀티 윈도우 지원**: 여러 마크다운 파일을 동시에 열기
- **스마트 창 관리**: 같은 파일을 두 번 열면 중복 생성 대신 기존 창에 포커스
- **내부 링크 탐색**: 다른 `.md` 파일 링크를 클릭하여 새 창에서 열기
- **외부 링크 지원**: 웹 링크(http/https)는 기본 브라우저에서 열기
- **이미지 링크**: 이미지 파일(jpg, png 등)을 기본 이미지 뷰어로 열기
- **Office 파일 링크**: Excel, Word, PowerPoint 파일 열기 지원
  - Excel 시트 이동: `파일.xlsx#시트이름!셀주소` 형식으로 특정 시트/셀로 바로 이동

### UI 및 타이포그래피
- **맞춤형 스타일**:
  - 본문: 18px, Malgun Gothic/Segoe UI 폰트
  - 제목: H1-H6 점진적 크기 조정(28px - 18px)
  - 코드 블록: 고정폭 폰트(D2Coding, Consolas 등) 13px
- **구문 강조**: 코드 블록을 독특한 서식으로 표시
- **반응형 레이아웃**: 창 크기와 위치를 세션 간에 기억
- **Drag & Drop**: .md 파일을 창에 드래그하여 열기

### 단축키
| 단축키 | 동작 |
|--------|------|
| **Esc** | 창 닫기 |
| **F5** | 현재 파일 새로고침 |
| **Alt+F4** | 창 닫기 (기본) |

## 기술 스택

- **프레임워크**: WPF (.NET 8.0 Windows)
- **마크다운 엔진**: [MdXaml](https://github.com/whistyun/MdXaml) 1.27.0
- **언어**: C#
- **빌드 시스템**: .NET CLI / Visual Studio
- **구성**: JSON (`%AppData%/MdViewer/settings.json`에 저장)

## 설치

### 필수 요구사항
- Windows 7 이상
- .NET 8.0 Runtime (미리 빌드된 실행 파일 실행 시)
- Visual Studio 2022 또는 .NET 8.0 SDK (소스에서 빌드 시)

### 릴리즈에서 설치
최신 릴리즈 실행 파일을 다운로드하고 실행하세요. 첫 실행 시 파일 연결이 등록됩니다.

### 소스에서 빌드
```bash
# 저장소 복제
git clone https://github.com/seeper0/MdViewer.git
cd MdViewer

# 프로젝트 빌드
dotnet build -c Release

# 애플리케이션 실행
dotnet run

# 또는 독립 실행 파일 게시
dotnet publish -c Release -o ./publish
```

## 사용법

### 파일 열기
- **탐색기에서**: `.md` 파일을 더블클릭 (첫 실행 후)
- **명령줄에서**: `MdViewer.exe "경로/파일.md"`
- **앱 내에서**: 다른 `.md` 파일 링크 클릭
- **Drag & Drop**: .md 파일을 창에 드래그

### 링크 기능
- **마크다운 링크**: 다른 .md 파일로 이동
- **웹 링크**: http/https 링크는 기본 브라우저에서 열기
- **이미지 링크**: .jpg, .png, .gif, .bmp, .webp, .svg, .ico 파일을 기본 이미지 뷰어로 열기
- **Excel 링크**:
  - 기본: `data.xlsx` → Excel로 파일 열기
  - 시트 이동: `data.xlsx#Sheet2!A1` → Excel을 열고 Sheet2의 A1 셀로 이동
- **Word/PowerPoint 링크**: .doc, .docx, .ppt, .pptx 파일 열기

### 창 상태
애플리케이션이 자동으로 저장하고 복원하는 항목:
- 창 너비와 높이
- 화면의 창 위치
- 이 설정은 `%AppData%/MdViewer/settings.json`에 세션 간 유지됩니다

### 파일 관리
- 각 `.md` 파일은 단일 창에서 열림(중복 방지)
- 같은 파일을 두 번 열면 기존 창에 포커스
- 다른 파일은 각각 새 창에서 열림
- 프로세스 간 통신은 Named Pipes 사용(MdViewer_Pipe)

## 프로젝트 구조

```
MdViewer/
├── App.xaml              # 애플리케이션 루트(리소스, 스타일)
├── App.xaml.cs           # 앱 시작, 단일 인스턴스 관리, 파이프 서버
├── MainWindow.xaml       # 메인 뷰어 창 UI
├── MainWindow.xaml.cs    # 창 로직, 파일 로딩, 렌더링, 키보드 단축키
├── AssemblyInfo.cs       # 어셈블리 메타데이터
├── MdViewer.csproj       # 프로젝트 구성
├── MdViewer.sln          # 솔루션 파일
│
├── Models/
│   └── AppSettings.cs    # 창 상태 모델(Width, Height, Left, Top)
│
├── Services/
│   ├── FileAssociationService.cs  # 레지스트리 기반 .md 파일 연결
│   ├── PipeService.cs             # Named Pipe 클라이언트/서버 통신
│   ├── SettingsService.cs         # 설정 저장(JSON)
│   └── WindowManager.cs           # 멀티 윈도우 추적 및 조정
│
└── docs/
    ├── MdViewer.md       # 완전한 명세서 문서
    ├── TaskPlan.md       # 구현 작업 체크리스트
    ├── HeadingTest.md    # 테스트 마크다운 파일
    ├── LinkTest.md
    ├── ImageLinkTest.md
    ├── TableTest.md
    └── DiagramTest.md
```

## 아키텍처

### 주요 구성 요소

**App.xaml.cs**
- 애플리케이션 진입점
- 명령줄 인수 처리
- Named Pipes를 통한 단일 인스턴스 동작 관리
- 첫 실행 시 파일 연결 등록
- 첫 번째 창 생성 및 표시

**MainWindow.xaml.cs**
- MdXaml의 MarkdownScrollViewer를 사용하여 마크다운 콘텐츠 렌더링
- `.md` 파일 로드 및 맞춤 스타일 적용
- 키보드 단축키 처리(Esc, F5)
- 링크 클릭 처리(내부 `.md`, 외부 웹 링크, 이미지, Office 파일)
- Excel 시트 이동 기능(Dynamic COM)
- SettingsService를 통한 창 상태 저장/복원
- Drag & Drop 지원

**WindowManager.cs**
- 열린 파일과 해당 창의 레지스트리 유지
- 동일 파일에 대한 중복 창 방지
- 새 파일 요청을 기존 창으로 라우팅하거나 새 창 생성

**PipeService.cs**
- Named Pipe 서버: 새 인스턴스의 파일 열기 요청 수신
- Named Pipe 클라이언트: 기존 인스턴스와 통신
- 프로세스 간 조정을 통한 단일 인스턴스 애플리케이션 구현

**FileAssociationService.cs**
- 첫 실행 시 Windows 레지스트리에 `.md` 확장자 등록
- HKEY_CURRENT_USER 사용(관리자 권한 불필요)
- 레지스트리 경로: `Software\Classes\.md`

**SettingsService.cs**
- 창 상태를 JSON 파일로 저장
- 위치: `%AppData%/MdViewer/settings.json`
- 애플리케이션 시작 시 설정 로드 및 적용

## 구성

### 기본 설정
```json
{
  "WindowWidth": 900.0,
  "WindowHeight": 600.0,
  "WindowLeft": 100.0,
  "WindowTop": 100.0
}
```

설정은 창을 닫을 때 자동으로 저장되고 다음 시작 시 복원됩니다.

## 레지스트리 통합

첫 실행 시 다음 레지스트리 항목이 생성됩니다:
```
HKEY_CURRENT_USER\Software\Classes\.md
  (Default) = "MdViewer.md"

HKEY_CURRENT_USER\Software\Classes\MdViewer.md
  (Default) = "Markdown File"

HKEY_CURRENT_USER\Software\Classes\MdViewer.md\shell\open\command
  (Default) = "C:\path\to\MdViewer.exe" "%1"
```

HKEY_CURRENT_USER에 변경사항이 적용되므로 관리자 권한이 필요하지 않습니다.

## 오류 처리

- **파일 없음**: 오류 메시지 표시 후 창 닫기
- **파일 읽기 오류**: 세부 정보와 함께 오류 메시지 표시
- **레지스트리 오류**: 경고 표시하지만 정상적으로 계속 실행

## 성능

- **경량**: 최소한의 메모리 사용
- **빠른 시작**: 즉각적인 창 열림
- **부드러운 렌더링**: MdXaml이 효율적인 마크다운 렌더링 제공
- **스마트 폰트 선택**: 사용 가능한 시스템 폰트 자동 사용(폴백 체인)

## 향후 개선 아이디어

- 다크 모드 / 테마 전환
- 폰트 크기 조정(Ctrl+마우스 휠)
- 탐색용 파일 탐색기 사이드바
- 인쇄 지원
- 북마크/히스토리 관리
- 검색/찾기 기능
- 오른쪽에서 왼쪽 텍스트 지원

## 빌드 및 개발

### 요구사항
- Visual Studio 2022 (또는 C# 확장이 있는 VS Code)
- .NET 8.0 SDK
- Windows 개발 도구

### 빌드 명령
```bash
# 디버그 빌드
dotnet build

# 최적화된 릴리즈 빌드
dotnet build -c Release

# 테스트 실행(추가 시)
dotnet test

# 독립 실행 파일 게시
dotnet publish -c Release --self-contained -r win-x64 -o ./publish
```

### NuGet 종속성
```xml
<PackageReference Include="MdXaml" Version="1.27.0" />
<PackageReference Include="MdXaml.Plugins" Version="1.27.0" />
<PackageReference Include="System.Text.Json" Version="8.0.5" />
```

## 기여

이슈와 풀 리퀘스트를 자유롭게 제출하세요. 주요 변경 사항의 경우 먼저 제안된 변경 사항을 논의할 이슈를 열어주세요.

## 라이선스

이 프로젝트는 MIT 라이선스에 따라 라이선스가 부여됩니다. 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

Copyright (c) 2025 seeper0

## 감사의 말

- [MdXaml](https://github.com/whistyun/MdXaml) - 훌륭한 WPF 마크다운 렌더링 라이브러리
- Windows Presentation Foundation(WPF) 팀의 강력한 UI 프레임워크
- .NET 8.0을 제공한 .NET 커뮤니티

---

**마크다운을 사랑하는 분들을 위해 ❤️로 제작**
