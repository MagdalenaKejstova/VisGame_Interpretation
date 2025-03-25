using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChartAndGraph;
using Febucci.UI;
using TMPro;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.Localization.Plugins.XLIFF.V20;
#endif
using UnityEngine;

public class PointPlottingActivityManager : ActivityManager
{
    public GameObject dataPointTargetSlot;
    public GameObject dataPointPanel;
    private GraphChartBase graph;
    private List<Vector3> dataWorldPositions = new();
    private List<DoubleVector3> dataPoints = new();
    private int _placedPoints = 0;
    private int _maxScore;

    public float placementTolerance = 400f;
    
    protected override void ActivitySpecificSetUp()
    {
        SetUpGraph();
        graph = graphDataFiller.GraphObject;
        SetUpGameObjects();
        _maxScore = CalculateMaxScore();
    }
    
    private int CalculateMaxScore()
    {
        return scoreBasePerUnit + scoreTimeBonus + scoreCorrectAnswerBonus * dataPoints.Count;
    }
    
    private void SetUpGameObjects()
    {
        dataPoints = GetChartData();
        dataWorldPositions = GetDataWorldPositions();
        
        SpawnDataPointSourceSlots();
        SpawnDataPointTargetSlots();
    }
    
    private void SetUpPointSlotListeners(GameObject dataPointSlot)
    {
        var draggableItemSlot = dataPointSlot.GetComponent<DraggableItemSlot>();

        draggableItemSlot.correctItemDropped.AddListener(() => {
            CorrectPointMarked(draggableItemSlot.lastDroppedItem);
        });

        draggableItemSlot.wrongItemDropped.AddListener(() => {
            WrongPointMarked(draggableItemSlot.lastDroppedItem);
        });
    }

    private bool IsPointWithinTolerance(Vector3 pointPosition, Vector3 targetPosition)
    {
        return Vector3.Distance(pointPosition, targetPosition) <= placementTolerance;
    }
    
    private void CorrectPointMarked(GameObject droppedItem)
    {
        _placedPoints++;
        UpdateScoreCorrectAnswer();
        CheckWinCondition();
        ShowSuccessFeedback("correctPlacement");
    }

    private void WrongPointMarked(GameObject droppedItem)
    {
        //UpdateScoreIncorrectAnswer();
        ShowMistakeFeedback("incorrectPlacement");
    }

    private void CheckWinCondition()
    {
        canFinishActivity = _placedPoints == dataPoints.Count;
        if (canFinishActivity)
        {
            EndActivityScoreCheck();
            onActivityCompleted.Invoke(_maxScore);
        }
    }

    private string GetFormattedDate(double dateNumber)
    {
        DateTime date = ChartDateUtility.ValueToDate(dateNumber);
        var format = graphDataFiller.DateDisplayFormat;
        var formattedDate = ChartDateUtility.DateToDateTimeString(date, (inputDate) => { return inputDate.ToString(format); });
        return formattedDate;
    }
    
    private void SpawnDataPointSourceSlots()
    {
        // Reverse during adding process to add as first sibling in order
        dataPoints.Reverse();
        foreach (var dataPoint in dataPoints)
        {
            var newPanel = Instantiate(dataPointPanel, dataPointPanel.transform.parent);
            newPanel.transform.SetAsFirstSibling();
            var xValueLabel = newPanel.transform.Find("XValue").GetComponent<TextMeshProUGUI>();
            var yValueLabel = newPanel.transform.Find("YValue").GetComponent<TextMeshProUGUI>();

            xValueLabel.text = "Tag: " + GetFormattedDate(dataPoint.x);
            yValueLabel.text = "Temperatur: " + dataPoint.y + "\u00b0C";
            newPanel.SetActive(true);
        }
        dataPoints.Reverse();
        dataPointPanel.SetActive(false);
    }
    
    private void SpawnDataPointTargetSlots()
    {
        for (int i = 0; i < dataWorldPositions.Count; i++)
        {
            var dataWorldPosition = dataWorldPositions[i];
            var parent = graph.transform;
            var dataPointSlot = Instantiate(dataPointTargetSlot, dataWorldPosition, Quaternion.identity, parent);

            var interactionPanel = dataPointPanel.transform.parent;
            var sourcePanel = interactionPanel.GetChild(i);
            var draggablePoint = sourcePanel.Find("DataPointFrame").transform.Find("DataPointSourceSlot").transform.Find("DraggableDataPoint").gameObject;
            SetUpPointSlotListeners(dataPointSlot);
            dataPointSlot.GetComponent<DraggableItemSlot>().acceptedItem = draggablePoint;
            dataPointSlot.SetActive(true);
        }
        dataPointTargetSlot.SetActive(false);
    }
    
    private List<Vector3> GetDataWorldPositions()
    {
        List<Vector3> worldPositions = new();
        
        foreach (var dataPoint in dataPoints)
        {
            Vector3 position;
            graph.PointToWorldSpace(out position, dataPoint.x, dataPoint.y);
            worldPositions.Add(position);
        }

        return worldPositions;
    }

    private GraphDataFiller.CategoryData GetCategory()
    {
        var categories = graphDataFiller.Categories;
        return categories[0];
    }
    
    private List<DoubleVector3> GetChartData()
    {
        
        List<DoubleVector3> dataPoints = new List<DoubleVector3>();
        var category = GetCategory();
        dataPoints = graph.DataSource.GetPoints(category.Name);

        return dataPoints;
    }

    private void SetUpGraph()
    {
        graphDataFiller.GraphObject.gameObject.SetActive(true);
        graphDataFiller.Fill();
    }
    
    public override void ActivitySpecificCleanup()
    {
        graphDataFiller.GraphObject.gameObject.SetActive(false);
        
        if (infoBox != null)
        {
            HideAIFeedbackBox();
        }
    }
}
