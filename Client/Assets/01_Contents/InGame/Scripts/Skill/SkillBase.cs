using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Player;

namespace Vams2.InGame.Skill
{
    // 모든 액티브 스킬의 베이스 클래스
    // 플레이어의 자식 오브젝트로 붙는다
    public abstract class SkillBase : MonoBehaviour
    {
        protected SkillData mSkillData;
        protected int mLevel;
        protected float mCooldownTimer;
        protected PlayerStats mPlayerStats;
        protected Transform mPlayerTransform;

        public SkillData SkillData => mSkillData;
        public int Level => mLevel;

        public virtual void Initialize(SkillData data, int level, PlayerStats stats)
        {
            mSkillData = data;
            mLevel = level;
            mPlayerStats = stats;
            mPlayerTransform = stats.transform;
            mCooldownTimer = 0f;
        }

        public virtual void OnLevelUp(int newLevel)
        {
            mLevel = newLevel;
        }

        protected virtual void Update()
        {
            if (mSkillData == null)
            {
                return;
            }

            mCooldownTimer += Time.deltaTime;

            float cooldown = mSkillData.GetCooldown(mLevel);
            float speedBonus = mPlayerStats != null ? mPlayerStats.AttackSpeedBonus : 0f;
            float adjustedCooldown = cooldown / (1f + speedBonus);

            if (mCooldownTimer >= adjustedCooldown)
            {
                mCooldownTimer = 0f;
                Execute();
            }
        }

        protected abstract void Execute();

        // 가장 가까운 적 찾기 (최대 사거리 제한)
        private const float MAX_ATTACK_RANGE = 10f;

        protected Transform FindClosestEnemy(float maxRange = MAX_ATTACK_RANGE)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform closest = null;
            float closestDist = maxRange;

            for (int i = 0; i < enemies.Length; i++)
            {
                if (!enemies[i].activeSelf)
                {
                    continue;
                }

                float dist = Vector2.Distance(transform.position, enemies[i].transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = enemies[i].transform;
                }
            }
            return closest;
        }
    }
}
