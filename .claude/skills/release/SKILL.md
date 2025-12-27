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

#### 프로젝트 구조 검증
```bash
# 필수 파일 존재 확인
- CHANGELOG.md (존재)
- CLAUDE.md (존재)
- README.md (존재)
- MdViewer.csproj (존재)
- installer.iss (존재)

# 불필요한 파일 확인
- RELEASE_NOTES_*.md (없어야 함)
- nul 파일 (없어야 함)
```

### 5. 변경 사항 보고

사용자에게 다음 정보를 보고:
- 업데이트된 파일 목록
- 버전 변경 내역 (이전 → 새 버전)
- CHANGELOG 추가 내용
- 검증 결과 요약

### 6. Git 작업

#### 6.1 커밋
```bash
git add .
git commit -m "release: prepare v새버전

- Update version to 새버전 in all files
- Update CHANGELOG.md with v새버전 changes
- [변경 사항 요약]

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

#### 6.2 Git 태그 생성

사용자에게 Git 태그를 생성할지 물어봅니다:

```bash
git tag -a v새버전 -m "MdViewer v새버전

[CHANGELOG 내용을 여기에 복사]

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

#### 6.3 푸시

```bash
git push
git push --tags  # 태그 생성한 경우
```

### 7. 다음 단계 안내

릴리스 완료 후 사용자에게 다음 작업을 안내:

1. **빌드 및 인스톨러 생성**
   ```bash
   dotnet publish -c Release -o ./publish
   # Inno Setup으로 installer.iss 컴파일
   ```

2. **GitHub Release 생성**
   - https://github.com/seeper0/MdViewer/releases/new
   - 태그: v새버전
   - 제목: MdViewer v새버전
   - 설명: CHANGELOG.md 내용 복사
   - 파일 첨부: MdViewer-v새버전-Setup.exe

3. **릴리스 공지**
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
7. 태그 생성 여부 확인
8. 다음 단계 안내

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
6. **Pre-commit hook**: nul 파일 자동 검증됨

## 필수 도구

- Git (커밋, 태그, 푸시)
- .NET 8.0 SDK (빌드용, Skill에서는 직접 사용 안 함)
- Inno Setup (인스톨러 생성용, 사용자가 수동 실행)

## 체크리스트

릴리스 전 확인사항:
- [ ] 모든 테스트 통과
- [ ] 문서 업데이트 완료
- [ ] Breaking changes 확인
- [ ] 버전 번호 결정 (Semantic Versioning)
- [ ] CHANGELOG 작성 준비

릴리스 후 확인사항:
- [ ] GitHub Release 생성
- [ ] 인스톨러 업로드
- [ ] 릴리스 공지
- [ ] 이슈/PR 정리
