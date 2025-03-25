using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PanelGroup : MonoBehaviour
{
    public GameObject[] panels;
    public TabGroup tabGroup;

    [FormerlySerializedAs("panelIndex")] public int activePanelIndex;
    
    protected void Awake()
    {
        ShowCurrentPanel();
    }

    private void ShowCurrentPanel()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].GameObject().SetActive(i == activePanelIndex);
        }
    }

    public void SetPage(int panelIndex)
    {
        activePanelIndex = panelIndex;
        ShowCurrentPanel();
    } 
}
