using UnityEngine;

namespace Vams2.InGame.Player
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class ExpCollector : MonoBehaviour
    {
        private PlayerStats mStats;
        private CircleCollider2D mCollider;

        private void Awake()
        {
            mStats = GetComponentInParent<PlayerStats>();
            mCollider = GetComponent<CircleCollider2D>();
            mCollider.isTrigger = true;

            if (mStats != null)
            {
                mCollider.radius = mStats.GemCollectRadius;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Gem"))
            {
                // ExpGem 컴포넌트에 흡수 시작 요청
                var gem = other.GetComponent<Vams2.InGame.Drop.ExpGem>();
                if (gem != null)
                {
                    gem.StartMoveToPlayer(transform.parent);
                }
            }

            if (other.CompareTag("DropItem"))
            {
                // DropItem 컴포넌트에 수집 처리
                var item = other.GetComponent<Vams2.InGame.Drop.DropItem>();
                if (item != null)
                {
                    item.OnPickedUp(mStats);
                }
            }
        }

        public void OnGemCollected(int expAmount)
        {
            if (mStats != null)
            {
                mStats.AddExp(expAmount);
            }
        }
    }
}
