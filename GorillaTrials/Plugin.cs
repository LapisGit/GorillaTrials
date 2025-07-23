using System;
using System.Collections;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GorillaTrials.Behaviours;
using GorillaTrials.Behaviours.Networking;
using GorillaTrials.Models;
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

        public static bool WrongVersion;

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
                root.AddComponent<EarlyEnd>();
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

