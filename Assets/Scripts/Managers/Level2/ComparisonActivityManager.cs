using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChartAndGraph;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ComparisonActivityManager : QuestionActivityManager
{
    [Tooltip("Button to toggle between a common an individual scales for displayed graphs")]
    public ActivatebleButton adjustScaleButton;
    
    [Tooltip("Name for the graph group to be displayed on the group label")]
    public string graphGroupLabelText = "Cumulative precipitation in August 2023 (mm)";
    public TextMeshProUGUI graphGroupLabel;
    
    [Tooltip("Name for the graph to be displayed on the individual graph label")]
    public string graph1LabelText = "Innsbruck, Austria";
    public TextMeshProUGUI graph1Label;

    [Tooltip("Name for the graph to be displayed on the individual graph label")]
    public string graph2LabelText = "Tutunendo, Colombia";
    public TextMeshProUGUI graph2Label;
    
    public List<GraphDataFiller> graphDataFillers;

    private GraphChartBase _graph1; 
    private GraphChartBase _graph2;
    private GraphChartBase _graphToAdjust;
    
    private int _supportedGraphsCount = 2;
    
    private double _verticalViewOriginCommon = 0;
    private double _verticalViewSizeCommon = 0;
    private double _verticalViewSizeOriginal = 0;
    
    
    protected override void ActivitySpecificSetUp()
    {
        base.ActivitySpecificSetUp();
        SetUpGraphs();
        SetUpLabels();
        SetUpToggleButton();
        SetUpActivityListeners();
    }
    
    private void SetUpActivityListeners()
    {
        onAllQuestionsAnswered.AddListener(CheckWinCondition);
    }

    private void CheckWinCondition()
    {
        canFinishActivity = true;
        onActivityCompleted.Invoke(MaxScore);
    }
    
    protected void SetUpGraphs()
    {
        if (graphDataFillers.Count > 0)
        {
            var graphGroupBox = GetGraphGroupBox();
            graphGroupBox.SetActive(true);
        }

        if (graphDataFillers.Count >= _supportedGraphsCount)
        {
            _graph1 = graphDataFillers[0].GraphObject;
            _graph2 = graphDataFillers[1].GraphObject;
        }

        _verticalViewSizeCommon = GetMaxVerticalViewSize(_graph1, _graph2);
        _graphToAdjust = GetMinSpreadGraph(_graph1, _graph2);
        _verticalViewSizeOriginal = _graphToAdjust.DataSource.VerticalViewSize;
        
        for (int i = 0; i < graphDataFillers.Count; i++)
        {
            if (i < _supportedGraphsCount)
            {
                var dataFiller = graphDataFillers[i];
                dataFiller.Fill();
                dataFiller.GraphObject.DataSource.VerticalViewOrigin = _verticalViewOriginCommon;
                ToggleCommonVerticalScale();   
            }
        }
    }

    private double GetMaxVerticalViewSize(GraphChartBase graph1, GraphChartBase graph2)
    {
        var size1 = graph1.DataSource.VerticalViewSize;
        var size2 = graph2.DataSource.VerticalViewSize;

        return size1 > size2 ? size1 : size2;
    }
    
    private GraphChartBase GetMinSpreadGraph(GraphChartBase graph1, GraphChartBase graph2)
    {
        var size1 = graph1.DataSource.VerticalViewSize;
        var size2 = graph2.DataSource.VerticalViewSize;

        return size1 < size2 ? graph1 : graph2;
    }

    public void ToggleCommonVerticalScale()
    {
        var hasCommonScale = _graphToAdjust.DataSource.VerticalViewSize == _verticalViewSizeCommon; 
        // Return to original
        if (hasCommonScale)
        {
            adjustScaleButton.Disable();
            _graphToAdjust.DataSource.VerticalViewSize = _verticalViewSizeOriginal;
        }
        // Adjust to common
        else
        {
            adjustScaleButton.Enable();
            _graphToAdjust.DataSource.VerticalViewSize = _verticalViewSizeCommon;
        }
    } 
    
    private void SetUpLabels()
    {
        if (graphGroupLabel != null)
        {
            graphGroupLabel.text = graphGroupLabelText;
        }
        
        if (graph1Label != null)
        {
            graph1Label.text = graph1LabelText;
        }
        
        if (graph2Label != null)
        {
            graph2Label.text = graph2LabelText;
        }
    }

    private void SetUpToggleButton()
    {
        adjustScaleButton.gameObject.SetActive(true);
    }

    public override void ActivitySpecificCleanup()
    {
        base.ActivitySpecificCleanup();
        if (graphDataFillers.Count > 0)
        {
            var graphGroupBox = GetGraphGroupBox();
            graphGroupBox.SetActive(false);
        }
        adjustScaleButton.gameObject.SetActive(false);
    }

    private GameObject GetGraphGroupBox()
    {
        var graph = graphDataFillers.First().GraphObject.gameObject; 
        var individualGraphLayout = graph.transform.parent.gameObject;
        var groupGraphLayout = individualGraphLayout.transform.parent.gameObject;
        var graphGroupBox = groupGraphLayout.transform.parent.gameObject;
        return graphGroupBox;
    }
}
