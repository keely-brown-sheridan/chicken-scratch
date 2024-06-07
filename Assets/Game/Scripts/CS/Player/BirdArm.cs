using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class BirdArm : MonoBehaviour
    {
        public SpriteRenderer sprite;
        public GameObject holdingObject;
        public ColourManager.BirdName birdName;
        public GameObject heldFolderObject;
        public Vector3 targetPosition;
        public float movementSpeed;

        public void Hold()
        {
            sprite.enabled = false;
            holdingObject.SetActive(true);
        }

        public void Unhold()
        {
            sprite.enabled = true;
            holdingObject.SetActive(false);
        }

        void Start()
        {
            targetPosition = transform.position;
        }

        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
        }
    }
}