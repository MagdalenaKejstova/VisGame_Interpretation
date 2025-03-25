using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using System.IO;

public class QuestionActivityManager : ActivityManager
{
    [FormerlySerializedAs("allQuestionsAnswered")]
    public UnityEvent onAllQuestionsAnswered;

    public GameObject questionsPanel;
    public GameObject optionTemplate;
    private string _questionOptionTag;
    private Dictionary<string, Question> _questions = new();
    private Dictionary<string, bool> _answeredQuestions = new();
    protected int MaxScore;
    // private Dictionary<string, List<string>> _selectedAnswers = new Dictionary<string, List<string>>();

    
    // protected override void ActivitySpecificSetUp()
    // {
    //     _questionOptionTag = optionTemplate.gameObject.tag;
    //     if (ActivityData.questions.Count > 0)
    //     {
    //         SetUpQuestions();
    //     }
    //     MaxScore = CalculateMaxScore();
    //     nextInfoButton.Disable();
    // }

    protected override void ActivitySpecificSetUp()
{
    _questionOptionTag = optionTemplate.gameObject.tag;
    
    // Clear answered questions to reset all answers for a fresh start.
    _answeredQuestions.Clear();

    // Clear seen pages as well to reset information navigation.
    seenPages.Clear();

    // Reset ActivityData information chunks.
    ActivityData.informationChunks.Clear();

    if (ActivityData.questions.Count > 0)
    {
        SetUpQuestions();
    }
    
    MaxScore = CalculateMaxScore();
    nextInfoButton.Disable();
}
    
    private int CalculateMaxScore()
    {
        return scoreBasePerUnit + scoreTimeBonus + scoreCorrectAnswerBonus * _questions.Count;
    }

    // private void SetUpQuestions()
    // {
    //     questionsPanel.SetActive(true);
    //     _questions = ActivityData.questions;
    //     foreach (var question in _questions)
    //     {
    //         _answeredQuestions.Add(question.Key, false);
    //         ActivityData.informationChunks.Add(question.Key, question.Value.question);
    //         seenPages.Add(false);
    //     }
    //     optionTemplate.SetActive(false);
    // }

    private void SetUpQuestions()
{
    questionsPanel.SetActive(true);
    _questions = ActivityData.questions;

    foreach (var question in _questions)
    {
        // Add to _answeredQuestions only if it doesn't already exist
        if (!_answeredQuestions.ContainsKey(question.Key))
        {
            _answeredQuestions.Add(question.Key, false);
        }

        // Add to informationChunks if it does not already exist
        if (!ActivityData.informationChunks.Contains(question.Key))
        {
            ActivityData.informationChunks.Add(question.Key, question.Value.question);
        }

        // Add seen page entry for each question
        seenPages.Add(false);
    }

    optionTemplate.SetActive(false);
}

    private void SetUpQuestionOptions(string questionName)
    {
        var currentQuestionOptions = FindQuestionOptions(questionName);
        var allQuestionOptions = GameObject.FindGameObjectsWithTag(_questionOptionTag);

        nextInfoButton.Disable();
        // List<GameObject> foundQuestionOptions = new();

        // Find if question options already exist (page was created but switched to other question)
        foreach (var questionOption in allQuestionOptions)
        {
            var relatedQuestionName = questionOption.GetComponent<QuestionOption>().relatedQuestionName;
            if (relatedQuestionName == questionName)
            {
                questionOption.gameObject.SetActive(true);
            }
            else
            {
                questionOption.gameObject.SetActive(false);
            }
        }

        var question = _questions[questionName];
        // Create options if non were found
        if (currentQuestionOptions.Count == 0)
        {
            foreach (var option in question.options)
            {
                var newOption = Instantiate(optionTemplate, optionTemplate.transform.parent);
                newOption.transform.SetAsLastSibling();
                newOption.GetComponentInChildren<TextMeshProUGUI>().text = option.option;

                var questionOption = newOption.GetComponent<QuestionOption>();
                questionOption.feedback = option.feedback;
                questionOption.isCorrect = option.correct;
                questionOption.relatedQuestionName = questionName;
                SetUpAnswerListeners(questionOption);
                newOption.gameObject.SetActive(true);
                currentQuestionOptions.Add(newOption);
            }
        }

        SetQuestionOptionButtonStates(questionName, currentQuestionOptions);
    }

