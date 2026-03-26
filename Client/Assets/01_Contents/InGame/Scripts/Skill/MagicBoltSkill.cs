using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Combat;

namespace Vams2.InGame.Skill
{
    // 매직 볼트: 가장 가까운 적에게 직선 투사체 발사
    // 레벨업 시 투사체 수 증가
    public class MagicBoltSkill : SkillBase
    {
        [SerializeField] private GameObject mProjectilePrefab;
        [SerializeField] private float mProjectileSpeed = 10f;
        [SerializeField] private float mProjectileLifeTime = 3f;

        public void SetProjectilePrefab(GameObject prefab)
        {
            mProjectilePrefab = prefab;
        }

        protected override void Execute()
        {
            Transform target = FindClosestEnemy();
            if (target == null)
            {
                return;
            }

            // 투사체 수: SpecialValue (Lv1=1, Lv2=2, ...)
            int projectileCount = Mathf.Max(1, (int)mSkillData.GetSpecialValue(mLevel));

            // 기본 방향
            Vector2 baseDirection = ((Vector2)target.position - (Vector2)mPlayerTransform.position).normalized;

            // 데미지 계산
            float baseDmg = CombatHelper.CalculateSkillDamage(mSkillData.GetDamage(mLevel), mLevel);
            float elementalBonus = mPlayerStats != null ? mPlayerStats.ElementalDmgBonus : 0f;
            float critChance = mPlayerStats != null ? mPlayerStats.CritChance : 0.05f;
            float critMult = mPlayerStats != null ? mPlayerStats.CritMultiplier : 1.5f;

            DamageResult dmgResult = CombatHelper.CalculateDamage(baseDmg, elementalBonus, critChance, critMult, 0);

            for (int i = 0; i < projectileCount; i++)
            {
                // 다수 투사체일 때 부채꼴로 발사
                float angleOffset = 0f;
                if (projectileCount > 1)
                {
                    float spreadAngle = 15f * (projectileCount - 1);
                    angleOffset = -spreadAngle / 2f + (spreadAngle / (projectileCount - 1)) * i;
                }

                Vector2 direction = RotateVector(baseDirection, angleOffset);
                SpawnProjectile(direction, dmgResult.mFinalDamage);
            }
        }

        private void SpawnProjectile(Vector2 direction, float damage)
        {
            if (mProjectilePrefab == null)
            {
                return;
            }

            GameObject go = Instantiate(mProjectilePrefab, mPlayerTransform.position, Quaternion.identity);
            go.layer = LayerMask.NameToLayer("PlayerProjectile");

            Projectile proj = go.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(damage, mProjectileSpeed, direction, mProjectileLifeTime);
            }

            // 투사체 회전 (방향을 향하도록)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private Vector2 RotateVector(Vector2 v, float angleDegrees)
        {
            float rad = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
