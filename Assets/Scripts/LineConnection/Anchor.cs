using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Anchor : MonoBehaviour
{
    public class AnchorInfo
    {
        public int index;
    }
    
    public UnityEvent<AnchorInfo> anchorClicked;
    public LineRenderer lineRenderer;
    public int order;
    public double value;
    public int expectedStartIndex;
    public int possibleObjectsToConnectCount;
    public int expectedObjectsToConnectCount;
    public Tendency currentTendency;

    public void NotifyManager()
    {
        var anchorInfo = new AnchorInfo();
        anchorInfo.index = order - 1;
        anchorClicked.Invoke(anchorInfo);
    }
}