# MVP 구현 설계서

GDD(`Docs/GDD/Core/GDD.md`) 기반 구현 설계.
리소스는 에셋 스토어 없이 **코드로 플레이스홀더를 생성**하여 프로토타입을 완성한다.

---

## 1. 프로젝트 구조

Unity 폴더 구조 규칙(`unity-folder-structure.md`) 적용.

```
Assets/
├── 00_BuildScenes/
│   ├── TitleScene.unity
│   ├── LobbyScene.unity
│   ├── InGameScene.unity
│   └── ResultScene.unity
│
├── 01_Contents/
│   ├── Title/
│   │   └── Scripts/
│   ├── Lobby/
│   │   ├── Scripts/
│   │   └── RES/Builtin/Resources/Prefabs/
│   ├── InGame/
│   │   ├── Scripts/
│   │   │   ├── Player/
│   │   │   ├── Enemy/
│   │   │   ├── Skill/
│   │   │   ├── Wave/
│   │   │   ├── Drop/
│   │   │   ├── Map/
│   │   │   └── Combat/
│   │   └── RES/
│   │       ├── Builtin/Resources/
│   │       │   ├── Prefabs/
│   │       │   └── Data/           # ScriptableObject 인스턴스
│   │       └── Bundle/
│   │           └── Textures/
│   └── Result/
│       └── Scripts/
│
├── 02_Modules/
│   ├── Core/                       # 게임 전역 시스템
│   │   ├── GameManager.cs
│   │   ├── SceneLoader.cs
│   │   └── ObjectPool.cs
│   ├── Input/
│   │   └── FloatingJoystick.cs
│   ├── UI/
│   │   ├── HudController.cs
│   │   ├── LevelUpCardUI.cs
│   │   └── ResultScreenUI.cs
│   ├── Data/
│   │   ├── SkillData.cs            # ScriptableObject 정의
│   │   ├── EnemyData.cs
│   │   ├── WaveData.cs
│   │   └── PlayerData.cs
│   └── Util/
│       └── SpriteUtil.cs              # 런타임 스프라이트 후처리 유틸 (필요 시)
│
└── 09_PreloadAssets/
    └── (필요 시)
```

---

## 2. 씬 구성 및 게임 플로우

### 2.1 씬 전환 흐름

```
TitleScene → LobbyScene → InGameScene → ResultScene → LobbyScene (반복)
```

### 2.2 씬별 책임

| 씬 | 역할 | 핵심 오브젝트 |
|----|------|--------------|
| TitleScene | 타이틀 로고, 터치하여 시작 | TitleUI |
| LobbyScene | 캐릭터 정보, 장비(추후), 출격 버튼 | LobbyUI, PlayerDataManager |
| InGameScene | 코어 게임플레이 전체 | GameSession, Player, EnemySpawner, WaveManager, SkillManager, MapScroller, HUD |
| ResultScene | 결과 정산 (처치 수, 레벨, 플레이 타임) | ResultScreenUI |

### 2.3 GameManager (DontDestroyOnLoad 싱글턴)

```
GameManager
├── mCurrentState: GameState (Title, Lobby, InGame, Result)
├── mSessionResult: SessionResult (처치 수, 레벨, 플레이 타임)
├── mPlayerPersistData: PlayerPersistData (장비 등 메타 데이터, 추후)
│
├── LoadScene(sceneName)
├── StartGame()
├── EndGame(isCleared)
└── ReturnToLobby()
```

---

## 3. 입력 시스템 (플로팅 조이스틱)

### 3.1 구현 방식

Unity New Input System의 터치 입력을 사용하되, 조이스틱 UI는 직접 구현.

### 3.2 FloatingJoystick 설계

```
FloatingJoystick : MonoBehaviour
├── [설정]
│   ├── mActivationArea: RectTransform    # 하단 60% 영역
│   ├── mDeadZone: float = 10f            # px
│   ├── mMaxRadius: float = 80f           # px
│
├── [상태]
│   ├── mIsActive: bool
│   ├── mOrigin: Vector2                  # 터치 시작점
│   ├── mDirection: Vector2               # 정규화된 방향
│   ├── mMagnitude: float                 # 0~1 강도
│
├── [UI]
│   ├── mBaseImage: Image                 # 조이스틱 바탕 원
│   ├── mKnobImage: Image                # 조이스틱 손잡이
│
├── [입력 처리]
│   ├── OnPointerDown(position)           # 터치 시작 → 조이스틱 표시
│   ├── OnPointerDrag(position)           # 드래그 → 방향/강도 계산
│   ├── OnPointerUp()                     # 터치 종료 → 조이스틱 숨김
│
└── [외부 인터페이스]
    ├── GetDirection(): Vector2
    └── GetMagnitude(): float
```

### 3.3 입력 → 이동 연결

```
PlayerMovement
├── mJoystick: FloatingJoystick (Inspector 참조)
├── mMoveSpeed: float = 5.0f
│
└── FixedUpdate()
    direction = mJoystick.GetDirection()
    magnitude = mJoystick.GetMagnitude()
    velocity = direction * magnitude * mMoveSpeed
    rigidbody2D.linearVelocity = velocity
```

