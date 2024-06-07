
using UnityEngine;
using static ChickenScratch.ReactionIndex;

namespace ChickenScratch
{
    public class ReactionButtonManager : MonoBehaviour
    {
        public Colourizer colourizer;

        void Start()
        {

        }

        public void Initialize()
        {
            Bird playerBird = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName];
            colourizer.Colourize(playerBird.colour);
        }
        public void OnReactionButtonPress(string reactionName)
        {
            Reaction reaction = ReactionIndex.GetReactionFromText(reactionName);
            if (reaction == Reaction.invalid)
            {
                Debug.LogError("Invalid reaction provided to reaction button press method[" + reactionName + "]");
            }
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_emoji");

            if (SettingsManager.Instance.isHost)
            {
                int currentRound = GameManager.Instance.playerFlowManager.slidesRound.currentSlideContentIndex;

                StatTracker.Instance.AddToReactionCounter(SettingsManager.Instance.birdName, reactionName, currentRound);
                GameManager.Instance.playerFlowManager.slidesRound.showReaction(SettingsManager.Instance.birdName, ReactionIndex.GetReactionFromText(reactionName));
                GameManager.Instance.gameDataHandler.RpcShowReaction(SettingsManager.Instance.birdName, ReactionIndex.GetReactionFromText(reactionName));
            }
            //Send the reaction to the server if not the server
            else
            {
                GameManager.Instance.gameDataHandler.CmdReaction(SettingsManager.Instance.birdName, reaction);
            }
        }
    }
}