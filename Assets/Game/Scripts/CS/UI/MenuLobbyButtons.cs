
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;
using static ChickenScratch.ColourManager;
using Steamworks;
using Mirror;

namespace ChickenScratch
{
    public class MenuLobbyButtons : Singleton<MenuLobbyButtons>
    {
        public enum MenuState
        {
            splash, zooming_in, fading_out, fading_in, pressing_pc_button, booting_pc, rooms, lobby, waiting
        }
        public MenuState currentMenuState = MenuState.splash;
        private MenuState previousMenuState = MenuState.splash;

        public GameObject SplashPageObject, RoomsPageObject, LobbyPageObject, LoadingPageObject;
        public List<PlayerIdentification> PlayerIdentifiers = new List<PlayerIdentification>();

        public Text CreateRoomNameText, SetPlayerNameText, RoomListingsPlayerNameText;
        public Button LobbyStartGameBtn;
        public GameObject LobbyStartGameBtnTextObject;
        public GameObject WaitingForHostMessageText;

        public CameraDock baseTransitionCameraDock;
        public CameraDock splashTransitionCameraDock;
        public RectTransform splashRectTransform;
        public GameObject openDoorObject;

        public GameObject RoomListingPrefab;
        public GameObject WaitingForHostPrompt;
        public Transform RoomListingsParent;

        public PlayerLayoutGroup PlayerListings;
        public PlayerIdentification SelectedID;
        public float TimeForIssueLogging = 4.0f;
        public Dictionary<ColourManager.BirdName, PlayerIdentification> PlayerIdentificationMap = new Dictionary<ColourManager.BirdName, PlayerIdentification>();
        public Dictionary<string, PlayerListing> PlayerListingMap = new Dictionary<string, PlayerListing>();

        public PlayerMessagePrompt PlayerPrompt;
        public MusicButton musicButton;

        public TutorialButton stickiesButton, tutorialsButton;

        public GameObject lobbyBGObject, logoObject, loginObject, roomListingsObject, createGameObject, joinGameObject;
        public InputField loginInputField, createRoomInputField, joinGameInputField;

        public GameObject settingsObject;
        public Text drawingRoundSettingText, accuseRoundSettingText, correctCabinetThresholdSettingText;
        public Slider drawingRoundSettingSlider, accuseRoundSettingSlider, correctCabinetThresholdSettingSlider;

        public float timeToBootUp = 2.0f, timeToTurnOn = 1.0f;
        public float timeToFadeIn = 1.0f, timeToFadeOut = 1.0f;

        public Image lobbyScreenImage;
        public Image splashScreenFadeImage;
        public Text gameModeInformationHeaderText;
        public Text gameModeDescriptionText;
        public Text gameModeButtonText;
        public Button gameModeButton;

        public TMPro.TMP_Text roomCodeText;

        [SerializeField]
        private GameObject roomCodeEyeOpenObject;
        [SerializeField]
        private GameObject roomCodeEyeClosedObject;

        public InputField findRoomCodeText;
        [SerializeField]
        private TMPro.TMP_Text computerSticky1Text;
        [SerializeField]
        private TMPro.TMP_Text computerSticky2Text;

        [SerializeField]
        private List<string> possibleComputerStickyMessages = new List<string>();

        [SerializeField]
        private GameObject textCopiedNotificationObject;

        public WordGroupsController wordGroupsController;

        public List<Animator> splashArmAnimators;

        public GameObject selectionInstructionsObject;
        [SerializeField]
        private GameObject selectedBirdParentObject;
        [SerializeField]
        private Image selectedBirdImage;
        public Toggle privacyToggle;
        [SerializeField]
        private PauseMenu splashScreenPauseMenu;
        [SerializeField]
        private GameObject quitCheckPromptObject;

        [SerializeField]
        private GameObject skipIntroButton;

        public LobbyNotReadyManager lobbyNotReadyManager;

        public Toggle exportTasksToggle;
        [SerializeField]
        private GameObject exportTasksContainerObject;


        private Dictionary<BirdName, Animator> splashArmAnimatorMap = new Dictionary<BirdName, Animator>();

        private List<RoomListing> activeRoomListings = new List<RoomListing>();

