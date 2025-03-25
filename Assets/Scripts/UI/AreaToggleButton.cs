using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AreaToggleButton : MonoBehaviour
{
    public string controlledArea;
    public UnityEvent<string> onAreaToggleButtonClicked;

    public void CategoryClicked()
    {
        onAreaToggleButtonClicked.Invoke(controlledArea);
    }
}
