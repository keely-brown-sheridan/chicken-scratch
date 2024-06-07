using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PencilTool : DrawingTool
    {
        public bool isActive => _isActive;
        private bool _isActive;

        private Material _currentLineMaterial;

        [SerializeField]
        private List<SpriteRenderer> colouredRenderers;

        [SerializeField]
        private Transform toolVisualsHolder;

        [SerializeField]
        private float selectionTranslationValue = 0.15f;

        [SerializeField]
        private string selectionSoundName = "";
        [SerializeField]
        private float timeForDotClick = 0.25f;
        [SerializeField]
        private GameObject drawingLinePrefab;
        [SerializeField]
        private GameObject cursorDrawingGhost;
        [SerializeField]
        private float maxSize = 0.5f;
        [SerializeField]
        private float minSize = 0.05f;
        [SerializeField]
        private float sizeAdjustmentIncrement = 0.05f;

        [SerializeField]
        private float pencilLineZOffset = -2;
        [SerializeField]
        private List<GameObject> objectsToShowOnSelect = new List<GameObject>();
        [SerializeField]
        private List<GameObject> objectsToHideOnSelect = new List<GameObject>();

        [SerializeField]
        private List<DrawingToolHand> allDrawingToolHands = new List<DrawingToolHand>();

        private DrawingToolHand drawingToolHand;
        [SerializeField]
        private Transform holdingPositionTransform;
        private GameObject _currentLineObject;
        private List<Vector2> _fingerPositions = new List<Vector2>();
        private LineRenderer _lineRenderer;
        private float _timeSinceLineStarted = 0.0f;
        private bool _startNewLine = true;
        [SerializeField]
        private float _currentDrawingSize = 0.05f;
        [SerializeField]
        private float ghostCurrentSizeRatio = 0.5f;
        [SerializeField]
        private float jaggednessAngleThreshold;
        [SerializeField]
        private float jaggednessDistanceThreshold;

        private int _globalSortingOrder;

        private List<DrawingLineData.LineColour> _lineColourOptions = new List<DrawingLineData.LineColour>() { DrawingLineData.LineColour.Base };
        private int _currentLineColourIndex = 0;

        private void Start()
        {
            foreach (DrawingToolHand currentHand in allDrawingToolHands)
            {
                if (currentHand.GetComponent<BirdTag>().birdName == SettingsManager.Instance.birdName)
                {
                    drawingToolHand = currentHand;
                }
            }
        }

        public override DrawingAction drawingUpdate()
        {
            bool requiresNewPencilLine = _lineRenderer == null;
            _timeSinceLineStarted += Time.deltaTime;

            if (Input.GetMouseButtonUp(0))
            {
                _startNewLine = true;
                if (_timeSinceLineStarted < timeForDotClick)
                {
                    return createDot();
                }
                if (_lineRenderer != null)
                {
                    StatTracker.Instance.drawingToolUsageMap[DrawingController.DrawingToolType.pencil] = true;
                    AddLineDrawingAction addLineDrawingAction = new AddLineDrawingAction();
                    addLineDrawingAction.drawingLineObject = _lineRenderer.gameObject;
                    controller.drawingLines.Add(_lineRenderer.gameObject);
                    _lineRenderer = null;
                    return addLineDrawingAction;
                }
                return null;
            }

            if (DrawingController.isMouseInDrawingArea)
            {

                Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                cursorDrawingGhost.transform.position = new Vector3(temp.x, temp.y, transform.position.z);
                cursorDrawingGhost.SetActive(true);
                if (_startNewLine)
                {
                    if (DrawingController.isLeftMouseDown || DrawingController.isLeftMouseHeld)
                    {
                        _timeSinceLineStarted = 0.0f;
                        _startNewLine = false;
                        createLine();
                        return null;

                    }
                }
                else
                {
                    if (DrawingController.isLeftMouseHeld)
                    {
                        if (_lineRenderer == null)
                        {
                            _startNewLine = false;
                            _timeSinceLineStarted = 0.0f;
                            createLine();
                        }
                        else
                        {
                            Vector2 tempFingerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            updateLine(tempFingerPos);
                        }
                    }
                }
            }
            else
            {
                cursorDrawingGhost.SetActive(false);
                if (DrawingController.isLeftMouseHeld)
                {
                    if (_lineRenderer != null)
                    {
                        StatTracker.Instance.drawingToolUsageMap[DrawingController.DrawingToolType.pencil] = true;
                        AddLineDrawingAction addLineDrawingAction = new AddLineDrawingAction();
                        addLineDrawingAction.drawingLineObject = _lineRenderer.gameObject;
                        controller.drawingLines.Add(_lineRenderer.gameObject);
                        _lineRenderer = null;
                        return addLineDrawingAction;
                    }

                    _startNewLine = true;
                }
            }
            return null;
        }

        void createLine()
        {
            _currentLineObject = Instantiate(drawingLinePrefab, Vector3.zero, Quaternion.identity, controller.transform);
            DrawingLineData lineData = _currentLineObject.GetComponent<DrawingLine>().drawingLineData;
            lineData.lineSize = _currentDrawingSize;
            lineData.author = SettingsManager.Instance.birdName;
            DrawingLineData.LineColour lineColour = DrawingLineData.LineColour.Invalid;
            Material lineMaterial = null;
            lineColour = _lineColourOptions[_currentLineColourIndex];
            lineMaterial = _currentLineMaterial;

            //Debug.LogError("Current line colour["+ lineData.lineColour.ToString() + "].");
            lineData.lineColour = lineColour;

            if (lineData.lineColour == DrawingLineData.LineColour.Invalid)
            {
                Debug.LogError("Invalid line colour name[" + lineColour.ToString() + "] set for creating foreground line.");
            }

            _currentLineObject.transform.position = new Vector3(0, 0, pencilLineZOffset);
            _lineRenderer = _currentLineObject.GetComponent<LineRenderer>();
            _lineRenderer.startWidth = _currentDrawingSize;
            _lineRenderer.endWidth = _currentDrawingSize;
            _lineRenderer.material = lineMaterial;
            lineData.zDepth = pencilLineZOffset;
            DrawingController.currentSortingOrder++;

            _fingerPositions.Clear();
            _fingerPositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            _fingerPositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Vector3 temp = new Vector3(_fingerPositions[0].x, _fingerPositions[0].y, pencilLineZOffset - DrawingController.zBufferValue * DrawingController.currentSortingOrder);
            _lineRenderer.SetPosition(0, temp);
            temp = new Vector3(_fingerPositions[1].x, _fingerPositions[1].y, pencilLineZOffset - DrawingController.zBufferValue * DrawingController.currentSortingOrder);
            _lineRenderer.SetPosition(1, temp);

        }

        private void updateLine(Vector2 newFingerPos)
        {
            if (!_lineRenderer)
            {
                createLine();
                return;
            }

            Vector3 temp = new Vector3(newFingerPos.x, newFingerPos.y, pencilLineZOffset - DrawingController.zBufferValue * DrawingController.currentSortingOrder);
            Vector3 lastPosition = _lineRenderer.GetPosition(_lineRenderer.positionCount - 1);
            if (temp == lastPosition) return;
            _fingerPositions.Add(newFingerPos);

            _lineRenderer.positionCount++;

            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, temp);
            //Check if the direction change from the last three points would warrant smoothing
            if (IsLineJaggednessDetected())
            {
                //Start a new line from the new point to prevent jaggedness
                Vector3 newStartPoint = _lineRenderer.GetPosition(_lineRenderer.positionCount - 2);
                _lineRenderer.positionCount--;

                AddLineDrawingAction addLineDrawingAction = new AddLineDrawingAction();
                addLineDrawingAction.drawingLineObject = _lineRenderer.gameObject;
                controller.drawingLines.Add(_lineRenderer.gameObject);
                _lineRenderer = null;
                controller.addDrawingAction(addLineDrawingAction);

                createLine();
                _lineRenderer.SetPosition(0, newStartPoint);
                _lineRenderer.SetPosition(1, lastPosition);
            }
        }

        private bool IsLineJaggednessDetected()
        {
            if (_lineRenderer.positionCount < 3)
            {
                return false;
            }

            Vector2 point1 = _lineRenderer.GetPosition(_lineRenderer.positionCount - 3);
            Vector2 point2 = _lineRenderer.GetPosition(_lineRenderer.positionCount - 2);
            Vector2 point3 = _lineRenderer.GetPosition(_lineRenderer.positionCount - 1);

            //Calculate the angle between line 1-2 and line 2-3
            Vector2 direction1 = (point2 - point1).normalized;
            Vector2 direction2 = (point3 - point2).normalized;

            float angle = Vector2.Angle(direction1, direction2);

            float distanceBetweenPoints = Vector2.Distance(point2, point3);
            bool isLineJaggednessDetected = angle > jaggednessAngleThreshold && distanceBetweenPoints > jaggednessDistanceThreshold;
            return isLineJaggednessDetected;
        }

        private DrawingAction createDot()
        {
            if (_lineRenderer == null) return null;
            StatTracker.Instance.drawingToolUsageMap[DrawingController.DrawingToolType.pencil] = true;
            Vector3 lastPosition = _lineRenderer.GetPosition(_lineRenderer.positionCount - 1);
            Vector3 temp = new Vector3(lastPosition.x + 0.0075f, lastPosition.y, pencilLineZOffset - DrawingController.zBufferValue * DrawingController.currentSortingOrder);
            AddLineDrawingAction addLineDrawingAction = new AddLineDrawingAction();
            addLineDrawingAction.drawingLineObject = _lineRenderer.gameObject;
            controller.drawingLines.Add(_lineRenderer.gameObject);
            _lineRenderer = null;
            return addLineDrawingAction;
        }

        public override void use()
        {
            foreach (GameObject objectToShowOnUse in objectsToShowOnSelect)
            {
                objectToShowOnUse.SetActive(true);
            }
            foreach (GameObject objectToHideOnUse in objectsToHideOnSelect)
            {
                objectToHideOnUse.SetActive(false);
            }
            drawingToolHand.SetTargetTransform(holdingPositionTransform);
            _currentLineMaterial = ColourManager.Instance.baseLineMaterial;
            setCursorSize();
            _currentSizeRatio = (_currentDrawingSize - minSize) / (maxSize - minSize);
            AudioManager.Instance.PlaySound(selectionSoundName);
            _isActive = true;
            toolVisualsHolder.position += new Vector3(0, selectionTranslationValue, 0);
            glowOutlineObject.SetActive(true);
            glowFillObject.SetActive(false);
        }
        public void setCursorSize()
        {
            cursorDrawingGhost.transform.localScale = new Vector3(_currentDrawingSize * ghostCurrentSizeRatio, _currentDrawingSize * ghostCurrentSizeRatio, cursorDrawingGhost.transform.localScale.z);
        }

        public override int getSizeIndex()
        {
            return (int)(_currentDrawingSize / sizeAdjustmentIncrement);
        }

        public override int getMaxSizeIndex()
        {
            return (int)((maxSize - minSize) / sizeAdjustmentIncrement) + 1;
        }

        public override void release()
        {
            foreach (GameObject objectToShowOnUse in objectsToShowOnSelect)
            {
                objectToShowOnUse.SetActive(false);
            }
            foreach (GameObject objectToHideOnUse in objectsToHideOnSelect)
            {
                objectToHideOnUse.SetActive(true);
            }
            _isActive = false;
            cursorDrawingGhost.SetActive(false);
            toolVisualsHolder.position -= new Vector3(0, selectionTranslationValue, 0);
            glowOutlineObject.SetActive(false);
        }

        public override void changeSize(float changeDirection)
        {
            if (_lineRenderer != null)
            {
                //For now, don't change the size of the line if you're currently drawing
                return;
            }
            if (changeDirection > 0)
            {
                if (Decimal.Round((Decimal)_currentDrawingSize, 2) >= Decimal.Round((Decimal)maxSize, 2))
                {
                    _currentDrawingSize = maxSize;
                }
                else
                {
                    _currentDrawingSize += sizeAdjustmentIncrement;
                }
            }
            else if (changeDirection < 0)
            {
                if (Decimal.Round((Decimal)_currentDrawingSize, 2) <= Decimal.Round((Decimal)minSize, 2))
                {
                    _currentDrawingSize = minSize;
                }
                else
                {
                    _currentDrawingSize -= sizeAdjustmentIncrement;
                }
            }
            _currentSizeRatio = (_currentDrawingSize - minSize) / (maxSize - minSize);
            setCursorSize();
        }
        public override void setSize(float ratio)
        {
            _currentSizeRatio = ratio;
            _currentDrawingSize = (maxSize - minSize) * _currentSizeRatio + minSize;
            setCursorSize();
        }

        public void turnOnGlow()
        {
            glowOutlineObject.SetActive(true);
        }

        public void saveCurrentLine()
        {
            if (_lineRenderer)
            {
                controller.drawingLines.Add(_lineRenderer.gameObject);
                _lineRenderer = null;
            }
        }
    }
}