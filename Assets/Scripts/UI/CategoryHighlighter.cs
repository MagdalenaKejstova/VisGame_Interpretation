using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ChartAndGraph;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class CategoryHighlighter : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public float nonHighlightedCategorySaturation = 0.25f;
    public TextMeshProUGUI labelVisualizer;
    public Image colorVisualizer;

    public TextMeshProUGUI amountVisualizer;
    public string amountName = "CO2";
    public string amountUnit = "mil. ton";
    public TextMeshProUGUI dateVisualizer;
    public string dateUnit = "Year";

    private bool _isInitialized;
    private Camera _canvasCamera;
    private GraphDataFiller _graphDataFiller;
    private GraphData _graph;
    private GameObject _categoryVisualizer;
    private GraphChartBase _graphStylePrefab;
    private string _highlightedCategory = "";
    private string _paddingCategoryName = "Padding";
    private Dictionary<string, Material> _originalCategoryFills = new();

    private void Awake()
    {
        _canvasCamera = GameObject.Find("CanvasCamera").GetComponent<Camera>();
    }

    public void Initialize(GraphDataFiller graphDataFiller, GameObject categoryVisualizer, GraphChartBase graphStylePrefab, string paddingCategoryName = "")
    {
        _graphDataFiller = graphDataFiller;
        _graph = _graphDataFiller.GraphObject.DataSource;
        _graphStylePrefab = graphStylePrefab;
        _categoryVisualizer = categoryVisualizer;
        _isInitialized = true;

        if (!string.IsNullOrEmpty(paddingCategoryName))
        {
            _paddingCategoryName = paddingCategoryName;
        }
        
        StoreOriginalCategoryFills();
    }

    private void StoreOriginalCategoryFills()
    {
        var categoryNames = _graphStylePrefab.DataSource.CategoryNames;
        foreach (var categoryName in categoryNames)
        {
            var categoryData = _graphStylePrefab.DataSource.GetCategoryData(categoryName);
            _originalCategoryFills[categoryData.Name] = categoryData.FillMaterial;
        }
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
        SetInfoPanelValues();
    }

    private string GetFormattedDate(double dateNumber)
    {
        DateTime date = ChartDateUtility.ValueToDate(dateNumber);
        var format = _graphDataFiller.DateDisplayFormat;
        var formattedDate =
            ChartDateUtility.DateToDateTimeString(date, (inputDate) => { return inputDate.ToString(format); });
        return formattedDate;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isInitialized)
        {
            SetInfoPanelValues(true);
        }
    }

    private void SetInfoPanelValues(bool changeHighlighting = false)
    {
        if (!_categoryVisualizer.activeSelf)
        {
            _categoryVisualizer.SetActive(true);
            _categoryVisualizer.transform.SetAsFirstSibling();
        }
        
        var mousePosition = Input.mousePosition;
        var worldPosition = _canvasCamera.ScreenToWorldPoint(mousePosition);

        double clickedPointX;
        double clickedPointY;
        _graphDataFiller.GraphObject.PointToClient(worldPosition, out clickedPointX, out clickedPointY);

        var closestRealX = FindClosestXRecord(clickedPointX);
        
        var closestRecord = GetClosestRecord(closestRealX, clickedPointY);
        
        if (closestRecord != null && closestRecord.Name != _paddingCategoryName)
        {
            DisplaySelectedData(closestRecord, closestRealX, changeHighlighting);
        }
    }
    
    private void DisplaySelectedData(StackedCategoryData categoryRecord, double closestRealX, bool changeHighlighting)
    {
        var closestDataPoint = GetClosestDataPoint(categoryRecord, closestRealX);
        // Show amount
        amountVisualizer.text = $"{amountName}: {closestDataPoint.y:F2} {amountUnit}";
        
        // Show date
        var formattedYear = GetFormattedDate(closestDataPoint.x);
        dateVisualizer.text = $"{dateUnit}: {formattedYear}";

        // Show category name
        var categoryName = categoryRecord.Name;
        labelVisualizer.text = categoryName;

        // Show category color
        colorVisualizer.color = _originalCategoryFills[categoryName].color;

        if (changeHighlighting)
        {
            ChangeHighlighting(categoryName);
        }
    }

    private void ChangeHighlighting(string categoryToHighlight)
    {
        _highlightedCategory = categoryToHighlight == _highlightedCategory ? "" : categoryToHighlight;
        var isHighlightingOn = !string.IsNullOrEmpty(_highlightedCategory);
        
        var categoryNames = _graphStylePrefab.DataSource.CategoryNames;
        foreach (var categoryName in categoryNames)
        {
            var currentFill = _originalCategoryFills[categoryName];
            var newFill = new Material(currentFill);
            
            var isAlreadyHighlighted = categoryToHighlight == _highlightedCategory; 
            // Restore original color
            if ((isHighlightingOn && categoryName == _highlightedCategory) || !isHighlightingOn)
            {
                newFill = _originalCategoryFills[categoryName];
            }
            // Desaturate original color
            else
            {
                var currentColor = new Color(currentFill.color.r, currentFill.color.g, currentFill.color.b);
                var newColor = AdjustSaturation(currentColor);
                newFill.color = newColor;
            }
            _graphStylePrefab.DataSource.SetCategoryFill(categoryName, newFill, false);
        }
        _graphDataFiller.Fill();
    }
    
    private Color AdjustSaturation(Color color, float adjustment)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp01(s - adjustment);
        return Color.HSVToRGB(h, s, v);
    }
    
    private Color AdjustSaturation(Color color)
    {
        return AdjustSaturation(color, nonHighlightedCategorySaturation);
    }
    
    public void UpdateVisualizedColor()
    {
        var categoryName = labelVisualizer.text;
        var category = GetCategory(categoryName);
        if (category != null)
        {
            var categoryColor = category.FillMaterial.color;
            colorVisualizer.color = categoryColor;
        }
        StoreOriginalCategoryFills();
    }
    
    private DoubleVector2 GetClosestDataPoint(StackedCategoryData categoryRecord, double closestRealX)
    {
        var realXDate = GetFormattedDate(closestRealX);
        var originalPoint = categoryRecord.DataPoints.Find(dataPoint => GetFormattedDate(dataPoint.x) == realXDate);
        return originalPoint;
    }

    private StackedCategoryData GetClosestRecord(double closestRealX,double clickedPointY)
    {
        var closestRecord = FindClosestStackedRecord(closestRealX, clickedPointY);
        
        return closestRecord;
    }

    private double FindClosestXRecord(double freeValueX)
    {
        // No matter which category is selected here, all have the same X values 
        var fstCategoryName = _graph.CategoryNames.First();
        var fstCategory = GetCategory(fstCategoryName);
        var fstCategoryXValues = GetChartData(fstCategory).Select(dataPoint => dataPoint.x);

        var closestRealX = fstCategoryXValues.OrderBy(graphX => Math.Abs(graphX - freeValueX)).First();
        
        return closestRealX;
    }

    private StackedCategoryData FindClosestStackedRecord(double realX, double freeValueY)
    {
        _graphDataFiller.stackedCategories.OrderBy(cat => cat.StackedDataPoints);

        double yValueDiff = double.MaxValue;
        StackedCategoryData closestRecord = null;
        double previousStackedY = 0;
        
        var realXDate = GetFormattedDate(realX);
        for (int i = 0; i < _graphDataFiller.stackedCategories.Count; i++)
        {
            var stackedCategory = _graphDataFiller.stackedCategories[i];
            var dataPoint = stackedCategory.StackedDataPoints.Find(cat => GetFormattedDate(cat.x) == realXDate);
            var currentStackedY = dataPoint.y;
            
            if (freeValueY <= currentStackedY && freeValueY > previousStackedY)
            {
                closestRecord = stackedCategory;
            }
            previousStackedY = currentStackedY;
        }

        return closestRecord;
    }

}
