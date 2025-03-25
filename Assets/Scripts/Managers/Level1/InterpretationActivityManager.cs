using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public enum SeekedFeature
{
    Max,
    Min,
    TrendUp,
    TrendDown
}

public class InterpretationActivityManager : LineDrawingActivityManager
{
    public Sprite maxPointMarker;
    public Sprite minPointMarker;

    public Material trendUpMaterial;
    public Material trendDownMaterial;

    public LineRenderer lineRenderer;

    private SeekedFeature _seekedFeature;
    private double _min;
    private double _max;
    private List<GameObject> _anchorDataPoints = new();
    private List<Trend> _trends = new();

    protected override void ActivitySpecificSetUp()
    {
        SetUpGraph();
        Graph = graphDataFiller.GraphObject;
        DataPoints = GetChartData();
        DataWorldPositions = GetDataWorldPositions();
        IsLockedInfoBox = true;

        FindTrends();
        SetUpGameObjects();
        SetUpConnectionManager();
        SetUpActivityListeners();
        SetUpExtremes();
        SetUpFirstAssignment();
        _maxScore = CalculateMaxScore();
    }
    
    private int CalculateMaxScore()
    {
        // find min, find max + find trends
        var answersCount = _trends.Count + 2;
        Debug.Log($"{scoreBasePerUnit} (score base per unit) and {scoreCorrectAnswerBonus * answersCount} (answer bonus * answers count) and {answersCount} (answers count) is max score for interpretation.");
        return scoreBasePerUnit + scoreTimeBonus + scoreCorrectAnswerBonus * answersCount;
    }
    
    private void FindTrends()
    {
        int trendStart = 1;
        int trendEnd = 1;
        double previousVal = 0;
        Tendency tendency = Tendency.Up;

        for (int i = 0; i < DataPoints.Count; i++)
        {
            var currentVal = DataPoints[i].y;
            if (i == 0)
            {
                previousVal = currentVal;
                continue;
            }

            if (currentVal > previousVal)
            {
                if (i == 1)
                {
                    tendency = Tendency.Up;
                }

                switch (tendency)
                {
                    case Tendency.Down:
                        if (trendEnd - trendStart > 1)
                        {
                            _trends.Add(new Trend(tendency, trendStart, trendEnd));
                        }
                        tendency = Tendency.Up;
                        trendStart = trendEnd;
                        trendEnd++;
                        break;
                    case Tendency.Up:
                        trendEnd = i + 1;
                        break;
                }
            }
            else if (currentVal < previousVal)
            {
                if (i == 1)
                {
                    tendency = Tendency.Down;
                }

                switch (tendency)
                {
                    case Tendency.Down:
                        trendEnd = i + 1;
                        break;
                    case Tendency.Up:
                        if (trendEnd - trendStart > 1)
                        {
                            _trends.Add(new Trend(tendency, trendStart, trendEnd));
                        }
                        tendency = Tendency.Down;
                        trendStart = trendEnd;
                        trendEnd++;
                        break;
                }
            }

            previousVal = currentVal;
        }
        if (trendEnd - trendStart > 1)
        {
            _trends.Add(new Trend(tendency, trendStart, trendEnd));
        }
    }

    private void SetUpFirstAssignment()
    {
        _seekedFeature = SeekedFeature.Max;
        SetInfoBoxPage("findMaxAssignment");
    }

    private void SetUpExtremes()
    {
        var yValues = DataPoints.Select(point => point.y);
        _max = yValues.Max();
        _min = yValues.Min();
    }

    protected override void SetUpActivityListeners()
    {
        ConnectionManager.correctConnectionCreated.AddListener(CorrectConnectionCreated);
        
        ConnectionManager.incompleteConnectionCreated.AddListener(IncompleteConnectionCreated);
        ConnectionManager.unorderedConnectionCreated.AddListener(UnorderedConnectionCreated);
        ConnectionManager.unexpectedStartConnectionCreated.AddListener(UnexpectedStartConnectionCreated);
        ConnectionManager.incorrectTendencyConnectionCreated.AddListener(IncorrectTendencyConnectionCreated);
        ConnectionManager.tooShortTrendCreated.AddListener(TooShortTrendConnectionCreated);
    }

