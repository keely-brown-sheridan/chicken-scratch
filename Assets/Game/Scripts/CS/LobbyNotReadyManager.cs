using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class LobbyNotReadyManager : MonoBehaviour
    {
        public bool lobbyIsReady => _gameModeHasEnoughPlayers && _playersAllHaveCardsSelected && _enoughWordGroupsAreSelected;

        [SerializeField]
        private GameObject notReadyContainerObject;

        [SerializeField]
        private TMPro.TMP_Text notReadyWarningMessageText;

        public bool gameModeHasEnoughPlayers
        {
            get
            {
                return _gameModeHasEnoughPlayers;
            }
            set
            {

                _gameModeHasEnoughPlayers = value;
                UpdatedReadyCondition();
            }
        }
        private bool _gameModeHasEnoughPlayers = false;
        public bool playerAllHaveCardsSelected
        {
            get
            {
                return _playersAllHaveCardsSelected;
            }
            set
            {

                _playersAllHaveCardsSelected = value;
                UpdatedReadyCondition();
            }
        }
        private bool _playersAllHaveCardsSelected = false;
        public bool enoughWordGroupsAreSelected
        {
            get
            {
                return _enoughWordGroupsAreSelected;
            }
            set
            {

                _enoughWordGroupsAreSelected = value;
                UpdatedReadyCondition();
            }
        }
        private bool _enoughWordGroupsAreSelected = false;

        private void UpdatedReadyCondition()
        {
            string message = "";
            if (lobbyIsReady)
            {
                notReadyContainerObject.SetActive(false);

            }
            else
            {
                notReadyContainerObject.SetActive(true);
                if (_gameModeHasEnoughPlayers)
                {
                    if (!_playersAllHaveCardsSelected)
                    {
                        message = "Players need to selected card.";
                    }
                }
                else
                {
                    message = "Not enough players.";
                }

                if (!_enoughWordGroupsAreSelected)
                {
                    if (message != "")
                    {
                        message += "\n";
                    }
                    message += "Not enough word groups are selected.";
                }
                notReadyWarningMessageText.text = message;
            }
        }
    }
}