using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.PlayerBirdArm;

public class CasePile : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> objectsToShowOnActivate;

    [SerializeField]
    private float timePerFlash;
    [SerializeField]
    private GameObject glowObject;

    private bool selectable = false;
    private float totalTimeFlashing = 0f;


    private void Start()
    {
        GameManager.Instance.playerFlowManager.drawingRound.onPlayerStartTask.AddListener(Deactivate);
        GameManager.Instance.playerFlowManager.drawingRound.onPlayerSubmitTask.AddListener(Activate);
    }

    private void Update()
    {
        if (selectable)
        {
            totalTimeFlashing += Time.deltaTime;
            if (totalTimeFlashing > timePerFlash)
            {
                glowObject.SetActive(!glowObject.activeSelf);
                totalTimeFlashing = Time.deltaTime;
            }
        }
    }


    public void Select()
    {
        if (selectable)
        {
            GameManager.Instance.playerFlowManager.drawingRound.onPlayerStartTask.Invoke();
            GameManager.Instance.gameDataHandler.CmdRequestCaseChoice(SettingsManager.Instance.birdName);
        }
    }

    public void Activate()
    {
        if(SettingsManager.Instance.gameMode.numberOfCases > 0)
        {
            foreach (GameObject objectToShow in objectsToShowOnActivate)
            {
                objectToShow.SetActive(true);
            }
            selectable = true;
        }
    }

    public void Deactivate()
    {
        foreach (GameObject objectToShow in objectsToShowOnActivate)
        {
            objectToShow.SetActive(false);
        }
        selectable = false;
    }
}
