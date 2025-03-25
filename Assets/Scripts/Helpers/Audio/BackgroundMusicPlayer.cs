using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour
{
    public AudioSource AudioSource;

    public AudioClip BackgroundMusic;

    public void PlayBackgroundMusic()
    {
        AudioSource.PlayOneShot(BackgroundMusic);
    }

    public void Pause()
    {
        AudioSource.Pause();
    }
    
    public void UnPause()
    {
        AudioSource.UnPause();
    }
}
