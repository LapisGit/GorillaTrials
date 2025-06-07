using System.Collections.Generic;
using UnityEngine;

namespace GorillaTrials.Models
{
    public class TrialPositions : MonoBehaviour
    {
        public static List<Vector3> trialTestBoxes;
        
        void Awake()
        {
            trialTestBoxes = new List<Vector3>();
            trialTestBoxes.Add(new Vector3(-65.5062f,2.556363f,-72.94588f));
            trialTestBoxes.Add(new Vector3(-69.15505f,4.061646f,-75.5445f));
            trialTestBoxes.Add(new Vector3(-68.22018f,5.746896f,-77.90948f));
            trialTestBoxes.Add(new Vector3(-67.33721f,8.802135f,-78.8488f));
            trialTestBoxes.Add(new Vector3(-66.6948f,12.32989f,-78.68581f));
            trialTestBoxes.Add(new Vector3(-66.51869f,16.78736f,-79.79868f));
            trialTestBoxes.Add(new Vector3(-66.83424f,21.94868f,-79.76862f));
            trialTestBoxes.Add(new Vector3(-63.53926f,21.88185f,-82.22379f));
            trialTestBoxes.Add(new Vector3(-50.99947f,20.56706f,-77.91679f));
            trialTestBoxes.Add(new Vector3(-43.39106f,21.32471f,-75.30914f));
            trialTestBoxes.Add(new Vector3(-40.28435f,21.79396f,-74.52564f));
            trialTestBoxes.Add(new Vector3(-36.86979f,18.28717f,-74.44807f));
        }
    }
}