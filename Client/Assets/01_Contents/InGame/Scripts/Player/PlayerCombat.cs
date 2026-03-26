using UnityEngine;
using Vams2.Core;

namespace Vams2.InGame.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        private PlayerStats mStats;
        private bool mIsDead;

        public bool IsDead => mIsDead;

        private void Awake()
        {
            mStats = GetComponent<PlayerStats>();
            mIsDead = false;
        }

        private void OnEnable()
        {
            if (mStats != null)
            {
                mStats.OnDeath = OnPlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (mStats != null)
            {
                mStats.OnDeath = null;
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (mIsDead)
            {
                return;
            }

            // 적과 접촉 시 데미지 (접촉 데미지)
            if (collision.gameObject.CompareTag("Enemy"))
            {
                // 적의 데미지는 EnemyBehaviour에서 처리
            }
        }

        public void ReceiveDamage(int damage)
        {
            if (mIsDead)
            {
                return;
            }
            mStats.TakeDamage(damage);
        }

        private void OnPlayerDeath()
        {
            mIsDead = true;
            GameManager.Instance.EndGame(false);
        }
    }
}
