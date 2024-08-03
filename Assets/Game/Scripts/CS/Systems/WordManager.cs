using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using static ChickenScratch.WordGroupData;

namespace ChickenScratch
{
    public class WordManager
    {
        private Dictionary<string, List<string>> categoriesMap = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> usedWordsMap = new Dictionary<string, List<string>>();
        private Dictionary<string, List<WordData>> queueMap = new Dictionary<string, List<WordData>>();
        public Dictionary<string, Dictionary<string, WordGroupData>> wordGroupMap => _wordGroupMap;
        private Dictionary<string, Dictionary<string, WordGroupData>> _wordGroupMap = new Dictionary<string, Dictionary<string, WordGroupData>>();

        public static string testingPrefixIdentifier = "prefixes-NEUTRAL-ATTACHED", testingNounIdentifier = "nouns-ANIMAL-AXOLOTL";


        public void LoadPromptWords()
        {
            _wordGroupMap.Clear();
            queueMap.Clear();
            categoriesMap.Clear();
            usedWordsMap.Clear();
            //Load in the potential prompts
            List<WordGroupData> nounWordGroups = GameDataManager.Instance.GetWordGroups(WordGroupData.WordType.nouns);
            List<WordGroupData> prefixWordGroups = GameDataManager.Instance.GetWordGroups(WordGroupData.WordType.prefixes);
            List<WordGroupData> variantWordGroups = GameDataManager.Instance.GetWordGroups(WordGroupData.WordType.variant);
            List<WordGroupData> locationWordGroups = GameDataManager.Instance.GetWordGroups(WordGroupData.WordType.location);

            InitializeWordType("prefixes", prefixWordGroups);
            InitializeWordType("nouns", nounWordGroups);
            InitializeWordType("variant", variantWordGroups);
            InitializeWordType("location", locationWordGroups);
        }

