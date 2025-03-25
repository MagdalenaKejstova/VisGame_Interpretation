using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackEffectClipPlayer : MonoBehaviour
{
    public AudioSource AudioSource;

    public AudioClip PositiveFeedbackAudio;
    public AudioClip NegativeFeedbackAudio;
    public AudioClip CelebrateFinishFeedbackAudio;

    public void PlayPositiveFeedback()
    {
        AudioSource.PlayOneShot(PositiveFeedbackAudio);
    }
    
    public void PlayNegativeFeedback()
    {
        AudioSource.PlayOneShot(NegativeFeedbackAudio);
    }
    
    public void PlayCelebrateFinish()
    {
        AudioSource.PlayOneShot(CelebrateFinishFeedbackAudio);
    }
}
