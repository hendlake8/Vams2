using UnityEngine;

namespace Vams2.Data
{
    public enum EnemyAiType
    {
        Chase = 0,
        FastChase,
        Ranged,
        EliteSplit,
        Max
    }

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Vams2/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("기본 정보")]
        public string mEnemyId;
        public string mEnemyName;
        public Sprite mSprite;
        public EnemyAiType mAiType;
        public bool mIsBoss;

        [Header("스탯")]
        public int mBaseHp = 10;
        public int mBaseDamage = 5;
        public float mMoveSpeed = 2.0f;
        public int mDropExp = 1;

        [Header("원거리 전용")]
        public float mAttackRange = 5f;
        public float mAttackInterval = 2f;
        public GameObject mProjectilePrefab;

        [Header("엘리트 전용")]
        public int mSplitCount = 2;
        public EnemyData mSplitEnemyData;

        [Header("접촉 데미지")]
        public float mContactDamageInterval = 0.5f;
    }
}
