using System;
using System.Collections.Generic;
using System.Text;
using GorillaTrials.Models;
using UnityEngine;

namespace GorillaTrials
{
    public class Trials : MonoBehaviour
    {
        public static List<Trial> All = new List<Trial>()
        {
           // new Trial("")
        };

        public static bool trialStarted;

        public static Trial currentTrial;
        
        public static int index = 0;
        public static void StartTrial(Trial trialData)
        {
            currentTrial = trialData;
            currentTrial.trialObject.SetActive(false);

            switch (currentTrial.TrialType)
            {
                case (int)TrialType.Box:
                    trialStarted = true;
                    PopulateTrialBoxes();
                    break;
                case (int)TrialType.Zone:
                    Debug.LogError("Not implemented yet.  (Guys we're not Another Axiom chill.)");
                    EndTrial();
                    break;
                default:
                    Console.WriteLine("default isn't supposed to be called what.");
                    EndTrial();
                    break;
            }
        }

        public static void PopulateTrialBoxes()
        {
            if (currentTrial != null)
            {
                if (currentTrial.boxPositions == null) //literally should never happen unless in debugging.
                    return;
                
                foreach (Vector3 boxPosition in currentTrial.boxPositions)
                {
                    GameObject box = Instantiate(LoadTrials.TrialBoxPrefab);
                    box.transform.position = boxPosition;
                    box.name = $"Box_{index}";
                    index++;
                }

            }
        }

        public static void EndTrial()
        {
            trialStarted = false;
            for (int i = 0; i < index; i++)
            {
                string boxName = $"Box_{i}";
                GameObject box = GameObject.Find(boxName);
                if (box != null)
                {
                    GameObject.Destroy(box);
                }
            }
            index = 0;
            currentTrial = null; //keep this line at the end of the method kplsthx :3
        }
    }
}
