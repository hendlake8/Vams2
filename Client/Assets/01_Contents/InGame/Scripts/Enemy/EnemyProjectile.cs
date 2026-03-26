using UnityEngine;
using Vams2.InGame.Player;

namespace Vams2.InGame.Enemy
{
    // 적 전용 투사체 (Player 레이어와 충돌)
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        private int mDamage;
        private float mSpeed;
        private Vector2 mDirection;
        private float mLifeTime;
        private Rigidbody2D mRigidbody;
        private bool mIsDestroyed;
        private int mPlayerLayerMask;

        public void Initialize(int damage, float speed, Vector2 direction, float lifeTime)
        {
            mDamage = damage;
            mSpeed = speed;
            mDirection = direction.normalized;
            mLifeTime = lifeTime;
            mIsDestroyed = false;

            mRigidbody = GetComponent<Rigidbody2D>();
            mRigidbody.bodyType = RigidbodyType2D.Kinematic;
            mRigidbody.linearVelocity = mDirection * mSpeed;

            mPlayerLayerMask = LayerMask.GetMask("Player");

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Projectiles";
            }
        }

        private void FixedUpdate()
        {
            if (mIsDestroyed) return;

            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.3f, mPlayerLayerMask);
            if (hit != null)
            {
                PlayerCombat combat = hit.GetComponent<PlayerCombat>();
                if (combat != null)
                {
                    combat.ReceiveDamage(mDamage);
                }
                mIsDestroyed = true;
                Destroy(gameObject);
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
