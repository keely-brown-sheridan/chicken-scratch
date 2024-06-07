using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChickenScratch
{
    public class SettingsManager : Singleton<SettingsManager>
    {
        public ColourManager.BirdName birdName;
        public Dictionary<ColourManager.BirdName, string> playerNameMap = new Dictionary<ColourManager.BirdName, string>();
        public Dictionary<ColourManager.BirdName, NetworkConnectionToClient> birdConnectionMap = new Dictionary<ColourManager.BirdName, NetworkConnectionToClient>();
        public bool isHost => NetworkServer.connections.Count > 0;
        public bool isHostInLobby = false;
        public bool disconnected = false;
        public bool playerQuit = false;
        public enum SceneTransitionState
        {
            disconnected, return_to_lobby_room, return_to_room_listings, invalid
        }
        public SceneTransitionState currentSceneTransitionState = SceneTransitionState.invalid;
        public List<string> intSettingNames = new List<string>();
        public List<string> stringSettingNames = new List<string>();
        public int winningThreshold = 1;
        public int correctCabinetThreshold = 1;

        public List<GameModeData> allGameModes = new List<GameModeData>();
        public GameModeData gameMode;
        public bool showFastResults = false;

        public string playerName => SteamManager.Initialized ? Steamworks.SteamFriends.GetPersonaName() : playerID;
        public string playerID = "";
        public string roomCode => _roomCode;

        private string _roomCode = "";

        public List<string> wordGroupNames = new List<string>();

        private Vector2 defaultScreenSize = new Vector2(1280, 720);

        private Dictionary<string, int> intSettings;
        private Dictionary<string, string> stringSettings;
        private bool isInitialized = false;

        public List<EndgameResult> resultPossibilities = new List<EndgameResult>();

        [System.Serializable]
        public class EndgameResult
        {
            public Color goalTextColour = Color.black;
            public Color sheetColour = new Color(0.5f, 0.5f, 0.0f);
            public Color resultTextColour = Color.black;
            public Color slideProgressBGColour;
            public Color slideProgressFillColour;
            public Material lineMaterial = null;
            public string resultName = "";
            public string shortFormIdentifier = "";
            public Sprite bossFaceReaction = null;
            public FinalEndgameResultManager.State finalFaceState;
            public string bossMessage = "";
            public string sfxToPlay = "";
            public List<GameModeRequiredPointThreshold> requiredPointThresholds;
            public WorkingGoalsManager.GoalType goal;

            public float getRequiredPointThreshold(string gameModeName)
            {
                foreach (GameModeRequiredPointThreshold requiredPointThreshold in requiredPointThresholds)
                {
                    if (gameModeName == requiredPointThreshold.gameModeName)
                    {
                        return requiredPointThreshold.requiredPointThreshold;
                    }
                }
                return -1;
            }

            [System.Serializable]
            public class GameModeRequiredPointThreshold
            {
                public string gameModeName = "";
                public float requiredPointThreshold = -1;
            }
        }

        private void Awake()
        {
            initialize();
        }

        private void initialize()
        {
            if (isInitialized)
            {
                return;
            }
            Screen.SetResolution((int)defaultScreenSize.x, (int)defaultScreenSize.y, false);

            DontDestroyOnLoad(this);

            intSettings = new Dictionary<string, int>();
            stringSettings = new Dictionary<string, string>();
            gameMode = allGameModes[0];
            if (MenuLobbyButtons.Instance.gameModeButtonText != null)
            {
                MenuLobbyButtons.Instance.gameModeButtonText.text = gameMode.name.ToUpper();
                MenuLobbyButtons.Instance.gameModeInformationHeaderText.text = "Game Mode: " + SettingsManager.Instance.gameMode.name;
                MenuLobbyButtons.Instance.gameModeDescriptionText.text = SettingsManager.Instance.gameMode.description;
            }
            foreach (string settingName in intSettingNames)
            {
                if (intSettings.ContainsKey(settingName))
                {
                    continue;
                }

                if (PlayerPrefs.HasKey(settingName))
                {
                    intSettings.Add(settingName, PlayerPrefs.GetInt(settingName));
                }
                else
                {
                    intSettings.Add(settingName, 1);
                }

            }
            foreach (string settingName in stringSettingNames)
            {
                if (stringSettings.ContainsKey(settingName))
                {
                    continue;
                }

                if (PlayerPrefs.HasKey(settingName))
                {
                    stringSettings.Add(settingName, PlayerPrefs.GetString(settingName));
                }
                else
                {
                    stringSettings.Add(settingName, "");
                }

            }
            isInitialized = true;
        }

        private void Update()
        {

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            if (this.defaultScreenSize != screenSize)
            {


            }
        }

        public void UpdateSetting(string inSettingName, int inSettingValue)
        {
            if (!intSettings.ContainsKey(inSettingName))
            {
                intSettings.Add(inSettingName, inSettingValue);
            }
            else
            {
                intSettings[inSettingName] = inSettingValue;
            }
            PlayerPrefs.SetInt(inSettingName, inSettingValue);
        }

        public void UpdateSetting(string inSettingName, string inSettingValue)
        {
            if (!stringSettings.ContainsKey(inSettingName))
            {
                stringSettings.Add(inSettingName, inSettingValue);
            }
            else
            {
                stringSettings[inSettingName] = inSettingValue;
            }
            PlayerPrefs.SetString(inSettingName, inSettingValue);
        }

        public void UpdateSetting(string inSettingName, bool inSettingValue)
        {
            if (!intSettings.ContainsKey(inSettingName))
            {
                intSettings.Add(inSettingName, inSettingValue ? 1 : 0);
            }
            else
            {
                intSettings[inSettingName] = inSettingValue ? 1 : 0;
            }
            PlayerPrefs.SetInt(inSettingName, inSettingValue ? 1 : 0);
        }

        public bool GetSetting(string inSettingName, bool defaultValue = true)
        {
            if (intSettings.ContainsKey(inSettingName))
            {
                return (intSettings[inSettingName] == 1);
            }
            else
            {
                return defaultValue;
            }


        }

        public string GetStringSetting(string inSettingName, string defaultValue = "")
        {
            if (stringSettings.ContainsKey(inSettingName))
            {
                return (stringSettings[inSettingName]);
            }
            else
            {
                return defaultValue;
            }
        }

        public int GetIntegerSetting(string inSettingName, int defaultValue = 1)
        {
            if (intSettings.ContainsKey(inSettingName))
            {
                return (intSettings[inSettingName]);
            }
            return defaultValue;
        }

        public string ChangeGameMode()
        {
            int currentIndex = allGameModes.IndexOf(gameMode);
            currentIndex++;
            if (currentIndex >= allGameModes.Count)
            {
                currentIndex = 0;
            }

            gameMode = allGameModes[currentIndex];

            return gameMode.name;
        }

        public void SetGameMode(string inGameMode)
        {
            MenuLobbyButtons.Instance.gameModeButtonText.text = inGameMode.ToUpper();

            foreach (GameModeData currentGameMode in allGameModes)
            {
                if (currentGameMode.name == inGameMode)
                {
                    gameMode = currentGameMode;
                }
            }
            MenuLobbyButtons.Instance.gameModeInformationHeaderText.text = "Game Mode: " + gameMode.name;
            MenuLobbyButtons.Instance.gameModeDescriptionText.text = gameMode.description;
        }

        public void GenerateRoomCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[11];
            var random = new System.Random();

            for (int i = 0; i < 5; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            _roomCode = new String(stringChars);

        }

        public void SetRoomCode(string inRoomCode)
        {
            _roomCode = inRoomCode;
        }
    }
}