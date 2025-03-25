using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoundEffectSettingButton : SettingsButton, IPointerClickHandler 
{
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.ToggleSoundEffects();
        Toggle();
    }
}
