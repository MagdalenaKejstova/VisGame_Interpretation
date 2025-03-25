using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.IO;


public class LevelManager : MonoBehaviour
{
    public AnimationProperties animationProperties;
    
    public List<ActivityManager> activityManagers;
    public string levelName;
    public ProgressBar progressBar;
    [FormerlySerializedAs("infoBoxToggleButton")] public GameObject infoBox;
    
    public ActivatebleButton nextActivityButton;
    public ActivatebleButton finishActivityButton;

    public Sprite mistakeBoxImage;
    public Sprite successBoxImage;
    // Scene which will be loaded once the level has been successfully finished
    public string nextSceneName;
    // Scene which will be loaded when level has been quit
    public string quitSceneName;

    public TextMeshProUGUI scoreDisplay;
    private int _currentActivityIndex;
    private int _displayedScore = 0;
    private int _maxPossibleScore = 0;

    private double _bronzeTierThreshold = 0.5f;
    private double _silverTierThreshold = 0.75f;
    private double _goldTierThreshold = 1f;

    private void UpdateProgressBar()
    {
        progressBar.currentFill = _currentActivityIndex + 1;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        progressBar.minFill = 0;
        progressBar.maxFill = activityManagers.Count;
        scoreDisplay.text = _displayedScore.ToString();
        
        StartActivity();
    }

    private void StartActivity()
    {
        UpdateProgressBar();
        var currentActivityManager = activityManagers[_currentActivityIndex];
        if (currentActivityManager.skipActivity)
        {
            SkipActivity();
        }
        else
        {
            currentActivityManager.onActivityCompleted.AddListener(HandleActivityFinished);
            currentActivityManager.onScoreChanged.AddListener(HandleScoreChanged);
            var activityName = currentActivityManager.activityName;
            var gameTextData = GameManager.Instance.gameTextData;
            var activityData = gameTextData.levelTexts[levelName].activityTexts[activityName];
            var isFirst = _currentActivityIndex == 0;
            currentActivityManager.StartActivity(isFirst, activityData, animationProperties, mistakeBoxImage, successBoxImage, infoBox);
            nextActivityButton.Disable();
            finishActivityButton.Disable();
            ShowCurrentActivity();
        }
    }

    private void ShowCurrentActivity()
    {
        for (int i = 0; i < activityManagers.Count; i++)
        {
            activityManagers[i].GameObject().SetActive(i == _currentActivityIndex);
        }

        var isLastActivity = _currentActivityIndex == activityManagers.Count - 1; 
        if (isLastActivity)
        {
            EvaluationActivityManager evaluationManager = (EvaluationActivityManager) activityManagers[_currentActivityIndex];
            var maxUnlockedTier = GetMaxUnlockedTier();
            evaluationManager.Evaluate(_displayedScore, _maxPossibleScore, maxUnlockedTier);
            
            nextActivityButton.Disable();
            // finishActivityButton.GameObject().SetActive(true);
        }
    }

    public void ReplayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void ExitLevel(string exitToScene)
    {
        CheckNextLevelUnlock();
        SceneManager.LoadScene(exitToScene);
    }

    private void CheckNextLevelUnlock()
    {
        var isLastActivity = _currentActivityIndex == activityManagers.Count - 1;
        var canFinishActivity = activityManagers[_currentActivityIndex];
        // Quitting last activity in level that is finished successfully - unlock next level
        if (isLastActivity && canFinishActivity)
        {
            CheckAchievements();
            GameManager.Instance.UnlockNextLevel(levelName);
        }
    }

    public BadgeTier GetMaxUnlockedTier()
    {
        var scorePercent = (double) _displayedScore / _maxPossibleScore;
        var maxUnlockedTier = BadgeTier.Locked;
        
        if (scorePercent >= _goldTierThreshold)
        {
            maxUnlockedTier = BadgeTier.Gold;
        }
        else if (scorePercent >= _silverTierThreshold)
        {
            maxUnlockedTier = BadgeTier.Silver;
        }
        else if (scorePercent >= _bronzeTierThreshold)
        {
            maxUnlockedTier = BadgeTier.Bronze;
        }

        return maxUnlockedTier;
    }
    
    private void CheckAchievements()
    {
        var maxUnlockedTier = GetMaxUnlockedTier();
        GameManager.Instance.UnlockBadge(levelName, maxUnlockedTier);
    }
    

    public void QuitLevel()
    {
        ExitLevel(quitSceneName);
    }

    public void ToggleInfoBox()
    {
        if (infoBox != null)
        {
            infoBox.SetActive(!infoBox.activeSelf);
            var infoBoxToggleButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            infoBoxToggleButton.GetComponent<ActivatebleButton>().SetState(!infoBox.activeSelf);
        }
        else
        {
            Debug.Log("Info box toggle button reference not set");
        }
    }

    public void FinishActivity()
    {
        var currentActivity = activityManagers[_currentActivityIndex];
        if (currentActivity.canFinishActivity)
        {
            ExitLevel(nextSceneName);
        }
    }
    
    public void SwitchNextActivity()
    {
        var currentActivity = activityManagers[_currentActivityIndex];
        var isLastActivity = _currentActivityIndex == activityManagers.Count - 1; 
        if (currentActivity.canFinishActivity && !isLastActivity)
        {
            currentActivity.ActivitySpecificCleanup();
            _currentActivityIndex++;
            StartActivity();
        }
    }

    private void HandleActivityFinished(int maxPossibleScore)
    {
        _maxPossibleScore += maxPossibleScore;
        var activityManager = activityManagers[_currentActivityIndex];
        activityManager.PlayAnimationLevelCompleteSuccess();
        nextActivityButton.Enable();
        finishActivityButton.Enable();
        NextActivityStart();
    }

    private void HandleScoreChanged(int addedScore)
    {
        _displayedScore += addedScore;
        scoreDisplay.text = _displayedScore.ToString();
    }

    public void HideAIFeedbackBox()
    {
        var activityManager = activityManagers[_currentActivityIndex];
        activityManager.HideAIFeedbackBox();
    }

    private void SkipActivity()
    {
        var currentActivityManager = activityManagers[_currentActivityIndex];
        if (_currentActivityIndex == activityManagers.Count - 1)
        {
            currentActivityManager.skipActivity = false;
            _currentActivityIndex--;
        }
        else
        {
            currentActivityManager.canFinishActivity = true;
        }
        NextActivityStart();
        SwitchNextActivity();
    }
    private void NextActivityStart()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "answersLog.txt");
        string logEntry = " === Next activity === ";
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // Write to file, appending each entry
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"\n{timestamp}");
            writer.WriteLine(logEntry);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
