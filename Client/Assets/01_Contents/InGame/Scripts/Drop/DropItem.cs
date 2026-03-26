using UnityEngine;
using Vams2.InGame.Player;

namespace Vams2.InGame.Drop
{
    public enum DropItemType
    {
        Heal = 0,
        Magnet,
        Bomb,
        Gold,
        Max
    }

    public class DropItem : MonoBehaviour
    {
        [SerializeField] private DropItemType mItemType;
        [SerializeField] private float mValue;

        private SpriteRenderer mSpriteRenderer;
        private float mAnimTimer;
        private Vector3 mBasePosition;
        private Color mPulseColor;

        public void Initialize(DropItemType itemType, float value, Vector3 position)
        {
            mItemType = itemType;
            mValue = value;
            transform.position = position;
            mBasePosition = position;
            mAnimTimer = Random.Range(0f, Mathf.PI * 2f);

            mSpriteRenderer = GetComponent<SpriteRenderer>();
            if (mSpriteRenderer != null)
            {
                mSpriteRenderer.sortingLayerName = "Drops";
                string spriteName = GetSpriteNameForType(itemType);
                Sprite spr = Resources.Load<Sprite>("Sprites/Drop/" + spriteName);
                if (spr != null) mSpriteRenderer.sprite = spr;
            }

            mPulseColor = GetPulseColorForType(itemType);
        }

        private void Update()
        {
            mAnimTimer += Time.deltaTime * 3.5f;

            // 색상 펄스
            if (mSpriteRenderer != null)
            {
                float pulse = 0.6f + 0.4f * Mathf.Sin(mAnimTimer);
                mSpriteRenderer.color = Color.Lerp(Color.white, mPulseColor, pulse);
            }

            // 바운스
            float bounce = Mathf.Sin(mAnimTimer * 0.7f) * 0.12f;
            transform.position = mBasePosition + new Vector3(0f, bounce, 0f);
        }

        private Color GetPulseColorForType(DropItemType type)
        {
            switch (type)
            {
                case DropItemType.Heal: return new Color(1f, 0.5f, 0.5f);
                case DropItemType.Magnet: return new Color(1f, 0.3f, 0.3f);
                case DropItemType.Bomb: return new Color(1f, 0.6f, 0f);
                case DropItemType.Gold: return new Color(1f, 1f, 0.3f);
                default: return Color.white;
            }
        }

        private string GetSpriteNameForType(DropItemType type)
        {
            switch (type)
            {
                case DropItemType.Heal: return "drop_heal_meat";
                case DropItemType.Magnet: return "drop_magnet";
                case DropItemType.Bomb: return "drop_bomb";
                case DropItemType.Gold: return "drop_gold_coin";
                default: return "drop_heal_meat";
            }
        }

        public void OnPickedUp(PlayerStats stats)
        {
            switch (mItemType)
            {
                case DropItemType.Heal:
                    stats.Heal(0.2f);
                    break;
                case DropItemType.Magnet:
                    CollectAllGems();
                    break;
                case DropItemType.Bomb:
                    DamageAllEnemies(50);
                    break;
                case DropItemType.Gold:
                    // SessionResult에 골드 추가 (추후 구현)
                    break;
            }

            gameObject.SetActive(false);
        }

        private void CollectAllGems()
        {
            ExpGem[] gems = FindObjectsByType<ExpGem>(FindObjectsSortMode.None);
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                return;
            }
            Transform playerTransform = player.transform;
            for (int i = 0; i < gems.Length; i++)
            {
                if (gems[i].gameObject.activeSelf)
                {
                    gems[i].StartMoveToPlayer(playerTransform);
                }
            }
        }

        private void DamageAllEnemies(int damage)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            for (int i = 0; i < enemies.Length; i++)
            {
                var health = enemies[i].GetComponent<Vams2.InGame.Enemy.EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }
        }
    }
}
