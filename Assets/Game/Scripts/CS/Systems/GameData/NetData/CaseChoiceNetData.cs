﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class CaseChoiceNetData
    {
        public List<List<int>> promptWordIndexMap = new List<List<int>>();
        public List<List<string>> possibleWordsMap = new List<List<string>>();
        public List<string> correctWordIdentifiersMap = new List<string>();
        public string correctPrompt;
        public string caseChoiceIdentifier;
        public float scoreModifierDecrement;
        public float modifierIncreaseValue;
        public float maxScoreModifier;
        public int bonusBirdbucks;
        public int birdbucksPerCorrectWord;

        public void SetWords(Dictionary<int,List<string>> inPossibleWordsMap, Dictionary<int,string> inCorrectWordsMap, Dictionary<int,List<int>> inCorrectPromptMap, CaseChoiceData.PromptFormat promptFormat)
        {
            List<List<string>> possibleWords = new List<List<string>>();
            List<string> correctWords = new List<string>();
            //Order the keys
            List<int> orderedKeys = inPossibleWordsMap.Keys.ToList();
            orderedKeys.Sort();
            foreach (int key in orderedKeys)
            {
                possibleWords.Add(inPossibleWordsMap[key]);
            }
            //Order the keys
            orderedKeys = inCorrectWordsMap.Keys.ToList();
            orderedKeys.Sort();
            foreach (int key in orderedKeys)
            {
                correctWords.Add(inCorrectWordsMap[key]);
            }

            orderedKeys = inCorrectPromptMap.Keys.ToList();
            orderedKeys.Sort();
            foreach(int key in orderedKeys)
            {
                List<int> currentPromptIndices = new List<int>();
                foreach(int wordIndex in inCorrectPromptMap[key])
                {
                    currentPromptIndices.Add(wordIndex);
                }
                promptWordIndexMap.Add(currentPromptIndices);
            }
            if(correctWords.Count != 2)
            {
                Debug.LogError("CorrectWords did not have the correct number of words for case type["+caseChoiceIdentifier +"]");

            }
            else
            {
                CaseWordData prefix = GameDataManager.Instance.GetWord(correctWords[0]);
                CaseWordData noun = GameDataManager.Instance.GetWord(correctWords[1]);
                if(prefix != null && noun != null)
                {
                    switch(promptFormat)
                    {
                        case CaseChoiceData.PromptFormat.standard:
                        case CaseChoiceData.PromptFormat.variant:
                            correctPrompt = prefix.value + " " + noun.value;
                            break;
                        case CaseChoiceData.PromptFormat.location:
                            correctPrompt = prefix.value + " in the " + noun.value;
                            break;
                    }
                    
                }
                
            }
            
            possibleWordsMap = possibleWords;
            correctWordIdentifiersMap = correctWords;
        }
    }
}
