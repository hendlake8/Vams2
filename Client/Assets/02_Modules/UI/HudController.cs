using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vams2.InGame.Player;
using Vams2.InGame.Skill;
using System.Collections.Generic;

namespace Vams2.UI
{
    // 인게임 HUD (HP바, EXP바, 경과시간, 스킬 아이콘)
    public class HudController : MonoBehaviour
    {
        [Header("HP")]
        [SerializeField] private Slider mHpBar;
        [SerializeField] private TMP_Text mHpText;

        [Header("EXP")]
        [SerializeField] private Slider mExpBar;
        [SerializeField] private TMP_Text mLevelText;

        [Header("시간")]
        [SerializeField] private TMP_Text mTimeText;

        [Header("스킬 아이콘")]
        [SerializeField] private UnityEngine.UI.Image[] mSkillIcons;

        private PlayerStats mPlayerStats;
        private SkillManager mSkillManager;
        private System.Func<float> mGetElapsedTime;

        public void Initialize(PlayerStats stats, SkillManager skillManager, System.Func<float> getElapsedTime)
        {
            mPlayerStats = stats;
            mSkillManager = skillManager;
            mGetElapsedTime = getElapsedTime;

            if (mPlayerStats != null)
            {
                mPlayerStats.OnHpChanged = UpdateHpBar;
                mPlayerStats.OnExpChanged = UpdateExpBar;
                mPlayerStats.OnLevelUp += UpdateExpBar;
                mPlayerStats.OnLevelUp += UpdateLevelText;
            }

            UpdateHpBar();
            UpdateExpBar();
            UpdateLevelText();

            // 스킬 아이콘 초기화 (비활성)
            if (mSkillIcons != null)
            {
                for (int i = 0; i < mSkillIcons.Length; i++)
                {
                    if (mSkillIcons[i] != null)
                    {
                        mSkillIcons[i].enabled = false;
                    }
                }
            }
        }

        private void Update()
        {
            UpdateTimeText();
            UpdateSkillIcons();
        }

        private void UpdateHpBar()
        {
            if (mHpBar == null || mPlayerStats == null) return;

            mHpBar.value = (float)mPlayerStats.CurrentHp / mPlayerStats.MaxHp;

            if (mHpText != null)
            {
                mHpText.text = mPlayerStats.CurrentHp + " / " + mPlayerStats.MaxHp;
            }
        }

        private void UpdateExpBar()
        {
            if (mExpBar == null || mPlayerStats == null) return;

            mExpBar.value = (float)mPlayerStats.CurrentExp / mPlayerStats.ExpToNextLevel;
        }

        private void UpdateLevelText()
        {
            if (mLevelText == null || mPlayerStats == null) return;

            mLevelText.text = "Lv." + mPlayerStats.Level;
        }

        private void UpdateTimeText()
        {
            if (mTimeText == null || mGetElapsedTime == null) return;

            float elapsed = mGetElapsedTime();
            int minutes = (int)(elapsed / 60f);
            int seconds = (int)(elapsed % 60f);
            mTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private void UpdateSkillIcons()
        {
            if (mSkillIcons == null || mSkillManager == null) return;

            List<SkillSlot> activeSlots = mSkillManager.ActiveSlots;
            if (activeSlots == null) return;

            for (int i = 0; i < mSkillIcons.Length; i++)
            {
                if (mSkillIcons[i] == null) continue;

                if (i < activeSlots.Count && activeSlots[i].mData.mIcon != null)
                {
                    mSkillIcons[i].sprite = activeSlots[i].mData.mIcon;
                    mSkillIcons[i].enabled = true;
                }
                else
                {
                    mSkillIcons[i].enabled = false;
                }
            }
        }
    }
}
