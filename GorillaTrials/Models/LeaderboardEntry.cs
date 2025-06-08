using System;

namespace GorillaTrials.Models;

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public double time;
    public int rank;
}