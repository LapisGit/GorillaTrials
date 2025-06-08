using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorillaNetworking;
using GorillaTrials.Models;
using GorillaTrials.Models.StateMachine;
using GorillaTrials.Tools;
using Photon.Pun;
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

        private Trial currentTrial;
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

            // City Trials
            CreateTrial("Shopping Spree Basics", "shoppingspreebasics", new Vector3(-65.72206f, 16.42499f, -121.2781f), 180, ETrialType.Box, TrialPositions.shoppingSpreeBasicsBoxes);
            CreateTrial("Wraparound", "wraparound", new Vector3(-30.88225f, 14.99187f, -108.6642f), 269.5f, ETrialType.Box, TrialPositions.wraparoundBoxes);

            // Canyons Trials
            CreateTrial("Canyon Run", "canyonrun", new Vector3(-80.93035f, 10.34146f, -103.9011f), 180f, ETrialType.Box, TrialPositions.canyonRunBoxes);
            CreateTrial("Swing", "swing", new Vector3(-87.95385f, 9.952705f, -117.7568f), 260f, ETrialType.Box, TrialPositions.swingBoxes);

            // Caves Trials
            CreateTrial("Cave Run", "caverun", new Vector3(-62.76687f, -12.5016f, -50.1683f), 0f, ETrialType.Box, TrialPositions.caveRunBoxes);

            // Mines Trials

            // MonkeBlocks Trials

            // Clouds Trials

            // Beach Trials

            // Hoverpark Trials

            // Hoverpark2 Trials

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
                return;
            }

            Logging.Fatal($"TRIAL FOR {trialId} IS NULL!");
            Logging.Error($"Type: {trialType}");
            Logging.Error($"Parameter Count: {(parameters is null ? "null" : parameters.Length)}");
        }

        public void StartTrial(Trial trialData)
        {
            if (Started)
                return;

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

            currentTrial = null;
        }

        public void SubmitTrial(double submitTime)
        {
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

            string pbKey = $"PB_{currentTrial.TrialServerName}";

            if (PlayerPrefs.GetFloat(pbKey, 0) > submitTime)
            {
                PlayerPrefs.SetFloat(pbKey, (float)submitTime);
                PlayerPrefs.Save();

                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(pbKey, out object value) && value is double pb)
                {
                    GameObject.Find(currentTrial.TrialServerName).transform.Find("UI/Info/PB").gameObject.GetComponent<TextMeshProUGUI>().text = "PB: " + value;
                }
            }
            else
            {
                PlayerPrefs.SetFloat(pbKey, (float)submitTime);
                PlayerPrefs.Save();
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(pbKey, out object value) && value is double pb)
                {
                    GameObject.Find(currentTrial.TrialServerName).transform.Find("UI/Info/PB").gameObject.GetComponent<TextMeshProUGUI>().text = "PB: " + value;
                }
            }
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

            Logging.Info("Trial results uploaded");
        }

    }
}
