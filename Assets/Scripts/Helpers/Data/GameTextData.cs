using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Windows;

[Serializable]
public class Option
{
    public string option;
    public bool correct;
    public string feedback;
}

[Serializable]
public class Question
{
    public string question;
    public List<Option> options;
}

[Serializable]
public class ActivityData
{
    public OrderedDictionary informationChunks;
    public Dictionary<string, Question> questions;
    public Dictionary<string, string> mistakeFeedbacks;
    public Dictionary<string, string> successFeedbacks;
}

[Serializable]
public class LevelTextData
{
    public Dictionary<string, ActivityData> activityTexts;
}

[Serializable]
public class GameTextData
{
    public Dictionary<string, LevelTextData> levelTexts;
}
