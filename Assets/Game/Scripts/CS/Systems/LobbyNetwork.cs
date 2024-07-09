
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Steamworks;
using Mirror;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class LobbyNetwork : Singleton<LobbyNetwork>
    {
        public LobbyDataHandler lobbyDataHandler;
        public int loadedPlayers = 0;

        [SerializeField]
        private LobbyNotReadyManager lobbyNotReadyManager;
        private List<CSteamID> roomIDs = new List<CSteamID>();
        

        private string sceneName;
        protected Callback<LobbyCreated_t> m_LobbyCreated;
        protected Callback<LobbyEnter_t> m_LobbyEntered;
        protected Callback<LobbyMatchList_t> m_LobbyListReceived;
        protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdated;
        private bool inLobby = false;

        [SerializeField]
        private GameObject networkManagerPrefab;

        private void Awake()
        {
            if(CSNetworkManager.singleton == null)
            {
                Instantiate(networkManagerPrefab);
            }
        }

        // Start is called before the first frame update
        void Start()
        {

            Screen.fullScreen = false;
            
            
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
            SettingsManager.Instance.currentRoomID = new CSteamID(pCallback.m_ulSteamIDLobby);
            SteamMatchmaking.SetLobbyData(SettingsManager.Instance.currentRoomID, "isPrivate", MenuLobbyButtons.Instance.privacyToggle.isOn.ToString());
 
            SteamMatchmaking.SetLobbyData(SettingsManager.Instance.currentRoomID, "roomName", MenuLobbyButtons.Instance.createRoomInputField.text);

            SettingsManager.Instance.GenerateRoomCode();
            SteamMatchmaking.SetLobbyData(SettingsManager.Instance.currentRoomID, "roomCode", SettingsManager.Instance.roomCode);

            CSteamID hostID = SteamMatchmaking.GetLobbyOwner(SettingsManager.Instance.currentRoomID);
            NetworkManager.singleton.networkAddress = hostID.ToString();
            CSNetworkManager.singleton.StartHost();
            SteamMatchmaking.SetLobbyData(new CSteamID(pCallback.m_ulSteamIDLobby), "HostAddress", SteamUser.GetSteamID().ToString());
            
            inLobby = true;

        }

        public void OnEnteredSteamLobby(LobbyEnter_t pCallback)
        {
            
            //Connecting to Server now
            SettingsManager.Instance.currentRoomID = new CSteamID(pCallback.m_ulSteamIDLobby);

            //Get the SteamID of the host
            string hostID = SteamMatchmaking.GetLobbyData(new CSteamID(pCallback.m_ulSteamIDLobby), "HostAddress");
            NetworkManager.singleton.networkAddress = hostID.ToString();
            NetworkManager.singleton.StartClient();
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
                CSteamID lobbyIndex = Steamworks.SteamMatchmaking.GetLobbyByIndex(i);
                roomIDs.Add(lobbyIndex);
            }

            MenuLobbyButtons.Instance.UpdateRoomListings(roomIDs);
        }

        public void UpdateClientPlayerListings(List<PlayerListingNetData> playerListingData)
        {
            MenuLobbyButtons.Instance.PlayerListings.Set(playerListingData);
            List<ColourManager.BirdName> selectedBirds = new List<ColourManager.BirdName>();
            foreach(PlayerListingNetData playerListing in playerListingData)
            {
                if(playerListing.selectedBird != ColourManager.BirdName.none && 
                    MenuLobbyButtons.Instance.PlayerIdentificationMap.ContainsKey(playerListing.selectedBird))
                {
                    if(playerListing.playerName == SettingsManager.Instance.playerName)
                    {
                        SettingsManager.Instance.birdName = playerListing.selectedBird;
                        MenuLobbyButtons.Instance.selectionInstructionsObject.SetActive(false);
                    }
                    SettingsManager.Instance.AssignBirdToPlayer(playerListing.selectedBird, playerListing.playerName);
                    selectedBirds.Add(playerListing.selectedBird);
                    MenuLobbyButtons.Instance.PlayerIdentificationMap[playerListing.selectedBird].Select(playerListing.playerName);
                }
            }
            foreach(KeyValuePair<ColourManager.BirdName, PlayerIdentification> playerID in MenuLobbyButtons.Instance.PlayerIdentificationMap)
            {
                if(!selectedBirds.Contains(playerID.Key))
                {
                    SettingsManager.Instance.DeassignBirdToPlayer(playerID.Key);
                    playerID.Value.Deselect();
                }
            }
        }

        public void OnPlayerDisconnectedFromLobby()
        {

        }



        public void OnJoinedLobby()
        {
            MenuLobbyButtons.Instance.LobbyStartGameBtn.interactable = SettingsManager.Instance.isHost;
            MenuLobbyButtons.Instance.WaitingForHostMessageText.SetActive(false);
            MenuLobbyButtons.Instance.gameModeButton.interactable = SettingsManager.Instance.isHost;

            MenuLobbyButtons.Instance.roomCodeText.text = "*****";
            MenuLobbyButtons.Instance.roomCodeText.gameObject.SetActive(true);
            MenuLobbyButtons.Instance.JoinedRoom();

            if (!SettingsManager.Instance.isHost)
            {
                
                if (SteamManager.Initialized)
                {
                    string currentCode = SteamMatchmaking.GetLobbyData(SettingsManager.Instance.currentRoomID, "roomCode");
                    SettingsManager.Instance.SetRoomCode(currentCode);
                }
            }

            inLobby = true;
        }

        #endregion

        
        public bool CreateRoom(string roomName, bool isPrivate)
        {
            Debug.Log("Creating room.");
            ELobbyType lobbyType = isPrivate ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypePublic;
            //Number of players?
            SteamAPICall_t createLobbyResult = Steamworks.SteamMatchmaking.CreateLobby(lobbyType, 8);

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
                    Debug.LogError("Trying to join lobby.");
                    SteamMatchmaking.JoinLobby(roomID);
                    return;
                }
            }
            Debug.Log("Could not find room with specified code[" + roomCode + "] Length[" + roomCode.Length + "].");


        }

        public void JoinRoom(CSteamID roomID)
        {
            SteamMatchmaking.JoinLobby(roomID);
        }





        #region Leave Callbacks
        public void LeaveLobby()
        {
            //PhotonNetwork.LeaveLobby();
            MenuLobbyButtons.Instance.LeftLobby();
        }

        public void LeaveRoom()
        {
            SettingsManager.Instance.DisconnectFromLobby();
            if (SettingsManager.Instance.isHost)
            {
                if(NetworkServer.active)
                {
                    NetworkManager.singleton.StopHost();
                }
            }
            else
            {
                if(NetworkClient.active)
                {
                    NetworkManager.singleton.StopClient();
                }
            }
            
            
            MenuLobbyButtons.Instance.WaitingForHostPrompt.SetActive(false);
        }

        #endregion


        public void LoadLevel(string levelName)
        {
            
            SceneManager.LoadScene(levelName, LoadSceneMode.Single);
        }
    }
}