using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    public ActivatebleButton settingButton;

    public void Enable()
    {
        settingButton.Enable();
    }

    public void Disable()
    {
        settingButton.Disable();
    }

    public void Toggle()
    {
        settingButton.SetState(!settingButton.isEnabled);
    }
}
