using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BadgeTier
{
    Locked = 0,
    Bronze = 1,
    Silver = 2,
    Gold = 3,
}
public class Badge : MonoBehaviour
{
    public string name;

    public Sprite lockedIcon;
    public Sprite bronzeIcon;
    public Sprite silverIcon;
    public Sprite goldIcon;
    
    public Image displayedIcon;
    private BadgeTier _currentTier = BadgeTier.Locked;
    
    void Awake()
    {
        SetBadgeIcon();
    }
    
    private void SetBadgeIcon()
    {
        switch (_currentTier)
        {
            case BadgeTier.Locked:
                displayedIcon.sprite = lockedIcon;
                break;
            case BadgeTier.Bronze:
                displayedIcon.sprite = bronzeIcon;
                break;
            case BadgeTier.Silver:
                displayedIcon.sprite = silverIcon;
                break;
            case BadgeTier.Gold:
                displayedIcon.sprite = goldIcon;
                break;
        }
    }
    public void Unlock(BadgeTier unlockedTier)
    {
        if (unlockedTier > _currentTier)
        {
            _currentTier = unlockedTier;
        }
        SetBadgeIcon();
    }
}
