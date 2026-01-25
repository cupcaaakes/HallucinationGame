using System;
using System.IO;
using System.Linq;
using UnityEngine;

public enum RankId
{
    Thinker,
    TechEnthusiast,
    Visionary,
    BridgeBuilder,
    Doubter,
    OpinionShaper,
    Individualist,
    Humanist
}

[Serializable]
public class GAIAData
{
    public string date; // "yyyy-MM-dd"

    // Total runs
    public int totalRuns;
    public int todayRuns;

    // TOTAL counters
    public int thinkerTotal;
    public int techEnthusiastTotal;
    public int visionaryTotal;
    public int bridgeBuilderTotal;
    public int doubterTotal;
    public int opinionShaperTotal;
    public int individualistTotal;
    public int humanistTotal;

    // TODAY counters
    public int thinkerToday;
    public int techEnthusiastToday;
    public int visionaryToday;
    public int bridgeBuilderToday;
    public int doubterToday;
    public int opinionShaperToday;
    public int individualistToday;
    public int humanistToday;
}