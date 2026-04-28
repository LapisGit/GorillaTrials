using GorillaNetworking;
using GorillaTrials.Behaviours;
using GorillaTrials.Behaviours.Networking;
using GorillaTrials.Models;
using GorillaTrials.Tools;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GorillaGameModes;
using GorillaLibrary.Attributes;
using GorillaTrials;
using MelonLoader;
using MelonLoader.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[assembly: MelonInfo(typeof(Plugin), "GorillaTrials", "1.5.0", "Lapis, dev9998, Mia")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
[assembly: MelonAdditionalDependencies("GorillaLibrary")]
[assembly: WardrobeCategory("Badges", typeof(BadgeWardrobe))]
namespace GorillaTrials
{
    [ModdedGamemode("gtrials", "GORILLATRIALS", GameModeType.Casual)]
    public class Plugin : MelonMod
    {
        public static new MelonPreferences_Category Config;
        public static MelonPreferences_Entry<string> APIKey;
        public static MelonPreferences_Entry<bool> PBNotify;
        public static MelonPreferences_Entry<float> EarlyEndTime;

        public static bool WrongVersion;
        public static bool InModdedGamemode;
        public static AchievementManager achievementManager;
        
        internal static WebSocketClient WebSocketClientInstance;

        public override void OnInitializeMelon()
        {
            Config = MelonPreferences.CreateCategory("GorillaTrials");

            string configPath = Path.Combine(MelonEnvironment.UserDataDirectory, "GorillaTrials.cfg");
            Config.SetFilePath(configPath);
            Config.LoadFromFile();
        }

        public override void OnLateInitializeMelon()
        {
            achievementManager = new AchievementManager(Config);

            achievementManager.RegisterAchievement(new Achievement("first_trial", "First Trial!",
                "Complete your first offical trial!"));
            achievementManager.RegisterAchievement(new Achievement("stump_climb_champ", "Stump Climb Champion!",
                "Complete the 'Stump Climb' trial in under 11 seconds."));
            achievementManager.RegisterAchievement(new Achievement("5trials", "5 Trials", "Complete 5 unique offical Trials"));
            achievementManager.RegisterAchievement(new Achievement("10trials", "10 Trials", "Complete 10 unique offical Trials"));
            achievementManager.RegisterAchievement(new Achievement("20trials", "20 Trials", "Complete 20 unique offical Trials"));
            achievementManager.RegisterAchievement(new Achievement("30trials", "30 Trials", "Complete 30 unique offical Trials"));
            achievementManager.RegisterAchievement(new Achievement("goldhoarder", "Gold Hoarder", "Get a Gold Medal on all trials."));
            achievementManager.RegisterAchievement(new Achievement("vinemaster", "Vine Master",
                "Complete the 'Swinging Around' trial in under 10 seconds."));
            achievementManager.RegisterAchievement(new Achievement("adv_hp2", "Hoverpark 2 Sprint Master",
                "Complete the 'Hoverpark 2 Sprint Advanced' trial."));
            achievementManager.RegisterAchievement(new Achievement("slowpoke", "Slowpoke",
                "Take over 2 minutes to complete a trial."));
            achievementManager.RegisterAchievement(new Achievement("ultraslowpoke", "Ultra Slowpoke",
                "Take over 5 minutes to complete a trial."));
            achievementManager.RegisterAchievement(new Achievement("trialanderror", "Trial and Error", "Attempt Trials 50 times."));
            achievementManager.RegisterAchievement(new Achievement("dedication", "Dedication", "Attempt Trials 100 times."));
            achievementManager.RegisterAchievement(new Achievement("perseverance", "Perseverance", "Attempt Trials 200 times."));
            achievementManager.RegisterAchievement(new Achievement("timeandtimeagain", "Time and Time Again", "Attempt Trials 500 times."));
            achievementManager.RegisterAchievement(new Achievement("giveup", "Give Up", "Attempt Trials 1000 times."));
            achievementManager.RegisterAchievement(new Achievement("gooutside", "Go Outside", "Attempt Trials 2500 times."));
            achievementManager.RegisterAchievement(new Achievement("pbpro", "PB Pro", "Complete trials 20 times."));
            achievementManager.RegisterAchievement(new Achievement("whatarethose", "WHAT ARE THOSE!!??", "Complete trials 100 times."));
            achievementManager.RegisterAchievement(new Achievement("trialmaster", "Trial Master", "Complete trials 500 times."));
            APIKey = Config.CreateEntry
            (
                "apikey",
                "Your-API-Key-Here",
                "Server API Key",
                "The API key used to authenticate server requests for trials. DO NOT SEND YOUR KEY TO ANYONE!"
            );
            PBNotify = Config.CreateEntry
            (
                "pbnotify",
                true,
                "PB Notify",
                "If true, the HUD will notify you if you get a Personal Best on a trial."
            );
            EarlyEndTime = Config.CreateEntry
            (
                "earlyendtime",
                1.5f,
                "Early End Time",
                "The value in seconds that determines how long you have to hold your primary face button to end a trial early."
            );
            Config.SaveToFile();
            
            WebSocketClientInstance = new WebSocketClient();
            WebSocketClientInstance.Start();

            GorillaTagger.OnPlayerSpawned(() =>
            {


                GameObject root = new(Constants.Name);
                UnityEngine.Object.DontDestroyOnLoad(root);
                root.AddComponent<TrialManager>();
                root.AddComponent<NetworkHandler>();
                root.AddComponent<EarlyEnd>();
                //root.AddComponent<TimeManager>();
                root.AddComponent<ControlPanel>();
                root.AddComponent<CustomMapManager>();
                root.AddComponent<AchievementChecker>();
                root.AddComponent<HUDManager>();
                root.AddComponent<FirstTimeUIManager>();
                root.AddComponent<RigBadgeManager>();
                HUDManager.instance.Init();
                root.AddComponent<ReplayManager>();
                root.AddComponent<TrialEditor>();
                root.AddComponent<TrialKeyboard>();
#if DEBUG
                //root.AddComponent<DebugEditor>();
#endif
                CompareVersion("https://raw.githubusercontent.com/LapisGit/GorillaTrials/refs/heads/main/version.txt",
                    version =>
                    {
#if RELEASE
                    WrongVersion = version == EVersionCompareResult.Outdated;
#endif
                    });
                MelonCoroutines.Start(PostRequest($"{Constants.ServerURL}/createaccount"));
            });
        }

        public async void CompareVersion(string url, Action<EVersionCompareResult> onVersionRecieved)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            TaskCompletionSource<UnityWebRequest> completionSource = new();

            MelonCoroutines.Start(YieldWebRequest(request, completionSource));
            await completionSource.Task;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Fatal($"Failed to check version {url}");
                Logging.Error(request.error);

                onVersionRecieved?.Invoke(EVersionCompareResult.Invalid);
                return;
            }

            Version current = new(Constants.Version);
            Version remote = new(request.downloadHandler.text.Trim());

            onVersionRecieved?.Invoke(
                remote > current ? EVersionCompareResult.Outdated : EVersionCompareResult.UpToDate);
        }

        private IEnumerator YieldWebRequest(UnityWebRequest webRequest,
            TaskCompletionSource<UnityWebRequest> completionSource)
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


            UnityWebRequest request = new(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (APIKey.Value != "Your-API-Key-Here")
            {
                WebSocketClientInstance.Authenticate(APIKey.Value);
                
                if (FirstTimeUIManager.instance != null && FirstTimeUIManager.instance.UI != null)
                {
                    FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorText").gameObject.SetActive(false);
                    FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/SuccessText").gameObject.SetActive(true);
                    FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/SuccessText")
                        .GetComponent<TextMeshProUGUI>().text = "You already seem to have an account!";
                }
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Fatal($"create account error {request.responseCode}");
                Logging.Error(request.error);
                if (FirstTimeUIManager.instance != null && FirstTimeUIManager.instance.UI != null)
                {
                    FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorText").gameObject.SetActive(true);
                    FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorResponse").gameObject
                        .SetActive(true);
                    FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorResponse").gameObject
                        .GetComponent<TextMeshProUGUI>().text = request.downloadHandler.text;
                }
                Logging.Error(request.downloadHandler.text);
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
                        Config.SaveToFile();
                        WebSocketClientInstance.Authenticate(APIKey.Value);
                        if (FirstTimeUIManager.instance != null && FirstTimeUIManager.instance.UI != null)
                        {
                            FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/SuccessText").gameObject
                                .SetActive(true);
                        }
                    }
                    else
                    {
                        Logging.Error("API key not found in server response.");
                        if (FirstTimeUIManager.instance != null && FirstTimeUIManager.instance.UI != null)
                        {
                            FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorText").gameObject
                                .SetActive(true);
                            FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorResponse").gameObject
                                .SetActive(true);
                            FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/SuccessText").gameObject
                                .SetActive(false);
                            FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorResponse").gameObject
                                .GetComponent<TextMeshProUGUI>().text = "API key not found in server response.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Failed to parse server response");
                    Logging.Error(ex);
                    if (FirstTimeUIManager.instance != null && FirstTimeUIManager.instance.UI != null)
                    {
                        FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorText").gameObject
                            .SetActive(true);
                        FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorResponse").gameObject
                            .SetActive(true);
                        FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/SuccessText").gameObject
                            .SetActive(false);
                        FirstTimeUIManager.instance.UI.transform.Find("StuffLol/Page6/ErrorResponse").gameObject
                                .GetComponent<TextMeshProUGUI>().text =
                            "Failed to parse server response, check logs for more details.";
                    }
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

        [ModdedGamemodeJoin]
        public void OnModdedGamemodeJoin()
        {
            InModdedGamemode = true;
        }

        [ModdedGamemodeLeave]
        public void OnModdedGamemodeLeave()
        {
            InModdedGamemode = false;
        }
    }
}

