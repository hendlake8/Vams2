using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Combat;
using Vams2.InGame.Enemy;

namespace Vams2.InGame.Skill
{
    // 포이즌 클라우드: 장판 설치, 범위 내 적에게 지속 데미지
    // SpecialValue = 장판 반경
    public class PoisonCloudSkill : SkillBase
    {
        [SerializeField] private Sprite mCloudSprite;
        [SerializeField] private float mCloudDuration = 3f;
        [SerializeField] private float mTickInterval = 0.5f;

        private int mEnemyLayerMask;

        public void SetCloudSprite(Sprite sprite)
        {
            mCloudSprite = sprite;
        }

        public override void Initialize(SkillData data, int level, Player.PlayerStats stats)
        {
            base.Initialize(data, level, stats);
            mEnemyLayerMask = LayerMask.GetMask("Enemy");
        }

        protected override void Execute()
        {
            Transform target = FindClosestEnemy();
            Vector3 spawnPos;

            if (target != null)
            {
                spawnPos = target.position;
            }
            else
            {
                // 적 없으면 플레이어 주변 랜덤
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                spawnPos = mPlayerTransform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 3f;
            }

            float radius = mSkillData.GetSpecialValue(mLevel);
            float dmg = CombatHelper.CalculateSkillDamage(mSkillData.GetDamage(mLevel), mLevel);

            SpawnCloud(spawnPos, radius, dmg);
        }

        private void SpawnCloud(Vector3 position, float radius, float damagePerTick)
        {
            GameObject cloudGo = new GameObject("PoisonCloud");
            cloudGo.transform.position = position;

            SpriteRenderer sr = cloudGo.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Drops";
            sr.sortingOrder = 5;
            if (mCloudSprite != null)
            {
                sr.sprite = mCloudSprite;
            }
            sr.color = new Color(0.5f, 0.2f, 0.8f, 0.5f);

            float spriteSize = sr.sprite != null ? sr.sprite.bounds.size.x : 1f;
            float scale = (radius * 2f) / spriteSize;
            cloudGo.transform.localScale = Vector3.one * scale;

            PoisonCloudZone zone = cloudGo.AddComponent<PoisonCloudZone>();
            zone.Initialize(radius, damagePerTick, mCloudDuration, mTickInterval, mEnemyLayerMask);
        }
    }

    // 장판 존 (독립 컴포넌트)
    public class PoisonCloudZone : MonoBehaviour
    {
        private float mRadius;
        private float mDamagePerTick;
        private float mDuration;
        private float mTickInterval;
        private float mTickTimer;
        private float mLifeTimer;
        private int mEnemyLayerMask;

        public void Initialize(float radius, float damagePerTick, float duration, float tickInterval, int enemyLayerMask)
        {
            mRadius = radius;
            mDamagePerTick = damagePerTick;
            mDuration = duration;
            mTickInterval = tickInterval;
            mEnemyLayerMask = enemyLayerMask;
            mTickTimer = 0f;
            mLifeTimer = 0f;
        }

        private void Update()
        {
            mLifeTimer += Time.deltaTime;
            if (mLifeTimer >= mDuration)
            {
                Destroy(gameObject);
                return;
            }

            mTickTimer += Time.deltaTime;
            if (mTickTimer >= mTickInterval)
            {
                mTickTimer = 0f;
                DealDamage();
            }

            // 페이드아웃
            float alpha = 1f - (mLifeTimer / mDuration);
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0.5f, 0.2f, 0.8f, alpha * 0.5f);
            }
        }

        private void DealDamage()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, mRadius, mEnemyLayerMask);
            for (int i = 0; i < hits.Length; i++)
            {
                EnemyHealth health = hits[i].GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(mDamagePerTick);
                }
            }
        }
    }
}
