using GorillaTrials.Behaviours.UI;
using GorillaTrials.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaTrials.Behaviours;


public class ControlPanel : MonoBehaviour
{
    public static ControlPanel instance;
    public GameObject controlPanelRoot, achievementUI, communityUI;
    public int currentPage = 1;
    public int maxPage = 2;
    public int minPage = 1;
    
    public int communityCurrentPage = 1;
    public int communityTotalPages = 1;
    public string communityFilter = "recent"; // can be recent, popular, or ranked
    async void Start()
    {
        await Initialize();
        UpdateAchievements();
    }

    async Task Initialize()
    {
        controlPanelRoot = await AssetLoader.LoadAsset<GameObject>("TrialUtilityMenu");
        controlPanelRoot = Instantiate(controlPanelRoot);
        DontDestroyOnLoad(controlPanelRoot);
        controlPanelRoot.transform.position = new Vector3(-69.3592f, 12.1929f, -83.4284f);
        controlPanelRoot.transform.rotation = Quaternion.Euler(358.9055f, 242.0654f, 0f);

        TrialButton achievements = controlPanelRoot.transform.Find("UI/ControlCenter/Buttons/Achievements").AddComponent<TrialButton>();
        TrialButton trialeditor = controlPanelRoot.transform.Find("UI/ControlCenter/Buttons/Trial Editor").AddComponent<TrialButton>();
        TrialButton communitytrials = controlPanelRoot.transform.Find("UI/ControlCenter/Buttons/Browse Trials").AddComponent<TrialButton>();
        
        communityUI = controlPanelRoot.transform.Find("UI/CommunityTrials").gameObject;
        achievementUI = controlPanelRoot.transform.Find("UI/Achievements").gameObject;

        achievements.onPressed = () =>
        {
            UpdateAchievements();
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(false);
            achievementUI.SetActive(true);
        };
        
        trialeditor.onPressed = () =>
        {
            if (TrialEditor.instance != null && TrialEditor.instance.panel != null)
            {
                TrialEditor.instance.panel.SetActive(true);
                TrialEditor.instance.editorUI.SetActive(true);
            }
            else
            {
                Logging.Error("TrialEditor instance or panel is null");
            }
        };

        communitytrials.onPressed = () =>
        {
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(false);
            communityUI.SetActive(true);
            LoadCommunityTrials();
        };

        
        // achievement logic
        
        achievementUI.transform.Find("Buttons/PrevPage").gameObject.layer = (int)UnityLayer.GorillaInteractable;
        achievementUI.transform.Find("Buttons/NextPage").gameObject.layer = (int)UnityLayer.GorillaInteractable;
        achievementUI.transform.Find("Buttons/Refresh").gameObject.layer = (int)UnityLayer.GorillaInteractable;
        TrialButton achinextpage = achievementUI.transform.Find("Buttons/NextPage").AddComponent<TrialButton>();
        TrialButton achiprevpage = achievementUI.transform.Find("Buttons/PrevPage").AddComponent<TrialButton>();
        TrialButton achirefresh = achievementUI.transform.Find("Buttons/Refresh").AddComponent<TrialButton>();
        TrialButton achireturn = achievementUI.transform.Find("Buttons/Return").AddComponent<TrialButton>();
        achievementUI.transform.Find("Text/Page").gameObject.GetComponent<TextMeshProUGUI>().text = $"Page {currentPage}/{maxPage}";


        achinextpage.onPressed = () =>
        {
            achievementUI.transform.Find($"Achievements/Page{currentPage}").gameObject.SetActive(false);
            currentPage += 1;
            if (currentPage > maxPage)
            {
                currentPage = maxPage;
            }
            achievementUI.transform.Find($"Achievements/Page{currentPage}").gameObject.SetActive(true);
            achievementUI.transform.Find("Info/Page").gameObject.GetComponent<TextMeshProUGUI>().text = $"Page {currentPage}/{maxPage}";
            UpdateAchievements();
        };

        achiprevpage.onPressed = () =>
        {
            achievementUI.transform.Find($"Achievements/Page{currentPage}").gameObject.SetActive(false);
            currentPage -= 1;
            if (currentPage < minPage)
            {
                currentPage = minPage;
            }
            achievementUI.transform.Find($"Achievements/Page{currentPage}").gameObject.SetActive(true);
            achievementUI.transform.Find("Info/Page").gameObject.GetComponent<TextMeshProUGUI>().text = $"Page {currentPage}/{maxPage}";
            UpdateAchievements();
        };

        achirefresh.onPressed = () =>
        {
            UpdateAchievements();
        };
        
        achireturn.onPressed = () =>
        {
            achievementUI.SetActive(false);
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(true);
        };
        
        // community trials logic
        TrialButton comreturn = communityUI.transform.Find("PageControls/Return").AddComponent<TrialButton>();
        TrialButton comnextpage = communityUI.transform.Find("PageControls/NextPage").AddComponent<TrialButton>();
        TrialButton comprevpage = communityUI.transform.Find("PageControls/BackPage").AddComponent<TrialButton>();
        TrialButton comrefresh = communityUI.transform.Find("PageControls/Refresh").AddComponent<TrialButton>();
        
        TrialButton filterRecent = communityUI.transform.Find("PageControls/Recent").AddComponent<TrialButton>();
        TrialButton filterPopular = communityUI.transform.Find("PageControls/Popular").AddComponent<TrialButton>();
        TrialButton filterRanked = communityUI.transform.Find("PageControls/Ranked").AddComponent<TrialButton>();
        
        
        comreturn.onPressed = () =>
        {
            communityUI.SetActive(false);
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(true);
        };
        
        comnextpage.onPressed = () =>
        {
            if (communityCurrentPage < communityTotalPages)
            {
                communityCurrentPage++;
                LoadCommunityTrials();
            }
        };
        
        comprevpage.onPressed = () =>
        {
            if (communityCurrentPage > 1)
            {
                communityCurrentPage--;
                LoadCommunityTrials();
            }
        };
        
        comrefresh.onPressed = () =>
        {
            LoadCommunityTrials();
        };
        
        if (filterRecent != null)
        {
            filterRecent.onPressed = () =>
            {
                communityFilter = "recent";
                communityCurrentPage = 1;
                LoadCommunityTrials();
            };
        }
        
        if (filterPopular != null)
        {
            filterPopular.onPressed = () =>
            {
                communityFilter = "popular";
                communityCurrentPage = 1;
                LoadCommunityTrials();
            };
        }
        
        if (filterRanked != null)
        {
            filterRanked.onPressed = () =>
            {
                communityFilter = "ranked";
                communityCurrentPage = 1;
                LoadCommunityTrials();
            };
        }
    }

