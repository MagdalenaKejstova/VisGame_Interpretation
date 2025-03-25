using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChartAndGraph;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class StreamGraphIntroductionActivityManager : IntroductionActivityManager
{
    //public GameObject toggleCurvesButton;
    public GameObject toggleCenteringButton;
    public GameObject auxiliaryPanel;

    [Tooltip("Name for the dataset to be displayed on the label")]
    public string graphNameLabelText = "Example dataset";

    public TextMeshProUGUI graphNameLabel;

    public string category1Name;
    public Material category1Color;

    public string category2Name;
    public Material category2Color;

    public string category3Name;
    public Material category3Color;

    public string category4Name;
    public Material category4Color;

    public string category5Name;
    public Material category5Color;

    public GameObject areaControlPanelTemplate;
    private List<GameObject> _areaControlPanels = new();


    protected override void ActivitySpecificSetUp()
    {
        AnswersStart();
        base.ActivitySpecificSetUp();
        SetUpGraph();
        SetUpLabel();
        SetUpAreaControlPanels();
        SetUpButtons();
    }
    
    private void SetUpButtons()
    {
        //toggleCurvesButton.transform.parent.gameObject.SetActive(true);
        toggleCenteringButton.transform.parent.gameObject.SetActive(true);
    }

    protected void SetUpGraph()
    {
        var graphObject = graphDataFiller.GraphObject.gameObject;
        graphObject.transform.parent.gameObject.SetActive(true);
        graphDataFiller.Fill();
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
        auxiliaryPanel.gameObject.SetActive(true);
        InstantiateControlPanel(category1Name, category1Color);
        InstantiateControlPanel(category2Name, category2Color);
        InstantiateControlPanel(category3Name, category3Color);
        InstantiateControlPanel(category4Name, category4Color);
        InstantiateControlPanel(category5Name, category5Color);
        areaControlPanelTemplate.gameObject.SetActive(false);
    }

    private void InstantiateControlPanel(string categoryName, Material categoryFill)
    {
        var newPanel = Instantiate(areaControlPanelTemplate, areaControlPanelTemplate.transform.parent);
        newPanel.transform.SetAsLastSibling();
        newPanel.GetComponentInChildren<TextMeshProUGUI>().text = categoryName;

        var panelImage = newPanel.transform.Find("CategoryColorPanel").transform.Find("CategoryColorFrame").transform
            .Find("CategoryColorIcon").GetComponent<Image>();
        panelImage.color = categoryFill.color;

        var areaToggleButton = newPanel.GetComponent<AreaToggleButton>();
        areaToggleButton.onAreaToggleButtonClicked.AddListener(ToggleAreaVisibility);
        areaToggleButton.controlledArea = categoryName;

        newPanel.gameObject.SetActive(true);
        newPanel.GetComponent<ActivatebleButton>().Enable();

        _areaControlPanels.Add(newPanel);
    }

    private void ToggleAreaVisibility(string categoryName)
    {
        var isEnabled = graphDataFiller.GraphObject.DataSource.GetCategoryData(categoryName).Enabled;

        var toggledPanel =
            _areaControlPanels.Find(panel => panel.GetComponent<AreaToggleButton>().controlledArea == categoryName);
        toggledPanel.GetComponent<ActivatebleButton>().SetState(!isEnabled);
        graphDataFiller.Categories.ToList().Find(cat => cat.Name == categoryName).Enabled = !isEnabled;
        graphDataFiller.Fill();
        graphDataFiller.GraphObject.DataSource.SetCategoryEnabled(categoryName, !isEnabled);
    }

    // public void ToggleCurves()
    // {
    //     if (!gameObject.activeSelf)
    //         return;
        
    //     var curvesButton = toggleCurvesButton.transform.GetComponentInChildren<ActivatebleButton>();
    //     curvesButton.SetState(!curvesButton.isEnabled);

    //     graphDataFiller.CurveCategories = !graphDataFiller.CurveCategories;
    //     graphDataFiller.Fill();
    // }

    public void ToggleCentering()
    {
        if (!gameObject.activeSelf)
            return;
        
        var centeringButton = toggleCenteringButton.GetComponentInChildren<ActivatebleButton>();
        centeringButton.SetState(!centeringButton.isEnabled);

        graphDataFiller.CenterCategories = !graphDataFiller.CenterCategories;
        graphDataFiller.Fill();
    }

    public override void ActivitySpecificCleanup()
    {
        base.ActivitySpecificCleanup();

        var graph = graphDataFiller.GraphObject.gameObject;
        graph.transform.parent.gameObject.SetActive(false);

        if (toggleCenteringButton != null)
        {
            toggleCenteringButton.transform.parent.gameObject.SetActive(false);
        }

        // if (toggleCurvesButton != null)
        // {
        //     toggleCurvesButton.transform.parent.gameObject.SetActive(false);
        // }

        foreach (var controlPanel in _areaControlPanels)
        {
            controlPanel.gameObject.SetActive(false);
        }
        auxiliaryPanel.gameObject.SetActive(false);

    }

    private void AnswersStart()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "answersLog.txt");
        string logEntry = " === STREAM GRAPH === ";
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // Write to file, appending each entry
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"\n{timestamp}");
            writer.WriteLine(logEntry);
        }
    }
}