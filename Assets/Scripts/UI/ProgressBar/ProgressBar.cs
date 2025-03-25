using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    public int minFill;
    public int maxFill;
    public int currentFill;
    public Image mask;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetCurrentFill();
    }

    void GetCurrentFill()
    {
        float currentOffset = currentFill - minFill;
        float maxOffset = maxFill - minFill;
        float fillAmount = currentOffset / maxOffset;
        mask.fillAmount = fillAmount;
    }
}
