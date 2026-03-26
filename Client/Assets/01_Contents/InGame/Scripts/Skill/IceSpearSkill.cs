using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Combat;

namespace Vams2.InGame.Skill
{
    // 아이스 스피어: 관통 투사체 + 감속
    // SpecialValue = 감속률 (0.3 ~ 0.7)
    public class IceSpearSkill : SkillBase
    {
        [SerializeField] private GameObject mProjectilePrefab;
        [SerializeField] private float mProjectileSpeed = 8f;
        [SerializeField] private float mProjectileLifeTime = 3f;

        public void SetProjectilePrefab(GameObject prefab)
        {
            mProjectilePrefab = prefab;
        }

        protected override void Execute()
        {
            Transform target = FindClosestEnemy();
            if (target == null) return;

            Vector2 direction = ((Vector2)target.position - (Vector2)mPlayerTransform.position).normalized;
            float dmg = CombatHelper.CalculateSkillDamage(mSkillData.GetDamage(mLevel), mLevel);

            float elementalBonus = mPlayerStats != null ? mPlayerStats.ElementalDmgBonus : 0f;
            DamageResult result = CombatHelper.CalculateDamage(dmg, elementalBonus,
                mPlayerStats.CritChance, mPlayerStats.CritMultiplier, 0);

            float slowAmount = mSkillData.GetSpecialValue(mLevel);

            SpawnIceSpear(direction, result.mFinalDamage, slowAmount);
        }

        private void SpawnIceSpear(Vector2 direction, float damage, float slowAmount)
        {
            if (mProjectilePrefab == null) return;

            GameObject go = Instantiate(mProjectilePrefab, mPlayerTransform.position, Quaternion.identity);
            go.layer = LayerMask.NameToLayer("PlayerProjectile");

            Projectile proj = go.GetComponent<Projectile>();
            if (proj == null) proj = go.AddComponent<Projectile>();

            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) rb = go.AddComponent<Rigidbody2D>();

            // 관통 + 감속
            proj.Initialize(damage, mProjectileSpeed, direction, mProjectileLifeTime,
                isPiercing: true, slowAmount: slowAmount, slowDuration: 2f);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
