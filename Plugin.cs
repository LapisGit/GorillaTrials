using BepInEx;
using System.IO;
using UnityEngine;
using System.Collections;
using GorillaTrials.Behaviors.UI;

namespace GorillaTrials
{
    [BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static GameObject loadedPrefab;

        void Awake()
        {

            Logger.LogInfo("Waiting for game to load to run LoadChallenges...");
            //StartCoroutine(WaitForGameLoad());
            GorillaTagger.OnPlayerSpawned(() =>
            {
                Load();
            });
        }

        IEnumerator WaitForGameLoad()
        {
            while (GameObject.Find("Player Objects") == null)
            {
                yield return new WaitForSeconds(1f);
            }
            Load();
        }

        public void Load()
        {
            GameObject manager = new GameObject("ButtonManager");
            manager.AddComponent<ButtonListener>();
            DontDestroyOnLoad(manager);
            Logger.LogInfo("Game Loaded! Now initializing mod...");
            LoadChallenges.LoadAssetBundle();
            FindAndModifyButton();


        }

        public GameObject buttonObject;
        public void FindAndModifyButton()
        {
            // Locate the button in the scene
            buttonObject = GameObject.Find("ForestZoneTrial1(Copy)/Stool/Button");

            if (buttonObject == null)
            {
                Logger.LogError("Button not found in scene!");
                return;
            }

            // Ensure it's on the correct layer
            if (buttonObject.layer != 18)
            {
                Logger.LogWarning("Button is not on the GorillaInteraction layer!");
            }

            // Locate the FinishZone object that needs to be activated
            GameObject finishZone = GameObject.Find("ForestZoneTrial1(Copy)/FinishZone");
            if (finishZone == null)
            {
                Logger.LogError("FinishZone not found in scene!");
                return;
            }

            // Add the Button script if not already present
            Button buttonScript = buttonObject.GetComponent<Button>();
            if (buttonScript == null)
                buttonScript = buttonObject.AddComponent<Button>();

            // Assign an action to log a message and activate FinishZone
            buttonScript.action = () =>
            {
                Logger.LogInfo("Trial Button Pressed!");
                finishZone.SetActive(true);  // Activate FinishZone
            };

        }
    }
}
