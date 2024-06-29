
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class CabinetDrawer : MonoBehaviour
    {
        public int id;
        public SpriteRenderer tab1, tab2, tab3, tab4, tab5;
        public List<SpriteRenderer> colouredElements;

        public Dictionary<int, SpriteRenderer> tabs = new Dictionary<int, SpriteRenderer>();
        public BirdName currentPlayer = BirdName.none;

        public Color playerColour;
        public GameObject glowObject;
        public float timePerFlash = 0.85f;
        public float shakeDuration = 0.4f;
        public bool ready = false;
        public SpriteRenderer folderRenderer, folderRenderer2, folderRenderer3, folderRenderer4;

        public GameObject ownerImageObject;
        public SpriteRenderer ownerImageRenderer;

        public enum ChainState
        {
            waiting, providing, receiving
        }
        public ChainState chainState = ChainState.waiting;

        public ChainData currentChainData;

        [SerializeField]
        private Transform doorTransform;
        [SerializeField]
        private float shakeVelocity;
        private Animator animator;
        private PlayerFlowManager playerFlowManager;
        private GameFlowManager gameFlowManager;
        private bool isInitialized = false;
        private float totalTimeFlashing = 0.0f;
        private float totalTimeShaking = 0.0f;
        private Vector3 originalDoorPosition;

        // Start is called before the first frame update
        void Awake()
        {
            if (!isInitialized)
            {
                initialize();
            }
        }

        private void Update()
        {
            if (totalTimeFlashing > 0.0f)
            {
                totalTimeFlashing += Time.deltaTime;
                if (totalTimeFlashing > timePerFlash)
                {
                    glowObject.SetActive(!glowObject.activeSelf);
                    if (currentPlayer == SettingsManager.Instance.birdName)
                    {
                        totalTimeFlashing = Time.deltaTime;
                    }
                    else
                    {
                        totalTimeFlashing = 0.0f;
                    }
                }
            }
            if (totalTimeShaking > 0.0f)
            {
                totalTimeShaking += Time.deltaTime;
                if (totalTimeShaking > shakeDuration)
                {
                    //Stop the shaking and reset the position of the door
                    totalTimeShaking = 0.0f;
                    doorTransform.localPosition = originalDoorPosition;
                }
                else
                {
                    //Get a random new position for the shake
                    float xPosition = originalDoorPosition.x + UnityEngine.Random.Range(-shakeVelocity, shakeVelocity);
                    float yPosition = originalDoorPosition.y + UnityEngine.Random.Range(-shakeVelocity, shakeVelocity);
                    doorTransform.localPosition = new Vector3(xPosition, yPosition);
                }

            }
        }

        private void initialize()
        {
            originalDoorPosition = doorTransform.localPosition;
            currentChainData = new ChainData();
            animator = GetComponent<Animator>();
            tabs.Add(1, tab1);
            tabs.Add(2, tab2);
            tabs.Add(3, tab3);
            tabs.Add(4, tab4);
            tabs.Add(5, tab5);
            isInitialized = true;
        }

        public void setCabinetOwner(BirdName cabinetOwner)
        {
            currentPlayer = cabinetOwner;
            BirdData cabinetBird = GameDataManager.Instance.GetBird(cabinetOwner);
            if (cabinetBird == null)
            {
                Debug.LogError("Could not update colour and image of cabinet because cabinet owner["+cabinetOwner.ToString()+"] is not mapped in the Colour Manager.");
            }
            else
            {
                playerColour = cabinetBird.folderColour;
                ownerImageRenderer.sprite = cabinetBird.cabinetFaceSprite;
            }
            List<Hourglass> hourglasses = GameManager.Instance.playerFlowManager.cabinetHourglasses;
            if (hourglasses.Count <= (id - 1))
            {
                Debug.LogError("Could not show cabinet hourglass for id["+(id - 1).ToString()+"] because it was not in the range of cabinetHourglasses["+ hourglasses.Count.ToString()+ "]");
            }
            else
            {
                hourglasses[id - 1].gameObject.SetActive(false);
            }
            
            
            ownerImageObject.SetActive(true);
        }

        public void setDrawerVisuals(BirdName inCurrentPlayer)
        {
            BirdData cabinetBird = GameDataManager.Instance.GetBird(inCurrentPlayer);
            if(cabinetBird == null)
            {
                Debug.LogError("Could not set colour for the drawer because the bird map does not contain currentplayer["+inCurrentPlayer.ToString()+"].");
                return;
            }

            if (inCurrentPlayer == SettingsManager.Instance.birdName)
            {
                animator.SetBool("PlayerCabinet", true);
            }
            currentPlayer = inCurrentPlayer;
            playerColour = cabinetBird.folderColour;
        }


        public void select()
        {
            animator.SetTrigger("Snap");
            if (currentChainData != null &&
                animator.GetBool("Open") &&
                currentPlayer == SettingsManager.Instance.birdName &&
                ready &&
                GameManager.Instance.playerFlowManager.drawingRound.playerIsReady)
            {
                //Turn off the glow
                totalTimeFlashing = 0.0f;
                glowObject.SetActive(false);

                GameManager.Instance.gameDataHandler.CmdDequeueFrontCase(currentPlayer);

                GameManager.Instance.playerFlowManager.drawingRound.UpdateToNewFolderState();
                GameManager.Instance.playerFlowManager.drawingRound.onPlayerStartTask.Invoke();

                ready = false;
            }
            else
            {
                //Shake the cabinet to indicate that it's not ready
                totalTimeShaking = Time.deltaTime;

                //Play a sound effect for it too
                AudioManager.Instance.PlaySound("sfx_game_int_cabinet_shake", true);
            }
        }

        public void hover()
        {
            if (ready)
            {
                AudioManager.Instance.PlaySoundVariant("sfx_game_int_folder_hover");
                animator.SetBool("Hover", true);
            }
            else
            {
                animator.SetBool("Hover", false);
            }

        }
        public void unhover()
        {
            animator.SetBool("Hover", false);
        }

        public void snap()
        {
            animator.SetTrigger("Snap");
            animator.SetBool("Hover", false);
        }


        public void setQueuedFolders(List<BirdName> queuedFolderEntries)
        {
            if (queuedFolderEntries.Count > 0)
            {
                BirdData queuedBird = GameDataManager.Instance.GetBird(queuedFolderEntries[0]);
                if(queuedBird != null)
                {
                    folderRenderer.color = queuedBird.folderColour;
                }
                else
                {
                    folderRenderer.color = playerColour;
                }
                folderRenderer.gameObject.SetActive(true);

                if (queuedFolderEntries.Count > 1)
                {
                    queuedBird = GameDataManager.Instance.GetBird(queuedFolderEntries[1]);
                    if (queuedBird != null)
                    {
                        folderRenderer2.color = queuedBird.folderColour;
                    }
                    else
                    {
                        folderRenderer2.color = playerColour;
                    }
                    folderRenderer2.gameObject.SetActive(true);

                    if (queuedFolderEntries.Count > 2)
                    {
                        queuedBird = GameDataManager.Instance.GetBird(queuedFolderEntries[2]);
                        if (queuedBird != null)
                        {
                            folderRenderer3.color = queuedBird.folderColour;
                        }
                        else
                        {
                            folderRenderer3.color = playerColour;
                        }
                        folderRenderer3.gameObject.SetActive(true);
                    }
                    else
                    {
                        folderRenderer3.gameObject.SetActive(false);
                        folderRenderer4.gameObject.SetActive(false);
                    }

                    if (queuedFolderEntries.Count > 3)
                    {
                        queuedBird = GameDataManager.Instance.GetBird(queuedFolderEntries[3]);
                        if (queuedBird != null)
                        {
                            folderRenderer4.color = queuedBird.folderColour;
                        }
                        else
                        {
                            folderRenderer4.color = playerColour;
                        }
                        folderRenderer4.gameObject.SetActive(true);
                    }
                    else
                    {
                        folderRenderer4.gameObject.SetActive(false);
                    }
                }
                else
                {
                    folderRenderer2.gameObject.SetActive(false);
                    folderRenderer3.gameObject.SetActive(false);
                    folderRenderer4.gameObject.SetActive(false);
                }
            }
            else
            {
                folderRenderer.gameObject.SetActive(false);
                folderRenderer2.gameObject.SetActive(false);
                folderRenderer3.gameObject.SetActive(false);
                folderRenderer4.gameObject.SetActive(false);
            }
        }

        public void setAsReady(BirdName queuedPlayer)
        {
            chainState = ChainState.providing;
            GameManager.Instance.playerFlowManager.hasRunOutOfTime = false;
            ready = true;
            currentPlayer = queuedPlayer;
            bool playerQueuedIsMe = SettingsManager.Instance.birdName == currentPlayer;
            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                GameManager.Instance.playerFlowManager.instructionRound.handleCabinetOpening(id, playerQueuedIsMe);
            }

            setDrawerVisuals(currentPlayer);

            //Pull the drawer open
            animator.SetBool("Open", true);
            AudioManager.Instance.PlaySoundVariant("sfx_game_env_drawer_open");

            if (playerQueuedIsMe)
            {
                //Initialize the drawer glow
                glowObject.SetActive(true);
                totalTimeFlashing += Time.deltaTime;
            }
            else
            {
                glowObject.SetActive(false);
                totalTimeFlashing = 0.0f;
            }
        }

        public void close()
        {
            animator.SetBool("Open", false);
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_folder_submit");
            glowObject.SetActive(false);
            totalTimeFlashing = 0.0f;
        }
    }
}