using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEffectClipPlayer : MonoBehaviour
{
    public AudioSource AudioSource;

    public AudioClip ButtonClickAudio;

    public void PlayButtonClicked()
    {
        AudioSource.PlayOneShot(ButtonClickAudio);
    }
}
