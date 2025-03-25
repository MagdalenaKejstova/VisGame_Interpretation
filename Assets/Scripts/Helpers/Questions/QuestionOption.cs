using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class QuestionOption : MonoBehaviour
{
    [FormerlySerializedAs("correctOption")] public bool isCorrect;
    public string feedback;
    public string relatedQuestionName;
    
    public UnityEvent<string, string> wrongOptionSelected;
    public UnityEvent<string, string> correctOptionSelected;
    
    public void OptionSelected()
    {
        if (isCorrect)
        {
            correctOptionSelected.Invoke(feedback, relatedQuestionName);
        }
        else
        {
            wrongOptionSelected.Invoke(feedback, relatedQuestionName);
        }
    }
}