---

## 4. 플레이어 시스템

### 4.1 컴포넌트 구성

```
[Player GameObject]
├── SpriteRenderer          # 플레이스홀더 스프라이트
├── Rigidbody2D             # Dynamic, Gravity 0
├── CircleCollider2D        # 피격 판정
├── PlayerMovement          # 이동 처리
├── PlayerStats             # 스탯 관리
├── PlayerCombat            # 피격, 사망 처리
└── ExpCollector            # 경험치 보석 흡수 (트리거 콜라이더)
```

### 4.2 PlayerStats

```
PlayerStats : MonoBehaviour
├── [기본 스탯]
│   ├── mMaxHp: int = 100
│   ├── mCurrentHp: int
│   ├── mBaseDamage: int = 10
│   ├── mMoveSpeed: float = 5.0f
│   ├── mCritChance: float = 0.05f
│   ├── mCritMultiplier: float = 1.5f
│   ├── mDefense: int = 0
│   ├── mDodgeChance: float = 0f
│
├── [패시브 보정값]
│   ├── mAttackSpeedBonus: float = 0f    # 마나 서지
│   ├── mElementalDmgBonus: float = 0f   # 원소 핵
│   ├── mDefenseBonus: int = 0           # 마법 갑옷
│   ├── mKnockbackResist: float = 0f     # 마법 갑옷
│   ├── mMoveSpeedBonus: float = 0f      # 신속의 부츠
│   ├── mDodgeBonus: float = 0f          # 신속의 부츠
│
├── [경험치]
│   ├── mLevel: int = 1
│   ├── mCurrentExp: int = 0
│   ├── mExpToNextLevel: int             # 10 + (Level × 5)
│   ├── mGemCollectRadius: float = 2.0f
│
├── AddExp(amount)
├── GetRequiredExp(): int
├── GetFinalDamage(baseDmg): float       # 크리티컬, 원소, 장비 보정 적용
├── TakeDamage(rawDmg)
├── ApplyPassiveBonus(passiveType, level)
└── RemovePassiveBonus(passiveType)
```

### 4.3 ExpCollector

```
ExpCollector : MonoBehaviour
├── mCollectRadius: float = 2.0f          # CircleCollider2D.radius와 동기화
│
├── OnTriggerEnter2D(collider)            # "Gem" 태그 → 흡수 시작
│   gem.StartMoveToPlayer(transform)
│
└── OnGemCollected(expAmount)
    mPlayerStats.AddExp(expAmount)
```

---

## 5. 적 시스템

### 5.1 컴포넌트 구성

```
[Enemy GameObject]
├── SpriteRenderer
├── Rigidbody2D              # Dynamic, Gravity 0
├── CircleCollider2D         # 피격 판정
├── EnemyBehaviour           # AI, 이동, 공격
├── EnemyHealth              # 체력, 피격, 사망 처리
└── EnemyDrop                # 드롭 아이템 결정
```

### 5.2 EnemyBehaviour (AI 유형)

```
EnemyBehaviour : MonoBehaviour
├── mEnemyData: EnemyData       # ScriptableObject 참조
├── mTarget: Transform           # 플레이어
├── mAiType: EnemyAiType         # Chase, FastChase, Ranged, EliteSplit
│
├── [Chase AI]
│   직선으로 플레이어를 향해 이동
│   rigidbody.linearVelocity = (target - position).normalized * speed
│
├── [FastChase AI]
│   Chase와 동일, 높은 이속
│
├── [Ranged AI]
│   일정 거리 유지 + 주기적 투사체 발사
│   사거리 내: 정지 후 공격
│   사거리 밖: 느린 추적
│
└── [EliteSplit AI]
    Chase AI + 사망 시 소형 2마리 스폰
```

### 5.3 EnemySpawner

```
EnemySpawner : MonoBehaviour
├── mPlayer: Transform
├── mWaveManager: WaveManager
├── mEnemyPool: ObjectPool<Enemy>
├── mSpawnRadius: float = 12f      # 화면 밖에서 스폰
├── mMaxEnemies: int = 150         # 동시 존재 최대 수
│
├── Update()
│   currentWave = mWaveManager.GetCurrentWaveConfig()
│   for each enemyType in currentWave.mEnemyTypes:
│       if spawnTimer elapsed:
│           SpawnEnemy(enemyType, GetRandomSpawnPosition())
│
├── SpawnEnemy(type, position)
│   enemy = mEnemyPool.Get()
│   enemy.Initialize(type, scaledStats)
│
├── GetRandomSpawnPosition(): Vector2
│   # 플레이어 주변 원형 영역, 화면 밖 위치에서 스폰
│   angle = Random(0, 360)
│   return playerPos + direction * mSpawnRadius
│
└── ReturnToPool(enemy)
```

---

## 6. 스킬 시스템

### 6.1 아키텍처 개요

