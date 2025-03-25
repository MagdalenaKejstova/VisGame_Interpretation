using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class ResetProgressButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        ResetProgress();
    }

    private void ResetProgress()
    {
        GameManager.Instance.ResetAllProgress();
        Debug.Log("All progress has been reset.");
        NextGameParticipant();
    }

    private void NextGameParticipant()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "answersLog.txt");
        string logEntry = "\n === New Answer Selection === \n";
        
        // Write to file, appending each entry
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(logEntry);
        }
    }
}