using System.Collections;
using System.Collections.Generic;
using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Linq;

public class LineDrawingActivityManager : LineActivityManagerBase
{
    protected int _maxScore;

    public GameObject auxiliaryPanel;

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
    public string category6Name;
    public Material category6Color;
    public GameObject areaControlPanelTemplate;
    private List<GameObject> _areaControlPanels = new();
    private Dictionary<string, Material> categoryColors = new Dictionary<string, Material>();
    private Dictionary<string, List<DoubleVector3>> regionDataPoints = new Dictionary<string, List<DoubleVector3>>();
    

    protected override void ActivitySpecificSetUp()
    {
        if (graphDataFiller.GraphObject != null)
    {
        graphDataFiller.GraphObject.DataSource.Clear();
        graphDataFiller.GraphObject.gameObject.SetActive(false);  // Hide the previous graph
    }

        SetUpGraph();
    
        Graph = graphDataFiller.GraphObject;
        
        // Ensure Graph and DataSource are filled before fetching data points
        graphDataFiller.Fill();

        regionDataPoints["Afrika"] = GetDataPointsForRegion("Afrika");
        regionDataPoints["Asien"] = GetDataPointsForRegion("Asien");
        regionDataPoints["Europa"] = GetDataPointsForRegion("Europa");
        regionDataPoints["Nord Amerika"] = GetDataPointsForRegion("Nord Amerika");
        regionDataPoints["Südamerika"] = GetDataPointsForRegion("Südamerika");
        regionDataPoints["Australien"] = GetDataPointsForRegion("Australien");

        // Debug to ensure regionDataPoints are populated
        foreach (var region in regionDataPoints.Keys)
        {
            Debug.Log($"{region} has {regionDataPoints[region].Count} data points.");
        }

        DataPoints = GetChartData();
        DataWorldPositions = GetDataWorldPositions();


        SetUpLabel();
        SetUpAreaControlPanels();

        SetUpGameObjects();
        SetUpConnectionManager();
        SetUpActivityListeners();

        HideAllGraphLines();
        _maxScore = CalculateMaxScore();
    }

    private void ClearPreviousGraphData()
{
    // Ensure all graph visual elements are cleared/reset
    graphDataFiller.GraphObject.DataSource.Clear(); // Clear the data source
    graphDataFiller.GraphObject.gameObject.SetActive(false); // Hide the graph
}

    private List<DoubleVector3> GetDataPointsForRegion(string region)
{
    // Return a list of data points for the specified region
    if (Graph == null || Graph.DataSource == null)
    {
        Debug.LogError("Graph or DataSource is null.");
        return new List<DoubleVector3>();  // Return an empty list to avoid further null reference exceptions
    }

    // Return a list of data points for the specified region
    return Graph.DataSource.GetPoints(region);
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
        InstantiateControlPanel(category6Name, category6Color);
        areaControlPanelTemplate.gameObject.SetActive(false);
    }

    private void HideAllGraphLines()
    {
        ToggleAreaVisibility(category1Name);
        ToggleAreaVisibility(category2Name);
        //ToggleAreaVisibility(category3Name);
        ToggleAreaVisibility(category4Name);
        ToggleAreaVisibility(category5Name);
        ToggleAreaVisibility(category6Name);
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

    // Set the state of the button to "disabled" (not enabled) initially
    newPanel.GetComponent<ActivatebleButton>().SetState(false); // Disable the button initially

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


    private int CalculateMaxScore()
    {
        Debug.Log($"{scoreBasePerUnit} (score base per unit) and {scoreTimeBonus} (score time bonus) and {scoreCorrectAnswerBonus} (answer bonus)  max score for multiline.");
        return scoreBasePerUnit + scoreTimeBonus; //+ scoreCorrectAnswerBonus;
    }

    protected override void SetUpActivityListeners()
    {
        //ConnectionManager.correctConnectionCreated.AddListener(CorrectConnectionCreated);
        //ConnectionManager.incompleteConnectionCreated.AddListener(IncompleteConnectionCreated);
        //ConnectionManager.unorderedConnectionCreated.AddListener(UnorderedConnectionCreated);
        onAllInfoPagesSeen.AddListener(CheckWinCondition);
    }

    protected override void SetUpConnectionManager()
    {
        ConnectionManager = GetComponent<ConnectionManager>();
        ConnectionManager.SetObjectsToConnect(DataPoints.Count);
    }

    protected override void SetUpGameObjects()
    {
        SpawnAnchorDataPoints();
    }

    private void SpawnAnchorDataPoints()
{
    // Define how many points belong to each region
    int pointsPerRegion = 71;

    // Define materials for each region in sequence
    List<Material> regionMaterials = new List<Material>
    {
        category1Color, // Africa
        category2Color, // Asia
        category6Color, // Europe 
        category3Color, // North America 
        category4Color, // South America
        category5Color  // Australia
    };

    for (int i = 0; i < DataWorldPositions.Count; i++)
    {
        var dataWorldPosition = DataWorldPositions[i];
        dataWorldPosition.z = 0;
        var parent = Graph.transform;

        var anchorDataPoint = Instantiate(anchorDataPointTemplate, dataWorldPosition, Quaternion.identity, parent);
        anchorDataPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(pointSize, pointSize);
        anchorDataPoint.GetComponent<SphereCollider>().radius = (float)pointSize / 2;

        int regionIndex = i / pointsPerRegion;  
        if (regionIndex >= regionMaterials.Count) regionIndex = regionMaterials.Count - 1; 

        var material = regionMaterials[regionIndex];

        var imageComponent = anchorDataPoint.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.color = material.color;
        }
        else
        {
            Debug.LogError("Image component not found on anchorDataPoint.");
        }

        // Configure the Anchor component
        var anchor = anchorDataPoint.GetComponent<Anchor>();
        anchor.order = i + 1;
        anchor.expectedStartIndex = 1;
        anchor.expectedObjectsToConnectCount = DataWorldPositions.Count;
        anchor.possibleObjectsToConnectCount = DataWorldPositions.Count;
        anchorDataPoint.SetActive(true);
    }

    // Deactivate the template after instantiating
    anchorDataPointTemplate.SetActive(false);
}


    protected override void CheckWinCondition()
    {
        canFinishActivity = true;
        EndActivityScoreCheck();
        onActivityCompleted.Invoke(_maxScore);
    }

    public override void ActivitySpecificCleanup()
    {
        // base.ActivitySpecificCleanup();

        // var graph = graphDataFiller.GraphObject.gameObject;
        // graph.transform.parent.gameObject.SetActive(false);

        // foreach (var controlPanel in _areaControlPanels)
        // {
        //     controlPanel.gameObject.SetActive(false);
        // }
        // auxiliaryPanel.gameObject.SetActive(false);

        StartCoroutine(DelayedCleanup());

    }

private IEnumerator DelayedCleanup()
{
    // Wait to allow coroutines/animations to finish
    yield return new WaitForSeconds(0.5f);

    // Deactivate graph and clear the data
    if (graphDataFiller.GraphObject != null)
    {
        graphDataFiller.GraphObject.DataSource.Clear();  // Clear the data source
        graphDataFiller.GraphObject.gameObject.SetActive(false);  // Hide the graph
    }

    // Hide all control panels
    foreach (var controlPanel in _areaControlPanels)
    {
        if (controlPanel != null)
        {
            controlPanel.SetActive(false);
        }
    }

    auxiliaryPanel.SetActive(false);  // Hide the auxiliary panel

    _areaControlPanels.Clear();  // Clear the area control panels list
}
}