        private RoomListing selectedRoomListing;
        private float timeTurningOn = 0.0f, timeBootingUp = 0.0f;
        [SerializeField]
        private float buttonPressTimeThreshold;
        [SerializeField]
        private float roomUpdateThreshold = 5f;


        private float timeFadingIn = 0.0f, timeFadingOut = 0.0f;

        public Color activeBirdColour, inactiveBirdColour;

        private bool isInitialized = false;
        private string sceneName = "";
        private bool buttonHasBeenPressed = false;
        private float timeSinceLastRoomListingUpdate = 0f;
        private static bool hasSkippedIntro = false;

        private void Start()
        {
            if (!isInitialized)
            {
                initialize();
            }
        }

        private bool initialize()
        {
            sceneName = SceneManager.GetActiveScene().name;
            if (sceneName != "MainMenu")
            {
                return false;
            }

            foreach (PlayerIdentification lobbyID in PlayerIdentifiers)
            {
                PlayerIdentificationMap.Add(lobbyID.birdName, lobbyID);
            }

            foreach (Animator splashArmAnimator in splashArmAnimators)
            {
                splashArmAnimatorMap.Add(splashArmAnimator.GetComponent<BirdTag>().birdName, splashArmAnimator);
            }

            possibleComputerStickyMessages = possibleComputerStickyMessages.OrderBy(a => Guid.NewGuid()).ToList();
            computerSticky1Text.text = possibleComputerStickyMessages[0];
            computerSticky2Text.text = possibleComputerStickyMessages[1];

            if (NetworkClient.isConnected && SettingsManager.Instance.currentSceneTransitionState == SettingsManager.SceneTransitionState.return_to_lobby_room)
            {
                currentMenuState = MenuState.lobby;
                AudioManager.Instance.PlaySound("LobbyMusic");
                roomListingsObject.SetActive(true);
                //We're already connected to a game so we should load up the lobby page
                LoadLobbyFromGame();
            }
            else if (SettingsManager.Instance.currentSceneTransitionState == SettingsManager.SceneTransitionState.return_to_room_listings)
            {
                currentMenuState = MenuState.rooms;
                AudioManager.Instance.PlaySound("LobbyMusic");
                SkipIntro();
                roomListingsObject.SetActive(true);
                RoomListingsPlayerNameText.text = "User: \n" + SettingsManager.Instance.playerName;
                SettingsManager.Instance.UpdateSetting("user_name", SettingsManager.Instance.playerName);
                SettingsManager.Instance.currentSceneTransitionState = SettingsManager.SceneTransitionState.invalid;
            }
            else
            {
                AudioManager.Instance.PlaySound("SplashMusic");
                AudioManager.Instance.PlaySound("sfx_lobby_amb_outdoor");
            }

            stickiesButton.Initialize(SettingsManager.Instance.GetSetting("stickies", true));
            if (!SettingsManager.Instance.GetSetting("music", true))
            {
                //AudioManager.Instance.SetMusic(false);
                musicButton.musicButtonStatusImage.sprite = musicButton.offSprite;
            }

            bool tutorialSetting = SettingsManager.Instance.GetSetting("stickies", true);

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
            isInitialized = true;
            return true;
        }

        private void Update()
        {
            if (sceneName == "Game")
            {
                return;
            }
            UpdateMenuState();

        }

