using BepInEx;
using System.IO;
using UnityEngine;
using System.Collections;

namespace GorillaTrials
{
    [BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static GameObject loadedPrefab;

        void Awake()
        {
            Logger.LogInfo("GorillaTrials - Waiting for game to load...");
            StartCoroutine(WaitForGameLoad());
        }

        IEnumerator WaitForGameLoad()
        {
            while (GameObject.Find("Player Objects") == null)
            {
                yield return new WaitForSeconds(1f);
            }

            Logger.LogInfo("Game Loaded! Now initializing mod...");
            LoadChallenges.LoadAssetBundle();
        }
    }
}
