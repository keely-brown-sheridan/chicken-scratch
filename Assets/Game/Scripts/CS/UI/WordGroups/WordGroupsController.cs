using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using Mirror;

namespace ChickenScratch
{
    public class WordGroupsController : MonoBehaviour
    {
        [SerializeField]
        private AddWordGroupsContainer addWordGroupContainer;

        [SerializeField]
        private EditWordGroupsContainer editWordGroupContainer;

        [SerializeField]
        private MainWordGroupsContainer mainWordGroupContainer;

        [SerializeField]
        private ReviewWordGroupsContainer reviewWordGroupContainer;

        [SerializeField]
        private WordGroupReviewContainer wordGroupReviewContainer;

        [SerializeField]
        private LobbyNotReadyManager lobbyNotReadyManager;

        [SerializeField]
        private GameObject editWordsStickyObject;


        private WordManager wordManager = new WordManager();
        private string editingGroupName = "";

        private List<WordGroupData> wordGroups = new List<WordGroupData>();
        private Dictionary<string, bool> toggledCategoryNamesMap = new Dictionary<string, bool>();
        private DateTime lastWordGroupUpdateTime = DateTime.MinValue;
        private bool isHost => NetworkServer.connections.Count > 0;
        private void Start()
        {
            //Initialize();
        }

        public void Initialize()
        {
            lastWordGroupUpdateTime = DateTime.MinValue;
            reviewWordGroupContainer.ClearWordGroups();
            editWordsStickyObject.SetActive(isHost);
            mainWordGroupContainer.SetEditButtonActiveState(isHost);
            wordGroups.Clear();
            if (isHost)
            {
                //Load all of the base word groups
                wordManager.LoadPromptWords();

                //Create prefabs of each of the groups
                foreach (KeyValuePair<string, WordGroupData> prefixGroup in wordManager.wordGroupMap["prefixes"])
                {
                    wordGroups.Add(prefixGroup.Value);
                    reviewWordGroupContainer.CreateLockedWordGroup(prefixGroup.Key, prefixGroup.Value.wordType, prefixGroup.Value.words, this, isHost);
                }
                foreach (KeyValuePair<string, WordGroupData> nounGroup in wordManager.wordGroupMap["nouns"])
                {
                    wordGroups.Add(nounGroup.Value);
                    reviewWordGroupContainer.CreateLockedWordGroup(nounGroup.Key, nounGroup.Value.wordType, nounGroup.Value.words, this, isHost);
                }
                foreach (KeyValuePair<string, WordGroupData> variantGroup in wordManager.wordGroupMap["variant"])
                {
                    wordGroups.Add(variantGroup.Value);
                    reviewWordGroupContainer.CreateLockedWordGroup(variantGroup.Key, variantGroup.Value.wordType, variantGroup.Value.words, this, isHost);
                }
                foreach (KeyValuePair<string, WordGroupData> locationGroup in wordManager.wordGroupMap["location"])
                {
                    wordGroups.Add(locationGroup.Value);
                    reviewWordGroupContainer.CreateLockedWordGroup(locationGroup.Key, locationGroup.Value.wordType, locationGroup.Value.words, this, isHost);
                }

                //Load the player created word groups
                string newWordsPath = Application.persistentDataPath + "\\new-word-groups.json";
                if(File.Exists(newWordsPath))
                {
                    string newWordGroupJSON = File.ReadAllText(newWordsPath);
                    List<WordGroupData> newWordGroups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WordGroupData>>(newWordGroupJSON);
                    foreach (WordGroupData wordGroupData in newWordGroups)
                    {
                        wordGroups.Add(wordGroupData);
                        reviewWordGroupContainer.CreateWordGroup(wordGroupData.name, wordGroupData.wordType, wordGroupData.words, this, isHost);
                    }
                }

                UpdateWordGroupList();
            }


        }

        public void AddWordGroup()
        {
            reviewWordGroupContainer.gameObject.SetActive(false);
            addWordGroupContainer.gameObject.SetActive(true);
        }

