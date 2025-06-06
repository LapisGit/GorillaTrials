using BepInEx;
using UnityEngine;

namespace GorillaTrials
{
    [BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static LoadChallenges challenges;
        
        void Awake()
        {
            GorillaTagger.OnPlayerSpawned(() => { Load(); });
        }

        public void Load()
        {
            GameObject tempChallengesObj = new GameObject("GorillaTrials Challenges");
            tempChallengesObj.AddComponent<LoadChallenges>();
            challenges = tempChallengesObj.GetComponent<LoadChallenges>();
            LoadChallenges.LoadAssetBundle();
        }
    }
}
    
