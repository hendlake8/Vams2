using UnityEngine;
using Vams2.InGame.Drop;

namespace Vams2.InGame.Enemy
{
    public class EnemyDrop : MonoBehaviour
    {
        private int mDropExp;

        private static GameObject mGemPrefabSmall;
        private static GameObject mGemPrefabLarge;
        private static GameObject mDropItemPrefab;

        private const float HEAL_DROP_CHANCE = 0.03f;
        private const float MAGNET_DROP_CHANCE = 0.02f;
        private const float BOMB_DROP_CHANCE = 0.02f;
        private const float GOLD_DROP_CHANCE = 0.10f;

        public static void SetGemPrefabs(GameObject small, GameObject large)
        {
            mGemPrefabSmall = small;
            mGemPrefabLarge = large;
        }

        public static void SetDropItemPrefab(GameObject prefab)
        {
            mDropItemPrefab = prefab;
        }

        public void Initialize(int dropExp)
        {
            mDropExp = dropExp;
        }

        public void SpawnDrops(Vector3 position)
        {
            SpawnExpGem(position);
            TrySpawnItem(position);
        }

        private void SpawnExpGem(Vector3 position)
        {
            bool isLarge = mDropExp >= 5;
            GameObject prefab = isLarge ? mGemPrefabLarge : mGemPrefabSmall;
            if (prefab == null) return;

            GameObject gemGo = Instantiate(prefab, position, Quaternion.identity);
            ExpGem gem = gemGo.GetComponent<ExpGem>();
            if (gem != null)
            {
                gem.Initialize(mDropExp, position);
            }
        }

        private void TrySpawnItem(Vector3 position)
        {
            if (mDropItemPrefab == null) return;

            float roll = Random.value;

            if (roll < HEAL_DROP_CHANCE)
            {
                SpawnItem(position, DropItemType.Heal, 0.2f);
            }
            else if (roll < HEAL_DROP_CHANCE + MAGNET_DROP_CHANCE)
            {
                SpawnItem(position, DropItemType.Magnet, 1f);
            }
            else if (roll < HEAL_DROP_CHANCE + MAGNET_DROP_CHANCE + BOMB_DROP_CHANCE)
            {
                SpawnItem(position, DropItemType.Bomb, 50f);
            }
            else if (roll < HEAL_DROP_CHANCE + MAGNET_DROP_CHANCE + BOMB_DROP_CHANCE + GOLD_DROP_CHANCE)
            {
                SpawnItem(position, DropItemType.Gold, 10f);
            }
        }

        private void SpawnItem(Vector3 position, DropItemType type, float value)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            GameObject itemGo = Instantiate(mDropItemPrefab, position + offset, Quaternion.identity);
            DropItem item = itemGo.GetComponent<DropItem>();
            if (item != null)
            {
                item.Initialize(type, value, position + offset);
            }
        }
    }
}
