using UnityEngine;
using Vams2.InGame.Drop;

namespace Vams2.InGame.Enemy
{
    public class EnemyDrop : MonoBehaviour
    {
        private int mDropExp;

        // 보석 프리팹 (외부에서 주입)
        private static GameObject mGemPrefabSmall;
        private static GameObject mGemPrefabLarge;

        public static void SetGemPrefabs(GameObject small, GameObject large)
        {
            mGemPrefabSmall = small;
            mGemPrefabLarge = large;
        }

        public void Initialize(int dropExp)
        {
            mDropExp = dropExp;
        }

        public void SpawnDrops(Vector3 position)
        {
            // 경험치 보석 스폰
            SpawnExpGem(position);
        }

        private void SpawnExpGem(Vector3 position)
        {
            bool isLarge = mDropExp >= 5;
            GameObject prefab = isLarge ? mGemPrefabLarge : mGemPrefabSmall;

            if (prefab == null)
            {
                return;
            }

            // 풀에서 가져오기 대신 간단히 Instantiate (추후 풀링으로 교체)
            GameObject gemGo = Instantiate(prefab, position, Quaternion.identity);
            ExpGem gem = gemGo.GetComponent<ExpGem>();
            if (gem != null)
            {
                gem.Initialize(mDropExp, position);
            }
        }
    }
}
