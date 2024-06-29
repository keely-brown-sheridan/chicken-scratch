using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace ChickenScratch
{
    public class WordManager
    {
        private List<string> allPrefixCategories = new List<string>();
        private List<string> allNounCategories = new List<string>();
        public Dictionary<string, WordGroupData> AllNouns => allNouns;
        private Dictionary<string, WordGroupData> allNouns = new Dictionary<string, WordGroupData>();
        public Dictionary<string, WordGroupData> AllPrefixes => allPrefixes;

        public static string testingPrefixIdentifier = "prefixes-NEUTRAL-ATTACHED", testingNounIdentifier = "nouns-ANIMAL-AXOLOTL";
        private Dictionary<string, WordGroupData> allPrefixes = new Dictionary<string, WordGroupData>();

        private List<WordData> nounQueue = new List<WordData>();
        private List<WordData> prefixQueue = new List<WordData>();
        private List<string> usedNouns = new List<string>();
        private List<string> usedPrefixes = new List<string>();

        public void LoadPromptWords()
        {
            allPrefixes.Clear();
            prefixQueue.Clear();
            allNouns.Clear();
            nounQueue.Clear();
            //Load in the potential prompts
            List<WordGroupData> nounWordGroups = GameDataManager.Instance.GetWordGroups(WordGroupData.WordType.nouns);
            List<WordGroupData> prefixWordGroups = GameDataManager.Instance.GetWordGroups(WordGroupData.WordType.prefixes);

            foreach (WordGroupData prefixWordGroup in prefixWordGroups)
            {
                prefixWordGroup.isBaseWordGroup = true;
                prefixWordGroup.isOn = true;
                prefixQueue.AddRange(prefixWordGroup.words);
                allPrefixes.Add(prefixWordGroup.name, prefixWordGroup);
                if (SettingsManager.Instance.wordGroupNames.Contains(prefixWordGroup.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
                {
                    allPrefixCategories.Add(prefixWordGroup.name);
                }

            }
            foreach (WordGroupData nounWordGroup in nounWordGroups)
            {
                nounWordGroup.isBaseWordGroup = true;
                nounWordGroup.isOn = true;
                nounQueue.AddRange(nounWordGroup.words);
                allNouns.Add(nounWordGroup.name, nounWordGroup);
                if (SettingsManager.Instance.wordGroupNames.Contains(nounWordGroup.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
                {
                    allNounCategories.Add(nounWordGroup.name);
                }


            }
            prefixQueue = prefixQueue.OrderBy(a => Guid.NewGuid()).ToList();
            nounQueue = nounQueue.OrderBy(a => Guid.NewGuid()).ToList();
        }

        public void LoadCustomWords()
        {
            
            string newWordsFilePath = Application.persistentDataPath + "\\new-word-groups.json";
            if (File.Exists(newWordsFilePath))
            {
                List<CaseWordData> customWords = new List<CaseWordData>();
                string newWordGroupJSON = File.ReadAllText(newWordsFilePath);
                List<WordGroupData> newWordGroups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WordGroupData>>(newWordGroupJSON);
                foreach (WordGroupData wordGroupData in newWordGroups)
                {
                    wordGroupData.isBaseWordGroup = false;
                    if (wordGroupData.wordType == WordGroupData.WordType.prefixes)
                    {
                        if(allPrefixes.ContainsKey(wordGroupData.name))
                        {
                            continue;
                        }
                        prefixQueue.AddRange(wordGroupData.words);
                        allPrefixes.Add(wordGroupData.name, wordGroupData);
                        if (SettingsManager.Instance.wordGroupNames.Contains(wordGroupData.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
                        {
                            allPrefixCategories.Add(wordGroupData.name);
                        }
                    }
                    else if (wordGroupData.wordType == WordGroupData.WordType.nouns)
                    {
                        if(allNouns.ContainsKey(wordGroupData.name))
                        {
                            continue;
                        }
                        nounQueue.AddRange(wordGroupData.words);
                        allNouns.Add(wordGroupData.name, wordGroupData);
                        if (SettingsManager.Instance.wordGroupNames.Contains(wordGroupData.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
                        {
                            allNounCategories.Add(wordGroupData.name);
                        }
                    }
                    foreach(WordData wordData in wordGroupData.words)
                    {
                        CaseWordData caseWordData = new CaseWordData() { category = wordGroupData.name, difficulty = wordData.difficulty, value = wordData.text, wordType = wordGroupData.wordType, identifier = wordGroupData.wordType.ToString() + "-" + wordData.category+ "-" + wordData.text };
                        customWords.Add(caseWordData);
                    }
                }
                GameManager.Instance.gameDataHandler.RpcSendWords(customWords);
            }
        }

        public void DisableInactiveCategories(List<string> activeWordGroupCategories)
        {
            if(activeWordGroupCategories.Count == 0)
            {
                return;
            }
            for (int i = allPrefixCategories.Count - 1; i >= 0; i--)
            {
                if (!activeWordGroupCategories.Contains(allPrefixCategories[i]))
                {
                    allPrefixCategories.RemoveAt(i);
                }
            }
            for (int i = allNounCategories.Count - 1; i >= 0; i--)
            {
                if (!activeWordGroupCategories.Contains(allNounCategories[i]))
                {
                    allNounCategories.RemoveAt(i);
                }
            }
        }

        public void PopulateStandardCaseWords(ChainData caseData, List<string> startingWordIdentifiers)
        {
            Dictionary<int, List<string>> possibleWordsMap = new Dictionary<int, List<string>>();
            Dictionary<int, string> correctWordsMap = new Dictionary<int, string>();

            string correctPrompt = PopulateWordMaps(ref possibleWordsMap, ref correctWordsMap, startingWordIdentifiers);
            if(correctPrompt == "")
            {
                Debug.LogError("ERROR[PopulateStandardCaseWords]: Could not generate a correct prompt for the case.");
                return;
            }

            caseData.possibleWordsMap = possibleWordsMap;
            caseData.correctWordIdentifierMap = correctWordsMap;
            caseData.correctPrompt = correctPrompt;
        }

        public CaseChoiceNetData PopulateChoiceWords(CaseChoiceData choice)
        {
            CaseChoiceNetData caseChoiceNetData = new CaseChoiceNetData();
            Dictionary<int, List<string>> possibleWordsMap = new Dictionary<int, List<string>>();
            Dictionary<int, string> correctWordsMap = new Dictionary<int, string>();

            caseChoiceNetData.correctPrompt = PopulateWordMaps(ref possibleWordsMap, ref correctWordsMap, choice.startingWordIdentifiers);
            if (caseChoiceNetData.correctPrompt == "")
            {
                Debug.LogError("ERROR[PopulateChoiceWords]: Could not generate a correct prompt for the case.");
                return null;
            }

            List<List<string>> possibleWords = new List<List<string>>();
            List<string> correctWords = new List<string>();
            //Order the keys
            List<int> orderedKeys = possibleWordsMap.Keys.ToList();
            orderedKeys.Sort();
            foreach(int key in orderedKeys)
            {
                possibleWords.Add(possibleWordsMap[key]);
            }
            //Order the keys
            orderedKeys = correctWordsMap.Keys.ToList();
            orderedKeys.Sort();
            foreach (int key in orderedKeys)
            {
                correctWords.Add(correctWordsMap[key]);
            }
            caseChoiceNetData.possibleWordsMap = possibleWords;
            caseChoiceNetData.correctWordIdentifiersMap = correctWords;
            caseChoiceNetData.caseChoiceIdentifier = choice.identifier;
            caseChoiceNetData.maxScoreModifier = choice.maxScoreModifier;
            caseChoiceNetData.scoreModifierDecrement = choice.modifierDecrement;
            return caseChoiceNetData;
        }

        private string PopulateWordMaps(ref Dictionary<int,List<string>> possibleWordsMap, ref Dictionary<int,string> correctWordIdentifiersMap, List<string> caseWordIdentifiers)
        {
            usedPrefixes.Clear();
            usedNouns.Clear();

            List<CaseWordTemplateData> caseWords = GameDataManager.Instance.GetWordTemplates(caseWordIdentifiers);
            string category;
            WordGroupData prefixGroupData;
            WordGroupData nounGroupData;
            int iterator;
            WordData currentWord;
            int currentWordIndex = 1;
            string correctPrompt = "";
            int totalDifficulty = 0;
            foreach (CaseWordTemplateData startingWord in caseWords)
            {
                switch (startingWord.type)
                {
                    case CaseWordTemplateData.CaseWordType.descriptor:
                        allPrefixCategories = allPrefixCategories.OrderBy(a => Guid.NewGuid()).ToList();
                        if(allPrefixCategories.Count == 0)
                        {
                            Debug.LogError("ERROR[PopulateWordMaps]: There were no valid prefix categories.");
                            return "";
                        }
                        category = allPrefixCategories[0];
                        if(!allPrefixes.ContainsKey(category))
                        {
                            Debug.LogError("ERROR[PopulateWordMaps]: allPrefixes did not contain category[" + category + "]");
                            return "";
                        }
                        prefixGroupData = allPrefixes[category];
                        prefixGroupData.Randomize();
                        possibleWordsMap.Add(currentWordIndex, new List<string>());
                        iterator = 0;
                        while (possibleWordsMap[currentWordIndex].Count < startingWord.numberOfOptionsForGuessing - 1)
                        {
                            if (prefixGroupData.wordCount <= iterator)
                            {
                                Debug.LogError("Not enough prefixes[" + iterator.ToString() + "] in category[" + category + "].");
                                return "";
                            }
                            currentWord = prefixGroupData.GetWord(iterator);
                            if (!usedPrefixes.Contains(currentWord.text))
                            {
                                possibleWordsMap[currentWordIndex].Add(currentWord.text);
                                usedPrefixes.Add(currentWord.text);
                            }
                            iterator++;
                        }

                        correctWordIdentifiersMap.Add(currentWordIndex, "");

                        while (correctWordIdentifiersMap[currentWordIndex] == "")
                        {
                            if (prefixGroupData.wordCount <= iterator)
                            {
                                Debug.LogError("Not enough prefixes[" + iterator.ToString() + "] in category[" + category + "].");
                                return "";
                            }
                            currentWord = prefixGroupData.GetWord(iterator);
                            if (!usedPrefixes.Contains(currentWord.text) &&
                                currentWord.difficulty >= startingWord.difficultyMinimum &&
                                currentWord.difficulty <= startingWord.difficultyMaximum)
                            {
                                usedPrefixes.Add(currentWord.text);
                                totalDifficulty += currentWord.difficulty;
                                correctWordIdentifiersMap[currentWordIndex] = ("prefixes-" + category + "-" + currentWord.text);
                                possibleWordsMap[currentWordIndex].Add(currentWord.text);
                            }

                            iterator++;
                            if (prefixGroupData.wordCount <= iterator)
                            {
                                Debug.LogError("Couldn't map a correct prefix for a prompt, reached the end of possible options.");
                                break;
                            }
                        }
                        possibleWordsMap[currentWordIndex] = possibleWordsMap[currentWordIndex].OrderBy(a => Guid.NewGuid()).ToList();
                        break;
                    case CaseWordTemplateData.CaseWordType.noun:
                        allNounCategories = allNounCategories.OrderBy(a => Guid.NewGuid()).ToList();
                        if (allNounCategories.Count == 0)
                        {
                            Debug.LogError("ERROR[PopulateWordMaps]: There were no valid noun categories.");
                            return "";
                        }
                        category = allNounCategories[0];
                        if (!allNouns.ContainsKey(category))
                        {
                            Debug.LogError("ERROR[PopulateWordMaps]: allNouns did not contain category[" + category + "]");
                            return "";
                        }
                        nounGroupData = allNouns[category];
                        nounGroupData.Randomize();


                        possibleWordsMap.Add(currentWordIndex, new List<string>());
                        iterator = 0;
                        while (possibleWordsMap[currentWordIndex].Count < startingWord.numberOfOptionsForGuessing - 1)
                        {
                            if (nounGroupData.wordCount <= iterator)
                            {
                                Debug.LogError("Could not map enough nouns from category: " + category + "[" + nounGroupData.wordCount.ToString() + "]");
                                return "";
                            }
                            currentWord = nounGroupData.GetWord(iterator);
                            if (!usedNouns.Contains(currentWord.text))
                            {
                                possibleWordsMap[currentWordIndex].Add(currentWord.text);
                                usedNouns.Add(currentWord.text);
                            }
                            iterator++;
                        }

                        correctWordIdentifiersMap.Add(currentWordIndex, "");

                        while (correctWordIdentifiersMap[currentWordIndex] == "")
                        {
                            currentWord = nounGroupData.GetWord(iterator);
                            bool isLastWord = caseWords[caseWords.Count - 1] == startingWord;
                            if (!usedNouns.Contains(currentWord.text) &&
                                currentWord.difficulty >= startingWord.difficultyMinimum &&
                                currentWord.difficulty <= startingWord.difficultyMaximum)
                            {
                                usedNouns.Add(currentWord.text);
                                totalDifficulty += currentWord.difficulty;
                                correctWordIdentifiersMap[currentWordIndex] = ("nouns-" + category + "-" + currentWord.text);
                                possibleWordsMap[currentWordIndex].Add(currentWord.text);
                            }

                            iterator++;
                            if (nounGroupData.wordCount <= iterator)
                            {
                                Debug.LogError("Couldn't map a correct noun for a prompt, reached the end of possible options.");
                                return "";
                            }
                        }
                        possibleWordsMap[currentWordIndex] = possibleWordsMap[currentWordIndex].OrderBy(a => Guid.NewGuid()).ToList();

                        break;
                    default:
                        Debug.LogError("Selected case has word with type[" + startingWord.type.ToString() + "] that isn't implemented.");
                        continue;
                }

                currentWordIndex++;
            }
            if(!correctWordIdentifiersMap.ContainsKey(1))
            {
                Debug.LogError("ERROR[PopulateWordMaps]: correctWordIdentifiersMap did not contain a prefix.");
                return "";
            }
            if(!correctWordIdentifiersMap.ContainsKey(2))
            {
                Debug.LogError("ERROR[PopulateWordMaps]: correctWordIdentifiersMap did not contain a noun.");
                return "";
            }

            //Remove the last added white space from the correct prompt
            CaseWordData correctPrefix = GameDataManager.Instance.GetWord(correctWordIdentifiersMap[1]);
            CaseWordData correctNoun = GameDataManager.Instance.GetWord(correctWordIdentifiersMap[2]);
            if(correctPrefix == null)
            {
                Debug.LogError("ERROR[PopulateWordMaps]: correctPrefix could not be found in the GameDataManager for identifier[" + correctWordIdentifiersMap[1] + "]");
                return "";
            }
            if(correctNoun == null)
            {
                Debug.LogError("ERROR[PopulateWordMaps]: correctNoun could not be found in the GameDataManager for identifier[" + correctWordIdentifiersMap[2] + "]");
                return "";
            }

            correctPrompt = correctPrefix.value + " " + correctNoun.value;
            

            return correctPrompt;
        }

        public void ClearUsedWords()
        {
            usedNouns.Clear();
            usedPrefixes.Clear();
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