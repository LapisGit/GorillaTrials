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
using System.Linq;
using BepInEx;
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

        public GTZone currentZoneID = GTZone.none;

        [NonSerialized]
        public GameObject trialObject; //DO NOT SERIALIZE/DESERIALIZE FROM SERVER, THE MOD IS SUPPOSED TO AUTOMATICALLY ASSIGN THIS.

        public string TrialLongName;
        public string TrialServerName;
        public float BronzeTime;
        public float SilverTime;
        public float GoldTime;
        public int TrialType; // When deserializing this, make sure to convert the enum on the server (ex: challenge type set to "box") and set it to its corresponding value (ex box challenge type is 0 and zone type is 1, refer to TrialType)
        public TrialZone zoneData;
        public List<Vector3> boxPositions;
        public List<LeaderboardEntry> leaderboardEntries;
        public string formattedLeaderboardText = "";
        public ETrialDifficulty TrialDifficulty;
        public float MaxTime;
        public bool isFromCustomMap = false;
        public bool onApprovedMap = false;
        public string downloadedFileName = null;
        public bool isPlaytest = false;
        public bool racing = false;
        public RaceType raceType = RaceType.None;
        public bool wrReplayAvailable = false;
        
        public bool hasAcceptedChallenge = false;
        public string acceptedChallengeId = null;
        public float challengeTimeToBeat = 0f;
        public string challengerUsername = "";
        
        private int challengeMenuCurrentPage = 1;
        private int challengeMenuTotalPages = 1;
        public string selectedChallengeRecipientId = null;
        public string selectedChallengeRecipientUsername = null;

        public Trial(Vector3 trialPosition, float yRotation, string trialLongName, string trialServerName, ETrialType trialType, ETrialDifficulty trialDifficulty, float maxTime, TrialZone zoneData = null, bool customMapTrial = false, List<Vector3> boxPositions = null, float bronzeTime = 0f, float silverTime = 0f, float goldTime = 0f, string downloadedFileName = null, bool isPlaytest = false)
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
            
            Transform badgesTransform = trialUIObject.transform.Find("UI/InfoMenu/TrialBadges");
            if (bronzeTime == 0f || silverTime == 0f || goldTime == 0f)
            {
                badgesTransform.gameObject.SetActive(false);
            }
            else
            {
                TimeSpan bronzeSpan = TimeSpan.FromSeconds(bronzeTime);
                TimeSpan silverSpan = TimeSpan.FromSeconds(silverTime);
                TimeSpan goldSpan = TimeSpan.FromSeconds(goldTime);
                
                badgesTransform.gameObject.SetActive(true);
                
                Transform bronzeTimeTransform = badgesTransform.Find("Bronze/Time");
                bronzeTimeTransform.GetComponent<TextMeshProUGUI>().text = $"{(bronzeSpan.TotalHours >= 1 ? bronzeSpan.ToString(@"h\:mm\:ss\.fff") : bronzeSpan.ToString(@"mm\:ss\.fff"))}"; 
                
                Transform silverTimeTransform = badgesTransform.Find("Silver/Time");
                silverTimeTransform.GetComponent<TextMeshProUGUI>().text = $"{(silverSpan.TotalHours >= 1 ? silverSpan.ToString(@"h\:mm\:ss\.fff") : silverSpan.ToString(@"mm\:ss\.fff"))}";
                
                Transform goldTimeTransform = badgesTransform.Find("Gold/Time"); 
                goldTimeTransform.GetComponent<TextMeshProUGUI>().text = $"{(goldSpan.TotalHours >= 1 ? goldSpan.ToString(@"h\:mm\:ss\.fff") : goldSpan.ToString(@"mm\:ss\.fff"))}";
            }
            trialUIObject.transform.Find("Stand/FrontCanvas/Text (TMP)").GetComponent<TMP_Text>().text = $"{colourTag}{trialLongName}";

            trialUIObject.transform.Find("UI/InfoMenu/TrialName").GetComponent<TMP_Text>().text = trialLongName;

            UseTrialMenu(TrialMenu.InfoMenu);
            
            TrialServerName = trialServerName;
            TrialLongName = trialLongName;
            
            BronzeTime = bronzeTime;
            SilverTime = silverTime;
            GoldTime = goldTime;
            
            string key = string.Concat("PB_", trialServerName);
            UsePlayerInfo(PlayerPrefs.HasKey(key));
            SetPersonalBest(PlayerPrefs.GetFloat(key, 0));
            
            float existingPB = PlayerPrefs.GetFloat(key, 0);
            
            if (existingPB > 0 && bronzeTime > 0 && silverTime > 0 && goldTime > 0)
            {
                BadgeType earnedBadge = CheckBadgeEarned(existingPB);
                SaveBadgeIfBetter(earnedBadge);
            }
            UpdateBadgeUI();

            trialUIObject.transform.Find("UI/InfoMenu/TrialType").gameObject.GetComponent<TextMeshProUGUI>().text = trialType switch
            {
                ETrialType.Box => "Box Trial",
                ETrialType.Zone => "Zone Trial",
                _ => "secret third type"
            };

            trialUIObject.transform.Find("UI/InfoMenu/TrialDifficulty").gameObject.GetComponent<TMP_Text>().text = $"Difficulty: {colourTag}{trialDifficulty.GetName()}";

            trialObject = trialUIObject;
            
            currentZoneID = GetZoneIdForGameObject(trialUIObject);
            Logging.Info("zone id: " + currentZoneID);
            
            OnZoneChange(ZoneManagement.instance.zones);
            ZoneManagement.OnZoneChange += OnZoneChange;

            position = trialPosition;
            y_rotation = yRotation;
            TrialType = (int)trialType;
            TrialDifficulty = trialDifficulty;
            MaxTime = maxTime;
            this.zoneData = zoneData;
            this.boxPositions = boxPositions;
            isFromCustomMap = customMapTrial;
            onApprovedMap = CustomMapManager.instance.approvedMap;
            this.downloadedFileName = downloadedFileName;
            this.isPlaytest = isPlaytest;
            
            TrialButton deleteTrial = trialUIObject.transform.Find("UI/InfoMenu/DeleteTrial").gameObject.AddComponent<TrialButton>();
            
            deleteTrial.onPressed = () =>
            {
                DeleteCustomTrial();
            };
            
            if (isFromCustomMap)
            {
                deleteTrial.gameObject.SetActive(true);
            }
            else
            {
                deleteTrial.gameObject.SetActive(false);
            }
            
            TrialButton raceButton = trialUIObject.transform.Find("Stand/TopCanvas/RaceButton").gameObject.AddComponent<TrialButton>();

            raceButton.onPressed = delegate()
            {
                UseTrialMenu(TrialMenu.RaceMenu);
            };
            
            TrialButton raceWRButton = trialUIObject.transform.Find("UI/RaceMenu/Allowed/WR/WRRace").gameObject.AddComponent<TrialButton>();

            raceWRButton.onPressed = async() =>
            {
                if (leaderboardEntries.Count == 0) return;

                var topEntry = leaderboardEntries[0];
                SetRaceMenuUI(true, false, false);

                try
                {
                    if (ReplayManager.Instance.isReplaying)
                    {
                        ReplayManager.Instance.StopReplay();
                    }
                    
                    var frames = await ReplayManager.Instance.DownloadReplayWR(trialServerName, topEntry.PlayerId, true);

                    if (frames != null && frames.Count > 0)
                    {
                        raceType = RaceType.WR;
                        racing = true;
                        wrReplayAvailable = true;
            
                        SetRaceMenuUI(false, true, false);
                        UpdateRaceMenuAvailability();
                        UpdateInfoMenuReplayButtons();
                    }
                    else
                    {
                        SetRaceMenuUIWithError("Replay Not Found");
                        raceType = RaceType.None;
                        racing = false;
                        wrReplayAvailable = false;
                        UpdateRaceMenuAvailability();
                        UpdateInfoMenuReplayButtons();
                        Logging.Error("WR replay download failed: No frames received or replay doesn't exist");
                    }
                }
                catch (Exception ex)
                {
                    SetRaceMenuUIWithError("Replay Not Found");
                    raceType = RaceType.None;
                    racing = false;
                    wrReplayAvailable = false;
                    UpdateRaceMenuAvailability();
                    UpdateInfoMenuReplayButtons();
                    Logging.Error($"WR replay download exception: {ex.Message}");
                }
            };

            
            TrialButton racePBButton = trialUIObject.transform.Find("UI/RaceMenu/Allowed/PB/PBRace").gameObject.AddComponent<TrialButton>();

            racePBButton.onPressed = () =>
            {
                if (!PlayerPrefs.HasKey(string.Concat("PB_", trialServerName))) return;
                SetRaceMenuUI(false,false,true);
                raceType = RaceType.PB;
                racing = true;
            };
            
            TrialButton noMoreRace = trialUIObject.transform.Find("UI/RaceMenu/Allowed/ClearRace").gameObject.AddComponent<TrialButton>();

            noMoreRace.onPressed = () =>
            {
                SetRaceMenuUI(false,false,false);
                raceType = RaceType.None;
                racing = false;
                
                ReplayManager.Instance.ClearCachedRaceFrames();
            };

            UpdateRaceMenuAvailability();
            
            Transform challengeButtonTransform = trialUIObject.transform.Find("Stand/TopCanvas/ChallengeButton");
            if (challengeButtonTransform != null)
            {
                TrialButton challengeButton = challengeButtonTransform.gameObject.AddComponent<TrialButton>();

                challengeButton.onPressed = delegate ()
                {
                    UseTrialMenu(TrialMenu.ChallengeMenu);
                    LoadChallengeMenuFriends();
                };
                
                SetupChallengeMenu();
            }
            else
            {
                Logging.Warning($"Challenge button not found for trial {trialServerName}, challenge menu will not be available");
            }

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

                if (GameModeUtils.CurrentGamemode is Gamemode gamemode && gamemode.ID == GameModeType.Casual.GetName() && gamemode.BaseGamemode.GetValueOrDefault(GameModeType.Infection) == GameModeType.Casual || Plugin.InModdedGamemode || GorillaComputer.instance.currentGameMode.ToString() == "MODDED_Casual")
                {
                    //TimeManager.instance.maxTime = maxTime;
                    
                    Singleton<TrialManager>.Instance.StartTrial(this);

                    if (racing)
                    {
                        if (raceType == RaceType.PB)
                        {
                            float pbTime = PlayerPrefs.GetFloat(string.Concat("PB_", trialServerName), 0);
                            ReplayManager.Instance.StartReplay($"{trialServerName}_{PlayerPrefs.GetFloat(string.Concat("PB_", trialServerName), 0)}");
                        }
                        else if (raceType == RaceType.WR)
                        {
                            ReplayManager.Instance.StartWRRaceReplay();
                        }
                    }
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

                var frames = await ReplayManager.Instance.DownloadReplayWR(trialServerName, topEntry.PlayerId, false);

                if (frames != null && frames.Count > 0)
                {
                    wrReplayAvailable = true;
                    UpdateRaceMenuAvailability();
                    UpdateInfoMenuReplayButtons();
                }
            };
            UpdateInfoMenuReplayButtons();
            LoadAcceptedChallenge();
        }

        public void SetPersonalBest(double value)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(value);
            trialUIObject.transform.Find("UI/InfoMenu/TrialPlayerData/PB").GetComponent<TMP_Text>().text = string.Concat("PB: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"));
            trialUIObject.transform.Find("UI/RaceMenu/Allowed/PB/PBData/PB").GetComponent<TMP_Text>().text = string.Concat("Personal Best: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"));
        }

        public void SetLastTime(double value)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(value);
            trialUIObject.transform.Find("UI/InfoMenu/TrialPlayerData/LastTime").GetComponent<TMP_Text>().text = string.Concat("Last Time: ", timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss\.fff") : timeSpan.ToString(@"mm\:ss\.fff"));
        }

        private void SetRankText(string text)
        {
            Transform rankObj = trialUIObject.transform.Find("UI/InfoMenu/TrialPlayerData/Rank");
            Transform rankObj2 = trialUIObject.transform.Find("UI/RaceMenu/Allowed/PB/PBData/Rank");
            rankObj.GetComponent<TextMeshProUGUI>().text = text;
            rankObj2.GetComponent<TextMeshProUGUI>().text = text;
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
                
                if (leaderboardEntries != null && leaderboardEntries.Count > 0)
                {
                    var topEntry = leaderboardEntries[0];
                    TimeSpan wrTimeSpan = TimeSpan.FromSeconds(topEntry.time);
                    trialUIObject.transform.Find("UI/RaceMenu/Allowed/WR/WRData/PB").GetComponent<TextMeshProUGUI>()
                            .text =
                        $"World Record: {(wrTimeSpan.TotalHours >= 1 ? wrTimeSpan.ToString(@"h\:mm\:ss\.fff") : wrTimeSpan.ToString(@"mm\:ss\.fff"))}";
                    Singleton<TrialManager>.Instance.StartCoroutine(CheckWRReplayAvailability(topEntry.PlayerId));
                }
                else
                {
                    UpdateRaceMenuAvailability();
                    UpdateInfoMenuReplayButtons();
                }
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
                UpdateRaceMenuAvailability(); // Update race menu to show rank text
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
        
        public static GTZone GetZoneIdForGameObject(GameObject obj)
        {
            if (ZoneGraphBSP.Instance == null)
            {
                Logging.Error("ZoneGraphBSP is null");
                return GTZone.none;
            }

            ZoneDef zoneDef = ZoneGraphBSP.Instance.FindZoneAtPoint(obj.transform.position);
            return zoneDef == null ? GTZone.none : zoneDef.zoneId;
        }
        
        // thanks dane
        public void OnZoneChange(ZoneData[] zoneData)
        {
            IEnumerable<GTZone> activeZones = zoneData.Where(zone => zone.active).Select(zone => zone.zone);
            OnZoneChange(activeZones.ToArray());
        }
        public void OnZoneChange(GTZone[] activeZones)
        {
            bool isInActiveZone = false;
    
            foreach (GTZone zone in activeZones)
            {
                if (zone == currentZoneID)
                {
                    isInActiveZone = true;
                    break;
                }
            }
    
            if (trialUIObject != null)
            {
                trialUIObject.transform.Find("Stand").gameObject.SetActive(isInActiveZone);
                trialUIObject.transform.Find("UI").gameObject.SetActive(isInActiveZone);
            }
        }

        private void SetRaceMenuUI(bool downloadingActive, bool wrChecked, bool pbChecked)
        {
            Transform downloadingIndicator = trialUIObject.transform.Find("UI/RaceMenu/Allowed/DownloadingIndicator");
            if (downloadingIndicator != null)
            {
                downloadingIndicator.gameObject.SetActive(downloadingActive);
            }

            Transform wrNone = trialUIObject.transform.Find("UI/RaceMenu/Allowed/WR/ImageWRNone");
            if (wrNone != null) wrNone.gameObject.SetActive(!wrChecked);

            Transform wrCheck = trialUIObject.transform.Find("UI/RaceMenu/Allowed/WR/ImageWRCheck");
            if (wrCheck != null) wrCheck.gameObject.SetActive(wrChecked);

            Transform pbNone = trialUIObject.transform.Find("UI/RaceMenu/Allowed/PB/ImagePBNone");
            if (pbNone != null) pbNone.gameObject.SetActive(!pbChecked);

            Transform pbCheck = trialUIObject.transform.Find("UI/RaceMenu/Allowed/PB/ImagePBCheck");
            if (pbCheck != null) pbCheck.gameObject.SetActive(pbChecked);
        }

        private void SetRaceMenuUIWithError(string errorMessage)
        {
            Transform raceMenu = trialUIObject.transform.Find("UI/RaceMenu/Allowed");
            Transform downloadingIndicator = raceMenu.Find("DownloadingIndicator");
            
            if (downloadingIndicator != null)
            {
                TextMeshProUGUI downloadText = downloadingIndicator.GetComponent<TextMeshProUGUI>();
                if (downloadText != null)
                {
                    downloadText.text = errorMessage;
                }
                downloadingIndicator.gameObject.SetActive(true);
            }

            raceType = RaceType.None;
            SetRaceMenuUI(false,false,false);
        }

        private void UpdateRaceMenuAvailability()
        {
            bool hasWR = wrReplayAvailable;
            bool hasPB = PlayerPrefs.HasKey(string.Concat("PB_", TrialServerName));

            Transform raceMenuRoot = trialUIObject.transform.Find("UI/RaceMenu");
            if (raceMenuRoot == null) return;

            Transform allowedMenu = raceMenuRoot.Find("Allowed");
            Transform notAllowedMenu = raceMenuRoot.Find("NotAllowed");

            if (allowedMenu == null || notAllowedMenu == null) return;

            if (!hasWR && !hasPB)
            {
                allowedMenu.gameObject.SetActive(false);
                notAllowedMenu.gameObject.SetActive(true);
                return;
            }

            allowedMenu.gameObject.SetActive(true);
            notAllowedMenu.gameObject.SetActive(false);

            Transform noWR = allowedMenu.Find("NoWR");
            Transform wrData = allowedMenu.Find("WR");
            if (noWR != null) noWR.gameObject.SetActive(!hasWR);
            if (wrData != null) wrData.gameObject.SetActive(hasWR);

            Transform noPB = allowedMenu.Find("NoPB");
            Transform pbData = allowedMenu.Find("PB");
            if (noPB != null) noPB.gameObject.SetActive(!hasPB);
            if (pbData != null) pbData.gameObject.SetActive(hasPB);
            
            else if (wrData != null && hasWR)
            {
                Transform wrTimeTransform = wrData.Find("WRData/Time");
                if (wrTimeTransform != null)
                {
                    wrTimeTransform.GetComponent<TextMeshProUGUI>().text = "World Record: Loading...";
                }
            }
        }

        private void UpdateInfoMenuReplayButtons()
        {
            bool hasWR = wrReplayAvailable;
            bool hasPB = PlayerPrefs.HasKey(string.Concat("PB_", TrialServerName));

            Transform infoMenu = trialUIObject.transform.Find("UI/InfoMenu");
            if (infoMenu == null) return;

            Transform wrReplayButton = infoMenu.Find("WRReplay");
            if (wrReplayButton != null)
            {
                wrReplayButton.gameObject.SetActive(hasWR);
            }

            Transform pbReplayButton = infoMenu.Find("PBReplay");
            if (pbReplayButton != null)
            {
                pbReplayButton.gameObject.SetActive(hasPB);
            }
        }

        public IEnumerator CheckWRReplayAvailability(string playerId)
        {
            string url = $"{Constants.ServerURL}/data/wr_replays/{TrialServerName}_{playerId}.json";
            
            using UnityWebRequest www = UnityWebRequest.Head(url);
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
            {
                wrReplayAvailable = true;
            }
            else
            {
                wrReplayAvailable = false;
            }
            
            UpdateRaceMenuAvailability();
            UpdateInfoMenuReplayButtons();
        }
        
        public BadgeType CheckBadgeEarned(float time)
        {
            
            if (BronzeTime == 0f || SilverTime == 0f || GoldTime == 0f)
            {
                return BadgeType.None;
            }

            if (time <= GoldTime)
            {
                return BadgeType.Gold;
            }
            if (time <= SilverTime)
            {
                return BadgeType.Silver;
            }
            if (time <= BronzeTime)
            {
                return BadgeType.Bronze;
            }
            
            return BadgeType.None;
        }
        
        public BadgeType GetSavedBadge()
        {
            string key = $"Badge_{TrialServerName}";
            return (BadgeType)PlayerPrefs.GetInt(key, 0);
        }
        
        public bool SaveBadgeIfBetter(BadgeType newBadge)
        {
            if (newBadge == BadgeType.None)
            {
                return false;
            }

            BadgeType currentBadge = GetSavedBadge();
            
            if ((int)newBadge > (int)currentBadge)
            {
                string key = $"Badge_{TrialServerName}";
                
                UpdateTotalBadgeCounts(currentBadge, newBadge);
                
                PlayerPrefs.SetInt(key, (int)newBadge);
                PlayerPrefs.Save();
                
                UpdateBadgeUI();
                return true;
            }
            
            return false;
        }

        private void UpdateTotalBadgeCounts(BadgeType oldBadge, BadgeType newBadge)
        {
            
            if (oldBadge != BadgeType.None)
            {
                string oldKey = $"Total_{oldBadge}Badges";
                int oldCount = PlayerPrefs.GetInt(oldKey, 0);
                int newOldCount = Math.Max(0, oldCount - 1);
                PlayerPrefs.SetInt(oldKey, newOldCount);
            }
            
            string newKey = $"Total_{newBadge}Badges";
            int newCount = PlayerPrefs.GetInt(newKey, 0);
            int updatedNewCount = newCount + 1;
            PlayerPrefs.SetInt(newKey, updatedNewCount);
        }
        
        private void UpdateBadgeUI()
        {
            BadgeType earnedBadge = GetSavedBadge();
            
            Transform badgesTransform = trialUIObject.transform.Find("UI/InfoMenu/TrialBadges");

            Transform bronzeuncheck = badgesTransform.Find("Bronze/BadgeIcon");
            Transform silveruncheck = badgesTransform.Find("Silver/BadgeIcon");
            Transform golduncheck = badgesTransform.Find("Gold/BadgeIcon");
            Transform bronzecheck = badgesTransform.Find("Bronze/BadgeIconCheck");
            Transform silvercheck = badgesTransform.Find("Silver/BadgeIconCheck");
            Transform goldcheck = badgesTransform.Find("Gold/BadgeIconCheck");
            
            if (bronzeuncheck != null) bronzeuncheck.gameObject.SetActive(earnedBadge < BadgeType.Bronze);
            if (bronzecheck != null) bronzecheck.gameObject.SetActive(earnedBadge >= BadgeType.Bronze);
            
            if (silveruncheck != null) silveruncheck.gameObject.SetActive(earnedBadge < BadgeType.Silver);
            if (silvercheck != null) silvercheck.gameObject.SetActive(earnedBadge >= BadgeType.Silver);
            
            if (golduncheck != null) golduncheck.gameObject.SetActive(earnedBadge < BadgeType.Gold);
            if (goldcheck != null) goldcheck.gameObject.SetActive(earnedBadge >= BadgeType.Gold);
        }
        
        public void LoadAcceptedChallenge()
        {
            string key = $"Challenge_{TrialServerName}";
            
            if (PlayerPrefs.HasKey(key))
            {
                string challengeId = PlayerPrefs.GetString(key, null);
                
                if (!string.IsNullOrEmpty(challengeId))
                {
                    hasAcceptedChallenge = true;
                    acceptedChallengeId = challengeId;
                    
                    challengeTimeToBeat = PlayerPrefs.GetFloat($"ChallengeTime_{TrialServerName}", 0f);
                    challengerUsername = PlayerPrefs.GetString($"ChallengeUsername_{TrialServerName}", "");
                    
                    
                    UpdateChallengeUI();
                }
                else
                {
                    Logging.Warning($"Challenge key exists but challengeId is empty for trial {TrialServerName}");
                }
            }
        }
        
        public void SaveAcceptedChallenge(string challengeId, float timeToBeat = 0f, string username = "")
        {
            if (string.IsNullOrEmpty(challengeId))
            {
                Logging.Error("Cannot save challenge: challengeId is null or empty");
                return;
            }
            
            string key = $"Challenge_{TrialServerName}";
            PlayerPrefs.SetString(key, challengeId);
            
            if (timeToBeat > 0f)
            {
                PlayerPrefs.SetFloat($"ChallengeTime_{TrialServerName}", timeToBeat);
            }
            if (!string.IsNullOrEmpty(username))
            {
                PlayerPrefs.SetString($"ChallengeUsername_{TrialServerName}", username);
            }
            
            PlayerPrefs.Save();
            
            hasAcceptedChallenge = true;
            acceptedChallengeId = challengeId;
            challengeTimeToBeat = timeToBeat;
            challengerUsername = username;
            
            UpdateChallengeUI();
        }
        
        public bool HasAcceptedChallenge()
        {
            return hasAcceptedChallenge && !string.IsNullOrEmpty(acceptedChallengeId);
        }
        
        public void UpdateChallengeUI()
        {
            if (trialUIObject == null)
            {
                Logging.Warning($"trialUIObject is null for trial {TrialServerName} (updatechallengeui)");
                return;
            }
            
            Transform challengeTextTransform = trialUIObject.transform.Find("UI/InfoMenu/ChallengeText");
            
            if (challengeTextTransform == null)
            {
                Logging.Warning($"challenge text not found for trial {TrialServerName}");
            }
            
            TextMeshProUGUI challengeText = challengeTextTransform.GetComponent<TextMeshProUGUI>();
            
            if (hasAcceptedChallenge && !string.IsNullOrEmpty(acceptedChallengeId))
            {
                challengeTextTransform.gameObject.SetActive(true);

                TimeSpan timeSpan = TimeSpan.FromSeconds(challengeTimeToBeat);
                string formattedTime = timeSpan.TotalHours >= 1 
                    ? timeSpan.ToString(@"h\:mm\:ss\.fff") 
                    : timeSpan.ToString(@"mm\:ss\.fff");
                
                string usernameDisplay = string.IsNullOrEmpty(challengerUsername) 
                    ? "" 
                    : $" ({challengerUsername})";
                
                string message = $"<color=yellow>You have a challenge for this Trial: Beat {formattedTime}{usernameDisplay}</color>";
                challengeText.text = message;
            }
            else
            {
                challengeTextTransform.gameObject.SetActive(false);
                challengeText.text = "";
            }
        }
        
        // challenge menu stuffs
        private void SetupChallengeMenu()
        {
            Transform challengeMenu = trialUIObject.transform.Find("UI/ChallengeMenu");
            if (challengeMenu == null)
            {
                Logging.Warning($"ChallengeMenu not found for trial {TrialServerName}");
                return;
            }
            
            Transform allowed = challengeMenu.Find("Allowed");
            if (allowed == null) return;
            
            TrialButton prevPage = allowed.Find("PrevPage")?.AddComponent<TrialButton>();
            if (prevPage != null)
            {
                prevPage.onPressed = () =>
                {
                    if (challengeMenuCurrentPage > 1)
                    {
                        challengeMenuCurrentPage--;
                        LoadChallengeMenuFriends();
                    }
                };
            }
            
            TrialButton nextPage = allowed.Find("NextPage")?.AddComponent<TrialButton>();
            if (nextPage != null)
            {
                nextPage.onPressed = () =>
                {
                    if (challengeMenuCurrentPage < challengeMenuTotalPages)
                    {
                        challengeMenuCurrentPage++;
                        LoadChallengeMenuFriends();
                    }
                };
            }
            
            Transform friendsContainer = allowed.Find("Friends");
            if (friendsContainer != null)
            {
                for (int i = 1; i <= 6; i++)
                {
                    Transform friendSlot = friendsContainer.Find(i.ToString());
                    if (friendSlot != null)
                    {
                        TrialButton selectButton = friendSlot.Find("Select")?.AddComponent<TrialButton>();
                        if (selectButton != null)
                        {
                            int slotIndex = i;
                            selectButton.onPressed = () => OnFriendSelected(slotIndex);
                        }
                    }
                }
            }
        }
        
        private void LoadChallengeMenuFriends()
        {
            if (isFromCustomMap && !onApprovedMap)
            {
                UpdateChallengeMenuAvailability(false, "Custom trials cannot send challenges.");
                return;
            }
            
            Singleton<TrialManager>.Instance.StartCoroutine(FetchChallengeMenuFriends());
        }
        
        private IEnumerator FetchChallengeMenuFriends()
        {
            string url = $"{Constants.ServerURL}/profile/friends?page={challengeMenuCurrentPage}";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Logging.Error($"Failed to fetch friends for challenge menu: {request.error}");
                    UpdateChallengeMenuAvailability(false, "Failed to load friends");
                    yield break;
                }
                
                string jsonResponse = request.downloadHandler.text;
                
                try
                {
                    var response = JsonConvert.DeserializeObject<FriendsResponse>(jsonResponse);
                    
                    if (response != null && response.friends != null && response.pagination != null)
                    {
                        challengeMenuTotalPages = response.pagination.totalPages;
                        
                        if (response.friends.Length == 0 && challengeMenuCurrentPage == 1)
                        {
                            UpdateChallengeMenuAvailability(false, "Sorry, you currently have no friends added so you cannot send a challenge request to anyone.\n\nAdd some friends at the Control Panel in Stump!");
                        }
                        else
                        {
                            UpdateChallengeMenuAvailability(true, "");
                            UpdateChallengeMenuFriendsUI(response.friends);
                        }
                    }
                    else
                    {
                        UpdateChallengeMenuAvailability(false, "Sorry, you currently have no friends added so you cannot send a challenge request to anyone.\n\nAdd some friends at the Control Panel in Stump!");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error($"Failed to parse friends response: {ex.Message}");
                    UpdateChallengeMenuAvailability(false, "Failed to load friends");
                }
            }
        }
        
        private void UpdateChallengeMenuAvailability(bool hasFriends, string message)
        {
            Transform challengeMenu = trialUIObject.transform.Find("UI/ChallengeMenu");
            if (challengeMenu == null) return;
            
            Transform allowed = challengeMenu.Find("Allowed");
            Transform notAllowed = challengeMenu.Find("NotAllowed");
            Transform notAllowedText = challengeMenu.Find("NotAllowed/Sorry :3");
            
            if (allowed != null) allowed.gameObject.SetActive(hasFriends);
            if (notAllowed != null)
            {
                notAllowed.gameObject.SetActive(!hasFriends);
                
                if (!hasFriends && !string.IsNullOrEmpty(message))
                {
                    TextMeshProUGUI messageText = notAllowedText.GetComponent<TextMeshProUGUI>();
                    if (messageText != null)
                    {
                        messageText.text = message;
                    }
                }
            }
        }
        
        private void UpdateChallengeMenuFriendsUI(FriendData[] friends)
        {
            Transform challengeMenu = trialUIObject.transform.Find("UI/ChallengeMenu");
            if (challengeMenu == null) return;
            
            Transform allowed = challengeMenu.Find("Allowed");
            if (allowed == null) return;
            
            Transform pageText = allowed.Find("Page");
            if (pageText != null)
            {
                TextMeshProUGUI pageTextComponent = pageText.GetComponent<TextMeshProUGUI>();
                if (pageTextComponent != null)
                {
                    pageTextComponent.text = $"Page {challengeMenuCurrentPage}/{challengeMenuTotalPages}";
                }
            }
            
            Transform friendsContainer = allowed.Find("Friends");
            if (friendsContainer == null) return;
            
            for (int i = 1; i <= 6; i++)
            {
                Transform friendSlot = friendsContainer.Find(i.ToString());
                if (friendSlot == null) continue;
                
                if (i <= friends.Length)
                {
                    FriendData friend = friends[i - 1];
                    
                    friendSlot.gameObject.SetActive(true);
                    
                    Transform nameTransform = friendSlot.Find("Name");
                    if (nameTransform != null)
                    {
                        TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                        if (nameText != null)
                        {
                            nameText.text = friend.username;
                        }
                    }
                    
                    bool isSelected = selectedChallengeRecipientId == friend.playerId;
                    Transform checkTransform = friendSlot.Find("Check");
                    Transform uncheckTransform = friendSlot.Find("UnCheck");
                    
                    if (checkTransform != null) checkTransform.gameObject.SetActive(isSelected);
                    if (uncheckTransform != null) uncheckTransform.gameObject.SetActive(!isSelected);
                    
                    var slotData = friendSlot.GetComponent<ChallengeMenuSlotData>();
                    if (slotData == null)
                    {
                        slotData = friendSlot.gameObject.AddComponent<ChallengeMenuSlotData>();
                    }
                    slotData.playerId = friend.playerId;
                    slotData.username = friend.username;
                }
                else
                {
                    friendSlot.gameObject.SetActive(false);
                }
            }
        }
        
        private void OnFriendSelected(int slotIndex)
        {
            Transform challengeMenu = trialUIObject.transform.Find("UI/ChallengeMenu");
            if (challengeMenu == null) return;
            
            Transform allowed = challengeMenu.Find("Allowed");
            if (allowed == null) return;
            
            Transform friendsContainer = allowed.Find("Friends");
            if (friendsContainer == null) return;
            
            Transform friendSlot = friendsContainer.Find(slotIndex.ToString());
            if (friendSlot == null) return;
            
            var slotData = friendSlot.GetComponent<ChallengeMenuSlotData>();
            if (slotData != null)
            {
                if (selectedChallengeRecipientId == slotData.playerId)
                {
                    selectedChallengeRecipientId = null;
                    selectedChallengeRecipientUsername = null;
                }
                else
                {
                    selectedChallengeRecipientId = slotData.playerId;
                    selectedChallengeRecipientUsername = slotData.username;
                    
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"{slotData.username} selected! Complete trial to send your time");
                    }
                }
                
                Singleton<TrialManager>.Instance.StartCoroutine(FetchChallengeMenuFriends());
            }
        }
        
        public IEnumerator SendChallengeAfterCompletion(double completedTime)
        {
            if (string.IsNullOrEmpty(selectedChallengeRecipientId))
            {
                Logging.Warning("No challenge recipient selected, skipping challenge send");
                yield break;
            }
            
            string url = $"{Constants.ServerURL}/challenges/send";
            
            var challengeData = new
            {
                friendId = selectedChallengeRecipientId,
                trialServerName = TrialServerName,
                trialLongName = TrialLongName,
                time = completedTime
            };
            
            string jsonBody = JsonConvert.SerializeObject(challengeData);
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
                
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Logging.Error($"Failed to send challenge: {request.error}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to send challenge: {request.downloadHandler.text}");
                    }
                }
                else
                {
                    Logging.Info($"Challenge sent to {selectedChallengeRecipientUsername} for trial {TrialServerName} with time {completedTime}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Challenge sent to {selectedChallengeRecipientUsername}!");
                    }
                    
                    selectedChallengeRecipientId = null;
                    selectedChallengeRecipientUsername = null;
                }
            }
        }
        
        public IEnumerator CompleteChallengeAfterCompletion(double completedTime)
        {
            if (!hasAcceptedChallenge || string.IsNullOrEmpty(acceptedChallengeId))
            {
                Logging.Warning("No accepted challenge for this trial, skipping challenge completion");
                yield break;
            }
            
            string url = $"{Constants.ServerURL}/challenges/complete/{acceptedChallengeId}";
            
            var completionData = new
            {
                completedTime = completedTime
            };
            
            string jsonBody = JsonConvert.SerializeObject(completionData);
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
                
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Logging.Error($"Failed to complete challenge: {request.error}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to complete challenge: {request.downloadHandler.text}");
                    }
                }
                else
                {
                    var response = JsonConvert.DeserializeObject<ChallengeCompletionResponse>(request.downloadHandler.text);
                    
                    string resultText = response.result == "win" ? "WON" : "LOST";
                    string resultColor = response.result == "win" ? "#00FF00" : "#FF0000";
                    
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"<color={resultColor}>Challenge {resultText}!</color> Your time: {response.yourTime:F3}s vs {challengerUsername}'s {response.challengeTime:F3}s");
                    }
                    
                    PlayerPrefs.DeleteKey($"Challenge_{TrialServerName}");
                    PlayerPrefs.DeleteKey($"ChallengeTime_{TrialServerName}");
                    PlayerPrefs.DeleteKey($"ChallengeUsername_{TrialServerName}");
                    PlayerPrefs.Save();
                    
                    TrialManager.Instance.RefreshAcceptedChallenges();
                    
                    hasAcceptedChallenge = false;
                    acceptedChallengeId = null;
                    challengeTimeToBeat = 0f;
                    challengerUsername = "";
                    UpdateChallengeUI();
                }
            }
        }
        
        [Serializable]
        private class ChallengeMenuSlotData : MonoBehaviour
        {
            public string playerId;
            public string username;
        }
        
        [Serializable]
        private class ChallengeCompletionResponse
        {
            public string message;
            public string result;
            public float yourTime;
            public float challengeTime;
        }
        
        [Serializable]
        private class FriendsResponse
        {
            public FriendData[] friends;
            public PaginationData pagination;
        }
        
        [Serializable]
        private class FriendData
        {
            public string playerId;
            public string username;
        }
        
        [Serializable]
        private class PaginationData
        {
            public int currentPage;
            public int totalPages;
            public int totalTrials;
            public int perPage;
            public bool hasNextPage;
            public bool hasPrevPage;
        }

        public void DeleteCustomTrial()
        {
            if (!isFromCustomMap)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(downloadedFileName))
            {
                Logging.Warning($"cannot delete trial {TrialServerName}: it doesnt have its filename stored ?");
                return;
            }
            
            try
            {
                // this shouldnt ever ever happen but just in case ig lol
                if (TrialManager.Instance != null && TrialManager.Instance.Started && TrialManager.Instance.currentTrial == this)
                {
                    stateMachine.SwitchState(new Trial_End(this, false));
                }
                
                if (trialUIObject != null)
                {
                    Destroy(trialUIObject);
                }
                
                if (TrialManager.Instance != null && TrialManager.Instance.Trials.Contains(this))
                {
                    TrialManager.Instance.Trials.Remove(this);
                }
                
                string executableDir = System.IO.Path.GetDirectoryName(Paths.ExecutablePath);
                string downloadedTrialsDir = System.IO.Path.Combine(executableDir, "downloadedtrials");
                string trialDataPath = System.IO.Path.Combine(downloadedTrialsDir, downloadedFileName);
                
                if (System.IO.File.Exists(trialDataPath))
                {
                    System.IO.File.Delete(trialDataPath);
                }
                else
                {
                    Logging.Error($"trial data file for {TrialServerName} not found at: {trialDataPath}");
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"error deleting custom trial {TrialServerName}: {ex.Message}");
            }
        }

        public enum TrialMenu
        {
            InfoMenu,
            DetailsMenu,
            RaceMenu,
            ChallengeMenu
        }

        public enum TrialDetailsText
        {
            AuthErrorText,
            BoardErrorText,
            CustomErrorText,
            GlobalBoardText
        }

        public enum RaceType
        {
            None,
            PB,
            WR
        }
    }
}
