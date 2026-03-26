using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Player;

namespace Vams2.InGame.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyBehaviour : MonoBehaviour
    {
        private EnemyData mEnemyData;
        private Transform mTarget;
        private Rigidbody2D mRigidbody;
        private EnemyHealth mHealth;
        private EnemyDrop mDrop;
        private float mContactDamageTimer;

        public EnemyData EnemyData => mEnemyData;

        public void Initialize(EnemyData data, Transform target, float hpScale)
        {
            mEnemyData = data;
            mTarget = target;

            mRigidbody = GetComponent<Rigidbody2D>();
            mHealth = GetComponent<EnemyHealth>();
            mDrop = GetComponent<EnemyDrop>();

            // 스프라이트 설정
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (data.mSprite != null)
                {
                    sr.sprite = data.mSprite;
                }
                sr.sortingLayerName = "Enemies";
            }

            // 체력 초기화 (시간 스케일링 적용)
            float scaledHp = data.mBaseHp * hpScale;
            mHealth.Initialize(scaledHp);
            mHealth.OnDeath = OnDeath;

            // 드롭 초기화
            if (mDrop != null)
            {
                mDrop.Initialize(data.mDropExp);
            }

            mContactDamageTimer = 0f;
        }

        private void FixedUpdate()
        {
            if (mTarget == null || mEnemyData == null)
            {
                return;
            }

            switch (mEnemyData.mAiType)
            {
                case EnemyAiType.Chase:
                case EnemyAiType.FastChase:
                case EnemyAiType.EliteSplit:
                    ChaseTarget();
                    break;
                case EnemyAiType.Ranged:
                    ChaseTarget();
                    break;
            }
        }

        private void ChaseTarget()
        {
            Vector2 direction = ((Vector2)mTarget.position - (Vector2)transform.position).normalized;
            mRigidbody.linearVelocity = direction * mEnemyData.mMoveSpeed;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
            {
                return;
            }

            mContactDamageTimer += Time.deltaTime;
            if (mContactDamageTimer >= mEnemyData.mContactDamageInterval)
            {
                mContactDamageTimer = 0f;
                PlayerCombat playerCombat = collision.gameObject.GetComponent<PlayerCombat>();
                if (playerCombat != null)
                {
                    playerCombat.ReceiveDamage(mEnemyData.mBaseDamage);
                }
            }
        }

        private void OnDeath()
        {
            // 드롭 처리
            if (mDrop != null)
            {
                mDrop.SpawnDrops(transform.position);
            }

            // 풀 반환 (비활성화)
            mRigidbody.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (mHealth != null)
            {
                mHealth.OnDeath = null;
            }
        }
    }
}
