using System;
using UnityEngine;

namespace GorillaTrials.Behaviors.UI
{
    public class ButtonListener : MonoBehaviour
    {
        private void ForestZoneTrial1Start()
        {
            GameObject finishZonezc1 = GameObject.Find("ForestZoneTrial1(Copy)/FinishZone");

            if (finishZonezc1 != null)
            {
                Renderer renderer = finishZonezc1.GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = true;

                finishZonezc1.SetActive(true);
            }

        }
    }
}
