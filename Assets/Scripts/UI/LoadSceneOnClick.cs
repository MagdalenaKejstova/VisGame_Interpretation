using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour, IPointerClickHandler
{
    public string sceneName; // Replace SceneAsset with string for scene name
    public ActivatebleButton triggerButton;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (triggerButton.isEnabled)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.Log("Scene name is not assigned to this object.");
                return;
            }

            Debug.Log("Loading scene: " + sceneName); // Output the scene being loaded
            SceneManager.LoadScene(sceneName);
        }
    }
}
