# MdViewer 프로젝트 규칙

## 언어
- 한국어 기본

## 문서
- 모든 문서는 `docs/` 폴더에 위치
- 명세서: `docs/MdViewer.md`
- 릴리스 이력: `CHANGELOG.md` (루트에 단일 파일로 관리)

## 기술 스택
- .NET 8.0 WPF
- MdXaml (마크다운 렌더링)

## Git 규칙

### commit 전 검증
git에 올리기 전에 반드시 nul 파일 존재 여부를 확인하고 삭제:

```bash
# nul 파일 검색 및 삭제
find . -name "nul" -type f -delete 2>/dev/null
git status
```

Windows에서 잘못된 리다이렉션으로 생성될 수 있는 "nul" 파일을 방지합니다.
