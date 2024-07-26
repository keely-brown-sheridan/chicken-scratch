using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreFrequencyChoicePanel : MonoBehaviour
{
    [SerializeField]
    private GameObject frequencyOptionPrefab;

    [SerializeField]
    private Transform frequencyOptionHolder;

    private Dictionary<string, int> casesFrequencyMap = new Dictionary<string, int>();
    private Dictionary<string, int> caseFrequencyIncreaseMap = new Dictionary<string, int>();

    public void UpdateFrequency(string caseType, int frequency, int rampIndex)
    {
        CaseChoiceData caseData = GameDataManager.Instance.GetCaseChoice(caseType);
        if(caseData != null)
        {
            int potentialIncrease = caseData.GetFrequencyIncreaseValue(rampIndex);
            if (!casesFrequencyMap.ContainsKey(caseType))
            {
                casesFrequencyMap.Add(caseType, frequency);
                caseFrequencyIncreaseMap.Add(caseType, frequency + potentialIncrease);
            }
            else
            {
                casesFrequencyMap[caseType] = frequency;
                caseFrequencyIncreaseMap[caseType] = frequency + potentialIncrease;
            }
        }
    }

    public void RemoveCase(string caseType)
    {
        if(casesFrequencyMap.ContainsKey(caseType))
        {
            casesFrequencyMap.Remove(caseType);
        }
        if(caseFrequencyIncreaseMap.ContainsKey(caseType))
        {
            caseFrequencyIncreaseMap.Remove(caseType);
        }
    }

    public void Open()
    {
        //Clear previous case folders
        List<Transform> previousFrequencies = new List<Transform>();
        foreach(Transform child in frequencyOptionHolder)
        {
            previousFrequencies.Add(child);
        }
        for(int i = previousFrequencies.Count - 1; i >= 0; i--)
        {
            Destroy(previousFrequencies[i].gameObject);
        }

        foreach(KeyValuePair<string,int> caseFrequency in casesFrequencyMap)
        {
            GameObject caseFrequencyChoiceObject = Instantiate(frequencyOptionPrefab, frequencyOptionHolder);
            StoreFrequencyChoiceOption choiceOption = caseFrequencyChoiceObject.GetComponent<StoreFrequencyChoiceOption>();
            choiceOption.Initialize(caseFrequency.Key, caseFrequency.Value, caseFrequencyIncreaseMap[caseFrequency.Key]);
        }

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
