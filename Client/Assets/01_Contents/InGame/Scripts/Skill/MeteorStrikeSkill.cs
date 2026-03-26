using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Combat;
using Vams2.InGame.Enemy;

namespace Vams2.InGame.Skill
{
    // 메테오 스트라이크 (파이어볼 + 원소핵 진화)
    // 가장 가까운 적 위치에 운석 낙하, 광역 폭발
    public class MeteorStrikeSkill : SkillBase
    {
        [SerializeField] private Sprite mMeteorSprite;
        private int mEnemyLayerMask;
        private float mExplosionRadius = 3f;

        public void SetMeteorSprite(Sprite sprite) { mMeteorSprite = sprite; }

        public override void Initialize(SkillData data, int level, Player.PlayerStats stats)
        {
            base.Initialize(data, level, stats);
            mEnemyLayerMask = LayerMask.GetMask("Enemy");

            Sprite spr = LoadSpriteFromTextures("Skill/meteor_proj");
            if (spr != null) mMeteorSprite = spr;
        }

        protected override void Execute()
        {
            Transform target = FindClosestEnemy();
            if (target == null) return;

            float dmg = CombatHelper.CalculateSkillDamage(mSkillData.GetDamage(mLevel), mLevel);
            float elementalBonus = mPlayerStats != null ? mPlayerStats.ElementalDmgBonus : 0f;
            DamageResult result = CombatHelper.CalculateDamage(dmg, elementalBonus,
                mPlayerStats.CritChance, mPlayerStats.CritMultiplier, 0);

            SpawnMeteor(target.position, result.mFinalDamage);
        }

        private void SpawnMeteor(Vector3 targetPos, float damage)
        {
            // 운석 이펙트 (간단한 스프라이트 + 폭발)
            GameObject meteorGo = new GameObject("Meteor");
            SpriteRenderer sr = meteorGo.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Projectiles";
            sr.sortingOrder = 10;
            if (mMeteorSprite != null) sr.sprite = mMeteorSprite;

            // 위에서 떨어지는 연출
            meteorGo.transform.position = targetPos + Vector3.up * 5f;
            meteorGo.transform.localScale = Vector3.one * 0.8f;

            MeteorEffect effect = meteorGo.AddComponent<MeteorEffect>();
            effect.Initialize(targetPos, damage, mExplosionRadius, mEnemyLayerMask);
        }

        private Sprite LoadSpriteFromTextures(string relativePath)
        {
            #if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/01_Contents/InGame/RES/Bundle/Textures/" + relativePath + ".png");
            #else
            return null;
            #endif
        }
    }

    public class MeteorEffect : MonoBehaviour
    {
        private Vector3 mTargetPos;
        private float mDamage;
        private float mRadius;
        private int mEnemyLayerMask;
        private float mFallSpeed = 15f;

        public void Initialize(Vector3 target, float damage, float radius, int enemyLayerMask)
        {
            mTargetPos = target;
            mDamage = damage;
            mRadius = radius;
            mEnemyLayerMask = enemyLayerMask;
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, mTargetPos, mFallSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, mTargetPos) < 0.1f)
            {
                Explode();
            }
        }

        private void Explode()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(mTargetPos, mRadius, mEnemyLayerMask);
            for (int i = 0; i < hits.Length; i++)
            {
                EnemyHealth health = hits[i].GetComponent<EnemyHealth>();
                if (health != null) health.TakeDamage(mDamage);
            }
            Destroy(gameObject);
        }
    }
}
