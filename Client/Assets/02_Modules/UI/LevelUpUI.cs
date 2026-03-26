using System.Collections.Generic;
using UnityEngine;
using Vams2.InGame.Skill;

namespace Vams2.UI
{
    // 레벨업 시 3카드 선택 UI
    // Time.timeScale = 0으로 게임 일시정지
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private LevelUpCard[] mCards;
        [SerializeField] private GameObject mPanel;

        private SkillManager mSkillManager;
        private bool mIsShowing;

        public bool IsShowing => mIsShowing;

        public void Initialize(SkillManager skillManager)
        {
            mSkillManager = skillManager;
            mIsShowing = false;
            if (mPanel != null)
            {
                mPanel.SetActive(false);
            }
        }

        public void Show(List<SkillChoice> choices)
        {
            if (choices == null || choices.Count == 0)
            {
                return;
            }

            mIsShowing = true;
            Time.timeScale = 0f;

            if (mPanel != null)
            {
                mPanel.SetActive(true);
            }

            // 카드 설정
            for (int i = 0; i < mCards.Length; i++)
            {
                if (i < choices.Count)
                {
                    mCards[i].gameObject.SetActive(true);
                    mCards[i].Setup(choices[i], this);
                }
                else
                {
                    mCards[i].gameObject.SetActive(false);
                }
            }
        }

        public void OnCardSelected(SkillChoice choice)
        {
            // 스킬 적용
            if (mSkillManager != null)
            {
                mSkillManager.AddOrUpgradeSkill(choice);
            }

            // UI 닫기, 게임 재개
            mIsShowing = false;
            Time.timeScale = 1f;

            if (mPanel != null)
            {
                mPanel.SetActive(false);
            }
        }
    }
}
