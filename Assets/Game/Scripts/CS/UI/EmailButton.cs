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
            playerFlowManager.resultsRound.OpenEmail(window, unreadImage);

            unreadObject.SetActive(false);
        }
    }
}