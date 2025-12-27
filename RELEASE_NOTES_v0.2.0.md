# MdViewer v0.2.0

## 🎉 주요 기능

- ✅ 마크다운 파일(.md) 렌더링 및 뷰어
- ✅ 파일 확장자 자동 연결
- ✅ 멀티 윈도우 지원
- ✅ 링크 처리 (마크다운, 웹, 이미지)
- ✅ **Office 파일 링크 지원** (Excel, Word, PowerPoint)
- ✅ **Excel 시트 이동 기능** (`파일.xlsx#시트이름!셀주소`)
- ✅ Drag & Drop 지원
- ✅ **단축키 지원** (Esc, F5, Ctrl+E)
- ✅ 창 크기/위치 저장

## 🆕 새로운 기능 (v0.2.0)

- 📊 **Excel, Word, PowerPoint 파일 링크 지원**
  - .xls, .xlsx, .doc, .docx, .ppt, .pptx 파일을 마크다운 링크로 열기
- 🎯 **Excel 시트 및 셀 직접 이동 기능**
  - 표준 형식: `파일.xlsx#시트이름!셀주소`
  - 예시: `data.xlsx#Sheet2!A1` → Excel을 열고 Sheet2의 A1 셀로 이동
  - Dynamic COM 바인딩 사용 (Interop DLL 의존성 없음)
- 🖼️ **이미지 파일 링크 지원**
  - jpg, png, gif, bmp, webp, svg, ico 파일을 기본 이미지 뷰어로 열기
- 📁 **Drag & Drop으로 md 파일 열기**
- ⌨️ **Ctrl+E 단축키**
  - 현재 마크다운 파일이 있는 폴더를 탐색기로 열기

## 📥 다운로드

### 1️⃣ MdViewer.exe (포터블)
- **파일 크기:** 약 1MB
- **.NET 8 Runtime 필요**
- 설치 없이 바로 실행 가능

**필수 요구사항:**
- [.NET 8 Desktop Runtime 다운로드](https://dotnet.microsoft.com/download/dotnet/8.0)

### 2️⃣ MdViewer-v0.2.0-Setup.exe (인스톨러) - 추천
- **파일 크기:** 약 2.2MB
- **자동 설치:** `C:\Program Files\MdViewer`에 설치
- **.NET 8 Runtime 필요** (설치 시 자동 안내)
- 시작 메뉴 바로가기 자동 생성
- 업그레이드 시 자동으로 기존 버전 덮어쓰기

## 🚀 설치 방법

### 방법 1: 인스톨러 사용 (추천)
1. `MdViewer-v0.2.0-Setup.exe` 다운로드
2. 실행하여 설치
3. .NET 8 Runtime이 없으면 다운로드 페이지로 안내
4. 설치 완료 후 시작 메뉴에서 실행

**업그레이드:** 새 버전의 Setup.exe를 실행하면 자동으로 기존 버전을 덮어씁니다.

### 방법 2: 포터블 실행
1. [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0/runtime) 설치 (처음 한 번만)
2. `MdViewer.exe` 다운로드
3. 원하는 폴더에 저장 후 실행 (첫 실행 시 .md 파일 연결 자동 등록)

## 💻 시스템 요구사항

- **Windows 7 이상**
- **.NET 8 Desktop Runtime**

## 📝 사용 예시

```markdown
# 마크다운에서 Office 파일 링크 사용하기

## Excel 링크
[월간 보고서](reports/monthly.xlsx#'월별 데이터'!A1)
[데이터 분석](data.xlsx#Sheet2!B5)

## Word 문서
[프로젝트 계획서](documents/plan.docx)

## PowerPoint
[발표 자료](presentation.pptx)

## 이미지
![스크린샷](images/screenshot.png)
```

## ⌨️ 단축키

| 단축키 | 동작 |
|--------|------|
| **Esc** | 창 닫기 |
| **F5** | 현재 파일 새로고침 |
| **Ctrl+E** | 현재 파일이 있는 폴더 열기 |
| **Alt+F4** | 창 닫기 (기본) |

## 🐛 알려진 제한사항

- Excel 시트 이동 기능은 Excel이 설치되어 있어야 합니다
- 시트 이름에 특수문자/공백이 있으면 작은따옴표로 감싸야 합니다 (`'시트 이름'`)

## 📖 자세한 정보

자세한 내용은 [README](https://github.com/seeper0/MdViewer/blob/main/README.md)를 참조하세요.

---

**마크다운을 사랑하는 분들을 위해 ❤️로 제작**