        private void UpdateMenuState()
        {
            if(previousMenuState != currentMenuState)
            {
                previousMenuState = currentMenuState;
            }
            switch(currentMenuState)
            {
                case MenuState.zooming_in:
                    cameraUpdate();
                    break;
                case MenuState.fading_out:
                    timeFadingOut += Time.deltaTime;
                    splashScreenFadeImage.color = new Color(splashScreenFadeImage.color.r, splashScreenFadeImage.color.g, splashScreenFadeImage.color.b, (timeFadingOut / timeToFadeOut));

                    if (timeFadingOut > timeToFadeOut)
                    {
                        currentMenuState = MenuState.fading_in;
                        timeFadingIn += Time.deltaTime;
                        timeFadingOut = 0.0f;
                    }
                    break;
                case MenuState.fading_in:
                    timeFadingIn += Time.deltaTime;
                    float progressRatio = timeFadingIn / timeToFadeIn;
                    computerSticky1Text.color = new Color(computerSticky1Text.color.r, computerSticky1Text.color.g, computerSticky1Text.color.b, progressRatio);
                    computerSticky2Text.color = new Color(computerSticky2Text.color.r, computerSticky2Text.color.g, computerSticky2Text.color.b, progressRatio);
                    lobbyScreenImage.color = new Color(lobbyScreenImage.color.r, lobbyScreenImage.color.g, lobbyScreenImage.color.b, progressRatio);
                    if (timeFadingIn > timeToFadeIn)
                    {
                        currentMenuState = MenuState.pressing_pc_button;
                        AudioManager.Instance.PlaySound("sfx_lobby_int_computer_reach");
                        AudioManager.Instance.FadeOutSound("SplashMusic", 2.0f);
                        AudioManager.Instance.FadeOutSound("sfx_lobby_amb_outdoor", 2.0f);
                        SplashPageObject.SetActive(false);
                        //Randomize the bird arm
                        int birdIndex = UnityEngine.Random.Range(0, ColourManager.Instance.allBirds.Count);
                        splashArmAnimatorMap[ColourManager.Instance.allBirds[birdIndex].name].SetTrigger("move");
                        timeTurningOn += Time.deltaTime;
                        timeFadingIn = 0.0f;
                    }
                    break;

                case MenuState.pressing_pc_button:
                    if (timeTurningOn > buttonPressTimeThreshold && !buttonHasBeenPressed)
                    {
                        buttonHasBeenPressed = true;
                        AudioManager.Instance.PlaySound("sfx_lobby_int_computer_press");
                    }
                    timeTurningOn += Time.deltaTime;
                    if (timeTurningOn > timeToTurnOn)
                    {
                        timeTurningOn = 0.0f;
                        currentMenuState = MenuState.booting_pc;
                        AudioManager.Instance.PlaySound("sfx_lobby_int_computer_boot");
                        lobbyBGObject.SetActive(true);
                        computerSticky1Text.color = new Color(computerSticky1Text.color.r, computerSticky1Text.color.g, computerSticky1Text.color.b, 1f);
                        computerSticky2Text.color = new Color(computerSticky2Text.color.r, computerSticky2Text.color.g, computerSticky2Text.color.b, 1f);
                        lobbyScreenImage.color = new Color(lobbyScreenImage.color.r, lobbyScreenImage.color.g, lobbyScreenImage.color.b, 1f);
                        splashScreenFadeImage.gameObject.SetActive(true);
                        SplashPageObject.SetActive(false);
                        timeBootingUp += Time.deltaTime;
                        logoObject.SetActive(true);
                    }
                    break;
                case MenuState.booting_pc:
                    timeBootingUp += Time.deltaTime;
                    if (timeBootingUp > timeToBootUp)
                    {
                        currentMenuState = MenuState.rooms;
                        AudioManager.Instance.PlaySound("LobbyMusic");
                        skipIntroButton.SetActive(false);
                        timeBootingUp = 0.0f;
                        roomListingsObject.SetActive(true);
                        RoomListingsPlayerNameText.text = "User: \n" + SettingsManager.Instance.playerName;
                    }
                    break;
                case MenuState.rooms:
                    timeSinceLastRoomListingUpdate += Time.deltaTime;
                    if(timeSinceLastRoomListingUpdate > roomUpdateThreshold)
                    {
                        timeSinceLastRoomListingUpdate = 0f;
                        LobbyNetwork.Instance.RequestSteamRoomListings();
                    }
                    break;
            }
        }

        #region SplashPage
        private float cameraStateTime = 0f;

        private void cameraUpdate()
        {

            cameraStateTime += Time.deltaTime;

            //Transition
            float transitionRatio = cameraStateTime / splashTransitionCameraDock.transitionDuration;
            splashRectTransform.anchoredPosition = Vector3.Lerp(baseTransitionCameraDock.position, splashTransitionCameraDock.position, transitionRatio);
            splashRectTransform.localScale = Vector3.Lerp(baseTransitionCameraDock.zoom, splashTransitionCameraDock.zoom, transitionRatio);

            if (transitionRatio > 1)
            {
                currentMenuState = MenuState.fading_out;
                cameraStateTime = 0.0f;
                lobbyBGObject.SetActive(true);

                timeFadingOut += Time.deltaTime;
                splashScreenFadeImage.gameObject.SetActive(true);
            }


        }

