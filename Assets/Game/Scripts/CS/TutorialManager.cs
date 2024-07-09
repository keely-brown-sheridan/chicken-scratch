using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class TutorialManager : MonoBehaviour
    {
        public float totalTimeToReachTarget;
        public float totalTimeToReturnFromTarget;
        public float totalTimeToRise;
        public float totalTimeToLower;
        public Transform leftArmTransform;
        public Transform rightArmTransform;
        public float leftImageSize;
        public float rightImageSize;
        public GameObject leftStickyObject;
        public GameObject rightStickyObject;
        public GameObject leftHawkManObject;
        public GameObject rightHawkManObject;

        public float startingHeight;
        public float risenHeight;

        private List<string> queuedStickyIdentifiers = new List<string>();
        private Dictionary<string, TutorialSticky> queuedStickies = new Dictionary<string, TutorialSticky>();

        private enum BossState
        {
            idle, left_rising, left_placing, left_returning, left_lowering, right_rising, right_placing, right_returning, right_lowering
        }

        [SerializeField]
        private BossState currentState = BossState.idle;

        private TutorialSticky currentSticky;


        private float timeReachingTarget = 0.0f;
        private float timeReturningFromTarget = 0.0f;
        private float timeRising = 0.0f;
        private float timeLowering = 0.0f;
        private Vector3 startingPosition;
        private Vector3 originalStartingPosition;
        private Vector3 targetScale;
        private Vector3 startingScale;
        private Vector3 leftOriginalStartingScale;
        private Vector3 leftStartingOrientation;
        private Vector3 rightOriginalStartingScale;
        private Vector3 rightStartingOrientation;
        private Vector3 targetDirection;
        private Vector3 targetPosition;


        // Start is called before the first frame update
        void Start()
        {
            leftImageSize = leftArmTransform.GetComponent<Image>().rectTransform.rect.width;
            leftStartingOrientation = leftArmTransform.right;
            leftOriginalStartingScale = leftArmTransform.localScale;
            rightImageSize = rightArmTransform.GetComponent<Image>().rectTransform.rect.width;
            rightStartingOrientation = rightArmTransform.right;
            rightOriginalStartingScale = rightArmTransform.localScale;
            //QueueSticky(GameManager.Instance.playerFlowManager.instructionRound.roleDeskSticky);
            //QueueSticky(GameManager.Instance.playerFlowManager.instructionRound.drawingToolsSticky);
            //StartCoroutine(PlaceTestSticky(10.0f));
        }

        private IEnumerator PlaceTestSticky(float t)
        {
            yield return new WaitForSeconds(t);
        }


        // Update is called once per frame
        void Update()
        {
            switch (currentState)
            {
                case BossState.left_rising:
                case BossState.right_rising:
                    Rise();
                    break;
                case BossState.left_lowering:
                case BossState.right_lowering:
                    Lower();
                    break;
                case BossState.left_placing:
                case BossState.right_placing:
                    ApproachTarget();
                    break;
                case BossState.left_returning:
                case BossState.right_returning:
                    ReturnFromTarget();
                    break;
            }

        }

        private void Rise()
        {
            Transform armTransform;
            GameObject stickyObject;
            GameObject hawkManObject;
            switch (currentState)
            {
                case BossState.left_rising:
                    armTransform = leftArmTransform;
                    stickyObject = leftStickyObject;
                    hawkManObject = leftHawkManObject;
                    break;
                case BossState.right_rising:
                    armTransform = rightArmTransform;
                    stickyObject = rightStickyObject;
                    hawkManObject = rightHawkManObject;
                    break;
                default:
                    return;
            }
            timeRising += Time.deltaTime;
            float currentYPosition = (risenHeight - startingHeight) * timeRising / totalTimeToRise + startingHeight;
            hawkManObject.transform.localPosition = new Vector3(hawkManObject.transform.localPosition.x, currentYPosition, hawkManObject.transform.localPosition.z);

            if (timeRising > totalTimeToRise)
            {
                originalStartingPosition = armTransform.position;
                startingPosition = originalStartingPosition;
                //QueueNextSticky();
            }
        }

        private void Lower()
        {
            GameObject stickyObject;
            GameObject hawkManObject;
            switch (currentState)
            {
                case BossState.left_lowering:
                    stickyObject = leftStickyObject;
                    hawkManObject = leftHawkManObject;
                    break;
                case BossState.right_lowering:
                    stickyObject = rightStickyObject;
                    hawkManObject = rightHawkManObject;
                    break;
                default:
                    return;
            }
            AudioManager.Instance.PlaySound("sfx_game_env_boss_lower");
            timeLowering += Time.deltaTime;
            float currentYPosition = (risenHeight - startingHeight) * (1 - timeLowering / totalTimeToLower) + startingHeight;
            hawkManObject.transform.localPosition = new Vector3(hawkManObject.transform.localPosition.x, currentYPosition, hawkManObject.transform.localPosition.z);

            if (timeLowering > totalTimeToLower)
            {
                timeLowering = 0.0f;
                if (queuedStickyIdentifiers.Count > 0)
                {
                    AudioManager.Instance.PlaySound("sfx_game_env_boss_rise");
                    TutorialSticky queuedSticky = queuedStickies[queuedStickyIdentifiers[0]];
                    if (queuedSticky.transform.position.x > 0)
                    {
                        currentState = BossState.right_rising;
                    }
                    else
                    {
                        currentState = BossState.left_rising;
                    }
                }
                else
                {
                    currentState = BossState.idle;
                }

            }
        }

        private void ApproachTarget()
        {
            Transform armTransform;
            GameObject stickyObject;
            BossState transitionState;
            Vector3 originalStartingScale;
            Vector3 startingOrientation;

            switch (currentState)
            {
                case BossState.left_placing:
                    armTransform = leftArmTransform;
                    stickyObject = leftStickyObject;
                    transitionState = BossState.left_lowering;
                    originalStartingScale = leftOriginalStartingScale;
                    startingOrientation = leftStartingOrientation;
                    break;
                case BossState.right_placing:
                    armTransform = rightArmTransform;
                    stickyObject = rightStickyObject;
                    transitionState = BossState.right_lowering;
                    originalStartingScale = rightOriginalStartingScale;
                    startingOrientation = rightStartingOrientation;
                    break;
                default:
                    return;
            }
            if (timeReachingTarget > 0.0f)
            {
                //Handling for if the sticky is cancelled halfway through placement
                if (currentSticky.hasBeenClicked)
                {
                    stickyObject.SetActive(false);
                    if (queuedStickyIdentifiers.Count > 0)
                    {
                        startingPosition = originalStartingPosition;
                        //QueueNextSticky();
                    }
                    else
                    {
                        armTransform.localScale = originalStartingScale;
                        armTransform.position = originalStartingPosition;
                        armTransform.right = startingOrientation;
                        currentState = transitionState;
                        timeReturningFromTarget = 0.0f;
                    }
                }
                float currentX = (targetScale.x - originalStartingScale.x) * timeReachingTarget / totalTimeToReachTarget + originalStartingScale.x;
                armTransform.localScale = new Vector3(currentX, 1f, 1f);
                timeReachingTarget += Time.deltaTime;
                if (timeReachingTarget > totalTimeToReachTarget)
                {
                    if (!currentSticky.hasBeenClicked)
                    {
                        currentSticky.Place();
                    }

                    stickyObject.SetActive(false);
                    if (queuedStickyIdentifiers.Count > 0)
                    {
                        startingPosition = originalStartingPosition;
                        //QueueNextSticky();
                    }
                    else
                    {
                        armTransform.localScale = originalStartingScale;
                        armTransform.position = originalStartingPosition;
                        armTransform.right = startingOrientation;
                        currentState = transitionState;
                        timeReturningFromTarget = 0.0f;
                    }

                }
            }
        }

        private void ReturnFromTarget()
        {
            Transform armTransform;
            BossState transitionState;
            Vector3 originalStartingScale;
            Vector3 startingOrientation;
            switch (currentState)
            {
                case BossState.left_returning:
                    armTransform = leftArmTransform;
                    transitionState = BossState.left_lowering;
                    originalStartingScale = leftOriginalStartingScale;
                    startingOrientation = leftStartingOrientation;
                    break;
                case BossState.right_returning:
                    armTransform = rightArmTransform;
                    transitionState = BossState.right_lowering;
                    originalStartingScale = rightOriginalStartingScale;
                    startingOrientation = rightStartingOrientation;
                    break;
                default:
                    return;
            }
            if (timeReturningFromTarget > 0.0f)
            {
                float currentX = (targetScale.x - startingScale.x) * (1 - timeReturningFromTarget / totalTimeToReturnFromTarget) + startingScale.x;
                armTransform.localScale = new Vector3(currentX, 1f, 1f);
                timeReturningFromTarget += Time.deltaTime;
                if (timeReturningFromTarget > totalTimeToReturnFromTarget)
                {
                    if (queuedStickyIdentifiers.Count > 0)
                    {
                        startingPosition = targetPosition;
                        //QueueNextSticky();
                    }
                    else
                    {
                        armTransform.localScale = originalStartingScale;
                        armTransform.position = originalStartingPosition;
                        armTransform.right = startingOrientation;
                        currentState = transitionState;
                        timeReturningFromTarget = 0.0f;
                    }

                }
            }
        }
    }
}