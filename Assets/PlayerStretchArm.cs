using ChickenScratch;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStretchArm : MonoBehaviour
{
    public enum Variant
    {
        store, accusation
    }
    [SerializeField]
    private Variant variant;

    [SerializeField]
    private Transform handTransform;

    public ColourManager.BirdName birdName => _birdName;
    [SerializeField]
    private ColourManager.BirdName _birdName;

    [SerializeField]
    private float moveDelay = 0.5f;

    private float timeSinceLastMove = 0f;
    private void Start()
    {
        timeSinceLastMove = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        handTransform.position = Input.mousePosition;
        timeSinceLastMove += Time.deltaTime;

        if (timeSinceLastMove > moveDelay)
        {
            timeSinceLastMove = 0.0f;
            if (GameManager.Instance.playerFlowManager.serverIsReady && NetworkClient.ready)
            {
                GameManager.Instance.gameDataHandler.CmdLongArmPosition(birdName, handTransform.position, variant);
            }

        }
    }
}
