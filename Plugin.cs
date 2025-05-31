using BepInEx;
using UnityEngine;

namespace GorillaTrials
{
    [BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static GameObject loadedPrefab;

        void Awake()
        {
            GorillaTagger.OnPlayerSpawned(() => { Load(); });
        }

        public void Load()
        {
            LoadChallenges.LoadAssetBundle();
        }


    }
}
    