        public void EditWordGroup(string groupName)
        {
            editingGroupName = groupName;
            WordGroupData selectedGroupData = null;

            //Find the group
            foreach (WordGroupData groupData in wordGroups)
            {
                if (groupData.name == groupName)
                {
                    selectedGroupData = groupData;
                }
            }
            if (selectedGroupData == null)
            {
                Debug.LogError("Could not find matching word group for editing.");
                return;
            }

            editWordGroupContainer.Open(selectedGroupData);

            reviewWordGroupContainer.gameObject.SetActive(false);
            editWordGroupContainer.gameObject.SetActive(true);
        }

        public void AddNewWordGroup()
        {
            List<string> existingWordGroupNames = new List<string>();
            foreach (WordGroupData wordGroup in wordGroups)
            {
                existingWordGroupNames.Add(wordGroup.name);
            }

            List<WordData> words = new List<WordData>();
            if (!addWordGroupContainer.CanSave(existingWordGroupNames, ref words))
            {
                return;
            }
            WordGroupData currentWordGroup = addWordGroupContainer.Save(words);
            wordGroups.Add(currentWordGroup);

            //Add a new word group to the list
            reviewWordGroupContainer.CreateWordGroup(currentWordGroup.name, currentWordGroup.wordType, currentWordGroup.words, this, isHost);

            addWordGroupContainer.ClearWords();
            addWordGroupContainer.gameObject.SetActive(false);
            editWordGroupContainer.gameObject.SetActive(false);
            reviewWordGroupContainer.gameObject.SetActive(true);
        }

        public void EditExistingWordGroup()
        {
            List<string> existingWordGroupNames = new List<string>();
            WordGroupData existingGroupData = null;
            foreach (WordGroupData wordGroup in wordGroups)
            {
                if (wordGroup.name == editWordGroupContainer.editingCategoryName)
                {
                    existingGroupData = wordGroup;
                }
                existingWordGroupNames.Add(wordGroup.name);
            }

            List<WordData> words = new List<WordData>();

            if (!editWordGroupContainer.CanSave(existingWordGroupNames, ref words))
            {
                return;
            }
            WordGroupData selectedWordGroup = editWordGroupContainer.Save(words, existingGroupData);
            addWordGroupContainer.ClearWords();
            addWordGroupContainer.gameObject.SetActive(false);
            editWordGroupContainer.gameObject.SetActive(false);
            reviewWordGroupContainer.gameObject.SetActive(true);

            //Update word group list if the name changed
            WordGroupItem[] wordGroupItems = transform.GetComponentsInChildren<WordGroupItem>();
            foreach (WordGroupItem wordGroupItem in wordGroupItems)
            {
                if (wordGroupItem.name == editingGroupName)
                {
                    selectedWordGroup.isOn = wordGroupItem.IsOn();
                    wordGroupItem.Initialize(selectedWordGroup, this, isHost);
                }
            }
        }

        public void CancelAddWordGroup()
        {
            addWordGroupContainer.ClearWords();
            addWordGroupContainer.gameObject.SetActive(false);
            reviewWordGroupContainer.gameObject.SetActive(true);
        }

        public void CancelEditWordGroup()
        {
            editWordGroupContainer.gameObject.SetActive(false);
            reviewWordGroupContainer.gameObject.SetActive(true);
        }

        public void CancelReviewWordGroups()
        {
            reviewWordGroupContainer.gameObject.SetActive(false);
            mainWordGroupContainer.gameObject.SetActive(true);
        }

        public void SaveWordGroups()
        {
            //Save it to file
            string newWordGroupsSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(wordGroups.Where(wg => !wg.isBaseWordGroup));
            File.WriteAllText(Application.persistentDataPath + "\\new-word-groups.json", newWordGroupsSerialized);

            //Update the main word groups list
            UpdateWordGroupList();

            reviewWordGroupContainer.gameObject.SetActive(false);
            mainWordGroupContainer.gameObject.SetActive(true);
        }

        public void UpdateWordGroups(List<WordGroupData> inWordGroups)
        {
            wordGroups = inWordGroups;
            mainWordGroupContainer.ClearWordGroupDisplayItems();
            foreach(WordGroupData wordGroup in wordGroups)
            {
                if(wordGroup.isOn)
                {
                    mainWordGroupContainer.CreateWordGroupDisplayItem(wordGroup.name, wordGroup.wordType, wordGroup.words, this);
                }
            }
        }
       

