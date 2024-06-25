using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ReactionIndex;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;
using static ChickenScratch.DrawingLineData;

namespace ChickenScratch
{
    public class PeanutBird : MonoBehaviour
    {
        public ColourManager.BirdName birdName;
        public bool isInitialized = false;
        public float totalTimeToShowReaction = 3.0f;
        [SerializeField]
        private GameObject likeEffectPrefab;
        [SerializeField]
        private List<GameObject> inactiveStickerObjects;

        [SerializeField]
        private Transform birdImageHolder;
        private float currentTimeShowingReaction = 0.0f;
        private Reaction currentReaction = Reaction.invalid;

        public List<ReactionIndex> reactions = new List<ReactionIndex>();
        public Colourizer colourizer;
        private Dictionary<Reaction, GameObject> reactionMap = new Dictionary<Reaction, GameObject>();

        public void Awake()
        {
            reactionMap.Clear();
            foreach (ReactionIndex reaction in reactions)
            {
                if (reaction.reaction == Reaction.invalid)
                {
                    Debug.LogError("Could not add reaction[" + reaction.gameObject.name + "] to the reaction map because the reaction was invalid.");
                    return;
                }
                if (reactionMap.ContainsKey(reaction.reaction))
                {
                    Debug.LogError("Could not add reaction[" + reaction.reaction.ToString() + "] to the reaction map because the key already exists in the reaction map.");
                    return;
                }
                reactionMap.Add(reaction.reaction, reaction.gameObject);
            }
        }

        void Update()
        {
            if (currentTimeShowingReaction > 0.0f)
            {
                currentTimeShowingReaction += Time.deltaTime;
                if (currentTimeShowingReaction > totalTimeToShowReaction)
                {
                    currentTimeShowingReaction = 0.0f;
                    reactionMap[currentReaction].SetActive(false);
                    currentReaction = Reaction.invalid;
                }
            }
        }

        public void Initialize(BirdName inBirdName)
        {
            gameObject.SetActive(true);
            Instantiate(ColourManager.Instance.birdMap[inBirdName].slidesBirdPrefab, birdImageHolder);
            colourizer.Colourize(ColourManager.Instance.birdMap[inBirdName].colour);
            birdName = inBirdName;
            isInitialized = true;
        }


        public void ShowReaction(Reaction reaction)
        {
            if (currentReaction == reaction)
            {
                currentTimeShowingReaction = Time.deltaTime;
            }
            else
            {
                if (currentReaction != Reaction.invalid)
                {
                    reactionMap[currentReaction].SetActive(false);
                }

                currentReaction = reaction;
                reactionMap[currentReaction].SetActive(true);
                AudioManager.Instance.PlaySoundVariant("sfx_vote_int_emoji");
                currentTimeShowingReaction = Time.deltaTime;
            }
        }

        public void AddLike()
        {
            //Spawn the like effect at the bird and give them a sticker
            Vector3 effectSpawnPosition = Camera.main.ScreenToWorldPoint(transform.position);
            effectSpawnPosition = new Vector3(effectSpawnPosition.x, effectSpawnPosition.y, 0f);
            GameObject likeEffectObject = Instantiate(likeEffectPrefab, effectSpawnPosition, Quaternion.identity);
            Destroy(likeEffectObject, 2.5f);
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_star_assign");

            if (inactiveStickerObjects.Count > 0)
            {
                int selectedInactiveStickerIndex = Random.Range(0, inactiveStickerObjects.Count);
                inactiveStickerObjects[selectedInactiveStickerIndex].SetActive(true);
                inactiveStickerObjects.RemoveAt(selectedInactiveStickerIndex);
            }

        }

    }
}