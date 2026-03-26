using System;
using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Enemy;

namespace Vams2.InGame.Wave
{
    // 시간 기반 웨이브 관리
    // WaveData의 엔트리에 따라 적을 스폰하고 보스를 관리한다
    public class WaveManager : MonoBehaviour
    {
        private WaveData mWaveData;
        private EnemySpawner mEnemySpawner;
        private float mElapsedTime;
        private bool mIsSessionActive;
        private bool mIsBossActive;

        // 각 엔트리별 스폰 타이머
        private float[] mEntryTimers;

        public Action OnSessionComplete;
        public Action<EnemyData> OnBossSpawn;

        public float ElapsedTime => mElapsedTime;
        public float SessionDuration => mWaveData != null ? mWaveData.mSessionDuration : 300f;
        public bool IsBossActive => mIsBossActive;

        public void Initialize(WaveData waveData, EnemySpawner spawner)
        {
            mWaveData = waveData;
            mEnemySpawner = spawner;
            mElapsedTime = 0f;
            mIsSessionActive = true;
            mIsBossActive = false;

            if (mWaveData != null)
            {
                mEntryTimers = new float[mWaveData.mEntries.Count];
            }
        }

        private void Update()
        {
            if (!mIsSessionActive || mWaveData == null)
            {
                return;
            }

            mElapsedTime += Time.deltaTime;

            // 세션 시간 초과 → 클리어
            if (mElapsedTime >= mWaveData.mSessionDuration)
            {
                mIsSessionActive = false;
                if (OnSessionComplete != null)
                {
                    OnSessionComplete();
                }
                return;
            }

            // 보스 활성 중에는 일반 스폰 중단
            if (mIsBossActive)
            {
                return;
            }

            // 웨이브 엔트리 처리
            for (int i = 0; i < mWaveData.mEntries.Count; i++)
            {
                WaveEntry entry = mWaveData.mEntries[i];

                if (mElapsedTime < entry.mStartTime || mElapsedTime > entry.mEndTime)
                {
                    continue;
                }

                if (entry.mIsBoss)
                {
                    // 보스 스폰 (1회만)
                    if (mEntryTimers[i] == 0f)
                    {
                        mEntryTimers[i] = 1f; // 스폰 완료 표시
                        SpawnBoss(entry);
                    }
                }
                else
                {
                    // 일반 적 스폰
                    mEntryTimers[i] += Time.deltaTime;
                    if (mEntryTimers[i] >= entry.mSpawnInterval)
                    {
                        mEntryTimers[i] = 0f;
                        for (int j = 0; j < entry.mSpawnCount; j++)
                        {
                            mEnemySpawner.SpawnEnemy(entry.mEnemyData);
                        }
                    }
                }
            }

            // EnemySpawner에 시간 전달 (체력 스케일링용)
            mEnemySpawner.SetElapsedTime(mElapsedTime);
        }

        private EnemyData mCurrentBossData;

        private void SpawnBoss(WaveEntry entry)
        {
            mIsBossActive = true;
            mCurrentBossData = entry.mEnemyData;

            Transform player = GameObject.FindWithTag("Player").transform;

            if (BossArenaManager.Instance != null)
            {
                BossArenaManager.Instance.CreateArena(player.position);
            }

            Vector3 bossSpawnPos = player.position + new Vector3(0f, 3f, 0f);
            SpawnBossAtPosition(entry.mEnemyData, bossSpawnPos);

            if (OnBossSpawn != null)
            {
                OnBossSpawn(entry.mEnemyData);
            }
        }

        private void SpawnBossAtPosition(EnemyData data, Vector3 position)
        {
            // EnemySpawner의 랜덤 위치 대신 직접 지정 위치에 스폰
            GameObject enemyPrefab = mEnemySpawner.GetEnemyPrefab();
            if (enemyPrefab == null) return;

            GameObject enemyGo = Instantiate(enemyPrefab, position, Quaternion.identity);
            Enemy.EnemyBehaviour behaviour = enemyGo.GetComponent<Enemy.EnemyBehaviour>();
            if (behaviour != null)
            {
                behaviour.Initialize(data, GameObject.FindWithTag("Player").transform, 1f);
                behaviour.SetOnDeathCallback(OnBossEnemyDeath);
            }
        }

        private void OnBossEnemyDeath()
        {
            bool isFinalBoss = mCurrentBossData != null && mCurrentBossData.mEnemyId == "final_boss";
            OnBossDefeated();

            if (isFinalBoss)
            {
                // 최종 보스 처치 → 2초 후 클리어
                mIsSessionActive = false;
                Invoke("DelayedSessionComplete", 2f);
            }
        }

        private void DelayedSessionComplete()
        {
            if (OnSessionComplete != null)
            {
                OnSessionComplete();
            }
        }

        public void OnBossDefeated()
        {
            mIsBossActive = false;

            if (BossArenaManager.Instance != null)
            {
                BossArenaManager.Instance.DestroyArena();
            }
        }
    }
}
