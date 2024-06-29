using ChickenScratch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChickenScratch.CaseWordTemplateData;
using static ChickenScratch.ColourManager;
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

    [SerializeField]
    private List<StoreItemData> storeItems = new List<StoreItemData>();
    private Dictionary<StoreItem.StoreItemType, StoreItemData> storeItemMap = new Dictionary<StoreItem.StoreItemType, StoreItemData>();

    [SerializeField]
    private List<CaseUpgradeStoreItemData> upgradeStoreItems = new List<CaseUpgradeStoreItemData>();

    [SerializeField]
    private List<CaseUnlockStoreItemData> unlockStoreItems = new List<CaseUnlockStoreItemData>();

    [SerializeField]
    private List<BirdData> birdDatas = new List<BirdData>();
    private Dictionary<BirdName, BirdData> birdDataMap = new Dictionary<BirdName, BirdData>();

    [SerializeField]
    private List<RoleData> roles = new List<RoleData>();
    private Dictionary<RoleData.RoleType, RoleData> roleMap = new Dictionary<RoleData.RoleType, RoleData>();

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

        storeItemMap.Clear();
        foreach(StoreItemData storeItem in storeItems)
        {
            storeItemMap.Add(storeItem.itemType, storeItem);
        }

        birdDataMap.Clear();
        foreach(BirdData bird in birdDatas)
        {
            birdDataMap.Add(bird.birdName, bird);
        }

        roleMap.Clear();
        foreach(RoleData role in roles)
        {
            roleMap.Add(role.roleType, role);
        }
        //RefreshWords(new List<CaseWordData>());
    }

    public void RefreshWords(List<CaseWordData> otherWords)
    {
        caseWordMap.Clear();
        foreach (CaseWordData caseWord in wordDataList.allWords)
        {
            if(caseWordMap.ContainsKey(caseWord.identifier))
            {
                continue;
            }
            caseWordMap.Add(caseWord.identifier, caseWord);
        }
        foreach(CaseWordData otherWord in otherWords)
        {
            if(caseWordMap.ContainsKey(otherWord.identifier))
            {
                continue;
            }
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

    public CaseUnlockStoreItemData GetUnlockStoreItem()
    {
        List<CaseUnlockStoreItemData> randomizedUnlocks = unlockStoreItems.OrderBy(cp => Guid.NewGuid()).ToList();

        foreach (CaseUnlockStoreItemData unlockStoreItem in randomizedUnlocks)
        {
            CaseChoiceData caseChoice = GetCaseChoice(unlockStoreItem.caseChoiceIdentifier);
            if(caseChoice != null)
            {
                int numberOfPlayers = GameManager.Instance.gameFlowManager.GetNumberOfConnectedPlayers();
                if (caseChoice.numberOfTasks <= numberOfPlayers)
                {
                    return unlockStoreItem;
                }
            }
        }
        return null;
    }

    public CaseUpgradeStoreItemData GetUpgradeStoreItem()
    {
        List<CaseUpgradeStoreItemData> randomizedUpgrades = upgradeStoreItems.OrderBy(cp => Guid.NewGuid()).ToList();

        foreach (CaseUpgradeStoreItemData upgradeStoreItem in randomizedUpgrades)
        {
            CaseChoiceData caseChoice = GetCaseChoice(upgradeStoreItem.caseChoiceIdentifier);
            if(caseChoice != null)
            {
                int numberOfPlayers = GameManager.Instance.gameFlowManager.GetNumberOfConnectedPlayers();
                if (caseChoice.numberOfTasks <= numberOfPlayers)
                {
                    return upgradeStoreItem;
                }
            }
        }
        return null;
    }


    public StoreItemData GetStoreItem(List<StoreItem.StoreItemType> usedTypes)
    {
        StoreItemData chosenStoreItem = null;

        List<StoreItemData> validItems = storeItems.Where(i => !i.isSinglePurchase || !usedTypes.Contains(i.itemType)).OrderBy(cp => Guid.NewGuid()).ToList();

        if(validItems.Count > 0)
        {
            chosenStoreItem = validItems[0];
        }

        return chosenStoreItem;
    }

    public StoreItemData GetMatchingStoreItem(StoreItem.StoreItemType type)
    {
        if (storeItemMap.ContainsKey(type))
        {
            return storeItemMap[type];
        }

        return null;
    }

    public CaseUpgradeStoreItemData GetMatchingCaseUpgradeStoreItem(string itemName)
    {
        foreach(CaseUpgradeStoreItemData caseUpgrade in upgradeStoreItems)
        {
            if(caseUpgrade.itemName == itemName)
            {
                return caseUpgrade;
            }
        }
        return null;
    }

    public CaseUnlockStoreItemData GetMatchingCaseUnlockStoreItem(string caseChoiceIdentifier)
    {
        foreach (CaseUnlockStoreItemData caseUnlockItem in unlockStoreItems)
        {
            if(caseUnlockItem.caseChoiceIdentifier == caseChoiceIdentifier)
            {
                return caseUnlockItem;
            }
        }
        return null;
    }

    public BirdData GetBird(BirdName birdName)
    {
        if (birdDataMap.ContainsKey(birdName))
        {
            return birdDataMap[birdName];
        }
        return null;
    }

    public void UnlockCaseChoice(CaseUnlockStoreItemData unlockData)
    {
        SettingsManager.Instance.gameMode.caseChoiceIdentifiers.Add(unlockData.caseChoiceIdentifier);
        unlockStoreItems.Remove(unlockData);
    }

    public void UpgradeCaseChoice(CaseUpgradeStoreItemData upgradeData)
    {
        CaseChoiceData upgradingCaseChoice = caseChoiceMap[upgradeData.caseChoiceIdentifier];
        if (upgradingCaseChoice != null)
        {
            upgradingCaseChoice.bonusPoints += upgradeData.bonusPointIncrease;
            upgradingCaseChoice.modifierDecrement += upgradeData.modifierDecrementDecrease;
            upgradingCaseChoice.pointsPerCorrectWord += upgradeData.correctWordPointIncrease;
            upgradingCaseChoice.maxScoreModifier += upgradeData.startingModifierIncrease;
            upgradingCaseChoice.startingScoreModifier += upgradeData.startingModifierIncrease;
            upgradeStoreItems.Remove(upgradeData);
        }
    }

    public RoleData GetRole(RoleData.RoleType roleType)
    {
        if(roleMap.ContainsKey(roleType))
        {
            return roleMap[roleType];
        }
        return null;
    }
}
