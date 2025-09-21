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
        public GameObject trialAssets, trialUIAsset, trialBoxAsset, trialZoneAsset, achievementsUI, leftHand, rightHand, Head;
        public string trialResultBackup;

        private bool isPB = false;
        public string lastTrialPlayed;

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
            leftHand = GTPlayer.Instance.leftHandFollower.gameObject;
            rightHand = GTPlayer.Instance.rightHandFollower.gameObject;
            Head = GTPlayer.Instance.headCollider.gameObject;

            string url = "https://raw.githubusercontent.com/LapisGit/GorillaTrials/refs/heads/main/trials.json";
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
                    [points]
                );
            }
        }


        public void CreateTrial(string displayName, string trialId, Vector3 position, float angle, ETrialType trialType = ETrialType.Box, ETrialDifficulty trialDifficulty = ETrialDifficulty.Easy, float maxTime = 0, bool customMapTrial = false, object[] parameters = null)
        {
            Trial trial = null;

            if (trialType == ETrialType.Box && parameters is not null && parameters.ElementAtOrDefault(0) is List<Vector3> points)
                trial = new(position, angle, displayName, trialId, trialType, trialDifficulty, maxTime, null, customMapTrial, points);
            else if (trialType == ETrialType.Zone && parameters?.ElementAtOrDefault(0) is List<Vector3> zonePoints)
                trial = new(position, angle, displayName, trialId, trialType, trialDifficulty, maxTime, null, customMapTrial, zonePoints);


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

            currentTrial = trialData;
            currentTrial.stateMachine.SwitchState(new Trial_Start(currentTrial));
            ReplayManager.Instance.SetTrackedObjects(Head, leftHand, rightHand);
            ReplayManager.Instance.StartRecording();
        }

        public void EndTrial(double? submitTime)
        {
            if (!Started)
                return;

            if (submitTime.HasValue && currentTrial.isFromCustomMap == false || submitTime.HasValue && currentTrial.onApprovedMap)
            {
                Logging.Info($"Submiting time {submitTime.Value}");
                SubmitTrial(submitTime.Value);
            }

            if (currentTrial.isFromCustomMap && !currentTrial.onApprovedMap)
            {
                Logging.Info("Trial was created by a Custom Map and was not approved, not submitting a time.");
            }

            StartCoroutine(currentTrial.GetLeaderboardCoroutine(currentTrial.TrialServerName));

            if (submitTime.HasValue)
            {
                AchievementChecker.instance.UpdateAchievements(submitTime.Value, currentTrial);
            }
            lastTrialPlayed = currentTrial.TrialServerName;

            currentTrial = null;
        }

        public void SubmitTrial(double submitTime)
        {
            string pbKey = string.Concat("PB_", currentTrial.TrialServerName);
            refreshBoard = currentTrial.TrialServerName;
            Logging.Info(PlayerPrefs.GetFloat(pbKey, 0));
            if (submitTime < PlayerPrefs.GetFloat(pbKey, 0) || PlayerPrefs.GetFloat(pbKey, 0) == 0)
            {
                isPB = true;
                Logging.Info($"New personal best for {currentTrial.TrialServerName}: {submitTime} seconds");
                PlayerPrefs.SetFloat(pbKey, (float)submitTime);
                PlayerPrefs.Save();
                currentTrial.SetPersonalBest(submitTime);
                ReplayManager.Instance.StopRecording();
                ReplayManager.Instance.SaveRecording($"{currentTrial.TrialServerName}_{submitTime}");
                if (Plugin.PBNotify.Value)
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(submitTime);
                    HUDManager.instance.SetHUDText($"New PB! {string.Concat("PB: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"))}");
                    StartCoroutine(ClearHUDDelayed(3f));
                }
            }

            string playerName = NetworkSystem.Instance.GetMyNickName().ToUpper();
            playerName = playerName.Substring(0, Math.Min(playerName.Length, 12));
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
                    string.Concat($"{Constants.ServerURL}/leaderboard/", currentTrial.TrialServerName),
                    jsonBody)
            );
            currentTrial.SetLastTime(submitTime);
            currentTrial.UsePlayerInfo(true);

            Logging.Info(GetTrialsWithPBCount(trials));
        }

        private IEnumerator PostRequest(string url, string json)
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
                            string.Concat($"{Constants.ServerURL}/leaderboard/", currentTrial.TrialServerName),
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

        private IEnumerator ClearHUDDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            HUDManager.instance.ClearHUD();
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


    }

}
