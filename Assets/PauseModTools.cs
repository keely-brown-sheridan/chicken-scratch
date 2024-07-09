using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class PauseModTools : MonoBehaviour
    {
        [SerializeField]
        private KickConfirmation kickConfirmationPrompt;

        [SerializeField]
        private CoverConfirmation coverConfirmationPrompt;

        [SerializeField]
        private Transform playerListHolder;

        [SerializeField]
        private GameObject playerListingPrefab;

        private Dictionary<ColourManager.BirdName,PausePlayerListing> playerListingsMap = new Dictionary<ColourManager.BirdName,PausePlayerListing>();

        public void Initialize()
        {
            
            List<ColourManager.BirdName> allBirds = SettingsManager.Instance.GetAllActiveBirds();
            foreach(ColourManager.BirdName bird in allBirds)
            {
                if(bird == SettingsManager.Instance.birdName)
                {
                    continue;
                }
                GameObject pauseListingObject = Instantiate(playerListingPrefab, playerListHolder);
                PausePlayerListing pauseListing = pauseListingObject.GetComponent<PausePlayerListing>();

                pauseListing.Initialize(bird, coverConfirmationPrompt, kickConfirmationPrompt);
                playerListingsMap.Add(bird, pauseListing);
            }
        }

        public void DisconnectPlayer(ColourManager.BirdName bird)
        {
            playerListingsMap[bird].OnDisconnect();
        }
    }
}