```
SkillManager (인게임 스킬 슬롯 관리)
├── mActiveSkills: SkillSlot[6]
├── mPassiveSkills: SkillSlot[6]
├── mEvolutions: List<EvolutionData>
│
├── AddOrUpgradeSkill(skillData)
├── CheckEvolution(): EvolutionData?
├── GetRandomChoices(count=3): List<SkillChoice>
└── ExecuteEvolution(evolutionData)

SkillSlot
├── mData: SkillData
├── mLevel: int (1~5)
└── mSkillInstance: SkillBase (실행 중인 스킬 컴포넌트)
```

### 6.2 스킬 실행 구조

모든 스킬은 `SkillBase`를 상속하여 개별 동작을 구현한다.

```
SkillBase : MonoBehaviour (플레이어의 자식 오브젝트)
├── mSkillData: SkillData
├── mLevel: int
├── mCooldown: float
├── mCooldownTimer: float
│
├── Initialize(data, level)
├── OnLevelUp(newLevel)
├── Update()                        # 쿨다운 체크 → Execute 호출
├── Execute()                       # abstract, 각 스킬이 구현
└── OnDestroy()                     # 정리

구체 스킬:
├── MagicBoltSkill    : SkillBase   # 직선 투사체 발사
├── SpinningBladeSkill: SkillBase   # 주변 회전 오브젝트
├── FireballSkill     : SkillBase   # 폭발 범위 투사체
├── IceSpearSkill     : SkillBase   # 관통 투사체 + 감속
├── LightningSkill    : SkillBase   # 체인 데미지
├── PoisonCloudSkill  : SkillBase   # 장판 생성
│
진화 스킬:
├── MeteorStrikeSkill : SkillBase   # 메테오 (파이어볼 진화)
├── ThunderStormSkill : SkillBase   # 썬더스톰 (라이트닝 진화)
└── BladeBarrierSkill : SkillBase   # 블레이드 배리어 (회전 검 진화)
```

### 6.3 패시브 스킬 처리

패시브는 별도 컴포넌트 없이 `PlayerStats`의 보정값을 직접 수정.

```
SkillManager.ApplyPassive(passiveType, level):
    switch passiveType:
        ManaSurge:
            mPlayerStats.mAttackSpeedBonus = level * 0.10f
        ElementalCore:
            mPlayerStats.mElementalDmgBonus = level * 0.15f
        MagicArmor:
            mPlayerStats.mDefenseBonus = level * 5
            mPlayerStats.mKnockbackResist = level * 0.20f
        SwiftBoots:
            mPlayerStats.mMoveSpeedBonus = level * 0.08f
            mPlayerStats.mDodgeBonus = level * 0.03f
```

### 6.4 스킬 진화 체크

```
SkillManager.CheckEvolution():
    for each evo in mEvolutions:
        activeSlot = FindSlot(evo.mRequiredActive)
        passiveSlot = FindSlot(evo.mRequiredPassive)
        if activeSlot != null && activeSlot.mLevel == 5
           && passiveSlot != null && passiveSlot.mLevel == 5:
            return evo
    return null
```

### 6.5 레벨업 스킬 선택지 생성

```
SkillManager.GetRandomChoices(count=3):
    candidates = []

    # 1. 진화 가능하면 진화를 최우선으로
    evolution = CheckEvolution()
    if evolution != null:
        candidates.Add(EvolutionChoice)

    # 2. 기존 스킬 레벨업 (Lv < 5인 것)
    for each slot in mActiveSkills + mPassiveSkills:
        if slot.mLevel < 5:
            candidates.Add(UpgradeChoice(slot))

    # 3. 새 스킬 (빈 슬롯이 있을 때)
    if hasEmptyActiveSlot:
        for each unownedActive:
            candidates.Add(NewSkillChoice)
    if hasEmptyPassiveSlot:
        for each unownedPassive:
            candidates.Add(NewSkillChoice)

    # 4. 규칙: 최소 1개는 미보유 (가능할 때)
    shuffle(candidates)
    return candidates.Take(count)   # 규칙 보장 로직 포함
```

---

## 7. 웨이브 시스템

### 7.1 WaveData (ScriptableObject)

```
WaveData : ScriptableObject
├── mWaveName: string
├── mEntries: List<WaveEntry>

WaveEntry
├── mStartTime: float              # 이 엔트리가 시작되는 시간 (초)
├── mEndTime: float                # 이 엔트리가 끝나는 시간 (초)
├── mEnemyData: EnemyData          # 스폰할 적 종류
├── mSpawnInterval: float          # 스폰 간격 (초)
├── mSpawnCount: int               # 1회 스폰 시 마리 수
├── mIsBoss: bool                  # 보스 여부
```

### 7.2 WaveManager

