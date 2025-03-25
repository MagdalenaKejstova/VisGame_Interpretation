using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChartAndGraph;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

public class StackedAreaChartIntroductionActivityManager : IntroductionActivityManager
{
    [Tooltip("Name for the dataset to be displayed on the label")]
    public string graphNameLabelText = "Example dataset";

    public TextMeshProUGUI graphNameLabel;

    public GameObject areaControlPanelTemplate;
    
    public Material category1Color;
    public string category1Name;

    public Material category2Color;
    public string category2Name;

    public Material category3Color;
    public string category3Name;

    public ActivatebleButton toggleStackingButton;

    private GraphChartBase _graphStylePrefab;
    private List<GameObject> _areaControlPanels = new();

    protected override void ActivitySpecificSetUp()
    {
        AnswersStart();
        SetUpGraph();
        SetUpLabel();
        SetUpActivityListeners();
        SetUpAreaControlPanels();

        if (toggleStackingButton != null)
        {
            toggleStackingButton.gameObject.transform.parent.gameObject.SetActive(true);
        }
    }

    protected void SetUpGraph()
    {
        var graphObject = graphDataFiller.GraphObject.gameObject;
        graphObject.transform.parent.gameObject.SetActive(true);
        graphDataFiller.Fill();
        
        _graphStylePrefab = graphDataFiller.CategoryPrefab;
    }

    private void SetUpLabel()
    {
        if (graphNameLabel != null)
        {
            graphNameLabel.text = graphNameLabelText;
        }
    }

    private void SetUpAreaControlPanels()
    {
        InstantiateControlPanel(category1Name, category1Color);
        InstantiateControlPanel(category2Name, category2Color);
        InstantiateControlPanel(category3Name, category3Color);
        areaControlPanelTemplate.gameObject.SetActive(false);
    }

    private void InstantiateControlPanel(string categoryName, Material categoryFill)
    {
        var newPanel = Instantiate(areaControlPanelTemplate, areaControlPanelTemplate.transform.parent);
        newPanel.transform.SetAsLastSibling();
        newPanel.GetComponentInChildren<TextMeshProUGUI>().text = categoryName;
        
        var panelImage = newPanel.transform.Find("CategoryColorPanel").transform.Find("CategoryColorFrame").transform.Find("CategoryColorIcon").GetComponent<Image>();
        panelImage.color = categoryFill.color;
        
        var areaToggleButton = newPanel.GetComponent<AreaToggleButton>(); 
        areaToggleButton.onAreaToggleButtonClicked.AddListener(ToggleAreaVisibility);
        areaToggleButton.controlledArea = categoryName;
        
        newPanel.gameObject.SetActive(true);
        newPanel.GetComponent<ActivatebleButton>().Enable();

        _areaControlPanels.Add(newPanel);
    }
    
    public void ToggleStacking()
    {
        var isStackingEnabled = toggleStackingButton.isEnabled;
        // Remove fill before unstacking
        if (isStackingEnabled)
        {
            _graphStylePrefab.DataSource.SetCategoryFill(category1Name, null, false);
            _graphStylePrefab.DataSource.SetCategoryFill(category2Name, null, false);
            _graphStylePrefab.DataSource.SetCategoryFill(category3Name, null, false);
        }
        // Add fill before stacking
        else
        {
            _graphStylePrefab.DataSource.SetCategoryFill(category1Name, category1Color, false);
            _graphStylePrefab.DataSource.SetCategoryFill(category2Name, category2Color, false);
            _graphStylePrefab.DataSource.SetCategoryFill(category3Name, category3Color, false);
        }

        graphDataFiller.StackCategories = !isStackingEnabled;
        graphDataFiller.Fill();
        toggleStackingButton.SetState(!isStackingEnabled);
    }

    private void ToggleAreaVisibility(string categoryName)
    {
        var isEnabled = graphDataFiller.GraphObject.DataSource.GetCategoryData(categoryName).Enabled;
        
        var toggledPanel = _areaControlPanels.Find(panel => panel.GetComponent<AreaToggleButton>().controlledArea == categoryName);
        toggledPanel.GetComponent<ActivatebleButton>().SetState(!isEnabled);
        graphDataFiller.Categories.ToList().Find(cat => cat.Name == categoryName).Enabled = !isEnabled;
        graphDataFiller.Fill();
        graphDataFiller.GraphObject.DataSource.SetCategoryEnabled(categoryName, !isEnabled);
    }

    public override void ActivitySpecificCleanup()
    {
        var graph = graphDataFiller.GraphObject.gameObject;
        graph.transform.parent.gameObject.SetActive(false);

        if (toggleStackingButton != null)
        {
            toggleStackingButton.gameObject.transform.parent.gameObject.SetActive(false);
        }
        
        foreach (var controlPanel in _areaControlPanels)
        {
            controlPanel.SetActive(false);
        }
    }

    private void AnswersStart()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "answersLog.txt");
        string logEntry = " === STACKED AREA CHART === ";
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // Write to file, appending each entry
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"\n{timestamp}");
            writer.WriteLine(logEntry);
        }
    }
}