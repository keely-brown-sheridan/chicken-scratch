
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Steamworks;
using Mirror;

namespace ChickenScratch
{
    public class LobbyNetwork : Singleton<LobbyNetwork>
    {
        public LobbyDataHandler lobbyDataHandler;

        [SerializeField]
        private LobbyNotReadyManager lobbyNotReadyManager;
        private List<CSteamID> roomIDs = new List<CSteamID>();
        private CSteamID currentRoomID;

        private string sceneName;
        protected Callback<LobbyCreated_t> m_LobbyCreated;
        protected Callback<LobbyEnter_t> m_LobbyEntered;
        protected Callback<LobbyMatchList_t> m_LobbyListReceived;
        protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdated;
        private bool inLobby = false;

        private Dictionary<string, ColourManager.BirdName> playerIDBirdMap = new Dictionary<string, ColourManager.BirdName>();
        //protected Callback<GameLobbyJoinRequested_t>

        // Start is called before the first frame update
        void Start()
        {

            //Screen.fullScreen = true;
            AudioManager.Instance.PlaySound("SplashMusic");
            AudioManager.Instance.PlaySound("sfx_lobby_amb_outdoor");
            if (SteamManager.Initialized)
            {
                string name = SteamFriends.GetPersonaName();
                Debug.Log("User["+name+"] is connected to Steam.");
                m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnCreatedSteamRoom);
                m_LobbyEntered = Callback<LobbyEnter_t>.Create(OnEnteredSteamLobby);
                m_LobbyListReceived = Callback<LobbyMatchList_t>.Create(OnSteamLobbyListReceived);
               // m_LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnSteamLobbyDataUpdate);

            }
            else
            {
                Debug.Log("Not connected to Steam.");
            }
            sceneName = SceneManager.GetActiveScene().name;
        }

        #region Create/Join/Connect Callbacks

        public void OnDisconnect()
        {
            //Close the room
            MenuLobbyButtons.Instance.PlayerPrompt.Activate("You have been disconnected.");
            MenuLobbyButtons.Instance.LobbyPageObject.SetActive(false);
            inLobby = false;
        }

        public void OnCreatedSteamRoom(LobbyCreated_t pCallback)
        {
            print("Created room:" + pCallback.m_ulSteamIDLobby.ToString());

            currentRoomID = new CSteamID(pCallback.m_ulSteamIDLobby);
 
            SteamMatchmaking.SetLobbyData(currentRoomID, "roomName", MenuLobbyButtons.Instance.createRoomInputField.text);
            SteamMatchmaking.SetLobbyData(currentRoomID, "roomCode", SettingsManager.Instance.roomCode);
            
            inLobby = true;

        }

        public void OnEnteredSteamLobby(LobbyEnter_t pCallback)
        {
            print("Joined room.");
            //Connecting to Server now
            currentRoomID = new CSteamID(pCallback.m_ulSteamIDLobby);
        }

        public void RequestSteamRoomListings()
        {
            Steamworks.SteamMatchmaking.RequestLobbyList();
        }

        public void OnSteamLobbyListReceived(LobbyMatchList_t pCallback)
        {
            roomIDs = new List<CSteamID>();
            for (int i = 0; i < pCallback.m_nLobbiesMatching; i++)
            {
                roomIDs.Add(Steamworks.SteamMatchmaking.GetLobbyByIndex(i));
                
            }

            MenuLobbyButtons.Instance.UpdateRoomListings(roomIDs);
        }

        public void ServerUpdatePlayerListings()
        {
            ServerRefreshBirds();
            //lobbyNotReadyManager.gameModeHasEnoughPlayers = SettingsManager.Instance.gameMode.numberOfPlayers <= PhotonNetwork.PlayerList.Count();
        }

        public void UpdateClientPlayerListings(List<PlayerListingNetData> playerListingData)
        {
            MenuLobbyButtons.Instance.PlayerListings.Set(playerListingData);
            foreach(PlayerListingNetData playerListing in playerListingData)
            {
                if(playerListing.selectedBird != ColourManager.BirdName.none)
                {
                    MenuLobbyButtons.Instance.PlayerIdentificationMap[playerListing.selectedBird].Select(playerListing.playerName);
                }
            }
        }

        public void OnPlayerDisconnectedFromLobby()
        {

        }

