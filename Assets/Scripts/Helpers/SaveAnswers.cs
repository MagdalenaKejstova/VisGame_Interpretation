using UnityEngine;
using UnityEngine.EventSystems;

public class SaveAnswersButton : MonoBehaviour, IPointerClickHandler
{
    public QuestionActivityManager questionActivityManager;
    public void OnPointerClick(PointerEventData eventData)
    {
        SaveAnswers();
    }

    private void SaveAnswers()
    {
        // if (questionActivityManager != null)
        // {
        //     questionActivityManager.AppendSelectedAnswersToFile();
        //     Debug.Log("Selected answers saved to file.");
        // }
        // else
        // {
        //     Debug.LogError("QuestionActivityManager reference is missing.");
        // }
    }
}