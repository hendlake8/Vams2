# Vams2 — AI-Driven Mobile Vampire Survivors-like

판타지 세계관의 모바일 싱글 뱀서라이크 게임.
**기획, 리소스 생성, 구현 전 과정을 AI(Claude Code + ComfyUI)로 진행한 프로젝트.**

<video src="https://github.com/user-attachments/assets/9b58bb35-4127-4786-bd94-90051583af74" autoplay loop muted playsinline width="300"></video>

## 개요

| 항목 | 내용 |
|------|------|
| 장르 | 로그라이크 서바이벌 (Vampire Survivors-like) |
| 플랫폼 | 모바일 (Android/iOS), 세로 Portrait 고정 |
| 엔진 | Unity 6 (6000.3.10f1), URP 2D |
| 세션 | 5분 생존, 스킬 빌드 + 스킬 진화 |
| 레퍼런스 | 탕탕특공대 (Survivor.io) |

## AI 활용 범위

| 단계 | AI 도구 | 역할 |
|------|---------|------|
| **기획** | Claude Code | 브레인스토밍, GDD 작성, 밸런스 수치 설계 |
| **리소스** | Claude Code → ComfyUI | GDD 분석 → 필요 리소스 판단 → 프롬프트 구성 → 이미지 생성 |
| **구현** | Claude Code + Unity MCP | 스크립트 작성, 프리팹 생성, 씬 조립, 데이터 설정 |
| **디버깅** | Claude Code | 스크린샷 기반 진단, 코드 수정 |

### 리소스 자동 생성 파이프라인

이 프로젝트의 모든 비주얼 리소스(40+장)는 **사용자가 직접 지정한 것이 아니라 LLM이 자율적으로 판단하여 생성**했습니다.

1. **GDD 분석**: LLM이 게임 기획서(GDD.md)와 구현 설계서를 읽고, 각 마일스톤에 필요한 리소스 목록을 스스로 도출
2. **사양 결정**: 각 리소스의 스타일, 크기, 투명 배경 여부, 색상 톤을 LLM이 게임 맥락에 맞춰 결정
3. **프롬프트 구성**: ComfyUI(Stable Diffusion) API용 프롬프트를 LLM이 태그 가중치까지 포함하여 자동 작성
4. **생성 및 후처리**: 이미지 생성 → 배경 제거(Rembg) → 센터링/리사이즈 → Unity Import Settings 적용까지 자동 수행

사용자의 개입은 **스타일 선택("chibi, 128px, 투명")** 한 줄뿐이며, 어떤 캐릭터/몬스터/아이콘을 몇 장 만들지는 전적으로 LLM이 기획서를 기반으로 판단했습니다.

## 구현 과정 문서

모든 기획/설계/구현 과정이 `Client/Docs/` 폴더에 기록되어 있습니다.

**기획**
- [GAME_CONCEPT_BRIEF.md](Client/Docs/GDD/Core/GAME_CONCEPT_BRIEF.md) — 브레인스토밍 결과 브리프
- [GDD.md](Client/Docs/GDD/Core/GDD.md) — 게임 기획서 (전체)

**설계/구현**
- [IMPLEMENTATION_DESIGN.md](Client/Docs/DEV/Dev/Core/IMPLEMENTATION_DESIGN.md) — 구현 설계서 (시스템 아키텍처)
- [MVP_PLAN.md](Client/Docs/DEV/Dev/Core/MVP_PLAN.md) — 구현 계획서 (Phase별 태스크)

**대화 기록**
- [최초 기획 CHAT_LOG](Client/Docs/CM/%EC%B5%9C%EC%B4%88%20%EA%B8%B0%ED%9A%8D/Phase_01/CHAT_LOG.md) — 기획 브레인스토밍 과정
- [구현1 CHAT_LOG](Client/Docs/CM/%EA%B5%AC%ED%98%841/Phase_01/CHAT_LOG.md) — MVP 구현 과정

**리소스**
- [GenHistory/](Client/GenHistory/) — 리소스 생성에 사용한 프롬프트 기록
- [GenImg/](Client/GenImg/) — AI로 생성한 게임 리소스

## 핵심 시스템

- **코어 루프**: 이동(조이스틱) → 자동공격 → 적 처치 → 보석 흡수 → 레벨업 → 스킬 선택
- **스킬**: 액티브 6종 + 패시브 4종 + 진화 3종
- **적**: 일반 4종 + 보스 4종 (미니보스 3 + 최종보스)
- **웨이브**: 시간 기반 5분 세션, 점진적 난이도 상승 + 보스 아레나
- **UI**: HUD, 레벨업 3카드, 결과 정산, Safe Area 대응

## 기술 스택

- Unity 6, URP 2D Renderer
- New Input System (플로팅 조이스틱)
- ScriptableObject 기반 데이터 (EnemyData, SkillData, WaveData, EvolutionData)
- TextMesh Pro (Noto Serif KR)
