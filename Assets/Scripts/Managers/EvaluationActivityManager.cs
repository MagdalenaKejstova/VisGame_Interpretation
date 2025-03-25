using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvaluationActivityManager : ActivityManager
{
    public GameObject evaluationBox;
    
    public GameObject successScoreEvalBox;
    public TextMeshProUGUI successScoreDisplay;
    public Badge awardedBadge;
    
    public GameObject failureScoreEvalBox;
    public TextMeshProUGUI failureScoreDisplay;

    protected override void ActivitySpecificSetUp()
    {
        evaluationBox.gameObject.SetActive(true);
    }

    public override void ActivitySpecificCleanup()
    {
        successScoreEvalBox.gameObject.SetActive(false);
        failureScoreEvalBox.gameObject.SetActive(false);
        evaluationBox.gameObject.SetActive(false);
    }

    public void Evaluate(int currentScore, int maxScore, BadgeTier unlockedTier)
    {
        // Level failed
        if (unlockedTier == BadgeTier.Locked)
        {
            failureScoreDisplay.text = $"{currentScore}/{maxScore}";
            failureScoreEvalBox.gameObject.SetActive(true);
            AudioManager.Instance.PlayNegativeFeedback();
            PlayAnimationLevelCompleteFail();
        }
        // Level succeeded
        else
        {
            successScoreDisplay.text = $"{currentScore}/{maxScore}";
            awardedBadge.Unlock(unlockedTier);
            successScoreEvalBox.gameObject.SetActive(true);
            AudioManager.Instance.PlayCelebrateFinish();
            PlayAnimationLevelCompleteSuccess();
        }
        
        canFinishActivity = true;
    }
}
