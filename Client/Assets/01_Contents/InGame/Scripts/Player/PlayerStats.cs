using System;
using UnityEngine;
using Vams2.InGame.Combat;

namespace Vams2.InGame.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("기본 스탯")]
        [SerializeField] private int mMaxHp = 100;
        [SerializeField] private int mBaseDamage = 10;
        [SerializeField] private float mCritChance = 0.05f;
        [SerializeField] private float mCritMultiplier = 1.5f;
        [SerializeField] private int mDefense = 0;
        [SerializeField] private float mGemCollectRadius = 2.0f;

        // 현재 상태
        private int mCurrentHp;
        private int mLevel;
        private int mCurrentExp;
        private int mExpToNextLevel;

        // 패시브 보정값
        private float mAttackSpeedBonus;
        private float mElementalDmgBonus;
        private int mDefenseBonus;
        private float mKnockbackResist;
        private float mMoveSpeedBonus;
        private float mDodgeBonus;

        // 프로퍼티
        public int MaxHp => mMaxHp;
        public int CurrentHp => mCurrentHp;
        public int BaseDamage => mBaseDamage;
        public float CritChance => mCritChance;
        public float CritMultiplier => mCritMultiplier;
        public int Defense => mDefense + mDefenseBonus;
        public float GemCollectRadius => mGemCollectRadius;
        public int Level => mLevel;
        public int CurrentExp => mCurrentExp;
        public int ExpToNextLevel => mExpToNextLevel;
        public float AttackSpeedBonus => mAttackSpeedBonus;
        public float ElementalDmgBonus => mElementalDmgBonus;
        public float KnockbackResist => mKnockbackResist;
        public float MoveSpeedBonus => mMoveSpeedBonus;
        public float DodgeBonus => mDodgeBonus;

        // 이벤트
        public Action OnLevelUp;
        public Action OnHpChanged;
        public Action OnDeath;
        public Action OnExpChanged;

        private void Awake()
        {
            mLevel = 1;
            mCurrentExp = 0;
            mExpToNextLevel = CombatHelper.CalculateRequiredExp(mLevel);
            mCurrentHp = mMaxHp;

            mAttackSpeedBonus = 0f;
            mElementalDmgBonus = 0f;
            mDefenseBonus = 0;
            mKnockbackResist = 0f;
            mMoveSpeedBonus = 0f;
            mDodgeBonus = 0f;
        }

        public void AddExp(int amount)
        {
            mCurrentExp += amount;

            while (mCurrentExp >= mExpToNextLevel)
            {
                mCurrentExp -= mExpToNextLevel;
                mLevel++;
                mExpToNextLevel = CombatHelper.CalculateRequiredExp(mLevel);

                if (OnLevelUp != null)
                {
                    OnLevelUp();
                }
            }

            if (OnExpChanged != null)
            {
                OnExpChanged();
            }
        }

        public void TakeDamage(int rawDamage)
        {
            // 회피 판정
            if (CombatHelper.CheckDodge(mDodgeBonus))
            {
                return;
            }

            // 방어력 적용
            float finalDmg = rawDamage * (100f / (100f + Defense));
            int dmg = Mathf.Max(1, Mathf.RoundToInt(finalDmg));

            mCurrentHp -= dmg;

            if (OnHpChanged != null)
            {
                OnHpChanged();
            }

            if (mCurrentHp <= 0)
            {
                mCurrentHp = 0;
                if (OnDeath != null)
                {
                    OnDeath();
                }
            }
        }

        public void Heal(float percent)
        {
            int healAmount = Mathf.RoundToInt(mMaxHp * percent);
            mCurrentHp = Mathf.Min(mCurrentHp + healAmount, mMaxHp);

            if (OnHpChanged != null)
            {
                OnHpChanged();
            }
        }

        // 패시브 보정값 설정
        public void SetAttackSpeedBonus(float value) { mAttackSpeedBonus = value; }
        public void SetElementalDmgBonus(float value) { mElementalDmgBonus = value; }
        public void SetDefenseBonus(int value) { mDefenseBonus = value; }
        public void SetKnockbackResist(float value) { mKnockbackResist = value; }
        public void SetMoveSpeedBonus(float value) { mMoveSpeedBonus = value; }
        public void SetDodgeBonus(float value) { mDodgeBonus = value; }
    }
}
