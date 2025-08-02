#if DEBUG
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CjLib;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTrials.Models;
using GorillaTrials.Tools;
using UnityEngine;

namespace GorillaTrials.Behaviours
{
    public class DebugEditor : MonoBehaviour
    {
        public Rect windowRect = new Rect(20, 20, 350, 580);

        public List<Vector3> boxPositions;
        public static string trialName = "DebugTrial";
        public ETrialType trialType = ETrialType.Box;

        private Vector3 trialStandPosition = Vector3.zero;
        private float trialStandRotation = 0f;

        void Awake()
        {
            boxPositions = new List<Vector3>();
        }

        public void Update()
        {
            foreach (Vector3 boxPos in boxPositions)
                DebugUtil.DrawBox(boxPos, Quaternion.identity, Vector3.one, Color.magenta, false);
            
            if (trialStandPosition != Vector3.zero)
                DebugUtil.DrawBox(trialStandPosition, Quaternion.Euler(0, trialStandRotation, 0), Vector3.one, Color.green, false);
        }

        public void OnGUI()
        {
            windowRect = GUI.Window(0, windowRect, windowFunc, "GorillaTrials Debug Editor");
        }

        void windowFunc(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Trial Name");
            trialName = GUILayout.TextField(trialName);

            GUILayout.Space(20);
            
            if (GUILayout.Button("Set Trial Stand Position + Rotation"))
            {
                trialStandPosition = GTPlayer.Instance.bodyCollider.transform.position;
                trialStandRotation = GTPlayer.Instance.bodyCollider.transform.eulerAngles.y;
            }

            GUILayout.Space(20);

            if (trialType == ETrialType.Box)
            {
                GUILayout.Box("Box Positions");
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Add box position"))
                    boxPositions.Add(GTPlayer.Instance.bodyCollider.transform.position);

                if (GUILayout.Button("Remove last box position") && boxPositions.Count > 0)
                    boxPositions.RemoveAt(boxPositions.Count - 1);

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Save trial JSON"))
                SaveTrialToJsonFile();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        void SaveTrialToJsonFile()
        {
            var trialData = new TrialJson
            {
                displayName = trialName,
                trialId = trialName.ToLower().Replace(" ", ""),
                position = trialStandPosition == Vector3.zero ? GTPlayer.Instance.bodyCollider.transform.position : trialStandPosition,
                angle = trialStandRotation,
                trialType = trialType.ToString(),
                trialDifficulty = "Easy",
                maxTime = 60,
                customMapTrial = true,
                points = boxPositions
            };

            string json = JsonUtility.ToJson(trialData, true);
            string filePath = Path.Combine(Application.persistentDataPath, $"{trialName}.json");

            File.WriteAllText(filePath, json);
        }

        [System.Serializable]
        public class TrialJson
        {
            public string displayName;
            public string trialId;
            public Vector3 position;
            public float angle;
            public string trialType;
            public string trialDifficulty;
            public float maxTime;
            public bool customMapTrial;
            public List<Vector3> points;
        }
    }
}
#endif
