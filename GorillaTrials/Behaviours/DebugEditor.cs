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
        public Rect windowRect = new Rect(20, 20, 350, 500);

        public List<Vector3> boxPositions;
        public Quaternion rotation;
        public static string trialName = "DebugTrial";
        public ETrialType trialType = ETrialType.Box;
        public static string ExecutablePath { get; }
        public float yRotation;

        void Awake()
        {
            boxPositions = new List<Vector3>();
        }

        public void Update()
        {
            foreach (Vector3 boxPos in boxPositions)
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
            
            if (GUILayout.Button("get playfab ticket"))
            {
                Logging.Info(PlayFabAuthenticator.instance._sessionTicket);
            }
            
            GUILayout.Label("Trial Name");
            trialName = GUILayout.TextField(trialName);
            GUILayout.Space(20);
            GUILayout.Label("Trial Type : " + trialType);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Box"))
                trialType = ETrialType.Box;
            if (GUILayout.Button("Zone"))
                trialType = ETrialType.Zone;
            GUILayout.EndHorizontal();
            GUILayout.Space(40);

            if (trialType == ETrialType.Box)
            {
                GUILayout.Box("Box Positions");
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("add box position"))
                {
                    if (boxPositions.Count == 0)
                    {
                        yRotation = GTPlayer.Instance.bodyCollider.transform.eulerAngles.y;
                    }

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
                SaveVector3ListToFile(boxPositions, rotation, ExecutablePath + trialName + ".txt");
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        public void SaveVector3ListToFile(List<Vector3> vectors, Quaternion rotation, string filePath)
        {
            using (StreamWriter writer = new(filePath))
            {
                writer.WriteLine($"{trialName} Y Rotation: {yRotation}f");
                writer.WriteLine($"{trialName} = new List<Vector3>();");
                foreach (Vector3 vec in vectors)
                {
                    string line = trialName + $".Add(new Vector3({vec.x}f,{vec.y}f,{vec.z}f));";
                    writer.WriteLine(line);
                }
                
            }

            Debug.Log($"Saved {vectors.Count} vectors to {filePath}");
        }
    }
}
#endif

