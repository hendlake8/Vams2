# Vams2 MVP 구현 계획서

## 개요

- **목적**: GDD 기반 MVP 코어 루프 구현 (M0~M5)
- **참조 설계 문서**: [IMPLEMENTATION_DESIGN.md](./IMPLEMENTATION_DESIGN.md)
- **참조 기획 문서**: [GDD.md](../../GDD/Core/GDD.md)
- **엔진**: Unity 6 (6000.3.10f1), URP 2D, New Input System
- **화면**: 세로 (Portrait) 고정, 1080×1920

## 구현 순서

### Phase 1-1: 프로젝트 기반 설정 (Unity)

- [x] Unity 프로젝트 세로 고정 설정 (Player Settings → Default Orientation: Portrait)
- [x] 해상도 설정 (Reference Resolution: 1080×1920)
- [x] Sorting Layer 생성 (Background, Drops, Enemies, Player, Projectiles, UI_World)
- [x] Physics Layer 생성 (Player, Enemy, PlayerProjectile, EnemyProjectile, Gem, DropItem, BossArena)
- [x] Physics Layer 충돌 매트릭스 설정
- [x] Tag 생성 (Player, Enemy, Gem, DropItem, BossArena, Projectile)
- [x] 폴더 구조 생성 (00_BuildScenes ~ 02_Modules)
- [x] 4개 씬 파일 생성 (Title, Lobby, InGame, Result) 및 Build Settings 등록

### Phase 1-2: 리소스 생성 — M1용 (`/gi`)

- [x] 플레이어 "루드" 스프라이트 생성 (2D 탑다운, 로브 입은 풋내기 마법사, 128×128, 투명 배경)
- [x] 슬라임 스프라이트 생성 (2D 탑다운, 초록 젤리형, 96×96, 투명 배경)
- [x] 매직 볼트 투사체 스프라이트 생성 (하늘색 마법 구체, 64×64, 투명 배경)
- [x] 배경 타일 기본 생성 (탑다운 풀밭 텍스처, 512×512, 타일링 가능)
- [x] 배경 타일 변형 2~3종 생성 (꽃/돌 포함 풀밭, 512×512)
- [x] 장식 나무 스프라이트 생성 (탑다운 판타지 나무, 128×128, 투명 배경)
- [x] 장식 바위 스프라이트 생성 (탑다운 돌/바위, 96×96, 투명 배경)
- [x] 경험치 보석 소/대 스프라이트 생성 (초록 보석, 32×32 / 64×64, 투명 배경)
- [x] 생성된 이미지 후처리 (배경 제거, 크기 조정)
- [x] Unity Import Settings 설정 (Sprite 2D, Pixels Per Unit)
- [x] Textures 폴더에 배치 (Player/, Enemy/, Skill/, Map/, Drop/)

### Phase 2-1: 핵심 모듈 구현 (Core)

- [x] ObjectPool 제네릭 클래스 구현 (02_Modules/Core/ObjectPool.cs)
- [x] GameManager 싱글턴 구현 (DontDestroyOnLoad, GameState 관리)
- [x] SceneLoader 구현 (씬 전환 유틸리티)
- [x] CombatHelper static 유틸리티 구현 (데미지 계산, 크리티컬, 방어력)

### Phase 2-2: 입력 시스템 구현

- [x] FloatingJoystick 구현 (터치 시작/드래그/종료, 방향/강도 계산)
- [x] 조이스틱 UI 프리팹 생성 (Canvas + Image 2개: 바탕, 손잡이)
- [x] 하단 60% 활성 영역, 데드존 10px, 최대 반경 80px
- [x] InputSystem_Actions.inputactions와 연동 (터치 입력)

### Phase 2-3: 플레이어 구현

- [x] Player 프리팹 생성 (SpriteRenderer, Rigidbody2D, CircleCollider2D)
- [x] PlayerMovement 구현 (조이스틱 방향/강도 → Rigidbody2D velocity)
- [x] PlayerStats 구현 (HP, 공격력, 이속, 크리티컬, 레벨, 경험치)
- [x] PlayerCombat 구현 (피격 처리, 사망 판정)
- [x] ExpCollector 구현 (트리거 콜라이더, 보석 흡수 범위)

### Phase 2-4: 적 시스템 구현

