using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIHandler : MonoBehaviour
{
    public void LoadSettingsScene()
    {
        SceneManager.LoadScene("SettingsScene");
    }

    public void LoadTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void LoadLanguageSelectorScene()
    {
        SceneManager.LoadScene("LanguageSelectorScreen");
    }
    
    public void LoadLevelSelectionScene()
    {
        SceneManager.LoadScene("LevelSelectionScreen");
    }
}