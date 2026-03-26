using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Combat;
using Vams2.InGame.Enemy;

namespace Vams2.InGame.Skill
{
    // 회전 검: 캐릭터 주변을 회전하며 접촉한 적에게 데미지
    // SpecialValue = 검 개수 (Lv1=2, Lv2=3, ...)
    public class SpinningBladeSkill : SkillBase
    {
        [SerializeField] private Sprite mBladeSprite;

        private GameObject[] mBlades;
        private float mRotationSpeed = 180f;
        private float mOrbitRadius = 1.5f;
        private float mCurrentAngle;
        private int mEnemyLayerMask;

        public void SetBladeSprite(Sprite sprite)
        {
            mBladeSprite = sprite;
        }

        public override void Initialize(SkillData data, int level, Player.PlayerStats stats)
        {
            base.Initialize(data, level, stats);
            mEnemyLayerMask = LayerMask.GetMask("Enemy");
            mCurrentAngle = 0f;
            CreateBlades();
        }

        // 회전 검은 쿨다운 기반이 아닌 상시 동작
        protected override void Execute() { }
        public override float GetCooldownRatio() { return 0f; }

        public override void OnLevelUp(int newLevel)
        {
            base.OnLevelUp(newLevel);
            DestroyBlades();
            CreateBlades();
        }

        private void CreateBlades()
        {
            int bladeCount = Mathf.Max(2, (int)mSkillData.GetSpecialValue(mLevel));
            mBlades = new GameObject[bladeCount];

            for (int i = 0; i < bladeCount; i++)
            {
                GameObject blade = new GameObject("Blade_" + i);
                blade.transform.SetParent(mPlayerTransform);
                blade.layer = LayerMask.NameToLayer("PlayerProjectile");

                SpriteRenderer sr = blade.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Projectiles";
                if (mBladeSprite != null)
                {
                    sr.sprite = mBladeSprite;
                }
                blade.transform.localScale = Vector3.one * 0.5f;

                mBlades[i] = blade;
            }
        }

        private void DestroyBlades()
        {
            if (mBlades == null) return;
            for (int i = 0; i < mBlades.Length; i++)
            {
                if (mBlades[i] != null) Destroy(mBlades[i]);
            }
            mBlades = null;
        }

        protected override void Update()
        {
            if (mBlades == null || mSkillData == null) return;

            mCurrentAngle += mRotationSpeed * Time.deltaTime;

            float angleStep = 360f / mBlades.Length;
            for (int i = 0; i < mBlades.Length; i++)
            {
                float angle = (mCurrentAngle + angleStep * i) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * mOrbitRadius;
                mBlades[i].transform.position = mPlayerTransform.position + offset;
                mBlades[i].transform.rotation = Quaternion.Euler(0, 0, mCurrentAngle + angleStep * i);

                // 적 충돌 체크
                Collider2D hit = Physics2D.OverlapCircle(mBlades[i].transform.position, 0.4f, mEnemyLayerMask);
                if (hit != null)
                {
                    EnemyHealth health = hit.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        float dmg = CombatHelper.CalculateSkillDamage(mSkillData.GetDamage(mLevel), mLevel);
                        health.TakeDamage(dmg);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            DestroyBlades();
        }
    }
}
