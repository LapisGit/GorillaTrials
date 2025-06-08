using System;
using System.Collections.Generic;
using System.Diagnostics;
using GorillaTrials.Behaviours;
using GorillaTrials.Behaviours.UI;
using GorillaTrials.Models.StateMachine;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GorillaTrials.Models
{
    public class Trial
    {
        public readonly TrialStateMachine stateMachine = new();

        public readonly Stopwatch stopwatch = new();

        public GameObject trialUIObject;

        public Vector3 position;
        public float y_rotation;
        public GameObject trialObject; //DO NOT SERIALIZE/DESERIALIZE FROM SERVER, THE MOD IS SUPPOSED TO AUTOMATICALLY ASSIGN THIS.
        public string TrialLongName;
        public string TrialServerName;
        public int TrialType; // When deserializing this, make sure to convert the enum on the server (ex: challenge type set to "box") and set it to its corresponding value (ex box challenge type is 0 and zone type is 1, refer to TrialType)
        public TrialZone zoneData;
        public List<Vector3> boxPositions;

        public Trial(Vector3 trialPosition, float yRotation, string trialLongName, string trialServerName, ETrialType trialType, TrialZone zoneData = null, List<Vector3> boxPositions = null)
        {
            trialUIObject = Object.Instantiate(Singleton<TrialManager>.Instance.trialUIAsset);
            trialUIObject.transform.SetParent(Singleton<TrialManager>.Instance.transform);
            trialUIObject.name = trialServerName;
            trialUIObject.transform.position = trialPosition;
            trialUIObject.transform.eulerAngles = new Vector3(0, yRotation, 0);
            trialUIObject.transform.Find("UI/Info/TrialName").gameObject.GetComponent<TextMeshProUGUI>().text = trialLongName;
            trialUIObject.transform.Find("UI/Buttons/PlayTrial").gameObject.layer = 18; //Gorilla Interactable
            TrialButton trialButton = trialUIObject.transform.Find("UI/Buttons/PlayTrial").AddComponent<TrialButton>();

            SetPersonalBest(PlayerPrefs.GetFloat(string.Concat("PB_", trialServerName), 0));

            if (trialType == ETrialType.Box)
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
                Singleton<TrialManager>.Instance.StartTrial(this);
            };
        }

        public void SetPersonalBest(double value)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(value);

            trialUIObject.transform.Find("UI/Info/PB").GetComponent<TextMeshProUGUI>().text = string.Concat("PB: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss") : timeSpan.ToString(@"mm\:ss"));
        }
    }
}