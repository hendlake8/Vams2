using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Combat;
using Vams2.InGame.Enemy;

namespace Vams2.InGame.Skill
{
    // 라이트닝: 가장 가까운 적에게 체인 데미지
    // SpecialValue = 체인 수 (Lv1=3, Lv2=4, ...)
    public class LightningSkill : SkillBase
    {
        private float mChainRange = 3f;
        private int mEnemyLayerMask;

        public override void Initialize(SkillData data, int level, Player.PlayerStats stats)
        {
            base.Initialize(data, level, stats);
            mEnemyLayerMask = LayerMask.GetMask("Enemy");
        }

        protected override void Execute()
        {
            Transform firstTarget = FindClosestEnemy();
            if (firstTarget == null) return;

            float dmg = CombatHelper.CalculateSkillDamage(mSkillData.GetDamage(mLevel), mLevel);
            float elementalBonus = mPlayerStats != null ? mPlayerStats.ElementalDmgBonus : 0f;
            DamageResult result = CombatHelper.CalculateDamage(dmg, elementalBonus,
                mPlayerStats.CritChance, mPlayerStats.CritMultiplier, 0);

            int chainCount = Mathf.Max(1, (int)mSkillData.GetSpecialValue(mLevel));
            ChainDamage(firstTarget, result.mFinalDamage, chainCount);
        }

        private void ChainDamage(Transform firstTarget, float damage, int maxChains)
        {
            Transform current = firstTarget;
            System.Collections.Generic.HashSet<int> hitIds = new System.Collections.Generic.HashSet<int>();
            System.Collections.Generic.List<Vector3> chainPoints = new System.Collections.Generic.List<Vector3>();
            chainPoints.Add(mPlayerTransform.position);

            for (int i = 0; i < maxChains; i++)
            {
                if (current == null) break;

                int instanceId = current.gameObject.GetInstanceID();
                if (hitIds.Contains(instanceId)) break;
                hitIds.Add(instanceId);

                chainPoints.Add(current.position);

                EnemyHealth health = current.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }

                current = FindNextChainTarget(current.position, hitIds);
            }

            // 번개 이펙트 표시
            if (chainPoints.Count > 1)
            {
                SpawnLightningEffect(chainPoints);
            }
        }

        private void SpawnLightningEffect(System.Collections.Generic.List<Vector3> points)
        {
            GameObject effectGo = new GameObject("LightningEffect");
            LineRenderer line = effectGo.AddComponent<LineRenderer>();
            line.positionCount = points.Count;
            line.startWidth = 0.15f;
            line.endWidth = 0.05f;
            line.startColor = new Color(1f, 1f, 0.3f, 1f);
            line.endColor = new Color(0.5f, 0.5f, 1f, 0.5f);
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.sortingLayerName = "Projectiles";
            line.sortingOrder = 10;
            line.useWorldSpace = true;

            for (int i = 0; i < points.Count; i++)
            {
                line.SetPosition(i, points[i]);
            }

            // 0.15초 후 자동 삭제
            Destroy(effectGo, 0.15f);
        }

        private Transform FindNextChainTarget(Vector3 fromPos, System.Collections.Generic.HashSet<int> excludeIds)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(fromPos, mChainRange, mEnemyLayerMask);
            Transform closest = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                if (!hits[i].gameObject.activeSelf) continue;
                if (excludeIds.Contains(hits[i].gameObject.GetInstanceID())) continue;

                float dist = Vector2.Distance(fromPos, hits[i].transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hits[i].transform;
                }
            }
            return closest;
        }
    }
}
