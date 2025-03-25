using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class IntroductionActivityManager : ActivityManager
{
    protected override void ActivitySpecificSetUp()
    {
        AnswersStart();
        SetUpGraph();
        SetUpActivityListeners();
    }
    
    protected virtual void SetUpGraph()
    {
        graphDataFiller.GraphObject.gameObject.SetActive(true);
        graphDataFiller.Fill();
    }

    protected void SetUpActivityListeners()
    {
        onAllInfoPagesSeen.AddListener(CheckWinCondition);
    }

    private void CheckWinCondition()
    {
        canFinishActivity = true;
        var maxPossibleScore = scoreBasePerUnit;
        onScoreChanged.Invoke(scoreBasePerUnit);
        onActivityCompleted.Invoke(maxPossibleScore);
    }

    public override void ActivitySpecificCleanup()
    {
        graphDataFiller.GraphObject.gameObject.SetActive(false);
    }

    private void AnswersStart()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "answersLog.txt");
        string logEntry = " === LINE CHART === ";
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // Write to file, appending each entry
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"{timestamp}");
            writer.WriteLine(logEntry);

        }
    }
    
}
