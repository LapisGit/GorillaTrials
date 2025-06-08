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
                CheckVersion();
            });
            
            async void CheckVersion()
            {
                string url = "https://raw.githubusercontent.com/LapisGit/GorillaTrials/refs/heads/main/version.txt";

                using var request = UnityEngine.Networking.UnityWebRequest.Get(url);
                var asyncOp = request.SendWebRequest();

                while (!asyncOp.isDone)
                    await System.Threading.Tasks.Task.Yield();
                
                if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Logger.LogWarning($"[Version Check] Failed to fetch version info: {request.error}");
                    return;
                }

                string remoteVersion = request.downloadHandler.text.Trim();

                if (remoteVersion != Constants.Version)
                {
                    Logger.LogWarning($"[Version Check] Your version ({Constants.Version}) is out of date! Latest is {remoteVersion}.");
                    Constants.UpToDate = false;
                }
                else
                {
                    Logger.LogInfo($"[Version Check] Plugin is up to date. Version: {Constants.Version}");
                    Constants.UpToDate = true;
                }
            }


            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.GUID);
        }
    }
}

