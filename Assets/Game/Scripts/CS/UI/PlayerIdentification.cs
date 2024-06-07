
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PlayerIdentification : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public ColourManager.BirdName birdName;
        public Image birdImage;
        public string playerID;

        [SerializeField]
        private GameObject selectedCardPrefab;
        [SerializeField]
        private Transform selectedCardsHolderTransform;
        [SerializeField]
        private GameObject visualsHolderObject;
        [SerializeField]
        private LobbyBirdArm lobbyBirdArm;

        private SelectedPlayerIdentification selectedPlayerCard = null;
        private bool isSelected = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("OnPointerDown playerName[" + playerID.ToString() + "] isSelected[" + isSelected.ToString() + "]");
            // this object was clicked - do something
            if (playerID == "" && !isSelected)
            {
                Debug.LogError("SettingsManager - PlayerName[" + SettingsManager.Instance.playerName + "]");
                LobbyNetwork.Instance.lobbyDataHandler.CmdSelectPlayerBird(SettingsManager.Instance.playerName, birdName);
            }
        }

        public void Select(string inPlayerID)
        {
            if (playerID != "") return;
            isSelected = true;
            playerID = inPlayerID;
            if (lobbyBirdArm.currentState == LobbyBirdArm.State.rest ||
                lobbyBirdArm.currentState == LobbyBirdArm.State.slide_left ||
                lobbyBirdArm.currentState == LobbyBirdArm.State.slide_right)
            {
                lobbyBirdArm.currentState = LobbyBirdArm.State.approach_card;
            }

        }

        public void Deselect()
        {
            if (lobbyBirdArm.currentState == LobbyBirdArm.State.approach_card ||
                lobbyBirdArm.currentState == LobbyBirdArm.State.grab_card ||
                lobbyBirdArm.currentState == LobbyBirdArm.State.holding)
            {
                lobbyBirdArm.returnRequested = true;
            }


            isSelected = false;
            if (selectedPlayerCard != null)
            {
                Destroy(selectedPlayerCard.gameObject);
            }
            playerID = "";
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (lobbyBirdArm.currentState == LobbyBirdArm.State.rest ||
                lobbyBirdArm.currentState == LobbyBirdArm.State.slide_left)
            {
                AudioManager.Instance.PlaySoundVariant("sfx_scan_int_card_hover");
                lobbyBirdArm.currentState = LobbyBirdArm.State.slide_right;
            }

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (lobbyBirdArm.currentState == LobbyBirdArm.State.hover ||
                lobbyBirdArm.currentState == LobbyBirdArm.State.slide_right)
            {
                AudioManager.Instance.PlaySoundVariant("sfx_scan_int_card_hover");
                lobbyBirdArm.currentState = LobbyBirdArm.State.slide_left;
            }

        }
    }
}