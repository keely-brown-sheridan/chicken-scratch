using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class DrawingController : MonoBehaviour
    {
        public static bool isMouseInDrawingArea = false;
        public static bool isLeftMouseHeld = false;
        public static bool isLeftMouseDown = false;
        public static bool isLeftMouseUp = false;
        public static float zBufferValue = 0.001f;

        public LayerMask drawingLayer;
        public GameObject drawingObjectPrefab;
        public List<GameObject> drawingLines = new List<GameObject>();
        public List<DrawingSquare> drawingSquares = new List<DrawingSquare>();
        public bool canDraw = true;
        public Transform maxSliderPositionTransform;
        public Transform minSliderPositionTransform;
        public float minMouseX;
        public float maxMouseX;

        public static int currentSortingOrder = 1;

        [SerializeField]
        private List<DrawingToolHand> allDrawingToolHands = new List<DrawingToolHand>();

        [SerializeField]
        private GameObject baseDrawingBox;
        [SerializeField]
        private List<DrawingBoxType> drawingBoxes;

        private int currentToolIndex = 0;
        [SerializeField]
        private List<DrawingToolType> usableToolTypes = new List<DrawingToolType>();

        [SerializeField]
        private Slider sizeSlider;

        [SerializeField]
        private List<BirdTag> taggedObjects;

        [SerializeField]
        private Transform drawingOriginTransform;

        [SerializeField]
        private List<Transform> drawingToolPouchSlots;
        [SerializeField]
        private Transform drawingToolPouchParent;
        [SerializeField]
        private Transform heldDrawingToolParent;

        [SerializeField]
        private List<DrawingToolData> possibleDrawingTools;
        [SerializeField]
        private WhiteOut whiteOut;
        [SerializeField]
        private TrashCan trashCan;

        [SerializeField]
        private GameObject sliderKnobObject;
        [SerializeField]
        private PauseMenu pauseMenu;

        public enum DrawingToolType
        {
            pencil, colour_marker, light_marker, eraser, square_stamp, circle_stamp, highlighter, invalid
        }
        private DrawingToolType currentDrawingToolType = DrawingToolType.pencil;



        private List<DrawingAction> playerDrawingActions = new List<DrawingAction>();

        private Dictionary<DrawingToolType, DrawingTool> activeDrawingToolMap = new Dictionary<DrawingToolType, DrawingTool>();
        private Dictionary<DrawingToolType, DrawingToolData> possibleDrawingToolMap = new Dictionary<DrawingToolType, DrawingToolData>();
        private Dictionary<TaskData.TaskModifier, GameObject> drawingBoxMap = new Dictionary<TaskData.TaskModifier, GameObject>();
        private Drawing currentDrawing = null;
        private TaskData.TaskModifier boxType;

        private bool isInitialized = false;

        private void OnEnable()
        {
            initialize();
            //test();
            open();
        }

        private void test()
        {
            StoreItemData storeItem = new MarkerStoreItemData() { itemType = StoreItem.StoreItemType.highlighter, markerColour = new Color(1f, 1f, 0f, 0.4f) };
            GameManager.Instance.playerFlowManager.AddStoreItem(storeItem);
        }

        public void initialize()
        {
            if (!isInitialized)
            {
                foreach (BirdTag birdTag in taggedObjects)
                {
                    if (birdTag.birdName == SettingsManager.Instance.birdName)
                    {
                        birdTag.gameObject.SetActive(true);
                    }
                }
                foreach (DrawingToolHand currentHand in allDrawingToolHands)
                {
                    if (currentHand.GetComponent<BirdTag>().birdName == SettingsManager.Instance.birdName)
                    {
                        currentHand.gameObject.SetActive(true);
                    }
                }
                foreach (DrawingBoxType drawingBox in drawingBoxes)
                {
                    drawingBoxMap.Add(drawingBox.modifier, drawingBox.gameObject);
                }
                foreach(DrawingToolData possibleTool in possibleDrawingTools)
                {
                    possibleDrawingToolMap.Add(possibleTool.toolType, possibleTool);
                }
                currentDrawingToolType = DrawingToolType.colour_marker;
                isInitialized = true;
            }
        }

        public void SetDrawingBoxType(TaskData.TaskModifier inBoxType)
        {
            boxType = inBoxType;
        }

        void Update()
        {
            if (pauseMenu != null && !pauseMenu.isOpen)
            {
                drawingUpdate();
            }

        }

        private void GenerateTools()
        {
            ClearTools();

            int currentDrawingToolPouchSlot = 0;
            if(usableToolTypes.Contains(DrawingToolType.pencil))
            {
                GenerateTool(DrawingToolType.pencil, currentDrawingToolPouchSlot);
                currentDrawingToolPouchSlot++;
            }
            if (usableToolTypes.Contains(DrawingToolType.colour_marker))
            {
                GenerateTool(DrawingToolType.colour_marker, currentDrawingToolPouchSlot);
                currentDrawingToolPouchSlot++;
            }

            if (!usableToolTypes.Contains(DrawingToolType.light_marker) && GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.marker))
            {
                usableToolTypes.Add(DrawingToolType.light_marker);
            }
            if(usableToolTypes.Contains(DrawingToolType.light_marker))
            {
                GenerateTool(DrawingToolType.light_marker, currentDrawingToolPouchSlot);
                currentDrawingToolPouchSlot++;
            }

            if (!usableToolTypes.Contains(DrawingToolType.highlighter) && GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.highlighter))
            {
                usableToolTypes.Add(DrawingToolType.highlighter);
            }
            if (usableToolTypes.Contains(DrawingToolType.highlighter))
            {
                GenerateTool(DrawingToolType.highlighter, currentDrawingToolPouchSlot);
                currentDrawingToolPouchSlot++;
            }

            if (!usableToolTypes.Contains(DrawingToolType.eraser) && GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.eraser))
            {
                usableToolTypes.Add(DrawingToolType.eraser);
            }
            if (usableToolTypes.Contains(DrawingToolType.eraser))
            {
                GenerateTool(DrawingToolType.eraser, currentDrawingToolPouchSlot);
                currentDrawingToolPouchSlot++;
            }

            if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.white_out))
            {
                whiteOut.gameObject.SetActive(true);
            }
        }

        private void GenerateTool(DrawingToolType toolType, int pouchSlot)
        {
            if(!possibleDrawingToolMap.ContainsKey(toolType))
            {
                Debug.LogError("Cannot generate tool["+toolType.ToString()+"] because it does not exist in the possible drawing tool map["+possibleDrawingToolMap.Count.ToString()+"].");
                return;
            }

            //Create the tool
            DrawingToolData selectedDrawingTool = possibleDrawingToolMap[toolType];
            GameObject pouchToolVisualObject = Instantiate(selectedDrawingTool.toolPrefab, drawingToolPouchParent);
            GameObject heldToolVisualObject = Instantiate(selectedDrawingTool.heldPrefab, heldDrawingToolParent);
            DrawingTool newTool = pouchToolVisualObject.GetComponent<DrawingTool>();
            HeldDrawingTool heldTool = heldToolVisualObject.GetComponent<HeldDrawingTool>();
            newTool.SetPouchVisualsPosition(drawingToolPouchSlots[pouchSlot].position);
            //Add to the drawing tool map
            activeDrawingToolMap.Add(toolType, newTool);
            heldTool.Initialize(newTool);
            

            Color toolColour;
            switch(toolType)
            {
                case DrawingToolType.colour_marker:
                    //Initialize the colour
                    MarkerTool colourMarkerTool = (MarkerTool)newTool;
                    toolColour = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].colour;
                    colourMarkerTool.setColour(toolColour);
                    heldTool.SetColour(toolColour);
                    break;
                case DrawingToolType.light_marker:
                    //Initialize the colour
                    toolColour = GameManager.Instance.playerFlowManager.GetStoreItemMarkerColour(StoreItem.StoreItemType.marker);
                    MarkerTool lightMarkerTool = (MarkerTool)newTool;
                    lightMarkerTool.setColour(toolColour);
                    heldTool.SetColour(toolColour);
                    break;
                case DrawingToolType.highlighter:
                    //Initialize the colour
                    toolColour = GameManager.Instance.playerFlowManager.GetStoreItemMarkerColour(StoreItem.StoreItemType.highlighter);
                    MarkerTool highlighterTool = (MarkerTool)newTool;
                    highlighterTool.setColour(toolColour);
                    heldTool.SetColour(toolColour);
                    break;
            }
            newTool.gameObject.SetActive(true);
            //usableToolTypes.Add(toolType);
        }

        private void ClearTools()
        {
            List<Transform> transformsToClear = new List<Transform>();
            foreach(Transform child in drawingToolPouchParent)
            {
                transformsToClear.Add(child);
            }
            foreach(Transform child in heldDrawingToolParent)
            {
                transformsToClear.Add(child);
            }
            for(int i = transformsToClear.Count -1; i >= 0; i--)
            {
                Destroy(transformsToClear[i].gameObject);
            }
            activeDrawingToolMap.Clear();
        }

        public void open()
        {
            GenerateTools();
            if (activeDrawingToolMap.ContainsKey(DrawingToolType.colour_marker))
            {
                if (currentDrawingToolType == DrawingToolType.eraser || currentDrawingToolType == DrawingToolType.colour_marker)
                {
                    MarkerTool colourMarkerTool = (MarkerTool)activeDrawingToolMap[DrawingToolType.colour_marker];
                    currentToolIndex = usableToolTypes.IndexOf(currentDrawingToolType);
                    colourMarkerTool.setCursorSize();
                    colourMarkerTool.turnOnGlow();
                    setMarkerAsToolWithoutSound();
                    sizeSlider.SetValueWithoutNotify(colourMarkerTool.currentSizeRatio);
                }
                else
                {
                    activeDrawingToolMap[usableToolTypes[currentToolIndex]].use();
                }
            }
            else
            {
                Debug.LogError("Tools have been generated in initialization but colour marker hasn't been added to the active drawing tool map.");
            }


            foreach(DrawingBoxType drawingBox in drawingBoxes)
            {
                drawingBox.gameObject.SetActive(false);
            }

            if (drawingBoxMap.ContainsKey(boxType))
            {
                drawingBoxMap[boxType].SetActive(true);
            }
            else
            {
                baseDrawingBox.SetActive(true);
            }
        }

        void drawingUpdate()
        {

            Vector2 temp = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100.0f, drawingLayer);
            isMouseInDrawingArea = hit;
            isLeftMouseHeld = Input.GetMouseButton(0);
            isLeftMouseDown = Input.GetMouseButtonDown(0);
            isLeftMouseUp = Input.GetMouseButtonUp(0);

            handleHotkeys();
            if (Input.GetMouseButtonDown(1))
            {
                //Switch current tool
                currentToolIndex++;
                if (currentToolIndex >= usableToolTypes.Count)
                {
                    currentToolIndex = 0;
                }
                setCurrentDrawingToolType(usableToolTypes[currentToolIndex]);
            }
            if (Input.mouseScrollDelta.magnitude > 0)
            {
                //Adjust the size of the current tool
                setCurrentDrawingToolSize(Input.mouseScrollDelta.y);
            }
            if (canDraw)
            {
                //Update the currently active tool
                DrawingAction action = activeDrawingToolMap[currentDrawingToolType].drawingUpdate();
                if (action != null)
                {
                    playerDrawingActions.Add(action);
                }
            }
        }

        private void handleHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                setCurrentDrawingToolType(DrawingToolType.pencil);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                setCurrentDrawingToolType(DrawingToolType.colour_marker);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.marker))
            {
                setCurrentDrawingToolType(DrawingToolType.light_marker);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) && GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.eraser))
            {
                setCurrentDrawingToolType(DrawingToolType.eraser);
            }
            else if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl) && GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.white_out))
            {
                whiteOut.undo();
            }
            else if (Input.GetKeyDown(KeyCode.Delete))
            {
                trashCan.showPromptToClearDrawing();
            }

        }

        public void setCurrentDrawingToolType(string drawingToolName)
        {
            DrawingToolType drawingToolType = DrawingToolType.invalid;
            if (Enum.TryParse<DrawingToolType>(drawingToolName, out drawingToolType))
            {
                setCurrentDrawingToolType(drawingToolType);
            }
        }

        public void setCurrentDrawingToolType(DrawingToolType newDrawingToolType)
        {
            if (newDrawingToolType == currentDrawingToolType) return;

            if (activeDrawingToolMap.ContainsKey(currentDrawingToolType))
            {
                activeDrawingToolMap[currentDrawingToolType].release();
            }
            currentToolIndex = usableToolTypes.IndexOf(newDrawingToolType);
            currentDrawingToolType = newDrawingToolType;
            if (activeDrawingToolMap.ContainsKey(currentDrawingToolType))
            {
                DrawingTool currentDrawingTool = activeDrawingToolMap[currentDrawingToolType];
                currentDrawingTool.use();
                sizeSlider.SetValueWithoutNotify(currentDrawingTool.currentSizeRatio);

            }
        }

        public void setMarkerAsToolWithoutSound()
        {
            if (activeDrawingToolMap.ContainsKey(currentDrawingToolType))
            {
                activeDrawingToolMap[currentDrawingToolType].release();
            }
            currentToolIndex = usableToolTypes.IndexOf(DrawingToolType.colour_marker);
            currentDrawingToolType = DrawingToolType.colour_marker;
            if (activeDrawingToolMap.ContainsKey(currentDrawingToolType))
            {
                MarkerTool colourMarkerTool = (MarkerTool)activeDrawingToolMap[DrawingToolType.colour_marker];
                colourMarkerTool.useWithoutSound();
                sizeSlider.SetValueWithoutNotify(colourMarkerTool.currentSizeRatio);
            }
        }

        public void setCurrentDrawingToolSize(float sizeChangeDelta)
        {
            DrawingTool currentDrawingTool = activeDrawingToolMap[currentDrawingToolType];
            currentDrawingTool.changeSize(sizeChangeDelta);
            sizeSlider.SetValueWithoutNotify(currentDrawingTool.currentSizeRatio);
            //updateHUDDrawingSize(currentDrawingTool.currentSizeRatio, currentDrawingTool.getMaxSizeIndex());
        }

        public void setSliderKnobPosition(float xPosition)
        {

        }

        public void AddDrawingLines(DrawingData inDrawingData)
        {
            if(!currentDrawing)
            {
                StartNewDrawing();
            }
            List<GameObject> newLines = currentDrawing.CreateDrawingsFromVisuals(inDrawingData.visuals, drawingOriginTransform.position, 1f);

            drawingLines.AddRange(newLines);
            currentSortingOrder += inDrawingData.visuals.Count;
        }

        private void updateHUDDrawingSize(float ratio, int maxSizeIndex)
        {
            float differenceBetweenMaxAndMin = maxSliderPositionTransform.localPosition.x - minSliderPositionTransform.localPosition.x;
            float xPosition = differenceBetweenMaxAndMin * (ratio) + minSliderPositionTransform.localPosition.x;
            sliderKnobObject.transform.localPosition = new Vector3(xPosition, sliderKnobObject.transform.localPosition.y, sliderKnobObject.transform.localPosition.z);
        }

        public void undoLastDrawingAction()
        {
            if (playerDrawingActions.Count == 0) return;
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_white_out");
            int lastDrawingActionIndex = playerDrawingActions.Count - 1;

            if (playerDrawingActions[lastDrawingActionIndex] is AddLineDrawingAction)
            {
                //Delete the lines
                AddLineDrawingAction addLineDrawingAction = (AddLineDrawingAction)playerDrawingActions[lastDrawingActionIndex];
                if (drawingLines.Contains(addLineDrawingAction.drawingLineObject))
                {
                    drawingLines.Remove(addLineDrawingAction.drawingLineObject);
                    Destroy(addLineDrawingAction.drawingLineObject);
                }
            }
            else if (playerDrawingActions[lastDrawingActionIndex] is RemoveLineDrawingAction)
            {
                //Add the lines back in
                RemoveLineDrawingAction removeLineAction = (RemoveLineDrawingAction)playerDrawingActions[lastDrawingActionIndex];
                drawingLines.Add(removeLineAction.drawingLineObject);
                removeLineAction.drawingLineObject.SetActive(true);
            }
            else if (playerDrawingActions[lastDrawingActionIndex] is AddSquareDrawingAction)
            {
                //Delete the square
                AddSquareDrawingAction addSquareAction = (AddSquareDrawingAction)playerDrawingActions[lastDrawingActionIndex];
                if (drawingSquares.Contains(addSquareAction.drawingSquareData))
                {
                    drawingSquares.Remove(addSquareAction.drawingSquareData);
                }
                Destroy(addSquareAction.drawingSquareData.gameObject);
            }
            else if (playerDrawingActions[lastDrawingActionIndex] is RemoveSquareDrawingAction)
            {
                //Add the square back in
                RemoveSquareDrawingAction removeSquareAction = (RemoveSquareDrawingAction)playerDrawingActions[lastDrawingActionIndex];
                drawingSquares.Add(removeSquareAction.drawingSquareData);
                removeSquareAction.drawingSquareData.gameObject.SetActive(true);
            }
            playerDrawingActions.RemoveAt(lastDrawingActionIndex);
        }

        public void addDrawingAction(DrawingAction inAction)
        {
            playerDrawingActions.Add(inAction);
        }

        void StartNewDrawing()
        {
            currentSortingOrder = 1;
            GameObject newDrawingObject = Instantiate(drawingObjectPrefab, Vector3.zero, Quaternion.identity);
            newDrawingObject.transform.SetParent(transform);
            currentDrawing = newDrawingObject.GetComponent<Drawing>();
        }

        public List<DrawingLineData> getDrawingVisuals()
        {
            if (currentDrawingToolType == DrawingToolType.pencil)
            {
                PencilTool pencilTool = (PencilTool)activeDrawingToolMap[DrawingToolType.pencil];
                pencilTool.saveCurrentLine();
            }
            else if (currentDrawingToolType == DrawingToolType.colour_marker)
            {
                MarkerTool colourMarkerTool = (MarkerTool)activeDrawingToolMap[DrawingToolType.colour_marker];
                colourMarkerTool.saveCurrentLine();
            }
            else if (currentDrawingToolType == DrawingToolType.light_marker)
            {
                MarkerTool lightMarkerTool = (MarkerTool)activeDrawingToolMap[DrawingToolType.light_marker];
                lightMarkerTool.saveCurrentLine();
            }

            currentDrawing = new Drawing();
            foreach (GameObject lineObject in drawingLines)
            {
                if (lineObject == null) Debug.LogError("Failing to add line object from drawingLines, it is null.");
                currentDrawing.AddShape(lineObject);
            }
            foreach (DrawingSquare drawingSquare in drawingSquares)
            {
                foreach (GameObject lineObject in drawingSquare.lineObjects)
                {
                    currentDrawing.AddShape(lineObject);
                }
            }
            Vector3 worldPosition = new Vector3(drawingOriginTransform.position.x, drawingOriginTransform.position.y, 0f);
            return currentDrawing.GetVisualsFromDrawing(worldPosition);
        }

        public void clearVisualsFromButton()
        {
            AudioManager.Instance.PlaySound("trash");
            clearVisuals();
        }

        public void clearVisuals()
        {
            if (currentDrawing != null)
            {
                currentDrawing.Clear();
            }

            for (int i = drawingLines.Count - 1; i >= 0; i--)
            {
                Destroy(drawingLines[i].gameObject);
            }
            for (int i = drawingSquares.Count - 1; i >= 0; i--)
            {
                for (int j = drawingSquares[i].lineObjects.Count - 1; j >= 0; j--)
                {
                    Destroy(drawingSquares[i].lineObjects[j]);
                }
                Destroy(drawingSquares[i].gameObject);
            }

            drawingLines.Clear();
            drawingSquares.Clear();
            playerDrawingActions.Clear();
            currentSortingOrder = 1;
        }

        public bool hasVisuals()
        {
            return (drawingLines.Count > 0 || drawingSquares.Count > 0);
        }

        public void onDrawingSizeSliderChange(float value)
        {
            activeDrawingToolMap[currentDrawingToolType].setSize(sizeSlider.value);
        }


    }
}