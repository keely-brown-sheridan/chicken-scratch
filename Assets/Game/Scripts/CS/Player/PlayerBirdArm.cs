using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class PlayerBirdArm : MonoBehaviour
    {
        public SpriteRenderer sprite;
        public SpriteRenderer outlineSprite;
        public ColourManager.BirdName birdName;
        public float moveDelay = 0.5f;
        public float barrierPosition = 100.0f;
        public LayerMask cabinetLayer;
        public LayerMask toolLayer;
        public LayerMask casePileLayer;
        public GameObject heldFolderObject;
        public enum FolderState
        {
            receiving, sending, inactive
        }
        public FolderState folderState = FolderState.inactive;

        [SerializeField]
        private List<BirdTag> holdingHands = new List<BirdTag>();

        [SerializeField]
        private LayerMask cabinetAreaMask;
        [SerializeField]
        private LayerMask holdableAreaMask;
        [SerializeField]
        private PauseMenu pauseMenu;
        private Dictionary<BirdName, GameObject> holdingHandMap = new Dictionary<BirdName, GameObject>();

        private float timeSinceLastMove;
        private Vector3 screenOrigin;
        private CabinetDrawer previouslyHoveredCabinet;

        // Start is called before the first frame update
        void Start()
        {
            foreach (BirdTag holdingHand in holdingHands)
            {
                holdingHandMap.Add(holdingHand.birdName, holdingHand.gameObject);
            }
            screenOrigin = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
            timeSinceLastMove = 0.0f;
        }

        public void Initialize()
        {
            birdName = SettingsManager.Instance.birdName;
            GameManager.Instance.playerFlowManager.drawingRound.GetBirdArm(birdName).gameObject.SetActive(false);
            sprite.sprite = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].armSprite;
            outlineSprite.sprite = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].armOutlineSprite;
            gameObject.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (!NetworkClient.isConnected || (pauseMenu && pauseMenu.isOpen))
            {
                return;
            }
            HandleHover();
            HandleClick();
            MoveArm();
        }

        private void HandleHover()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100.0f, cabinetLayer);
            if (hit)
            {
                CabinetDrawer hitCabinet = hit.collider.gameObject.GetComponent<CabinetDrawer>();

                if (hitCabinet)
                {
                    if (previouslyHoveredCabinet != hitCabinet && previouslyHoveredCabinet != null)
                    {
                        previouslyHoveredCabinet.unhover();
                    }
                    if (hitCabinet != previouslyHoveredCabinet)
                    {
                        hitCabinet.hover();
                        previouslyHoveredCabinet = hitCabinet;
                    }


                    return;
                }
                else if (previouslyHoveredCabinet != null)
                {
                    previouslyHoveredCabinet.unhover();
                    previouslyHoveredCabinet = null;
                }
            }
            else
            {
                if (previouslyHoveredCabinet != null)
                {
                    previouslyHoveredCabinet.unhover();
                    previouslyHoveredCabinet = null;
                }
            }
        }

        private void HandleClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100.0f, cabinetLayer);
                if (hit)
                {
                    CabinetDrawer hitCabinet = hit.collider.gameObject.GetComponentInParent<CabinetDrawer>();

                    if (hitCabinet)
                    {
                        hitCabinet.select();
                        return;
                    }
                }
                hit = Physics2D.Raycast(ray.origin, ray.direction, 100.0f, casePileLayer);
                if (hit)
                {
                    CasePile pileOfFiles = hit.collider.gameObject.GetComponentInParent<CasePile>();
                    if(pileOfFiles)
                    {
                        pileOfFiles.Select();
                        return;
                    }
                }
            }
        }

        private void MoveArm()
        {
            Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (temp.x < screenOrigin.x)
            {
                temp.x = screenOrigin.x;
            }
            Vector3 newPosition = new Vector3(temp.x, temp.y, transform.position.z);
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, 50, cabinetAreaMask))
            {
                StatTracker.Instance.totalDistanceMoved += Vector3.Magnitude(newPosition - transform.position);
            }

            transform.position = newPosition;

            timeSinceLastMove += Time.deltaTime;

            if (timeSinceLastMove > moveDelay)
            {
                timeSinceLastMove = 0.0f;
                if(GameManager.Instance.playerFlowManager.serverIsReady && NetworkClient.ready)
                {
                    GameManager.Instance.gameDataHandler.CmdDrawingArmPosition(birdName, transform.position);
                }
                
            }
        }
    }
}