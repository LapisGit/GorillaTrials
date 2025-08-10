using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTrials.Models;

[Serializable]
public class TrialDataModel
{
    public string displayName;
    public string trialId;
    public Position position;
    public float angle;
    public string trialType;
    public string trialDifficulty;
    public float maxTime;
    public bool customMapTrial;
    public List<Position> points;
}

[Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3() => new Vector3(x, y, z);
}