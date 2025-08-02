using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GorillaNetworking;
using GorillaTrials.Behaviours;
using GorillaTrials.Behaviours.Networking;
using GorillaTrials.Models;
using GorillaTrials.Tools;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaTrials
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger;

        public static new ConfigFile Config;
        public static ConfigEntry<string> APIKey;
        public static ConfigEntry<bool> PBNotify;

        public static bool WrongVersion;
        public static AchievementManager achievementManager;

        public void Awake()
        {
            Logger = base.Logger;

            Config = base.Config;
            achievementManager = new AchievementManager(Config);
            
            achievementManager.RegisterAchievement(new Achievement("first_trial", "First Trial!", "Complete your first trial!"));
            achievementManager.RegisterAchievement(new Achievement("stump_climb_champ", "Stump Climb Champion!", "Complete the 'Stump Climb' trial in under 11 seconds."));
            achievementManager.RegisterAchievement(new Achievement("adv_hp2", "Hoverpark 2 Sprint Master", "Complete the 'Hoverpark 2 Sprint Advanced' trial."));
            achievementManager.RegisterAchievement(new Achievement("5trials", "5 Trials", "Complete 5 Trials"));
            achievementManager.RegisterAchievement(new Achievement("10trials", "10 Trials", "Complete 10 Trials"));
            achievementManager.RegisterAchievement(new Achievement("20trials", "20 Trials", "Complete 20 Trials"));
            achievementManager.RegisterAchievement(new Achievement("30trials", "30 Trials", "Complete 30 Trials"));
            achievementManager.RegisterAchievement(new Achievement("vinemaster", "Vine Master", "Complete the 'Swinging Around' trial in under 10 seconds."));
            achievementManager.RegisterAchievement(new Achievement("masterswimmer", "Master Swimmer", "Complete the 'Master Swimmer' trial."));
            achievementManager.RegisterAchievement(new Achievement("slowpoke", "Slowpoke", "Take over 2 minutes to complete a trial."));
            achievementManager.RegisterAchievement(new Achievement("ultraslowpoke", "Ultra Slowpoke", "Take over 2 minutes to complete a trial."));
            APIKey = Config.Bind
            (
                "Server",
                "APIKey",
                "Your-API-Key-Here",
                "The API key used to authenticate server requests for trials. DO NOT SEND YOUR KEY TO ANYONE!"
            );
            PBNotify = Config.Bind
            (
                "Gameplay",
                "PB Notify",
                true,
                "If true, the HUD will notify you if you get a Personal Best on a trial."
            );
            
            
            GorillaTagger.OnPlayerSpawned(() =>
            {
                
                
                GameObject root = new(Constants.Name);
                DontDestroyOnLoad(root);
                root.AddComponent<TrialManager>();
                root.AddComponent<NetworkHandler>();
                root.AddComponent<EarlyEnd>();
                //root.AddComponent<TimeManager>();
                root.AddComponent<AchievementUI>();
                root.AddComponent<CustomMapManager>();
                root.AddComponent<AchievementChecker>();
                root.AddComponent<HUDManager>();
                HUDManager.instance.Init();
#if DEBUG
                root.AddComponent<DebugEditor>();
#endif
                CompareVersion("https://raw.githubusercontent.com/LapisGit/GorillaTrials/refs/heads/main/version.txt", version =>
                {
#if RELEASE
                    WrongVersion = version == EVersionCompareResult.Outdated;
#endif
                });
            });
            
            
            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.GUID);
        }

        public async void CompareVersion(string url, Action<EVersionCompareResult> onVersionRecieved)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            TaskCompletionSource<UnityWebRequest> completionSource = new();

            StartCoroutine(YieldWebRequest(request, completionSource));
            await completionSource.Task;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogFatal($"Failed to check version {url}");
                Logger.LogError(request.error);

                onVersionRecieved?.Invoke(EVersionCompareResult.Invalid);
                return;
            }

            Version current = new(Constants.Version);
            Version remote = new(request.downloadHandler.text.Trim());

            onVersionRecieved?.Invoke(remote > current ? EVersionCompareResult.Outdated : EVersionCompareResult.UpToDate);
        }

        private IEnumerator YieldWebRequest(UnityWebRequest webRequest, TaskCompletionSource<UnityWebRequest> completionSource)
        {
            yield return webRequest.SendWebRequest();
            completionSource.SetResult(webRequest);
        }
    }
}

