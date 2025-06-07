using System;
using System.Collections.Generic;
using System.Text;
using GorillaTrials.Behaviors;
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
        public static int boxesCollected;
        public static int boxesToCollect;

        public static float trialTime;

        public static Trial currentTrial;
        
        public static int index = 0;
        public static void StartTrial(Trial trialData)
        {
            if (trialStarted == true)
            {
                return;
            }

            currentTrial = trialData;
            currentTrial.trialObject.SetActive(false);

            trialTime = Time.time;

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
            index = 0;
            boxesCollected = 0;
            boxesToCollect = currentTrial.boxPositions.Count;
            if (currentTrial != null)
            {
                if (currentTrial.boxPositions == null)
                {
                    Debug.LogError("Current trial box positions are null.");
                    return;
                }
                
                foreach (Vector3 boxPosition in currentTrial.boxPositions)
                {
                    GameObject box = Instantiate(LoadTrials.TrialBoxPrefab);
                    box.SetActive(true);
                    box.layer = 15; //Gorilla Boundary layer (body collider only triggers this with OnTriggerEnter iirc)

                    box.GetComponent<SphereCollider>().isTrigger = true;

                    TrialBoxCollider trialBoxCollider = box.AddComponent<TrialBoxCollider>();
                    trialBoxCollider.index = index;
                    trialBoxCollider.trialservername = currentTrial.TrialServerName;

                    box.transform.position = boxPosition;
                    box.name = $"Box_{index}";
                    Debug.Log("Instantiated box (" + index + ")");
                    index++;
                }
            }
            else
            {
                Debug.LogError("Current trial is null.");
            }
        }

        public static void EndTrial()
        {
            Debug.Log("Trial " + currentTrial.TrialLongName + " completed!");

            for (int i = 0; i < index; i++)
            {
                string boxName = $"Box_{i}";
                GameObject box = GameObject.Find(boxName);
                if (box != null)
                {
                    Destroy(box);
                }
            }

            trialStarted = false;
            index = 0;
            currentTrial.trialObject.SetActive(true);
            currentTrial = null; //keep this line at the end of the method kplsthx :3
        }
    }
}
