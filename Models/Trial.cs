using System.Collections.Generic;
using GorillaTrials.Behaviors.UI;
using TMPro;
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
        public float y_rotation;
        public GameObject trialObject; //DO NOT SERIALIZE/DESERIALIZE FROM SERVER, THE MOD IS SUPPOSED TO AUTOMATICALLY ASSIGN THIS.
        public string TrialLongName;
        public string TrialServerName;
        public int TrialType; // When deserializing this, make sure to convert the enum on the server (ex: challenge type set to "box") and set it to its corresponding value (ex box challenge type is 0 and zone type is 1, refer to TrialType)
        public ZoneData? zoneData;
        public List<Vector3>? boxPositions;

        public Trial(Vector3 trialPosition, float yRotation, string trialLongName, string trialServerName, TrialType trialType, ZoneData zoneData = null, List<Vector3> boxPositions = null)
        {
            GameObject trialUIObject = GameObject.Instantiate(LoadTrials.TrialUIPrefab);
            trialUIObject.name = trialServerName;
            trialUIObject.transform.position = trialPosition;
            trialUIObject.transform.eulerAngles = new Vector3(0,yRotation,0);
            trialUIObject.transform.Find("UI/Info/TrialName").gameObject.GetComponent<TextMeshProUGUI>().text = trialLongName;
            trialUIObject.transform.Find("UI/Buttons/PlayTrial").gameObject.layer = 18; //Gorilla Interactable
            UIButton trialButton = trialUIObject.transform.Find("UI/Buttons/PlayTrial").AddComponent<UIButton>();
            if (trialType == Models.TrialType.Box)
            {
                trialUIObject.transform.Find("UI/Info/TrialType").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Box Trial";
            }
            else
            {
                trialUIObject.transform.Find("UI/Info/TrialType").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Zone Trial";
            }

            trialObject = trialUIObject;

            position = trialPosition;
            y_rotation = yRotation;
            TrialLongName = trialLongName;
            TrialServerName = trialServerName;
            TrialType = (int)trialType;
            this.zoneData = zoneData;
            this.boxPositions = boxPositions;

            trialButton.onPressed = () =>
            {
                Trials.StartTrial(this);
                Debug.Log("TrialButton pressed!");
            };

            Trials.All.Add(this);
        }
    }
}