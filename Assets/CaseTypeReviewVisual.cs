using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class CaseTypeReviewVisual : MonoBehaviour
    {
        [SerializeField]
        private Image bgImage;

        [SerializeField]
        private TMPro.TMP_Text caseTitleText;

        [SerializeField]
        private TMPro.TMP_Text baseBirdbucksText;

        [SerializeField]
        private TMPro.TMP_Text modifierText;

        [SerializeField]
        private TMPro.TMP_Text frequencyText;

        [SerializeField]
        private CertificationSlot certificationSlot1, certificationSlot2;

        [SerializeField]
        private GameObject previousAverageHolder;

        [SerializeField]
        private TMPro.TMP_Text previousAverageText;

        [SerializeField]
        private TMPro.TMP_Text caseTypeTooltipText;

        int certificationsCount = 0;
        public void Initialize(CaseChoiceData possibleCaseChoice)
        {
            bgImage.color = possibleCaseChoice.colour;
            caseTitleText.text = possibleCaseChoice.identifier;
            caseTypeTooltipText.text = possibleCaseChoice.description;
            List<string> certifications = GameManager.Instance.playerFlowManager.GetCaseCertifications(possibleCaseChoice.identifier);

            if(certifications.Count > 0)
            {
                AddCertification(certifications[0]);
                if(certifications.Count > 1)
                {
                    AddCertification(certifications[1]);
                }
            }
            UpdateVisual(possibleCaseChoice);
        }

        public void AddCertification(string certificationType)
        {
            if (certificationsCount == 0)
            {
                certificationSlot1.Initialize(certificationType);
                certificationsCount++;
            }
            else if(certificationsCount == 1)
            {
                certificationSlot2.Initialize(certificationType);
                certificationsCount++;
            }
        }

        public void UpdateCaseStats(CaseChoiceData caseChoice)
        {
            baseBirdbucksText.text = caseChoice.GetEarningsDescription();
            modifierText.text = caseChoice.maxScoreModifier.ToString();
            frequencyText.text = caseChoice.selectionFrequency.ToString() + "x";
        }

        public void UpdateVisual(CaseChoiceData caseChoice)
        {
            int totalEarnings = 0;
            int totalCases = 0;
            foreach (EndgameCaseData previousCase in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                if (previousCase.caseTypeName == caseChoice.identifier)
                {
                    totalEarnings += previousCase.scoringData.GetTotalPoints();
                    totalCases++;
                }
            }
            if (totalCases == 0)
            {
                previousAverageHolder.gameObject.SetActive(false);
            }
            else
            {
                previousAverageText.text = (((float)totalEarnings) / ((float)totalCases)).ToString("0.##");
            }
            UpdateCaseStats(caseChoice);
        }
    }

}
