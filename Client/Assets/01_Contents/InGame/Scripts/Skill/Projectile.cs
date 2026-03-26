using UnityEngine;
using Vams2.InGame.Enemy;

namespace Vams2.InGame.Skill
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        private float mDamage;
        private float mSpeed;
        private Vector2 mDirection;
        private float mLifeTime;
        private bool mIsPiercing;
        private float mSlowAmount;
        private float mSlowDuration;
        private Rigidbody2D mRigidbody;
        private float mHitRadius = 0.3f;
        private bool mIsDestroyed;
        private int mEnemyLayerMask;

        public void Initialize(float damage, float speed, Vector2 direction, float lifeTime,
            bool isPiercing = false, float slowAmount = 0f, float slowDuration = 0f)
        {
            mDamage = damage;
            mSpeed = speed;
            mDirection = direction.normalized;
            mLifeTime = lifeTime;
            mIsPiercing = isPiercing;
            mSlowAmount = slowAmount;
            mSlowDuration = slowDuration;
            mIsDestroyed = false;

            mRigidbody = GetComponent<Rigidbody2D>();
            mRigidbody.bodyType = RigidbodyType2D.Kinematic;
            mRigidbody.linearVelocity = mDirection * mSpeed;

            mEnemyLayerMask = LayerMask.GetMask("Enemy");

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Projectiles";
            }
        }

        private void FixedUpdate()
        {
            if (mIsDestroyed)
            {
                return;
            }

            // 수동 충돌 체크 (Physics2D.OverlapCircle)
            Collider2D hit = Physics2D.OverlapCircle(transform.position, mHitRadius, mEnemyLayerMask);
            if (hit != null)
            {
                EnemyHealth health = hit.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(mDamage);
                }

                if (!mIsPiercing)
                {
                    mIsDestroyed = true;
                    Destroy(gameObject);
                    return;
                }
            }
        }

        private void Update()
        {
            mLifeTime -= Time.deltaTime;
            if (mLifeTime <= 0f && !mIsDestroyed)
            {
                mIsDestroyed = true;
                Destroy(gameObject);
            }
        }
    }
}
