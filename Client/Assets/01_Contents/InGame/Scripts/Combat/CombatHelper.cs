using UnityEngine;

namespace Vams2.InGame.Combat
{
    public struct DamageResult
    {
        public float mFinalDamage;
        public bool mIsCritical;

        public DamageResult(float finalDamage, bool isCritical)
        {
            mFinalDamage = finalDamage;
            mIsCritical = isCritical;
        }
    }

    public static class CombatHelper
    {
        // 스킬 데미지 계산: BaseDmg × (1 + SkillLv × 0.2)
        public static float CalculateSkillDamage(float baseDmg, int skillLevel)
        {
            return baseDmg * (1f + skillLevel * 0.2f);
        }

        // 전체 데미지 계산 (원소 보정, 크리티컬, 방어력 적용)
        public static DamageResult CalculateDamage(
            float baseDmg,
            float elementalDmgBonus,
            float critChance,
            float critMultiplier,
            int targetDefense)
        {
            // 1. 원소 보정
            float dmg = baseDmg * (1f + elementalDmgBonus);

            // 2. 크리티컬 판정
            bool isCrit = Random.value < critChance;
            if (isCrit)
            {
                dmg *= critMultiplier;
            }

            // 3. 방어력 감소: receivedDmg = rawDmg × (100 / (100 + DEF))
            dmg = dmg * (100f / (100f + targetDefense));

            // 최소 데미지 1
            if (dmg < 1f)
            {
                dmg = 1f;
            }

            return new DamageResult(dmg, isCrit);
        }

        // 회피 판정
        public static bool CheckDodge(float dodgeChance)
        {
            return Random.value < dodgeChance;
        }

        // 적 체력 스케일링: BaseHP × (1 + elapsedMinutes × 0.15)
        public static float CalculateScaledEnemyHp(float baseHp, float elapsedSeconds)
        {
            float elapsedMinutes = elapsedSeconds / 60f;
            return baseHp * (1f + elapsedMinutes * 0.15f);
        }

        // 경험치 요구량: 10 + (Lv × 5)
        public static int CalculateRequiredExp(int level)
        {
            return 10 + (level * 5);
        }
    }
}
