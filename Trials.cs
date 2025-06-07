using System;
using System.Collections.Generic;
using System.Text;
using GorillaTrials.Models;
using UnityEngine;

namespace GorillaTrials
{
    public class Trials
    {
        public static List<Trial> All = new List<Trial>()
        {
           // new Trial("")
        };

        public static bool trialStarted;

        public static Trial currentTrial;
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
                    //ahuhawhuahwuahuuhwa :3
                }
            }
        }

        public static void EndTrial()
        {
            trialStarted = false;
            currentTrial = null; //keep this line at the end of the method kplsthx :3
        }
    }
}
