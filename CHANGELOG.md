# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.2] - 2025-12-28

### Added
- **About 대화상자 추가**
  - F1 단축키로 About 실행
  - 시스템 메뉴(Alt+Space)에 "MdViewer 정보(&A)" 추가
  - 버전 정보 및 클릭 가능한 Home URL 표시
  - ESC 키로 About 창만 닫기

- **인스톨러 개선**
  - README.md를 설치 폴더에 포함
  - 시작 메뉴에 바로가기 추가 (README, MdViewer 페이지, 제거)
  - 설치 완료 화면에 "MdViewer 페이지 방문" 옵션 추가
  - 바탕화면 바로가기 옵션 제거

## [0.2.1] - 2025-12-27

### Fixed
- **파일 연결 경로 검증 개선**
  - .md 파일 확장자 연결 시 실행 파일 경로가 일치하는지 확인
  - 다른 위치에서 실행 시 자동으로 재등록
  - 파일 연결 레지스트리 경로 불일치 문제 해결

- **인스톨러 DLL 포함**
  - 설치 파일에 필수 DLL 누락 문제 해결
  - `MdXaml.dll`, `MdXaml.Plugins.dll`, `ICSharpCode.AvalonEdit.dll` 포함
  - 설치 후 런타임 오류 해결

### Changed
- 프로젝트 파일에 버전 정보 추가 (AssemblyVersion, FileVersion)

## [0.2.0] - 2025-12-27

### Added
- **Office 파일 링크 지원**
  - Excel 파일 (.xls, .xlsx) 링크 클릭 시 자동 실행
  - Word 파일 (.doc, .docx) 링크 지원
  - PowerPoint 파일 (.ppt, .pptx) 링크 지원

- **Excel 시트 및 셀 직접 이동 기능**
  - 표준 형식: `파일.xlsx#시트이름!셀주소`
  - 예시: `data.xlsx#Sheet2!A1` → Excel을 열고 Sheet2의 A1 셀로 이동
  - Dynamic COM 바인딩 사용 (Interop DLL 의존성 없음)

- **이미지 파일 링크 지원**
  - jpg, png, gif, bmp, webp, svg, ico 파일을 기본 이미지 뷰어로 열기
  - 마크다운 링크 문법으로 이미지 파일 열기 가능

- **Drag & Drop 기능**
  - .md 파일을 창에 드래그하여 열기
  - 드래그 중 파일이 .md인 경우에만 복사 커서 표시

- **Ctrl+E 단축키**
  - 현재 마크다운 파일이 있는 폴더를 탐색기로 열기

- **Inno Setup 인스톨러**
  - Windows용 설치 프로그램 제공
  - .NET 8 Runtime 자동 체크 및 다운로드 안내
  - 시작 메뉴 바로가기 자동 생성
  - 한국어/영어 지원

### Fixed
- **이미지 표시 문제 해결**
  - MdXaml의 `AssetPathRoot` 속성을 사용하여 상대 경로 이미지 로드
  - 마크다운 파일과 같은 폴더의 이미지 정상 표시

### Technical
- Dynamic COM binding으로 Excel 제어 (Interop DLL 불필요)
- Framework-dependent 배포 방식 채택 (약 2MB)
- Named Pipe를 통한 단일 인스턴스 관리

## Initial Features

### Core Features
- 마크다운 파일(.md) 렌더링 및 뷰어
- 파일 확장자 자동 연결 (첫 실행 시)
- 멀티 윈도우 지원
- 중복 창 방지 (같은 파일은 하나의 창에서만)
- 내부 링크 탐색 (다른 .md 파일로 이동)
- 외부 링크 지원 (웹 링크는 기본 브라우저로)
- 창 상태 저장 (크기, 위치)

### UI & Typography
- 본문: 18px, Malgun Gothic/Segoe UI
- 제목: H1-H6 점진적 크기 조정 (28px - 18px)
- 코드 블록: D2Coding, Consolas 등 고정폭 폰트, 13px
- 구문 강조 및 반응형 레이아웃

### Shortcuts
- **Esc**: 창 닫기
- **F5**: 현재 파일 새로고침
- **Ctrl+E**: 현재 파일이 있는 폴더 열기
- **Alt+F4**: 창 닫기 (기본)

### Tech Stack
- .NET 8.0 WPF
- MdXaml 1.27.0
- System.Text.Json 8.0.5

---

**마크다운을 사랑하는 분들을 위해 ❤️로 제작**
