using System.Collections.Generic;
using UnityEngine;

namespace GorillaTrials.Models
{
    public enum TrialType
    {
        Box,
        Zone
    }

    public class ZoneData
    {
        public Vector3 startPosition;
        public Vector3 endPosition;
    }
    
    public class Trial
    {
        public Vector3 position;
        public GameObject trialObject; //DO NOT SERIALIZE/DESERIALIZE FROM SERVER, THE MOD IS SUPPOSED TO AUTOMATICALLY ASSIGN THIS.
        public string TrialLongName;
        public string TrialServerName;
        public int TrialType; // When deserializing this, make sure to convert the enum on the server (ex: challenge type set to "box") and set it to its corresponding value (ex box challenge type is 0 and zone type is 1, refer to TrialType)
        public ZoneData? zoneData;
        public List<Vector3>? boxPositions;

        public Trial(Vector3 trialPosition, string trialLongName, string trialServerName, TrialType trialType, ZoneData zoneData = null, List<Vector3> boxPositions = null)
        {
            position = trialPosition;
            TrialLongName = trialLongName;
            TrialServerName = trialServerName;
            TrialType = (int)trialType;
        }
    }
}