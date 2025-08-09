using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Text;
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
using Newtonsoft.Json.Linq;
using TMPro;
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
        private static readonly HttpClient httpClient = new HttpClient();

        public string accountCreationResponseCode;
        public string accountCreationResponse;

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
                root.AddComponent<FirstTimeUIManager>();
                HUDManager.instance.Init();
                root.AddComponent<ReplayManager>();
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
            StartCoroutine(PostRequest($"{Constants.ServerURL}/createaccount"));
            
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
        public IEnumerator PostRequest(string url)
        {
            string playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
            while (string.IsNullOrEmpty(playerId))
            {
                yield return new WaitForSeconds(3f);
                playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
            }

            AccountRequest reqData = new AccountRequest { playerid = playerId };
            string json = JsonUtility.ToJson(reqData);
            Debug.Log("JSON to send: " + json);


            UnityWebRequest request = new(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            
            

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Fatal($"create account error {request.responseCode}"); 
                Logging.Error(request.error);
                FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page2/ErrorText").gameObject.SetActive(true);
                FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page2/ErrorResponse").gameObject
                    .GetComponent<TextMeshProUGUI>().text = request.error;
                Logging.Error(request.downloadHandler.text);
                yield break;
            }

            if (APIKey.Value != "Your-API-Key-Here" && !string.IsNullOrEmpty(APIKey.Value))
            {
                FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page2/SuccessText").gameObject.SetActive(true);
                FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page2/SuccessText").GetComponent<TextMeshProUGUI>().text = "You already seem to already have an account!";
                yield break;
            }
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;

                try
                {
                    AccountResponse response = JsonUtility.FromJson<AccountResponse>(responseText);
                    if (!string.IsNullOrEmpty(response.api_key))
                    {
                        APIKey.Value = response.api_key;
                        Config.Save();
                        FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page2/SuccessText").gameObject.SetActive(true);
                    }
                    else
                    {
                        Logging.Error("API key not found in server response.");
                        FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page2/ErrorText").gameObject.SetActive(true);
                        FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page2/ErrorResponse").gameObject.GetComponent<TextMeshProUGUI>().text = "API key not found in server response.";
                    }
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Failed to parse server response");
                    Logging.Error(ex);
                    FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page2/ErrorText").gameObject.SetActive(true);
                    FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page2/ErrorResponse").gameObject.GetComponent<TextMeshProUGUI>().text = "Failed to parse server response, check logs for more details.";
                }
            }
     
        }
        [Serializable]
        public class AccountRequest
        {
            public string playerid;
        }
        [Serializable]
        public class AccountResponse
        {
            public string message;
            public string api_key;
        }
    }
}

