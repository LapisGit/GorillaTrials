using GorillaNetworking;
using GorillaTrials.Behaviours;
using GorillaTrials.Behaviours.UI;
using GorillaTrials.Models.StateMachine;
using GorillaTrials.Tools;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Utilla.Utils;
using Utilla.Models;
using GorillaGameModes;

namespace GorillaTrials.Models
{
    public class Trial : MonoBehaviour
    {
        public readonly TrialStateMachine stateMachine = new();

        public readonly Stopwatch stopwatch = new();

        public GameObject trialUIObject;

        public Vector3 position;
        public float y_rotation;

        [NonSerialized]
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

            string colourTag = trialDifficulty switch
            {
                ETrialDifficulty.Easy => "<color=#90EE90>",
                ETrialDifficulty.Medium => "<color=#FDFA72>",
                ETrialDifficulty.Hard => "<color=#FF6700>",
                ETrialDifficulty.Insane => "<color=#EE61BD>",
                ETrialDifficulty.Extreme => "<color=#FF474D>",
                _ => "<color=red>"
            };

            trialUIObject.transform.Find("Stand/FrontCanvas/Text (TMP)").GetComponent<TMP_Text>().text = $"{colourTag}{trialLongName}";

            trialUIObject.transform.Find("UI/InfoMenu/TrialName").GetComponent<TMP_Text>().text = trialLongName;

            UseTrialMenu(TrialMenu.InfoMenu);

            string key = string.Concat("PB_", trialServerName);
            UsePlayerInfo(PlayerPrefs.HasKey(key));
            SetPersonalBest(PlayerPrefs.GetFloat(key, 0));

            trialUIObject.transform.Find("UI/InfoMenu/TrialType").gameObject.GetComponent<TextMeshProUGUI>().text = trialType switch
            {
                ETrialType.Box => "Box Trial",
                ETrialType.Zone => "Zone Trial",
                _ => "secret third type"
            };

            trialUIObject.transform.Find("UI/InfoMenu/TrialDifficulty").gameObject.GetComponent<TMP_Text>().text = $"Difficulty: {colourTag}{trialDifficulty.GetName()}";

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

            TrialButton infoButton = trialUIObject.transform.Find("Stand/TopCanvas/InfoButton").gameObject.AddComponent<TrialButton>();

            infoButton.onPressed = delegate ()
            {
                UseTrialMenu(TrialMenu.InfoMenu);
            };

            TrialButton detailsButton = trialUIObject.transform.Find("Stand/TopCanvas/DetailsButton").gameObject.AddComponent<TrialButton>();

            detailsButton.onPressed = delegate ()
            {
                UseTrialMenu(TrialMenu.DetailsMenu);
            };

            TrialButton trialButton = trialUIObject.transform.Find("Stand/TopCanvas/PlayTrial").gameObject.AddComponent<TrialButton>();

            trialButton.onPressed = () =>
            {
                if (Plugin.WrongVersion)
                {
                    UseTrialMenu(TrialMenu.DetailsMenu);
                    UseDetailsText(TrialDetailsText.CustomErrorText).text = "Please update your mod. It is out of date.";
                    return;
                }

                if (ReplayManager.Instance.isReplaying)
                {
                    ReplayManager.Instance.StopReplay();
                }

                if (GameModeUtils.CurrentGamemode is Gamemode gamemode && gamemode.ID == GameModeType.Casual.GetName() && gamemode.BaseGamemode.GetValueOrDefault(GameModeType.Infection) == GameModeType.Casual || Plugin.InModdedGamemode)
                {
                    //TimeManager.instance.maxTime = maxTime;

                    Singleton<TrialManager>.Instance.StartTrial(this);
                }
                else
                {
                    UseTrialMenu(TrialMenu.DetailsMenu);
                    UseDetailsText(TrialDetailsText.CustomErrorText).text = "Please enter a casual lobby or the GorillaTrial custom gamemode to begin a trial.";

                    Logging.Error($"Gamemode is {GorillaComputer.instance.currentGameMode._value}, and that is not a casual lobby. Not beginning trial.");
                }

            };

            TrialButton refreshButton = trialUIObject.transform.Find("UI/DetailsMenu/RefreshBoard").gameObject.AddComponent<TrialButton>();

            refreshButton.onPressed = () =>
            {
                Singleton<TrialManager>.Instance.StartCoroutine(GetLeaderboardCoroutine(TrialServerName));
            };

            TrialButton PBReplay = trialUIObject.transform.Find("UI/InfoMenu/PBReplay").gameObject.AddComponent<TrialButton>();

            PBReplay.onPressed = () =>
            {
                ReplayManager.Instance.StartReplay($"{trialServerName}_{PlayerPrefs.GetFloat(string.Concat("PB_", trialServerName), 0)}");
            };

            TrialButton WRReplay = trialUIObject.transform.Find("UI/InfoMenu/WRReplay").gameObject.AddComponent<TrialButton>();

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

