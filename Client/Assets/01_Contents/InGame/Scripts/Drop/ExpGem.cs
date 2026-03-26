using UnityEngine;
using Vams2.InGame.Player;

namespace Vams2.InGame.Drop
{
    public class ExpGem : MonoBehaviour
    {
        [SerializeField] private int mExpAmount = 1;
        [SerializeField] private float mMoveSpeed = 10f;

        private bool mIsBeingCollected;
        private Transform mTarget;
        private SpriteRenderer mSpriteRenderer;
        private float mAnimTimer;
        private Vector3 mBasePosition;

        public void Initialize(int expAmount, Vector3 position)
        {
            mExpAmount = expAmount;
            transform.position = position;
            mBasePosition = position;
            mIsBeingCollected = false;
            mTarget = null;
            mAnimTimer = Random.Range(0f, Mathf.PI * 2f);

            mSpriteRenderer = GetComponent<SpriteRenderer>();
            if (mSpriteRenderer != null)
            {
                mSpriteRenderer.sortingLayerName = "Drops";
            }
        }

        public void StartMoveToPlayer(Transform target)
        {
            if (mIsBeingCollected)
            {
                return;
            }
            mIsBeingCollected = true;
            mTarget = target;
        }

        private void Update()
        {
            mAnimTimer += Time.deltaTime * 4f;

            if (!mIsBeingCollected)
            {
                // 대기 중 애니메이션: 밝기 펄스 + 바운스
                if (mSpriteRenderer != null)
                {
                    float pulse = 0.7f + 0.3f * Mathf.Sin(mAnimTimer);
                    mSpriteRenderer.color = new Color(pulse, 1f, pulse, 1f);
                }
                float bounce = Mathf.Sin(mAnimTimer * 0.8f) * 0.1f;
                transform.position = mBasePosition + new Vector3(0f, bounce, 0f);
                return;
            }

            if (mTarget == null) return;

            Vector3 dir = mTarget.position - transform.position;
            float distance = dir.magnitude;

            if (distance < 0.2f)
            {
                OnCollected();
                return;
            }

            transform.position += dir.normalized * mMoveSpeed * Time.deltaTime;
        }

        private void OnCollected()
        {
            ExpCollector collector = mTarget.GetComponentInChildren<ExpCollector>();
            if (collector != null)
            {
                collector.OnGemCollected(mExpAmount);
            }

            gameObject.SetActive(false);
        }
    }
}
