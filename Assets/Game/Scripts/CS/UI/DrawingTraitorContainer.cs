using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class DrawingTraitorContainer : MonoBehaviour
    {
        public GameObject traitorOptionsContainer, correctOptionPromptContainer, correctOptionDrawingContainer;

        public Text correctOptionPromptText, correctOptionDrawingText;
        public List<PossiblePrompt> prefixOptions, nounOptions;

        public string selectedPrefix = "", selectedNoun = "";
        public bool readyToShowBotcherOptions = false;
        public bool readyToShowCorrectOption = false;

        public void loadNewBotcherOptions(int cabinetID)
        {
            if (!GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap.ContainsKey(cabinetID))
            {
                Debug.LogError("Cabinet[" + cabinetID.ToString() + "] has not been loaded, cannot load botcher options.");
                return;
            }

            ChainData chain = GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetID].currentChainData;
            //for(int i = 0; i < chain.botcherPrefixOptions.Count; i++)
            //{
            //    prefixOptions[i].displayText.text = chain.botcherPrefixOptions[i];
            //}
            //for (int i = 0; i < chain.botcherNounOptions.Count; i++)
            //{
            //    nounOptions[i].displayText.text = chain.botcherNounOptions[i];
            //}

            traitorOptionsContainer.SetActive(true);
        }

        public void loadCorrectOption(int cabinetID, int tab)
        {
            if (!GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap.ContainsKey(cabinetID))
            {
                Debug.LogError("Cabinet[" + cabinetID.ToString() + "] has not been loaded, cannot load botcher options.");
                return;
            }
            ChainData chain = GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetID].currentChainData;

            if (tab % 2 == 0)
            {
                //This is a prompting round
                correctOptionPromptText.text = chain.correctPrompt;
                correctOptionPromptContainer.SetActive(true);
            }
            else
            {
                //This is a drawing round
                correctOptionDrawingText.text = chain.correctPrompt;
                correctOptionDrawingContainer.SetActive(true);
            }
        }

        public string getSelectedPrefix()
        {
            return selectedPrefix != "" && prefixOptions.Where(p => p.identifier == selectedPrefix).Count() == 1 ? prefixOptions.Single(p => p.identifier == selectedPrefix).displayText.text : "";
        }

        public string getSelectedNoun()
        {
            return selectedNoun != "" && nounOptions.Where(n => n.identifier == selectedNoun).Count() == 1 ? nounOptions.Single(n => n.identifier == selectedNoun).displayText.text : "";
        }

        public void hideBotcherOptions()
        {
            traitorOptionsContainer.SetActive(false);
            correctOptionPromptContainer.SetActive(false);
            correctOptionDrawingContainer.SetActive(false);

            correctOptionDrawingText.text = "";
            correctOptionPromptText.text = "";
            foreach (PossiblePrompt prefix in prefixOptions)
            {
                prefix.displayText.text = "";
                prefix.backgroundImage.color = Color.white;
            }
            foreach (PossiblePrompt noun in nounOptions)
            {
                noun.displayText.text = "";
                noun.backgroundImage.color = Color.white;
            }
            selectedPrefix = "";
            selectedNoun = "";

        }

        public void selectPrefix(string id)
        {
            foreach (PossiblePrompt prefix in prefixOptions)
            {
                if (prefix.identifier == id)
                {
                    prefix.backgroundImage.color = Color.green;
                    selectedPrefix = id;
                }
                else
                {
                    prefix.backgroundImage.color = Color.white;
                }
            }

        }

        public void selectNoun(string id)
        {
            foreach (PossiblePrompt noun in nounOptions)
            {
                if (noun.identifier == id)
                {
                    noun.backgroundImage.color = Color.green;
                    selectedNoun = id;
                }
                else
                {
                    noun.backgroundImage.color = Color.white;
                }
            }
        }
    }
}