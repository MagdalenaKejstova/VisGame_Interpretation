using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InfoBoxActiveStateChanged : MonoBehaviour
{
    public UnityEvent onInfoBoxEnabled;
    public UnityEvent onInfoBoxDisabled;
    
    private void OnEnable()
    {
        onInfoBoxEnabled.Invoke();
    }
    
    private void OnDisable()
    {
        onInfoBoxDisabled.Invoke();
    }
}
