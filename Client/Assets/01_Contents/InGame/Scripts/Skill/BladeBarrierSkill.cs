using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Combat;
using Vams2.InGame.Enemy;

namespace Vams2.InGame.Skill
{
    // 블레이드 배리어 (회전검 + 마법갑옷 진화)
    // 넓은 반경 회전 + 넉백 + 데미지 2배
    public class BladeBarrierSkill : SkillBase
    {
        private GameObject[] mBlades;
        private float mRotationSpeed = 240f;
        private float mOrbitRadius = 3f;
        private float mCurrentAngle;
        private int mEnemyLayerMask;
        private int mBladeCount = 8;
        private Sprite mBladeSprite;

        public override void Initialize(SkillData data, int level, Player.PlayerStats stats)
        {
            base.Initialize(data, level, stats);
            mEnemyLayerMask = LayerMask.GetMask("Enemy");

            mBladeSprite = Resources.Load<Sprite>("Sprites/Skill/spinning_blade");

            CreateBlades();
        }

        protected override void Execute() { }
        public override float GetCooldownRatio() { return 0f; }

        private void CreateBlades()
        {
            mBlades = new GameObject[mBladeCount];
            for (int i = 0; i < mBladeCount; i++)
            {
                GameObject blade = new GameObject("BarrierBlade_" + i);
                blade.transform.SetParent(mPlayerTransform);
                blade.layer = LayerMask.NameToLayer("PlayerProjectile");

                SpriteRenderer sr = blade.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Projectiles";
                if (mBladeSprite != null) sr.sprite = mBladeSprite;
                blade.transform.localScale = Vector3.one * 0.6f;

                mBlades[i] = blade;
            }
        }

        protected override void Update()
        {
            if (mBlades == null) return;

            mCurrentAngle += mRotationSpeed * Time.deltaTime;
            float angleStep = 360f / mBlades.Length;
            float dmg = CombatHelper.CalculateSkillDamage(mSkillData.GetDamage(mLevel), mLevel);

            for (int i = 0; i < mBlades.Length; i++)
            {
                float angle = (mCurrentAngle + angleStep * i) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * mOrbitRadius;
                mBlades[i].transform.position = mPlayerTransform.position + offset;
                mBlades[i].transform.rotation = Quaternion.Euler(0, 0, mCurrentAngle + angleStep * i);

                Collider2D hit = Physics2D.OverlapCircle(mBlades[i].transform.position, 0.5f, mEnemyLayerMask);
                if (hit != null)
                {
                    EnemyHealth health = hit.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(dmg);

                        // 넉백
                        Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            Vector2 knockDir = ((Vector2)hit.transform.position - (Vector2)mPlayerTransform.position).normalized;
                            rb.AddForce(knockDir * 5f, ForceMode2D.Impulse);
                        }
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (mBlades == null) return;
            for (int i = 0; i < mBlades.Length; i++)
            {
                if (mBlades[i] != null) Destroy(mBlades[i]);
            }
        }
    }
}
