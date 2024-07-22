using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class DailyEmailContents : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text dayText;

        [SerializeField]
        private GameObject caseButtonPrefab;

        [SerializeField]
        private Transform caseButtonHolder;

        [SerializeField]
        private CaseEmail caseEmail;

        public void Initialize(string dayName)
        {
            dayText.text = dayName.ToUpper() + " REPORT";
            bool hasOpenedFirstCase = false;
            foreach (EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                if(caseData.dayName == dayName)
                {
                    //Create a corresponding button for the case
                    GameObject caseButtonObject = Instantiate(caseButtonPrefab, caseButtonHolder);
                    ResultsCaseButton resultsCaseButton = caseButtonObject.GetComponent<ResultsCaseButton>();
                    resultsCaseButton.Initialize(caseData.identifier, caseData.correctPrompt, caseData.caseTypeName, this);

                    if(!hasOpenedFirstCase)
                    {
                        hasOpenedFirstCase = true;
                        PopulateCase(caseData.identifier);
                    }
                }
            }
        }

        public void PopulateCase(int identifier)
        {
            caseEmail.initialize(GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[identifier]);
        }
    }
}

