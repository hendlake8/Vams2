using UnityEngine;

namespace Vams2.Core
{
    public enum GameState
    {
        Title = 0,
        Lobby,
        InGame,
        Result,
        Max
    }

    public class SessionResult
    {
        public int mKillCount;
        public int mPlayerLevel;
        public float mPlayTime;
        public bool mIsCleared;

        public void Reset()
        {
            mKillCount = 0;
            mPlayerLevel = 1;
            mPlayTime = 0f;
            mIsCleared = false;
        }
    }

    public class GameManager : MonoBehaviour
    {
        private static GameManager mInstance;
        public static GameManager Instance => mInstance;

        private GameState mCurrentState;
        public GameState CurrentState => mCurrentState;

        private SessionResult mSessionResult;
        public SessionResult SessionResult => mSessionResult;

        private void Awake()
        {
            if (mInstance != null && mInstance != this)
            {
                Destroy(gameObject);
                return;
            }
            mInstance = this;
            DontDestroyOnLoad(gameObject);

            mSessionResult = new SessionResult();
            mCurrentState = GameState.Title;
        }

        public void StartGame()
        {
            mSessionResult.Reset();
            mCurrentState = GameState.InGame;
            SceneLoader.LoadScene("InGameScene");
        }

        public void EndGame(bool isCleared)
        {
            mSessionResult.mIsCleared = isCleared;
            mCurrentState = GameState.Result;
            SceneLoader.LoadScene("ResultScene");
        }

        public void ReturnToLobby()
        {
            mCurrentState = GameState.Lobby;
            SceneLoader.LoadScene("LobbyScene");
        }

        public void GoToTitle()
        {
            mCurrentState = GameState.Title;
            SceneLoader.LoadScene("TitleScene");
        }
    }
}
