using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ChickenScratch
{
    public class ResultsCaseButton : MonoBehaviour
    {
        [SerializeField]
        private Image folderButtonImage;

        [SerializeField]
        private TMPro.TMP_Text caseNameText;

        private int identifier = -1;
        private DailyEmailContents email;

        public void Initialize(int inIdentifier, string caseName, string caseType, DailyEmailContents inEmail)
        {
            identifier = inIdentifier;
            caseNameText.text = caseName;
            CaseChoiceData caseChoice = GameDataManager.Instance.GetCaseChoice(caseType);
            if(caseChoice != null)
            {
                folderButtonImage.color = caseChoice.colour;
            }
            email = inEmail;
        }

        public void OnPress()
        {
            email.PopulateCase(identifier);
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
        }
    }
}