- [x] EnemyData ScriptableObject 정의 (ID, 스프라이트, AI유형, HP, DMG, 이속, EXP)
- [x] 슬라임 EnemyData 인스턴스 생성
- [x] Enemy 프리팹 생성 (SpriteRenderer, Rigidbody2D, CircleCollider2D)
- [x] EnemyBehaviour 구현 (Chase AI: 플레이어 방향 직선 추적)
- [x] EnemyHealth 구현 (피격, 사망, 드롭 트리거)
- [x] EnemyDrop 구현 (사망 시 경험치 보석 드롭)
- [x] EnemySpawner 구현 (화면 밖 원형 스폰, 최대 수 제한, 오브젝트 풀링)

### Phase 2-5: 자동공격 & 투사체 구현

- [x] SkillBase 추상 클래스 구현 (쿨다운 관리, Execute 추상 메서드)
- [x] MagicBoltSkill 구현 (가장 가까운 적에게 직선 투사체 발사)
- [x] Projectile 구현 (이동, 수명, 충돌 시 데미지, 풀 반환)
- [x] SkillData ScriptableObject 정의 (ID, 이름, 타입, 레벨별 수치 배열)
- [x] 매직 볼트 SkillData 인스턴스 생성
- [x] 투사체 오브젝트 풀 설정

### Phase 2-6: 무한 맵 구현

- [x] InfiniteMap 구현 (3×3 청크 그리드 배치, 플레이어 기준 재배치)
- [x] 배경 청크 프리팹 생성 (SpriteRenderer + 배경 타일 스프라이트)
- [x] 청크 변형 (기본/변형 타일 랜덤 선택)
- [x] 장식 오브젝트 배치 (청크당 2~5개, 나무/바위 랜덤)

### Phase 2-7: 카메라 & InGame 씬 조립

- [x] CameraFollow 구현 (Orthographic Size 10, 플레이어 추적, Lerp)
- [x] InGameScene 조립 (Player, EnemySpawner, InfiniteMap, Camera, Canvas)
- [x] M1 플레이 테스트 (이동, 자동공격, 적 스폰/처치, 맵 스크롤)

### Phase 3-1: 리소스 생성 — M2용 (`/gi`)

- [x] 스킬 아이콘 10종 생성 (매직볼트, 회전검, 파이어볼, 아이스스피어, 라이트닝, 포이즌클라우드, 마나서지, 원소핵, 마법갑옷, 신속의부츠)
- [x] 드롭 아이템 4종 생성 (회복 고기, 자석, 폭탄, 골드 코인)
- [x] Unity Import 및 Textures/Icon/, Textures/Drop/에 배치

### Phase 3-2: 경험치 & 레벨업 시스템 구현

- [x] ExpGem 구현 (드롭, 플레이어 접근 시 흡수 이동, 수집 시 EXP 부여, 풀링)
- [x] PlayerStats.AddExp() 구현 (경험치 누적, 요구량 체크, 레벨업 판정)
- [x] 경험치 요구량 공식 적용: 10 + (Lv × 5)

### Phase 3-3: 스킬 매니저 & 선택지 시스템 구현

- [x] SkillManager 구현 (액티브 6슬롯, 패시브 6슬롯 관리)
- [x] SkillSlot 구조 구현 (SkillData 참조, 레벨, 스킬 인스턴스)
- [x] SkillManager.AddOrUpgradeSkill() 구현 (새 스킬 추가 또는 레벨업)
- [x] SkillManager.GetRandomChoices(3) 구현 (빈 슬롯 시 미보유 1개 보장)

### Phase 3-4: 레벨업 UI 구현

- [x] LevelUpUI 구현 (Time.timeScale = 0 일시정지, 3카드 표시)
- [x] LevelUpCard 구현 (아이콘, 스킬명, 레벨/NEW 표시, 설명, 탭 선택)
- [x] PlayerStats 레벨업 → LevelUpUI 호출 연결
- [x] 카드 선택 → SkillManager 적용 → 게임 재개 플로우

### Phase 3-5: HUD 구현

- [x] HudController 구현 (HP바, EXP바, 경과 시간, 스킬 아이콘 6개)
- [x] Canvas 프리팹 구성 (Slider ×2, TMP_Text ×1, Image ×6)
- [x] PlayerStats/WaveManager와 데이터 바인딩
- [x] M2 플레이 테스트 (보석 흡수, 레벨업, 스킬선택 UI, HUD 표시)

