using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ActivatebleButton : MonoBehaviour
{
    public bool isEnabled;
    public bool disableOnAwake = true;
    public Sprite enabledStateIcon;
    public Sprite disabledStateIcon;
    
    public void Enable()
    {
        isEnabled = true;
        GetComponent<Image>().sprite = enabledStateIcon;
    }

    public void Disable()
    {
        isEnabled = false;
        GetComponent<Image>().sprite = disabledStateIcon;
    }

    public void SetState(bool enabled)
    {
        if (enabled)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }
    
    void Awake()
    {
        if (disableOnAwake)
        {
            Disable();
        }
    }
}
