using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Combat;

namespace Vams2.InGame.Skill
{
    // 파이어볼: 랜덤 방향으로 폭발 투사체 발사
    // SpecialValue = 폭발 반경
    public class FireballSkill : SkillBase
    {
        [SerializeField] private GameObject mProjectilePrefab;
        [SerializeField] private float mProjectileSpeed = 7f;
        [SerializeField] private float mProjectileLifeTime = 2f;

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

            SpawnFireball(direction, result.mFinalDamage);
        }

        private void SpawnFireball(Vector2 direction, float damage)
        {
            if (mProjectilePrefab == null) return;

            GameObject go = Instantiate(mProjectilePrefab, mPlayerTransform.position, Quaternion.identity);
            go.layer = LayerMask.NameToLayer("PlayerProjectile");

            Projectile proj = go.GetComponent<Projectile>();
            if (proj == null) proj = go.AddComponent<Projectile>();

            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) rb = go.AddComponent<Rigidbody2D>();

            proj.Initialize(damage, mProjectileSpeed, direction, mProjectileLifeTime);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