        public void UpdateWordGroupList()
        {
            SettingsManager.Instance.wordGroupNames.Clear();

            int numberOfPrefixGroups = 0;
            int numberOfNounGroups = 0;

            foreach (WordGroupData wordGroup in wordGroups)
            {
                if (!toggledCategoryNamesMap.ContainsKey(wordGroup.name))
                {
                    toggledCategoryNamesMap.Add(wordGroup.name, wordGroup.isOn);
                }
                else
                {
                    toggledCategoryNamesMap[wordGroup.name] = wordGroup.isOn;
                }

                if (wordGroup.isOn)
                {
                    if(toggledCategoryNamesMap.ContainsKey(wordGroup.name))
                    {
                        SettingsManager.Instance.wordGroupNames.Add(wordGroup.name);
                    }
                    if (wordGroup.wordType == WordGroupData.WordType.prefixes)
                    {
                        numberOfPrefixGroups++;
                    }
                    else if (wordGroup.wordType == WordGroupData.WordType.nouns)
                    {
                        numberOfNounGroups++;
                    }
                }
            }
            lobbyNotReadyManager.enoughWordGroupsAreSelected = (numberOfPrefixGroups >= 3 && numberOfNounGroups >= 3);
            if(LobbyNetwork.Instance != null && LobbyNetwork.Instance.lobbyDataHandler != null)
            {
                //Send message to players about what word groups there are and are active
                LobbyNetwork.Instance.lobbyDataHandler.RpcUpdateWordGroups(wordGroups);
            }
            
            
        }

        public void DeleteWordGroup(string groupName)
        {
            for (int i = wordGroups.Count - 1; i >= 0; i--)
            {
                if (wordGroups[i].name == groupName)
                {
                    wordGroups.RemoveAt(i);
                }
            }
            string newWordGroupsSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(wordGroups.Where(wg => !wg.isBaseWordGroup));
            File.WriteAllText(Application.persistentDataPath + "\\new-word-groups.json", newWordGroupsSerialized);
        }

        public void ToggleWordGroup(string groupName, bool toggleValue)
        {
            foreach (WordGroupData wordGroup in wordGroups)
            {
                if (wordGroup.name == groupName)
                {
                    wordGroup.isOn = toggleValue;
                }
            }
        }

        public void ReviewWordGroup(string groupName, List<WordData> words)
        {
            wordGroupReviewContainer.Refresh(groupName, words);
            mainWordGroupContainer.gameObject.SetActive(false);
            wordGroupReviewContainer.gameObject.SetActive(true);
        }

        public void EditWordGroups()
        {
            mainWordGroupContainer.gameObject.SetActive(false);
            reviewWordGroupContainer.gameObject.SetActive(true);
        }

        public void CloseReviewWordGroup()
        {
            mainWordGroupContainer.gameObject.SetActive(true);
            wordGroupReviewContainer.gameObject.SetActive(false);
        }

    }

    [System.Serializable]
    public class WordGroupData
    {
        public string name = "";
        public enum WordType
        {
            prefixes, nouns, variant, location, invalid
        }
        public WordType wordType = WordType.invalid;
        public List<WordData> words = new List<WordData>();
        public bool isOn = true;
        public int wordCount => words.Count;
        public bool isBaseWordGroup = true;

        public WordGroupData()
        {

        }

        public WordGroupData(string inName, WordType inWordType, List<WordData> inWords, bool inIsOn, bool inIsBaseWordGroup)
        {
            name = inName;
            wordType = inWordType;
            words = inWords;
            isOn = inIsOn;
            isBaseWordGroup = inIsBaseWordGroup;
        }

        public void AddWord(WordData newWord)
        {
            if (words.Any(w => w.text == newWord.text))
            {
                //Cannot add the same word twice
                return;
            }
            words.Add(newWord);
        }
        public void RemoveWord(WordData removingWord)
        {
            for (int i = words.Count - 1; i <= 0; i--)
            {
                if (words[i].text == removingWord.text)
                {
                    words.RemoveAt(i);
                    return;
                }
            }
        }
        public void SetType(WordType newType)
        {
            wordType = newType;
        }
        public void SetName(string newName)
        {
            name = newName;
        }

        public void Randomize()
        {
            words = words.OrderBy(a => Guid.NewGuid()).ToList();
        }

        public WordData GetWord(int index)
        {
            if(words.Count <= index)
            {
                return null;
            }
            return words[index];
        }


    }
}