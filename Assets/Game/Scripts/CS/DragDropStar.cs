using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class DragDropStar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Shimmy hoverShimmy;
        [SerializeField]
        private float snapbackDuration;
        [SerializeField]
        private float restockDuration;
        [SerializeField]
        private Image dropStarImage;
        public enum State
        {
            idle, hovered, held, snap_back, award, inactive, restock, invalid
        }
        public State currentState = State.idle;
        private Vector3 startingPosition;
        private Vector3 snapbackPosition;
        private Vector3 targetPosition;
        private float snapbackTime = 0.0f;
        private float restockTime = 0.0f;
        private PeanutBird likedBird = null;

        void OnEnable()
        {
            startingPosition = transform.position;
            hoverShimmy.Stop();
            switch (currentState)
            {
                case State.idle:
                case State.restock:
                    dropStarImage.raycastTarget = true;
                    //Do nothing
                    break;
                case State.inactive:
                    gameObject.SetActive(false);
                    break;
                default:
                    Debug.LogError("Drag drop star is in an invalid state[" + currentState.ToString() + "] to start the slides round.");
                    break;
            }
        }

        void OnDisable()
        {

        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            switch (currentState)
            {
                case State.hovered:
                    if (Input.GetMouseButtonDown(0))
                    {
                        AudioManager.Instance.PlaySoundVariant("sfx_vote_int_star_select");
                        hoverShimmy.Stop();
                        //AudioManager.Instance.PlaySound("StickyPeel");
                        currentState = State.held;
                    }
                    break;
                case State.held:
                    dropStarImage.raycastTarget = false;
                    transform.position = Input.mousePosition;
                    SlidesRound slidesRound = GameManager.Instance.playerFlowManager.slidesRound;
                    GoldStarDetectionArea currentGoldStarDetectionArea = slidesRound.currentHoveredGoldStarDetectionArea;

                    if (currentGoldStarDetectionArea != null)
                    {
                        currentGoldStarDetectionArea.Hover();
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (currentGoldStarDetectionArea != null && currentGoldStarDetectionArea.GiveStar())
                        {
                            //Send the star to the target bird
                            likedBird = slidesRound.GetChatBird(currentGoldStarDetectionArea.birdName);
                            if (likedBird == null)
                            {
                                Debug.LogError("Could not send star to target bird["+currentGoldStarDetectionArea.birdName.ToString()+"] because it doesn't exist as a chat bird in the slides round.");
                                return;
                            }
                            targetPosition = likedBird.transform.position;
                            snapbackPosition = transform.position;
                            currentState = State.award;
                            snapbackTime = 0.0f;
                        }
                        else
                        {
                            snapbackPosition = transform.position;
                            currentState = State.snap_back;
                            snapbackTime = 0.0f;
                        }
                    }
                    break;
                case State.snap_back:
                    snapbackTime += Time.deltaTime;
                    transform.position = snapbackPosition + (startingPosition - snapbackPosition) * snapbackTime / snapbackDuration;
                    if (snapbackTime > snapbackDuration)
                    {
                        dropStarImage.raycastTarget = true;
                        transform.position = startingPosition;
                        currentState = State.idle;
                    }
                    break;
                case State.award:
                    snapbackTime += Time.deltaTime;
                    float progressRatio = snapbackTime / snapbackDuration;

                    if (snapbackTime > snapbackDuration)
                    {
                        likedBird.AddLike();
                        currentState = State.inactive;
                        gameObject.SetActive(false);
                    }
                    transform.localScale = new Vector3(1 - snapbackTime / snapbackDuration * 0.6f, 1 - snapbackTime / snapbackDuration * 0.6f, 1 - snapbackTime / snapbackDuration * 0.6f);
                    transform.position = snapbackPosition + (targetPosition - snapbackPosition) * snapbackTime / snapbackDuration;

                    break;
                case State.restock:
                    restockTime += Time.deltaTime;
                    float newScale = restockTime / restockDuration;
                    transform.localScale = new Vector3(newScale, newScale, 1.0f);
                    if (restockTime > restockDuration)
                    {
                        transform.localScale = Vector3.one;
                        restockTime = 0.0f;
                        currentState = State.idle;
                    }
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentState == State.idle)
            {
                currentState = State.hovered;
                hoverShimmy.Resume();
            }


        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentState == State.hovered)
            {
                hoverShimmy.Stop();
                currentState = State.idle;
            }

        }

        public bool Restock()
        {
            if (currentState != State.inactive)
            {
                return false;
            }
            transform.position = startingPosition;
            currentState = State.restock;
            gameObject.SetActive(true);

            return true;
        }


    }
}