```
WaveManager : MonoBehaviour
├── mWaveData: WaveData
├── mElapsedTime: float = 0
├── mSessionDuration: float = 300f  # 5분
├── mIsSessionActive: bool
├── mIsBossActive: bool
│
├── Update()
│   mElapsedTime += Time.deltaTime
│   if mElapsedTime >= mSessionDuration:
│       OnSessionComplete()
│
│   for each entry in mWaveData.mEntries:
│       if entry.mStartTime <= mElapsedTime <= entry.mEndTime:
│           if entry.mIsBoss && !mIsBossActive:
│               SpawnBoss(entry)
│           else if !entry.mIsBoss:
│               SpawnWave(entry)
│
├── SpawnBoss(entry)
│   mIsBossActive = true
│   BossArenaManager.CreateArena()
│   EnemySpawner.SpawnBoss(entry.mEnemyData)
│
├── OnBossDefeated()
│   mIsBossActive = false
│   BossArenaManager.DestroyArena()
│
├── OnSessionComplete()
│   GameManager.EndGame(isCleared: true)
│
├── GetElapsedTime(): float
└── GetEnemyHealthScale(): float
    # BaseHP × (1 + mElapsedTime/60 × 0.15)
```

### 7.3 BossArenaManager

```
BossArenaManager : MonoBehaviour
├── mArenaPrefab: GameObject        # 원형 벽 프리팹
├── mArenaInstance: GameObject
├── mArenaRadius: float = 8f
│
├── CreateArena()
│   mArenaInstance = Instantiate(mArenaPrefab, playerPosition)
│   # 원형 EdgeCollider2D로 경계 생성
│
└── DestroyArena()
    Destroy(mArenaInstance)
```

---

## 8. 전투 시스템

### 8.1 데미지 계산

```
CombatHelper (static 유틸리티)
├── static CalculateDamage(baseDmg, attackerStats, defenderStats): DamageResult
│   # 1. 원소 보정
│   dmg = baseDmg * (1 + attackerStats.mElementalDmgBonus)
│
│   # 2. 크리티컬 판정
│   isCrit = Random(0,1) < attackerStats.mCritChance
│   if isCrit: dmg *= attackerStats.mCritMultiplier
│
│   # 3. 방어력 감소
│   dmg = dmg * (100f / (100f + defenderStats.mDefense))
│
│   return DamageResult(finalDmg, isCrit)
│
├── static CheckDodge(dodgeChance): bool
│   return Random(0,1) < dodgeChance
```

### 8.2 투사체 시스템

```
Projectile : MonoBehaviour
├── mDamage: float
├── mSpeed: float
├── mDirection: Vector2
├── mLifeTime: float
├── mIsPiercing: bool               # 관통 여부 (아이스 스피어)
├── mSlowAmount: float              # 감속량
├── mSlowDuration: float            # 감속 지속시간
│
├── Initialize(dmg, speed, dir, options)
├── Update()
│   transform.Translate(mDirection * mSpeed * Time.deltaTime)
│   mLifeTime -= Time.deltaTime
│   if mLifeTime <= 0: ReturnToPool()
│
└── OnTriggerEnter2D(collider)
    if collider.tag == "Enemy":
        EnemyHealth.TakeDamage(mDamage)
        if mSlowAmount > 0: ApplySlow()
        if !mIsPiercing: ReturnToPool()
```

---

## 9. 드롭 시스템

### 9.1 경험치 보석

```
ExpGem : MonoBehaviour
├── mExpAmount: int
├── mIsBeingCollected: bool
├── mMoveSpeed: float = 10f
│
├── Initialize(expAmount, position)
├── StartMoveToPlayer(target)
│   mIsBeingCollected = true
│
├── Update()
│   if mIsBeingCollected:
│       transform.position = MoveTowards(target, mMoveSpeed * Time.deltaTime)
│       if reached: OnCollected()
│
└── OnCollected()
    ExpCollector.OnGemCollected(mExpAmount)
    ReturnToPool()
```

### 9.2 기타 드롭 아이템

```
DropItem : MonoBehaviour
├── mItemType: DropItemType         # Heal, Magnet, Bomb, Gold
├── mValue: float
│
└── OnTriggerEnter2D(collider)
    if collider.tag == "Player":
        switch mItemType:
            Heal: PlayerStats.Heal(maxHp * 0.2)
            Magnet: CollectAllGems()
            Bomb: DamageAllEnemies(50)
            Gold: SessionResult.AddGold(mValue)
        ReturnToPool()
```

---

## 10. 무한 맵 시스템

### 10.1 설계 방식

타일맵이 아닌 **청크 기반 스프라이트 반복**으로 구현.
큰 배경 스프라이트(예: 512×512px)를 3×3 그리드로 배치하고, 플레이어 이동에 따라 재배치.

```
InfiniteMap : MonoBehaviour
├── mChunkPrefab: GameObject         # 배경 청크 (스프라이트)
├── mChunkSize: float = 10f          # 월드 유닛 기준
├── mChunks: GameObject[3,3]         # 3×3 그리드
├── mCurrentChunkCenter: Vector2Int
│
├── Start()
│   # 3×3 그리드 초기 배치
│   for x in -1..1, y in -1..1:
│       mChunks[x+1,y+1] = Instantiate(chunk, offset)
│
├── Update()
│   playerChunk = WorldToChunk(player.position)
│   if playerChunk != mCurrentChunkCenter:
│       RecenterChunks(playerChunk)
│
└── RecenterChunks(newCenter)
    # 이탈한 청크를 반대쪽으로 재배치
    # 장식 오브젝트 (나무, 바위)를 랜덤 재배치
```

