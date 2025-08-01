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
        public GameObject trialAssets, trialUIAsset, trialBoxAsset, achievementsUI;
        public string trialResultBackup;

        public async override void Initialize()
        {
            trialAssets = await AssetLoader.LoadAsset<GameObject>("GorillaTrials");
            trialUIAsset = trialAssets.transform.Find("Trial").gameObject;
            trialBoxAsset = trialAssets.transform.Find("Trial Box").gameObject;

            TrialPositions.Initialize();
            // START OF OLD CODE

            // Forest Trials
            CreateTrial("Stump Climb", "stumpclimb", new Vector3(-65.6918f, 2.5123f, -72.0744f), 180, ETrialType.Box, ETrialDifficulty.Easy, 20, false, new object[] { TrialPositions.stumpClimbBoxes });
            CreateTrial("Cross The Forest", "ctf", new Vector3(-46.75191f, 5.50911f, -26.79142f), 180, ETrialType.Box, ETrialDifficulty.Easy, 25, false, new object[] { TrialPositions.ctfBoxes });
            CreateTrial("Tallest Tree", "tallesttree", new Vector3(-26.4936f,2.137212f,-77.43867f), 300, ETrialType.Zone, ETrialDifficulty.Medium, 45, false, new object[] { TrialPositions.tallestTreeBoxes });
            CreateTrial("Tree Scale", "treescale", new Vector3(-55.89836f,0.4074497f,-74.61014f), 73.83477f, ETrialType.Zone, ETrialDifficulty.Medium, 0, false, new object[] { TrialPositions.TreeScale });
            CreateTrial("Long Jump", "longjump", new Vector3(-69.63531f,21.07899f,-62.07482f), 162.8339f, ETrialType.Box, ETrialDifficulty.Medium, 0, false, new object[] { TrialPositions.LongJump });

            // City Trials 
            CreateTrial("Shopping Spree Basics", "shoppingspreebasics", new Vector3(-65.72206f, 16.42499f, -121.2781f), 180, ETrialType.Box, ETrialDifficulty.Easy, 10, false, new object[] { TrialPositions.shoppingSpreeBasicsBoxes });
            CreateTrial("Wraparound", "wraparound", new Vector3(-30.88225f, 14.99187f, -108.6642f), 269.5f, ETrialType.Box, ETrialDifficulty.Easy, 6, false, new object[] { TrialPositions.wraparoundBoxes });
            CreateTrial("Going Up!", "goingup", new Vector3(-52.92646f,19.07714f,-101.7573f), 75f, ETrialType.Box, ETrialDifficulty.Easy, 15, false, new object[] { TrialPositions.goingUpBoxes });
            CreateTrial("Competitive Course", "compcourse", new Vector3(-44.25076f,11.05946f,-127.3902f), 110f, ETrialType.Zone, ETrialDifficulty.Easy, 15, false, new object[] { TrialPositions.compCourseBoxes });
            CreateTrial("It's TV Time!", "tvtime", new Vector3(-66.84679f,20.13389f,-133.6934f), 29.3185f, ETrialType.Box, ETrialDifficulty.Easy, 15, false, new object[] { TrialPositions.TVTime });

            // Canyons Trials
            CreateTrial("Canyon Run", "canyonrun", new Vector3(-80.93035f, 10.34146f, -103.9011f), 180f, ETrialType.Box, ETrialDifficulty.Easy, 40, false, new object[] { TrialPositions.canyonRunBoxes });
            CreateTrial("Swing", "swing", new Vector3(-87.95385f, 9.952705f, -117.7568f), 260f, ETrialType.Box, ETrialDifficulty.Easy, 15, false, new object[] { TrialPositions.swingBoxes });

            // Caves Trials
            CreateTrial("Cave Run", "caverun", new Vector3(-62.76687f, -12.5016f, -50.1683f), 0f, ETrialType.Box, ETrialDifficulty.Medium,  50, false, new object[] { TrialPositions.caveRunBoxes});
            CreateTrial("Loopback", "loopback", new Vector3(-63.18544f,-7.311106f,-35.69535f), 0f, ETrialType.Box, ETrialDifficulty.Medium, 25, false, new object[] { TrialPositions.loopBackBoxes });

            // Mines Trials

            // MonkeBlocks Trials

            // Clouds Trials
            CreateTrial("Cross The Sky Bridge", "ctsb", new Vector3(-94.06928f,220.7724f,-77.95302f), 180f, ETrialType.Box, ETrialDifficulty.Easy, 25, false, new object[] { TrialPositions.CrossTheSkyBridge });
            CreateTrial("Around You Go", "aroundyougo", new Vector3(-64.38399f,233.9328f,-89.04852f), 110f, ETrialType.Box, ETrialDifficulty.Easy, 25, false, new object[] { TrialPositions.AroundYouGo });
            CreateTrial("Swinging Around", "swingingaround", new Vector3(-97.83932f,220.5139f,-76.19705f), 245f, ETrialType.Zone, ETrialDifficulty.Insane, 25, false, new object[] { TrialPositions.SwingingAround });
            
            // Beach Trials
            CreateTrial("Ziplining", "ziplining", new Vector3(-13.08672f,28.29308f,-19.87826f), 100f, ETrialType.Box, ETrialDifficulty.Medium, 15, false, new object[] { TrialPositions.zipliningBoxes });
            
            // Hoverpark Trials

            // Hoverpark2 Trials
            
            // Metro Trials
            CreateTrial("Master Swimmer", "masterswimmer", new Vector3(-27.56499f,0.3799381f,-138.2608f), 28.56395f, ETrialType.Box, ETrialDifficulty.Hard, 15, false, new object[] { TrialPositions.MasterSwimmer });
            CreateTrial("Rooftop Jumping", "rooftopjumping", new Vector3(-2.468079f,7.529253f,-175.662f), 119.8114f, ETrialType.Box, ETrialDifficulty.Insane, 15, false, new object[] { TrialPositions.RooftopJumping });
            
            // END OF OLD CODE
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
                Logging.Info($"Created trial '{displayName}' ({trialId})");
                trials.Add(trial);
                StartCoroutine(trial.GetLeaderboardCoroutine(trialId));
                StartCoroutine(GetPlayerRank(trialId));
                Logging.Info($"Is Custom Map Trial? {trial.isFromCustomMap}");
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

            if (submitTime.HasValue && currentTrial.isFromCustomMap == false)
            {
                Logging.Info($"Submiting time {submitTime.Value}");
                SubmitTrial(submitTime.Value);
            }

            if (currentTrial.isFromCustomMap)
            {
                Logging.Info("Trial was created by a Custom Map, not submitting a time.");
            }
            StartCoroutine(currentTrial.GetLeaderboardCoroutine(currentTrial.TrialServerName));
            
            if (submitTime.HasValue)
            {
                AchievementChecker.instance.UpdateAchievements(submitTime.Value, currentTrial);   
            }

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
                TimeSpan timeSpan = TimeSpan.FromSeconds(submitTime);
                HUDManager.instance.SetHUDText($"New PB! {string.Concat("PB: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"))}");
                StartCoroutine(WaitDelay(3f));
                HUDManager.instance.ClearHUD();
            }
            string playerName = NetworkSystem.Instance.GetMyNickName();
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
                    string.Concat("https://trials.lapis.codes/leaderboard/", currentTrial.TrialServerName),
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
                
                if (request.responseCode == 0)
                {
                    StartCoroutine(WaitDelay(5f));
                    StartCoroutine(PostRequest
                        (
                            string.Concat("https://trials.lapis.codes/leaderboard/", currentTrial.TrialServerName),
                            trialResultBackup)
                    );
                }
                
                Logging.Error(request.downloadHandler.text);
                yield break;
            }
            refreshBoard = "";
            Logging.Info("Trial results uploaded");
        }

        private IEnumerator GetPlayerRank(string trial = null)
        {
            string url = "https://trials.lapis.codes/rank/" + trial + "/" +
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
                if (www.responseCode == 404)
                {
                    Logging.Error("nuh uh no rank!");
                    trialUIAsset.transform.Find("UI/Info/Rank").GetComponent<TextMeshProUGUI>().text = "Rank: N/A";
                    yield break;
                }
            }
            else
            {
                string json = www.downloadHandler.text;
                LeaderboardEntry result = JsonUtility.FromJson<LeaderboardEntry>(json);
                Logging.Info($"Player rank is: {result.rank}");
                trialUIAsset.transform.Find("UI/Info/Rank").GetComponent<TextMeshProUGUI>().text = "Rank: #" + result.rank;
            }
            
        }
        private IEnumerator WaitDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
        }
    }
    
}
