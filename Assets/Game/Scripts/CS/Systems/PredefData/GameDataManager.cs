using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.CaseWordTemplateData;
using static ChickenScratch.WordGroupData;

public class GameDataManager : Singleton<GameDataManager>
{
    [SerializeField]
    private List<CaseChoiceData> caseChoices = new List<CaseChoiceData>();

    private Dictionary<string,CaseChoiceData> caseChoiceMap = new Dictionary<string,CaseChoiceData>();

    [SerializeField]
    private List<CaseWordTemplateData> caseWordTemplateDatas = new List<CaseWordTemplateData>();

    private Dictionary<string, CaseWordTemplateData> caseWordTemplateMap = new Dictionary<string, CaseWordTemplateData>();

    [SerializeField]
    private WordDataList wordDataList;

    private Dictionary<string, CaseWordData> caseWordMap = new Dictionary<string, CaseWordData>();

    // Start is called before the first frame update
    void Awake()
    {
        caseChoiceMap.Clear();
        foreach (CaseChoiceData caseChoice in caseChoices)
        {
            caseChoiceMap.Add(caseChoice.identifier, caseChoice);
        }

        caseWordTemplateMap.Clear();
        foreach (CaseWordTemplateData caseWordTemplate in caseWordTemplateDatas)
        {
            caseWordTemplateMap.Add(caseWordTemplate.identifier, caseWordTemplate);
        }
        //RefreshWords(new List<CaseWordData>());
    }

    public void RefreshWords(List<CaseWordData> otherWords)
    {
        caseWordMap.Clear();
        foreach (CaseWordData caseWord in wordDataList.allWords)
        {
            caseWordMap.Add(caseWord.identifier, caseWord);
        }
        foreach(CaseWordData otherWord in otherWords)
        {
            caseWordMap.Add(otherWord.identifier, otherWord);
        }
    }

    public CaseChoiceData GetCaseChoice(string inIdentifier)
    {
        if(caseChoiceMap.ContainsKey(inIdentifier))
        {
            return caseChoiceMap[inIdentifier];
        }
        return null;
    }

    public List<CaseChoiceData> GetCaseChoices(List<string> inIdentifiers)
    {
        List<CaseChoiceData> caseChoices = new List<CaseChoiceData>();
        foreach(string identifier in inIdentifiers)
        {
            CaseChoiceData caseChoice = GetCaseChoice(identifier);
            if(caseChoice != null)
            {
                caseChoices.Add(caseChoice);
            }
        }
        return caseChoices;
    }

    public CaseWordTemplateData GetWordTemplate(string inIdentifier)
    {
        if(caseWordTemplateMap.ContainsKey(inIdentifier))
        {
            return caseWordTemplateMap[inIdentifier];
        }
        return null;
    }

    public List<CaseWordTemplateData> GetWordTemplates(List<string> inIdentifiers)
    {
        List<CaseWordTemplateData> wordTemplates = new List<CaseWordTemplateData>();
        foreach(string identifier in inIdentifiers)
        {
            CaseWordTemplateData wordTemplate = GetWordTemplate(identifier);
            if(wordTemplate != null)
            {
                wordTemplates.Add(wordTemplate);
            }
        }
        return wordTemplates;
    }

    public CaseWordData GetWord(string inIdentifier)
    {
        if(caseWordMap.ContainsKey(inIdentifier))
        {
            return caseWordMap[inIdentifier];
        }
        Debug.LogError("Could not find word with identifier[" + inIdentifier + "]");
        return null;
    }

    public List<WordGroupData> GetWordGroups(WordType wordType)
    {
        List<WordGroupData> wordGroups = new List<WordGroupData>();
        Dictionary<string, int> wordGroupCategoryMap = new Dictionary<string, int>();
        int wordGroupIndex;
        foreach (CaseWordData caseWord in wordDataList.allWords)
        {
            if(caseWord.wordType == wordType)
            {
                if (!wordGroupCategoryMap.ContainsKey(caseWord.category))
                {
                    wordGroups.Add(new WordGroupData());
                    wordGroupIndex = wordGroups.Count - 1;
                    wordGroupCategoryMap.Add(caseWord.category, wordGroupIndex);
                    wordGroups[wordGroupIndex].wordType = wordType;
                    wordGroups[wordGroupIndex].name = caseWord.category;
                }
                wordGroupIndex = wordGroupCategoryMap[caseWord.category];
                WordData word = new WordData();
                word.text = caseWord.value;
                word.difficulty = caseWord.difficulty;
                word.category = caseWord.category;
                wordGroups[wordGroupIndex].AddWord(word);
            }

        }
        return wordGroups;
    }
}