### 10.2 장식 오브젝트

각 청크에 2~5개의 장식 오브젝트를 랜덤 배치. 충돌 없음(비주얼 전용).

---

## 11. UI 시스템

### 11.1 InGame HUD

```
HudController : MonoBehaviour
├── mHpBar: Slider
├── mExpBar: Slider
├── mTimeText: TMP_Text
├── mSkillIcons: Image[6]           # 보유 스킬 아이콘
├── mPlayerStats: PlayerStats
├── mWaveManager: WaveManager
│
└── Update()
    mHpBar.value = mPlayerStats.mCurrentHp / mPlayerStats.mMaxHp
    mExpBar.value = mPlayerStats.mCurrentExp / mPlayerStats.mExpToNextLevel
    mTimeText.text = FormatTime(mWaveManager.GetElapsedTime())
    UpdateSkillIcons()
```

### 11.2 레벨업 스킬선택 UI

```
LevelUpUI : MonoBehaviour
├── mCardPrefab: GameObject
├── mCardContainer: Transform
├── mCards: LevelUpCard[3]
│
├── Show(choices: List<SkillChoice>)
│   Time.timeScale = 0              # 게임 일시정지
│   for i in 0..2:
│       mCards[i].Setup(choices[i])
│   gameObject.SetActive(true)
│
├── OnCardSelected(choice)
│   SkillManager.AddOrUpgradeSkill(choice)
│   Time.timeScale = 1
│   gameObject.SetActive(false)

LevelUpCard : MonoBehaviour
├── mIcon: Image
├── mNameText: TMP_Text
├── mLevelText: TMP_Text           # "Lv 2" 또는 "NEW" 또는 "진화!"
├── mDescText: TMP_Text
├── mBorderImage: Image             # 진화 시 금색
├── mChoice: SkillChoice
│
├── Setup(choice)
└── OnClick()
    LevelUpUI.OnCardSelected(mChoice)
```

### 11.3 결과 화면

```
ResultScreenUI : MonoBehaviour
├── mResultText: TMP_Text           # "클리어!" 또는 "패배"
├── mKillCountText: TMP_Text
├── mLevelText: TMP_Text
├── mPlayTimeText: TMP_Text
├── mReturnButton: Button
│
├── Initialize(sessionResult)
└── OnReturnClicked()
    GameManager.ReturnToLobby()
```

---

## 12. 데이터 설계 (ScriptableObject)

### 12.1 SkillData

```
SkillData : ScriptableObject
├── mSkillId: string                # "MagicBolt", "Fireball" 등
├── mSkillName: string              # "매직 볼트"
├── mDescription: string            # "직선 투사체 발사"
├── mIcon: Sprite                   # 아이콘 (플레이스홀더)
├── mSkillType: SkillType           # Active, Passive
├── mSkillCategory: SkillCategory   # Projectile, Orbit, Area, Chain, Zone, Buff
│
├── [레벨별 수치 - 배열 인덱스가 레벨]
│   ├── mBaseDamage: float[5]       # Lv1~5 데미지
│   ├── mCooldown: float[5]         # Lv1~5 쿨다운
│   ├── mSpecialValue: float[5]     # 스킬별 특수값 (투사체 수, 검 수, 반경, 감속률, 체인 수, 장판 크기)
│
├── mProjectilePrefab: GameObject   # 투사체형 스킬에서 사용
└── mEffectPrefab: GameObject       # 이펙트 프리팹
```

### 12.2 EnemyData

```
EnemyData : ScriptableObject
├── mEnemyId: string
├── mEnemyName: string
├── mSprite: Sprite
├── mAiType: EnemyAiType            # Chase, FastChase, Ranged, EliteSplit
├── mBaseHp: int
├── mBaseDamage: int
├── mMoveSpeed: float
├── mDropExp: int
├── mIsBoss: bool
│
├── [원거리 전용]
│   ├── mAttackRange: float
│   ├── mAttackInterval: float
│   └── mProjectilePrefab: GameObject
│
└── [엘리트 전용]
    ├── mSplitCount: int
    └── mSplitEnemyData: EnemyData
```

### 12.3 WaveData

위 7.1 참조. 5분 세션의 웨이브 테이블 전체를 하나의 ScriptableObject로 정의.

### 12.4 EvolutionData

```
EvolutionData : ScriptableObject
├── mRequiredActiveSkill: SkillData
├── mRequiredPassiveSkill: SkillData
├── mResultSkill: SkillData         # 진화 결과 스킬
├── mEvolutionName: string          # "메테오 스트라이크"
```

---

## 13. 오브젝트 풀링

