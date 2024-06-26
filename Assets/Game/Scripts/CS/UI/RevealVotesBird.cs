﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class RevealVotesBird : MonoBehaviour
    {
        public List<IndexMap> votesForBird;
        public int index;
        public bool isInitialized = false;
        public Image birdImage;

        public Dictionary<int, Image> voteImageMap;
        public bool isActive = false;
        public int numberOfVotes = 0;


        void Start()
        {
            if (!isInitialized)
            {
                initialize();
            }
        }

        public void initialize()
        {
            voteImageMap = new Dictionary<int, Image>();
            foreach (IndexMap voteForBird in votesForBird)
            {
                voteImageMap.Add(voteForBird.index, voteForBird.gameObject.GetComponent<Image>());
            }

            isInitialized = true;
        }

    }
}