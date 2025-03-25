using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public bool HasBackgroundMusic = true;

    public bool HasSoundEffects = true;

    public BackgroundMusicPlayer backgroundMusicPlayer;
    public ButtonEffectClipPlayer buttonClickPlayer;
    public CharacterSoundClipPlayer characterEffectPlayer;
    public FeedbackEffectClipPlayer feedbackEffectPlayer;
    
    public static AudioManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            backgroundMusicPlayer.PlayBackgroundMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleBackgroundMusic()
    {
        HasBackgroundMusic = !HasBackgroundMusic;
        if (HasBackgroundMusic)
        {
            backgroundMusicPlayer.UnPause();
        }
        else
        {
            backgroundMusicPlayer.Pause();
        }
    }
    
    public void ToggleSoundEffects()
    {
        HasSoundEffects = !HasSoundEffects;
    }

    public void PlayButtonClicked()
    {
        if (HasSoundEffects)
        {
            buttonClickPlayer.PlayButtonClicked();
        }
    }
    
    public void PlayClap()
    {
        if (HasSoundEffects)
        {
            characterEffectPlayer.PlayCharacterClap();
        }
    }
    
    public void StopClap()
    {
        if (HasSoundEffects)
        {
            characterEffectPlayer.StopCharacterClap();
        }
    }
    
    public void PlayPositiveFeedback()
    {
        if (HasSoundEffects)
        {
            feedbackEffectPlayer.PlayPositiveFeedback();
        }
    }
    
    public void PlayNegativeFeedback()
    {
        if (HasSoundEffects)
        {
            feedbackEffectPlayer.PlayNegativeFeedback();
        }
    }
    
    public void PlayCelebrateFinish()
    {
        if (HasSoundEffects)
        {
            feedbackEffectPlayer.PlayCelebrateFinish();
        }
    }
    
}
