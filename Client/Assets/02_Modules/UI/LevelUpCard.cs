using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vams2.InGame.Skill;

namespace Vams2.UI
{
    // 레벨업 시 표시되는 스킬 선택 카드 1개
    public class LevelUpCard : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image mIcon;
        [SerializeField] private TMP_Text mNameText;
        [SerializeField] private TMP_Text mLevelText;
        [SerializeField] private TMP_Text mDescText;
        [SerializeField] private UnityEngine.UI.Image mBorderImage;
        [SerializeField] private Button mButton;

        private SkillChoice mChoice;
        private LevelUpUI mParentUI;

        public void Setup(SkillChoice choice, LevelUpUI parentUI)
        {
            mChoice = choice;
            mParentUI = parentUI;

            // 아이콘
            if (mIcon != null && choice.mSkillData.mIcon != null)
            {
                mIcon.sprite = choice.mSkillData.mIcon;
                mIcon.enabled = true;
            }

            // 스킬명
            if (mNameText != null)
            {
                mNameText.text = choice.mSkillData.mSkillName;
            }

            // 레벨 표시
            if (mLevelText != null)
            {
                switch (choice.mChoiceType)
                {
                    case SkillChoiceType.NewSkill:
                        mLevelText.text = "NEW";
                        mLevelText.color = Color.green;
                        break;
                    case SkillChoiceType.Upgrade:
                        mLevelText.text = "Lv " + choice.mNextLevel;
                        mLevelText.color = Color.white;
                        break;
                    case SkillChoiceType.Evolution:
                        mLevelText.text = "진화!";
                        mLevelText.color = Color.yellow;
                        break;
                }
            }

            // 설명
            if (mDescText != null)
            {
                mDescText.text = choice.mSkillData.mDescription;
            }

            // 진화 시 금색 테두리
            if (mBorderImage != null)
            {
                bool isEvolution = choice.mChoiceType == SkillChoiceType.Evolution;
                mBorderImage.color = isEvolution ? new Color(1f, 0.84f, 0f) : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        public void OnCardClicked()
        {
            if (mParentUI != null)
            {
                mParentUI.OnCardSelected(mChoice);
            }
        }
    }
}