### 13.1 범용 ObjectPool

```
ObjectPool<T> where T : MonoBehaviour
├── mPrefab: GameObject
├── mPool: Queue<T>
├── mActiveCount: int
├── mParent: Transform              # 풀 오브젝트 부모 (Hierarchy 정리)
│
├── Initialize(prefab, initialCount)
├── Get(): T
│   if mPool.Count > 0:
│       obj = mPool.Dequeue()
│       obj.gameObject.SetActive(true)
│   else:
│       obj = Instantiate(mPrefab).GetComponent<T>()
│   mActiveCount++
│   return obj
│
├── Return(obj)
│   obj.gameObject.SetActive(false)
│   mPool.Enqueue(obj)
│   mActiveCount--
│
└── GetActiveCount(): int
```

### 13.2 풀링 대상

| 대상 | 초기 생성 수 | 최대 수 |
|------|-------------|---------|
| 적 (종류별) | 30 | 150 (전체) |
| 투사체 | 50 | 200 |
| 경험치 보석 | 100 | 500 |
| 드롭 아이템 | 10 | 30 |
| 데미지 텍스트 | 20 | 50 |
| 히트 이펙트 | 20 | 50 |

---

## 14. 리소스 제작 (`/gi` Skill 활용)

에셋 스토어 대신 `/gi` Skill(ComfyUI)로 게임 스프라이트를 직접 생성한다.
`/gi` 기능 테스트를 겸하므로, 가능한 한 모든 비주얼 리소스를 `/gi`로 제작한다.

### 14.1 리소스 제작 전략

**`/gi`로 생성하는 리소스 (주력):**
- 캐릭터/몬스터/보스 스프라이트
- 투사체/스킬 이펙트 이미지
- 배경 타일, 장식 오브젝트
- 스킬 아이콘
- UI 배경/프레임

**코드로 처리하는 리소스 (보조):**
- 보스 아레나 벽 (원형 EdgeCollider2D, 반투명 링은 코드로 생성)
- 조이스틱 UI (단순 원형, Unity UI Image로 충분)
- HP/EXP 바 (Unity UI Slider 기본 컴포넌트)
- 데미지 텍스트 (TextMesh Pro)

### 14.2 `/gi` 생성 리소스 목록

각 리소스에 대해 마일스톤 진입 전에 `/gi`로 생성하고 프로젝트에 배치한다.

#### M0 (리소스 준비) — M1 진입 전

| 대상 | 스타일 | 크기 | 배경 | 비고 |
|------|--------|------|------|------|
| **플레이어 "루드"** | 2D 탑다운, 로브 입은 풋내기 마법사 | 128×128 | 투명 | 4방향 or 정면 1장 (MVP) |
| **슬라임** | 2D 탑다운, 초록 젤리형 | 96×96 | 투명 | |
| **매직 볼트 투사체** | 하늘색 마법 구체, 빛나는 이펙트 | 64×64 | 투명 | |
| **배경 타일 (숲/초원)** | 탑다운 풀밭 텍스처 | 512×512 | 불투명 | 타일링 가능하게 |
| **배경 타일 변형** | 꽃/돌 포함 풀밭 변형 | 512×512 | 불투명 | 2~3종 |
| **장식 — 나무** | 탑다운 판타지 나무 | 128×128 | 투명 | |
| **장식 — 바위** | 탑다운 돌/바위 | 96×96 | 투명 | |
| **경험치 보석 (소)** | 작은 초록 보석, 빛나는 효과 | 32×32 | 투명 | |
| **경험치 보석 (대)** | 큰 초록 보석, 빛나는 효과 | 64×64 | 투명 | |

#### M2 진입 전 (추가)

| 대상 | 스타일 | 크기 | 배경 | 비고 |
|------|--------|------|------|------|
| **스킬 아이콘 — 매직 볼트** | 판타지 아이콘, 파란 빛 구체 | 128×128 | 투명 | |
| **스킬 아이콘 — 회전 검** | 판타지 아이콘, 은빛 검 | 128×128 | 투명 | |
| **스킬 아이콘 — 파이어볼** | 판타지 아이콘, 불꽃 구체 | 128×128 | 투명 | |
| **스킬 아이콘 — 아이스 스피어** | 판타지 아이콘, 얼음 창 | 128×128 | 투명 | |
| **스킬 아이콘 — 라이트닝** | 판타지 아이콘, 번개 | 128×128 | 투명 | |
| **스킬 아이콘 — 포이즌 클라우드** | 판타지 아이콘, 보라 연기 | 128×128 | 투명 | |
| **스킬 아이콘 — 마나 서지** | 판타지 아이콘, 마법진 | 128×128 | 투명 | |
| **스킬 아이콘 — 원소 핵** | 판타지 아이콘, 다색 원소 구슬 | 128×128 | 투명 | |
| **스킬 아이콘 — 마법 갑옷** | 판타지 아이콘, 빛나는 갑옷 | 128×128 | 투명 | |
| **스킬 아이콘 — 신속의 부츠** | 판타지 아이콘, 날개달린 부츠 | 128×128 | 투명 | |
| **회복 고기** | 고기 아이템 아이콘 | 64×64 | 투명 | |
| **자석** | 빨간 말굽자석 | 64×64 | 투명 | |
| **폭탄** | 검은 폭탄 | 64×64 | 투명 | |
| **골드 코인** | 금화 | 32×32 | 투명 | |

