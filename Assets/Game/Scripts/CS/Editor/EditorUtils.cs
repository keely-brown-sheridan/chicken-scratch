using ChickenScratch;
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
}
