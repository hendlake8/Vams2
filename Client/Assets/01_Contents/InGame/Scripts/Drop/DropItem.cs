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

        public void Initialize(DropItemType itemType, float value, Vector3 position)
        {
            mItemType = itemType;
            mValue = value;
            transform.position = position;
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