#### M3 진입 전 (추가)

| 대상 | 스타일 | 크기 | 배경 | 비고 |
|------|--------|------|------|------|
| **늑대** | 2D 탑다운, 회색 늑대 | 96×96 | 투명 | |
| **스켈레톤** | 2D 탑다운, 해골 전사 | 96×96 | 투명 | |
| **엘리트 슬라임** | 2D 탑다운, 크고 진한 초록 슬라임 | 128×128 | 투명 | |
| **파이어볼 투사체** | 불꽃 구체 | 64×64 | 투명 | |
| **아이스 스피어 투사체** | 얼음 화살 | 64×64 | 투명 | |
| **회전 검** | 은빛 검 (옆모습) | 32×96 | 투명 | |
| **포이즌 클라우드 장판** | 보라색 독안개 | 256×256 | 투명 | 반투명 처리 |
| **스켈레톤 화살** | 뼈 화살 투사체 | 48×48 | 투명 | |

#### M4 진입 전 (추가)

| 대상 | 스타일 | 크기 | 배경 | 비고 |
|------|--------|------|------|------|
| **미니보스 3종** | 탑다운 판타지 보스, 큰 크기 | 192×192 | 투명 | 각각 다른 외형 |
| **최종 보스** | 탑다운 드래곤/마왕 계열 | 256×256 | 투명 | |
| **진화 아이콘 — 메테오 스트라이크** | 판타지 아이콘, 운석 낙하 | 128×128 | 투명 | |
| **진화 아이콘 — 썬더 스톰** | 판타지 아이콘, 폭풍 번개 | 128×128 | 투명 | |
| **진화 아이콘 — 블레이드 배리어** | 판타지 아이콘, 검 회오리 | 128×128 | 투명 | |
| **메테오 투사체** | 불타는 운석 | 128×128 | 투명 | |
| **보스 투사체** | 어둠 에너지 구체 | 64×64 | 투명 | |

#### M5 진입 전 (추가)

| 대상 | 스타일 | 크기 | 배경 | 비고 |
|------|--------|------|------|------|
| **타이틀 로고** | "Vams2" 판타지 스타일 텍스트 | 512×256 | 투명 | |
| **UI 카드 프레임** | 판타지 RPG 스타일 카드 배경 | 256×384 | 반투명 | 레벨업 UI용 |
| **UI 버튼 프레임** | 판타지 RPG 버튼 | 256×96 | 투명 | 출격/로비 버튼용 |

### 14.3 `/gi` 생성 후 후처리

1. **배경 제거**: 생성된 이미지에서 투명 배경이 필요한 경우 후처리
2. **크기 조정**: 지정 크기에 맞게 리사이즈
3. **Import Settings**: Unity에서 Sprite (2D and UI), Filter Mode: Point (픽셀아트) 또는 Bilinear
4. **배치 경로**: `Assets/01_Contents/InGame/RES/Bundle/Textures/` 하위에 카테고리별 정리

```
Textures/
├── Player/          # 루드 스프라이트
├── Enemy/           # 슬라임, 늑대, 스켈레톤, 엘리트, 보스
├── Skill/           # 투사체, 이펙트 이미지
├── Icon/            # 스킬 아이콘, 진화 아이콘
├── Map/             # 배경 타일, 장식 오브젝트
├── Drop/            # 보석, 아이템
└── UI/              # 카드 프레임, 버튼, 로고

```

### 14.4 코드 생성 보조 리소스

`/gi`로 제작하지 않는 보조 리소스:

| 대상 | 처리 방식 |
|------|-----------|
| 보스 아레나 벽 | 코드로 반투명 원형 링 Texture2D 생성 |
| 조이스틱 바탕/손잡이 | Unity UI Image, 흰색 원형 스프라이트 (Unity 기본 제공) |
| HP/EXP 바 | Unity UI Slider, Fill 색상만 설정 |
| 데미지 숫자 팝업 | TextMesh Pro 텍스트 |
| 라이트닝 시각 효과 | LineRenderer 또는 코드로 직선 생성 |

---

## 15. 레이어 & 태그 설정

### 15.1 Sorting Layer (렌더링 순서)

| Layer | 순서 | 용도 |
|-------|------|------|
| Background | 0 | 배경 청크, 장식 |
| Drops | 1 | 경험치 보석, 드롭 아이템 |
| Enemies | 2 | 적 |
| Player | 3 | 플레이어 |
| Projectiles | 4 | 투사체, 스킬 이펙트 |
| UI_World | 5 | 데미지 텍스트, 체력바 |

### 15.2 Physics Layer (충돌 판정)

