using UnityEngine;

namespace Vams2.InGame.Map
{
    // 3x3 청크 그리드로 무한 맵 구현
    // 플레이어 이동에 따라 이탈한 청크를 반대쪽으로 재배치
    public class InfiniteMap : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private float mChunkSize = 10f;
        [SerializeField] private Transform mPlayer;

        [Header("배경 스프라이트")]
        [SerializeField] private Sprite[] mTileSprites; // 기본 + 변형 타일
        [SerializeField] private Sprite[] mDecoSprites; // 나무, 바위 등 장식

        [Header("장식")]
        [SerializeField] private int mDecoPerChunk = 3;
        [SerializeField] private float mDecoMinScale = 0.3f;
        [SerializeField] private float mDecoMaxScale = 0.6f;

        private GameObject[,] mChunks;
        private Vector2Int mCurrentCenter;

        public void SetPlayer(Transform player)
        {
            mPlayer = player;
        }

        public void SetSprites(Sprite[] tileSprites, Sprite[] decoSprites)
        {
            mTileSprites = tileSprites;
            mDecoSprites = decoSprites;
        }

        private void Start()
        {
            mChunks = new GameObject[3, 3];
            mCurrentCenter = WorldToChunk(mPlayer != null ? mPlayer.position : Vector3.zero);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int chunkPos = new Vector2Int(mCurrentCenter.x + x, mCurrentCenter.y + y);
                    mChunks[x + 1, y + 1] = CreateChunk(chunkPos);
                }
            }
        }

        private void Update()
        {
            if (mPlayer == null)
            {
                return;
            }

            Vector2Int playerChunk = WorldToChunk(mPlayer.position);
            if (playerChunk != mCurrentCenter)
            {
                RecenterChunks(playerChunk);
            }
        }

        private Vector2Int WorldToChunk(Vector3 worldPos)
        {
            int cx = Mathf.RoundToInt(worldPos.x / mChunkSize);
            int cy = Mathf.RoundToInt(worldPos.y / mChunkSize);
            return new Vector2Int(cx, cy);
        }

        private void RecenterChunks(Vector2Int newCenter)
        {
            Vector2Int delta = newCenter - mCurrentCenter;
            mCurrentCenter = newCenter;

            // 기존 청크 중 재사용 가능한 것과 재배치 필요한 것 분류
            GameObject[,] newChunks = new GameObject[3, 3];

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int oldX = x + delta.x + 1;
                    int oldY = y + delta.y + 1;

                    if (oldX >= 0 && oldX < 3 && oldY >= 0 && oldY < 3)
                    {
                        // 기존 청크 재사용
                        newChunks[x + 1, y + 1] = mChunks[oldX, oldY];
                        mChunks[oldX, oldY] = null;
                    }
                }
            }

            // 남은 이전 청크 제거
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (mChunks[x, y] != null)
                    {
                        Destroy(mChunks[x, y]);
                    }
                }
            }

            // 비어있는 슬롯에 새 청크 생성
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (newChunks[x + 1, y + 1] == null)
                    {
                        Vector2Int chunkPos = new Vector2Int(mCurrentCenter.x + x, mCurrentCenter.y + y);
                        newChunks[x + 1, y + 1] = CreateChunk(chunkPos);
                    }
                }
            }

            mChunks = newChunks;
        }

        private GameObject CreateChunk(Vector2Int chunkPos)
        {
            GameObject chunk = new GameObject("Chunk_" + chunkPos.x + "_" + chunkPos.y);
            chunk.transform.SetParent(transform);

            Vector3 worldPos = new Vector3(
                chunkPos.x * mChunkSize,
                chunkPos.y * mChunkSize,
                0f);
            chunk.transform.position = worldPos;

            // 배경 타일
            GameObject tileGo = new GameObject("Tile");
            tileGo.transform.SetParent(chunk.transform, false);
            SpriteRenderer tileSr = tileGo.AddComponent<SpriteRenderer>();
            tileSr.sortingLayerName = "Background";
            tileSr.sortingOrder = 0;

            if (mTileSprites != null && mTileSprites.Length > 0)
            {
                int tileIdx = Random.Range(0, mTileSprites.Length);
                tileSr.sprite = mTileSprites[tileIdx];
            }

            // 스프라이트를 청크 크기에 맞게 스케일
            if (tileSr.sprite != null)
            {
                float spriteWidth = tileSr.sprite.bounds.size.x;
                float spriteHeight = tileSr.sprite.bounds.size.y;
                tileGo.transform.localScale = new Vector3(
                    mChunkSize / spriteWidth,
                    mChunkSize / spriteHeight,
                    1f);
            }

            // 장식 오브젝트 배치
            SpawnDecorations(chunk.transform, chunkPos);

            return chunk;
        }

        private void SpawnDecorations(Transform chunkTransform, Vector2Int chunkPos)
        {
            if (mDecoSprites == null || mDecoSprites.Length == 0)
            {
                return;
            }

            // 시드 기반 랜덤 (같은 청크 위치면 같은 장식)
            int seed = chunkPos.x * 73856093 ^ chunkPos.y * 19349663;
            Random.State prevState = Random.state;
            Random.InitState(seed);

            int decoCount = Random.Range(mDecoPerChunk - 1, mDecoPerChunk + 2);

            for (int i = 0; i < decoCount; i++)
            {
                GameObject decoGo = new GameObject("Deco_" + i);
                decoGo.transform.SetParent(chunkTransform, false);

                float offsetX = Random.Range(-mChunkSize * 0.4f, mChunkSize * 0.4f);
                float offsetY = Random.Range(-mChunkSize * 0.4f, mChunkSize * 0.4f);
                decoGo.transform.localPosition = new Vector3(offsetX, offsetY, 0f);

                float scale = Random.Range(mDecoMinScale, mDecoMaxScale);
                decoGo.transform.localScale = Vector3.one * scale;

                SpriteRenderer decoSr = decoGo.AddComponent<SpriteRenderer>();
                decoSr.sortingLayerName = "Background";
                decoSr.sortingOrder = 1;
                decoSr.sprite = mDecoSprites[Random.Range(0, mDecoSprites.Length)];
            }

            Random.state = prevState;
        }
    }
}
