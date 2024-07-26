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
    private HatStoreItemData hatStoreItem;

    [SerializeField]
    private List<CaseUpgradeStoreItemData> upgradeStoreItems = new List<CaseUpgradeStoreItemData>();
    private List<CaseUpgradeStoreItemData> activeUpgradeStoreItems = new List<CaseUpgradeStoreItemData>();

    [SerializeField]
    private List<CaseUnlockStoreItemData> unlockStoreItems = new List<CaseUnlockStoreItemData>();
    private List<CaseUnlockStoreItemData> activeUnlockStoreItems = new List<CaseUnlockStoreItemData>();

    [SerializeField]
    private List<BirdData> birdDatas = new List<BirdData>();
    private Dictionary<BirdName, BirdData> birdDataMap = new Dictionary<BirdName, BirdData>();

    [SerializeField]
    private List<RoleData> roles = new List<RoleData>();
    private Dictionary<RoleData.RoleType, RoleData> roleMap = new Dictionary<RoleData.RoleType, RoleData>();

    [SerializeField]
    private List<HatData> hats = new List<HatData>();
    private Dictionary<BirdHatData.HatType, HatData> hatMap = new Dictionary<BirdHatData.HatType, HatData>();

    [SerializeField]
    private List<CertificationData> certificationDatas = new List<CertificationData>();
    private Dictionary<CertificationData.CertificationType, CertificationData> certificationTypeMap = new Dictionary<CertificationData.CertificationType, CertificationData>();
    private Dictionary<string, CertificationData> certificationIdentifierMap = new Dictionary<string, CertificationData>();

    [SerializeField]
    private CaseCertificationStoreItemData certificationStoreItem;
    private List<string> currentShowingCertificationCases = new List<string>();

    [SerializeField]
    private List<TeamMemberData> teamMembers;
    private Dictionary<string, TeamMemberData> teamMemberMap = new Dictionary<string, TeamMemberData>();

    [SerializeField]
    private List<WordCategoryFrequencyData> categoryWordFrequencies = new List<WordCategoryFrequencyData>();
    private Dictionary<string, Dictionary<string,int>> categoryWordFrequencyMap = new Dictionary<string, Dictionary<string,int>>();

    private List<string> currentShowingItems = new List<string>();

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

        hatMap.Clear();
        foreach(HatData hat in hats)
        {
            hatMap.Add(hat.hatType, hat);
        }

        certificationTypeMap.Clear();
        certificationIdentifierMap.Clear();
        foreach (CertificationData certificationData in certificationDatas)
        {
            certificationTypeMap.Add(certificationData.type, certificationData);
            certificationIdentifierMap.Add(certificationData.identifier, certificationData);
        }

        teamMemberMap.Clear();
        foreach(TeamMemberData teamMember in teamMembers)
        {
            teamMemberMap.Add(teamMember.name, teamMember);
        }

        categoryWordFrequencyMap.Clear();
        foreach(WordCategoryFrequencyData frequencyData in categoryWordFrequencies)
        {
            if(!categoryWordFrequencyMap.ContainsKey(frequencyData.wordType))
            {
                categoryWordFrequencyMap.Add(frequencyData.wordType, new Dictionary<string, int>());
            }
            categoryWordFrequencyMap[frequencyData.wordType].Add(frequencyData.categoryName.ToUpper(), frequencyData.frequency);
        }
        //RefreshWords(new List<CaseWordData>());
    }

    public void Reset()
    {
        activeUpgradeStoreItems = new List<CaseUpgradeStoreItemData>(upgradeStoreItems);
        activeUnlockStoreItems = new List<CaseUnlockStoreItemData>(unlockStoreItems);
        foreach (CaseChoiceData choice in caseChoices)
        {
            choice.Reset();
        }
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

    public Dictionary<int,List<CaseWordTemplateData>> GetWordTemplates(List<WordPromptTemplateData> promptDatas)
    {
        Dictionary<int,List<CaseWordTemplateData>> wordTemplates = new Dictionary<int,List<CaseWordTemplateData>>();
        int iterator = 1;
        foreach(WordPromptTemplateData promptData in promptDatas)
        {
            wordTemplates.Add(iterator, new List<CaseWordTemplateData>());
            foreach (string identifier in promptData.caseWordIdentifiers)
            {
                CaseWordTemplateData wordTemplate = GetWordTemplate(identifier);
                if (wordTemplate != null)
                {
                    wordTemplates[iterator].Add(wordTemplate);
                }
            }
            iterator++;
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
        List<CaseUnlockStoreItemData> randomizedUnlocks = activeUnlockStoreItems.OrderBy(cp => Guid.NewGuid()).ToList();

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

    public void AddUpgradeOption(CaseUpgradeStoreItemData upgrade)
    {
        if(!activeUpgradeStoreItems.Contains(upgrade))
        {
            activeUpgradeStoreItems.Add(upgrade);
        }
        else
        {
            Debug.LogError("Failed to add upgrade because it already exists in the active upgrade store items.");
        }
    }

    public CaseUpgradeStoreItemData GetUpgradeStoreItem(int storeTier)
    {
        List<CaseUpgradeStoreItemData> randomizedUpgrades = activeUpgradeStoreItems.OrderBy(cp => Guid.NewGuid()).ToList();
        foreach (CaseUpgradeStoreItemData upgradeStoreItem in randomizedUpgrades)
        {
            //Skip if the upgrade's choice hasn't been unlocked
            if (!GameManager.Instance.playerFlowManager.unlockedCaseChoiceIdentifiers.Contains(upgradeStoreItem.caseChoiceIdentifier) ||
                upgradeStoreItem.tier > storeTier)
            {
                continue;
            }

            //Skip if this upgrade is already showing in the shop
            if (currentShowingItems.Contains(upgradeStoreItem.itemName))
            {
                continue;
            }

            //Skip if this case is trademarked and can't be improved
            if(GameManager.Instance.playerFlowManager.CaseHasCertification(upgradeStoreItem.caseChoiceIdentifier, "Trademark"))
            {
                continue;
            }
            CaseChoiceData caseChoice = GetCaseChoice(upgradeStoreItem.caseChoiceIdentifier);
            if(caseChoice != null)
            {
                int numberOfPlayers = GameManager.Instance.gameFlowManager.GetNumberOfConnectedPlayers();
                if (caseChoice.numberOfTasks <= numberOfPlayers)
                {
                    currentShowingItems.Add(upgradeStoreItem.itemName);
                    return upgradeStoreItem;
                }
            }
            else
            {
                Debug.LogError("Could not find matching case["+upgradeStoreItem.caseChoiceIdentifier+"] for upgrade["+upgradeStoreItem.itemName+"]");
            }
        }
        return null;
    }


    public StoreItemData GetStoreItem(List<StoreItem.StoreItemType> usedTypes, int storeTier)
    {
        StoreItemData chosenStoreItem = null;

        List<StoreItemData> validItems = storeItems.Where(i => (!i.isSinglePurchase || !usedTypes.Contains(i.itemType)) && i.tier <= storeTier ).OrderBy(cp => Guid.NewGuid()).ToList();

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

    public HatStoreItemData GetHatStoreItem()
    {
        return hatStoreItem;
    }

    public string GetHatString()
    {
        string hatString = "";
        foreach(StoreItemData storeItem in storeItemMap.Values)
        {
            if(storeItem.itemType == StoreItem.StoreItemType.hat)
            {
                hatString += ((HatStoreItemData)storeItem).hatType.ToString();
            }
        }
        return hatString;
    }

    public CaseUpgradeStoreItemData GetMatchingCaseUpgradeStoreItem(string itemName)
    {
        foreach(CaseUpgradeStoreItemData caseUpgrade in activeUpgradeStoreItems)
        {
            if(caseUpgrade.itemName == itemName)
            {
                return caseUpgrade;
            }
        }
        Debug.LogError("Could not find matching store item["+itemName+"]");
        return null;
    }

    public CaseUnlockStoreItemData GetMatchingCaseUnlockStoreItem(string caseChoiceIdentifier)
    {
        foreach (CaseUnlockStoreItemData caseUnlockItem in activeUnlockStoreItems)
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

    public void UnlockCaseChoice(string identifier, string certificationIdentifier)
    {
        if(GameManager.Instance.playerFlowManager.caseChoiceUnlockPool.Contains(identifier))
        {
            GameManager.Instance.playerFlowManager.caseChoiceUnlockPool.Remove(identifier);
        }

        GameManager.Instance.playerFlowManager.unlockedCaseChoiceIdentifiers.Add(identifier);
        for(int i = activeUnlockStoreItems.Count - 1; i >= 0; i--)
        {
            if (activeUnlockStoreItems[i].caseChoiceIdentifier == identifier)
            {
                activeUnlockStoreItems.RemoveAt(i);
            }
        }

        CaseChoiceData caseChoice = GetCaseChoice(identifier);
        if (caseChoice != null && caseChoice.numberOfTasks <= SettingsManager.Instance.GetPlayerNameCount())
        {
            caseChoice.SendFrequencyToClients();
        }
        activeUpgradeStoreItems.AddRange(caseChoice.upgrades);

        //Universally set the certification for the case
        if(certificationIdentifier != "")
        {
            GameManager.Instance.playerFlowManager.AddCaseCertification(identifier, certificationIdentifier);
            GameManager.Instance.gameDataHandler.RpcAddCaseCertification(identifier, certificationIdentifier);
            CertificationData certification = GameDataManager.Instance.GetCertification(certificationIdentifier);
            if(certification != null)
            {
                certification.Enable(caseChoice);
            }
            
        }
    }

    public void RemoveStoreItemType(StoreItem.StoreItemType storeItemType)
    {
        for(int i = activeUnlockStoreItems.Count - 1; i >= 0; i--)
        {
            if (activeUnlockStoreItems[i].itemType == storeItemType)
            {
                activeUnlockStoreItems.RemoveAt(i);
            }
        }
    }

    public void UpgradeCaseChoice(CaseUpgradeStoreItemData upgradeData)
    {
        CaseChoiceData upgradingCaseChoice = caseChoiceMap[upgradeData.caseChoiceIdentifier];
        if (upgradingCaseChoice != null)
        {
            upgradingCaseChoice.bonusPoints += upgradeData.upgradeRampData.bonusPointsIncrease;
            upgradingCaseChoice.modifierDecrement += upgradeData.modifierDecrementDecrease;
            upgradingCaseChoice.pointsPerCorrectWord += upgradeData.upgradeRampData.pointsPerCorrectWordIncrease;
            upgradingCaseChoice.startingScoreModifier += upgradeData.upgradeRampData.modifierIncrease;
            for(int i = activeUpgradeStoreItems.Count -1 ; i >= 0; i--)
            {
                if (activeUpgradeStoreItems[i].itemName == upgradeData.itemName)
                {
                    activeUpgradeStoreItems.RemoveAt(i);
                }
            }            
        }
    }

    public void RemoveUpgrade(CaseUpgradeStoreItemData upgradeData)
    {
        for (int i = activeUpgradeStoreItems.Count - 1; i >= 0; i--)
        {
            if (activeUpgradeStoreItems[i].itemName == upgradeData.itemName)
            {
                activeUpgradeStoreItems.RemoveAt(i);
            }
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

    public HatData GetHat(BirdHatData.HatType hatType)
    {
        if(hatMap.ContainsKey(hatType))
        {
            return hatMap[hatType];
        }
        return null;
    }

    public void ResetCurrentShowingItems()
    {
        currentShowingItems.Clear();
        currentShowingCertificationCases.Clear();
    }

    public string GetRandomCertificationIdentifier(int goodChance, int badChance)
    { 
        List<string> options = new List<string>();
        int totalPercent = 0;
        for(int i = 0; i < goodChance; i++)
        {
            options.Add("good");
            totalPercent++;
        }
        for(int i = 0; i < badChance; i++)
        {
            options.Add("bad");
            totalPercent++;
        }
        for(int i = totalPercent; i < 100; i++)
        {
            options.Add("");
        }

        
        options = options.OrderBy(x => Guid.NewGuid()).ToList();
        switch(options[0])
        {
            case "good":
                List<string> goodOptions = certificationDatas.Where(cd => cd.quality == CertificationData.CertificationQuality.good)
                                            .Select(cd => cd.identifier)
                                            .OrderBy(x => Guid.NewGuid()).ToList();
                return goodOptions.Count > 0 ? goodOptions[0] : "";
            case "bad":
                List<string> badOptions = certificationDatas.Where(cd => cd.quality == CertificationData.CertificationQuality.bad)
                                            .Select(cd => cd.identifier)
                                            .OrderBy(x => Guid.NewGuid()).ToList();
                return badOptions.Count > 0 ? badOptions[0] : "";
            default:
                return "";
        }
    }

    public CertificationData GetCertification(string identifier)
    {
        if(certificationIdentifierMap.ContainsKey(identifier))
        {
            return certificationIdentifierMap[identifier];
        }
        return null;
    }

    public void IncreaseCaseChoiceModifier(string caseTypeName, float modifierIncrease)
    {
        CaseChoiceData caseChoice = GetCaseChoice(caseTypeName);
        if(caseChoice != null)
        {
            caseChoice.startingScoreModifier += modifierIncrease;
        }
    }
    
    public void UpgradeRandomCaseChoice()
    {
        List<CaseUpgradeStoreItemData> upgrades = activeUpgradeStoreItems.OrderBy(x => Guid.NewGuid()).ToList();

        foreach (CaseUpgradeStoreItemData upgradeData in upgrades)
        {
            //Skip if the upgrade's choice hasn't been unlocked
            if (!GameManager.Instance.playerFlowManager.unlockedCaseChoiceIdentifiers.Contains(upgradeData.caseChoiceIdentifier))
            {
                continue;
            }

            //Skip if this case is trademarked and can't be improved
            if (GameManager.Instance.playerFlowManager.CaseHasCertification(upgradeData.caseChoiceIdentifier, "Trademark"))
            {
                continue;
            }

            if (upgradeData.upgradeRampData.modifierIncrease > 0)
            {
                GameManager.Instance.gameDataHandler.RpcStoreIncreaseModifierForCase(upgradeData.caseChoiceIdentifier);
            }
            else
            {
                GameManager.Instance.gameDataHandler.RpcStoreIncreaseBirdbucksForCase(upgradeData.caseChoiceIdentifier);
            }
            foreach (StoreItemData upgrade in upgradeData.unlocks)
            {
                if (upgrade.itemType == StoreItem.StoreItemType.case_upgrade)
                {
                    AddUpgradeOption((CaseUpgradeStoreItemData)upgrade);
                }
            }
            UpgradeCaseChoice(upgradeData);
            GameManager.Instance.gameDataHandler.RpcUpgradeCaseChoice(upgradeData.itemName);
            RemoveUpgrade(upgradeData);
            return;
        }


    }

    public CaseCertificationStoreItemData GetCertificationStoreItem()
    {
        return certificationStoreItem;
    }

    public CaseChoiceData GetUncertifiedCaseChoice()
    {
        foreach(CaseChoiceData caseChoice in caseChoiceMap.Values)
        {
            //Skip if the certification is currently showing
            if(currentShowingCertificationCases.Contains(caseChoice.identifier))
            {
                continue;
            }
            if(GameManager.Instance.playerFlowManager.unlockedCaseChoiceIdentifiers.Contains(caseChoice.identifier) && 
                GameManager.Instance.playerFlowManager.GetCaseCertificationCount(caseChoice.identifier) < caseChoice.maxNumberOfSeals &&
                caseChoice.numberOfTasks <= SettingsManager.Instance.GetPlayerNameCount())
            {
                currentShowingCertificationCases.Add(caseChoice.identifier);
                return caseChoice;
            }
        }
        return null;
    }

    public CertificationData GetUnusedGoodCertification(string caseIdentifier)
    {
        List<CertificationData> randomizedCertifications = certificationDatas.OrderBy(x => System.Guid.NewGuid()).ToList();
        foreach (CertificationData certification in randomizedCertifications)
        {
            if (certification.quality != CertificationData.CertificationQuality.good || GameManager.Instance.playerFlowManager.CaseHasCertification(caseIdentifier, certification.identifier))
            {
                continue;
            }
            return certification;
        }
        return null;
    }

    public TeamMemberData GetTeamMember(string teamMemberName)
    {
        if(teamMemberMap.ContainsKey(teamMemberName))
        {
            return teamMemberMap[teamMemberName];
        }
        return null;
    }

    public int GetCategoryFrequency(string wordType, string category)
    {
        if (categoryWordFrequencyMap.ContainsKey(wordType) && categoryWordFrequencyMap[wordType].ContainsKey(category))
        {
            return categoryWordFrequencyMap[wordType][category];
        }
        return 0;
    }

    public void SendInitialCaseFrequencies()
    {
        foreach(string unlockedChoice in GameManager.Instance.playerFlowManager.unlockedCaseChoiceIdentifiers)
        {
            CaseChoiceData caseChoice = GetCaseChoice(unlockedChoice);
            if(caseChoice != null && caseChoice.numberOfTasks <= SettingsManager.Instance.GetPlayerNameCount())
            {
                caseChoice.SendFrequencyToClients();
            }
        }
    }
}
