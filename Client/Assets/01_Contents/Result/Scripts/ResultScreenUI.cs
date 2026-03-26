using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vams2.Core;

namespace Vams2.Result
{
    public class ResultScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text mResultText;
        [SerializeField] private TMP_Text mKillCountText;
        [SerializeField] private TMP_Text mLevelText;
        [SerializeField] private TMP_Text mPlayTimeText;
        [SerializeField] private Button mReturnButton;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                SessionResult result = GameManager.Instance.SessionResult;
                Initialize(result);
            }

            if (mReturnButton != null)
            {
                mReturnButton.onClick.AddListener(OnReturnClicked);
            }
        }

        public void Initialize(SessionResult result)
        {
            if (mResultText != null)
            {
                mResultText.text = result.mIsCleared ? "클리어!" : "패배";
                mResultText.color = result.mIsCleared ? Color.yellow : Color.red;
            }

            if (mKillCountText != null)
            {
                mKillCountText.text = "처치 수: " + result.mKillCount;
            }

            if (mLevelText != null)
            {
                mLevelText.text = "도달 레벨: " + result.mPlayerLevel;
            }

            if (mPlayTimeText != null)
            {
                int minutes = (int)(result.mPlayTime / 60f);
                int seconds = (int)(result.mPlayTime % 60f);
                mPlayTimeText.text = string.Format("플레이 타임: {0:00}:{1:00}", minutes, seconds);
            }
        }

        private void OnReturnClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToLobby();
            }
        }
    }
}
