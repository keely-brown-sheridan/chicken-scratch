using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CasePlayerTabs : MonoBehaviour
{
    [SerializeField]
    private Color notUsedColour;

    [SerializeField]
    private List<Image> tabBases;

    [SerializeField]
    private List<GameObject> tabHighlights;

    public void Initialize(int round, int caseID)
    {
        ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];

        foreach(GameObject tabHighlight in tabHighlights)
        {
            tabHighlight.SetActive(false);

        }
        foreach(Image tabBase in tabBases)
        {
            tabBase.gameObject.SetActive(false);
            tabBase.color = notUsedColour;
        }

        //tabHighlights[round-1].SetActive(true);
        for(int i = 0; i < currentCase.playerOrder.Count; i++)
        {
            tabBases[i].gameObject.SetActive(true);
            if(i < round)
            {
                tabBases[i].color = GameDataManager.Instance.GetBird(currentCase.playerOrder[i+1]).colour;
            }
        }
    }
}