        public void SplashPage_MultiplayerOnClick()
        {
            splashScreenPauseMenu.canBeOpened = false;
            if (hasSkippedIntro)
            {
                SkipIntro();
            }
            else
            {
                currentMenuState = MenuState.zooming_in;
                AudioManager.Instance.PlaySound("sfx_lobby_int_enter_sign");
                openDoorObject.SetActive(true);
                skipIntroButton.SetActive(true);
                LobbyNetwork.Instance.RequestSteamRoomListings();
            }


            //EventSystem.current.SetSelectedGameObject(CreateGameBtn);
        }

        public void OpenDiscordLink()
        {
            Application.OpenURL("https://discord.gg/57mrnW6QPW");
            AudioManager.Instance.PlaySound("sfx_ui_int_discord_sel");
        }

        public void SkipIntro_OnClick()
        {
            currentMenuState = MenuState.rooms;
            AudioManager.Instance.PlaySound("sfx_lobby_int_skip_sticky");
            SkipIntro();
        }
        public void SkipIntro()
        {
            AudioManager.Instance.StopSound("SplashMusic");
            AudioManager.Instance.StopSound("sfx_lobby_amb_outdoor");
            AudioManager.Instance.PlaySound("LobbyMusic");
            computerSticky1Text.color = new Color(computerSticky1Text.color.r, computerSticky1Text.color.g, computerSticky1Text.color.b, 1f);
            computerSticky2Text.color = new Color(computerSticky2Text.color.r, computerSticky2Text.color.g, computerSticky2Text.color.b, 1f);
            lobbyScreenImage.color = new Color(lobbyScreenImage.color.r, lobbyScreenImage.color.g, lobbyScreenImage.color.b, 1f);
            lobbyBGObject.SetActive(true);
            splashScreenFadeImage.gameObject.SetActive(true);
            roomListingsObject.SetActive(true);
            RoomListingsPlayerNameText.text = "User: \n" + SettingsManager.Instance.playerName;
            SplashPageObject.SetActive(false);
            skipIntroButton.SetActive(false);
            cameraStateTime = 0.0f;
            timeBootingUp = 0.0f;
            timeTurningOn = 0.0f;
            timeFadingIn = 0.0f;
            timeFadingOut = 0.0f;
            hasSkippedIntro = true;
        }

        public void SplashPage_QuitClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_ok");
            //Exit the game
            Application.Quit();
        }
        #endregion

        #region Network Responses

        public void JoinedRoom()
        {
            LobbyStartGameBtnTextObject.SetActive(SettingsManager.Instance.isHost);

            currentMenuState = MenuState.lobby;
            roomCodeText.text = "*****";
            splashScreenPauseMenu.canBeOpened = false;
            LobbyPageObject.SetActive(true);
            RoomsPageObject.SetActive(false);
            selectionInstructionsObject.SetActive(true);
            selectedBirdParentObject.SetActive(false);
            wordGroupsController.Initialize();
            if (SettingsManager.Instance.isHost)
            {

                gameModeButton.interactable = true;
                tutorialsButton.Initialize(SettingsManager.Instance.GetSetting("tutorials", true));
                tutorialsButton.containerObject.SetActive(true);
                exportTasksContainerObject.SetActive(true);
            }
            else
            {
                gameModeButton.interactable = false;
                tutorialsButton.containerObject.SetActive(false);
                exportTasksContainerObject.SetActive(false);
            }
            stickiesButton.Initialize(SettingsManager.Instance.GetSetting("stickies", true));
        }



        public void LeftLobby()
        {
            currentMenuState = MenuState.rooms;
            splashScreenPauseMenu.canBeOpened = true;
            PlayerListingMap.Clear();
            SettingsManager.Instance.ClearPlayerNameMap();
            SplashPageObject.SetActive(true);
        }

        public void LeftRoom()
        {
            currentMenuState = MenuState.rooms;
            //Clear room contents
            PlayerListingMap.Clear();
            SettingsManager.Instance.ClearPlayerNameMap();

            foreach (KeyValuePair<ColourManager.BirdName, PlayerIdentification> playerID in PlayerIdentificationMap)
            {
                //playerID.Value.Deselect();
            }


            if (RoomsPageObject)
            {
                RoomsPageObject.SetActive(true);
            }

        }

