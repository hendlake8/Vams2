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

        public void Initialize(int expAmount, Vector3 position)
        {
            mExpAmount = expAmount;
            transform.position = position;
            mIsBeingCollected = false;
            mTarget = null;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Drops";
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
            if (!mIsBeingCollected || mTarget == null)
            {
                return;
            }

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
