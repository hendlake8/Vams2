using UnityEngine;
using Vams2.Core;
using Vams2.Data;
using Vams2.Input;
using Vams2.InGame.Enemy;
using Vams2.InGame.Map;
using Vams2.InGame.Player;
using Vams2.InGame.Skill;
using Vams2.InGame.Drop;
using Vams2.UI;

namespace Vams2.InGame
{
    // InGameScene의 진입점
    // 모든 게임 시스템을 초기화하고 연결한다
    public class GameSession : MonoBehaviour
    {
        [Header("프리팹")]
        [SerializeField] private GameObject mPlayerPrefab;
        [SerializeField] private GameObject mEnemyPrefab;
        [SerializeField] private GameObject mJoystickCanvasPrefab;
        [SerializeField] private GameObject mExpGemSmallPrefab;
        [SerializeField] private GameObject mExpGemLargePrefab;

        [Header("데이터")]
        [SerializeField] private EnemyData mSlimeData;
        [SerializeField] private SkillData mMagicBoltData;
        [SerializeField] private SkillData[] mAllActiveSkills;
        [SerializeField] private SkillData[] mAllPassiveSkills;

        [Header("UI")]
        [SerializeField] private GameObject mLevelUpUIPrefab;
        [SerializeField] private GameObject mHudPrefab;

        [Header("맵 설정")]
        [SerializeField] private Sprite[] mTileSprites;
        [SerializeField] private Sprite[] mDecoSprites;

        // 런타임 참조
        private GameObject mPlayerGo;
        private PlayerStats mPlayerStats;
        private PlayerMovement mPlayerMovement;
        private EnemySpawner mEnemySpawner;
        private InfiniteMap mInfiniteMap;
        private CameraFollow mCameraFollow;
        private SkillManager mSkillManager;
        private LevelUpUI mLevelUpUI;
        private HudController mHud;
        private float mSpawnTimer;
        private float mSpawnInterval = 1.0f;
        private float mElapsedTime;
        private int mKillCount;

        private void Start()
        {
            SetupPlayer();
            SetupJoystick();
            SetupMap();
            SetupCamera();
            SetupEnemySpawner();
            SetupDropSystem();
            SetupSkillManager();
            SetupLevelUpUI();
            SetupPlayerSkill();
            SetupHud();

            mElapsedTime = 0f;
            mKillCount = 0;
            mSpawnTimer = 0f;
        }

        private void SetupPlayer()
        {
            mPlayerGo = Instantiate(mPlayerPrefab, Vector3.zero, Quaternion.identity);
            mPlayerStats = mPlayerGo.GetComponent<PlayerStats>();
            mPlayerMovement = mPlayerGo.GetComponent<PlayerMovement>();
        }

