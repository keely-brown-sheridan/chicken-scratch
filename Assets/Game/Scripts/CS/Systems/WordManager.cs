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
        private List<string> allVariantCategories = new List<string>();
        public Dictionary<string, WordGroupData> AllNouns => allNouns;
        private Dictionary<string, WordGroupData> allNouns = new Dictionary<string, WordGroupData>();
        public Dictionary<string, WordGroupData> AllPrefixes => allPrefixes;

        public static string testingPrefixIdentifier = "prefixes-NEUTRAL-ATTACHED", testingNounIdentifier = "nouns-ANIMAL-AXOLOTL";
        private Dictionary<string, WordGroupData> allPrefixes = new Dictionary<string, WordGroupData>();

        public Dictionary<string, WordGroupData> AllVariants => allVariants;
        private Dictionary<string, WordGroupData> allVariants = new Dictionary<string, WordGroupData>();

        private List<WordData> nounQueue = new List<WordData>();
        private List<WordData> prefixQueue = new List<WordData>();
        private List<WordData> variantQueue = new List<WordData>();
        private List<string> usedNouns = new List<string>();
        private List<string> usedPrefixes = new List<string>();
        private List<string> usedVariants = new List<string>();

        public void LoadPromptWords()
        {
            allPrefixes.Clear();
            prefixQueue.Clear();
            allNouns.Clear();
            nounQueue.Clear();
            allVariants.Clear();
            variantQueue.Clear();
            //Load in the potential prompts
            List<WordGroupData> nounWordGroups = GameDataManager.Instance.GetWordGroups(WordGroupData.WordType.nouns);
            List<WordGroupData> prefixWordGroups = GameDataManager.Instance.GetWordGroups(WordGroupData.WordType.prefixes);
            List<WordGroupData> variantWordGroups = GameDataManager.Instance.GetWordGroups(WordGroupData.WordType.variant);
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
            foreach (WordGroupData variantWordGroup in variantWordGroups)
            {
                variantWordGroup.isBaseWordGroup = true;
                variantWordGroup.isOn = true;
                variantQueue.AddRange(variantWordGroup.words);
                allVariants.Add(variantWordGroup.name, variantWordGroup);
                if (SettingsManager.Instance.wordGroupNames.Contains(variantWordGroup.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
                {
                    allVariantCategories.Add(variantWordGroup.name);
                }
            }
            prefixQueue = prefixQueue.OrderBy(a => Guid.NewGuid()).ToList();
            nounQueue = nounQueue.OrderBy(a => Guid.NewGuid()).ToList();
            variantQueue = variantQueue.OrderBy(a => Guid.NewGuid()).ToList();
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
                    else if (wordGroupData.wordType == WordGroupData.WordType.variant)
                    {
                        if (allVariants.ContainsKey(wordGroupData.name))
                        {
                            continue;
                        }
                        variantQueue.AddRange(wordGroupData.words);
                        allVariants.Add(wordGroupData.name, wordGroupData);
                        if (SettingsManager.Instance.wordGroupNames.Contains(wordGroupData.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
                        {
                            allVariantCategories.Add(wordGroupData.name);
                        }
                    }
                    foreach (WordData wordData in wordGroupData.words)
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
            for (int i = allVariantCategories.Count - 1; i >= 0; i--)
            {
                if (!activeWordGroupCategories.Contains(allVariantCategories[i]))
                {
                    allVariantCategories.RemoveAt(i);
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
            caseChoiceNetData.SetWords(possibleWordsMap, correctWordsMap, correctPromptsMap);
            caseChoiceNetData.caseChoiceIdentifier = choice.identifier;
            caseChoiceNetData.maxScoreModifier = choice.maxScoreModifier;
            caseChoiceNetData.scoreModifierDecrement = choice.modifierDecrement;
            return caseChoiceNetData;
        }

        private Dictionary<int,List<int>> PopulateWordMaps(ref Dictionary<int,List<string>> possibleWordsMap, ref Dictionary<int,string> correctWordIdentifiersMap, List<WordPromptTemplateData> promptData)
        {
            usedPrefixes.Clear();
            usedNouns.Clear();
            usedVariants.Clear();

            Dictionary<int,List<CaseWordTemplateData>> caseWords = GameDataManager.Instance.GetWordTemplates(promptData);
            string category;
            WordGroupData prefixGroupData;
            WordGroupData nounGroupData;
            WordGroupData variantGroupData;
            int promptIterator = 1;
            int iterator;
            WordData currentWord;
            int currentWordIndex = 1;
            Dictionary<int, List<int>> correctPromptMap = new Dictionary<int, List<int>>();
            string correctPrompt = "";
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
                            allPrefixCategories = allPrefixCategories.OrderBy(a => Guid.NewGuid()).ToList();
                            if (allPrefixCategories.Count == 0)
                            {
                                Debug.LogError("ERROR[PopulateWordMaps]: There were no valid prefix categories.");
                                return new Dictionary<int, List<int>>();
                            }
                            category = allPrefixCategories[0];
                            if (!allPrefixes.ContainsKey(category))
                            {
                                Debug.LogError("ERROR[PopulateWordMaps]: allPrefixes did not contain category[" + category + "]");
                                return new Dictionary<int, List<int>>();
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
                                    return new Dictionary<int, List<int>>();
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
                                    break;
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
                            }


                            if (correctWordIdentifiersMap[currentWordIndex] == "")
                            {
                                iterator = 0;
                                //Try again without considering difficulty
                                while (correctWordIdentifiersMap[currentWordIndex] == "")
                                {
                                    if (prefixGroupData.wordCount <= iterator)
                                    {
                                        Debug.LogError("Failed to generate correct prefix before running out of options.");
                                        return new Dictionary<int, List<int>>();
                                    }
                                    currentWord = prefixGroupData.GetWord(iterator);
                                    if (!usedPrefixes.Contains(currentWord.text))
                                    {
                                        usedPrefixes.Add(currentWord.text);
                                        totalDifficulty += currentWord.difficulty;
                                        correctWordIdentifiersMap[currentWordIndex] = ("prefixes-" + category + "-" + currentWord.text);
                                        possibleWordsMap[currentWordIndex].Add(currentWord.text);
                                    }

                                    iterator++;
                                }
                            }

                            possibleWordsMap[currentWordIndex] = possibleWordsMap[currentWordIndex].OrderBy(a => Guid.NewGuid()).ToList();
                            break;
                        case CaseWordTemplateData.CaseWordType.variant:
                            allVariantCategories = allVariantCategories.OrderBy(a => Guid.NewGuid()).ToList();
                            if (allVariantCategories.Count == 0)
                            {
                                Debug.LogError("ERROR[PopulateWordMaps]: There were no valid variant categories.");
                                return new Dictionary<int, List<int>>();
                            }
                            category = allVariantCategories[0];
                            if (!allVariants.ContainsKey(category))
                            {
                                Debug.LogError("ERROR[PopulateWordMaps]: allVariants did not contain category[" + category + "]");
                                return new Dictionary<int, List<int>>();
                            }
                            variantGroupData = allVariants[category];
                            variantGroupData.Randomize();
                            possibleWordsMap.Add(currentWordIndex, new List<string>());
                            iterator = 0;
                            while (possibleWordsMap[currentWordIndex].Count < startingWord.numberOfOptionsForGuessing - 1)
                            {
                                if (variantGroupData.wordCount <= iterator)
                                {
                                    Debug.LogError("Not enough variants[" + iterator.ToString() + "] in category[" + category + "].");
                                    return new Dictionary<int, List<int>>();
                                }
                                currentWord = variantGroupData.GetWord(iterator);
                                if (!usedVariants.Contains(currentWord.text))
                                {
                                    possibleWordsMap[currentWordIndex].Add(currentWord.text);
                                    usedVariants.Add(currentWord.text);
                                }
                                iterator++;
                            }

                            correctWordIdentifiersMap.Add(currentWordIndex, "");

                            while (correctWordIdentifiersMap[currentWordIndex] == "")
                            {
                                if (variantGroupData.wordCount <= iterator)
                                {
                                    break;
                                }
                                currentWord = variantGroupData.GetWord(iterator);
                                if (!usedVariants.Contains(currentWord.text) &&
                                    currentWord.difficulty >= startingWord.difficultyMinimum &&
                                    currentWord.difficulty <= startingWord.difficultyMaximum)
                                {
                                    usedVariants.Add(currentWord.text);
                                    totalDifficulty += currentWord.difficulty;
                                    correctWordIdentifiersMap[currentWordIndex] = ("variant-" + category + "-" + currentWord.text);
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
                                    if (variantGroupData.wordCount <= iterator)
                                    {
                                        Debug.LogError("Failed to generate correct variant before running out of options.");
                                        return new Dictionary<int, List<int>>();
                                    }
                                    currentWord = variantGroupData.GetWord(iterator);
                                    if (!usedVariants.Contains(currentWord.text))
                                    {
                                        usedVariants.Add(currentWord.text);
                                        totalDifficulty += currentWord.difficulty;
                                        correctWordIdentifiersMap[currentWordIndex] = ("variant-" + category + "-" + currentWord.text);
                                        possibleWordsMap[currentWordIndex].Add(currentWord.text);
                                    }

                                    iterator++;
                                }
                            }

                            possibleWordsMap[currentWordIndex] = possibleWordsMap[currentWordIndex].OrderBy(a => Guid.NewGuid()).ToList();
                            break;
                        case CaseWordTemplateData.CaseWordType.noun:
                            allNounCategories = allNounCategories.OrderBy(a => Guid.NewGuid()).ToList();
                            if (allNounCategories.Count == 0)
                            {
                                Debug.LogError("ERROR[PopulateWordMaps]: There were no valid noun categories.");
                                return new Dictionary<int, List<int>>();
                            }
                            category = allNounCategories[0];
                            if (!allNouns.ContainsKey(category))
                            {
                                Debug.LogError("ERROR[PopulateWordMaps]: allNouns did not contain category[" + category + "]");
                                return new Dictionary<int, List<int>>();
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
                                    return new Dictionary<int, List<int>>();
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
                                if (nounGroupData.wordCount <= iterator)
                                {
                                    break;
                                }
                                currentWord = nounGroupData.GetWord(iterator);

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

                            }
                            if (correctWordIdentifiersMap[currentWordIndex] == "")
                            {
                                //Try again without considering difficulty
                                iterator = 0;
                                while (correctWordIdentifiersMap[currentWordIndex] == "")
                                {
                                    if (nounGroupData.wordCount <= iterator)
                                    {
                                        Debug.LogError("Failed to generate correct noun before running out of options[" + category + "].");
                                        return new Dictionary<int, List<int>>();
                                    }
                                    currentWord = nounGroupData.GetWord(iterator);
                                    if (!usedNouns.Contains(currentWord.text))
                                    {
                                        usedNouns.Add(currentWord.text);
                                        totalDifficulty += currentWord.difficulty;
                                        correctWordIdentifiersMap[currentWordIndex] = ("nouns-" + category + "-" + currentWord.text);
                                        possibleWordsMap[currentWordIndex].Add(currentWord.text);
                                    }

                                    iterator++;

                                }
                            }
                            possibleWordsMap[currentWordIndex] = possibleWordsMap[currentWordIndex].OrderBy(a => Guid.NewGuid()).ToList();

                            break;
                        default:
                            Debug.LogError("Selected case has word with type[" + startingWord.type.ToString() + "] that isn't implemented.");
                            continue;
                    }
                    wordsInPrompt.Add(currentWordIndex);
                    correctPromptMap[promptIterator].Add(currentWordIndex);
                    currentWordIndex++;
                }

                correctPrompt = "";
                foreach(int wordInPrompt in wordsInPrompt)
                {
                    CaseWordData correctWord = GameDataManager.Instance.GetWord(correctWordIdentifiersMap[wordInPrompt]);
                    if(correctWord == null)
                    {
                        Debug.LogError("ERROR[PopulateWordMaps]: correct word could not be found in the GameDataManager for identifier[" + correctWordIdentifiersMap[wordInPrompt] + "]");
                        return new Dictionary<int, List<int>>();
                    }
                    correctPrompt += correctWord.value;
                }
                correctPrompt = correctPrompt.Trim();
            }
            
            return correctPromptMap;
        }

        public void ClearUsedWords()
        {
            usedVariants.Clear();
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