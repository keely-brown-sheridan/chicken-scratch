using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class SquareStampTool : ColourDrawingTool
    {

        private bool _isActive = false;

        [SerializeField]
        private float selectionTranslationValue = 0.15f;

        [SerializeField]
        private string selectionSoundName = "";
        [SerializeField]
        private string useSoundName = "";

        private Material _currentLineMaterial;
        private Material _currentGhostLineMaterial;
        [SerializeField]
        private GameObject drawingLinePrefab;
        [SerializeField]
        private List<SpriteRenderer> colouredRenderers;
        [SerializeField]
        private Transform toolVisualsHolder;

        [SerializeField]
        private float maxSize = 0.5f;
        [SerializeField]
        private float minSize = 0.05f;

        public float SizeAdjustmentIncrement
        {
            get
            {
                return sizeAdjustmentIncrement;
            }
        }
        [SerializeField]
        private float sizeAdjustmentIncrement = 0.05f;
        [SerializeField]
        private float initialToolSizeRatio = 0.5f;

        [SerializeField]
        private float stampLineZOffset = -1;

        [SerializeField]
        private float currentDrawingSize = 0.05f;

        [SerializeField]
        private LayerMask drawingAreaLayerMask;

        private List<DrawingLineData.LineColour> _lineColourOptions = new List<DrawingLineData.LineColour>() { DrawingLineData.LineColour.Base, DrawingLineData.LineColour.Colour, DrawingLineData.LineColour.Light, DrawingLineData.LineColour.Erase };
        private int _currentLineColourIndex = 2;

        private int _currentSortingOrder = -1;

        [SerializeField]
        private GameObject squareLinePrefab;
        [SerializeField]
        private float borderLineZOffset = -0.00001f;

        [SerializeField]
        private GameObject cursorStampGhost;

        void Start()
        {
            //createNewSquare(transform.position, 1.0f);
        }

        public override DrawingAction drawingUpdate()
        {
            if (DrawingController.isMouseInDrawingArea)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                bool isSquareAtCursorInBounds = isSquareAtPositionInBounds(mousePosition);
                cursorStampGhost.SetActive(isSquareAtCursorInBounds);
                if (DrawingController.isLeftMouseDown)
                {
                    //Create a square stamp!
                    Vector3 placementPosition = new Vector3(mousePosition.x, mousePosition.y, stampLineZOffset);

                    //Check the four corners to see whether the corners are in bounds
                    if (isSquareAtCursorInBounds)
                    {
                        return createNewSquare(placementPosition, currentDrawingSize);
                    }

                }
            }
            else
            {
                cursorStampGhost.SetActive(false);
            }
            return null;
        }

        private bool isSquareAtPositionInBounds(Vector3 position)
        {
            float squareZDepth = -(_currentSortingOrder + 1) * DrawingController.zBufferValue;
            Vector3 topLeftCorner = new Vector3(-currentDrawingSize + position.x, currentDrawingSize + position.y, squareZDepth);
            Vector3 topRightCorner = new Vector3(currentDrawingSize + position.x, currentDrawingSize + position.y, squareZDepth);
            Vector3 bottomLeftCorner = new Vector3(-currentDrawingSize + position.x, -currentDrawingSize + position.y, squareZDepth);
            Vector3 bottomRightCorner = new Vector3(currentDrawingSize + position.x, -currentDrawingSize + position.y, squareZDepth);
            bool topLeftCornerIsInBounds = isPointInDrawingArea(topLeftCorner);
            bool topRightCornerIsInBounds = isPointInDrawingArea(topRightCorner);
            bool bottomLeftCornerIsInBounds = isPointInDrawingArea(bottomLeftCorner);
            bool bottomRightCornerIsInBounds = isPointInDrawingArea(bottomRightCorner);
            bool cornersAreInBounds = topLeftCornerIsInBounds && topRightCornerIsInBounds && bottomLeftCornerIsInBounds && bottomRightCornerIsInBounds;

            return cornersAreInBounds;
        }

        public override void release()
        {
            _isActive = false;
            toolVisualsHolder.position -= new Vector3(0, selectionTranslationValue, 0);
            cursorStampGhost.SetActive(false);
            glowOutlineObject.SetActive(false);
        }

        public override int getSizeIndex()
        {
            return (int)((currentDrawingSize - minSize) / sizeAdjustmentIncrement) + 1;
        }
        public override int getMaxSizeIndex()
        {
            return (int)((maxSize - minSize) / sizeAdjustmentIncrement) + 1;
        }

        public void updateColour()
        {
            Color currentColourValue = Color.clear;
            DrawingLineData.LineColour currentLineColour = _lineColourOptions[_currentLineColourIndex];
            _currentColourType = currentLineColour;
            Bird currentBird = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName];
            switch (currentLineColour)
            {
                case DrawingLineData.LineColour.Base:
                    currentColourValue = Color.black;
                    _currentLineMaterial = ColourManager.Instance.baseLineMaterial;
                    _currentGhostLineMaterial = ColourManager.Instance.stampBorderGhostMaterial;
                    break;
                case DrawingLineData.LineColour.Colour:
                    currentColourValue = currentBird.colour;
                    _currentLineMaterial = currentBird.material;
                    _currentGhostLineMaterial = currentBird.ghostMaterial;
                    break;
                case DrawingLineData.LineColour.Light:
                    currentColourValue = currentBird.bgColour;
                    _currentLineMaterial = currentBird.bgLineMaterial;
                    _currentGhostLineMaterial = currentBird.ghostBGMaterial;
                    break;
                case DrawingLineData.LineColour.Erase:
                    currentColourValue = Color.white;
                    _currentLineMaterial = ColourManager.Instance.eraseLineMaterial;
                    _currentGhostLineMaterial = ColourManager.Instance.eraseGhostMaterial;
                    break;
            }

            foreach (SpriteRenderer colouredRenderer in colouredRenderers)
            {
                colouredRenderer.color = currentColourValue;
            }
            updateSquareGhost(currentDrawingSize);
        }

        public override void changeSize(float changeDirection)
        {
            if (changeDirection > 0)
            {
                if (Decimal.Round((Decimal)currentDrawingSize, 2) >= Decimal.Round((Decimal)maxSize, 2))
                {
                    currentDrawingSize = maxSize;
                }
                else
                {
                    currentDrawingSize += sizeAdjustmentIncrement;
                }

                updateSquareGhost(currentDrawingSize);
            }
            else if (changeDirection < 0)
            {
                if (Decimal.Round((Decimal)currentDrawingSize, 2) <= Decimal.Round((Decimal)minSize, 2))
                {
                    currentDrawingSize = minSize;
                }
                else
                {
                    currentDrawingSize -= sizeAdjustmentIncrement;
                }
                updateSquareGhost(currentDrawingSize);
            }
            _currentSizeRatio = (currentDrawingSize - minSize) / (maxSize - minSize);
        }

        public override void setSize(float inRatio)
        {
            _currentSizeRatio = inRatio;
            currentDrawingSize = (maxSize - minSize) * _currentSizeRatio + minSize;
            updateSquareGhost(currentDrawingSize);
        }

        public override void use()
        {
            AudioManager.Instance.PlaySound(selectionSoundName);
            updateSquareGhost(currentDrawingSize);
            _isActive = true;
            toolVisualsHolder.position += new Vector3(0, selectionTranslationValue, 0);
            glowOutlineObject.SetActive(true);
            glowFillObject.SetActive(false);
        }

        public DrawingAction createNewSquare(Vector3 position, float radius)
        {
            StatTracker.Instance.drawingToolUsageMap[DrawingController.DrawingToolType.square_stamp] = true;
            _currentSortingOrder++;
            GameObject newSquareObject = new GameObject();
            DrawingSquare drawingSquareData = newSquareObject.AddComponent<DrawingSquare>();
            newSquareObject.transform.position = new Vector3(0, 0, 0);
            Vector3 linePosition = position;

            //Create the borders
            GameObject topBorderLineObject = Instantiate(squareLinePrefab, linePosition, Quaternion.identity, newSquareObject.transform);
            topBorderLineObject.transform.localPosition = new Vector3(0, 0, linePosition.z);

            LineRenderer topBorderLineRenderer = topBorderLineObject.GetComponent<LineRenderer>();
            topBorderLineRenderer.material = ColourManager.Instance.baseLineMaterial;
            float lineWidth = topBorderLineRenderer.startWidth * (currentDrawingSize / minSize);
            topBorderLineRenderer.startWidth = lineWidth;
            topBorderLineRenderer.endWidth = lineWidth;
            float squareZDepth = linePosition.z - (_currentSortingOrder + 1) * DrawingController.zBufferValue;
            Vector3 topLeftCorner = new Vector3(-radius + linePosition.x, radius + linePosition.y, squareZDepth);
            Vector3 topRightCorner = new Vector3(radius + linePosition.x, radius + linePosition.y, squareZDepth);
            Vector3 bottomLeftCorner = new Vector3(-radius + linePosition.x, -radius + linePosition.y, squareZDepth);
            Vector3 bottomRightCorner = new Vector3(radius + linePosition.x, -radius + linePosition.y, squareZDepth);
            Vector3[] topCorners = { topLeftCorner, topRightCorner };
            topBorderLineRenderer.positionCount = 2;
            topBorderLineRenderer.SetPositions(topCorners);

            DrawingLineData topLineData = topBorderLineObject.GetComponent<DrawingLine>().drawingLineData;
            topLineData.lineSize = lineWidth;
            topLineData.author = SettingsManager.Instance.birdName;
            topLineData.lineColour = DrawingLineData.LineColour.Base;
            topLineData.zDepth = stampLineZOffset;

            GameObject bottomBorderLineObject = Instantiate(squareLinePrefab, linePosition, Quaternion.identity, newSquareObject.transform);
            bottomBorderLineObject.transform.localPosition = new Vector3(0, 0, linePosition.z);
            LineRenderer bottomBorderLineRenderer = bottomBorderLineObject.GetComponent<LineRenderer>();
            bottomBorderLineRenderer.material = ColourManager.Instance.baseLineMaterial;
            bottomBorderLineRenderer.startWidth = lineWidth;
            bottomBorderLineRenderer.endWidth = lineWidth;
            Vector3[] bottomCorners = { bottomRightCorner, bottomLeftCorner };
            bottomBorderLineRenderer.positionCount = 2;
            bottomBorderLineRenderer.SetPositions(bottomCorners);

            DrawingLineData bottomLineData = bottomBorderLineObject.GetComponent<DrawingLine>().drawingLineData;
            bottomLineData.lineSize = lineWidth;
            bottomLineData.author = SettingsManager.Instance.birdName;
            bottomLineData.lineColour = DrawingLineData.LineColour.Base;
            bottomLineData.zDepth = stampLineZOffset;

            GameObject leftBorderLineObject = Instantiate(squareLinePrefab, linePosition, Quaternion.identity, newSquareObject.transform);
            leftBorderLineObject.transform.localPosition = new Vector3(0, 0, linePosition.z);
            LineRenderer leftBorderLineRenderer = leftBorderLineObject.GetComponent<LineRenderer>();
            leftBorderLineRenderer.material = ColourManager.Instance.baseLineMaterial;
            leftBorderLineRenderer.startWidth = lineWidth;
            leftBorderLineRenderer.endWidth = lineWidth;
            Vector3[] leftCorners = { topLeftCorner, bottomLeftCorner };
            leftBorderLineRenderer.positionCount = 2;
            leftBorderLineRenderer.SetPositions(leftCorners);

            DrawingLineData leftLineData = leftBorderLineObject.GetComponent<DrawingLine>().drawingLineData;
            leftLineData.lineSize = lineWidth;
            leftLineData.author = SettingsManager.Instance.birdName;
            leftLineData.lineColour = DrawingLineData.LineColour.Base;
            leftLineData.zDepth = stampLineZOffset;

            GameObject rightBorderLineObject = Instantiate(squareLinePrefab, linePosition, Quaternion.identity, newSquareObject.transform);
            rightBorderLineObject.transform.localPosition = new Vector3(0, 0, linePosition.z);
            LineRenderer rightBorderLineRenderer = rightBorderLineObject.GetComponent<LineRenderer>();
            rightBorderLineRenderer.material = ColourManager.Instance.baseLineMaterial;
            rightBorderLineRenderer.startWidth = lineWidth;
            rightBorderLineRenderer.endWidth = lineWidth;
            Vector3[] rightCorners = { topRightCorner, bottomRightCorner };
            rightBorderLineRenderer.positionCount = 2;
            rightBorderLineRenderer.SetPositions(rightCorners);

            DrawingLineData rightLineData = rightBorderLineObject.GetComponent<DrawingLine>().drawingLineData;
            rightLineData.lineSize = lineWidth;
            rightLineData.author = SettingsManager.Instance.birdName;
            rightLineData.lineColour = DrawingLineData.LineColour.Base;
            rightLineData.zDepth = stampLineZOffset;

            //Create the fill
            float topHeight = linePosition.y + radius;
            float bottomHeight = linePosition.y - radius;
            float leftPosition = linePosition.x - radius - lineWidth / 4;
            float rightPosition = linePosition.x + radius + lineWidth / 4;
            GameObject fillLineObject = Instantiate(squareLinePrefab, linePosition, Quaternion.identity, newSquareObject.transform);
            DrawingLineData fillLineData = fillLineObject.GetComponent<DrawingLine>().drawingLineData;
            fillLineData.lineSize = lineWidth;
            fillLineData.author = SettingsManager.Instance.birdName;
            fillLineData.lineColour = _lineColourOptions[_currentLineColourIndex];
            fillLineData.zDepth = stampLineZOffset;
            fillLineObject.transform.localPosition = new Vector3(0, 0, linePosition.z);
            LineRenderer fillLineRenderer = fillLineObject.GetComponent<LineRenderer>();
            fillLineRenderer.startWidth = lineWidth;
            fillLineRenderer.endWidth = lineWidth;
            List<Vector3> edgeVertices = new List<Vector3>();
            float fillLineZDepth = -_currentSortingOrder * DrawingController.zBufferValue;
            for (float i = topHeight; i > bottomHeight + lineWidth / 4; i -= lineWidth)
            {

                Vector3 leftVertex = new Vector3(leftPosition, i, fillLineZDepth);
                Vector3 rightVertex = new Vector3(rightPosition, i, fillLineZDepth);

                if (i == topHeight)
                {
                    rightVertex -= new Vector3(lineWidth / 4, 0, 0);
                }
                if ((i - lineWidth) <= bottomHeight + lineWidth / 4)
                {
                    leftVertex += new Vector3(lineWidth / 4, 0, 0);
                }
                if (i % 2 == 0)
                {
                    edgeVertices.Add(leftVertex);
                    edgeVertices.Add(rightVertex);
                }
                else
                {
                    edgeVertices.Add(rightVertex);
                    edgeVertices.Add(leftVertex);
                }

            }
            fillLineRenderer.positionCount = edgeVertices.Count;
            fillLineRenderer.SetPositions(edgeVertices.ToArray());
            fillLineRenderer.material = _currentLineMaterial;
            _currentSortingOrder++;


            drawingSquareData.lineObjects.Add(topBorderLineObject);
            drawingSquareData.lineObjects.Add(leftBorderLineObject);
            drawingSquareData.lineObjects.Add(rightBorderLineObject);
            drawingSquareData.lineObjects.Add(bottomBorderLineObject);
            drawingSquareData.lineObjects.Add(fillLineObject);
            drawingSquareData.position = position;
            drawingSquareData.radius = radius;
            drawingSquareData.zDepth = squareZDepth + linePosition.z;
            controller.drawingSquares.Add(drawingSquareData);
            AddSquareDrawingAction addSquareAction = new AddSquareDrawingAction();
            addSquareAction.drawingSquareData = drawingSquareData;

            AudioManager.Instance.PlaySound(useSoundName);
            return addSquareAction;
        }

        private bool isPointInDrawingArea(Vector3 point)
        {

            Vector3 raycastPoint = new Vector3(point.x, point.y, Camera.main.transform.position.z);
            bool pointIsInDrawingArea = Physics2D.Raycast(raycastPoint, Vector3.forward, 100.0f, drawingAreaLayerMask);

            return pointIsInDrawingArea;
        }

        public void updateSquareGhost(float radius)
        {
            //Delete all children on the ghost
            List<Transform> ghostChildren = new List<Transform>();
            foreach (Transform ghostChild in cursorStampGhost.transform)
            {
                ghostChildren.Add(ghostChild);
            }
            for (int i = ghostChildren.Count - 1; i >= 0; i--)
            {
                Destroy(ghostChildren[i].gameObject);
            }
            Vector3 ghostPosition = Vector3.zero;

            //Create the borders
            GameObject topBorderLineObject = Instantiate(squareLinePrefab, ghostPosition, Quaternion.identity, cursorStampGhost.transform);
            topBorderLineObject.transform.localPosition = Vector3.zero;

            LineRenderer topBorderLineRenderer = topBorderLineObject.GetComponent<LineRenderer>();
            topBorderLineRenderer.material = ColourManager.Instance.stampBorderGhostMaterial;
            float lineWidth = topBorderLineRenderer.startWidth;
            Vector3 topLeftCorner = ghostPosition + new Vector3(-radius, radius, borderLineZOffset * 2);
            Vector3 topRightCorner = ghostPosition + new Vector3(radius, radius, borderLineZOffset * 2);
            Vector3 bottomLeftCorner = ghostPosition + new Vector3(-radius, -radius, borderLineZOffset * 2);
            Vector3 bottomRightCorner = ghostPosition + new Vector3(radius, -radius, borderLineZOffset * 2);
            Vector3[] topCorners = { topLeftCorner, topRightCorner };
            topBorderLineRenderer.positionCount = 2;
            topBorderLineRenderer.SetPositions(topCorners);

            GameObject bottomBorderLineObject = Instantiate(squareLinePrefab, ghostPosition, Quaternion.identity, cursorStampGhost.transform);
            bottomBorderLineObject.transform.localPosition = Vector3.zero;
            LineRenderer bottomBorderLineRenderer = bottomBorderLineObject.GetComponent<LineRenderer>();
            bottomBorderLineRenderer.material = ColourManager.Instance.stampBorderGhostMaterial;
            Vector3[] bottomCorners = { bottomRightCorner, bottomLeftCorner };
            bottomBorderLineRenderer.positionCount = 2;
            bottomBorderLineRenderer.SetPositions(bottomCorners);

            GameObject leftBorderLineObject = Instantiate(squareLinePrefab, ghostPosition, Quaternion.identity, cursorStampGhost.transform);
            leftBorderLineObject.transform.localPosition = Vector3.zero;
            LineRenderer leftBorderLineRenderer = leftBorderLineObject.GetComponent<LineRenderer>();
            leftBorderLineRenderer.material = ColourManager.Instance.stampBorderGhostMaterial;
            Vector3[] leftCorners = { topLeftCorner, bottomLeftCorner };
            leftBorderLineRenderer.positionCount = 2;
            leftBorderLineRenderer.SetPositions(leftCorners);

            GameObject rightBorderLineObject = Instantiate(squareLinePrefab, ghostPosition, Quaternion.identity, cursorStampGhost.transform);
            rightBorderLineObject.transform.localPosition = Vector3.zero;
            LineRenderer rightBorderLineRenderer = rightBorderLineObject.GetComponent<LineRenderer>();
            rightBorderLineRenderer.material = ColourManager.Instance.stampBorderGhostMaterial;
            Vector3[] rightCorners = { topRightCorner, bottomRightCorner };
            rightBorderLineRenderer.positionCount = 2;
            rightBorderLineRenderer.SetPositions(rightCorners);

            //Create the fill
            float topHeight = ghostPosition.y + radius;
            float bottomHeight = ghostPosition.y - radius;
            float leftPosition = ghostPosition.x - radius;
            float rightPosition = ghostPosition.x + radius;
            for (float i = topHeight; i >= bottomHeight; i -= lineWidth / 8)
            {
                GameObject fillLineObject = Instantiate(squareLinePrefab, ghostPosition, Quaternion.identity, cursorStampGhost.transform);
                fillLineObject.transform.localPosition = Vector3.zero;
                LineRenderer fillLineRenderer = fillLineObject.GetComponent<LineRenderer>();
                fillLineRenderer.positionCount = 2;
                Vector3 leftVertex = new Vector3(leftPosition, i, borderLineZOffset);
                Vector3 rightVertex = new Vector3(rightPosition, i, borderLineZOffset);
                Vector3[] edgeVertices = { leftVertex, rightVertex };
                fillLineRenderer.SetPositions(edgeVertices);
                fillLineRenderer.material = _currentGhostLineMaterial;

            }

        }
    }
}