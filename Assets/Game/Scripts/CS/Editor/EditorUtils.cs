using ChickenScratch;
using Codice.CM.Client.Differences;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorUtils : MonoBehaviour
{
    [MenuItem("GameData/Regenerate Word Identifiers")]
    public static void RegenerateWordIdentifiers()
    {
        //Load the scriptable object
        WordDataList wordList = (WordDataList)AssetDatabase.LoadAssetAtPath("Assets/Game/Data/Words/Wo_word-list.asset", typeof(WordDataList));
        
        //iterate through the words
        foreach(CaseWordData word in wordList.allWords)
        {
            //set their identifiers
            word.identifier = word.wordType.ToString() + "-" + word.category.ToString() + "-" + word.value;
        }

        //save
        EditorUtility.SetDirty(wordList);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("GameData/Load Words from CSV")]
    public static void LoadWordsFromCSV()
    {
        //Load the scriptable object
        WordDataList wordList = (WordDataList)AssetDatabase.LoadAssetAtPath("Assets/Game/Data/Words/Wo_word-list.asset", typeof(WordDataList));

        Dictionary<string, List<string>> categoryWords = new Dictionary<string, List<string>>();

        //iterate through the words to ensure we don't repeat any words
        foreach (CaseWordData word in wordList.allWords)
        {
            if(!categoryWords.ContainsKey(word.category))
            {
                categoryWords.Add(word.category, new List<string>());
            }
            categoryWords[word.category].Add(word.value);
        }

        //Load the CSV file 
        TextAsset allPromptsText = (TextAsset)Resources.Load("prompts", typeof(TextAsset));
        List<string> allPrompts = new List<string>(allPromptsText.text.Split('\n'));
        foreach(string prompt in allPrompts)
        {
            List<string> allFields = new List<string>(prompt.Split(','));
            if(allFields.Count < 4)
            {
                Debug.Log("Skipping prompt because it doesn't have enough fields: " + prompt);
                continue;
            }
            string wordValue = allFields[2].ToUpper();
            string category = allFields[3].ToUpper();
            int difficulty = -1;
            if (!int.TryParse(allFields[1], out difficulty))
            {
                Debug.Log("Skipping prompt because the difficulty couldn't be loaded: " + prompt);
                continue;
            }

            WordGroupData.WordType wordType = WordGroupData.WordType.invalid;
            switch(allFields[0])
            {
                case "noun":
                    wordType = WordGroupData.WordType.nouns;
                    break;
                case "descriptor":
                    wordType = WordGroupData.WordType.prefixes;
                    break;
                case "variant":
                    wordType = WordGroupData.WordType.variant;
                    break;
                case "location":
                    wordType = WordGroupData.WordType.location;
                    break;
                default:
                    continue;
            }
            
            
            if (!categoryWords.ContainsKey(category))
            {
                categoryWords.Add(category, new List<string>());
            }
            if (categoryWords[category].Contains(wordValue))
            {
                Debug.Log("Skipping prompt because the word already existed: " + prompt);
                continue;
            }
            string identifier = wordType.ToString() + "-" + category.ToString() + "-" + wordValue;
            CaseWordData newWord = new CaseWordData() { category = category, value = wordValue, difficulty = difficulty, wordType = wordType, identifier = identifier };

            categoryWords[category].Add(wordValue);
            wordList.allWords.Add(newWord);
        }
        //save
        EditorUtility.SetDirty(wordList);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
