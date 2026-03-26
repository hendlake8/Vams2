using System;
using UnityEngine;

namespace Vams2.InGame.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private float mMaxHp = 10f;

        private float mCurrentHp;

        public Action OnDeath;

        public void Initialize(float maxHp)
        {
            mMaxHp = maxHp;
            mCurrentHp = mMaxHp;
        }

        public void TakeDamage(float damage)
        {
            mCurrentHp -= damage;

            if (mCurrentHp <= 0f)
            {
                mCurrentHp = 0f;
                if (OnDeath != null)
                {
                    OnDeath();
                }
            }
        }

        public float GetHpRatio()
        {
            return mCurrentHp / mMaxHp;
        }
    }
}
