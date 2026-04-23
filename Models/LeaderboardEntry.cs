using System;

namespace GorillaTrials.Models;

[Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public double time;
    public int rank;
    public string PlayerId;
}