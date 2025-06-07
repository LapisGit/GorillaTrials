using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CjLib;
using GorillaLocomotion;
using GorillaTrials.Models;
using UnityEngine;

namespace GorillaTrials.Behaviors
{
    public class DebugEditor : MonoBehaviour
    {
#if DEBUG
        public Rect windowRect = new Rect(20, 20, 350, 500);

        public List<Vector3> boxPositions;
        public string trialName = "DebugTrial";
        public TrialType trialType = TrialType.Box;

        void Awake()
        {
            boxPositions = new List<Vector3>();
        }

        public void Update()
        {
            foreach(Vector3 boxPos in boxPositions)
            {
                DebugUtil.DrawBox(boxPos, Quaternion.identity, Vector3.one, Color.magenta, false);
            }
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
            GUILayout.Label("Trial Type : "+trialType);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Box"))
                trialType = TrialType.Box;
            if (GUILayout.Button("Zone"))
                trialType = TrialType.Zone;
            GUILayout.EndHorizontal();
            GUILayout.Space(40);

            if (trialType == TrialType.Box)
            {
                GUILayout.Box("Box Positions");
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("add box position"))
                {
                    boxPositions.Add(GTPlayer.Instance.bodyCollider.transform.position);
                }
                if (GUILayout.Button("remove last box position"))
                {
                    boxPositions.Remove(boxPositions.Last());
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(40);
            if (GUILayout.Button("Save trial data"))
            {
                string trialTextData = "new Trial(){\n}";
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
#endif
}
