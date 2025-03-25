using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class GameSaveData
{
    [FormerlySerializedAs("isUnlocked")] public Dictionary<string, bool> isUnlockedLevel = new();
    public Dictionary<string, BadgeTier> unlockedBadgeTier = new();
    public Dictionary<string, int> levelOrder = new();
}
