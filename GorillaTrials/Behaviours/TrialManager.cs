using BepInEx;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTrials.Models;
using GorillaTrials.Models.StateMachine;
using GorillaTrials.Tools;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaTrials.Behaviours
{
    internal class TrialManager : Singleton<TrialManager>
    {
        public Trial CurrentTrial => currentTrial;
        public bool Started => currentTrial is not null;
        public List<Trial> Trials => trials;
        public string refreshBoard = null;
        public Trial currentTrial;
        private readonly List<Trial> trials = [];
        public GameObject trialAssets, trialUIAsset, trialBoxAsset, achievementsUI, trialZoneAsset, leftHand, rightHand, Head;
        public string trialResultBackup;

        private bool isPB = false;
        public string lastTrialPlayed;
        
        private Dictionary<string, string> rankedTrialIds = new Dictionary<string, string>();
        private bool rankedIdsLoaded = false;

        public JsonSerializerSettings SerializeSettings { get; private set; }

        public JsonSerializerSettings DeserializeSettings { get; private set; }

        public async override void Initialize()
        {
            Vector3Converter vector3Converter = new();
            QuaternionConverter quaternionConverter = new();

            SerializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                CheckAdditionalContent = true,
                Formatting = Formatting.None
            };
            SerializeSettings.Converters.Add(vector3Converter);
            SerializeSettings.Converters.Add(quaternionConverter);

            DeserializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            DeserializeSettings.Converters.Add(vector3Converter);
            DeserializeSettings.Converters.Add(quaternionConverter);

            trialAssets = await AssetLoader.LoadAsset<GameObject>("GorillaTrials");
            trialUIAsset = trialAssets.transform.Find("Trial").gameObject;
            trialBoxAsset = trialAssets.transform.Find("Trial Box").gameObject;
            trialZoneAsset = trialAssets.transform.Find("Trial Zone").gameObject;
            leftHand = GTPlayer.Instance.LeftHand.handFollower.gameObject;
            rightHand = GTPlayer.Instance.RightHand.handFollower.gameObject;
            Head = GTPlayer.Instance.headCollider.gameObject;

            string url = "https://suwiparty.lapis.codes/Trial.json";
            using UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Fatal($"Failed to load trials JSON: {request.error}");
                return;
            }

            List<TrialDataModel> trialsData = JsonConvert.DeserializeObject<List<TrialDataModel>>(request.downloadHandler.text);

            foreach (var data in trialsData)
            {
                List<Vector3> points = data.points?.ConvertAll(p => p.ToVector3());

                if (!Enum.TryParse<ETrialType>(data.trialType, true, out var trialType))
                    trialType = ETrialType.Box;

                if (!Enum.TryParse<ETrialDifficulty>(data.trialDifficulty, true, out var trialDifficulty))
                    trialDifficulty = ETrialDifficulty.Easy;

                CreateTrial
                (
                    data.displayName,
                    data.trialId,
                    data.position.ToVector3(),
                    data.angle,
                    trialType,
                    trialDifficulty,
                    data.maxTime,
                    data.customMapTrial,
                    [points],
                    data.bronzeTime,
                    data.silverTime,
                    data.goldTime
                );
            }
            
            await LoadRankedTrialIds();
            LoadDownloadedTrials();
        }
        
        private async Task LoadRankedTrialIds()
        {
            string url = $"{Constants.ServerURL}/trials/rankedids";
            
            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    TaskCompletionSource<UnityWebRequest> completionSource = new();
                    StartCoroutine(YieldWebRequest(request, completionSource));
                    await completionSource.Task;
                    
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Logging.Warning($"Failed to fetch ranked trial IDs: {request.error}");
                        return;
                    }
                    
                    string jsonResponse = request.downloadHandler.text;
                    rankedTrialIds = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
                    rankedIdsLoaded = true;
                    Logging.Info($"Loaded {rankedTrialIds.Count} ranked trial IDs");
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Error loading ranked trial IDs: {ex.Message}");
            }
        }
        
        private IEnumerator YieldWebRequest(UnityWebRequest webRequest, TaskCompletionSource<UnityWebRequest> completionSource)
        {
            yield return webRequest.SendWebRequest();
            completionSource.SetResult(webRequest);
        }

        private void LoadDownloadedTrials()
        {
            string executableDir = System.IO.Path.GetDirectoryName(Paths.ExecutablePath);
            if (string.IsNullOrEmpty(executableDir))
            {
                Logging.Error("Failed to get executable directory path for loading downloaded trials");
                return;
            }

            string downloadedTrialsDir = System.IO.Path.Combine(executableDir, "downloadedtrials");

            if (!System.IO.Directory.Exists(downloadedTrialsDir))
            {
                Logging.Info("No downloadedtrials folder found, skipping downloaded trials loading");
                return;
            }

            string[] trialFiles = System.IO.Directory.GetFiles(downloadedTrialsDir, "*.json");

            foreach (string filePath in trialFiles)
            {
                try
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    TrialDataModel data = JsonConvert.DeserializeObject<TrialDataModel>(json);

                    if (data == null)
                    {
                        Logging.Warning($"Failed to parse trial file: {System.IO.Path.GetFileName(filePath)}");
                        continue;
                    }
                    
                    if (trials.Any(t => t.TrialServerName == data.trialId))
                    {
                        Logging.Info($"Trial {data.trialId} already exists, skipping");
                        continue;
                    }
                    
                    bool isRanked = rankedIdsLoaded && rankedTrialIds.ContainsKey(data.trialId);
                    bool needsUpdate = false;
                    
                    if (isRanked)
                    {
                        string friendlyId = rankedTrialIds[data.trialId];
                        Logging.Info($"Trial {data.trialId} is ranked with friendly ID: {friendlyId}");
                        
                        if (data.customMapTrial)
                        {
                            data.customMapTrial = false;
                            needsUpdate = true;
                            Logging.Info($"Updated trial {data.trialId} to no longer be a custom map trial");
                        }
                        
                        if (data.trialId != friendlyId)
                        {
                            data.trialId = friendlyId;
                            needsUpdate = true;
                            Logging.Info($"Updated trial ID to friendly ID: {friendlyId}");
                        }
                    }
                    
                    if (needsUpdate)
                    {
                        string updatedJson = JsonConvert.SerializeObject(data, Formatting.Indented);
                        System.IO.File.WriteAllText(filePath, updatedJson);
                        Logging.Info($"Saved updated trial data to: {filePath}");
                    }

                    List<Vector3> points = data.points?.ConvertAll(p => p.ToVector3());

                    if (!Enum.TryParse<ETrialType>(data.trialType, true, out var trialType))
                        trialType = ETrialType.Box;

                    if (!Enum.TryParse<ETrialDifficulty>(data.trialDifficulty, true, out var trialDifficulty))
                        trialDifficulty = ETrialDifficulty.Easy;

                    Logging.Info($"Loading downloaded trial '{data.displayName}' with medal times: Bronze={data.bronzeTime}, Silver={data.silverTime}, Gold={data.goldTime}");

                    CreateTrial
                    (
                        data.displayName,
                        data.trialId,
                        data.position.ToVector3(),
                        data.angle,
                        trialType,
                        trialDifficulty,
                        data.maxTime,
                        data.customMapTrial,
                        [points],
                        data.bronzeTime,
                        data.silverTime,
                        data.goldTime,
                        System.IO.Path.GetFileName(filePath)
                    );
                }
                catch (Exception ex)
                {
                    Logging.Error($"Error loading trial file {System.IO.Path.GetFileName(filePath)}: {ex.Message}");
                }
            }
        }


        public void CreateTrial(string displayName, string trialId, Vector3 position, float angle, ETrialType trialType = ETrialType.Box, ETrialDifficulty trialDifficulty = ETrialDifficulty.Easy, float maxTime = 0, bool customMapTrial = false, object[] parameters = null, float bronzeTime = 0, float silverTime = 0, float goldTime = 0, string downloadedFileName = null, bool isPlaytest = false)
        {
            Trial trial = null;

            if (trialType == ETrialType.Box && parameters is not null && parameters.ElementAtOrDefault(0) is List<Vector3> points)
                trial = new(position, angle, displayName, trialId, trialType, trialDifficulty, maxTime, null, customMapTrial, points, bronzeTime, silverTime, goldTime, downloadedFileName, isPlaytest);
            else if (trialType == ETrialType.Zone && parameters?.ElementAtOrDefault(0) is List<Vector3> zonePoints)
                trial = new(position, angle, displayName, trialId, trialType, trialDifficulty, maxTime, null, customMapTrial, zonePoints, bronzeTime, silverTime, goldTime, downloadedFileName, isPlaytest);


            if (trial is not null)
            {
#if DEBUG
                Logging.Info($"Created trial '{displayName}' ({trialId})");
#endif
                trials.Add(trial);
                StartCoroutine(trial.GetLeaderboardCoroutine(trialId)); ;
                return;
            }


            Logging.Fatal($"TRIAL FOR {trialId} IS NULL!");
            Logging.Error($"Type: {trialType}");
            Logging.Error($"Parameter Count: {(parameters is null ? "null" : parameters.Length)}");
        }

        public void StartTrial(Trial trialData)
        {
            if (Started)
            {
                return;
            }

            ControlPanel.IncrementTrialsAttempted();
            
            currentTrial = trialData;
            currentTrial.stateMachine.SwitchState(new Trial_Start(currentTrial));
            ReplayManager.Instance.SetTrackedObjects(Head, leftHand, rightHand);
            ReplayManager.Instance.StartRecording();
        }

        public void EndTrial(double? submitTime)
        {
            if (!Started)
                return;

            if (submitTime.HasValue)
            {
                if (currentTrial.isFromCustomMap && !currentTrial.onApprovedMap)
                {
                    Logging.Info("Trial was created by a Custom Map and was not approved, not submitting a time.");
                    UpdateLocalPersonalBest(submitTime.Value);
                }
                else
                {
                    Logging.Info($"Submiting time {submitTime.Value}");
                    ControlPanel.IncrementTrialsCompleted();
                    SubmitTrial(submitTime.Value);
                }
            }

            StartCoroutine(currentTrial.GetLeaderboardCoroutine(currentTrial.TrialServerName));

            if (submitTime.HasValue)
            {
                AchievementChecker.instance.UpdateAchievements(submitTime.Value, currentTrial);
            }
            lastTrialPlayed = currentTrial.TrialServerName;

            currentTrial = null;
        }

        private void UpdateLocalPersonalBest(double submitTime)
        {
            if (currentTrial == null)
            {
                Logging.Warning("No current trial when updating local PB.");
                return;
            }

            string pbKey = string.Concat("PB_", currentTrial.TrialServerName);
            Logging.Info(PlayerPrefs.GetFloat(pbKey, 0));
            if (submitTime < PlayerPrefs.GetFloat(pbKey, 0) || PlayerPrefs.GetFloat(pbKey, 0) == 0)
            {
                isPB = true;
                Logging.Info($"New personal best for {currentTrial.TrialServerName}: {submitTime} seconds");
                PlayerPrefs.SetFloat(pbKey, (float)submitTime);
                PlayerPrefs.Save();
                ControlPanel.instance.CalculateSumOfBest();
                currentTrial.SetPersonalBest(submitTime);
                ReplayManager.Instance.StopRecording();
                ReplayManager.Instance.SaveRecording($"{currentTrial.TrialServerName}_{submitTime}");

                BadgeType earnedBadge = currentTrial.CheckBadgeEarned((float)submitTime);
                bool newBadgeEarned = currentTrial.SaveBadgeIfBetter(earnedBadge);

                if (Plugin.PBNotify.Value)
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(submitTime);
                    string hudText = $"New PB! {string.Concat("PB: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"))}";

                    if (newBadgeEarned && earnedBadge != BadgeType.None)
                    {
                        hudText += $"\n{earnedBadge} Badge Earned!";
                    }

                    HUDManager.instance.SetHUDText(hudText);
                }
            }
            else
            {
                float currentPB = PlayerPrefs.GetFloat(pbKey, 0);
                if (currentPB > 0)
                {
                    BadgeType earnedBadge = currentTrial.CheckBadgeEarned(currentPB);
                    currentTrial.SaveBadgeIfBetter(earnedBadge);
                }
            }
        }

        public void SubmitTrial(double submitTime)
        {
            string pbKey = string.Concat("PB_", currentTrial.TrialServerName);
            refreshBoard = currentTrial.TrialServerName;
            UpdateLocalPersonalBest(submitTime);

            string playerName = NetworkSystem.Instance.GetMyNickName().ToUpper();
            playerName = playerName[..Math.Min(playerName.Length, 12)];
            string playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();

            string jsonBody = JsonUtility.ToJson(new TrialResult
            {
                PlayerName = playerName,
                Time = submitTime,
                PlayerId = playerId
            });

            trialResultBackup = jsonBody;

            StartCoroutine(PostRequest
                (
                    currentTrial,
                    string.Concat($"{Constants.ServerURL}/leaderboard/", currentTrial.TrialServerName),
                    jsonBody)
            );
            
            if (!string.IsNullOrEmpty(currentTrial.selectedChallengeRecipientId))
            {
                StartCoroutine(currentTrial.SendChallengeAfterCompletion(submitTime));
            }
            
            if (currentTrial.hasAcceptedChallenge && !string.IsNullOrEmpty(currentTrial.acceptedChallengeId))
            {
                StartCoroutine(currentTrial.CompleteChallengeAfterCompletion(submitTime));
            }
            
            currentTrial.SetLastTime(submitTime);
            currentTrial.UsePlayerInfo(true);

            Logging.Info(GetTrialsWithPBCount(trials));
        }

        private IEnumerator PostRequest(Trial trial, string url, string json)
        {
            string apiKey = Plugin.APIKey.Value;

            UnityWebRequest request = new(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Fatal($"Trial post error {request.responseCode}: {request.error}");

                if (request.responseCode == 0)
                {
                    StartCoroutine(WaitDelay(5f));
                    StartCoroutine(PostRequest
                        (
                            trial,
                            string.Concat($"{Constants.ServerURL}/leaderboard/", trial.TrialServerName),
                            trialResultBackup)
                    );
                }

                Logging.Error(request.downloadHandler.text);
                yield break;
            }
            refreshBoard = "";
            Logging.Info("Trial results uploaded");
            
            TrialResult result = JsonConvert.DeserializeObject<TrialResult>(json);
            Task.Run(() => ThreadingHelper.Instance.StartSyncInvoke(async () =>
            {
                await ReplayManager.Instance.UploadReplayWR(lastTrialPlayed, result.PlayerId, result.Time);
            }));
            
            if (isPB)
            {
                isPB = false;
            }
        }

        private IEnumerator WaitDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
        }

        public static int GetTrialsWithPBCount(List<Trial> allTrials)
        {
            int count = 0;

            foreach (var trial in allTrials)
            {
                float pb = PlayerPrefs.GetFloat(string.Concat("PB_", trial.TrialServerName), 0);

                if (pb > 0)
                    count++;
            }

            return count;
        }
        
        public static int GetTrialsWithBadgesConfigured(List<Trial> allTrials)
        {
            int count = 0;

            foreach (var trial in allTrials)
            {
                if (trial.BronzeTime > 0 && trial.SilverTime > 0 && trial.GoldTime > 0)
                    count++;
            }

            return count;
        }
        public static int GetTotalBadgeCount(BadgeType badgeType)
        {
            if (badgeType == BadgeType.None)
                return 0;
                
            string key = $"Total_{badgeType}Badges";
            return PlayerPrefs.GetInt(key, 0);
        }

        public void RefreshAcceptedChallenges()
        {
            foreach (var trial in trials)
            {
                try
                {
                    trial.LoadAcceptedChallenge();
                }
                catch (Exception ex)
                {
                    Logging.Error($"Failed to refresh accepted challenge for {trial.TrialServerName}: {ex.Message}");
                }
            }
        }

        public void DeleteAllPlaytestTrials()
        {
            List<Trial> playtestTrials = trials.Where(t => t.isPlaytest).ToList();
            
            foreach (Trial trial in playtestTrials)
            {
                try
                {
                    if (Started && currentTrial == trial)
                    {
                        currentTrial.stateMachine.SwitchState(new Trial_End(trial, false));
                    }
                    
                    if (trial.trialUIObject != null)
                    {
                        Destroy(trial.trialUIObject);
                    }

                    trials.Remove(trial);
                    Logging.Info($"Deleted playtest trial: {trial.TrialServerName}");
                }
                catch (Exception ex)
                {
                    Logging.Error($"Error deleting playtest trial {trial.TrialServerName}: {ex.Message}");
                }
            }
        }
    }

}
