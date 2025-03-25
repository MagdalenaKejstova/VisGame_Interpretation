using System.Collections;
using System.Collections.Generic;
using ChartAndGraph;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StreamGraphInterpretationActivityManager : QuestionActivityManager
{
    public GameObject categoryLabelTemplate;
    public GameObject categoryVisualizer;
    
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
    
    private GraphChartBase _graphStylePrefab;

    private Dictionary<int, string> _colorSchemeFeedbackNames;

    private List<GameObject> _categoryLabels = new();
    
    private CategoryHighlighter _categoryHighlighter;

    protected override void ActivitySpecificSetUp()
    {
        base.ActivitySpecificSetUp();
        SetUpGraph();
        SetUpLabel();
        InitializeCategoryHighlighting();
        SetUpCategoryLabels();
        SetUpColorScheme();
        SetUpActivityListeners();
    }
    
    private void SetUpActivityListeners()
    {
        onAllQuestionsAnswered.AddListener(CheckWinCondition);
    }
    
    private void InitializeCategoryHighlighting()
    {
        _categoryHighlighter = graphDataFiller.GraphObject.gameObject.GetComponent<CategoryHighlighter>();
        _categoryHighlighter.Initialize(graphDataFiller, categoryVisualizer, _graphStylePrefab);
    }

    protected void SetUpGraph()
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
        SetColorScheme();
        SetCategoryLabelColors();
    }

    private void SetColorScheme()
    {
        _graphStylePrefab.DataSource.SetCategoryFill(category1Name, category1Color, false);
        _graphStylePrefab.DataSource.SetCategoryFill(category2Name, category2Color, false);
        _graphStylePrefab.DataSource.SetCategoryFill(category3Name, category3Color, false);
        _graphStylePrefab.DataSource.SetCategoryFill(category4Name, category4Color, false);
        _graphStylePrefab.DataSource.SetCategoryFill(category5Name, category5Color, false);
        
        graphDataFiller.Fill();
        _categoryHighlighter.UpdateVisualizedColor();
    }

    private void SetUpCategoryLabels()
    {
        InstantiateCategoryLabel(category1Name);
        InstantiateCategoryLabel(category2Name);
        InstantiateCategoryLabel(category3Name);
        InstantiateCategoryLabel(category4Name);
        InstantiateCategoryLabel(category5Name);
        
        SetCategoryLabelColors();
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
        for (var i = 0; i < _categoryLabels.Count; i++)
        {
            var label = _categoryLabels[i];
            var colorLabel = label.transform.Find("CategoryColor").gameObject;
            var panelImage = colorLabel.GetComponent<Image>();

            switch (i)
            {
                case 0:
                    panelImage.color = category1Color.color;
                    break;
                case 1:
                    panelImage.color = category2Color.color;
                    break;
                case 2:
                    panelImage.color = category3Color.color;
                    break;
                case 3:
                    panelImage.color = category4Color.color;
                    break;
                case 4:
                    panelImage.color = category5Color.color;
                    break;
            }
        }
    }
    
    private void CheckWinCondition()
    {
        canFinishActivity = true;
        Debug.Log("CheckWinCondition called, setting canFinishActivity to true.");
        Debug.Log(MaxScore);
        onActivityCompleted.Invoke(MaxScore);
    }
    
    public override void ActivitySpecificCleanup()
    {
        base.ActivitySpecificCleanup();

        var graph = graphDataFiller.GraphObject.gameObject;
        graph.transform.parent.gameObject.SetActive(false);
        
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
