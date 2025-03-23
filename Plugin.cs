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
            StartCoroutine(WaitForGameLoad());
        }

        IEnumerator WaitForGameLoad()
        {
            while (GameObject.Find("Player Objects") == null)
            {
                yield return new WaitForSeconds(1f);
            }
            GameObject manager = new GameObject("ButtonManager");
            manager.AddComponent<ButtonListener>();
            DontDestroyOnLoad(manager);
            Logger.LogInfo("Game Loaded! Now initializing mod...");
            LoadChallenges.LoadAssetBundle();
        }
    }
}