    protected override void SetUpConnectionManager()
    {
        ConnectionManager = GetComponent<ConnectionManager>();
        ConnectionManager.maxConnectionSegments = _trends.Count;
        ConnectionManager.SetObjectsToConnect(DataPoints.Count);
        ConnectionManager.checkTendency = true;
        ConnectionManager.Disable();
    }

    protected override void SetUpGameObjects()
    {
        SpawnAnchorDataPoints();
    }

    private void SpawnAnchorDataPoints()
    {
        for (int i = 0; i < DataWorldPositions.Count; i++)
        {
            var dataWorldPosition = DataWorldPositions[i];
            dataWorldPosition.z = 0;
            var parent = Graph.transform;
            var anchorDataPoint = Instantiate(anchorDataPointTemplate, dataWorldPosition, Quaternion.identity, parent);

            anchorDataPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(pointSize, pointSize);
            anchorDataPoint.GetComponent<SphereCollider>().radius = (float)pointSize * 2;

            var anchor = anchorDataPoint.GetComponent<Anchor>();
            var anchorIndex = i + 1;
            anchor.order = anchorIndex;
            anchor.value = DataPoints[i].y;
            anchor.expectedStartIndex = FindExpectedStartIndex(anchorIndex);
            anchor.expectedObjectsToConnectCount = FindExpectedObjectsToConnectCount(anchorIndex);
            anchor.possibleObjectsToConnectCount = DataPoints.Count;
            anchor.currentTendency = FindCurrentTendency(anchorIndex);
            anchor.anchorClicked.AddListener(CheckClickResult);
            anchor.lineRenderer = Instantiate(lineRenderer);
            anchorDataPoint.SetActive(true);
            _anchorDataPoints.Add(anchorDataPoint);
        }

        anchorDataPointTemplate.SetActive(false);
    }

    private Trend FindMemberTrend(int anchorIndex)
    {
        var lastAnchorIndex = _trends.Last().EndIndex;
        var memberTrend = _trends.Find(trend => anchorIndex >= trend.StartIndex && (anchorIndex < trend.EndIndex || anchorIndex == lastAnchorIndex));
        return memberTrend;
    }
    
    private int FindExpectedStartIndex(int anchorIndex)
    {
        var memberTrend = FindMemberTrend(anchorIndex);
        return memberTrend?.StartIndex ?? -1;
    }
    
    private int FindExpectedObjectsToConnectCount(int anchorIndex)
    {
        var memberTrend = FindMemberTrend(anchorIndex);
        var endIndex = memberTrend?.EndIndex ?? -2;
        var startIndex = memberTrend?.StartIndex ?? 0;
        return  endIndex - startIndex + 1;
    }

    private Tendency FindCurrentTendency(int anchorIndex)
    {
        var memberTrend = FindMemberTrend(anchorIndex);
        return memberTrend?.Tendency ?? Tendency.Undefined;
    }

    private void CheckClickResult(Anchor.AnchorInfo anchorInfo)
    {
        var clickedPoint = _anchorDataPoints[anchorInfo.index];
        var clickedAnchor = clickedPoint.GetComponent<Anchor>();
        switch (_seekedFeature)
        {
            case SeekedFeature.Max:
                if (clickedAnchor.value == _max)
                {
                    MaxIdentified(clickedPoint);
                }
                else
                {
                    MaxMisidentified();
                }

                break;
            case SeekedFeature.Min:
                if (clickedAnchor.value == _min)
                {
                    MinIdentified(clickedPoint);
                }
                else
                {
                    MinMisidentified();
                }

                break;
        }
    }

    private void MaxIdentified(GameObject maxPoint)
    {
        LogAnswer("Find Max", true);
        maxPoint.GetComponent<Image>().sprite = maxPointMarker;

        _seekedFeature = SeekedFeature.Min;
        SetInfoBoxPage("findMinAssignment");
        UpdateScoreCorrectAnswer();
        ShowSuccessFeedback("maxIdentified");
    }

    private void MaxMisidentified()
    {
        LogAnswer("Find Max", false);
        UpdateScoreIncorrectAnswer();
        ShowMistakeFeedback("maxMisidentified");
    }

