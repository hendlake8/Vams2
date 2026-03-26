using UnityEngine;
using Vams2.Core;

namespace Vams2.Title
{
    // 타이틀 화면: 터치하여 로비로 이동
    public class TitleUI : MonoBehaviour
    {
        private void Update()
        {
            // 아무 곳이나 터치/클릭 시 로비로
            if (UnityEngine.InputSystem.Touchscreen.current != null &&
                UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                GoToLobby();
                return;
            }

            if (UnityEngine.InputSystem.Mouse.current != null &&
                UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                GoToLobby();
            }
        }

        private void GoToLobby()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToLobby();
            }
            else
            {
                // GameManager가 없으면 직접 씬 전환
                SceneLoader.LoadScene("LobbyScene");
            }
        }
    }
}
