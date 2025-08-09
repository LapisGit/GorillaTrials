using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GorillaNetworking;
using GorillaTrials.Behaviours;
using GorillaTrials.Behaviours.UI;
using GorillaTrials.Models.StateMachine;
using GorillaTrials.Tools;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace GorillaTrials.Models
{
    public class Trial : MonoBehaviour
    {
        public readonly TrialStateMachine stateMachine = new();

        public readonly Stopwatch stopwatch = new();

        public GameObject trialUIObject;

        public Vector3 position;
        public float y_rotation;
        public GameObject trialObject; //DO NOT SERIALIZE/DESERIALIZE FROM SERVER, THE MOD IS SUPPOSED TO AUTOMATICALLY ASSIGN THIS.
        public string TrialLongName;
        public string TrialServerName;
        public int TrialType; // When deserializing this, make sure to convert the enum on the server (ex: challenge type set to "box") and set it to its corresponding value (ex box challenge type is 0 and zone type is 1, refer to TrialType)
        public TrialZone zoneData;
        public List<Vector3> boxPositions;
        public List<LeaderboardEntry> leaderboardEntries;
        public string formattedLeaderboardText = "";
        public ETrialDifficulty TrialDifficulty;
        public float MaxTime;
        public bool isFromCustomMap = false;
        public bool onApprovedMap = false;


        public Trial(Vector3 trialPosition, float yRotation, string trialLongName, string trialServerName, ETrialType trialType, ETrialDifficulty trialDifficulty, float maxTime, TrialZone zoneData = null, bool customMapTrial = false, List<Vector3> boxPositions = null)
        {
            trialUIObject = Instantiate(Singleton<TrialManager>.Instance.trialUIAsset);
            trialUIObject.transform.SetParent(Singleton<TrialManager>.Instance.transform);
            trialUIObject.name = trialServerName;
            trialUIObject.transform.position = trialPosition;
            trialUIObject.transform.eulerAngles = new Vector3(0, yRotation, 0);
            trialUIObject.transform.Find("UI/Info/TrialName").gameObject.GetComponent<TextMeshProUGUI>().text = trialLongName;
            trialUIObject.transform.Find("UI/Buttons/PlayTrial").gameObject.layer = (int)UnityLayer.GorillaInteractable;
            trialUIObject.transform.Find("UI/Buttons/RefreshBoard").gameObject.layer = (int)UnityLayer.GorillaInteractable;
            trialUIObject.transform.Find("UI/Buttons/WRReplay").gameObject.layer = (int)UnityLayer.GorillaInteractable;
            trialUIObject.transform.Find("UI/Buttons/PBReplay").gameObject.layer = (int)UnityLayer.GorillaInteractable;

            TrialButton trialButton = trialUIObject.transform.Find("UI/Buttons/PlayTrial").AddComponent<TrialButton>();
            TrialButton refreshButton = trialUIObject.transform.Find("UI/Buttons/RefreshBoard").AddComponent<TrialButton>();
            TrialButton WRReplay = trialUIObject.transform.Find("UI/Buttons/WRReplay").AddComponent<TrialButton>();
            TrialButton PBReplay = trialUIObject.transform.Find("UI/Buttons/PBReplay").AddComponent<TrialButton>();

            SetPersonalBest(PlayerPrefs.GetFloat(string.Concat("PB_", trialServerName), 0));

            if (trialType == ETrialType.Box)
            {
                trialUIObject.transform.Find("UI/Info/TrialType").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Box Trial";
            }
            else
            {
                trialUIObject.transform.Find("UI/Info/TrialType").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Zone Trial";
            }
            if (trialDifficulty == ETrialDifficulty.Easy)
            {
                trialUIObject.transform.Find("UI/Info/TrialDifficulty").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Difficulty: <color=#90EE90>Easy";
            }
            if (trialDifficulty == ETrialDifficulty.Medium)
            {
                trialUIObject.transform.Find("UI/Info/TrialDifficulty").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Difficulty: <color=#FDFA72>Medium";
            }
            if (trialDifficulty == ETrialDifficulty.Hard)
            {
                trialUIObject.transform.Find("UI/Info/TrialDifficulty").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Difficulty: <color=#FF6700>Hard";
            }
            if (trialDifficulty == ETrialDifficulty.Insane)
            {
                trialUIObject.transform.Find("UI/Info/TrialDifficulty").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Difficulty: <color=#EE61BD>Insane";
            }
            if (trialDifficulty == ETrialDifficulty.Extreme)
            {
                trialUIObject.transform.Find("UI/Info/TrialDifficulty").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Difficulty: <color=#FF474D>Extreme";
            } 

            trialObject = trialUIObject;

            position = trialPosition;
            y_rotation = yRotation;
            TrialLongName = trialLongName;
            TrialServerName = trialServerName;
            TrialType = (int)trialType;
            TrialDifficulty = trialDifficulty;
            MaxTime = maxTime;
            this.zoneData = zoneData;
            this.boxPositions = boxPositions;
            isFromCustomMap = customMapTrial;
            onApprovedMap = CustomMapManager.instance.approvedMap;

            trialButton.onPressed = () =>
            {
                if (Plugin.WrongVersion)
                {
                    trialUIObject.transform.Find("UI/GlobalBoard/GlobalBoardText").GetComponent<TextMeshProUGUI>().text =
                        "Please update your mod. It is out of date.";
                    return;
                }
                
                if (ReplayManager.Instance.isReplaying)
                {
                    ReplayManager.Instance.StopReplay();
                }

                if (GorillaComputer.instance.currentGameMode._value == "MODDED_Casual" || GorillaComputer.instance.currentGameMode._value == "Casual")
                {
                    //TimeManager.instance.maxTime = maxTime;
                    
                    Singleton<TrialManager>.Instance.StartTrial(this);
                }
                else
                {
                    trialUIObject.transform.Find("UI/GlobalBoard/GlobalBoardText").GetComponent<TextMeshProUGUI>().text =
                        "Please enter a casual lobby to begin a trial.";
                    Logging.Error($"Gamemode is {GorillaComputer.instance.currentGameMode._value}, and that is not a casual lobby. Not beginning trial.");
                }
                
            };
            refreshButton.onPressed = () =>
            {
                Singleton<TrialManager>.Instance.StartCoroutine(GetLeaderboardCoroutine(TrialServerName));
            };
            PBReplay.onPressed = () =>
            {
                ReplayManager.Instance.StartReplay($"{trialServerName}_{PlayerPrefs.GetFloat(string.Concat("PB_", trialServerName), 0)}");
            };
            WRReplay.onPressed = async () =>
            {
                if (leaderboardEntries == null || leaderboardEntries.Count == 0)
                {
                    Logging.Error("No leaderboard data available.");
                    return;
                }

                var topEntry = leaderboardEntries[0];
                if (string.IsNullOrEmpty(topEntry.PlayerId))
                {
                    Logging.Error("Top leaderboard entry has no playerId.");
                    return;
                }

                await ReplayManager.Instance.DownloadReplayWR(trialServerName, topEntry.PlayerId);
            };

        }

        public IEnumerator GetLeaderboardCoroutine(string trialID)
        {
            if (isFromCustomMap)
            {
                if (!onApprovedMap)
                {
                    trialUIObject.transform.Find("UI/GlobalBoard/GlobalBoardText").gameObject
                            .GetComponent<TextMeshProUGUI>().text =
                        "This trial was created by a custom map and this trial is\nnot approved by the GorillaTrials team.\n\nYou may still play the trial, but nothing will be sent\nto any servers.";
                    yield break;
                }
            }
            string url = $"{Constants.ServerURL}/leaderboard/{trialID}?limit=10";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                string apiKey = Plugin.APIKey.Value;
                www.SetRequestHeader("Authorization", apiKey);
                
                yield return www.SendWebRequest();
                
                if (www.responseCode == 401)
                {
                    Logging.Error("Not Authorized.");
                    trialUIObject.transform.Find("UI/GlobalBoard/GlobalBoardText").gameObject
                            .GetComponent<TextMeshProUGUI>().text =
                        "If you're seeing this, you're most likely not authenticated.\nPlease make sure your API key generated by the mod is in\nthe BepInEx config.\n(Located at BepInEx/config/Lapis.GorillaTrials.cfg)";
                }
                if (www.responseCode == 400)
                {
                    Logging.Error("Not Connected.");
                    trialUIObject.transform.Find("UI/GlobalBoard/GlobalBoardText").gameObject
                            .GetComponent<TextMeshProUGUI>().text =
                        "You couldn't connect to the server, try hitting Refresh,\nif that doesn't work, then try restarting your game, and if\\nthat doesn't fix this error, report this to Lapis in the discord\nserver.\n\nhttps://discord.gg/Yc8VXZSPQK";
                }

                if (www.responseCode == 404)
                {
                    Logging.Error("Trial leaderboard not found");
                    trialUIObject.transform.Find("UI/GlobalBoard/GlobalBoardText").gameObject
                            .GetComponent<TextMeshProUGUI>().text =
                        "This trials leaderboard could not be found, try hitting Refresh,\nif that doesn't work, then try restarting your game, and if\\nthat doesn't fix this error, report this to Lapis in the discord\nserver.\n\nhttps://discord.gg/Yc8VXZSPQK";
                }
                Singleton<TrialManager>.Instance.StartCoroutine(GetPlayerRank());
                if (www.result == UnityWebRequest.Result.ConnectionError || 
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Logging.Error("Error fetching leaderboard: " + www.error);
                }
                else
                {
                    string json = www.downloadHandler.text;
                    
                    try
                    {
                        leaderboardEntries = JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json);
                        formattedLeaderboardText = "";

                        foreach (var entry in leaderboardEntries)
                        {
                            if (entry.rank > 10) continue;
                            TimeSpan timeSpan = TimeSpan.FromSeconds(entry.time);
                            string formattedTime = timeSpan.TotalHours >= 1
                                ? timeSpan.ToString(@"h\:mm\:ss\.fff")
                                : timeSpan.ToString(@"mm\:ss\.fff");
                            string line = $"{entry.rank}. {entry.playerName} - {formattedTime}\n\n";
                            formattedLeaderboardText += line;
                            trialUIObject.transform.Find("UI/GlobalBoard/GlobalBoardText").gameObject.GetComponent<TextMeshProUGUI>().text = formattedLeaderboardText;
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Error("Failed to parse leaderboard JSON: " + e.Message);
                    }
                }
            }
        }
        public void SetPersonalBest(double value)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(value);
            trialUIObject.transform.Find("UI/Info/PB").GetComponent<TextMeshProUGUI>().text = string.Concat("PB: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"));
        }

        public void SetLastTime(double value)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(value);
            trialUIObject.transform.Find("UI/Info/LastTime").GetComponent<TextMeshProUGUI>().text = string.Concat("Last Time: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"));
        }
        
        private IEnumerator WaitDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
        }
        
        public IEnumerator GetPlayerRank()
        {
            string playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
            while (string.IsNullOrEmpty(playerId))
            {
                yield return new WaitForSeconds(3f);
                playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
            }

            string url = $"{Constants.ServerURL}/rank/{TrialServerName}/{playerId}";

            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SetRequestHeader("Authorization", Plugin.APIKey.Value);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Error fetching rank: {www.responseCode} - {www.error}");

                if (www.responseCode == 404)
                {
                    SetRankText("Rank: N/A");
                }
                yield break;
            }

            string json = www.downloadHandler.text;

            try
            {
                RankedLeaderboardEntry result = JsonConvert.DeserializeObject<RankedLeaderboardEntry>(json);

                if (result == null)
                {
                    Logging.Error("Parsed rank result is null.");
                    yield break;
                }
                
                SetRankText($"Rank: #{result.Rank}");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse rank JSON: " + ex.Message);
            }
        }

        private void SetRankText(string text)
        {
            Transform rankObj = trialUIObject.transform.Find("UI/Info/Rank");
            if (rankObj == null)
            {
                Logging.Error($"Rank object not found in trial {TrialServerName}");
                return;
            }

            rankObj.GetComponent<TextMeshProUGUI>().text = text;
        }


    }
}