### Phase 4-1: 리소스 생성 — M3용 (`/gi`)

- [x] 늑대 스프라이트 생성 (2D 탑다운, 회색 늑대, 96×96, 투명 배경)
- [x] 스켈레톤 스프라이트 생성 (2D 탑다운, 해골 전사, 96×96, 투명 배경)
- [x] 엘리트 슬라임 스프라이트 생성 (큰 진한 초록 슬라임, 128×128, 투명 배경)
- [x] 파이어볼 투사체 생성 (불꽃 구체, 64×64, 투명 배경)
- [x] 아이스 스피어 투사체 생성 (얼음 화살, 64×64, 투명 배경)
- [x] 회전 검 스프라이트 생성 (은빛 검, 32×96, 투명 배경)
- [x] 포이즌 클라우드 장판 생성 (보라색 독안개, 256×256, 반투명)
- [x] 스켈레톤 화살 투사체 생성 (뼈 화살, 48×48, 투명 배경)
- [x] Unity Import 및 Textures/에 배치

### Phase 4-2: 액티브 스킬 5종 구현 (매직 볼트 제외)

- [x] SpinningBladeSkill 구현 (캐릭터 주변 회전, 레벨업 시 검 수 증가)
- [x] FireballSkill 구현 (랜덤 방향 폭발 투사체, 레벨업 시 반경 증가)
- [x] IceSpearSkill 구현 (관통 투사체 + 감속 디버프, 레벨업 시 감속률 증가)
- [x] LightningSkill 구현 (가까운 적 체인 데미지, 레벨업 시 체인 수 증가)
- [x] PoisonCloudSkill 구현 (장판 설치, 지속 데미지, 레벨업 시 크기 증가)
- [x] 각 스킬 SkillData 인스턴스 생성 (Lv1~5 수치 테이블)

### Phase 4-3: 패시브 스킬 4종 구현

- [x] SkillManager.ApplyPassive() 구현 (패시브 타입별 PlayerStats 보정)
- [x] 마나 서지: 공격속도 +10%/Lv
- [x] 원소 핵: 원소 데미지 +15%/Lv
- [x] 마법 갑옷: 방어 +5/Lv, 넉백 저항 +20%/Lv
- [x] 신속의 부츠: 이속 +8%/Lv, 회피 +3%/Lv
- [x] 각 패시브 SkillData 인스턴스 생성

### Phase 4-4: 적 종류 추가

- [x] 늑대 EnemyData 생성 (FastChase AI, HP 15, DMG 8, 이속 6.0)
- [x] 스켈레톤 EnemyData 생성 (Ranged AI, HP 20, DMG 10, 이속 2.5)
- [x] 엘리트 슬라임 EnemyData 생성 (EliteSplit AI, HP 50, DMG 12, 이속 3.0)
- [x] EnemyBehaviour에 FastChase AI 추가 (높은 이속 직선 추적)
- [x] EnemyBehaviour에 Ranged AI 추가 (사거리 유지 + 투사체 발사)
- [x] EnemyBehaviour에 EliteSplit AI 추가 (사망 시 소형 2마리 스폰)
- [x] 적 투사체 프리팹 생성 (스켈레톤 화살)
- [x] M3 플레이 테스트 (10종 스킬, 패시브 적용, 4종 적 동작)

### Phase 5-1: 리소스 생성 — M4용 (`/gi`)

- [x] 미니보스 3종 스프라이트 생성 (탑다운 판타지, 192×192, 투명 배경)
- [x] 최종 보스 스프라이트 생성 (탑다운 드래곤/마왕, 256×256, 투명 배경)
- [x] 진화 아이콘 3종 생성 (메테오 스트라이크, 썬더 스톰, 블레이드 배리어, 128×128)
- [x] 메테오 투사체 스프라이트 생성 (불타는 운석, 128×128, 투명 배경)
- [x] 보스 투사체 스프라이트 생성 (어둠 에너지 구체, 64×64, 투명 배경)
- [x] Unity Import 및 Textures/에 배치

### Phase 5-2: 스킬 진화 시스템 구현

