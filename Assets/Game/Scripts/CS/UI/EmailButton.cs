using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class EmailButton : MonoBehaviour
    {
        public GameObject window;
        public Text text;
        public Image unreadImage;
        public GameObject unreadObject;
        public Text honkText, honkText2, unreadText;


        public void Click()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
            PlayerFlowManager playerFlowManager = GameManager.Instance.playerFlowManager;

            if (playerFlowManager.resultsRound.currentOpenEmail)
            {
                playerFlowManager.resultsRound.currentOpenEmail.SetActive(false);

            }
            playerFlowManager.resultsRound.currentOpenEmail = window;
            window.SetActive(true);
            unreadObject.SetActive(false);

            if (playerFlowManager.resultsRound.lastSelectedButtonImage)
            {
                playerFlowManager.resultsRound.lastSelectedButtonImage.color = playerFlowManager.resultsRound.unselectedEmailButtonColour;
            }

            unreadImage.color = playerFlowManager.resultsRound.selectedEmailButtonColour;
            playerFlowManager.resultsRound.lastSelectedButtonImage = unreadImage;
        }
    }
}