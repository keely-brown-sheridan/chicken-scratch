using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class StoreFrequencyChoiceOption : MonoBehaviour
    {
        [SerializeField]
        private Image folderImage;

        [SerializeField]
        private TMPro.TMP_Text caseTypeText;

        [SerializeField]
        private TMPro.TMP_Text frequencyImprovementText;

        private string caseChoiceType;

        public void Initialize(string inCaseChoiceType, int currentFrequency, int newFrequency)
        {
            CaseChoiceData caseChoiceData = GameDataManager.Instance.GetCaseChoice(inCaseChoiceType);
            if(caseChoiceData != null)
            {
                caseChoiceType = inCaseChoiceType;
                folderImage.color = caseChoiceData.colour;
                caseTypeText.text = caseChoiceType;
                frequencyImprovementText.text = currentFrequency.ToString() + "x -> " + newFrequency.ToString() + "x";
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void Choose()
        {
            GameManager.Instance.gameDataHandler.CmdIncreaseCaseFrequency(caseChoiceType);
            GameManager.Instance.playerFlowManager.storeRound.CloseStoreFrequencyPanel();
        }
    }
}

