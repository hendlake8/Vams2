using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Combat;
using Vams2.InGame.Enemy;

namespace Vams2.InGame.Skill
{
    // 썬더 스톰 (라이트닝 + 마나서지 진화)
    // 화면 내 모든 적에게 번개 데미지
    public class ThunderStormSkill : SkillBase
    {
        private int mEnemyLayerMask;

        public override void Initialize(SkillData data, int level, Player.PlayerStats stats)
        {
            base.Initialize(data, level, stats);
            mEnemyLayerMask = LayerMask.GetMask("Enemy");
        }

        protected override void Execute()
        {
            float dmg = CombatHelper.CalculateSkillDamage(mSkillData.GetDamage(mLevel), mLevel);
            float elementalBonus = mPlayerStats != null ? mPlayerStats.ElementalDmgBonus : 0f;
            DamageResult result = CombatHelper.CalculateDamage(dmg, elementalBonus,
                mPlayerStats.CritChance, mPlayerStats.CritMultiplier, 0);

            // 화면 내 모든 적 타격 (넓은 범위)
            Collider2D[] hits = Physics2D.OverlapCircleAll(mPlayerTransform.position, 12f, mEnemyLayerMask);
            System.Collections.Generic.List<Vector3> points = new System.Collections.Generic.List<Vector3>();
            points.Add(mPlayerTransform.position);

            for (int i = 0; i < hits.Length; i++)
            {
                if (!hits[i].gameObject.activeSelf) continue;

                EnemyHealth health = hits[i].GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(result.mFinalDamage);
                    points.Add(hits[i].transform.position);
                }
            }

            // 번개 이펙트
            if (points.Count > 1)
            {
                for (int i = 1; i < points.Count; i++)
                {
                    SpawnBolt(mPlayerTransform.position, points[i]);
                }
            }
        }

        private void SpawnBolt(Vector3 from, Vector3 to)
        {
            GameObject go = new GameObject("ThunderBolt");
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.startWidth = 0.2f;
            line.endWidth = 0.08f;
            line.startColor = new Color(1f, 1f, 0f, 1f);
            line.endColor = new Color(0.3f, 0.3f, 1f, 0.8f);
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.sortingLayerName = "Projectiles";
            line.useWorldSpace = true;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            Destroy(go, 0.2f);
        }
    }
}
