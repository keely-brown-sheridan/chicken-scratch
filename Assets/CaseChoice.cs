using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class CaseChoice : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text titleText;

        [SerializeField]
        private TMPro.TMP_Text descriptionText;

        [SerializeField]
        private TMPro.TMP_Text prefixText;

        [SerializeField]
        private TMPro.TMP_Text nounText;

        [SerializeField]
        private Image bgImage;

        [SerializeField]
        private TMPro.TMP_Text rewardText;

        [SerializeField]
        private TMPro.TMP_Text modifierText;

        [SerializeField]
        private GameObject caseTabObject;

        [SerializeField]
        private Image caseTabFillImage;

        private CaseChoiceData caseChoiceData;

        public void Initialize(CaseChoiceNetData inCaseChoiceData)
        {
            caseChoiceData = GameDataManager.Instance.GetCaseChoice(inCaseChoiceData.caseChoiceIdentifier);
            if(caseChoiceData == null)
            {
                Debug.LogError("Could not initialize case choice because identifier["+inCaseChoiceData.caseChoiceIdentifier.ToString()+"] did not exist in the Game Data Manager.");
                return;
            }
            titleText.text = caseChoiceData.identifier;
            descriptionText.text = caseChoiceData.description;
            int totalReward = 0;
            if (inCaseChoiceData.correctWordIdentifiersMap.Count == 2)
            {
                CaseWordData prefixWord = GameDataManager.Instance.GetWord(inCaseChoiceData.correctWordIdentifiersMap[0]);
                CaseWordData nounWord = GameDataManager.Instance.GetWord(inCaseChoiceData.correctWordIdentifiersMap[1]);
                prefixText.text = SettingsManager.Instance.CreatePromptText(prefixWord.value, nounWord.value);
                totalReward += prefixWord.difficulty;
                totalReward += nounWord.difficulty;
            }
            else
            {
                Debug.LogError("Invalid number of correct words in the identifiers map.");
            }
            
            nounText.text = "";
            bgImage.color = caseChoiceData.colour;
            totalReward += caseChoiceData.pointsPerCorrectWord * 2 + caseChoiceData.bonusPoints;

            rewardText.text = totalReward.ToString();
            modifierText.text = caseChoiceData.startingScoreModifier.ToString("F2");
        }

        public void SetCaseTab(float caseTabModifierValue)
        {
            caseTabObject.SetActive(true);
            BirdData playerBird = GameDataManager.Instance.GetBird(SettingsManager.Instance.birdName);
            if(playerBird == null)
            {
                Debug.LogError("ERROR[SetCaseTab]: Could not get player bird["+SettingsManager.Instance.birdName.ToString()+"] because it doesn't exist in the ColourManager.");
            }
            caseTabFillImage.color = playerBird.colour;
            modifierText.text = (caseChoiceData.startingScoreModifier + caseTabModifierValue).ToString("F2");
        }
    }
}

