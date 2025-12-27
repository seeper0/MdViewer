---
name: release
description: MdViewer 프로젝트의 새 버전을 릴리스합니다. 버전 업데이트, 문서 검증, CHANGELOG 작성, Git 태그 생성을 자동화합니다. "릴리스", "버전 올리기", "새 버전 배포" 등의 요청 시 사용됩니다.
---

# MdViewer 릴리스 Skill

이 Skill은 MdViewer 프로젝트의 새 버전을 릴리스하는 전체 프로세스를 자동화합니다.

## 릴리스 프로세스

### 1. 버전 번호 확인

사용자에게 새 버전 번호를 물어봅니다:
- 현재 버전을 표시
- 새 버전 번호 입력 요청 (예: 0.2.2, 0.3.0, 1.0.0)
- Semantic Versioning 규칙 설명

### 2. 버전 정보 업데이트

다음 파일들의 버전을 업데이트합니다:

#### MdViewer.csproj
```xml
<Version>새버전</Version>
<AssemblyVersion>새버전.0</AssemblyVersion>
<FileVersion>새버전.0</FileVersion>
```

#### installer.iss
```iss
#define MyAppVersion "새버전"
```

### 3. CHANGELOG.md 업데이트

사용자에게 변경 사항을 물어보고 CHANGELOG.md 상단에 새 버전 섹션 추가:

```markdown
## [새버전] - YYYY-MM-DD

### Added
- 새로운 기능들...

### Changed
- 변경된 사항들...

### Fixed
- 버그 수정들...

### Removed
- 제거된 기능들...
```

**중요**:
- 날짜는 오늘 날짜 사용 (YYYY-MM-DD 형식)
- 변경 사항은 사용자에게 직접 물어봐서 작성
- Keep a Changelog 형식 준수

### 4. 문서 무결성 검증

다음 항목들을 검증:

#### 버전 일관성
- MdViewer.csproj 버전
- installer.iss 버전
- CHANGELOG.md 버전
- 모두 동일한지 확인

#### README.md 검증
- 빌드 명령어가 실제 사용과 일치하는지
- NuGet 패키지 버전 정보 확인
- 주요 기능 설명이 최신인지

#### CLAUDE.md 검증
- 문서 규칙 준수 확인
- Git 규칙 존재 확인

#### docs/MdViewer.md 검증
- 명세서 내용이 최신인지
- 빌드 방법 확인

#### 프로젝트 구조 검증
```bash
# 필수 파일 존재 확인
- CHANGELOG.md (존재)
- CLAUDE.md (존재)
- README.md (존재)
- MdViewer.csproj (존재)
- installer.iss (존재)
- docs/MdViewer.md (존재)

# 불필요한 파일 확인
- nul 파일 (없어야 함)
- RELEASE_NOTES_*.md (없어야 함)
```

### 5. 변경 사항 보고

사용자에게 다음 정보를 보고:
- 업데이트된 파일 목록
- 버전 변경 내역 (이전 → 새 버전)
- CHANGELOG 추가 내용
- 검증 결과 요약

### 6. Git 커밋 및 푸시

```bash
git add .
git commit -m "release: prepare v새버전

- Update version to 새버전 in all files
- Update CHANGELOG.md with v새버전 changes
- [변경 사항 요약]

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"

git push
```

**중요**: 태그는 빌드 성공 후에 생성합니다.

### 7. 빌드 및 테스트

#### 7.1 프로젝트 빌드
```bash
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

#### 7.2 빌드 검증

빌드된 파일 확인:
```bash
ls ./publish/MdViewer.exe
```

**빌드 실패 시**:
- 문제 수정 후 재빌드
- 태그 생성 안 함
- 사용자에게 오류 보고

#### 7.3 포터블 zip 생성
```bash
cd publish && tar -a -c -f ../MdViewer-v새버전-portable.zip * && cd ..
```

#### 7.4 인스톨러 생성
```bash
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

출력: `installer-output/MdViewer-v새버전-Setup.exe`

#### 7.5 인스톨러 테스트

**필수**: 인스톨러 생성 후 반드시 자동으로 실행하여 테스트합니다.

```bash
# 인스톨러 자동 실행 (필수 - 빼먹지 말 것!)
start installer-output/MdViewer-v새버전-Setup.exe
```

**테스트 체크리스트**:
1. 인스톨러 UI가 정상적으로 표시되는지 확인
2. .NET 8 Runtime 체크가 동작하는지 확인
3. 설치 완료 화면 옵션 확인:
   - "MdViewer 실행" 옵션 (기본 체크)
   - "GitHub 페이지 방문" 옵션 (기본 체크 안 됨)
4. 설치 완료 후 MdViewer 실행 확인
5. .md 파일 연결 확인:
   - 탐색기에서 .md 파일 우클릭 → 연결 프로그램 확인
   - .md 파일 더블클릭으로 MdViewer 실행 확인
6. 설치된 폴더 확인:
   ```bash
   ls "%LocalAppData%\Programs\MdViewer"
   ```

**테스트 실패 시**:
- 인스톨러 스크립트 수정
- 재빌드 (`7.1`부터 다시 실행)
- 태그 생성 안 함