    private void MinIdentified(GameObject minPoint)
    {
        LogAnswer("Find Min", true);
        minPoint.GetComponent<Image>().sprite = minPointMarker;

        _seekedFeature = SeekedFeature.TrendUp;
        ConnectionManager.targetTendency = Tendency.Up;
        ConnectionManager.Enable();
        SetInfoBoxPage("findTrendUpAssignment");
        UpdateScoreCorrectAnswer();
        ShowSuccessFeedback("minIdentified");
    }

    private void MinMisidentified()
    {
        LogAnswer("Find Min", false);
        UpdateScoreIncorrectAnswer();
        ShowMistakeFeedback("minMisidentified");
    }

    private void TrendUpIdentified(int startAnchorIndex)
    {
        LogAnswer("Trend Up", true);
        var trendStartAnchor = _anchorDataPoints[startAnchorIndex - 1].GetComponent<Anchor>();
        trendStartAnchor.lineRenderer.material = trendUpMaterial;
        
        UpdateScoreCorrectAnswer();
        ShowSuccessFeedback("trendUpIdentified");
        
        if (_trends.Where(trend => trend.Tendency == Tendency.Up).All(upTrend => upTrend.Marked))
        {
            ConnectionManager.targetTendency = Tendency.Down;
            _seekedFeature = SeekedFeature.TrendDown;
            SetInfoBoxPage("findTrendDownAssignment");    
        }
    }
    
    private void TrendDownIdentified(int startAnchorIndex)
    {
        LogAnswer("Trend Down", true);
        var trendStartAnchor = _anchorDataPoints[startAnchorIndex - 1].GetComponent<Anchor>();
        trendStartAnchor.lineRenderer.material = trendDownMaterial;
        UpdateScoreCorrectAnswer();
        ShowSuccessFeedback("trendDownIdentified");
        CheckWinCondition();
    }

    private void TrendDownMisidentified()
    {
        LogAnswer("Trend Down", false);
        UpdateScoreIncorrectAnswer();
        ShowMistakeFeedback("trendDownMisidentified");
    }

    protected override void CheckWinCondition()
    {
        canFinishActivity = _trends.All(trend => trend.Marked);
        if (canFinishActivity)
        {
            EndActivityScoreCheck();
            onActivityCompleted.Invoke(_maxScore);
        }
    }

    private void CorrectConnectionCreated(int startAnchorIndex)
    {
        var trend = _trends.Find( trend => trend.StartIndex == startAnchorIndex);
        trend.Marked = true;
        switch (trend.Tendency)
        {
            case Tendency.Up:
                TrendUpIdentified(startAnchorIndex);
                break;
            case Tendency.Down:
                TrendDownIdentified(startAnchorIndex);
                break;
        }
    }
    
    private void IncompleteConnectionCreated()
    {
        LogAnswer("Incomplete Connection", false);
        UpdateScoreIncorrectAnswer();
        ShowMistakeFeedback("incompleteConnection");
    }

    private void UnorderedConnectionCreated()
    {
         LogAnswer("Unordered Connection", false);
        UpdateScoreIncorrectAnswer();
        ShowMistakeFeedback("unorderedConnection");
    }
    
    private void UnexpectedStartConnectionCreated()
    {
         LogAnswer("Unexpected Connection", false);
        UpdateScoreIncorrectAnswer();
        ShowMistakeFeedback("unexpectedStartConnection");
    }
    
    private void IncorrectTendencyConnectionCreated()
    {
         LogAnswer("Incorrect Tendency", false);
        UpdateScoreIncorrectAnswer();
        ShowMistakeFeedback("incorrectTendencyConnection");
    }
    
    private void TooShortTrendConnectionCreated()
    {
         LogAnswer("Too short trend", false);
        UpdateScoreIncorrectAnswer();
        ShowMistakeFeedback("tooShortTrendConnection");
    }
    
    public override void ActivitySpecificCleanup()
    {
        base.ActivitySpecificCleanup();

        if (graphDataFiller.GraphObject != null)
        {
            graphDataFiller.GraphObject.DataSource.Clear(); 
            graphDataFiller.GraphObject.gameObject.SetActive(false); 
        }

        foreach (var anchorDataPoint in _anchorDataPoints)
        {
            var anchor = anchorDataPoint.GetComponent<Anchor>();
            if (anchor.lineRenderer != null)
            {
                anchor.lineRenderer.positionCount = 0;  
            }
        }

        _anchorDataPoints.Clear();

        if (infoBox != null)
            {
                HideAIFeedbackBox();
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


}