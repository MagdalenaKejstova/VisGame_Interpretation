using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChartAndGraph;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableGraphArea : MonoBehaviour, IDragHandler
{
    public TextMeshProUGUI precipitationAmountVisualizer;
    public TextMeshProUGUI precipitationDateVisualizer;
    
    private bool _isInitialized;
    private Camera _canvasCamera;
    private GraphDataFiller _graphDataFiller;
    private GraphData _graph;
    
    private GraphData.CategoryData _lineCategoryData;
    private GraphData.CategoryData _areaCategoryData;
    private List<DoubleVector3> _referenceDataPoints = new();
    private List<DoubleVector3> _dynamicDataPoints = new();

    // Start is called before the first frame update
    private void Start()
    {
        _canvasCamera = GameObject.Find("CanvasCamera").GetComponent<Camera>();
    }

    public void Initialize(GraphDataFiller graphDataFiller, string lineCategoryName, string areaCategoryName)
    {
        _graphDataFiller = graphDataFiller;
        _graph = _graphDataFiller.GraphObject.DataSource;
        
        _lineCategoryData = GetCategory(lineCategoryName);
        _areaCategoryData = GetCategory(areaCategoryName);
        
        _referenceDataPoints = GetChartData(_lineCategoryData);
        _dynamicDataPoints = GetChartData(_areaCategoryData);

        _isInitialized = true;
    }
    
    private GraphData.CategoryData GetCategory(string categoryName)
    {
        var categoryData = _graph.GetCategoryData(categoryName);
        return categoryData;
    }
    
    private List<DoubleVector3> GetChartData(GraphData.CategoryData categoryData)
    {
        List<DoubleVector3> dataPoints = new List<DoubleVector3>();
        dataPoints = _graph.GetPoints(categoryData.Name);

        return dataPoints;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (_isInitialized)
        {
            var mousePosition = Input.mousePosition;
            var worldPosition = _canvasCamera.ScreenToWorldPoint(mousePosition);

            _graph.ClearCategory(_areaCategoryData.Name);

            double clickedPointX;
            double clickedPointY;
            _graphDataFiller.GraphObject.PointToClient(worldPosition, out clickedPointX, out clickedPointY);
            // Limit min selected value to 0
            clickedPointY = clickedPointY > 0 ? clickedPointY : 0;
            
            var initialDate = _referenceDataPoints[0].x;
            List<DoubleVector3> newDynamicDataPoints = new();
            for (int i = 0; i < _dynamicDataPoints.Count; i++)
            {
                var dynamicDataPoint = _dynamicDataPoints[i];
                var referenceDataPoint = _referenceDataPoints[i];

                if (clickedPointX >= initialDate)
                {
                    if (clickedPointX > dynamicDataPoint.x)
                    {
                        dynamicDataPoint.y = referenceDataPoint.y;
                    }
                    else if (clickedPointX <= dynamicDataPoint.x)
                    {
                        dynamicDataPoint.y = 0;
                    }
                }
                newDynamicDataPoints.Add(dynamicDataPoint);
                _graph.AddPointToCategory(_areaCategoryData.Name, dynamicDataPoint.x, dynamicDataPoint.y);
            }
            _dynamicDataPoints = newDynamicDataPoints;
            
            if (precipitationAmountVisualizer != null)
            {
                // Find closest graph value to current pointer horizontal position
                var closestDataPoint = _referenceDataPoints.OrderBy(point => Math.Abs(point.x - clickedPointX)).First();
                precipitationAmountVisualizer.text = $"Gesamtniederschlag:\n {closestDataPoint.y:F2} mm";
            }
            
            if (precipitationDateVisualizer != null)
            {
                // Find closest graph value to current pointer horizontal position
                var closestDataPoint = _referenceDataPoints.OrderBy(point => Math.Abs(point.x - clickedPointX)).First();
                var date = GetFormattedDate(closestDataPoint.x);
                precipitationDateVisualizer.text = $"Bis heute: {date}";
            } 
        }
    }
    
    private string GetFormattedDate(double dateNumber)
    {
        DateTime date = ChartDateUtility.ValueToDate(dateNumber);
        var format = _graphDataFiller.DateDisplayFormat;
        var formattedDate = ChartDateUtility.DateToDateTimeString(date, (inputDate) => { return inputDate.ToString(format); });
        return formattedDate;
    }
}