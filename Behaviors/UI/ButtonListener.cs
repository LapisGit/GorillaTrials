using System;
using UnityEngine;

namespace GorillaTrials.Behaviors.UI
{
    public class ButtonListener : MonoBehaviour
    {
        private void Start()
        {
            Button[] buttons = FindObjectsOfType<Button>();

            foreach (Button button in buttons)
            {
                button.OnPress += HandleButtonPress;
            }
        }

        private void HandleButtonPress(GorillaTriggerColliderHandIndicator hand, GameObject buttonObj)
        {
            string objectPath = GetGameObjectPath(buttonObj);
            Debug.Log($"Button Pressed: {objectPath}, Pressed by: {(hand.isLeftHand ? "Left Hand" : "Right Hand")}");

            if (objectPath == "ForestZoneTrial1(Copy)/Stool/Button")
            {
                ForestZoneTrial1Start();
            }
        }

        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "null";
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }

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
