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
        private SpriteRenderer mSpriteRenderer;
        private float mContactDamageTimer;
        private float mRangedAttackTimer;

        public EnemyData EnemyData => mEnemyData;

        public void Initialize(EnemyData data, Transform target, float hpScale)
        {
            mEnemyData = data;
            mTarget = target;

            mRigidbody = GetComponent<Rigidbody2D>();
            mHealth = GetComponent<EnemyHealth>();
            mDrop = GetComponent<EnemyDrop>();
            mSpriteRenderer = GetComponent<SpriteRenderer>();

            if (mSpriteRenderer != null)
            {
                if (data.mSprite != null)
                {
                    mSpriteRenderer.sprite = data.mSprite;
                }
                mSpriteRenderer.sortingLayerName = "Enemies";
            }

            float scaledHp = data.mBaseHp * hpScale;
            mHealth.Initialize(scaledHp);
            mHealth.OnDeath = OnDeath;

            if (mDrop != null)
            {
                mDrop.Initialize(data.mDropExp);
            }

            mContactDamageTimer = 0f;
            mRangedAttackTimer = 0f;
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
                    RangedBehaviour();
                    break;
            }

            // X축 이동 방향에 따라 좌우 반전
            if (mSpriteRenderer != null && mRigidbody.linearVelocity.x != 0f)
            {
                mSpriteRenderer.flipX = mRigidbody.linearVelocity.x > 0f;
            }
        }

        private void ChaseTarget()
        {
            Vector2 direction = ((Vector2)mTarget.position - (Vector2)transform.position).normalized;
            mRigidbody.linearVelocity = direction * mEnemyData.mMoveSpeed;
        }

        private void RangedBehaviour()
        {
            float distToTarget = Vector2.Distance(transform.position, mTarget.position);

            // 사거리 밖이면 접근
            if (distToTarget > mEnemyData.mAttackRange)
            {
                ChaseTarget();
            }
            else
            {
                // 사거리 안이면 정지 + 공격
                mRigidbody.linearVelocity = Vector2.zero;

                mRangedAttackTimer += Time.fixedDeltaTime;
                if (mRangedAttackTimer >= mEnemyData.mAttackInterval)
                {
                    mRangedAttackTimer = 0f;
                    FireProjectile();
                }
            }
        }

        private void FireProjectile()
        {
            if (mEnemyData.mProjectilePrefab == null || mTarget == null)
            {
                return;
            }

            Vector2 direction = ((Vector2)mTarget.position - (Vector2)transform.position).normalized;

            GameObject projGo = Instantiate(mEnemyData.mProjectilePrefab, transform.position, Quaternion.identity);
            projGo.layer = LayerMask.NameToLayer("EnemyProjectile");

            // EnemyProjectile 컴포넌트 사용 (Projectile과 별도)
            EnemyProjectile proj = projGo.GetComponent<EnemyProjectile>();
            if (proj == null)
            {
                proj = projGo.AddComponent<EnemyProjectile>();
            }
            proj.Initialize(mEnemyData.mBaseDamage, 6f, direction, 3f);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projGo.transform.rotation = Quaternion.Euler(0, 0, angle);
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
            if (mDrop != null)
            {
                mDrop.SpawnDrops(transform.position);
            }

            // 엘리트 분열
            if (mEnemyData.mAiType == EnemyAiType.EliteSplit && mEnemyData.mSplitEnemyData != null)
            {
                SpawnSplitEnemies();
            }

            mRigidbody.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }

        private void SpawnSplitEnemies()
        {
            for (int i = 0; i < mEnemyData.mSplitCount; i++)
            {
                float offsetX = (i == 0) ? -0.5f : 0.5f;
                Vector3 spawnPos = transform.position + new Vector3(offsetX, 0f, 0f);

                // EnemySpawner를 통하지 않고 직접 생성
                GameObject splitGo = Instantiate(gameObject, spawnPos, Quaternion.identity);
                splitGo.SetActive(true);

                EnemyBehaviour splitBehaviour = splitGo.GetComponent<EnemyBehaviour>();
                if (splitBehaviour != null)
                {
                    splitBehaviour.Initialize(mEnemyData.mSplitEnemyData, mTarget, 1f);
                }

                // 작은 크기
                splitGo.transform.localScale = Vector3.one * 0.7f;
            }
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
