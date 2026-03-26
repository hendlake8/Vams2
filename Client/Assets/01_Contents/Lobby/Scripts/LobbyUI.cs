using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vams2.Core;

namespace Vams2.Lobby
{
    // 로비 화면: 캐릭터 정보 + 출격 버튼
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text mCharacterNameText;
        [SerializeField] private TMP_Text mStatsText;
        [SerializeField] private Button mStartButton;

        private void Start()
        {
            // GameManager 없으면 생성
            if (GameManager.Instance == null)
            {
                GameObject gmGo = new GameObject("GameManager");
                gmGo.AddComponent<GameManager>();
            }

            if (mCharacterNameText != null)
            {
                mCharacterNameText.text = "루드";
            }

            if (mStatsText != null)
            {
                mStatsText.text = "HP: 100  ATK: 10";
            }

            if (mStartButton != null)
            {
                mStartButton.onClick.AddListener(OnStartClicked);
            }
        }

        private void OnStartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
