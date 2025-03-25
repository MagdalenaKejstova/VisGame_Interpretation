using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    private Animator _characterAnimator;
    // Start is called before the first frame update
    void Start()
    {
        _characterAnimator = GetComponent<Animator>();
    }

    public void PlayAnimation(string animationName)
    {
        switch (animationName)
        {
            case "Wave":
                Wave();
                break;
            case "Talk":
                Talk();
                break;
            case "Clap":
                Clap();
                break;
            case "Disapprove":
                Disapprove();
                break;
            case "Celebrate":
                Celebrate();
                break;
            case "Idle":
                Idle();
                break;
        }
    }

    private void DisableAllAnimations()
    {
        foreach(AnimatorControllerParameter parameter in _characterAnimator.parameters) {            
            _characterAnimator.SetBool(parameter.name, false);            
        }
    }
    public void Clap()
    {
        if (_characterAnimator != null)
        {
            DisableAllAnimations();
            _characterAnimator.SetBool("Clap", true);
        }
    }
    
    public void Idle()
    {
        if (_characterAnimator != null)
        {
            DisableAllAnimations();
            _characterAnimator.SetBool("Idle", true);
        }
    }
    
    public void Disapprove()
    {
        if (_characterAnimator != null)
        {
            DisableAllAnimations();
            _characterAnimator.SetBool("Disapprove", true);
        }
    }
    
    public void Celebrate()
    {
        if (_characterAnimator != null)
        {
            DisableAllAnimations();
            _characterAnimator.SetBool("Celebrate", true);
        }
    }
    
    public void Wave()
    {
        if (_characterAnimator != null)
        {
            DisableAllAnimations();
            _characterAnimator.SetBool("Wave", true);
        }
    }
    
    public void Talk()
    {
        if (_characterAnimator != null)
        {
            DisableAllAnimations();
            _characterAnimator.SetBool("Talk", true);
        }
    }
}
