using BepInEx;
using GorillaTrials.Behaviors;
using GorillaTrials.Models;
using UnityEngine;

namespace GorillaTrials
{
    [BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static LoadTrials trials;
        
        void Awake()
        {
            GorillaTagger.OnPlayerSpawned(() => { Load(); });
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
    