        public void SetPersonalBest(double value)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(value);
            trialUIObject.transform.Find("UI/InfoMenu/TrialPlayerData/PB").GetComponent<TMP_Text>().text = string.Concat("PB: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"));
        }

        public void SetLastTime(double value)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(value);
            trialUIObject.transform.Find("UI/InfoMenu/TrialPlayerData/LastTime").GetComponent<TMP_Text>().text = string.Concat("Last Time: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"));
        }

        private void SetRankText(string text)
        {
            Transform rankObj = trialUIObject.transform.Find("UI/InfoMenu/TrialPlayerData/Rank");
            rankObj.GetComponent<TextMeshProUGUI>().text = text;
        }

        public IEnumerator GetLeaderboardCoroutine(string trialID)
        {
            if (isFromCustomMap && !onApprovedMap)
            {
                UseTrialMenu(TrialMenu.DetailsMenu);
                UseDetailsText(TrialDetailsText.CustomErrorText).text = "This trial was created by a custom map and this trial is\nnot approved by the GorillaTrials team.\n\nYou may still play the trial, but nothing will be sent\nto any servers.";
                yield break;
            }

            string url = $"{Constants.ServerURL}/leaderboard/{trialID}?limit=10";

            using UnityWebRequest www = UnityWebRequest.Get(url);
            string apiKey = Plugin.APIKey.Value;
            www.SetRequestHeader("Authorization", apiKey);

            yield return www.SendWebRequest();

            if (www.responseCode == 401)
            {
                UseTrialMenu(TrialMenu.DetailsMenu);
                UseDetailsText(TrialDetailsText.AuthErrorText);

                Logging.Error("Not Authorized.");
            }

            if (www.responseCode == 400)
            {
                UseTrialMenu(TrialMenu.DetailsMenu);
                UseDetailsText(TrialDetailsText.BoardErrorText);

                Logging.Error("Not Connected.");
            }

            if (www.responseCode == 404)
            {
                UseTrialMenu(TrialMenu.DetailsMenu);
                UseDetailsText(TrialDetailsText.BoardErrorText);

                Logging.Error("Trial leaderboard not found");
            }

            Singleton<TrialManager>.Instance.StartCoroutine(GetPlayerRank());

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Logging.Error("Error fetching leaderboard: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;

            try
            {
                leaderboardEntries = JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json);
                formattedLeaderboardText = "";

                foreach (var entry in leaderboardEntries)
                {
                    if (entry.rank > 10) continue;
                    TimeSpan timeSpan = TimeSpan.FromSeconds(entry.time);
                    string formattedTime = timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff");
                    string line = $"{entry.rank}. {entry.playerName} - {formattedTime}\n";
                    formattedLeaderboardText += line;
                }

                UseDetailsText(TrialDetailsText.GlobalBoardText).text = formattedLeaderboardText;
            }
            catch (Exception e)
            {
                Logging.Error("Failed to parse leaderboard JSON: " + e.Message);
            }
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
                    SetRankText("");
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
                UsePlayerInfo(true);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse rank JSON: " + ex.Message);
            }
        }

        public void UsePlayerInfo(bool choice)
        {
            Transform[] roots =
            [
                trialUIObject.transform.Find("UI/InfoMenu/TrialPlayerData"),
                trialUIObject.transform.Find("UI/InfoMenu/PBReplay")
            ];

            foreach(Transform transform in roots)
            {
                if (transform.gameObject.activeSelf == choice) continue;
                transform.gameObject.SetActive(choice);
            }
        }

        public void UseTrialMenu(TrialMenu choice)
        {
            Transform root = trialUIObject.transform.Find("UI");

            foreach(Transform child in root)
            {
                if (!Enum.TryParse(child.gameObject.name, out TrialMenu foundType)) continue;

                bool isActive = foundType == choice;
                if (child.gameObject.activeSelf != isActive) child.gameObject.SetActive(isActive);
            }
        }

        public TMP_Text UseDetailsText(TrialDetailsText choice)
        {
            Transform root = trialUIObject.transform.Find("UI/DetailsMenu");

            TMP_Text chosenText = null;

            Dictionary<TrialDetailsText, GameObject> dict = [];

            foreach(Transform child in root)
            {
                if (!child.GetComponent<TMP_Text>() || !Enum.TryParse(child.gameObject.name, out TrialDetailsText type) || dict.ContainsKey(type)) continue;

                dict.TryAdd(type, child.gameObject);
            }

            foreach(var (type, gameObject) in dict)
            {
                bool isActive = type == choice;

                TMP_Text text = gameObject.GetComponent<TMP_Text>();
                text.enabled = isActive;

                if (isActive && chosenText is null) chosenText = text;
            }

            return chosenText;
        }

        public enum TrialMenu
        {
            InfoMenu,
            DetailsMenu
        }

        public enum TrialDetailsText
        {
            AuthErrorText,
            BoardErrorText,
            CustomErrorText,
            GlobalBoardText
        }
    }
}