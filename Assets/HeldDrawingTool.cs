using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HeldDrawingTool : MonoBehaviour
{
    [SerializeField]
    private List<Image> colouredImages;

    private DrawingTool mappedTool;

    public void Initialize(DrawingTool newTool)
    {
        newTool.OnDeselect.AddListener(Deselect);
        newTool.OnSelect.AddListener(Select);
    }

    private void Select()
    {
        gameObject.SetActive(true);
    }

    private void Deselect()
    {
        gameObject.SetActive(false);
    }

    public void SetColour(Color inColour)
    {
        foreach(Image colouredImage in colouredImages)
        {
            colouredImage.color = inColour;
        }
    }

    private void OnDestroy()
    {
        if(mappedTool)
        {
            mappedTool.OnDeselect.RemoveListener(Deselect);
            mappedTool.OnSelect.RemoveListener(Select);
        }
    }
}
