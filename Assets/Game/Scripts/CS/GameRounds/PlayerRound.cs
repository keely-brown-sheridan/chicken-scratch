
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class PlayerRound : MonoBehaviour
    {
        public List<GameObject> objectsToHideOnStart = new List<GameObject>(), objectsToShowOnStart = new List<GameObject>();
        public List<string> soundsToPlayOnStart, soundsToStopOnStart;
        public float timeInRound;
        public GameFlowManager.GamePhase gamePhaseName;

        public virtual void StartRound()
        {
            foreach (GameObject objectToShowOnStart in objectsToShowOnStart)
            {
                if (objectToShowOnStart)
                {
                    objectToShowOnStart.SetActive(true);
                }
            }
            foreach (GameObject objectToHideOnStart in objectsToHideOnStart)
            {
                if (objectToHideOnStart)
                {
                    objectToHideOnStart.SetActive(false);
                }
            }



            foreach (string soundToStopOnStart in soundsToStopOnStart)
            {
                AudioManager.Instance.StopSound(soundToStopOnStart);
            }

            foreach (string soundToPlayOnStart in soundsToPlayOnStart)
            {
                AudioManager.Instance.PlaySound(soundToPlayOnStart);
            }

            GameManager.Instance.playerFlowManager.loadingCircleObject.SetActive(false);
            GameManager.Instance.playerFlowManager.currentTimeInRound = timeInRound;

            if (SettingsManager.Instance.isHost)
            {
                GameManager.Instance.gameFlowManager.currentGamePhase = gamePhaseName;
            }

        }
    }
}