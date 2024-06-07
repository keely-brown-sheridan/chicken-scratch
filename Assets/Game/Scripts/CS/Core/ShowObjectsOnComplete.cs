using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class ShowObjectsOnComplete : StateMachineBehaviour
    {
        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            List<GameObject> objectsToShow = animator.gameObject.GetComponent<AnimationComplete>().objectsToShow;
            foreach (GameObject objectToShow in objectsToShow)
            {
                if (objectToShow)
                {
                    objectToShow.SetActive(true);
                }
            }
        }

    }
}