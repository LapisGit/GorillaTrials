using BepInEx;
using BepInEx.Configuration;
using GorillaTrials.Behaviors;
using GorillaTrials.Models;
using UnityEngine;

namespace GorillaTrials
{
    [BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static LoadTrials trials;
        public static ConfigEntry<string> apiKeyEntry;
        
        void Awake()
        {
            GorillaTagger.OnPlayerSpawned(() => { Load(); });
            
            apiKeyEntry = Config.Bind(
                "Server",
                "APIKey",
                "Your-Default-API-Key-Here",
                "The API key used to authenticate HTTP requests for trials."
            );
        }

        public void Load()
        {
            //Trials.Initialize();
#if DEBUG
            gameObject.AddComponent<DebugEditor>();
            TrialPositions.Initialize();
#endif
            GameObject tempTrialsObj = new GameObject("GorillaTrials Challenges");
            tempTrialsObj.AddComponent<LoadTrials>();
            trials = tempTrialsObj.GetComponent<LoadTrials>();
            LoadTrials.LoadAssetBundle();
        }
    }
}
    
