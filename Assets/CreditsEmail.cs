using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class CreditsEmail : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text memberNameText;

        [SerializeField]
        private TMPro.TMP_Text memberDescriptionText;

        [SerializeField]
        private Image memberImage;

        public void OnMemberPress(string memberName)
        {
            TeamMemberData teamMemberData = GameDataManager.Instance.GetTeamMember(memberName);
            if(teamMemberData != null)
            {
                memberNameText.text = memberName;
                memberDescriptionText.text = teamMemberData.description;
                memberImage.sprite = teamMemberData.sprite;
            }
        }
    }

}