        private List<CSteamID> previousRoomIDs = new List<CSteamID>();
        private Dictionary<CSteamID, RoomListing> roomListingMap = new Dictionary<CSteamID, RoomListing>();
        public void UpdateRoomListings(List<CSteamID> currentRoomIDs)
        {
            GameObject newRoomListing;
            RoomListing rListingBehaviour;
            for (int i = previousRoomIDs.Count - 1; i >= 0; i--)
            {
                if (!currentRoomIDs.Contains(previousRoomIDs[i]))
                {
                    if (roomListingMap.ContainsKey(previousRoomIDs[i]))
                    {
                        
                        RoomListing roomListingsToRemove = roomListingMap[previousRoomIDs[i]];
                        if(roomListingsToRemove.gameObject != null)
                        {
                            Destroy(roomListingsToRemove.gameObject);
                        }
                        
                        roomListingMap.Remove(previousRoomIDs[i]);
                    }
                    previousRoomIDs.RemoveAt(i);
                }

            }
            foreach (CSteamID roomID in currentRoomIDs)
            {

                if (!previousRoomIDs.Contains(roomID))
                {
                    bool isPrivate = bool.Parse(SteamMatchmaking.GetLobbyData(roomID, "isPrivate"));
                    if (isPrivate)
                    {
                        continue;
                    }
                    newRoomListing = Instantiate(RoomListingPrefab, RoomListingsParent);
                    rListingBehaviour = newRoomListing.GetComponent<RoomListing>();
                    rListingBehaviour.roomID = roomID;
                    if (rListingBehaviour)
                    {
                        string roomName = Steamworks.SteamMatchmaking.GetLobbyData(roomID, "roomName");
                        roomListingMap.Add(roomID, rListingBehaviour);
                        previousRoomIDs.Add(roomID);
                        rListingBehaviour.RoomNameText.text = roomName.Length < 25 ? roomName : roomName.Substring(0, 25) + "..";
                        rListingBehaviour.AttendeeCountText.text = SteamMatchmaking.GetNumLobbyMembers(roomID).ToString() + "/" + Steamworks.SteamMatchmaking.GetLobbyMemberLimit(roomID).ToString();
                        rListingBehaviour.roomName = roomName;
                        activeRoomListings.Add(rListingBehaviour);
                    }
                }
                else
                {
                    if (roomListingMap.ContainsKey(roomID))
                    {
                        rListingBehaviour = roomListingMap[roomID];
                        rListingBehaviour.AttendeeCountText.text = SteamMatchmaking.GetNumLobbyMembers(roomID).ToString() + "/" + SteamMatchmaking.GetLobbyMemberLimit(roomID).ToString();
                    }
                    else
                    {
                        Debug.LogError("ERROR: Room exists but room listing object has been destroyed.");
                        previousRoomIDs.Remove(roomID);
                    }

                }
            }
        }

        #endregion

        #region LoginPage
        public void LoginPage_OKClick()
        {
            if (loginInputField.text == "")
            {
                return;
            }
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_ok");
            
            SettingsManager.Instance.playerID = loginInputField.text;
            roomListingsObject.SetActive(true);
            RoomListingsPlayerNameText.text = "User: \n" + SettingsManager.Instance.playerName;
            SettingsManager.Instance.UpdateSetting("user_name", SettingsManager.Instance.playerName);
            LobbyNetwork.Instance.RequestSteamRoomListings();
        }

        public void LoginPage_CancelClick()
        {

            AudioManager.Instance.StopSound("LobbyMusic");
            AudioManager.Instance.PlaySound("SplashMusic");
            AudioManager.Instance.PlaySound("sfx_lobby_amb_outdoor");
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_cancel_back");
            splashScreenFadeImage.color = new Color(splashScreenFadeImage.color.r, splashScreenFadeImage.color.g, splashScreenFadeImage.color.b, 0.0f);
            computerSticky1Text.color = new Color(computerSticky1Text.color.r, computerSticky1Text.color.g, computerSticky1Text.color.b, 0.0f);
            computerSticky2Text.color = new Color(computerSticky2Text.color.r, computerSticky2Text.color.g, computerSticky2Text.color.b, 0.0f);
            lobbyScreenImage.color = new Color(lobbyScreenImage.color.r, lobbyScreenImage.color.g, lobbyScreenImage.color.b, 0.0f);
            splashRectTransform.anchoredPosition = baseTransitionCameraDock.position;
            splashRectTransform.localScale = baseTransitionCameraDock.zoom;
            splashScreenFadeImage.gameObject.SetActive(false);
            buttonHasBeenPressed = false;
            openDoorObject.SetActive(false);
            loginObject.SetActive(false);
            logoObject.SetActive(false);
            lobbyBGObject.SetActive(false);
            SplashPageObject.SetActive(true);
            splashScreenPauseMenu.canBeOpened = true;
        }
        #endregion

