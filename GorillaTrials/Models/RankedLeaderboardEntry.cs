using System;

namespace GorillaTrials.Models;

[Serializable]
public class RankedLeaderboardEntry
{
    public string PlayerName;
    public double Time;
    public string PlayerId;
    public int Rank;
}