    public void LoadCommunityTrials()
    {
        StartCoroutine(FetchCommunityTrials());
    }
    
    private IEnumerator FetchCommunityTrials()
    {
        Logging.Info($"fetching started - filter: {communityFilter}, page: {communityCurrentPage}");
        string url = $"{Constants.ServerURL}/trials/browse?filter={communityFilter}&page={communityCurrentPage}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch community trials: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            
            BrowseTrialsResponse response = JsonConvert.DeserializeObject<BrowseTrialsResponse>(jsonResponse);
            
            if (response != null)
            {
                if (response.trials != null && response.pagination != null)
                {
                    communityTotalPages = response.pagination.totalPages;
                    Logging.Info($"Total pages: {communityTotalPages}");
                    UpdateCommunityTrialsUI(response.trials);
                }
            }
        }
    }
    
    private void UpdateCommunityTrialsUI(CommunityTrialData[] trials)
    {
        Logging.Info($"UpdateCommunityTrialsUI called with {trials.Length} trials");
        Transform trialsContainer = communityUI.transform.Find("Trials");
        
        if (trialsContainer == null)
        {
            Logging.Error("Trials container not found!");
            return;
        }
        
        Logging.Info($"Trials container found, updating slots...");
        
        for (int i = 1; i <= 6; i++)
        {
            Transform trialSlot = trialsContainer.Find(i.ToString());
            
            if (trialSlot == null)
            {
                Logging.Warning($"Trial slot {i} not found!");
                continue;
            }
            
            if (i <= trials.Length)
            {
                CommunityTrialData trial = trials[i - 1];
                
                Logging.Info($"Updating slot {i} with trial: {trial.name}");
                
                trialSlot.gameObject.SetActive(true);
                trialSlot.Find("TrialName").GetComponent<TextMeshProUGUI>().text = trial.name;
                trialSlot.Find("TrialDescription").GetComponent<TextMeshProUGUI>().text = trial.description;
                trialSlot.Find("TrialID").GetComponent<TextMeshProUGUI>().text = $"ID: {trial.trialId}";
                trialSlot.Find("CreatedAt").GetComponent<TextMeshProUGUI>().text = $"{FormatDate(trial.uploadedAt)}" + (trial.isRanked ? " [RANKED]" : "");
                
                Transform downloadBtn = trialSlot.Find("Download");
                TrialButton btn = downloadBtn.GetComponent<TrialButton>();
                if (btn == null)
                {
                    btn = downloadBtn.AddComponent<TrialButton>();
                }
                
                string trialId = trial.trialId;
                btn.onPressed = () =>
                {
                    DownloadTrial(trialId);
                };
            }
            else
            {
                Logging.Info($"Hiding slot {i} (no data)");
                trialSlot.gameObject.SetActive(false);
            }
        }
        
        Logging.Info("UpdateCommunityTrialsUI complete");
    }
    private string FormatDate(string dateString)
    {
        try
        {
            DateTime date = DateTime.Parse(dateString);
            return date.ToString("MMM dd, yyyy");
        }
        catch
        {
            return dateString;
        }
    }
    
    private void DownloadTrial(string trialId)
    {
        Logging.Info($"Downloading trial: {trialId}");
        StartCoroutine(DownloadTrialData(trialId));
    }
    
    private IEnumerator DownloadTrialData(string trialId)
    {
        string url = $"{Constants.ServerURL}/trials/download/{trialId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to download trial {trialId}: {request.error}");
                yield break;
            }
            
            string trialJson = request.downloadHandler.text;
            
            string executableDir = Path.GetDirectoryName(Paths.ExecutablePath);
            if (string.IsNullOrEmpty(executableDir))
            {
                Logging.Error("Failed to get executable directory path");
                yield break;
            }
            
            string downloadedTrialsDir = Path.Combine(executableDir, "downloadedtrials");
            
            if (!Directory.Exists(downloadedTrialsDir))
            {
                Directory.CreateDirectory(downloadedTrialsDir);
            }
            
            string filePath = Path.Combine(downloadedTrialsDir, $"{trialId}.json");
            File.WriteAllText(filePath, trialJson);
            
            Logging.Info($"Successfully downloaded and saved trial to: {filePath}");
            
            bool needsUpdate = false;
            string rankedUrl = $"{Constants.ServerURL}/trials/rankedids";
            using (UnityWebRequest rankedRequest = UnityWebRequest.Get(rankedUrl))
            {
                yield return rankedRequest.SendWebRequest();
                
                if (rankedRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string rankedJson = rankedRequest.downloadHandler.text;
                        Dictionary<string, string> rankedTrialIds = JsonConvert.DeserializeObject<Dictionary<string, string>>(rankedJson);
                        Models.TrialDataModel tempData = JsonConvert.DeserializeObject<Models.TrialDataModel>(trialJson);
                        
                        if (tempData != null && rankedTrialIds != null && rankedTrialIds.ContainsKey(tempData.trialId))
                        {
                            string friendlyId = rankedTrialIds[tempData.trialId];
                            Logging.Info($"Trial {tempData.trialId} is ranked with friendly ID: {friendlyId}");
                            
                            if (tempData.customMapTrial)
                            {
                                tempData.customMapTrial = false;
                                needsUpdate = true;
                                Logging.Info($"Updated trial {tempData.trialId} to no longer be a custom map trial");
                            }
                            
                            if (tempData.trialId != friendlyId)
                            {
                                tempData.trialId = friendlyId;
                                needsUpdate = true;
                                Logging.Info($"Updated trial ID to friendly ID: {friendlyId}");
                            }
                            
                            if (needsUpdate)
                            {
                                trialJson = JsonConvert.SerializeObject(tempData, Formatting.Indented);
                                File.WriteAllText(filePath, trialJson);
                                Logging.Info($"Saved updated trial data to: {filePath}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Warning($"Failed to process ranked trial check: {ex.Message}");
                    }
                }
            }
            
            try
            {
                Models.TrialDataModel trialData = JsonConvert.DeserializeObject<Models.TrialDataModel>(trialJson);
                
                if (trialData != null)
                {
                    if (TrialManager.Instance.Trials.Any(t => t.TrialServerName == trialData.trialId))
                    {
                        Logging.Info($"Trial {trialData.trialId} already exists");
                        if (HUDManager.instance != null)
                        {
                            HUDManager.instance.SetHUDText($"Trial already loaded: {trialData.displayName}");
                        }
                        StartCoroutine(ClearHUDDelayed(3f));
                        yield break;
                    }
                    
                    if (!Enum.TryParse(trialData.trialType, true, out Models.ETrialType trialType))
                    {
                        Logging.Error($"Invalid trial type '{trialData.trialType}' for trial '{trialId}'");
                        yield break;
                    }
                    
                    if (!Enum.TryParse(trialData.trialDifficulty, true, out Models.ETrialDifficulty trialDifficulty))
                    {
                        Logging.Warning($"Invalid trial difficulty '{trialData.trialDifficulty}' for trial '{trialId}'. Defaulting to Easy.");
                        trialDifficulty = Models.ETrialDifficulty.Easy;
                    }
                    
                    List<Vector3> points = trialData.points?.ConvertAll(p => p.ToVector3());
                    object[] parameters = null;
                    if (points != null && points.Count > 0)
                    {
                        parameters = new object[] { points };
                    }
                    
                    TrialManager.Instance.CreateTrial(
                        trialData.displayName,
                        trialData.trialId,
                        trialData.position.ToVector3(),
                        trialData.angle,
                        trialType,
                        trialDifficulty,
                        trialData.maxTime,
                        trialData.customMapTrial,
                        parameters
                    );
                    
                    Logging.Info($"Successfully spawned trial: {trialData.displayName}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Downloaded & Spawned: {trialData.displayName}");
                    }
                }
                else
                {
                    Logging.Error($"Failed to parse trial JSON for {trialId}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Downloaded but failed to spawn: {trialId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Error spawning downloaded trial: {ex.Message}");
                if (HUDManager.instance != null)
                {
                    HUDManager.instance.SetHUDText($"Downloaded but failed to spawn: {trialId}");
                }
            }
            
            StartCoroutine(ClearHUDDelayed(3f));
        }
    }



    public void UpdateAchievements()
    {
        if (Plugin.achievementManager.IsUnlocked("first_trial"))
        {
            achievementUI.transform.Find("Achievements/Page1/FirstTrial/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("stump_climb_champ"))
        {
            achievementUI.transform.Find("Achievements/Page1/StumpClimbMaster/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("adv_hp2"))
        {
            achievementUI.transform.Find("Achievements/Page1/HP2SM/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("5trials"))
        {
            achievementUI.transform.Find("Achievements/Page1/5Trials/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("10trials"))
        {
            achievementUI.transform.Find("Achievements/Page1/10Trials/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("20trials"))
        {
            achievementUI.transform.Find("Achievements/Page1/20Trials/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("30trials"))
        {
            achievementUI.transform.Find("Achievements/Page2/30Trials/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("vinemaster"))
        {
            achievementUI.transform.Find("Achievements/Page2/VineMaster/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("masterswimmer"))
        {
            achievementUI.transform.Find("Achievements/Page2/MasterSwimmer/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("slowpoke"))
        {
            achievementUI.transform.Find("Achievements/Page2/Slowpoke/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("ultraslowpoke"))
        {
            achievementUI.transform.Find("Achievements/Page2/UltraSlowpoke/CompletedText").gameObject.SetActive(true);
        }
    }
    
    private IEnumerator ClearHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        HUDManager.instance.ClearHUD();
    }
    
    [Serializable]
    public class BrowseTrialsResponse
    {
        public CommunityTrialData[] trials;
        public PaginationData pagination;
    }
    
    [Serializable]
    public class CommunityTrialData
    {
        public string trialId;
        public string uploadedBy;
        public string uploadedAt;
        public string name;
        public string description;
        public int downloads;
        public bool isRanked;
    }


    [Serializable]
    public class PaginationData
    {
        public int currentPage;
        public int totalPages;
        public int totalTrials;
        public int perPage;
        public bool hasNextPage;
        public bool hasPrevPage;
    }
}