- [x] EvolutionData ScriptableObject 정의 (필요 액티브, 필요 패시브, 결과 스킬)
- [x] EvolutionData 인스턴스 3개 생성 (파이어볼+원소핵, 라이트닝+마나서지, 회전검+마법갑옷)
- [x] SkillManager.CheckEvolution() 구현 (양쪽 Lv5 체크)
- [x] SkillManager.ExecuteEvolution() 구현 (슬롯 회수, 진화 스킬 장착)
- [x] 레벨업 UI에 진화 선택지 표시 (금색 테두리 + "진화!")
- [x] MeteorStrikeSkill 구현 (운석 낙하, 광역 폭발, DMG ×2, 반경 ×2)
- [x] ThunderStormSkill 구현 (화면 전체 번개, 전체 적 타격)
- [x] BladeBarrierSkill 구현 (회전 반경 확대, 넉백, DMG ×2)

### Phase 5-3: 보스 시스템 구현

- [x] BossArenaManager 구현 (원형 벽 생성/제거, EdgeCollider2D)
- [x] 보스 아레나 벽 스프라이트 코드 생성 (반투명 링)
- [x] 보스 EnemyData 4개 생성 (미니보스 3 + 최종보스 1, 원거리 공격)
- [x] EnemyBehaviour 보스용 확장 (원거리 투사체 패턴)
- [x] 보스 처치 시 아레나 해제 + 보상 드롭
- [x] M4 플레이 테스트 (진화 발동, 보스 아레나, 보스 전투)

### Phase 6-1: 리소스 생성 — M5용 (`/gi`)

- [x] 타이틀 로고 생성 ("Vams2" 판타지 스타일, 512×256, 투명 배경)
- [x] UI 카드 프레임 생성 (판타지 RPG 카드 배경, 256×384, 반투명)
- [x] UI 버튼 프레임 생성 (판타지 RPG 버튼, 256×96, 투명 배경)
- [x] Unity Import 및 Textures/UI/에 배치

### Phase 6-2: 웨이브 시스템 구현

- [x] WaveData ScriptableObject 정의 (WaveEntry 리스트: 시작시간, 종료시간, 적 종류, 스폰 간격, 보스 여부)
- [x] 5분 세션 WaveData 인스턴스 작성 (GDD 웨이브 페이싱 테이블 반영)
- [x] WaveManager 구현 (시간 경과 추적, WaveEntry 기반 스폰 트리거, 보스 스폰 연동)
- [x] 세션 종료 판정 (5분 경과 or 최종 보스 처치 → GameManager.EndGame())

### Phase 6-3: 드롭 아이템 구현

- [x] DropItem 구현 (Heal, Magnet, Bomb, Gold 타입별 효과)
- [x] 회복 고기: HP 20% 회복
- [x] 자석: 화면 내 모든 보석 즉시 흡수
- [x] 폭탄: 화면 내 모든 적에게 50 데미지
- [x] 골드: SessionResult에 골드 추가
- [x] EnemyDrop에 아이템 드롭 확률 추가
- [x] 드롭 아이템 오브젝트 풀 설정

### Phase 6-4: 결과 화면 구현

- [x] SessionResult 데이터 구조 정의 (처치 수, 레벨, 플레이 타임)
- [x] ResultScreenUI 구현 (클리어/패배 표시, 처치 수, 레벨, 타임)
- [x] 로비 복귀 버튼 → SceneLoader.LoadScene("LobbyScene")
- [x] ResultScene 조립

### Phase 6-5: 로비 & 타이틀 화면 구현

- [x] TitleUI 구현 (로고 표시, 터치하여 시작)
- [x] TitleScene 조립
- [x] LobbyUI 기본 구현 (캐릭터 정보 표시, 출격 버튼)
- [x] LobbyScene 조립
- [x] 장비/합성 버튼 배치 (비활성, MVP 이후)

### Phase 6-6: 게임 플로우 통합 & 최종 테스트

- [x] GameManager 전체 플로우 연결 (Title → Lobby → InGame → Result → Lobby)
- [x] SessionResult 데이터 InGame → Result 전달
- [x] PlayerCombat 사망 시 GameManager.EndGame(false) 호출
- [x] 전체 5분 세션 플레이 테스트
- [x] 웨이브 밸런스 조정 (적 밀도, 보스 타이밍)
- [x] 성능 확인 (적 100+ 동시 존재 시 프레임 체크)
