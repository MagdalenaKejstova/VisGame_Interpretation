using System;
using System.Collections;
using System.Collections.Generic;
using ChartAndGraph;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LayerIdentificationActivity : IntroductionActivityManager
{
    public GameObject categoryLabelTemplate;
    public GameObject cycleColorSchemeButton;
    public GameObject categoryVisualizer;
    
    [Tooltip("Name for the dataset to be displayed on the label")]
    public string graphNameLabelText = "Example dataset";
    
    public TextMeshProUGUI graphNameLabel;

    public string category1Name;
    
    [Tooltip("Colors used to represent category an unsuitable way")]
    public List<Material> category1Colors;

    public string category2Name;
    
    [Tooltip("Colors used to represent category an unsuitable way")]
    public List<Material> category2Colors;

    public string category3Name;

    [Tooltip("Colors used to represent category an unsuitable way")]
    public List<Material> category3Colors;

    private GraphChartBase _graphStylePrefab;

    private Dictionary<int, string> _colorSchemeFeedbackNames;

    private List<GameObject> _categoryLabels = new();
    private int _currentColorSchemeIndex = 0;
    
    private CategoryHighlighter _categoryHighlighter;

    protected override void ActivitySpecificSetUp()
    {
        SetUpGraph();
        SetUpLabel();
        InitializeCategoryHighlighting();
        SetUpCategoryLabels();
        SetUpColorScheme();
        cycleColorSchemeButton.gameObject.SetActive(true);
    }

    private void InitializeCategoryHighlighting()
    {
        _categoryHighlighter = graphDataFiller.GraphObject.gameObject.GetComponent<CategoryHighlighter>();
        _categoryHighlighter.Initialize(graphDataFiller, categoryVisualizer, _graphStylePrefab);
    }

    protected override void SetUpGraph()
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

    private void SetUpColorScheme()
    {
        _colorSchemeFeedbackNames = new Dictionary<int, string>
        {
            {0, "correctScheme"},
            {1, "singleColorScheme"},
            {2, "biasedColorScheme"},
        };
        
        CycleColorScheme(false);
    }
    
    public void CycleColorScheme(bool showFeedback = true)
    {
        SetColorScheme(showFeedback);
        SetCategoryLabelColors();
        
        var colorSchemesCount = _colorSchemeFeedbackNames.Count;
        _currentColorSchemeIndex++;
        if (_currentColorSchemeIndex == colorSchemesCount)
        {
            _currentColorSchemeIndex = 0;
        }

        // Player has seen all color schemes
        if (_currentColorSchemeIndex == 0)
        {
            CheckWinCondition();
        }
    }

    private void SetColorScheme(bool showFeedback = true)
    {
        var index = _currentColorSchemeIndex;
        var color1 = category1Colors[index];
        var color2 = category2Colors[index];
        var color3 = category3Colors[index];
        
        _graphStylePrefab.DataSource.SetCategoryFill(category1Name, color1, false);
        _graphStylePrefab.DataSource.SetCategoryFill(category2Name, color2, false);
        _graphStylePrefab.DataSource.SetCategoryFill(category3Name, color3, false);

        graphDataFiller.Fill();
        _categoryHighlighter.UpdateVisualizedColor();
        
        if (showFeedback)
        {
            var feedbackName = _colorSchemeFeedbackNames[index];
            var isCorrectScheme = index == 0;

            if (isCorrectScheme)
            {
                ShowSuccessFeedback(feedbackName);
            }
            else
            {
                ShowMistakeFeedback(feedbackName);
            }
        }
    }

    private void SetUpCategoryLabels()
    {
        InstantiateCategoryLabel(category1Name);
        InstantiateCategoryLabel(category2Name);
        InstantiateCategoryLabel(category3Name);
        
        SetCategoryLabelColors(0);
        categoryLabelTemplate.gameObject.SetActive(false);
    }

    private void InstantiateCategoryLabel(string categoryName)
    {
        var newLabel = Instantiate(categoryLabelTemplate, categoryLabelTemplate.transform.parent);
        newLabel.transform.SetAsLastSibling();
        newLabel.GetComponentInChildren<TextMeshProUGUI>().text = categoryName;
        
        newLabel.gameObject.SetActive(true);

        _categoryLabels.Add(newLabel);
    }

    private void SetCategoryLabelColors()
    {
        SetCategoryLabelColors(_currentColorSchemeIndex);
    }

    private void SetCategoryLabelColors(int colorSchemeIndex)
    {
        for (var i = 0; i < _categoryLabels.Count; i++)
        {
            var label = _categoryLabels[i];
            var colorLabel = label.transform.Find("CategoryColor").gameObject;
            var panelImage = colorLabel.GetComponent<Image>();

            switch (i)
            {
                case 0:
                    panelImage.color = category1Colors[colorSchemeIndex].color;
                    break;
                case 1:
                    panelImage.color = category2Colors[colorSchemeIndex].color;
                    break;
                case 2:
                    panelImage.color = category3Colors[colorSchemeIndex].color;
                    break;
            }
        }
    }

    private void CheckWinCondition()
    {
        canFinishActivity = true;
        onScoreChanged.Invoke(scoreBasePerUnit);
        var maxPossibleScore = scoreBasePerUnit;
        onActivityCompleted.Invoke(maxPossibleScore);
    }
    
    
    public override void ActivitySpecificCleanup()
    {
        var graph = graphDataFiller.GraphObject.gameObject;
        graph.transform.parent.gameObject.SetActive(false);
        
        if (cycleColorSchemeButton != null)
        {
            cycleColorSchemeButton.gameObject.SetActive(false);
        }

        if (categoryVisualizer != null)
        {
            categoryVisualizer.gameObject.SetActive(false);
        }
        
        foreach (var label in _categoryLabels)
        {
            label.gameObject.SetActive(false);
        }
    }
}
