using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundClipPlayer : MonoBehaviour
{
    public AudioSource AudioSource;

    public AudioClip ClapAudio;

    private void Start()
    {
        AudioSource.clip = ClapAudio;
    }

    public void PlayCharacterClap()
    {
        AudioSource.Play();
    }
    
    public void StopCharacterClap()
    {
        AudioSource.Stop();
    }
}
