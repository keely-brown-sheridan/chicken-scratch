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

        public enum WordType
        {
            noun, descriptor, invalid
        }

        private List<string> allPrefixCategories = new List<string>();
        private List<string> allNounCategories = new List<string>();
        public Dictionary<string, WordGroupData> AllNouns => allNouns;
        private Dictionary<string, WordGroupData> allNouns = new Dictionary<string, WordGroupData>();
        public Dictionary<string, WordGroupData> AllPrefixes => allPrefixes;
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
            string prefixGroupJSON = File.ReadAllText(Application.persistentDataPath + "\\prefix-groups.json");
            string nounGroupJSON = File.ReadAllText(Application.persistentDataPath + "\\nouns-groups.json");
            List<WordGroupData> nounWordGroups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WordGroupData>>(nounGroupJSON);
            List<WordGroupData> prefixWordGroups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WordGroupData>>(prefixGroupJSON);

            foreach (WordGroupData prefixWordGroup in prefixWordGroups)
            {
                prefixWordGroup.isBaseWordGroup = true;
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
            string newWordGroupJSON = File.ReadAllText(Application.persistentDataPath + "\\new-word-groups.json");
            List<WordGroupData> newWordGroups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WordGroupData>>(newWordGroupJSON);
            foreach (WordGroupData wordGroupData in newWordGroups)
            {
                wordGroupData.isBaseWordGroup = false;
                if (wordGroupData.wordType == WordGroupData.WordType.prefixes)
                {
                    prefixQueue.AddRange(wordGroupData.words);
                    allPrefixes.Add(wordGroupData.name, wordGroupData);
                    if (SettingsManager.Instance.wordGroupNames.Contains(wordGroupData.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
                    {
                        allPrefixCategories.Add(wordGroupData.name);
                    }
                }
                else if (wordGroupData.wordType == WordGroupData.WordType.nouns)
                {
                    nounQueue.AddRange(wordGroupData.words);
                    allNouns.Add(wordGroupData.name, wordGroupData);
                    if (SettingsManager.Instance.wordGroupNames.Contains(wordGroupData.name) || SettingsManager.Instance.wordGroupNames.Count == 0)
                    {
                        allNounCategories.Add(wordGroupData.name);
                    }
                }
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

        public void PopulateStandardCaseWords(ChainData caseData, List<CaseWordTemplateData> startingWords)
        {
            Dictionary<int, List<string>> possibleWordsMap = new Dictionary<int, List<string>>();
            Dictionary<int, CaseWordData> correctWordsMap = new Dictionary<int, CaseWordData>();

            string correctPrompt = PopulateWordMaps(ref possibleWordsMap, ref correctWordsMap, startingWords);

            caseData.possibleWordsMap = possibleWordsMap;
            caseData.correctWordsMap = correctWordsMap;
            caseData.correctPrompt = correctPrompt;
        }

        public void PopulateChoiceWords(CaseChoiceData choice)
        {
            Dictionary<int, List<string>> possibleWordsMap = new Dictionary<int, List<string>>();
            Dictionary<int, CaseWordData> correctWordsMap = new Dictionary<int, CaseWordData>();

            choice.correctPrompt = PopulateWordMaps(ref possibleWordsMap, ref correctWordsMap, choice.startingWords);

            List<List<string>> possibleWords = new List<List<string>>();
            List<CaseWordData> correctWords = new List<CaseWordData>();
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
            choice.possibleWordsMap = possibleWords;
            choice.correctWordsMap = correctWords;

        }

        private string PopulateWordMaps(ref Dictionary<int,List<string>> possibleWordsMap, ref Dictionary<int,CaseWordData> correctWordsMap, List<CaseWordTemplateData> caseWords)
        {
            usedPrefixes.Clear();
            usedNouns.Clear();
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
                        category = allPrefixCategories[0];
                        prefixGroupData = allPrefixes[category];
                        prefixGroupData.Randomize();
                        possibleWordsMap.Add(currentWordIndex, new List<string>());
                        iterator = 0;
                        while (possibleWordsMap[currentWordIndex].Count < startingWord.numberOfOptionsForGuessing - 1)
                        {
                            if (prefixGroupData.wordCount <= iterator)
                            {
                                Debug.LogError("Not enough prefixes[" + iterator.ToString() + "] in category[" + category + "].");
                            }
                            currentWord = prefixGroupData.GetWord(iterator);
                            if (!usedPrefixes.Contains(currentWord.text))
                            {
                                possibleWordsMap[currentWordIndex].Add(currentWord.text);
                                usedPrefixes.Add(currentWord.text);
                            }
                            iterator++;
                        }

                        correctWordsMap.Add(currentWordIndex, new CaseWordData("", startingWord, -1));

                        while (correctWordsMap[currentWordIndex].value == "")
                        {
                            if (prefixGroupData.wordCount <= iterator)
                            {
                                Debug.LogError("Not enough prefixes[" + iterator.ToString() + "] in category[" + category + "].");
                            }
                            currentWord = prefixGroupData.GetWord(iterator);
                            if (!usedPrefixes.Contains(currentWord.text) &&
                                currentWord.difficulty >= startingWord.difficultyMinimum &&
                                currentWord.difficulty <= startingWord.difficultyMaximum)
                            {
                                usedPrefixes.Add(currentWord.text);
                                totalDifficulty += currentWord.difficulty;
                                correctWordsMap[currentWordIndex].value = currentWord.text;
                                correctWordsMap[currentWordIndex].difficulty = currentWord.difficulty;
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
                        category = allNounCategories[0];
                        nounGroupData = allNouns[category];
                        nounGroupData.Randomize();


                        possibleWordsMap.Add(currentWordIndex, new List<string>());
                        iterator = 0;
                        while (possibleWordsMap[currentWordIndex].Count < startingWord.numberOfOptionsForGuessing - 1)
                        {
                            if (nounGroupData.wordCount <= iterator)
                            {
                                Debug.LogError("Could not map enough nouns from category: " + category + "[" + nounGroupData.wordCount.ToString() + "]");
                            }
                            currentWord = nounGroupData.GetWord(iterator);
                            if (!usedNouns.Contains(currentWord.text))
                            {
                                possibleWordsMap[currentWordIndex].Add(currentWord.text);
                                usedNouns.Add(currentWord.text);
                            }
                            iterator++;
                        }

                        correctWordsMap.Add(currentWordIndex, new CaseWordData("", startingWord, -1));

                        while (correctWordsMap[currentWordIndex].value == "")
                        {
                            currentWord = nounGroupData.GetWord(iterator);
                            bool isLastWord = caseWords[caseWords.Count - 1] == startingWord;
                            if (!usedNouns.Contains(currentWord.text) &&
                                currentWord.difficulty >= startingWord.difficultyMinimum &&
                                currentWord.difficulty <= startingWord.difficultyMaximum)
                            {
                                usedNouns.Add(currentWord.text);
                                totalDifficulty += currentWord.difficulty;
                                correctWordsMap[currentWordIndex].value = currentWord.text;
                                correctWordsMap[currentWordIndex].difficulty = currentWord.difficulty;
                                possibleWordsMap[currentWordIndex].Add(currentWord.text);
                            }

                            iterator++;
                            if (nounGroupData.wordCount <= iterator)
                            {
                                Debug.LogError("Couldn't map a correct noun for a prompt, reached the end of possible options.");
                                break;
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
            //Remove the last added white space from the correct prompt
            correctPrompt = correctWordsMap[1].value + " " + correctWordsMap[2].value;
            

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