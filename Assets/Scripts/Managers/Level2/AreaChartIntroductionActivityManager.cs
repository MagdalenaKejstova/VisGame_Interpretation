using System;
using System.Collections;
using System.Collections.Generic;
using ChartAndGraph;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class AreaChartIntroductionActivityManager : IntroductionActivityManager
{
    [Tooltip("'Name' parameter of Category element area chart in IntroductionActivityDataFiller")]
    public string lineCategoryName = "LineCumulativePrecip";
    [Tooltip("'Name' parameter of Category element representing line chart in IntroductionActivityDataFiller")]
    public string areaCategoryName = "AreaCumulativePrecip";

    [Tooltip("Name for the dataset to be displayed on the label")]
    public string graphNameLabelText = "Accumulated precipitation in Innsbruck - March 2023 (mm)";
    public TextMeshProUGUI graphNameLabel;
    
    public bool isAreaVisible = true;
    private Material _innerFill;

    private DraggableGraphArea _draggableGraphArea;
    
    protected override void ActivitySpecificSetUp()
    {
        AnswersStart();
        SetUpGraph();
        SetUpLabel();
        SetUpActivityListeners();
        InitializeDragging();
    }
    
    protected void SetUpGraph()
    {
        var graph = graphDataFiller.GraphObject.gameObject;
        graph.transform.parent.gameObject.SetActive(true);
        graphDataFiller.Fill();
    }
    
    private void InitializeDragging()
    {
        _draggableGraphArea = graphDataFiller.GraphObject.gameObject.GetComponent<DraggableGraphArea>();
        _draggableGraphArea.precipitationAmountVisualizer.transform.parent.gameObject.SetActive(true);
        _draggableGraphArea.precipitationDateVisualizer.transform.parent.gameObject.SetActive(true);
        _draggableGraphArea.Initialize(graphDataFiller, lineCategoryName, areaCategoryName);
    }

    private void SetUpLabel()
    {
        if (graphNameLabel != null)
        {
            graphNameLabel.text = graphNameLabelText;
        }
    }
    
    public override void ActivitySpecificCleanup()
    {
        if (_draggableGraphArea != null)
        {
            _draggableGraphArea.precipitationAmountVisualizer.transform.parent.gameObject.SetActive(false);
            _draggableGraphArea.precipitationDateVisualizer.transform.parent.gameObject.SetActive(false);
        }
        
        var graph = graphDataFiller.GraphObject.gameObject;
        graph.transform.parent.gameObject.SetActive(false);
    }

    private void AnswersStart()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "answersLog.txt");
        string logEntry = " === AREA CHART === ";
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // Write to file, appending each entry
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"\n{timestamp}");
            writer.WriteLine(logEntry);
        }
    }
}
