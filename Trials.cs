using System;
using System.Collections.Generic;
using System.Text;
using GorillaTrials.Models;
using UnityEngine;

namespace GorillaTrials
{
    public class Trials
    {
        public static List<Trial> All;

        public static void Initialize()
        {
            All = new List<Trial>();

#if DEBUG
            All.Add(new Trial()
            {
                TrialName = "testtrial",
                TrialType = (int)TrialType.Box,
                zoneData = null,
                boxPositions = new List<Vector3>()
                {

                }
            });
#endif
        }
    }
}
