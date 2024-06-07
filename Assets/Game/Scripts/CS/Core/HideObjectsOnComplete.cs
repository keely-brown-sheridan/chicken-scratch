using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class HideObjectsOnComplete : StateMachineBehaviour
    {
        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            List<GameObject> objectsToHide = animator.gameObject.GetComponent<AnimationComplete>().objectsToHide;
            foreach (GameObject objectToHide in objectsToHide)
            {
                if (objectToHide)
                {
                    objectToHide.SetActive(false);
                }
            }
        }
    }
}