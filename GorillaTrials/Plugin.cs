using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GorillaTrials.Behaviours;
using GorillaTrials.Behaviours.Networking;
using HarmonyLib;
using UnityEngine;

namespace GorillaTrials
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger;

        public static new ConfigFile Config;
        public static ConfigEntry<string> APIKey;

        public void Awake()
        {
            Logger = base.Logger;

            Config = base.Config;
            APIKey = Config.Bind
            (
                "Server",
                "APIKey",
                "Your-API-Key-Here",
                "The API key used to authenticate server requests for trials. DO NOT SEND YOUR KEY TO ANYONE!"
            );

            GorillaTagger.OnPlayerSpawned(() =>
            {
                GameObject root = new(Constants.Name);
                DontDestroyOnLoad(root);

                root.AddComponent<TrialManager>();
                root.AddComponent<NetworkHandler>();
#if DEBUG
                root.AddComponent<DebugEditor>();
#endif
            });

            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.GUID);
        }
    }
}

