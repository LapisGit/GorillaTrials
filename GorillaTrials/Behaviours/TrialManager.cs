using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorillaNetworking;
using GorillaTrials.Models;
using GorillaTrials.Models.StateMachine;
using GorillaTrials.Tools;
using TMPro;
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
        public GameObject trialAssets, trialUIAsset, trialBoxAsset;

        public async override void Initialize()
        {
            
            trialAssets = await AssetLoader.LoadAsset<GameObject>("GorillaTrials");
            trialUIAsset = trialAssets.transform.Find("Trial").gameObject;
            trialBoxAsset = trialAssets.transform.Find("Trial Box").gameObject;

            TrialPositions.Initialize();
            // START OF OLD CODE

            // Forest Trials
            CreateTrial("Stump Climb", "stumpclimb", new Vector3(-65.6918f, 2.5123f, -72.0744f), 180, ETrialType.Box, TrialPositions.stumpClimbBoxes);
            CreateTrial("Cross The Forest", "ctf", new Vector3(-46.75191f, 5.50911f, -26.79142f), 180, ETrialType.Box, TrialPositions.ctfBoxes);
            CreateTrial("Tallest Tree", "tallesttree", new Vector3(-26.4936f,2.137212f,-77.43867f), 300, ETrialType.Box, TrialPositions.tallestTreeBoxes);
            CreateTrial("Zone Test", "zonetest", new Vector3(-68.12813f,11.5433f,-82.66145f), 0, ETrialType.Zone, TrialPositions.ZoneTest);

            // City Trials
            CreateTrial("Shopping Spree Basics", "shoppingspreebasics", new Vector3(-65.72206f, 16.42499f, -121.2781f), 180, ETrialType.Box, TrialPositions.shoppingSpreeBasicsBoxes);
            CreateTrial("Wraparound", "wraparound", new Vector3(-30.88225f, 14.99187f, -108.6642f), 269.5f, ETrialType.Box, TrialPositions.wraparoundBoxes);
            CreateTrial("Going Up!", "goingup", new Vector3(-52.92646f,19.07714f,-101.7573f), 75f, ETrialType.Box, TrialPositions.goingUpBoxes);
            CreateTrial("Competitive Course", "compcourse", new Vector3(-44.25076f,11.05946f,-127.3902f), 110f, ETrialType.Box, TrialPositions.compCourseBoxes);

            // Canyons Trials
            CreateTrial("Canyon Run", "canyonrun", new Vector3(-80.93035f, 10.34146f, -103.9011f), 180f, ETrialType.Box, TrialPositions.canyonRunBoxes);
            CreateTrial("Swing", "swing", new Vector3(-87.95385f, 9.952705f, -117.7568f), 260f, ETrialType.Box, TrialPositions.swingBoxes);

            // Caves Trials
            CreateTrial("Cave Run", "caverun", new Vector3(-62.76687f, -12.5016f, -50.1683f), 0f, ETrialType.Box, TrialPositions.caveRunBoxes);
            CreateTrial("Loopback", "loopback", new Vector3(-63.18544f,-7.311106f,-35.69535f), 0f, ETrialType.Box, TrialPositions.loopBackBoxes);

            // Mines Trials

            // MonkeBlocks Trials

            // Clouds Trials

            // Beach Trials
            CreateTrial("Ziplining", "ziplining", new Vector3(-13.08672f,28.29308f,-19.87826f), 100f, ETrialType.Box, TrialPositions.zipliningBoxes);
            
            // Hoverpark Trials

            // Hoverpark2 Trials
            
            // Ghost Reactor Trials
            CreateTrial("RUN!!", "run", new Vector3(-22.25639f,-29.7322f,-80.10743f), 90f, ETrialType.Box, TrialPositions.runBoxes);

            // END OF OLD CODE
        }

        public void CreateTrial(string displayName, string trialId, Vector3 position, float angle, ETrialType trialType = ETrialType.Box, params object[] parameters)
        {
            Trial trial = null;

            if (trialType == ETrialType.Box && parameters is not null && parameters.ElementAtOrDefault(0) is List<Vector3> points)
                trial = new(position, angle, displayName, trialId, trialType, null, points);
            else if (trialType == ETrialType.Zone && parameters is not null && parameters.ElementAtOrDefault(0) is TrialZone trialZone)
                trial = new(position, angle, displayName, trialId, trialType, trialZone, null);

            if (trial is not null)
            {
                Logging.Info($"Created trial '{displayName}' ({trialId})");
                trials.Add(trial);
                StartCoroutine(trial.GetLeaderboardCoroutine(trialId));
                //StartCoroutine(GetPlayerRank(trialId));
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
        }

        public void EndTrial(double? submitTime)
        {
            if (!Started)
                return;

            if (submitTime.HasValue)
            {
                Logging.Info($"Submiting time {submitTime.Value}");
                SubmitTrial(submitTime.Value);
            }
            StartCoroutine(currentTrial.GetLeaderboardCoroutine(currentTrial.TrialServerName));
            currentTrial = null;
        } 

        public void SubmitTrial(double submitTime)
        {
            string pbKey = string.Concat("PB_", currentTrial.TrialServerName);
            refreshBoard = currentTrial.TrialServerName;
            Logging.Info(PlayerPrefs.GetFloat(pbKey,0));
            if (submitTime < PlayerPrefs.GetFloat(pbKey, 0) || PlayerPrefs.GetFloat(pbKey, 0) == 0)
            {
                Logging.Info($"New personal best for {currentTrial.TrialServerName}: {submitTime} seconds");

                PlayerPrefs.SetFloat(pbKey, (float)submitTime);
                PlayerPrefs.Save();
                currentTrial.SetPersonalBest(submitTime);
            }
            string playerName = NetworkSystem.Instance.GetMyNickName();
            string playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();

            string jsonBody = JsonUtility.ToJson(new TrialResult
            {
                PlayerName = playerName,
                Time = submitTime,
                PlayerId = playerId
            });

            StartCoroutine(PostRequest
                (
                    string.Concat("https://trials.freebranchcoins.xyz/leaderboard/", currentTrial.TrialServerName),
                    jsonBody)
            );
            currentTrial.SetLastTime(submitTime);
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
                Logging.Error(request.downloadHandler.text);
                yield break;
            }
            refreshBoard = "";
            Logging.Info("Trial results uploaded");
        }

        private IEnumerator GetPlayerRank(string trial = null)
        {
            string url = "https://trials.freebranchcoins.xyz/rank/" + trial + "/" +
                         PlayFabAuthenticator.instance.GetPlayFabPlayerId();
            Logging.Info(url);
            string apiKey = Plugin.APIKey.Value;
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SetRequestHeader("Authorization", apiKey);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Logging.Error($"Error: {www.responseCode} - {www.error}");
                if (www.responseCode == 401)
                    Logging.Error("Unauthorized. Check your API key.");
            }
            else
            {
                string json = www.downloadHandler.text;
                LeaderboardEntry result = JsonUtility.FromJson<LeaderboardEntry>(json);
                Logging.Info($"Player rank is: {result.rank}");
                trialUIAsset.transform.Find("UI/Info/Rank").GetComponent<TextMeshProUGUI>().text = "#" + result.rank;
            }
            
        }
    }
}
