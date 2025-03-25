using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Febucci.UI;
using Febucci.UI.Core;
using TMPro;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.Localization.Plugins.XLIFF.V20;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class ActivityManager : MonoBehaviour
{
    public UnityEvent<int> onActivityCompleted;
    public UnityEvent onAllInfoPagesSeen;
    public UnityEvent<int> onScoreChanged;
    public GraphDataFiller graphDataFiller;
    public bool skipActivity = false;
    public bool canFinishActivity = false;
    public string activityName;
    protected GameObject infoBox;
    protected bool IsLockedInfoBox = false;

    private Sprite _mistakeBoxImage;
    private Sprite _successBoxImage;

    private InfoBoxActiveStateChanged boxState;
    protected ActivatebleButton prevInfoButton;
    protected ActivatebleButton nextInfoButton;
    protected GameObject infoArea;
    protected GameObject infoText;
    protected TypewriterCore typeWriter;
    protected List<bool> seenPages = new();


    protected int _currentInfoIndex = 0;
    private int _greetingDuration = 1;
    private int _feedbackDuration = 5;

    protected ActivityData ActivityData;
    protected AnimationProperties AnimationProperties;

    protected int scoreBasePerUnit = 10;
    protected int scoreCorrectAnswerBonus = 50;
    protected int scoreIncorrectAnswerPenalty = 25;
    // Score time bonus not active
    protected int scoreTimeBonus = 0;
    
    
    protected float TimeBonusWindow = 5f * 60f;
    protected float StartTime;
    protected int CurrentScore = 0;
    protected bool HasMaxScore;

    public void StartActivity(bool isFirst, ActivityData activityData, AnimationProperties animationProperties,
        Sprite mistakeBoxImage, Sprite successBoxImage, GameObject passedInfoBox)
    {
        StartTime = Time.time;
        
        ActivityData = activityData;
        AnimationProperties = animationProperties;
        infoBox = passedInfoBox;

        _mistakeBoxImage = mistakeBoxImage;
        _successBoxImage = successBoxImage;
        InitialSetUp();

        if (isFirst)
        {
            GreetPlayer();
        }
        else
        {
            SetInfoBoxPage(_currentInfoIndex);
        }
    }

    private void InitialSetUp()
    {
        SetUpAttributes();
        SetUpAnimations();
        SetUpEventHandlers();
        SetUpInfoBox();
        ActivitySpecificSetUp();
    }

    protected abstract void ActivitySpecificSetUp();

    public abstract void ActivitySpecificCleanup();

    private void SetUpEventHandlers()
    {
        typeWriter.onTypewriterStart.AddListener(MarkCurrentPageSeen);
        boxState.onInfoBoxDisabled.AddListener(PauseExplanation);
        boxState.onInfoBoxEnabled.AddListener(ResumeExplanation);
        //typeWriter.onTextShowed.AddListener(MarkCurrentPageSeen);
    }

    private void SetUpAttributes()
    {
        boxState = infoBox.GetComponent<InfoBoxActiveStateChanged>();
        infoArea = infoBox.transform.Find("InfoArea").gameObject;
        infoText = infoArea.transform.Find("SpeechBubble").transform.Find("Viewport").transform.Find("InfoText").gameObject;
        typeWriter = infoText.GetComponent<TypewriterCore>();

        foreach (var _ in ActivityData.informationChunks)
        {
            seenPages.Add(false);
        }
    }

    private IEnumerator PlayAnimationIdleWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        PlayAnimationIdle(); // Then play the idle animation
    }

    private void SetUpAnimations()
    {
        typeWriter.onTypewriterStart.AddListener(PlayAnimationExplain);
        //typeWriter.onTextShowed.AddListener(PlayAnimationIdle);
        typeWriter.onTextShowed.AddListener(() => StartCoroutine(PlayAnimationIdleWithDelay(10f)));
    }

    private void MarkCurrentPageSeen()
    {
        var allPagesSeenBefore = seenPages.All(pageSeen => pageSeen);
        if (_currentInfoIndex < seenPages.Count)
        {
            seenPages[_currentInfoIndex] = true;
        }

        CheckAllPagesSeen(allPagesSeenBefore);
    }

    private void SetUpNavigationButtons()
    {
        var prevInfoGameObject = infoBox.transform.Find("PrevInfo").gameObject;
        prevInfoButton = prevInfoGameObject.GetComponent<ActivatebleButton>();
        prevInfoGameObject.GetComponent<Button>().onClick.AddListener(SetPrevInfoBoxPage);

        var nextInfoGameObject = infoBox.transform.Find("NextInfo").gameObject;
        nextInfoButton = infoBox.transform.Find("NextInfo").gameObject.GetComponent<ActivatebleButton>();
        nextInfoGameObject.GetComponent<Button>().onClick.AddListener(SetNextInfoBoxPage);
    }

    private void SetUpInfoBox()
    {
        SetUpNavigationButtons();
    }

    public void SetNextInfoBoxPage()
    {
        if (_currentInfoIndex != ActivityData.informationChunks.Count - 1 && !IsLockedInfoBox)
        {
            _currentInfoIndex++;
            SetInfoBoxPage(_currentInfoIndex);
        }
    }

    public void SetPrevInfoBoxPage()
    {
        if (_currentInfoIndex != 0 && !IsLockedInfoBox)
        {
            _currentInfoIndex--;
            SetInfoBoxPage(_currentInfoIndex);
        }
    }

    protected void SetInfoBoxPage(string pageName)
    {
        var pageIndex = GetIndexOfKey(pageName);
        if (pageIndex >= 0)
        {
            SetInfoBoxPage(pageIndex);
        }
        else
        {
            Debug.Log("No activity chunks found for the key: " + pageName);
        }
    }

    protected virtual void SetInfoBoxPage(int pageIndex)
    {
        // Enable/disable next/prev buttons
        prevInfoButton.SetState(pageIndex != 0 && !IsLockedInfoBox);
        nextInfoButton.SetState(pageIndex != ActivityData.informationChunks.Count - 1 && !IsLockedInfoBox);

        // Reset scrollbar to top position
        var scrollBar = infoArea.transform.Find("SpeechBubble").transform.Find("Scrollbar").gameObject;
        scrollBar.GetComponent<Scrollbar>().value = 1;

        // Fill text area with current info chunk
        var infoTextMeshPro = infoText.GetComponent<TextMeshProUGUI>();
        var infoChunksCount = ActivityData.informationChunks.Count;
        var textToShow = infoChunksCount > pageIndex
            ? (string)ActivityData.informationChunks[pageIndex]
            : "";
        infoTextMeshPro.text = textToShow;

        // KEEP - should update automatically with TextMeshPro component but it doesn't work and causes problems with displaying text when not updated manually
        var textAnimator = infoText.GetComponent<TextAnimator_TMP>();
        textAnimator.SetText(textToShow);

        typeWriter.StartShowingText(true);
        if (_currentInfoIndex < seenPages.Count && seenPages[_currentInfoIndex])
        {
            typeWriter.SkipTypewriter();
        }
    }

    private void CheckAllPagesSeen(bool allPagesSeenBefore)
    {
        var areAllPagesSeen = seenPages.All(pageSeen => pageSeen);
        if (areAllPagesSeen && !allPagesSeenBefore)
        {
            onAllInfoPagesSeen.Invoke();
        }
    }

    private int GetIndexOfKey(string key)
    {
        int index = 0;
        foreach (DictionaryEntry entry in ActivityData.informationChunks)
        {
            if (entry.Key.Equals(key))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    private void PlayAnimationIdle()
    {
        PlayAnimation(AnimationProperties.idleAnimation);
    }

    private void PlayAnimationExplain()
    {
        PlayAnimation(AnimationProperties.explainAnimation);
    }

    public void PlayAnimationLevelCompleteSuccess()
    {
        AudioManager.Instance.StopClap();
        PlayAnimationWithTimeout(AnimationProperties.levelCompleteAnimation, _feedbackDuration);
    }
    
    public void PlayAnimationLevelCompleteFail()
    {
        AudioManager.Instance.StopClap();
        PlayAnimationWithTimeout(AnimationProperties.mistakeAnimation, _feedbackDuration);
    }

    private void PlayAnimation(string animationName)
    {
        // Don't interrupt level finish animation if this was the last successful action
        if (!canFinishActivity)
        {
            AnimationProperties.animController.PlayAnimation(animationName);
        }
    }

    public void PlayAnimationWithTimeout(string animationName, int timeout)
    {
        AnimationProperties.animController.PlayAnimation(animationName);
        StartCoroutine(StopAnimationTimeout(timeout));
    }

    private IEnumerator StopAnimationTimeout(int timeout)
    {
        yield return new WaitForSeconds(timeout);
        AnimationProperties.animController.Idle();
    }

    private void GreetPlayer()
    {
        AnimationProperties.animController.Wave();
        StartCoroutine(StopGreetingTimeout());
    }

    private IEnumerator StopGreetingTimeout()
    {
        yield return new WaitForSeconds(_greetingDuration);
        SetInfoBoxPage(_currentInfoIndex);
    }

    public void PauseExplanation()
    {
        typeWriter.StopShowingText();
        PlayAnimation(AnimationProperties.idleAnimation);
    }

    public void ResumeExplanation()
    {
        typeWriter.StartShowingText(false);
        PlayAnimation(AnimationProperties.explainAnimation);
    }

    protected void ShowAIFeedbackBox(string feedback)
    {
        PauseExplanation();
        foreach (Transform child in infoBox.transform)
        {
            child.gameObject.SetActive(false);
        }

        var feedbackBox = infoBox.transform.Find("AIFeedbackBox").gameObject;
        var feedbackBoxText = feedbackBox.GetComponentInChildren<TextMeshProUGUI>();
        feedbackBoxText.text = feedback;
        feedbackBox.SetActive(true);
    }

    public void HideAIFeedbackBox()
    {
        foreach (Transform child in infoBox.transform)
        {
            child.gameObject.SetActive(true);
        }

        infoBox.transform.Find("AIFeedbackBox").gameObject.SetActive(false);

        PlayAnimation(AnimationProperties.idleAnimation);
        AudioManager.Instance.StopClap();
        ResumeExplanation();
    }

    protected void ShowMistakeFeedback(string mistakeName, bool readFromFile = true)
    {
        string feedback;
        if (readFromFile)
        {
            try
            {
                feedback = ActivityData.mistakeFeedbacks[mistakeName];
            }
            catch (KeyNotFoundException ex)
            {
                Debug.Log("Mistake feedback not found: " + ex.Message);
                return;
            }
        }
        else
        {
            feedback = mistakeName;
        }


        var feedbackBox = infoBox.transform.Find("AIFeedbackBox").gameObject;
        feedbackBox.GetComponent<Image>().sprite = _mistakeBoxImage;
        ShowAIFeedbackBox(feedback);
        AudioManager.Instance.PlayNegativeFeedback();
        PlayAnimation(AnimationProperties.mistakeAnimation);
    }

    protected void ShowDirectMistakeFeedback(string feedback)
    {
        ShowMistakeFeedback(feedback, false);
    }

    protected void ShowSuccessFeedback(string successName, bool readFromFile = true)
    {
        string feedback;
        if (readFromFile)
        {
            try
            {
                feedback = ActivityData.successFeedbacks[successName];
            }
            catch (KeyNotFoundException ex)
            {
                Debug.Log("Success feedback not found: " + ex.Message);
                return;
            }
        }
        else
        {
            feedback = successName;
        }
        
        var feedbackBox = infoBox.transform.Find("AIFeedbackBox").gameObject;
        feedbackBox.GetComponent<Image>().sprite = _successBoxImage;
        ShowAIFeedbackBox(feedback);
        AudioManager.Instance.PlayPositiveFeedback();
        if (canFinishActivity)
        {
            AudioManager.Instance.StopClap();
            AudioManager.Instance.PlayCelebrateFinish();
        }
        else
        {
            AudioManager.Instance.PlayClap();
        }
        PlayAnimation(AnimationProperties.successAnimation);
    }

    protected void ShowDirectSuccessFeedback(string feedback)
    {
        ShowSuccessFeedback(feedback, false);
    }
    
    protected void UpdateScoreCorrectAnswer()
    {
        CurrentScore += scoreCorrectAnswerBonus;
        onScoreChanged.Invoke(scoreCorrectAnswerBonus);
    }

    protected void UpdateScoreIncorrectAnswer()
    {
        // scoreCorrectAnswerBonus = Mathf.Max(0, scoreCorrectAnswerBonus - scoreIncorrectAnswerPenalty);
        CurrentScore -= scoreIncorrectAnswerPenalty;
        onScoreChanged.Invoke(-scoreIncorrectAnswerPenalty);
    }
    
    protected void EndActivityScoreCheck()
    {
        CurrentScore += scoreBasePerUnit;
        CheckTimeBonus();
        onScoreChanged.Invoke(scoreBasePerUnit);
    }

    protected void CheckTimeBonus()
    {
        var elapsedTime = Time.time - StartTime;
        if (elapsedTime < TimeBonusWindow)
        {
            CurrentScore += scoreTimeBonus;
            onScoreChanged.Invoke(scoreTimeBonus);
        }
    }
}