using UnityEngine;

namespace Vams2.Data
{
    public enum SkillType
    {
        Active = 0,
        Passive,
        Evolution,
        Max
    }

    public enum SkillCategory
    {
        Projectile = 0,
        Orbit,
        Area,
        Chain,
        Zone,
        Buff,
        Max
    }

    [CreateAssetMenu(fileName = "NewSkillData", menuName = "Vams2/SkillData")]
    public class SkillData : ScriptableObject
    {
        [Header("기본 정보")]
        public string mSkillId;
        public string mSkillName;
        public string mDescription;
        public Sprite mIcon;
        public SkillType mSkillType;
        public SkillCategory mSkillCategory;

        [Header("레벨별 수치 (인덱스 0=Lv1, 4=Lv5)")]
        public float[] mBaseDamage = new float[5];
        public float[] mCooldown = new float[5];
        public float[] mSpecialValue = new float[5]; // 투사체 수, 검 수, 반경, 감속률, 체인 수, 장판 크기

        [Header("프리팹")]
        public GameObject mProjectilePrefab;
        public GameObject mEffectPrefab;

        public float GetDamage(int level)
        {
            int idx = Mathf.Clamp(level - 1, 0, mBaseDamage.Length - 1);
            return mBaseDamage[idx];
        }

        public float GetCooldown(int level)
        {
            int idx = Mathf.Clamp(level - 1, 0, mCooldown.Length - 1);
            return mCooldown[idx];
        }

        public float GetSpecialValue(int level)
        {
            int idx = Mathf.Clamp(level - 1, 0, mSpecialValue.Length - 1);
            return mSpecialValue[idx];
        }
    }
}
