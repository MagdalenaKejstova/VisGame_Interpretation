using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUnlocker : MonoBehaviour
{
    public string levelName;
    public ActivatebleButton triggerButton;

    public void Unlock()
    {
        Debug.Log($"Unlocking {levelName}");
        triggerButton.Enable();
    }
}