| Layer | 충돌 대상 |
|-------|-----------|
| Player | Enemy, EnemyProjectile |
| Enemy | PlayerProjectile, BossArena |
| PlayerProjectile | Enemy |
| EnemyProjectile | Player |
| Gem | Player (Trigger) |
| DropItem | Player (Trigger) |
| BossArena | Enemy, Player |

### 15.3 Tag

`Player`, `Enemy`, `Gem`, `DropItem`, `BossArena`, `Projectile`

---

## 16. 카메라 설정

```
Main Camera
├── Projection: Orthographic
├── Size: 10 (세로 기준, 세로뷰에서 위아래 10 유닛)
├── Position: 플레이어 추적
├── CameraFollow : MonoBehaviour
│   ├── mTarget: Transform (Player)
│   ├── mSmoothSpeed: float = 0.125f
│   └── LateUpdate()
│       desiredPos = Vector3(target.x, target.y, -10)
│       transform.position = Lerp(current, desiredPos, mSmoothSpeed)
```

---

## 17. MVP 마일스톤별 구현 범위

### M0: 리소스 준비 (M1 진입 전)

`/gi` Skill로 M1에 필요한 핵심 스프라이트를 생성한다.

| 리소스 | `/gi` 생성 |
|--------|-----------|
| 플레이어 "루드" | 2D 탑다운 풋내기 마법사 (128×128, 투명 배경) |
| 슬라임 | 2D 탑다운 초록 슬라임 (96×96, 투명 배경) |
| 매직 볼트 투사체 | 하늘색 마법 구체 (64×64, 투명 배경) |
| 배경 타일 (기본) | 탑다운 풀밭 텍스처 (512×512, 타일링 가능) |
| 배경 타일 (변형 2~3종) | 꽃/돌 포함 풀밭 변형 (512×512) |
| 장식 — 나무 | 탑다운 판타지 나무 (128×128, 투명 배경) |
| 장식 — 바위 | 탑다운 돌/바위 (96×96, 투명 배경) |
| 경험치 보석 (소/대) | 초록 보석 (32×32, 64×64, 투명 배경) |

### M1: 이동 + 자동공격 + 적 스폰

| 구현 대상 | 스크립트 |
|-----------|----------|
| 플로팅 조이스틱 | FloatingJoystick |
| 플레이어 이동 | PlayerMovement |
| 플레이어 기본 스탯 | PlayerStats (기본값만) |
| 자동공격 (매직 볼트만) | MagicBoltSkill, Projectile |
| 적 스폰 (슬라임만) | EnemySpawner, EnemyBehaviour (Chase AI) |
| 적 체력/피격/사망 | EnemyHealth |
| 오브젝트 풀링 | ObjectPool |
| 카메라 추적 | CameraFollow |
| 무한 맵 | InfiniteMap |
| 씬 | InGameScene만 |

### M2: 경험치 + 레벨업 UI

| 구현 대상 | 스크립트 |
|-----------|----------|
| 경험치 보석 드롭/흡수 | ExpGem, ExpCollector, EnemyDrop |
| 레벨업 판정 | PlayerStats.AddExp() |
| 레벨업 UI (3카드) | LevelUpUI, LevelUpCard |
| HUD (HP, EXP, 시간) | HudController |
| 스킬 선택지 생성 | SkillManager.GetRandomChoices() |

### M3: 스킬 6종 + 패시브 4종

| 구현 대상 | 스크립트 |
|-----------|----------|
| 스킬 슬롯 관리 | SkillManager |
| 액티브 스킬 6종 | 각 Skill 클래스 |
| 패시브 스킬 4종 | SkillManager.ApplyPassive() |
| 스킬 데이터 | SkillData ScriptableObject ×10 |
| 적 추가 (늑대, 스켈레톤, 엘리트) | EnemyData ×4, EnemyBehaviour 확장 |

### M4: 스킬 진화 + 보스

| 구현 대상 | 스크립트 |
|-----------|----------|
| 스킬 진화 체크/실행 | SkillManager.CheckEvolution() |
| 진화 스킬 3종 | MeteorStrike, ThunderStorm, BladeBarrier |
| 보스 아레나 | BossArenaManager |
| 보스 적 (원거리 공격) | EnemyBehaviour (Ranged AI 확장) |
| 보스 데이터 | EnemyData ×4 (미니보스 3 + 최종보스 1) |

### M5: 5분 세션 완성

| 구현 대상 | 스크립트 |
|-----------|----------|
| 웨이브 테이블 | WaveData (전체 5분 구성) |
| 웨이브 매니저 | WaveManager |
| 세션 종료 판정 | GameManager.EndGame() |
| 결과 화면 | ResultScreenUI |
| 로비 화면 | LobbyUI (기본) |
| 타이틀 화면 | TitleUI (기본) |
| 씬 전환 | SceneLoader |
| 드롭 아이템 | DropItem (회복, 자석, 폭탄) |
| 게임 매니저 | GameManager (전체 플로우) |