        public void OnJoinedLobby()
        {
            NetworkManager.singleton.StartHost();
            MenuLobbyButtons.Instance.JoinedLobby();
            MenuLobbyButtons.Instance.JoinedRoom();

            MenuLobbyButtons.Instance.LobbyStartGameBtn.interactable = SettingsManager.Instance.isHost;
            MenuLobbyButtons.Instance.WaitingForHostMessageText.SetActive(false);
            MenuLobbyButtons.Instance.gameModeButton.interactable = SettingsManager.Instance.isHost;

            SettingsManager.Instance.GenerateRoomCode();
            MenuLobbyButtons.Instance.roomCodeText.text = "*****";
            MenuLobbyButtons.Instance.roomCodeText.gameObject.SetActive(true);
            MenuLobbyButtons.Instance.JoinedRoom();

            if(!SettingsManager.Instance.isHost)
            {
                Text startGameButtonText = MenuLobbyButtons.Instance.LobbyStartGameBtn.GetComponentInChildren<Text>();
                if (startGameButtonText)
                {
                    startGameButtonText.gameObject.SetActive(false);
                }
            }

            if(SteamManager.Initialized)
            {
                string currentCode = SteamMatchmaking.GetLobbyData(currentRoomID, "roomCode");
                SettingsManager.Instance.SetRoomCode(currentCode);
                MenuLobbyButtons.Instance.roomCodeText.text = "*****";
                MenuLobbyButtons.Instance.roomCodeText.gameObject.SetActive(true);
                
            }

            inLobby = true;
        }

        #endregion

        
        public bool CreateRoom(string roomName, bool isPrivate)
        {
            Debug.Log("Creating room.");
            ELobbyType lobbyType = isPrivate ? ELobbyType.k_ELobbyTypePrivate : ELobbyType.k_ELobbyTypePublic;
            //Number of players?
            SteamAPICall_t createLobbyResult = Steamworks.SteamMatchmaking.CreateLobby(lobbyType, 8);

            
            //PhotonNetwork.CreateRoom(roomName + GameDelim.BASE + isPrivate.ToString() + GameDelim.BASE + SettingsManager.Instance.roomCode, roomOptions, TypedLobby.Default);
            
            return true;
        }

        public void JoinRoomByCode(string roomCode)
        {
            //Get room code from name
            foreach (CSteamID roomID in roomIDs)
            {
                string currentCode = SteamMatchmaking.GetLobbyData(roomID, "roomCode");
                if (currentCode == roomCode)
                {
                    Debug.Log("Trying to join room.");
                    //Get the SteamID of the host
                    CSteamID hostID = SteamMatchmaking.GetLobbyOwner(roomID);
                    NetworkManager.singleton.networkAddress = hostID.ToString();
                    NetworkManager.singleton.StartClient();
                    return;
                }
            }
            Debug.Log("Could not find room with specified code[" + roomCode + "] Length[" + roomCode.Length + "].");


        }

        public void JoinRoom(CSteamID roomID)
        {
            Debug.Log("Trying to join room.");
            CSteamID hostID = SteamMatchmaking.GetLobbyOwner(roomID);
            NetworkManager.singleton.networkAddress = hostID.ToString();
            NetworkManager.singleton.StartClient();
        }

        private void ServerRefreshBirds()
        {
            List<PlayerListingNetData> playerListingData = new List<PlayerListingNetData>();
            if (SteamManager.Initialized)
            {
                int numberOfPlayers = Steamworks.SteamMatchmaking.GetNumLobbyMembers(currentRoomID);
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    CSteamID playerID = Steamworks.SteamMatchmaking.GetLobbyMemberByIndex(currentRoomID, i);
                    string nickname = SteamFriends.GetFriendPersonaName(playerID);
                    PlayerListingNetData data = new PlayerListingNetData();
                    data.playerID = playerID.m_SteamID.ToString();
                    data.playerName = nickname;
                    data.selectedBird = playerIDBirdMap.ContainsKey(data.playerID) ? playerIDBirdMap[data.playerID] : ColourManager.BirdName.none;
                    playerListingData.Add(data);
                }
            }
            else
            {
                foreach (KeyValuePair<int, NetworkConnectionToClient> connection in NetworkServer.connections)
                {
                    PlayerListingNetData data = new PlayerListingNetData();
                    data.playerID = connection.Key.ToString();
                    data.playerName = connection.Key.ToString();
                    data.selectedBird = playerIDBirdMap.ContainsKey(data.playerID) ? playerIDBirdMap[data.playerID] : ColourManager.BirdName.none;
                    playerListingData.Add(data);
                }
            }

            lobbyDataHandler.RpcSetPlayerListings(playerListingData);
        }

        public void SetBirdForPlayerID(string playerID, ColourManager.BirdName bird)
        {
            if(!playerIDBirdMap.ContainsKey(playerID))
            {
                playerIDBirdMap.Add(playerID, bird);
            }
            else
            {
                playerIDBirdMap[playerID] = bird;
            }
        }

        #region Leave Callbacks
        public void LeaveLobby()
        {
            //PhotonNetwork.LeaveLobby();
            MenuLobbyButtons.Instance.LeftLobby();
        }

        public void LeaveRoom()
        {
            NetworkManager.singleton.StopClient();
            MenuLobbyButtons.Instance.WaitingForHostPrompt.SetActive(false);
        }

        #endregion


        public void LoadLevel(string levelName)
        {
            
            SceneManager.LoadScene(levelName, LoadSceneMode.Single);
        }
    }
}