**테스트 완료 후 정리**:
- 제어판에서 "MdViewer" 제거
- 또는 다음 릴리스 시 덮어쓰기 설치

### 8. Git 태그 생성 및 푸시

**빌드 성공 확인 후** Git 태그 생성:

```bash
git tag -a v새버전 -m "MdViewer v새버전

[CHANGELOG 내용을 여기에 복사]

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"

git push --tags
```

### 9. GitHub Release 생성

```bash
gh release create v새버전 \
  --title "MdViewer v새버전" \
  --notes "[CHANGELOG 내용]" \
  MdViewer-v새버전-portable.zip \
  installer-output/MdViewer-v새버전-Setup.exe
```

### 10. 다음 단계 안내

릴리스 완료 후 사용자에게 확인사항 안내:

1. **GitHub Release 확인**
   - https://github.com/seeper0/MdViewer/releases/tag/v새버전
   - 파일 업로드 확인 (portable.zip, Setup.exe)

2. **릴리스 테스트**
   - 포터블 버전 다운로드 및 실행 테스트
   - 인스톨러 설치 및 .md 파일 연결 테스트

3. **릴리스 공지** (선택)
   - README.md 업데이트 (필요시)
   - 사용자 공지 (필요시)

## 오류 처리

### 버전 형식 오류
- Semantic Versioning (X.Y.Z) 형식이 아닌 경우 재입력 요청
- 이전 버전보다 낮은 버전인 경우 경고

### 파일 누락
- 필수 파일이 없는 경우 오류 메시지 표시
- 계속 진행할지 사용자에게 확인

### Git 오류
- 커밋되지 않은 변경사항이 있는 경우 경고
- 푸시 실패 시 원인 안내

### 검증 실패
- 버전 불일치 발견 시 수정 후 재검증
- 문서 오류 발견 시 사용자에게 알림

### Inno Setup 미설치
- ISCC.exe를 찾을 수 없는 경우:
  - 포터블 버전만 먼저 릴리스
  - 인스톨러는 수동으로 나중에 추가하도록 안내

## 예시

### 사용 시나리오 1: 패치 버전 릴리스

**사용자**: "버그 수정했으니 v0.2.2로 릴리스해줘"

**Skill 실행**:
1. 현재 버전 확인 (0.2.1)
2. 새 버전 확인 (0.2.2)
3. 변경 사항 물어보기
4. 파일 업데이트
5. 검증
6. 커밋 & 푸시
7. 빌드 (portable.zip, Setup.exe)
8. GitHub Release 생성
9. 다음 단계 안내

### 사용 시나리오 2: 마이너 버전 릴리스

**사용자**: "새 기능 추가했으니 0.3.0으로 릴리스"

**Skill 실행**:
1. 버전 0.2.1 → 0.3.0 확인
2. Added 섹션에 새 기능 추가 요청
3. 전체 프로세스 실행

## 주의사항

1. **항상 현재 버전 확인**: installer.iss에서 현재 버전 읽기
2. **CHANGELOG.md 맨 위에 추가**: 역순 정렬 유지
3. **날짜 형식**: YYYY-MM-DD (예: 2025-12-27)
4. **커밋 메시지**: "release: prepare v버전" 형식 사용
5. **태그 메시지**: CHANGELOG 내용 포함
6. **nul 파일 검증**: Git 커밋 전 자동 검증

## 필수 도구

- Git (커밋, 태그, 푸시)
- GitHub CLI (`gh`) - Release 생성용
- .NET 8.0 SDK (빌드용)
- Inno Setup (인스톨러 생성용, 선택)
- tar (zip 생성용, Windows 10+ 내장)

## 체크리스트

릴리스 전 확인사항:
- [ ] 모든 코드 변경 완료
- [ ] 문서 업데이트 완료 (README.md, docs/MdViewer.md)
- [ ] Breaking changes 확인
- [ ] 버전 번호 결정 (Semantic Versioning)
- [ ] CHANGELOG 작성 준비

릴리스 후 확인사항:
- [ ] GitHub Release 생성 확인
- [ ] portable.zip 다운로드 테스트
- [ ] Setup.exe 설치 테스트
- [ ] .md 파일 연결 동작 확인

## MdViewer 특화 기능

### WPF 애플리케이션
- 콘솔 앱과 달리 GUI로 실행되므로 빌드 테스트가 제한적
- 인스톨러 테스트에서 실제 실행 확인 필요

### 설치 위치 및 권한
- **설치 위치**: `%LocalAppData%\Programs\MdViewer`
- **권한**: `PrivilegesRequired=lowest` (관리자 권한 불필요)
- **이유**: PATH 등록 불필요, 사용자별 설치로 충분
- **참고**: NoljiMa는 Program Files (admin 권한, PATH 등록 필요)

### .md 파일 연결
- 첫 실행 시 자동 등록 (HKEY_CURRENT_USER)
- 인스톨러 설치 시 즉시 동작 확인 가능
- 파일 연결 레지스트리 경로 검증 기능 포함

### 멀티 파일 구조
- Services/, Models/ 등 디렉터리 구조
- 단일 파일 프로젝트와 달리 여러 파일 관리 필요
