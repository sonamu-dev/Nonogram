# AGENTS.md

## 목표
- WinUI 3(Fluent) 기반 Windows용 노노그램 앱을 개발한다.
- 핵심 목표는 퍼즐 자동 생성과 난이도 분석의 신뢰성을 높이는 것이다.

## 작업 단위
- 1 PR = 1 목적을 원칙으로 한다.
- 작은 변경을 선호하며, 단계적으로 병합한다.

## 금지 사항
- 사용자 승인 없이 새 의존성을 추가하지 않는다.
- 사용자 승인 없이 Preview/Experimental SDK를 사용하지 않는다.

## 품질 규칙
- 변경 PR 요약에는 반드시 빌드/테스트 방법을 포함한다.
- 가능하면 실행 결과(성공/실패, 핵심 로그)를 함께 요약한다.

## 구조 원칙
- 엔진 로직은 `Nonogram.Core`에 둔다.
- UI 로직은 `Nonogram.App`에 둔다.
- `Nonogram.Core`는 UI 프레임워크 의존성을 갖지 않는다.

## 성능/스레딩
- UI 스레드 블로킹 작업을 금지한다.
- 장시간 작업은 백그라운드에서 실행하고 취소 가능해야 한다.
- 장시간 작업에는 `CancellationToken`과 진행률(`IProgress` 등) 보고를 적용한다.

## 버전/빌드 관리
- 제품 버전은 SemVer(`Major.Minor.Patch`)를 기준으로 관리한다.
- Windows 배포/파일 버전은 4자리(`Major.Minor.Patch.Revision`)를 사용한다.
- `Revision`은 CI 빌드 카운터로 자동 증가시키는 것을 기본으로 한다.
- 릴리즈 시점에는 `Major/Minor/Patch`만 사람이 결정해 올린다.

## Setup Commands
- `dotnet restore Nonogram.sln`
- `dotnet build Nonogram.sln -c Debug`
- `dotnet test Nonogram.sln -c Debug --no-build`

## Review Guidelines
- 우선순위는 기능 회귀, 예외 처리 누락, 스레딩/응답성 리스크, 테스트 공백 순서로 둔다.
- `Nonogram.Core`가 UI 의존성을 갖지 않는지 반드시 확인한다.
- 장시간 연산 경로에는 취소 가능성(`CancellationToken`)과 진행률 확장 지점이 있는지 확인한다.
- UI 변경 PR은 사용자 영향도(입력/상태표시/접근성)를 요약에 명시한다.
- 로그/오류 리포트 개선 여지가 있으면 대안을 함께 제안한다.

## 문서 규칙
- 공용 동작(사용자 관찰 가능 동작)이 바뀌면 `docs/`를 함께 갱신한다.

## 핵심 결론
- 안정성: 승인 없는 스택 확장 금지.
- 유지보수성: Core/UI 분리 강제.
- 사용자 경험: 비차단 UI와 취소 가능한 장시간 작업을 기본값으로 한다.