        #region RoomListingsPage
        public void RoomListingsPage_JoinRoomClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_gen");
            joinGameObject.SetActive(true);
            joinGameInputField.Select();
        }

        public void RoomListingsPage_BookRoomClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_gen");
            createGameObject.SetActive(true);
            createRoomInputField.Select();
        }
        #endregion

        #region CreateGamePrompt
        public void CreateGamePrompt_OKClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_ok");
            if (createRoomInputField.text != "")
            {
                LobbyNetwork.Instance.CreateRoom(createRoomInputField.text, privacyToggle.isOn);
                roomCodeText.text = "*****";
                roomCodeText.gameObject.SetActive(true);
            }
        }

        public void CreateGamePrompt_CancelClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_cancel_back");
            createGameObject.SetActive(false);
        }

        #endregion

        #region JoinGamePrompt
        public void JoinGamePrompt_OKClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_ok");
            if (joinGameInputField.text != "")
            {
                LobbyNetwork.Instance.JoinRoomByCode(joinGameInputField.text);
            }
            else
            {
                Debug.LogError("Please enter a room code.");
            }
        }

        public void JoinGamePrompt_CancelClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_cancel_back");
            joinGameObject.SetActive(false);
        }
        #endregion

        #region RoomsPage
        public void RoomsPage_CreateGameOnClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_gen");

            if (SetPlayerNameText.text == "")
            {
                PlayerPrompt.Activate("Please choose a player name.");
                return;
            }
            else if (CreateRoomNameText.text == "")
            {
                PlayerPrompt.Activate("Please choose a room name.");
                return;
            }
            else
            {
                SettingsManager.Instance.playerID = GameDelim.stripGameDelims(SetPlayerNameText.text);
            }
        }

        public void RoomsPage_JoinGameOnClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_gen");

            if (SetPlayerNameText.text == "")
            {
                PlayerPrompt.Activate("Please choose a player name.");
                return;
            }
            else
            {
                SettingsManager.Instance.playerID = GameDelim.stripGameDelims(SetPlayerNameText.text);
            }

            if (selectedRoomListing == null)
            {

                return;
            }

            LobbyNetwork.Instance.JoinRoom(selectedRoomListing.roomID);
        }

        public void RoomsPage_BackClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_cancel_back");

            SplashPageObject.SetActive(true);
            loginObject.SetActive(false);
            lobbyBGObject.SetActive(false);
            logoObject.SetActive(false);
            roomListingsObject.SetActive(false);
            
            splashRectTransform.anchoredPosition = baseTransitionCameraDock.position;
            splashRectTransform.localScale = baseTransitionCameraDock.zoom;
            splashScreenFadeImage.gameObject.SetActive(false);
        }

        public void SelectRoomListing(RoomListing roomListing)
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_gen");

            selectedRoomListing = roomListing;

            if (selectedRoomListing == null)
            {
                return;
            }

            LobbyNetwork.Instance.JoinRoom(selectedRoomListing.roomID);
        }
        #endregion

        #region LobbyPage
        public void LobbyPage_StartGameClick()
        {
            AudioManager.Instance.PlaySound("sfx_scan_int_start");

            string loadingLevel = "Game";
            int playerNameCount = SettingsManager.Instance.GetPlayerNameCount();

            if (playerNameCount >= SettingsManager.Instance.gameMode.minimumNumberOfPlayers)
            {
                Steamworks.SteamMatchmaking.SetLobbyJoinable(SettingsManager.Instance.currentRoomID, false);
                NetworkManager.singleton.ServerChangeScene(loadingLevel);
            }
        }

        public void LobbyPage_DisconnectClick()
        {
            if (SelectedID)
            {

            }
            CSNetworkManager.intentionalDisconnection = true;
            AudioManager.Instance.PlaySound("sfx_scan_int_leave");
            SelectedID = null;
            LobbyPageObject.SetActive(false);
            LoadingPageObject.SetActive(false);
            LobbyNetwork.Instance.LeaveRoom();

        }

        public bool IsBirdSelected(ColourManager.BirdName birdName)
        {
            if (PlayerIdentificationMap[birdName].playerName != "")
            {
                return false;
            }
            return SettingsManager.Instance.IsBirdSelected(birdName);
        }

        public BirdName GetPreviouslySelectedBird(string playerName)
        {
            foreach (PlayerIdentification playerIdentification in PlayerIdentifiers)
            {
                if (playerIdentification.playerName == playerName)
                {
                    return playerIdentification.birdName;
                }
            }
            return BirdName.none;
        }

        public void SelectPlayerBird(ColourManager.BirdName birdName, string playerName)
        {
            SettingsManager.Instance.AssignBirdToPlayer(birdName, playerName);
            lobbyNotReadyManager.playerAllHaveCardsSelected = SettingsManager.Instance.GetPlayerNameCount() == NetworkServer.connections.Count();
            //If this is the current player's bird
            if (playerName == SettingsManager.Instance.playerName)
            {
                BirdData selectedBird = GameDataManager.Instance.GetBird(birdName);
                if(selectedBird == null)
                {
                    Debug.LogError("Could not set face sprite for selected bird because player bird["+birdName.ToString()+"] is not mapped in the Colour Manager.");
                }
                else
                {
                    selectedBirdImage.sprite = selectedBird.faceSprite;
                }
                
                selectedBirdParentObject.SetActive(true);
                selectionInstructionsObject.SetActive(false);
                SettingsManager.Instance.birdName = birdName;
            }

            PlayerIdentificationMap[birdName].Select(playerName);

            //Update the player listing
            if(PlayerListingMap.ContainsKey(playerName))
            {
                PlayerListingMap[playerName].ChangePlayerBird(birdName);
            }
            else
            {
                Debug.LogError("Player listing map is missing player["+playerName+"], outputting listings to review.");
                foreach(KeyValuePair<string,PlayerListing> playerListing in PlayerListingMap)
                {
                    Debug.LogError("Player listing: " + playerListing.Key);
                }
            }
        }

        public void DeselectPlayerBird(ColourManager.BirdName birdName)
        {
            SettingsManager.Instance.DeassignBirdToPlayer(birdName);
            PlayerIdentificationMap[birdName].Deselect();
        }

        public void ChangeGameMode()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_scan_int_gamemode_select");
            string newGameMode = SettingsManager.Instance.ChangeGameMode();
            gameModeButtonText.text = newGameMode.ToUpper();
            gameModeInformationHeaderText.text = "Game Mode: " + SettingsManager.Instance.gameMode.title;
            gameModeDescriptionText.text = SettingsManager.Instance.gameMode.description;
            //LobbyNetwork.Instance.BroadcastQueue.Add("UpdateGameMode" + GameDelim.BASE + newGameMode);
        }
        #endregion


        public void UpdatePlayerCount()
        {
            Debug.Log("Number of players required[" + SettingsManager.Instance.gameMode.minimumNumberOfPlayers.ToString() + "], current connections[" + NetworkServer.connections.Count.ToString() + "]");
            lobbyNotReadyManager.gameModeHasEnoughPlayers = SettingsManager.Instance.gameMode.minimumNumberOfPlayers <= NetworkServer.connections.Count;

        }

        #region GameSettings

        public void UpdateCorrectCabinetThreshold(int inCorrectCabinetThreshold)
        {
            correctCabinetThresholdSettingText.text = "Correct Cabinet Threshold:\n" + ((int)correctCabinetThresholdSettingSlider.value).ToString();
            SettingsManager.Instance.correctCabinetThreshold = (int)correctCabinetThresholdSettingSlider.value;
        }

        public void UpdateCorrectCabinetThresholdMaximum(int newMax)
        {
            correctCabinetThresholdSettingSlider.maxValue = newMax;
            correctCabinetThresholdSettingSlider.value = newMax > 1 ? newMax - 1 : newMax;
        }

        #endregion

        public ColourManager.BirdName GetBirdNameFromText(string birdText)
        {
            switch (birdText)
            {
                case "red":
                    return ColourManager.BirdName.red;
                case "blue":
                    return ColourManager.BirdName.blue;
                case "green":
                    return ColourManager.BirdName.green;
                case "purple":
                    return ColourManager.BirdName.purple;
                case "maroon":
                    return ColourManager.BirdName.maroon;
                case "grey":
                    return ColourManager.BirdName.grey;
                case "orange":
                    return ColourManager.BirdName.orange;
                case "black":
                    return ColourManager.BirdName.black;
                case "brown":
                    return ColourManager.BirdName.brown;
                case "yellow":
                    return ColourManager.BirdName.yellow;
                case "teal":
                    return ColourManager.BirdName.teal;
                case "pink":
                    return ColourManager.BirdName.pink;
            }

            return ColourManager.BirdName.none;
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void OpenQuitGamePrompt()
        {
            AudioManager.Instance.PlaySound("sfx_lobby_int_exit_sign");
            splashScreenPauseMenu.canBeOpened = false;
            quitCheckPromptObject.SetActive(true);
        }

        public void CloseQuitGamePrompt()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_cancel_back");
            splashScreenPauseMenu.canBeOpened = true;
            quitCheckPromptObject.SetActive(false);
        }

        public void CopyRoomCodeClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_scan_int_click_gen");
            textCopiedNotificationObject.SetActive(true);
            GUIUtility.systemCopyBuffer = SettingsManager.Instance.roomCode;
        }

        public void PasteRoomCodeClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_scan_int_click_gen");
            findRoomCodeText.text = GUIUtility.systemCopyBuffer;
        }
        public void RevealRoomCodeClick()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_scan_int_click_gen");
            if (roomCodeText.text == "*****")
            {
                roomCodeText.text = SettingsManager.Instance.roomCode;
                roomCodeEyeOpenObject.SetActive(true);
                roomCodeEyeClosedObject.SetActive(false);
            }
            else
            {
                roomCodeText.text = "*****";
                roomCodeEyeOpenObject.SetActive(false);
                roomCodeEyeClosedObject.SetActive(true);
            }
        }

        public void UpdateQuickplayToggle()
        {
            //LobbyNetwork.Instance.BroadcastQueue.Add("update_quickplay" + GameDelim.BASE + quickplayToggle.isOn.ToString());
            SettingsManager.Instance.saveAdminReviewData = exportTasksToggle.isOn;
        }

        public void LoadLobbyFromGame()
        {
            roomCodeText.text = "*****";
            AudioManager.Instance.StopSound("SplashMusic");
            AudioManager.Instance.StopSound("sfx_lobby_amb_outdoor");
            AudioManager.Instance.PlaySound("LobbyMusic");
            LobbyPageObject.SetActive(true);
            RoomsPageObject.SetActive(true);
            LoadingPageObject.SetActive(true);
            LobbyStartGameBtn.interactable = SettingsManager.Instance.isHost;
            SettingsManager.Instance.SetGameMode(SettingsManager.Instance.gameMode.title);

            LobbyStartGameBtnTextObject.SetActive(SettingsManager.Instance.isHost);
            wordGroupsController.Initialize();
            if (SettingsManager.Instance.isHost)
            {
                UpdatePlayerCount();
                gameModeButton.interactable = true;
                tutorialsButton.Initialize(SettingsManager.Instance.GetSetting("tutorials", true));
                tutorialsButton.containerObject.SetActive(true);
                exportTasksContainerObject.SetActive(true);
                lobbyNotReadyManager.playerAllHaveCardsSelected = SettingsManager.Instance.GetPlayerNameCount() == NetworkServer.connections.Count();
            }
            else
            {
                
                gameModeButton.interactable = false;
                tutorialsButton.containerObject.SetActive(false);
                exportTasksContainerObject.SetActive(false);
            }
        }

        public void OpenRoomsPage()
        {
            currentMenuState = MenuLobbyButtons.MenuState.rooms;
            LobbyPageObject.SetActive(false);
            RoomsPageObject.SetActive(true);
        }
    }
}