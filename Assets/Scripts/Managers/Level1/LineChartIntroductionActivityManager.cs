using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChartAndGraph;
using UnityEngine;

public class LineChartIntroductionActivityManager : IntroductionActivityManager
{
    public float flashSpeed = 4;
    public float flashDuration = 30;
    
    [Tooltip("Color to flash when emphasizing points")]
    public Material pointFlashColor;
    private const string PointFlashInfoChunkName = "dataPoints";
    
    [Tooltip("Color to flash when emphasizing line")]
    public Material lineFlashColor;
    private const string LineFlashInfoChunkName = "line";

    [Tooltip("Color to flash when emphasizing xAxis")]
    public Material xAxisFlashColor;
    private const string XAxisInfoChunkName = "xAxis";

    [Tooltip("Color to flash when emphasizing yAxis")]
    public Material yAxisFlashColor;
    private const string YAxisInfoChunkName = "yAxis";

    private GraphData.CategoryData _originalCategoryStyle;
    private GraphChartBase _graphStylePrefab;
    private string _currentFlashId = "";
    
    protected override void ActivitySpecificSetUp()
    {
        base.ActivitySpecificSetUp();
        SetUpStyleSettings();
    }

    private void SetUpStyleSettings()
    {
        _graphStylePrefab = graphDataFiller.CategoryPrefab;
        var categoryNames = graphDataFiller.GraphObject.DataSource.CategoryNames;
        var categoryName = categoryNames.First();
        
        if (!string.IsNullOrEmpty(categoryName))
        {
            _originalCategoryStyle =_originalCategoryStyle = graphDataFiller.GraphObject.DataSource.GetCategoryData(categoryName);
        }
    }
    
    protected override void SetInfoBoxPage(int pageIndex)
    {
        var questionName = GetDisplayedPageName(pageIndex);
        switch (questionName)
        {
            case PointFlashInfoChunkName:
                StartCoroutine(FlashPointForSeconds(flashDuration, flashSpeed));
                break;
            case LineFlashInfoChunkName:
                StartCoroutine(FlashLineForSeconds(flashDuration, flashSpeed));
                break;
            case XAxisInfoChunkName:
                StartCoroutine(FlashXAxisForSeconds(flashDuration, flashSpeed));
                break;
            case YAxisInfoChunkName:
                StartCoroutine(FlashYAxisForSeconds(flashDuration, flashSpeed));
                break;
        }

        base.SetInfoBoxPage(pageIndex);
    }

    private IEnumerator FlashPointForSeconds(float duration, float speed = 10f)
    {
        bool stopFlashing = false;
        const string flashId = "point";
        _currentFlashId = flashId;
        
        var startTime = Time.time;
        
        var categoryName = _originalCategoryStyle.Name;
        var pointSize = _originalCategoryStyle.PointSize;
        var pointMaterial = _originalCategoryStyle.PointMaterial;
        
        // Flash color
        while (Time.time - startTime < duration && !stopFlashing)
        {
            var newMaterial = new Material(pointMaterial);
            var newColor = Color.Lerp(pointFlashColor.color, pointMaterial.GetColor("_ColorFrom"), Mathf.Sin(Time.time * speed));
            newMaterial.SetColor("_ColorFrom", newColor);
            newMaterial.SetColor("_ColorTo", newColor);
            
            _graphStylePrefab.DataSource.SetCategoryPoint(categoryName, newMaterial, pointSize);
            graphDataFiller.UpdateVisualStyles();
            stopFlashing = _currentFlashId != flashId;
            yield return null;
        }

        // restore original color
        _graphStylePrefab.DataSource.SetCategoryPoint(categoryName, pointMaterial, pointSize);
        graphDataFiller.UpdateVisualStyles();
    }
    
    private IEnumerator FlashLineForSeconds(float duration, float speed = 10f)
    {
        bool stopFlashing = false;
        const string flashId = "line";
        _currentFlashId = flashId;
        var startTime = Time.time;
        
        var categoryName = _originalCategoryStyle.Name;
        var lineThickness = _originalCategoryStyle.LineThickness;
        var lineMaterial = _originalCategoryStyle.LineMaterial;
        var lineTiling = _originalCategoryStyle.LineTiling;
        
        // Flash color
        while (Time.time - startTime < duration && !stopFlashing)
        {
            var newMaterial = new Material(lineMaterial);
            var newColor = Color.Lerp(lineFlashColor.color, lineMaterial.color, Mathf.Sin(Time.time * speed));
            newMaterial.color = newColor;
            
            _graphStylePrefab.DataSource.SetCategoryLine(categoryName, newMaterial, lineThickness, lineTiling);
            graphDataFiller.UpdateVisualStyles();
            stopFlashing = _currentFlashId != flashId;
            yield return null;
        }

        // restore original color
        _graphStylePrefab.DataSource.SetCategoryLine(categoryName, lineMaterial, lineThickness, lineTiling);
        graphDataFiller.UpdateVisualStyles();
    }
    
    private IEnumerator FlashXAxisForSeconds(float duration, float speed = 10f)
    {
        bool stopFlashing = false;
        const string flashId = "xAxis";
        _currentFlashId = flashId;

        var startTime = Time.time;

        // Get the original color of the x-axis
        var originalColor = graphDataFiller.GetAxisColor(Axis.X);

        // Flash color
        while (Time.time - startTime < duration && !stopFlashing)
        {
            var newColor = Color.Lerp(xAxisFlashColor.color, originalColor, Mathf.Sin(Time.time * speed));

            // Apply the new color to the x-axis
            graphDataFiller.SetAxisColor(Axis.X, newColor);
            graphDataFiller.UpdateVisualStyles();

            stopFlashing = _currentFlashId != flashId;
            yield return null;
        }

        // Restore the original color when done flashing
        graphDataFiller.SetAxisColor(Axis.X, originalColor);
        graphDataFiller.UpdateVisualStyles();
    }
    
    private IEnumerator FlashYAxisForSeconds(float duration, float speed = 10f)
    {
        bool stopFlashing = false;
        const string flashId = "yAxis";
        _currentFlashId = flashId;

        var startTime = Time.time;

        // Get the original color of the y-axis
        var originalColor = graphDataFiller.GetAxisColor(Axis.Y);

        // Flash color
        while (Time.time - startTime < duration && !stopFlashing)
        {
            var newColor = Color.Lerp(yAxisFlashColor.color, originalColor, Mathf.Sin(Time.time * speed));

            // Apply the new color to the y-axis
            graphDataFiller.SetAxisColor(Axis.Y, newColor);
            graphDataFiller.UpdateVisualStyles();

            stopFlashing = _currentFlashId != flashId;
            yield return null;
        }

        // Restore the original color when done flashing
        graphDataFiller.SetAxisColor(Axis.Y, originalColor);
        graphDataFiller.UpdateVisualStyles();
    }
    

    private string GetDisplayedPageName(int pageIndex)
    {
        var infoChunks = ActivityData.informationChunks;
        int browsedIndex = 0;
        foreach (DictionaryEntry infoChunk in infoChunks)
        {
            if (browsedIndex == pageIndex)
            {
                return (string)infoChunk.Key;
            }

            browsedIndex++;
        }

        return null;
    }
}