    protected void SetQuestionOptionButtonStates(string questionName, List<GameObject> questionOptions)
    {
        var isAnswered = _answeredQuestions[questionName];
        foreach (var questionOption in questionOptions)
        {
            var questionButton = questionOption.gameObject.GetComponent<ActivatebleButton>();
            var option = questionOption.gameObject.GetComponent<QuestionOption>();
            var isEnabled = !isAnswered || option.isCorrect;
            questionButton.SetState(isEnabled);
        }
    }

    protected override void SetInfoBoxPage(int pageIndex)
    {
        var questionName = GetDisplayedPageName(pageIndex);
        //nextInfoButton.Disable();
        if (_questions.Keys.Contains(questionName))
        {
            SetUpQuestionOptions(questionName);
            nextInfoButton.Disable();
            Debug.Log("Disabling Next Info button as question page is shown.");
        }
        else{
            nextInfoButton.Enable();
        }

        base.SetInfoBoxPage(pageIndex);
        
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

    private void SetUpAnswerListeners(QuestionOption questionOption)
    {
        questionOption.correctOptionSelected.AddListener(CorrectOptionSelected);
        questionOption.wrongOptionSelected.AddListener(WrongOptionSelected);
    }

    private void CorrectOptionSelected(string feedback, string questionName)
    {
        // Log the correct answer
        LogAnswer(questionName, true);
        //LogSelectedAnswer(questionName, feedback);

        if (_answeredQuestions[questionName]) 
        {
            return; // Exit the function if the question was already answered
        }

        _answeredQuestions[questionName] = true;

        var questionOptions = FindQuestionOptions(questionName);
        SetQuestionOptionButtonStates(questionName, questionOptions);

        UpdateScoreCorrectAnswer(); // Add points for a correct answer
        CheckAnsweredCondition();
        ShowDirectSuccessFeedback(feedback);
        nextInfoButton.Enable();
        //SetNextInfoBoxPage();
    }

    private List<GameObject> FindQuestionOptions(string questionName)
    {
        List<GameObject> foundOptions = new();
        var allQuestionOptions = GameObject.FindGameObjectsWithTag(_questionOptionTag);
        foreach (var questionOption in allQuestionOptions)
        {
            var option = questionOption.GetComponent<QuestionOption>();
            if (option.relatedQuestionName == questionName)
            {
                foundOptions.Add(questionOption);
            }
        }

        return foundOptions;
    }

    private void WrongOptionSelected(string feedback, string questionName)
    {
        // Log the incorrect answer
        LogAnswer(questionName, false);
        //LogSelectedAnswer(questionName, feedback);
        
        UpdateScoreIncorrectAnswer();
        ShowDirectMistakeFeedback(feedback);
        nextInfoButton.Disable();
    }

    private void CheckAnsweredCondition()
    {
        var areAllAnswered = _answeredQuestions.Select(question => question.Value).All(answered => answered);
        if (areAllAnswered)
        {
            EndActivityScoreCheck();
            HasMaxScore = CurrentScore == MaxScore;
            onAllQuestionsAnswered.Invoke();
            nextInfoButton.Disable();
        }
    }

    public override void ActivitySpecificCleanup()
    {
        var questionOptions = GameObject.FindGameObjectsWithTag(_questionOptionTag);
        foreach (var option in questionOptions)
        {
            option.gameObject.SetActive(false);
        }
        questionsPanel.SetActive(false);
    }

    // private void LogSelectedAnswer(string questionName, string selectedAnswer)
    // {
    //     if (!_selectedAnswers.ContainsKey(questionName))
    //     {
    //         _selectedAnswers[questionName] = new List<string>();
    //     }

    //     _selectedAnswers[questionName].Add(selectedAnswer);
    // }

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
    
    // public void AppendSelectedAnswersToFile()
    // {
    //     string filePath = Path.Combine(Application.persistentDataPath, "selectedAnswers.txt");
        
    //     // Append text to the file
    //     using (StreamWriter writer = new StreamWriter(filePath, true))
    //     {
    //         writer.WriteLine("=== New Answer Selection ===");
    //         foreach (var question in _selectedAnswers)
    //         {
    //             writer.WriteLine($"Question: {question.Key}");
    //             foreach (var answer in question.Value)
    //             {
    //                 writer.WriteLine($"- Selected Answer: {answer}");
    //             }
    //             writer.WriteLine(); // Blank line between questions
    //         }
    //     }
        
    //     Debug.Log($"Answers appended to {filePath}");
    // }

}