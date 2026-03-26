using UnityEngine;

namespace Vams2.InGame.Wave
{
    // 보스 등장 시 원형 벽을 생성하여 전투 영역을 제한
    public class BossArenaManager : MonoBehaviour
    {
        private static BossArenaManager mInstance;
        public static BossArenaManager Instance => mInstance;

        private GameObject mArenaInstance;
        private float mArenaRadius = 8f;
        private bool mIsActive;

        public bool IsActive => mIsActive;

        private void Awake()
        {
            mInstance = this;
            mIsActive = false;
        }

        public void CreateArena(Vector3 centerPos)
        {
            if (mIsActive) return;

            mIsActive = true;

            mArenaInstance = new GameObject("BossArena");
            mArenaInstance.tag = "BossArena";
            mArenaInstance.layer = LayerMask.NameToLayer("BossArena");
            mArenaInstance.transform.position = centerPos;

            // 원형 EdgeCollider2D로 벽 생성
            EdgeCollider2D edge = mArenaInstance.AddComponent<EdgeCollider2D>();

            int segments = 32;
            Vector2[] points = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * mArenaRadius;
            }
            edge.points = points;

            // 시각적 표시 (반투명 원)
            SpriteRenderer sr = mArenaInstance.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Drops";
            sr.sortingOrder = -1;
            sr.color = new Color(1f, 0f, 0f, 0.15f);

            // 코드로 원형 스프라이트 생성
            Texture2D tex = new Texture2D(256, 256);
            Color clear = new Color(0, 0, 0, 0);
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(128, 128));
                    if (dist > 120 && dist < 128)
                        tex.SetPixel(x, y, Color.white);
                    else
                        tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 16f);

            // Rigidbody2D (Static)
            Rigidbody2D rb = mArenaInstance.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }

        public void DestroyArena()
        {
            if (mArenaInstance != null)
            {
                Destroy(mArenaInstance);
                mArenaInstance = null;
            }
            mIsActive = false;
        }
    }
}