        private void SetupJoystick()
        {
            if (mJoystickCanvasPrefab == null)
            {
                return;
            }

            GameObject joystickCanvas = Instantiate(mJoystickCanvasPrefab);
            FloatingJoystick joystick = joystickCanvas.GetComponentInChildren<FloatingJoystick>();
            if (joystick != null && mPlayerMovement != null)
            {
                mPlayerMovement.SetJoystick(joystick);
            }

            // EventSystem이 없으면 생성
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private void SetupMap()
        {
            GameObject mapGo = new GameObject("InfiniteMap");
            mInfiniteMap = mapGo.AddComponent<InfiniteMap>();
            mInfiniteMap.SetPlayer(mPlayerGo.transform);
            mInfiniteMap.SetSprites(mTileSprites, mDecoSprites);
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;

            // Main Camera가 없으면 생성
            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();

                // URP용 UniversalAdditionalCameraData 추가
                var urpCamData = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.backgroundColor = new Color(0.15f, 0.3f, 0.15f, 1f);

            mCameraFollow = cam.gameObject.GetComponent<CameraFollow>();
            if (mCameraFollow == null)
            {
                mCameraFollow = cam.gameObject.AddComponent<CameraFollow>();
            }
            mCameraFollow.SetTarget(mPlayerGo.transform);
        }

        private void SetupEnemySpawner()
        {
            GameObject spawnerGo = new GameObject("EnemySpawner");
            mEnemySpawner = spawnerGo.AddComponent<EnemySpawner>();
            mEnemySpawner.SetEnemyPrefab(mEnemyPrefab);
            mEnemySpawner.SetPlayer(mPlayerGo.transform);
        }

        private void SetupDropSystem()
        {
            EnemyDrop.SetGemPrefabs(mExpGemSmallPrefab, mExpGemLargePrefab);
        }

        private void SetupSkillManager()
        {
            GameObject managerGo = new GameObject("SkillManager");
            mSkillManager = managerGo.AddComponent<SkillManager>();
            mSkillManager.Initialize(mPlayerStats, mAllActiveSkills, mAllPassiveSkills);
        }

        private void SetupLevelUpUI()
        {
            if (mLevelUpUIPrefab != null)
            {
                GameObject uiGo = Instantiate(mLevelUpUIPrefab);
                mLevelUpUI = uiGo.GetComponent<LevelUpUI>();
            }
            else
            {
                // 프리팹 없으면 코드로 생성
                GameObject uiGo = new GameObject("LevelUpUI");
                mLevelUpUI = uiGo.AddComponent<LevelUpUI>();
            }

            if (mLevelUpUI != null)
            {
                mLevelUpUI.Initialize(mSkillManager);
            }

            // PlayerStats 레벨업 이벤트 → LevelUpUI 연결
            if (mPlayerStats != null)
            {
                mPlayerStats.OnLevelUp = OnPlayerLevelUp;
            }
        }

        private void OnPlayerLevelUp()
        {
            if (mSkillManager == null || mLevelUpUI == null)
            {
                return;
            }

            var choices = mSkillManager.GetRandomChoices(3);
            if (choices.Count > 0)
            {
                mLevelUpUI.Show(choices);
            }
        }

        private void SetupHud()
        {
            if (mHudPrefab != null)
            {
                GameObject hudGo = Instantiate(mHudPrefab);
                mHud = hudGo.GetComponent<HudController>();
            }
            else
            {
                GameObject hudGo = new GameObject("HudCanvas");
                mHud = hudGo.AddComponent<HudController>();
            }

            if (mHud != null)
            {
                mHud.Initialize(mPlayerStats, mSkillManager, GetElapsedTime);
            }
        }

        private float GetElapsedTime()
        {
            return mElapsedTime;
        }

        private void SetupPlayerSkill()
        {
            if (mMagicBoltData == null || mSkillManager == null)
            {
                return;
            }

            // 매직 볼트를 기본 스킬로 장착 (SkillManager를 통해)
            SkillChoice initialSkill = new SkillChoice();
            initialSkill.mChoiceType = SkillChoiceType.NewSkill;
            initialSkill.mSkillData = mMagicBoltData;
            initialSkill.mNextLevel = 1;
            mSkillManager.AddOrUpgradeSkill(initialSkill);
        }

        private void Update()
        {
            mElapsedTime += Time.deltaTime;

            if (mEnemySpawner != null)
            {
                mEnemySpawner.SetElapsedTime(mElapsedTime);

                // 간단한 슬라임 스폰 (M1: 웨이브 매니저 없이 타이머 기반)
                mSpawnTimer += Time.deltaTime;
                if (mSpawnTimer >= mSpawnInterval && mSlimeData != null)
                {
                    mSpawnTimer = 0f;
                    mEnemySpawner.SpawnEnemy(mSlimeData);
                }
            }

            // 세션 결과 업데이트
            if (GameManager.Instance != null && mPlayerStats != null)
            {
                GameManager.Instance.SessionResult.mPlayTime = mElapsedTime;
                GameManager.Instance.SessionResult.mPlayerLevel = mPlayerStats.Level;
            }
        }
    }
}
