using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StretchArm : MonoBehaviour
{
    public Vector3 targetPosition;
    public ColourManager.BirdName birdName => _birdName; 
    [SerializeField]
    private ColourManager.BirdName _birdName;

    [SerializeField]
    private Transform handTransform;

    [SerializeField]
    private float movementSpeed;

    private void Start()
    {
        targetPosition = transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        handTransform.position = Vector3.MoveTowards(handTransform.position, targetPosition, movementSpeed);
    }
}
