using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AxesActivityManager : ActivityManager
{
    public GameObject xAxisLabelSlot;
    public GameObject yAxisLabelSlot;

    public GameObject xAxisLabel;
    public GameObject yAxisLabel;

    private bool _hasCorrectXLabel = false;
    private bool _hasCorrectYLabel = false;
    private int _maxScore;
    private int _answersCount = 2;
    
    protected override void ActivitySpecificSetUp()
    {
        SetUpActivityListeners();
        SetUpGameObjects();
        SetUpGraph();
        _maxScore = CalculateMaxScore();
    }

    private int CalculateMaxScore()
    {
        Debug.Log($"{scoreBasePerUnit} (score base per unit)  and {scoreTimeBonus} (score time bonus) and {scoreCorrectAnswerBonus * _answersCount} (answer bonus * answers count) and {_answersCount} (answers count) max score for axes.");
        return scoreBasePerUnit + scoreTimeBonus + scoreCorrectAnswerBonus * _answersCount;
    }

    public override void ActivitySpecificCleanup()
    {
        
        xAxisLabel.SetActive(false);
        yAxisLabel.SetActive(false);
        
        xAxisLabelSlot.SetActive(false);
        yAxisLabelSlot.SetActive(false);
        
        graphDataFiller.GraphObject.gameObject.SetActive(false);
        
        if (infoBox != null)
        {
            HideAIFeedbackBox();
        }
    }

    private void SetUpGameObjects()
    {
        xAxisLabel.SetActive(true);
        yAxisLabel.SetActive(true);
        
        xAxisLabelSlot.SetActive(true);
        yAxisLabelSlot.SetActive(true);
    }

    private void SetUpGraph()
    {
        graphDataFiller.GraphObject.gameObject.SetActive(true);
        graphDataFiller.Fill();
    }
    
    private void SetUpActivityListeners()
    {
        var yAxisDraggableItem = yAxisLabelSlot.transform.Find("DraggableYAxisLabelSlot").GetComponent<DraggableItemSlot>();
        var xAxisDraggableItem = xAxisLabelSlot.transform.Find("DraggableXAxisLabelSlot").GetComponent<DraggableItemSlot>();

        xAxisDraggableItem.correctItemDropped.AddListener(CorrectXLabelMarked);
        xAxisDraggableItem.wrongItemDropped.AddListener(WrongXLabelMarked);
        yAxisDraggableItem.correctItemDropped.AddListener(CorrectYLabelMarked);
        yAxisDraggableItem.wrongItemDropped.AddListener(WrongYLabelMarked);
    }
    
    private void WrongXLabelMarked()
    {
        UpdateScoreIncorrectAnswer();
        LogAnswer("xAxisMisidentified", false);
        ShowMistakeFeedback("xAxisMisidentified");
    }

    private void WrongYLabelMarked()
    {
        UpdateScoreCorrectAnswer();
        LogAnswer("yAxisMisidentified", false);
        ShowMistakeFeedback("yAxisMisidentified");
    }

    private void CorrectXLabelMarked()
    {
        _hasCorrectXLabel = true;
        UpdateScoreCorrectAnswer();
        CheckWinCondition();
        LogAnswer("xAxisIdentified", true);
        ShowSuccessFeedback("xAxisIdentified");
    }

    private void CorrectYLabelMarked()
    {
        _hasCorrectYLabel = true;
        UpdateScoreCorrectAnswer();
        CheckWinCondition();
        LogAnswer("yAxisIdentified", true);
        ShowSuccessFeedback("yAxisIdentified");
    }
    
    protected void CheckWinCondition()
    {
        canFinishActivity = _hasCorrectXLabel && _hasCorrectYLabel;
        if (canFinishActivity)
        {
            EndActivityScoreCheck();
            onActivityCompleted.Invoke(_maxScore);
        }
    }

    private void LogAnswer(string questionName, bool isCorrect)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "answersLog.txt");
        string logEntry = $"{System.DateTime.Now}: Question '{questionName}' answered. Correct: {isCorrect}\n";
        // Write to file, appending each entry
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(logEntry);
        }
    }

    private void EnableFinishButton()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
