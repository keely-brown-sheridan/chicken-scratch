using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChickenScratch
{
    public class GameManager : Singleton<GameManager>
    {
        public enum GameScene
        {
            lobby, game, theater
        }

        public GameScene currentGameScene;
        public Color workerColour, traitorColour;
        public GameObject linePrefab;
        public HideCursorOnHover cursorHider;
        public GameObject submitBtn;
        public Sprite folderFailSprite, folderSuccessSprite;

        public GameFlowManager gameFlowManager;
        public PlayerFlowManager playerFlowManager;

        public DrawingTestManager drawingTestManager;
        public string gameID;
        public DCManager dcManager;

        public GameDataHandler gameDataHandler;

        // Start is called before the first frame update
        void Start()
        {
            if (!gameFlowManager)
            {
                gameFlowManager = FindObjectOfType<GameFlowManager>();
            }

            if (NetworkServer.connections.Count > 0)
            {
                if (gameFlowManager)
                {
                    gameFlowManager.gameObject.SetActive(true);
                }

            }
        }

        public void RestartGame()
        {
            GameManager.Instance.gameDataHandler.RpcRestartGame();
            SceneManager.LoadScene("Game");
        }
    }
}