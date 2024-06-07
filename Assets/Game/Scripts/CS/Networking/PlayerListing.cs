﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

namespace ChickenScratch
{
    public class PlayerListing : MonoBehaviour
    {
        public string PlayerID { get; private set; }

        public Text NameText;
        private Text PlayerName
        {
            get { return NameText; }
        }

        public Image PlayerImage;

        [SerializeField]
        private GameObject hostIndicatorObject;
        [SerializeField]
        private GameObject kickButtonObject;

        public ColourManager.BirdName SelectedBird => selectedBird;

        private ColourManager.BirdName selectedBird = ColourManager.BirdName.none;

        public void ApplyPlayer(string playerID, string name, bool isHost)
        {
            PlayerID = playerID;
            PlayerName.text = name;

            hostIndicatorObject.SetActive(isHost);
            //kickButtonObject.SetActive(PhotonNetwork.IsMasterClient && photonPlayer.NickName != PhotonNetwork.NickName);
        }

        public void ChangePlayerBird(ColourManager.BirdName inBirdName)
        {
            selectedBird = inBirdName;
            if (inBirdName == ColourManager.BirdName.none) return;
            PlayerImage.sprite = ColourManager.Instance.birdMap[inBirdName].scannerFaceSprite;
            //PlayerName.color = ColourManager.Instance.birdMap[inBirdName].colour;
            PlayerImage.gameObject.SetActive(true);
        }

        public void ResetPlayerText()
        {
            //PlayerName.color = Color.black;
            PlayerImage.gameObject.SetActive(false);
        }

        public void KickPlayer()
        {
            //foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
            //{
            //    if (!player.Value.IsLocal && player.Value.NickName == PlayerName.text)
            //    {
            //        PhotonNetwork.CloseConnection(player.Value);
            //    }
            //}
        }


    }
}