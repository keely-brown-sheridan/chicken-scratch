using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingBoxType : MonoBehaviour
{
    public TaskData.TaskModifier modifier => _modifier;

    [SerializeField]
    private TaskData.TaskModifier _modifier;
}
