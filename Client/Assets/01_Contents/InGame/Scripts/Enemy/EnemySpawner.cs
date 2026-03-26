using System.Collections.Generic;
using UnityEngine;
using Vams2.Data;

namespace Vams2.InGame.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject mEnemyPrefab;
        [SerializeField] private Transform mPlayer;
        [SerializeField] private float mSpawnRadius = 12f;
        [SerializeField] private int mMaxEnemies = 150;

        private List<GameObject> mActiveEnemies;
        private float mElapsedTime;

        // 스폰 요청 큐 (WaveManager에서 호출)
        private struct SpawnRequest
        {
            public EnemyData mData;
            public float mInterval;
            public float mTimer;
            public int mCountPerSpawn;
        }

        private List<SpawnRequest> mSpawnRequests;

        private void Awake()
        {
            mActiveEnemies = new List<GameObject>();
            mSpawnRequests = new List<SpawnRequest>();
            mElapsedTime = 0f;
        }

        public void SetPlayer(Transform player)
        {
            mPlayer = player;
        }

        public void SetEnemyPrefab(GameObject prefab)
        {
            mEnemyPrefab = prefab;
        }

        public void AddSpawnRequest(EnemyData data, float interval, int countPerSpawn)
        {
            SpawnRequest request = new SpawnRequest();
            request.mData = data;
            request.mInterval = interval;
            request.mTimer = 0f;
            request.mCountPerSpawn = countPerSpawn;
            mSpawnRequests.Add(request);
        }

        public void ClearSpawnRequests()
        {
            mSpawnRequests.Clear();
        }

        private void Update()
        {
            if (mPlayer == null)
            {
                return;
            }

            mElapsedTime += Time.deltaTime;

            // 비활성화된 적 제거
            CleanupInactiveEnemies();

            // 스폰 요청 처리
            for (int i = 0; i < mSpawnRequests.Count; i++)
            {
                SpawnRequest req = mSpawnRequests[i];
                req.mTimer += Time.deltaTime;

                if (req.mTimer >= req.mInterval)
                {
                    req.mTimer = 0f;

                    for (int j = 0; j < req.mCountPerSpawn; j++)
                    {
                        if (mActiveEnemies.Count >= mMaxEnemies)
                        {
                            break;
                        }
                        SpawnEnemy(req.mData);
                    }
                }

                mSpawnRequests[i] = req;
            }
        }

        public void SpawnEnemy(EnemyData data)
        {
            if (mActiveEnemies.Count >= mMaxEnemies)
            {
                return;
            }

            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemyGo = Instantiate(mEnemyPrefab, spawnPos, Quaternion.identity);

            EnemyBehaviour behaviour = enemyGo.GetComponent<EnemyBehaviour>();
            if (behaviour != null)
            {
                float hpScale = 1f + (mElapsedTime / 60f) * 0.15f;
                behaviour.Initialize(data, mPlayer, hpScale);
            }

            mActiveEnemies.Add(enemyGo);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * mSpawnRadius;
            return (Vector2)mPlayer.position + offset;
        }

        private void CleanupInactiveEnemies()
        {
            for (int i = mActiveEnemies.Count - 1; i >= 0; i--)
            {
                if (mActiveEnemies[i] == null || !mActiveEnemies[i].activeSelf)
                {
                    if (mActiveEnemies[i] != null)
                    {
                        Destroy(mActiveEnemies[i]);
                    }
                    mActiveEnemies.RemoveAt(i);
                }
            }
        }

        public int GetActiveEnemyCount()
        {
            return mActiveEnemies.Count;
        }

        public void SetElapsedTime(float time)
        {
            mElapsedTime = time;
        }
    }
}
