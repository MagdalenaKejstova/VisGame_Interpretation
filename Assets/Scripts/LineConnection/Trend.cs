using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tendency
{
    Up,
    Down,
    Undefined
}

public class Trend
{
    public Trend()
    {
    }
    
    public Trend(Tendency tendency, int startIndex, int endIndex)
    {
        Tendency = tendency;
        Marked = false;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }
    
    public Trend(Tendency tendency, bool marked, int startIndex, int endIndex)
    {
        Tendency = tendency;
        Marked = marked;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public Tendency Tendency { get; set; }
    public bool Marked { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
