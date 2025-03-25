using System.Collections;
using System.Collections.Generic;
using ChartAndGraph;
using UnityEngine;

public abstract class LineActivityManagerBase : ActivityManager
{
    public GameObject anchorDataPointTemplate;
    public int pointSize = 50;
    protected GraphChartBase Graph;
    protected ConnectionManager ConnectionManager;
    protected List<Vector3> DataWorldPositions = new();
    protected List<DoubleVector3> DataPoints = new();

    protected abstract override void ActivitySpecificSetUp();

    protected abstract void SetUpActivityListeners();

    protected abstract void SetUpConnectionManager();

    protected abstract void SetUpGameObjects();
    protected abstract void CheckWinCondition();

    protected List<Vector3> GetDataWorldPositions()
    {
        List<Vector3> worldPositions = new();

        foreach (var dataPoint in DataPoints)
        {
            //Debug.Log(dataPoint);
            Vector3 position;
            Graph.PointToWorldSpace(out position, dataPoint.x, dataPoint.y);
            worldPositions.Add(position);
        }

        return worldPositions;
    }

    protected GraphDataFiller.CategoryData GetCategory()
    {
        var categories = graphDataFiller.Categories;
        return categories[0];
    }

    protected List<DoubleVector3> GetChartData()
    {
        // List<DoubleVector3> dataPoints = new List<DoubleVector3>();
        // var category = GetCategory();
        // dataPoints = Graph.DataSource.GetPoints(category.Name);
        

        // return dataPoints;

        List<DoubleVector3> dataPoints = new List<DoubleVector3>();
    var categories = graphDataFiller.Categories;

    // Collect data points for each category (region)
    foreach (var category in categories)
    {
        List<DoubleVector3> categoryPoints = Graph.DataSource.GetPoints(category.Name);
        dataPoints.AddRange(categoryPoints);
    }

    return dataPoints;
    }

    protected void SetUpGraph()
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