        private void InitializeWordType(string wordType, List<WordGroupData> wordGroups)
        {
            queueMap.Add(wordType, new List<WordData>());
            _wordGroupMap.Add(wordType, new Dictionary<string, WordGroupData>());
            categoriesMap.Add(wordType, new List<string>());
            usedWordsMap.Add(wordType, new List<string>());
            foreach (WordGroupData wordGroup in wordGroups)
            {
                wordGroup.isBaseWordGroup = true;
                wordGroup.isOn = true;
                queueMap[wordType].AddRange(wordGroup.words);
                _wordGroupMap[wordType].Add(wordGroup.name, wordGroup);
                if (SettingsManager.Instance.wordGroupNames.Contains(wordGroup.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
                {
                    categoriesMap[wordType].Add(wordGroup.name);
                }
            }
            queueMap[wordType] = queueMap[wordType].OrderBy(a => Guid.NewGuid()).ToList();
        }

        public void LoadCustomWords()
        {
            
            string newWordsFilePath = Application.persistentDataPath + "\\new-word-groups.json";
            List<CaseWordData> customWords = new List<CaseWordData>();
            if (File.Exists(newWordsFilePath))
            {
                
                string newWordGroupJSON = File.ReadAllText(newWordsFilePath);
                List<WordGroupData> newWordGroups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WordGroupData>>(newWordGroupJSON);
                foreach (WordGroupData wordGroupData in newWordGroups)
                {
                    wordGroupData.isBaseWordGroup = false;
                    switch(wordGroupData.wordType)
                    {
                        case WordGroupData.WordType.prefixes:
                            InitializeCustomWordGroup("prefixes", wordGroupData);
                            break;
                        case WordGroupData.WordType.nouns:
                            InitializeCustomWordGroup("nouns", wordGroupData);
                            break;
                        case WordGroupData.WordType.variant:
                            InitializeCustomWordGroup("variant", wordGroupData);
                            break;
                        case WordGroupData.WordType.location:
                            InitializeCustomWordGroup("location", wordGroupData);
                            break;
                    }
                    
                    foreach (WordData wordData in wordGroupData.words)
                    {
                        CaseWordData caseWordData = new CaseWordData() { category = wordGroupData.name, difficulty = wordData.difficulty, value = wordData.text, wordType = wordGroupData.wordType, identifier = wordGroupData.wordType.ToString() + "-" + wordData.category+ "-" + wordData.text };
                        customWords.Add(caseWordData);
                    }
                }
            }
            GameManager.Instance.gameDataHandler.RpcSendWords(customWords);
        }

        private void InitializeCustomWordGroup(string wordType, WordGroupData wordGroupData)
        {
            if (_wordGroupMap[wordType].ContainsKey(wordGroupData.name))
            {
                return;
            }
            queueMap[wordType].AddRange(wordGroupData.words);
            _wordGroupMap[wordType].Add(wordGroupData.name, wordGroupData);
            if (SettingsManager.Instance.wordGroupNames.Contains(wordGroupData.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
            {
                categoriesMap[wordType].Add(wordGroupData.name);
            }
        }

        public void DisableInactiveCategories(List<string> activeWordGroupCategories)
        {
            if(activeWordGroupCategories.Count == 0)
            {
                return;
            }
            List<string> categoryNames = categoriesMap.Keys.ToList();
            for(int i = 0; i < categoryNames.Count; i++)
            {
                List<string> category = categoriesMap[categoryNames[i]];
                for (int j = category.Count - 1; j >= 0; j--)
                {
                    if (!activeWordGroupCategories.Contains(category[j]))
                    {
                        category.RemoveAt(j);
                    }
                }
            }
        }

        public CaseChoiceNetData PopulateChoiceWords(CaseChoiceData choice)
        {
            CaseChoiceNetData caseChoiceNetData = new CaseChoiceNetData();
            Dictionary<int, List<string>> possibleWordsMap = new Dictionary<int, List<string>>();
            Dictionary<int, string> correctWordsMap = new Dictionary<int, string>();

            Dictionary<int, List<int>> correctPromptsMap = new Dictionary<int, List<int>>();

            correctPromptsMap = PopulateWordMaps(ref possibleWordsMap, ref correctWordsMap, choice.startingWordIdentifiers);
            caseChoiceNetData.SetWords(possibleWordsMap, correctWordsMap, correctPromptsMap, choice.promptFormat);
            caseChoiceNetData.caseChoiceIdentifier = choice.identifier;
            caseChoiceNetData.maxScoreModifier = choice.maxScoreModifier;
            caseChoiceNetData.scoreModifierDecrement = choice.modifierDecrement;
            caseChoiceNetData.birdbucksPerCorrectWord = choice.pointsPerCorrectWord;
            caseChoiceNetData.bonusBirdbucks = choice.bonusPoints;
            return caseChoiceNetData;
        }

        private Dictionary<int,List<int>> PopulateWordMaps(ref Dictionary<int,List<string>> possibleWordsMap, ref Dictionary<int,string> correctWordIdentifiersMap, List<WordPromptTemplateData> promptData)
        {
            usedWordsMap["prefixes"].Clear();
            usedWordsMap["nouns"].Clear();
            usedWordsMap["variant"].Clear();
            usedWordsMap["location"].Clear();
            Dictionary<int,List<CaseWordTemplateData>> caseWords = GameDataManager.Instance.GetWordTemplates(promptData);
            
            int promptIterator = 1;
            int currentWordIndex = 1;
            Dictionary<int, List<int>> correctPromptMap = new Dictionary<int, List<int>>();
            int totalDifficulty = 0;
            foreach (KeyValuePair<int,List<CaseWordTemplateData>> prompt in caseWords)
            {
                correctPromptMap.Add(promptIterator, new List<int>());
                List<int> wordsInPrompt = new List<int>();
                foreach(CaseWordTemplateData startingWord in prompt.Value)
                {
                    switch (startingWord.type)
                    {
                        case CaseWordTemplateData.CaseWordType.descriptor:
                            
                            totalDifficulty += PopulateWord("prefixes", possibleWordsMap, correctWordIdentifiersMap, startingWord, currentWordIndex);
                            break;
                        case CaseWordTemplateData.CaseWordType.variant:
                            totalDifficulty += PopulateWord("variant", possibleWordsMap, correctWordIdentifiersMap, startingWord, currentWordIndex);
                            break;
                        case CaseWordTemplateData.CaseWordType.noun:
                            totalDifficulty += PopulateWord("nouns", possibleWordsMap, correctWordIdentifiersMap, startingWord, currentWordIndex);
                            break;
                        case CaseWordTemplateData.CaseWordType.location:
                            totalDifficulty += PopulateWord("location", possibleWordsMap, correctWordIdentifiersMap, startingWord, currentWordIndex);
                            break;
                        default:
                            Debug.LogError("Selected case has word with type[" + startingWord.type.ToString() + "] that isn't implemented.");
                            continue;
                    }
                    wordsInPrompt.Add(currentWordIndex);
                    correctPromptMap[promptIterator].Add(currentWordIndex);
                    currentWordIndex++;
                }
            }
            
            return correctPromptMap;
        }

        private int PopulateWord(string wordType, Dictionary<int, List<string>> possibleWordsMap, Dictionary<int,string> correctWordIdentifiersMap, CaseWordTemplateData startingWord, int currentWordIndex)
        {
            int difficulty = 0;
            string category;
            WordGroupData groupData;
            WordData currentWord;
            List<string> possibleCategories = new List<string>();

            foreach(string possibleCategory in categoriesMap[wordType])
            {
                int categoryFrequency = GameDataManager.Instance.GetCategoryFrequency(wordType, possibleCategory);
                for(int i = 0; i < categoryFrequency; i++)
                {
                    possibleCategories.Add(possibleCategory);
                }
            }
            possibleCategories = possibleCategories.OrderBy(a => Guid.NewGuid()).ToList();
            if (possibleCategories.Count == 0)
            {
                Debug.LogError("ERROR[PopulateWordMaps]: There were no valid "+wordType+" categories.");
                return -1;
            }
            category = possibleCategories[0];
            if (!_wordGroupMap[wordType].ContainsKey(category))
            {
                Debug.LogError("ERROR[PopulateWordMaps]: "+ wordType + " did not contain category[" + category + "]");
                return -1;
            }
            groupData = wordGroupMap[wordType][category];
            groupData.Randomize();
            possibleWordsMap.Add(currentWordIndex, new List<string>());
            int iterator = 0;
            while (possibleWordsMap[currentWordIndex].Count < startingWord.numberOfOptionsForGuessing - 1)
            {
                if (groupData.wordCount <= iterator)
                {
                    Debug.LogError("Not enough "+wordType+"[" + iterator.ToString() + "] in category[" + category + "].");
                    return -1;
                }
                currentWord = groupData.GetWord(iterator);
                if (!usedWordsMap[wordType].Contains(currentWord.text))
                {
                    possibleWordsMap[currentWordIndex].Add(currentWord.text);
                    usedWordsMap[wordType].Add(currentWord.text);
                }
                iterator++;
            }

            correctWordIdentifiersMap.Add(currentWordIndex, "");

            while (correctWordIdentifiersMap[currentWordIndex] == "")
            {
                if (groupData.wordCount <= iterator)
                {
                    break;
                }
                currentWord = groupData.GetWord(iterator);
                if (!usedWordsMap[wordType].Contains(currentWord.text) &&
                    currentWord.difficulty >= startingWord.difficultyMinimum &&
                    currentWord.difficulty <= startingWord.difficultyMaximum)
                {
                    usedWordsMap[wordType].Add(currentWord.text);
                    difficulty = currentWord.difficulty;
                    correctWordIdentifiersMap[currentWordIndex] = (wordType + "-" + category + "-" + currentWord.text);
                    possibleWordsMap[currentWordIndex].Add(currentWord.text);
                }

                iterator++;
            }


            if (correctWordIdentifiersMap[currentWordIndex] == "")
            {
                iterator = 0;
                //Try again without considering difficulty
                while (correctWordIdentifiersMap[currentWordIndex] == "")
                {
                    if (groupData.wordCount <= iterator)
                    {
                        Debug.LogError("Failed to generate correct "+wordType+" before running out of options.");
                        return -1;
                    }
                    currentWord = groupData.GetWord(iterator);
                    if (!usedWordsMap[wordType].Contains(currentWord.text))
                    {
                        usedWordsMap[wordType].Add(currentWord.text);
                        difficulty = currentWord.difficulty;
                        correctWordIdentifiersMap[currentWordIndex] = ("prefixes-" + category + "-" + currentWord.text);
                        possibleWordsMap[currentWordIndex].Add(currentWord.text);
                    }

                    iterator++;
                }
            }

            possibleWordsMap[currentWordIndex] = possibleWordsMap[currentWordIndex].OrderBy(a => Guid.NewGuid()).ToList();
            return difficulty;
        }

        public void ClearUsedWords()
        {
            usedWordsMap["prefixes"].Clear();
            usedWordsMap["nouns"].Clear();
            usedWordsMap["variant"].Clear();
            usedWordsMap["location"].Clear();
        }

    }

    [System.Serializable]
    public class WordData
    {
        public string text = "";
        public string category = "";
        public int difficulty = -1;

        public WordData()
        {

        }

        public WordData(string inText, string inCategory, int inDifficulty)
        {
            text = inText;
            category = inCategory;
            difficulty = inDifficulty;
        }
    }
}