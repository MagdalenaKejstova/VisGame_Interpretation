using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public Color tabIdle;
    public Color tabActive;
    public Color textActive;
    public Color textIdle;
    public TabButton selectedTab;
    public PanelGroup panelGroup;

    public void Subscribe(TabButton tabButton)
    {
        tabButtons ??= new List<TabButton>();
        tabButtons.Add(tabButton);
    }

    public void OnTabSelected(TabButton tabButton)
    {
        selectedTab = tabButton;
        ResetTabs();
        tabButton.GetComponent<Image>().color = tabActive;
        tabButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = textActive;

        if (panelGroup != null)
        {
            var index = tabButton.transform.GetSiblingIndex();
            panelGroup.SetPage(index);
        }
    }

    public void ResetTabs()
    {
        foreach (var button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab)
            {
                continue;
            }

            button.GetComponent<Image>().color = tabIdle;
            button.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = textIdle;
        }
    }
    
    protected void Start()
    {
        if (selectedTab != null)
        {
            OnTabSelected(selectedTab);
        }
    }
}

