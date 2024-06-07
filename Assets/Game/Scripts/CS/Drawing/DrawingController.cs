using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class DrawingController : MonoBehaviour
    {

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

        private Drawing currentDrawing = null;

        private bool isInitialized = false;

        private int currentToolIndex = 0;
        [SerializeField]
        private List<DrawingToolType> usableToolTypes = new List<DrawingToolType>();
        [SerializeField]
        private Slider sizeSlider;
        [SerializeField]
        private List<BirdTag> taggedObjects;
        [SerializeField]
        private Transform drawingOriginTransform;
        private void Start()
        {
            if (!isInitialized)
            {
                initialize();
            }
        }

        public enum DrawingToolType
        {
            pencil, colour_marker, light_marker, eraser, square_stamp, circle_stamp, invalid
        }
        private DrawingToolType currentDrawingToolType = DrawingToolType.pencil;

        [SerializeField]
        private PencilTool pencilTool;

        [SerializeField]
        private MarkerTool colourMarkerTool;

        [SerializeField]
        private MarkerTool lightMarkerTool;

        [SerializeField]
        private EraserTool eraserTool;

        [SerializeField]
        private SquareStampTool squareStampTool;
        [SerializeField]
        private CircleStampTool circleStampTool;
        [SerializeField]
        private WhiteOut whiteOut;
        [SerializeField]
        private TrashCan trashCan;
        [SerializeField]
        private List<HUDDrawingColourBtn> drawingColourButtons = new List<HUDDrawingColourBtn>();
        [SerializeField]
        private GameObject drawingButtonsObject;
        [SerializeField]
        private GameObject sizeButtonsObject;
        [SerializeField]
        private GameObject sliderKnobObject;
        [SerializeField]
        private PauseMenu pauseMenu;

        private List<DrawingAction> playerDrawingActions = new List<DrawingAction>();
        private Dictionary<DrawingToolType, DrawingTool> drawingToolMap = new Dictionary<DrawingToolType, DrawingTool>();

        public static bool isMouseInDrawingArea = false;
        public static bool isLeftMouseHeld = false;
        public static bool isLeftMouseDown = false;
        public static bool isLeftMouseUp = false;
        public static float zBufferValue = 0.001f;

        void Update()
        {
            if (pauseMenu != null && !pauseMenu.isOpen)
            {
                drawingUpdate();
            }

        }

        public void open()
        {
            if (currentDrawingToolType == DrawingToolType.eraser)
            {
                setMarkerAsToolWithoutSound();
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
                DrawingAction action = drawingToolMap[currentDrawingToolType].drawingUpdate();
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
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                setCurrentDrawingToolType(DrawingToolType.light_marker);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                setCurrentDrawingToolType(DrawingToolType.eraser);
            }
            else if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
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

            if (drawingToolMap.ContainsKey(currentDrawingToolType))
            {
                drawingToolMap[currentDrawingToolType].release();
            }
            currentToolIndex = usableToolTypes.IndexOf(newDrawingToolType);
            currentDrawingToolType = newDrawingToolType;
            if (drawingToolMap.ContainsKey(currentDrawingToolType))
            {
                DrawingTool currentDrawingTool = drawingToolMap[currentDrawingToolType];
                currentDrawingTool.use();
                sizeSlider.SetValueWithoutNotify(currentDrawingTool.currentSizeRatio);

            }
        }

        public void setMarkerAsToolWithoutSound()
        {
            if (DrawingToolType.colour_marker == currentDrawingToolType) return;

            if (drawingToolMap.ContainsKey(currentDrawingToolType))
            {
                drawingToolMap[currentDrawingToolType].release();
            }
            currentToolIndex = usableToolTypes.IndexOf(DrawingToolType.colour_marker);
            currentDrawingToolType = DrawingToolType.colour_marker;
            if (drawingToolMap.ContainsKey(currentDrawingToolType))
            {
                colourMarkerTool.useWithoutSound();
                sizeSlider.SetValueWithoutNotify(colourMarkerTool.currentSizeRatio);
            }
        }

        public void setCurrentDrawingToolSize(float sizeChangeDelta)
        {
            DrawingTool currentDrawingTool = drawingToolMap[currentDrawingToolType];
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

        private void selectCurrentDrawingColourButton(DrawingLineData.LineColour inDrawingColourType)
        {
            foreach (HUDDrawingColourBtn colourButton in drawingColourButtons)
            {
                if (colourButton.colour == inDrawingColourType)
                {
                    colourButton.Select();
                }
            }
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
                drawingToolMap.Add(pencilTool.type, pencilTool);
                drawingToolMap.Add(colourMarkerTool.type, colourMarkerTool);
                drawingToolMap.Add(lightMarkerTool.type, lightMarkerTool);
                drawingToolMap.Add(eraserTool.type, eraserTool);
                colourMarkerTool.setColour();
                lightMarkerTool.setColour();
                currentDrawingToolType = DrawingToolType.colour_marker;
                colourMarkerTool.setCursorSize();
                colourMarkerTool.turnOnGlow();
                colourMarkerTool.useWithoutSound();
                sizeSlider.SetValueWithoutNotify(colourMarkerTool.currentSizeRatio);
                isInitialized = true;
            }
        }

        public void ShowColourButtons()
        {
            drawingButtonsObject.SetActive(true);
        }
        public void HideColourButtons()
        {
            drawingButtonsObject.SetActive(false);
        }
        public void ShowSizeButtons()
        {
            sizeButtonsObject.SetActive(true);
        }
        public void HideSizeButtons()
        {
            sizeButtonsObject.SetActive(false);
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
                pencilTool.saveCurrentLine();
            }
            else if (currentDrawingToolType == DrawingToolType.colour_marker)
            {
                colourMarkerTool.saveCurrentLine();
            }
            else if (currentDrawingToolType == DrawingToolType.light_marker)
            {
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

        public void deselectColourButtons()
        {
            foreach (HUDDrawingColourBtn drawingColourButton in drawingColourButtons)
            {
                drawingColourButton.Deselect();
            }
        }

        public void onDrawingSizeSliderChange(float value)
        {
            drawingToolMap[currentDrawingToolType].setSize(sizeSlider.value);

        }


    }
}