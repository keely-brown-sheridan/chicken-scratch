using Mirror;
using Steamworks;
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
        
        public bool isHost => NetworkServer.connections.Count > 0;
        public bool isHostInLobby = false;
        public bool disconnected = false;
        public bool playerQuit = false;
        public bool waitingForPlayers = false;
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

        public List<ResultData> resultPossibilities = new List<ResultData>();

        [SerializeField]
        private List<TaskSpriteData> taskSprites;

        public Color prefixBGColour, prefixFontColour;
        public Color nounBGColour, nounFontColour;
        public Gradient scoreModifierGradient;

        private Dictionary<ColourManager.BirdName, string> playerNameMap = new Dictionary<ColourManager.BirdName, string>();
        private Dictionary<ColourManager.BirdName, NetworkConnectionToClient> birdConnectionMap = new Dictionary<ColourManager.BirdName, NetworkConnectionToClient>();
        private Dictionary<CaseChoiceData.TaskSprite, TaskSpriteData> taskSpriteDataMap = new Dictionary<CaseChoiceData.TaskSprite, TaskSpriteData>();

        private Dictionary<NetworkConnectionToClient, string> lobbyConnectionUsernameMap = new Dictionary<NetworkConnectionToClient, string>();
        private Dictionary<NetworkConnectionToClient, ColourManager.BirdName> lobbyConnectionBirdMap = new Dictionary<NetworkConnectionToClient, ColourManager.BirdName>();

        public CSteamID currentRoomID;

        [System.Serializable]
        public class TaskSpriteData
        {
            public CaseChoiceData.TaskSprite taskSprite;
            public Sprite sprite;
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
                MenuLobbyButtons.Instance.gameModeButtonText.text = gameMode.title.ToUpper();
                MenuLobbyButtons.Instance.gameModeInformationHeaderText.text = "Game Mode: " + SettingsManager.Instance.gameMode.title;
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
            foreach(TaskSpriteData taskSprite in taskSprites)
            {
                taskSpriteDataMap.Add(taskSprite.taskSprite, taskSprite);
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

            return gameMode.title;
        }

        public void SetGameMode(string inGameMode)
        {
            MenuLobbyButtons.Instance.gameModeButtonText.text = inGameMode.ToUpper();

            foreach (GameModeData currentGameMode in allGameModes)
            {
                if (currentGameMode.title == inGameMode)
                {
                    gameMode = currentGameMode;
                }
            }
            MenuLobbyButtons.Instance.gameModeInformationHeaderText.text = "Game Mode: " + gameMode.title;
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

        public int GetPlayerNameCount()
        {
            return playerNameMap.Count;
        }

        public void AssignBirdToPlayer(ColourManager.BirdName bird, string playerName)
        {
            if(!playerNameMap.ContainsKey(bird))
            {
                playerNameMap.Add(bird, playerName);
            }
        }

        public void DeassignBirdToPlayer(ColourManager.BirdName bird)
        {
            if(playerNameMap.ContainsKey(bird))
            {
                playerNameMap.Remove(bird);
            }
        }

        public void BroadcastBirdAssignment()
        {
            foreach(KeyValuePair<ColourManager.BirdName,string> birdAssignment in playerNameMap)
            {
                GameManager.Instance.gameDataHandler.RpcSetPlayerBird(birdAssignment.Value, birdAssignment.Key);
            }
            
        }

        public bool IsBirdSelected(ColourManager.BirdName bird)
        {
            return playerNameMap.ContainsKey(bird);
        }

        public void ClearPlayerNameMap()
        {
            playerNameMap.Clear();
        }

        public string GetPlayerName(ColourManager.BirdName birdName)
        {
            if(playerNameMap.ContainsKey(birdName))
            {
                return playerNameMap[birdName];
            }
            return "";
        }

        public void AssignBirdToConnection(ColourManager.BirdName bird, NetworkConnectionToClient connection)
        {
            if(!birdConnectionMap.ContainsKey(bird))
            {
                birdConnectionMap.Add(bird, connection);
            }
        }

        public NetworkConnectionToClient GetConnection(ColourManager.BirdName bird)
        {
            if(birdConnectionMap.ContainsKey(bird))
            {
                return birdConnectionMap[bird];
            }
            Debug.LogError("Could not find connection for bird["+bird.ToString()+"] in connections["+birdConnectionMap.Count.ToString()+"].");
            return null;
        }

        public List<ColourManager.BirdName> GetAllActiveBirds()
        {
            return playerNameMap.Keys.ToList();
        }

        public ColourManager.BirdName GetDisconnectedPlayerBird(NetworkConnectionToClient connection)
        {
            foreach(KeyValuePair<ColourManager.BirdName, NetworkConnectionToClient> birdConnection in birdConnectionMap)
            {
                if(birdConnection.Value == connection)
                {
                    return birdConnection.Key;
                }
            }
            return ColourManager.BirdName.none;
        }

        public void ServerBroadcastPlayerNames()
        {
            GameManager.Instance.gameDataHandler.RpcPlayerInitializationWrapper(SettingsManager.Instance.playerNameMap);
        }

        public Sprite GetTaskSprite(CaseChoiceData.TaskSprite inTaskSpriteType)
        {
            if(taskSpriteDataMap.ContainsKey(inTaskSpriteType))
            {
                return taskSpriteDataMap[inTaskSpriteType].sprite;
            }
            return null;
        }

        public void DisconnectFromLobby()
        {
            if(currentRoomID != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(currentRoomID);
                currentRoomID = CSteamID.Nil;
            }
            lobbyConnectionBirdMap.Clear();
            lobbyConnectionUsernameMap.Clear();
        }

        public void SetBirdForPlayerID(NetworkConnectionToClient playerID, ColourManager.BirdName bird)
        {
            if (!lobbyConnectionBirdMap.ContainsKey(playerID))
            {
                lobbyConnectionBirdMap.Add(playerID, bird);
            }
            else
            {
                lobbyConnectionBirdMap[playerID] = bird;
            }
        }

        public void ServerRefreshBirds()
        {
            List<PlayerListingNetData> playerListingData = new List<PlayerListingNetData>();
            foreach (KeyValuePair<NetworkConnectionToClient, ColourManager.BirdName> connectedPlayer in SettingsManager.Instance.lobbyConnectionBirdMap)
            {
                PlayerListingNetData data = new PlayerListingNetData();
                data.playerName = lobbyConnectionUsernameMap.ContainsKey(connectedPlayer.Key) ? lobbyConnectionUsernameMap[connectedPlayer.Key] : "";
                data.selectedBird = lobbyConnectionBirdMap.ContainsKey(connectedPlayer.Key) ? lobbyConnectionBirdMap[connectedPlayer.Key] : ColourManager.BirdName.none;
                playerListingData.Add(data);
            }

            LobbyNetwork.Instance.lobbyDataHandler.RpcSetPlayerListings(playerListingData);
        }

        public void AddConnection(NetworkConnectionToClient connection)
        {
            if (!lobbyConnectionUsernameMap.ContainsKey(connection))
            {
                lobbyConnectionUsernameMap.Add(connection, "");
            }
            if (!lobbyConnectionBirdMap.ContainsKey(connection))
            {
                lobbyConnectionBirdMap.Add(connection, ColourManager.BirdName.none);
            }
        }

        public void SetConnectionSteamName(string steamName, NetworkConnectionToClient connection)
        {
            if (lobbyConnectionUsernameMap.ContainsKey(connection))
            {
                lobbyConnectionUsernameMap[connection] = steamName;
            }
            ServerRefreshBirds();
        }

        public void RemoveConnection(NetworkConnectionToClient connection)
        {
            if (lobbyConnectionBirdMap.ContainsKey(connection))
            {
                lobbyConnectionBirdMap.Remove(connection);
            }
            if (lobbyConnectionUsernameMap.ContainsKey(connection))
            {
                lobbyConnectionUsernameMap.Remove(connection);
            }
        }

        public string CreatePromptText(string prefixText, string nounText)
        {
            string prefixHexcode = ColorUtility.ToHtmlStringRGB(prefixFontColour);
            string nounHexcode = ColorUtility.ToHtmlStringRGB(nounFontColour);
            string fullPrefixValue = "<color=#"+ prefixHexcode + ">" + prefixText + "</color>";
            string fullNounValue = "<color=#"+nounHexcode+">" + nounText + "</color>";
            return fullPrefixValue + " " + fullNounValue;
        }

        public string CreatePrefixText(string prefixText)
        {
            string hexcode = ColorUtility.ToHtmlStringRGB(prefixFontColour);
            return "<color=#" + hexcode + ">" + prefixText + "</color>";
        }

        public string CreateNounText(string nounText)
        {
            string hexcode = ColorUtility.ToHtmlStringRGB(nounFontColour);
            return "<color=#" + hexcode + ">" + nounText + "</color>";
        }

        public Color GetModifierColour(float ratio)
        {
            Color modifierColour = scoreModifierGradient.Evaluate(ratio);
            return modifierColour;
        }

        public int GetCurrentGoal()
        {
            return gameMode.days[GameManager.Instance.playerFlowManager.currentDay].goalPerPlayer * GetPlayerNameCount();
        }

        public int GetCaseCountForDay()
        {
            return (int)(gameMode.days[GameManager.Instance.playerFlowManager.currentDay].casesPerPlayer * GetPlayerNameCount());
        }

        public ResultData GetDayResult()
        {
            DayData currentDayData = gameMode.days[GameManager.Instance.playerFlowManager.currentDay];
            if(DidPlayersPassDay())
            {
                return currentDayData.winResult;
            }
            else
            {
                return currentDayData.loseResult;
            }
        }

        public bool DidPlayersPassDay()
        {
            return GameManager.Instance.playerFlowManager.slidesRound.currentBirdBuckTotal >= GetCurrentGoal();
        }
    }
}