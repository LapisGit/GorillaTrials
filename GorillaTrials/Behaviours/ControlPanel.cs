using GorillaTrials.Behaviours.UI;
using GorillaTrials.Models;
using GorillaTrials.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using GorillaNetworking;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace GorillaTrials.Behaviours;


public class ControlPanel : MonoBehaviour
{
    public static ControlPanel instance;
    public GameObject controlPanelRoot, achievementUI, communityUI, accountUI, profileUI, notificationUI, friendsUI;
    public int currentPage = 1;
    public int maxPage = 2;
    public int minPage = 1;
    
    public int communityCurrentPage = 1;
    public int communityTotalPages = 1;
    public string communityFilter = "recent"; // can be recent, popular, or ranked
    
    public int notificationCurrentPage = 1;
    public int notificationTotalPages = 1;
    private int lastNotificationCount = 0;
    
    public int friendsCurrentPage = 1;
    public int friendsTotalPages = 1;
    public int friendRequestsCurrentPage = 1;
    public int friendRequestsTotalPages = 1;
    
    public int searchCurrentPage = 1;
    public int searchTotalPages = 1;
    public string searchQuery = "";

    public string username = "";
    public string bio = "";
    public CommunityTrialData[] createdTrials;
    
    public bool openProfile = false;
    public bool openFromSearch = false;
    public bool openFromCommunity = false;

    public PlayerProfileData myProfile;
    public PlayerProfileData profile;

    public bool requestsPage;
    public bool searchPage;

    public string eventUrl;
    public int eventTotalCompleted;
    public int eventRequiredAmount;
    public int eventPlayerRank;
    
    public void Awake()
    {
        instance = this;
    }
    
    async void Start()
    {
        await Initialize();
        LoadNotifications();
        UpdateAchievements();
        StartCoroutine(FetchOwnPlayerProfile());
        CalculateSumOfBest();
        StartCoroutine(CheckForEvent());
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
        TrialButton accountManager = controlPanelRoot.transform.Find("UI/ControlCenter/Buttons/AccountManager").AddComponent<TrialButton>();
        TrialButton notifications = controlPanelRoot.transform.Find("UI/ControlCenter/Notifications").AddComponent<TrialButton>();
        TrialButton friends = controlPanelRoot.transform.Find("UI/ControlCenter/Friends").AddComponent<TrialButton>();
        
        communityUI = controlPanelRoot.transform.Find("UI/CommunityTrials").gameObject;
        achievementUI = controlPanelRoot.transform.Find("UI/Achievements").gameObject;
        accountUI = controlPanelRoot.transform.Find("UI/Account").gameObject;
        profileUI = controlPanelRoot.transform.Find("UI/ViewingPlayer").gameObject;
        notificationUI = controlPanelRoot.transform.Find("UI/Notifications").gameObject;
        friendsUI = controlPanelRoot.transform.Find("UI/Friends").gameObject;

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

        accountManager.onPressed = () =>
        {
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(false);
            accountUI.SetActive(true);
        };
        
        notifications.onPressed = () =>
        {
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(false);
            notificationUI.SetActive(true);
            LoadNotifications();
        };

        friends.onPressed = () =>
        {
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(false);
            friendsUI.SetActive(true);
            friendsUI.transform.Find("Friends").gameObject.SetActive(true);
            friendsUI.transform.Find("Requests").gameObject.SetActive(false);
            friendsUI.transform.Find("SearchResults").gameObject.SetActive(false);
            friendsCurrentPage = 1;
            LoadFriends();
        };

        
        // achievement logic
        
        if (achievementUI == null)
        {
            Logging.Error("achievementUI is null in ControlPanel.Initialize");
            return;
        }
        
        Transform prevPageBtn = achievementUI.transform.Find("Buttons/PrevPage");
        Transform nextPageBtn = achievementUI.transform.Find("Buttons/NextPage");
        Transform refreshBtn = achievementUI.transform.Find("Buttons/Refresh");
        Transform returnBtn = achievementUI.transform.Find("Buttons/Return");
        Transform pageText = achievementUI.transform.Find("Text/Page");
        
        if (prevPageBtn == null || nextPageBtn == null || refreshBtn == null || returnBtn == null || pageText == null)
        {
            Logging.Error("One or more achievement UI elements not found. Check asset structure.");
            Logging.Error($"prevPageBtn: {prevPageBtn != null}, nextPageBtn: {nextPageBtn != null}, refreshBtn: {refreshBtn != null}, returnBtn: {returnBtn != null}, pageText: {pageText != null}");
            return;
        }
        
        prevPageBtn.gameObject.layer = (int)UnityLayer.GorillaInteractable;
        nextPageBtn.gameObject.layer = (int)UnityLayer.GorillaInteractable;
        refreshBtn.gameObject.layer = (int)UnityLayer.GorillaInteractable;
        TrialButton achinextpage = nextPageBtn.AddComponent<TrialButton>();
        TrialButton achiprevpage = prevPageBtn.AddComponent<TrialButton>();
        TrialButton achirefresh = refreshBtn.AddComponent<TrialButton>();
        TrialButton achireturn = returnBtn.AddComponent<TrialButton>();
        pageText.gameObject.GetComponent<TextMeshProUGUI>().text = $"Page {currentPage}/{maxPage}";


        achinextpage.onPressed = () =>
        {
            currentPage += 1;
            if (currentPage > maxPage)
            {
                currentPage = maxPage;
            }
            UpdateAchievements();
        };

        achiprevpage.onPressed = () =>
        {
            currentPage -= 1;
            if (currentPage < minPage)
            {
                currentPage = minPage;
            }
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
                communityUI.transform.Find("Text/SearchType").GetComponent<TextMeshProUGUI>().text = "Recent";
            };
        }
        
        if (filterPopular != null)
        {
            filterPopular.onPressed = () =>
            {
                communityFilter = "popular";
                communityCurrentPage = 1;
                LoadCommunityTrials();
                communityUI.transform.Find("Text/SearchType").GetComponent<TextMeshProUGUI>().text = "Popular";
            };
        }
        
        if (filterRanked != null)
        {
            filterRanked.onPressed = () =>
            {
                communityFilter = "ranked";
                communityCurrentPage = 1;
                LoadCommunityTrials();
                communityUI.transform.Find("Text/SearchType").GetComponent<TextMeshProUGUI>().text = "Ranked";
            };
        }
        
        // account manager logic
        
        TrialButton accreturn = accountUI.transform.Find("Buttons/Return").AddComponent<TrialButton>();
        TrialButton pageuploadedtrials = accountUI.transform.Find("Buttons/UploadedTrials").AddComponent<TrialButton>();
        
        TrialButton pageaccountinfo = accountUI.transform.Find("Buttons/AccountInfo").AddComponent<TrialButton>();
        TrialButton editusername = accountUI.transform.Find("AccountInfo/UsernameHeader/Edit").AddComponent<TrialButton>();
        TrialButton editbio = accountUI.transform.Find("AccountInfo/BioHeader/Edit").AddComponent<TrialButton>();
        
        TrialButton pagestats = accountUI.transform.Find("Buttons/Stats").AddComponent<TrialButton>();

        accreturn.onPressed = () =>
        {
            accountUI.SetActive(false);
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(true);
        };
        
        pageaccountinfo.onPressed = () =>
        {
            accountUI.transform.Find("AccountInfo").gameObject.SetActive(true);
            accountUI.transform.Find("UploadedTrials").gameObject.SetActive(false);
            accountUI.transform.Find("Stats").gameObject.SetActive(false);
        };
        
        pageuploadedtrials.onPressed = () =>
        {
            accountUI.transform.Find("UploadedTrials").gameObject.SetActive(true);
            accountUI.transform.Find("AccountInfo").gameObject.SetActive(false);
            accountUI.transform.Find("Stats").gameObject.SetActive(false);
        };
        
        pagestats.onPressed = () =>
        {
            UpdateStatsUI();
            accountUI.transform.Find("Stats").gameObject.SetActive(true);
            accountUI.transform.Find("AccountInfo").gameObject.SetActive(false);
            accountUI.transform.Find("UploadedTrials").gameObject.SetActive(false);
        };
        
        editusername.onPressed = () =>
        {
            OpenKeyboardForUsername();
        };
        
        editbio.onPressed = () =>
        {
            OpenKeyboardForBio();
        };
        
        TrialButton profilereturn = profileUI.transform.Find("Buttons/Return").AddComponent<TrialButton>();
        
        profilereturn.onPressed = () =>
        {
            if (openFromCommunity)
            {
                openFromCommunity = false;
                communityUI.SetActive(true);
                profileUI.transform.Find("Info").gameObject.SetActive(true);
                profileUI.transform.Find("UploadedTrials").gameObject.SetActive(false);
            }
            else if (openFromSearch)
            {
                openFromSearch = false;
                friendsUI.SetActive(true);
                profileUI.transform.Find("Info").gameObject.SetActive(true);
                profileUI.transform.Find("UploadedTrials").gameObject.SetActive(false);
            }
            else
            {
                accountUI.SetActive(true);
            }
            profileUI.SetActive(false);
        };
        
        TrialButton addfriend = profileUI.transform.Find("Buttons/Friend").AddComponent<TrialButton>();
        
        addfriend.onPressed = () =>
        {
            if (profile != null)
            {
                StartCoroutine(SendFriendRequest(profile.playerId));
            }
        };
        
        TrialButton profileInfo = profileUI.transform.Find("Buttons/Info").AddComponent<TrialButton>();
        
        profileInfo.onPressed = () =>
        {
            if (profile != null)
            {
                profileUI.transform.Find("Info").gameObject.SetActive(true);
                profileUI.transform.Find("UploadedTrials").gameObject.SetActive(false);
                profileUI.transform.Find("Tab").gameObject.GetComponent<TextMeshProUGUI>().text = "Info";
            }
        };
        
        TrialButton uploadedTrials = profileUI.transform.Find("Buttons/UploadedTrials").AddComponent<TrialButton>();
        
        uploadedTrials.onPressed = () =>
        {
            if (profile != null)
            {
                UpdateProfileUploadedTrialsUI();
                profileUI.transform.Find("Info").gameObject.SetActive(false);
                profileUI.transform.Find("UploadedTrials").gameObject.SetActive(true);
                profileUI.transform.Find("Tab").gameObject.GetComponent<TextMeshProUGUI>().text = "Uploaded Trials";
            }
        };
        
        // notification logic
        
        TrialButton notifreturn = notificationUI.transform.Find("Buttons/Return").AddComponent<TrialButton>();
        TrialButton notifnextpage = notificationUI.transform.Find("Buttons/NextPage").AddComponent<TrialButton>();
        TrialButton notifprevpage = notificationUI.transform.Find("Buttons/PrevPage").AddComponent<TrialButton>();
        TrialButton notifrefresh = notificationUI.transform.Find("Buttons/Refresh").AddComponent<TrialButton>();
        
        notifreturn.onPressed = () =>
        {
            notificationUI.SetActive(false);
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(true);
        };
        
        notifnextpage.onPressed = () =>
        {
            if (notificationCurrentPage < notificationTotalPages)
            {
                notificationCurrentPage++;
                LoadNotifications();
            }
        };
        
        notifprevpage.onPressed = () =>
        {
            if (notificationCurrentPage > 1)
            {
                notificationCurrentPage--;
                LoadNotifications();
            }
        };
        
        notifrefresh.onPressed = () =>
        {
            LoadNotifications();
        };
        
        
        // friends logic
        
        TrialButton friendsreturn = friendsUI.transform.Find("Buttons/Return").AddComponent<TrialButton>();
        TrialButton friendsnextpage = friendsUI.transform.Find("Buttons/NextPage").AddComponent<TrialButton>();
        TrialButton friendsprevpage = friendsUI.transform.Find("Buttons/PrevPage").AddComponent<TrialButton>();
        TrialButton friendsrefresh = friendsUI.transform.Find("Buttons/Refresh").AddComponent<TrialButton>();
        TrialButton toggle = friendsUI.transform.Find("Buttons/Toggle").AddComponent<TrialButton>();
        TrialButton search = friendsUI.transform.Find("Buttons/Search").AddComponent<TrialButton>();
        
        friendsreturn.onPressed = () =>
        {
            friendsUI.SetActive(false);
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(true);
        };
        
        friendsnextpage.onPressed = () =>
        {
            bool isFriendsTab = friendsUI.transform.Find("Friends").gameObject.activeSelf;
            bool isRequestsTab = friendsUI.transform.Find("Requests").gameObject.activeSelf;
            
            if (isFriendsTab)
            {
                if (friendsCurrentPage < friendsTotalPages)
                {
                    friendsCurrentPage++;
                    LoadFriends();
                }
            }
            else if (isRequestsTab)
            {
                if (friendRequestsCurrentPage < friendRequestsTotalPages)
                {
                    friendRequestsCurrentPage++;
                    LoadFriendRequests();
                }
            }
            else if (searchPage)
            {
                if (searchCurrentPage < searchTotalPages)
                {
                    searchCurrentPage++;
                    PerformSearch(searchQuery);
                }
            }
        };
        
        friendsprevpage.onPressed = () =>
        {
            bool isFriendsTab = friendsUI.transform.Find("Friends").gameObject.activeSelf;
            bool isRequestsTab = friendsUI.transform.Find("Requests").gameObject.activeSelf;
            
            if (isFriendsTab)
            {
                if (friendsCurrentPage > 1)
                {
                    friendsCurrentPage--;
                    LoadFriends();
                }
            }
            else if (isRequestsTab)
            {
                if (friendRequestsCurrentPage > 1)
                {
                    friendRequestsCurrentPage--;
                    LoadFriendRequests();
                }
            }
            else if (searchPage)
            {
                if (searchCurrentPage > 1)
                {
                    searchCurrentPage--;
                    PerformSearch(searchQuery);
                }
            }
        };
        
        friendsrefresh.onPressed = () =>
        {
            bool isFriendsTab = friendsUI.transform.Find("Friends").gameObject.activeSelf;
            bool isRequestsTab = friendsUI.transform.Find("Requests").gameObject.activeSelf;
            
            if (isFriendsTab)
            {
                LoadFriends();
            }
            else if (isRequestsTab)
            {
                LoadFriendRequests();
            }
            else if (searchPage)
            {
                PerformSearch(searchQuery);
            }
        };
        
        toggle.onPressed = () =>
        {
            if (!requestsPage)
            {
                friendsUI.transform.Find("Buttons/Toggle/Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Friends";
                friendsUI.transform.Find("Friends").gameObject.SetActive(false);
                friendsUI.transform.Find("Requests").gameObject.SetActive(true);
                friendsUI.transform.Find("SearchResults").gameObject.SetActive(false);
                requestsPage = true;
                searchPage = false;
                friendRequestsCurrentPage = 1;
                LoadFriendRequests();
            }
            else
            {
                friendsUI.transform.Find("Buttons/Toggle/Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Requests";
                friendsUI.transform.Find("Friends").gameObject.SetActive(true);
                friendsUI.transform.Find("Requests").gameObject.SetActive(false);
                friendsUI.transform.Find("SearchResults").gameObject.SetActive(false);
                requestsPage = false;
                searchPage = false;
                friendsCurrentPage = 1;
                LoadFriends();   
            }
        };
        
        if (search != null)
        {
            search.onPressed = () =>
            {
                if (TrialKeyboard.instance != null && TrialKeyboard.instance.keyboard != null)
                {
                    TrialKeyboard.instance.keyboard.SetActive(true);
                    TrialKeyboard.instance.SetMaxLength(20);
                    TrialKeyboard.instance.SetText("@");
                    TrialKeyboard.instance.forUsername = true;
                    
                    TrialKeyboard.instance.onSubmit = (query) =>
                    {
                        TrialKeyboard.instance.keyboard.SetActive(false);
                        if (!string.IsNullOrEmpty(query))
                        {
                            searchQuery = query;
                            searchCurrentPage = 1;
                            searchPage = true;
                            requestsPage = false;
                            
                            friendsUI.transform.Find("Friends").gameObject.SetActive(false);
                            friendsUI.transform.Find("Requests").gameObject.SetActive(false);
                            friendsUI.transform.Find("SearchResults")?.gameObject.SetActive(true);
                            
                            PerformSearch(query);
                        }
                    };
                    
                    TrialKeyboard.instance.onCancel = () =>
                    {
                        TrialKeyboard.instance.keyboard.SetActive(false);
                    };
                }
            };
        }
        
        // event logic
        
        TrialButton eventBtn = controlPanelRoot.transform.Find("UI/ControlCenter/Event").AddComponent<TrialButton>();
        TrialButton eventReturn = controlPanelRoot.transform.Find("UI/Event/Buttons/Return").AddComponent<TrialButton>();;
        TrialButton eventTrailer = controlPanelRoot.transform.Find("UI/Event/Buttons/WhatIsThis").AddComponent<TrialButton>();
        TrialButton eventRefresh = controlPanelRoot.transform.Find("UI/Event/Buttons/RefreshBoard").AddComponent<TrialButton>();
        
        eventBtn.onPressed = () =>
        {
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(false);
            controlPanelRoot.transform.Find("UI/Event").gameObject.SetActive(true);
            StartCoroutine(GetEventLeaderboard());
        };
        
        eventReturn.onPressed = () =>
        {
            controlPanelRoot.transform.Find("UI/ControlCenter").gameObject.SetActive(true);
            controlPanelRoot.transform.Find("UI/Event").gameObject.SetActive(false);
        };
        
        eventTrailer.onPressed = () =>
        {
            Application.OpenURL(eventUrl);
            HUDManager.instance.SetHUDText("Opening event details in browser...");
        };
        
        eventRefresh.onPressed = () =>
        {
            StartCoroutine(GetEventLeaderboard());
        };
    }

    public void LoadCommunityTrials()
    {
        StartCoroutine(FetchCommunityTrials());
    }
    
    private IEnumerator FetchCommunityTrials()
    {
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
                    UpdateCommunityTrialsUI(response.trials);
                }
            }
        }
    }
    
    private void UpdateCommunityTrialsUI(CommunityTrialData[] trials)
    {
        Transform trialsContainer = communityUI.transform.Find("Trials");
        
        if (trialsContainer == null)
        {
            Logging.Error("Trials container not found!");
            return;
        }
        
        Transform pageText = communityUI.transform.Find("Text/Page");
        if (pageText != null)
        {
            pageText.GetComponent<TextMeshProUGUI>().text = $"Page {communityCurrentPage}/{communityTotalPages}";
        }
        
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
                
                trialSlot.gameObject.SetActive(true);
                trialSlot.Find("TrialName").GetComponent<TextMeshProUGUI>().text = trial.name;
                trialSlot.Find("TrialDescription").GetComponent<TextMeshProUGUI>().text = trial.description;
                trialSlot.Find("TrialID").GetComponent<TextMeshProUGUI>().text = $"ID: {trial.trialId}";
                trialSlot.Find("CreatedAt").GetComponent<TextMeshProUGUI>().text = $"{FormatDate(trial.uploadedAt)}" + (trial.isRanked ? " [RANKED]" : "");
                
                TextMeshProUGUI creatorText = trialSlot.Find("Creator").GetComponent<TextMeshProUGUI>();
                creatorText.text = "Made by Loading...";
                StartCoroutine(FetchAndDisplayCreatorName(trial.uploadedBy, creatorText));
                
                Transform downloadBtn = trialSlot.Find("Download");
                downloadBtn.gameObject.layer = (int)UnityLayer.GorillaInteractable;
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
                
                Transform profileBtn = trialSlot.Find("ViewProfile");
                profileBtn.gameObject.layer = (int)UnityLayer.GorillaInteractable;
                TrialButton btn2 = profileBtn.GetComponent<TrialButton>();
                if (btn2 == null)
                {
                    btn2 = profileBtn.AddComponent<TrialButton>();
                }

                string creatorId = trial.uploadedBy;
                btn2.onPressed = () =>
                {
                    openProfile = true;
                    openFromCommunity = true;
                    openFromSearch = false;
                    StartCoroutine(FetchPlayerProfileCoroutine(creatorId));
                };
            }
            else
            {
                trialSlot.gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateUploadedTrialsUI()
    {
        if (createdTrials == null || createdTrials.Length == 0)
        {
            accountUI.transform.Find("UploadedTrials/HasTrials").gameObject.SetActive(false);
            accountUI.transform.Find("UploadedTrials/NoUploadedTrials").gameObject.SetActive(true);
            return;
        }
        
        Transform trialsContainer = accountUI.transform.Find("UploadedTrials/HasTrials/Trials");
        
        if (trialsContainer == null)
        {
            Logging.Error("Uploaded trials container not found!");
            return;
        }
        
        for (int i = 1; i <= 6; i++)
        {
            Transform trialSlot = trialsContainer.Find(i.ToString());
            
            if (trialSlot == null)
            {
                continue;
            }
            
            if (i <= createdTrials.Length)
            {
                CommunityTrialData trial = createdTrials[i - 1];
                
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
                trialSlot.gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateProfileUploadedTrialsUI()
    {
        var trials = profile?.uploadedTrials;
        var hasTrialsObj = profileUI.transform.Find("UploadedTrials/HasTrials")?.gameObject;
        var noTrialsObj = profileUI.transform.Find("UploadedTrials/NoUploadedTrials")?.gameObject;

        if (trials == null || trials.Length == 0)
        {
            hasTrialsObj?.SetActive(false);
            noTrialsObj?.SetActive(true);
            return;
        }

        hasTrialsObj?.SetActive(true);
        noTrialsObj?.SetActive(false);

        Transform trialsContainer = profileUI.transform.Find("UploadedTrials/HasTrials/Trials");
        if (trialsContainer == null)
        {
            Logging.Error("Uploaded trials container not found!");
            return;
        }

        for (int i = 1; i <= 6; i++)
        {
            Transform trialSlot = trialsContainer.Find(i.ToString());
            if (trialSlot == null) continue;

            if (i <= trials.Length)
            {
                CommunityTrialData trial = trials[i - 1];

                trialSlot.gameObject.SetActive(true);
                trialSlot.Find("TrialName").GetComponent<TextMeshProUGUI>().text = trial.name;
                trialSlot.Find("TrialDescription").GetComponent<TextMeshProUGUI>().text = trial.description;
                trialSlot.Find("TrialID").GetComponent<TextMeshProUGUI>().text = $"ID: {trial.trialId}";
                trialSlot.Find("CreatedAt").GetComponent<TextMeshProUGUI>().text = $"{FormatDate(trial.uploadedAt)}" + (trial.isRanked ? " [RANKED]" : "");

                Transform downloadBtn = trialSlot.Find("Download");
                TrialButton btn = downloadBtn.GetComponent<TrialButton>() ?? downloadBtn.gameObject.AddComponent<TrialButton>();
                string trialId = trial.trialId;
                btn.onPressed = () => { DownloadTrial(trialId); };
            }
            else
            {
                trialSlot.gameObject.SetActive(false);
            }
        }
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
    
    private void UpdateStatsUI()
    {
        Transform statsContainer = accountUI.transform.Find("Stats");
        
        if (statsContainer == null)
        {
            Logging.Error("Stats container not found!");
            return;
        }
        
        int trialsAttempted = PlayerPrefs.GetInt("Stats_TrialsAttempted", 0);
        int trialsCompleted = PlayerPrefs.GetInt("Stats_TrialsCompleted", 0);
        int customTrialsUploaded = PlayerPrefs.GetInt("Stats_CustomTrialsUploaded", 0);
        
        float sumOfBestOfficial = CalculateSumOfBest();
        
        Transform trialsAttemptedText = statsContainer.Find("TrialsAttempted/Text");
        if (trialsAttemptedText != null)
        {
            trialsAttemptedText.GetComponent<TextMeshProUGUI>().text = trialsAttempted.ToString();
        }
        
        Transform trialsCompletedText = statsContainer.Find("TrialsCompleted/Text");
        if (trialsCompletedText != null)
        {
            trialsCompletedText.GetComponent<TextMeshProUGUI>().text = trialsCompleted.ToString();
        }
        
        Transform customTrialsUploadedText = statsContainer.Find("CustomTrialsUploaded/Text");
        if (customTrialsUploadedText != null)
        {
            customTrialsUploadedText.GetComponent<TextMeshProUGUI>().text = customTrialsUploaded.ToString();
        }
        
        Transform sumOfBestOfficialText = statsContainer.Find("SOBOffical/Text");
        if (sumOfBestOfficialText != null)
        {
            string formattedTime = FormatTime(sumOfBestOfficial);
            sumOfBestOfficialText.GetComponent<TextMeshProUGUI>().text = formattedTime;
        }
    }
    
    public float CalculateSumOfBest()
    {
        float sum = 0f;
        
        if (TrialManager.Instance != null && TrialManager.Instance.Trials != null)
        {
            foreach (var trial in TrialManager.Instance.Trials)
            {
                bool isCustomTrial = trial.isFromCustomMap && !trial.onApprovedMap;
                if (!isCustomTrial)
                {
                    string pbKey = $"PB_{trial.TrialServerName}";
                    float pb = PlayerPrefs.GetFloat(pbKey, 0f);
                    if (pb > 0f)
                    {
                        sum += pb;
                    }
                }
            }
        }
        
        return sum;
    }
    
    public static void IncrementTrialsAttempted()
    {
        int current = PlayerPrefs.GetInt("Stats_TrialsAttempted", 0);
        PlayerPrefs.SetInt("Stats_TrialsAttempted", current + 1);
        PlayerPrefs.Save();
    }
    
    public static void IncrementTrialsCompleted()
    {
        int current = PlayerPrefs.GetInt("Stats_TrialsCompleted", 0);
        PlayerPrefs.SetInt("Stats_TrialsCompleted", current + 1);
        PlayerPrefs.Save();
    }
    
    public static void IncrementCustomTrialsUploaded()
    {
        int current = PlayerPrefs.GetInt("Stats_CustomTrialsUploaded", 0);
        PlayerPrefs.SetInt("Stats_CustomTrialsUploaded", current + 1);
        PlayerPrefs.Save();
    }
    
    private string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds == 0)
        {
            return "N/A";
        }
        
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);
        
        if (minutes > 0)
        {
            return $"{minutes}:{seconds:D2}.{milliseconds:D3}";
        }
        else
        {
            return $"{seconds}.{milliseconds:D3}";
        }
    }
    
    private void DownloadTrial(string trialId)
    {
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
                            
                            if (tempData.customMapTrial)
                            {
                                tempData.customMapTrial = false;
                                needsUpdate = true;
                                Logging.Info($"Updated trial {tempData.trialId} to be ranked");
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
                        if (HUDManager.instance != null)
                        {
                            HUDManager.instance.SetHUDText($"Trial already loaded: {trialData.displayName}");
                        }
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
                        parameters,
                        0,
                        0,
                        0,
                        $"{trialId}.json"
                    );
                    
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Loaded: {trialData.displayName}");
                    }
                }
                else
                {
                    Logging.Error($"Failed to parse trial JSON for {trialId}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to parse trial JSON for {trialId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Error loading downloaded trial: {ex.Message}");
                if (HUDManager.instance != null)
                {
                    HUDManager.instance.SetHUDText($"Downloaded but failed to load: {trialId}. Check logs for more details");
                }
            }
        }
    }



    public void UpdateAchievements()
    {
        List<Achievement> allAchievements = Plugin.achievementManager.GetAllAchievements();
        
        int achievementsPerPage = 6;
        int totalAchievements = allAchievements.Count;
        
        maxPage = Mathf.CeilToInt((float)totalAchievements / achievementsPerPage);
        if (maxPage < 1) maxPage = 1;
        
        Transform pageText = achievementUI.transform.Find("Text/Page");
        if (pageText != null)
        {
            pageText.GetComponent<TextMeshProUGUI>().text = $"Page {currentPage}/{maxPage}";
        }
        
        Transform achievementsContainer = achievementUI.transform.Find("Achievements");
        
        int startIndex = (currentPage - 1) * achievementsPerPage;
        int endIndex = Mathf.Min(startIndex + achievementsPerPage, totalAchievements);
        
        for (int i = 1; i <= achievementsPerPage; i++)
        {
            Transform achievementSlot = achievementsContainer.Find(i.ToString());
            
            if (achievementSlot == null)
            {
                continue;
            }

            int achievementIndex = startIndex + (i - 1);
            
            if (achievementIndex < endIndex)
            {
                Achievement achievement = allAchievements[achievementIndex];
                
                achievementSlot.gameObject.SetActive(true);
                
                Transform nameText = achievementSlot.Find("AchievementName");
                if (nameText != null)
                {
                    nameText.GetComponent<TextMeshProUGUI>().text = achievement.Name;
                }
                
                Transform descText = achievementSlot.Find("AchievementDescription");
                if (descText != null)
                {
                    descText.GetComponent<TextMeshProUGUI>().text = achievement.Description;
                }
                
                Transform completedText = achievementSlot.Find("CompletedText");
                if (completedText != null)
                {
                    completedText.gameObject.SetActive(achievement.Unlocked);
                }
            }
            else
            {
                achievementSlot.gameObject.SetActive(false);
            }
        }
    }
    
    private IEnumerator NotificationPoller()
    {
        var wait = new WaitForSeconds(5f);
        while (true)
        {
            yield return wait;
            yield return FetchNotificationsAndAlert();
        }
    }

    private IEnumerator FetchNotificationsAndAlert()
    {
        string url = $"{Constants.ServerURL}/notifications?page={notificationCurrentPage}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch notifications: {request.error}");
                yield break;
            }

            string jsonResponse = request.downloadHandler.text;
            NotificationsResponse response = JsonConvert.DeserializeObject<NotificationsResponse>(jsonResponse);

            if (response != null && response.notifications != null && response.pagination != null)
            {
                int currentCount = response.notifications.Length;
                notificationTotalPages = response.pagination.totalPages;
                if (currentCount > lastNotificationCount)
                {
                    lastNotificationCount = currentCount;
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.ShowNotificationAlert("New notifications received!");
                    }
                    controlPanelRoot.transform.Find("UI/ControlCenter/Notifications/Backdrop/NoNotif").gameObject.SetActive(false);
                    controlPanelRoot.transform.Find("UI/ControlCenter/Notifications/Backdrop/Notif").gameObject.SetActive(true);
                }
                else if (currentCount < lastNotificationCount)
                {
                    lastNotificationCount = currentCount;
                }

                if (lastNotificationCount == 0)
                {
                    controlPanelRoot.transform.Find("UI/ControlCenter/Notifications/Backdrop/NoNotif").gameObject.SetActive(true);
                    controlPanelRoot.transform.Find("UI/ControlCenter/Notifications/Backdrop/Notif").gameObject.SetActive(false);
                }
            }
        }
    }
    
    public void LoadNotifications()
    {
        StartCoroutine(FetchNotifications());
    }
    
    private IEnumerator FetchNotifications()
    {
        string url = $"{Constants.ServerURL}/notifications?page={notificationCurrentPage}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch notifications: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            
            NotificationsResponse response = JsonConvert.DeserializeObject<NotificationsResponse>(jsonResponse);
            
            if (response != null)
            {
                if (response.notifications != null && response.pagination != null)
                {
                    notificationTotalPages = response.pagination.totalPages;
                    lastNotificationCount = response.notifications.Length;
                    UpdateNotificationsUI(response.notifications);
                }

                if (lastNotificationCount == 0)
                {
                    controlPanelRoot.transform.Find("UI/ControlCenter/Notifications/Backdrop/NoNotif").gameObject
                        .SetActive(true);
                    controlPanelRoot.transform.Find("UI/ControlCenter/Notifications/Backdrop/Notif").gameObject
                        .SetActive(false);
                }
                else
                {
                    controlPanelRoot.transform.Find("UI/ControlCenter/Notifications/Backdrop/NoNotif").gameObject.SetActive(false);
                    controlPanelRoot.transform.Find("UI/ControlCenter/Notifications/Backdrop/Notif").gameObject.SetActive(true);
                }
            }
        }
    }
    
    private void UpdateNotificationsUI(NotificationData[] notifications)
    {
        
        notificationUI.transform.Find("Text/Page").GetComponent<TextMeshProUGUI>().text = $"Page {notificationCurrentPage}/{notificationTotalPages}";
        
        Transform notificationsContainer = notificationUI.transform.Find("Notifs");
        
        int notificationsPerPage = 6;
        
        for (int i = 1; i <= notificationsPerPage; i++)
        {
            Transform notificationSlot = notificationsContainer.Find(i.ToString());
            
            Transform acceptFriendTransform = notificationSlot.transform.Find("FriendReqButtons/Accept");
            Transform declineFriendTransform = notificationSlot.transform.Find("FriendReqButtons/Decline");
            Transform acceptChallengeTransform = notificationSlot.transform.Find("ChallengeReqButtons/Accept");
            Transform declineChallengeTransform = notificationSlot.transform.Find("ChallengeReqButtons/Decline");
            Transform markReadTransform = notificationSlot.transform.Find("Basic/MarkRead");
            
            TrialButton acceptfriend = acceptFriendTransform.GetComponent<TrialButton>();
            if (acceptfriend == null) acceptfriend = acceptFriendTransform.gameObject.AddComponent<TrialButton>();
            
            TrialButton declinefriend = declineFriendTransform.GetComponent<TrialButton>();
            if (declinefriend == null) declinefriend = declineFriendTransform.gameObject.AddComponent<TrialButton>();
            
            TrialButton acceptchallenge = acceptChallengeTransform.GetComponent<TrialButton>();
            if (acceptchallenge == null) acceptchallenge = acceptChallengeTransform.gameObject.AddComponent<TrialButton>();
            
            TrialButton declinechallenge = declineChallengeTransform.GetComponent<TrialButton>();
            if (declinechallenge == null) declinechallenge = declineChallengeTransform.gameObject.AddComponent<TrialButton>();
            
            TrialButton markread = markReadTransform.GetComponent<TrialButton>();
            if (markread == null) markread = markReadTransform.gameObject.AddComponent<TrialButton>();
            
            Transform friendReqButtons = notificationSlot.transform.Find("FriendReqButtons");
            Transform challengeReqButtons = notificationSlot.transform.Find("ChallengeReqButtons");
            Transform basicButtons = notificationSlot.transform.Find("Basic");
            
            friendReqButtons.gameObject.SetActive(false);
            challengeReqButtons.gameObject.SetActive(false);
            basicButtons.gameObject.SetActive(false);
            
            if (i <= notifications.Length)
            {
                NotificationData notification = notifications[i - 1];
                
                notificationSlot.gameObject.SetActive(true);
                
                notificationSlot.Find("Description").GetComponent<TextMeshProUGUI>().text = notification.message;
                
                if (notification.notificationType == "friendreq")
                {
                    notificationSlot.Find("Name").GetComponent<TextMeshProUGUI>().text = "New Friend Request!";
                    string friendPlayerId = notification.fromPlayerId;
                    acceptfriend.onPressed = () => StartCoroutine(AcceptFriendRequest(friendPlayerId));
                    declinefriend.onPressed = () => StartCoroutine(DeclineFriendRequest(friendPlayerId));
                    friendReqButtons.gameObject.SetActive(true);
                }
                else if (notification.notificationType == "challenge")
                {
                    string challengeId = notification.challengeId;
                    string trialServerName = notification.trialServerName;
                    float timeToBeat = notification.time;
                    string challengerUsername = notification.fromUsername;
                    notificationSlot.Find("Name").GetComponent<TextMeshProUGUI>().text = "New Challenge Request!";
                    acceptchallenge.onPressed = () => StartCoroutine(AcceptChallenge(challengeId, trialServerName, timeToBeat, challengerUsername));
                    declinechallenge.onPressed = () => StartCoroutine(DeclineChallenge(challengeId));
                    challengeReqButtons.gameObject.SetActive(true);
                }
                else
                {
                    markread.onPressed = () => StartCoroutine(MarkNotificationAsRead(notification.notificationId));
                    basicButtons.gameObject.SetActive(true);
                }
            }
            else
            {
                notificationSlot.gameObject.SetActive(false);
            }
        }
    }
    
    public void LoadFriends()
    {
        StartCoroutine(FetchFriends());
    }
    
    private IEnumerator FetchFriends()
    {
        string url = $"{Constants.ServerURL}/profile/friends?page={friendsCurrentPage}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch friends: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            
            FriendsResponse response = JsonConvert.DeserializeObject<FriendsResponse>(jsonResponse);
            
            if (response != null && response.friends != null && response.pagination != null)
            {
                friendsTotalPages = response.pagination.totalPages;
                UpdateFriendsUI(response.friends);
            }
        }
    }
    
    private void UpdateFriendsUI(FriendData[] friends)
    {
        friendsUI.transform.Find("Text/Page").GetComponent<TextMeshProUGUI>().text = $"Page {friendsCurrentPage}/{friendsTotalPages}";
        
        Transform friendsContainer = friendsUI.transform.Find("Friends/Friends");
        
        if (friendsTotalPages == 0)
        {
            friendsUI.transform.Find("Friends/None").gameObject.SetActive(true);
            friendsUI.transform.Find("Text/Page").GetComponent<TextMeshProUGUI>().text = $"";
            
            if (friendsContainer != null)
            {
                for (int i = 1; i <= 6; i++)
                {
                    friendsContainer.Find(i.ToString())?.gameObject.SetActive(false);
                }
            }
            return;
        }
        
        friendsUI.transform.Find("Friends/None").gameObject.SetActive(false);
        
        if (friendsContainer == null) return;
        
        for (int i = 1; i <= 6; i++)
        {
            Transform friendSlot = friendsContainer.Find(i.ToString());
            if (friendSlot == null) continue;
            
            if (i <= friends.Length)
            {
                FriendData friend = friends[i - 1];
                friendSlot.gameObject.SetActive(true);
                
                friendSlot.Find("Name").GetComponent<TextMeshProUGUI>().text = friend.username;
                
                Transform removeTransform = friendSlot.Find("Remove");
                if (removeTransform != null)
                {
                    removeTransform.gameObject.layer = (int)UnityLayer.GorillaInteractable;
                    
                    TrialButton removeButton = removeTransform.GetComponent<TrialButton>();
                    if (removeButton == null)
                    {
                        removeButton = removeTransform.gameObject.AddComponent<TrialButton>();
                    }
                    
                    string friendId = friend.playerId;
                    removeButton.onPressed = () => StartCoroutine(RemoveFriend(friendId));
                }
            }
            else
            {
                friendSlot.gameObject.SetActive(false);
            }
        }
    }
    
    public void LoadFriendRequests()
    {
        StartCoroutine(FetchFriendRequests());
    }
    
    private IEnumerator FetchFriendRequests()
    {
        string url = $"{Constants.ServerURL}/profile/friendrequests?page={friendRequestsCurrentPage}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch friend requests: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            
            FriendRequestsResponse response = JsonConvert.DeserializeObject<FriendRequestsResponse>(jsonResponse);
            
            if (response != null && response.friendRequests != null && response.pagination != null)
            {
                friendRequestsTotalPages = response.pagination.totalPages;
                UpdateFriendRequestsUI(response.friendRequests);
            }
        }
    }
    
    private void UpdateFriendRequestsUI(FriendRequestData[] requests)
    {
        friendsUI.transform.Find("Text/Page").GetComponent<TextMeshProUGUI>().text = $"Page {friendRequestsCurrentPage}/{friendRequestsTotalPages}";
        
        Transform requestsContainer = friendsUI.transform.Find("Requests/Friends");
        int requestsPerPage = 6;
        
        if (friendRequestsTotalPages == 0)
        {
            friendsUI.transform.Find("Requests/None").gameObject.SetActive(true);
            friendsUI.transform.Find("Text/Page").GetComponent<TextMeshProUGUI>().text = $"";
            
            if (requestsContainer != null)
            {
                for (int i = 1; i <= requestsPerPage; i++)
                {
                    requestsContainer.Find(i.ToString()).gameObject.SetActive(false);
                }
            }
            return;
        }
        
        friendsUI.transform.Find("Requests/None").gameObject.SetActive(false);
        
        for (int i = 1; i <= requestsPerPage; i++)
        {
            Transform requestSlot = requestsContainer.Find(i.ToString());
            
            if (requestSlot == null)
            {
                continue;
            }
            
            if (i <= requests.Length)
            {
                FriendRequestData request = requests[i - 1];
                
                requestSlot.gameObject.SetActive(true);
                
                requestSlot.Find("Name").GetComponent<TextMeshProUGUI>().text = "New Friend Request!";
                requestSlot.Find("Description").GetComponent<TextMeshProUGUI>().text = $"{request.username} has sent you a friend request!";
                
                Transform acceptTransform = requestSlot.Find("FriendReqButtons/Accept");
                if (acceptTransform != null)
                {
                    acceptTransform.gameObject.layer = (int)UnityLayer.GorillaInteractable;
                    
                    TrialButton acceptButton = acceptTransform.GetComponent<TrialButton>();
                    if (acceptButton == null)
                    {
                        acceptButton = acceptTransform.gameObject.AddComponent<TrialButton>();
                    }
                    
                    string friendId = request.playerId;
                    acceptButton.onPressed = () => StartCoroutine(AcceptFriendRequest(friendId));
                }
                
                Transform declineTransform = requestSlot.Find("FriendReqButtons/Decline");
                if (declineTransform != null)
                {
                    declineTransform.gameObject.layer = (int)UnityLayer.GorillaInteractable;
                    
                    TrialButton declineButton = declineTransform.GetComponent<TrialButton>();
                    if (declineButton == null)
                    {
                        declineButton = declineTransform.gameObject.AddComponent<TrialButton>();
                    }
                    
                    string friendIddecline = request.playerId;
                    declineButton.onPressed = () => StartCoroutine(DeclineFriendRequest(friendIddecline));
                }
            }
            else
            {
                requestSlot.gameObject.SetActive(false);
            }
        }
    }
    
    private IEnumerator RemoveFriend(string friendId)
    {
        string url = $"{Constants.ServerURL}/profile/removefriend";
        
        var requestData = new { friendId = friendId };
        string jsonData = JsonConvert.SerializeObject(requestData);
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to remove friend: {request.error}");
                string errorMessage = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logging.Error($"Server response: {errorMessage}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to remove friend: {errorMessage}");
                    }
                }
                yield break;
            }
            
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Friend removed");
            }
            
            LoadFriends();
        }
    }
    
    public void PerformSearch(string query)
    {
        StartCoroutine(FetchSearchResults(query));
    }
    
    private IEnumerator FetchSearchResults(string query)
    {
        string url = $"{Constants.ServerURL}/profile/search?query={UnityWebRequest.EscapeURL(query)}&page={searchCurrentPage}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to search players: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            SearchResponse response = JsonConvert.DeserializeObject<SearchResponse>(jsonResponse);
            
            if (response != null && response.results != null)
            {
                if (response.pagination != null)
                {
                    searchTotalPages = response.pagination.totalPages;
                }
                else
                {
                    searchTotalPages = response.results.Length > 0 ? 1 : 0;
                }
                
                UpdateSearchUI(response.results);
            }
        }
    }
    
    private void UpdateSearchUI(SearchResult[] results)
    {
        friendsUI.transform.Find("Text/Page").GetComponent<TextMeshProUGUI>().text = $"Page {searchCurrentPage}/{searchTotalPages}";
        
        Transform searchResultsContainer = friendsUI.transform.Find("SearchResults/Friends");
        
        if (searchTotalPages == 0 || results.Length == 0)
        {
            friendsUI.transform.Find("SearchResults/None")?.gameObject.SetActive(true);
            friendsUI.transform.Find("Text/Page").GetComponent<TextMeshProUGUI>().text = $"";
            
            if (searchResultsContainer != null)
            {
                for (int i = 1; i <= 6; i++)
                {
                    searchResultsContainer.Find(i.ToString())?.gameObject.SetActive(false);
                }
            }
            return;
        }
        
        friendsUI.transform.Find("SearchResults/None")?.gameObject.SetActive(false);
        
        if (searchResultsContainer == null) return;
        
        for (int i = 1; i <= 6; i++)
        {
            Transform searchSlot = searchResultsContainer.Find(i.ToString());
            if (searchSlot == null) continue;
            
            if (i <= results.Length)
            {
                SearchResult result = results[i - 1];
                searchSlot.gameObject.SetActive(true);
                
                searchSlot.Find("Name").GetComponent<TextMeshProUGUI>().text = result.username;
                
                Transform viewTransform = searchSlot.Find("ViewProfile");
                if (viewTransform != null)
                {
                    TrialButton viewButton = viewTransform.AddComponent<TrialButton>();
                    
                    string playerId = result.playerId;
                    viewButton.onPressed = () => ViewPlayerProfile(playerId);
                }
            }
            else
            {
                searchSlot.gameObject.SetActive(false);
            }
        }
    }
    
    private IEnumerator AcceptFriendRequest(string playerId)
    {
        string url = $"{Constants.ServerURL}/profile/acceptfriend";
        
        var requestData = new { friendId = playerId };
        string jsonData = JsonConvert.SerializeObject(requestData);
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to accept friend request: {request.error}");
                string errorMessage = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logging.Error($"Server response: {errorMessage}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to accept: {errorMessage}");
                    }
                }
                yield break;
            }
            
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Friend request accepted!");
            }
            
            if (friendsUI.activeSelf && friendsUI.transform.Find("Requests").gameObject.activeSelf)
            {
                LoadFriendRequests();
            }
            else
            {
                LoadNotifications();
            }
        }
    }
    
    private IEnumerator DeclineFriendRequest(string playerId)
    {
        string url = $"{Constants.ServerURL}/profile/declinefriend";
        
        var requestData = new { friendId = playerId };
        string jsonData = JsonConvert.SerializeObject(requestData);
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to decline friend request: {request.error}");
                string errorMessage = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logging.Error($"Server response: {errorMessage}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to decline: {errorMessage}");
                    }
                }
                yield break;
            }
            
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Friend request declined");
            }
            
            if (friendsUI.activeSelf && friendsUI.transform.Find("Requests").gameObject.activeSelf)
            {
                LoadFriendRequests();
            }
            else
            {
                LoadNotifications();
            }
        }
    }

    private IEnumerator AcceptChallenge(string challengeId, string trialServerName, float timeToBeat,
        string challengerUsername)
    {
        Trial targetTrial = TrialManager.Instance.Trials.Find(t => t.TrialServerName == trialServerName);
        if (targetTrial != null && targetTrial.HasAcceptedChallenge())
        {
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("You already have an active challenge for this trial!");
            }

            yield break;
        }

        string url = $"{Constants.ServerURL}/challenges/accept/{challengeId}";

        using (UnityWebRequest request = UnityWebRequest.Post(url, "", "application/json"))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to accept challenge: {request.error}");
                string errorMessage = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logging.Error($"Server response: {errorMessage}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to accept: {errorMessage}");
                    }
                }

                yield break;
            }

            if (targetTrial != null)
            {
                targetTrial.SaveAcceptedChallenge(challengeId, timeToBeat, challengerUsername);
            }
            else
            {
                string key = $"Challenge_{trialServerName}";
                PlayerPrefs.SetString(key, challengeId);
                PlayerPrefs.SetFloat($"ChallengeTime_{trialServerName}", timeToBeat);
                PlayerPrefs.SetString($"ChallengeUsername_{trialServerName}", challengerUsername);
                PlayerPrefs.Save();
            }

            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Challenge accepted!");

                LoadNotifications();
                TrialManager.Instance.RefreshAcceptedChallenges();
            }
        }
    }

    private IEnumerator DeclineChallenge(string challengeId)
    {
        string url = $"{Constants.ServerURL}/challenges/decline/{challengeId}";
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, "", "application/json"))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to decline challenge: {request.error}");
                string errorMessage = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logging.Error($"Server response: {errorMessage}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to decline: {errorMessage}");
                    }
                }
                yield break;
            }
            
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Challenge declined");
            }
            
            LoadNotifications();
        }
    }
    
    public IEnumerator MarkNotificationAsRead(string notificationId)
    {
        string url = $"{Constants.ServerURL}/notifications/read/{notificationId}";
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, "", "application/json"))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to mark notification as read: {request.error}");
                string errorMessage = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logging.Error($"Server response: {errorMessage}");
                }
                yield break;
            }
            LoadNotifications();
        }
    }
    
    
    public void ViewPlayerProfile(string playerId)
    {
        openProfile = true;
        openFromSearch = true;
        openFromCommunity = false;
        StartCoroutine(FetchPlayerProfileCoroutine(playerId));
    }
    
    public IEnumerator FetchOwnPlayerProfile()
    {
        string playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
        while (string.IsNullOrEmpty(playerId))
        {
            yield return new WaitForSeconds(3f);
            playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
        }
        StartCoroutine(FetchPlayerProfileCoroutine(playerId));
    }
    
    private IEnumerator FetchPlayerProfileCoroutine(string playerId)
    {
        string url = $"{Constants.ServerURL}/profile/{playerId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch player profile for {playerId}: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            
            profile = JsonConvert.DeserializeObject<PlayerProfileData>(jsonResponse);
            
            if (openProfile)
            {
                if (profile != null)
                {
                    communityUI.gameObject.SetActive(false);
                    friendsUI.gameObject.SetActive(false);
                    profileUI.gameObject.SetActive(true);
                    profileUI.transform.Find("Info/UsernameHeader/Username/Text").GetComponent<TextMeshProUGUI>().text = profile.username;
                    profileUI.transform.Find("Info/BioHeader/Text").GetComponent<TextMeshProUGUI>().text = profile.bio ?? "";
                    openProfile = false;
                }
            }
            else if (playerId == PlayFabAuthenticator.instance.GetPlayFabPlayerId())
            {
                if (profile != null)
                {
                    username = profile.username;
                    bio = profile.bio;
                    createdTrials = profile.uploadedTrials;
                    
                    if (profile.bio == null)
                    {
                        bio = "";
                    }
                    
                    accountUI.transform.Find("AccountInfo/UsernameHeader/Username/Text").GetComponent<TextMeshProUGUI>().text = username;
                    accountUI.transform.Find("AccountInfo/BioHeader/Text").GetComponent<TextMeshProUGUI>().text = bio;
                    
                    UpdateUploadedTrialsUI();
                    UpdateStatsUI();
                    
                    myProfile = profile;
                }
            }
        }
    }
    
    private IEnumerator FetchAndDisplayCreatorName(string playerId, TextMeshProUGUI textField)
    {
        string url = $"{Constants.ServerURL}/profile/{playerId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch creator profile for {playerId}: {request.error}");
                if (textField != null)
                {
                    textField.text = "Made by Unknown";
                }
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            PlayerProfileData creatorProfile = JsonConvert.DeserializeObject<PlayerProfileData>(jsonResponse);
            
            if (creatorProfile != null && textField != null)
            {
                textField.text = $"Made by {creatorProfile.username ?? "Unknown"}";
            }
        }
    }
    
    private IEnumerator CheckForEvent()
    {
        string url = $"{Constants.ServerURL}/event";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"failed to get event: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            
            EventResponse response = JsonConvert.DeserializeObject<EventResponse>(jsonResponse);
            
            if (response != null && response.active == true)
            {
                string startIso = response.createdAt;
                string endIso = response.endDate;
                DateTime startdate = DateTime.Parse(startIso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                DateTime enddate = DateTime.Parse(endIso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                int startday = startdate.Day;
                int endday = enddate.Day;
                string startmonth = startdate.ToString("MMMM", CultureInfo.InvariantCulture);
                string endmonth = enddate.ToString("MMMM", CultureInfo.InvariantCulture);
                string startsuffix = GetDateSuffix(startday);
                string endsuffix = GetDateSuffix(endday);

                eventUrl = response.url;
                
                eventTotalCompleted = response.totalCompleted;
                eventRequiredAmount = response.requiredAmount;
                
                controlPanelRoot.transform.Find("UI/ControlCenter/Event/EventDate").gameObject
                    .GetComponent<TextMeshProUGUI>().text = $"{startmonth} {startday}{startsuffix} - {endmonth} {endday}{endsuffix}";
                
                controlPanelRoot.transform.Find("UI/ControlCenter/Buttons").gameObject
                    .GetComponent<VerticalLayoutGroup>().spacing = 30;
                
                controlPanelRoot.transform.Find("UI/ControlCenter/Event/EventName").gameObject
                    .GetComponent<TextMeshProUGUI>().text = response.displayName;

                controlPanelRoot.transform.Find("UI/Event/Text/Header (5)").GetComponent<TextMeshProUGUI>().text =
                    response.displayName;
                
                UpdateEventProgress();
                
                controlPanelRoot.transform.Find("UI/ControlCenter/Event").gameObject.SetActive(true);
            }
            else
            {
                controlPanelRoot.transform.Find("UI/ControlCenter/Buttons").gameObject
                    .GetComponent<VerticalLayoutGroup>().spacing = 75;
                controlPanelRoot.transform.Find("UI/ControlCenter/Event").gameObject.SetActive(false); 
            }
        }
    }
    
    private IEnumerator GetEventLeaderboard()
    {
        yield return CheckForEvent();
        
        yield return GetEventPlayerRank();
        
        string url = $"{Constants.ServerURL}/event/leaderboard";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"failed to get event leaderboard: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            
            try
            {
                List<EventLeaderboardEntry> leaderboardEntries = JsonConvert.DeserializeObject<List<EventLeaderboardEntry>>(jsonResponse);
                
                if (leaderboardEntries != null && leaderboardEntries.Count > 0)
                {
                    UpdateEventLeaderboard(leaderboardEntries);
                    Logging.Info($"Event leaderboard refreshed successfully");
                }
                else
                {
                    Logging.Warning("no leaderboard entries found");
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"failed to parse event leaderboard response: {ex.Message}");
            }
        }
    }

    private IEnumerator GetEventPlayerRank()
    {
        string url = $"{Constants.ServerURL}/event/myrank";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"failed to get player event rank: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            
            try
            {
                EventPlayerRankResponse response = JsonConvert.DeserializeObject<EventPlayerRankResponse>(jsonResponse);
                
                if (response != null)
                {
                    eventPlayerRank = response.rank;
                    UpdateEventPlayerRankUI();
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"failed to parse player event rank response: {ex.Message}");
            }
        }
    }

    private void UpdateEventPlayerRankUI()
    {
        try
        {
            Transform rankTextTransform = controlPanelRoot.transform.Find("UI/Event/EventStats/Leaderboard/YourRank");
            if (rankTextTransform != null)
            {
                TextMeshProUGUI rankText = rankTextTransform.GetComponent<TextMeshProUGUI>();
                if (rankText != null)
                {
                    rankText.text = $"Your Rank: #{eventPlayerRank}";
                }
                else
                {
                    Logging.Warning("no text comp found");
                }
            }
            else
            {
                Logging.Warning("gameobject not found for myrank");
            }
        }
        catch (Exception ex)
        {
            Logging.Error($"error updating player rank: {ex.Message}");
        }
    }
    
    public IEnumerator SendFriendRequest(string playerId)
    {
        string url = $"{Constants.ServerURL}/profile/addfriend";
        
        var requestData = new { friendId = playerId };
        string jsonData = JsonConvert.SerializeObject(requestData);
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to send friend request: {request.error}");
                string errorMessage = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logging.Error($"Server response: {errorMessage}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to send friend request: {errorMessage}");
                    }
                }
                yield break;
            }
            
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Friend request sent!");
            }
        }
    }
    
    private IEnumerator SavePlayerProfileCoroutine()
    {
        if (!string.IsNullOrEmpty(username))
        {
            yield return SaveUsername(username);
        }
        
        yield return SaveBio(bio);
        
        string playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
        while (string.IsNullOrEmpty(playerId))
        {
            yield return new WaitForSeconds(3f);
            playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
        }
        
        yield return FetchPlayerProfileCoroutine(playerId);
    }
    
    private IEnumerator SaveUsername(string newUsername)
    {
        string url = $"{Constants.ServerURL}/profile/setusername";
        
        var requestData = new { username = newUsername };
        string jsonData = JsonConvert.SerializeObject(requestData);
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to save username: {request.error}");
                string errorMessage = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logging.Error($"Server response: {errorMessage}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to save username.\nCheck logs for more details.");
                    }
                }
                yield break;
            }
        }
    }
    
    private IEnumerator SaveBio(string newBio)
    {
        string url = $"{Constants.ServerURL}/profile/setbio";
        
        var requestData = new { bio = newBio ?? "" };
        string jsonData = JsonConvert.SerializeObject(requestData);
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to save bio: {request.error}");
                string errorMessage = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Logging.Error($"Server response: {errorMessage}");
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Failed to save bio.\nCheck logs for more details.");
                    }
                }
                yield break;
            }
        }
    }
    
    private void OpenKeyboardForUsername()
    {
        TrialKeyboard keyboard = FindFirstObjectByType<TrialKeyboard>();

        keyboard.forUsername = true;
        
        keyboard.SetMaxLength(12);
        
        keyboard.SetText("@");
        
        keyboard.onSubmit = (text) =>
        {
            if (!string.IsNullOrEmpty(text))
            {
                string processedUsername = text.StartsWith("@") ? text : "@" + text;
                
                if (processedUsername.Length < 4)
                {
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText("Username must be at least 3 characters (excluding @)");
                    }
                    keyboard.keyboard.SetActive(false);
                    return;
                }
                
                username = processedUsername;
                accountUI.transform.Find("AccountInfo/UsernameHeader/Username/Text").GetComponent<TextMeshProUGUI>().text = username;
                StartCoroutine(SavePlayerProfileCoroutine());
                
                if (HUDManager.instance != null)
                {
                    HUDManager.instance.SetHUDText("Username updated!");
                }
            }
            keyboard.forUsername = false;
            keyboard.keyboard.SetActive(false);
        };
        
        keyboard.onCancel = () =>
        {
            keyboard.forUsername = false;
            keyboard.keyboard.SetActive(false);
        };
        
        keyboard.keyboard.SetActive(true);
    }
    
    private void OpenKeyboardForBio()
    {
        TrialKeyboard keyboard = FindFirstObjectByType<TrialKeyboard>();
        if (keyboard == null)
        {
            Logging.Error("TrialKeyboard not found!");
            return;
        }
        
        keyboard.SetMaxLength(200);
        
        keyboard.onSubmit = (text) =>
        {
            bio = text ?? "";
            accountUI.transform.Find("AccountInfo/BioHeader/Text").GetComponent<TextMeshProUGUI>().text = bio;
            StartCoroutine(SavePlayerProfileCoroutine());
            
            keyboard.keyboard.SetActive(false);
            
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Bio updated!");
            }
        };
        
        keyboard.onCancel = () =>
        {
            keyboard.keyboard.SetActive(false);
        };
        
        keyboard.keyboard.SetActive(true);
    }
    
    public static string GetDateSuffix(int day)
    {
        if (day % 100 >= 11 && day % 100 <= 13)
        {
            return "th";
        }
        switch (day % 10)
        {
            case 1:
                return "st";
            case 2:
                return "nd";
            case 3:
                return "rd";
            default:
                return "th";
        }
    }
    
    public float GetEventProgressPercentage()
    {
        if (eventRequiredAmount <= 0)
            return 0f;
        
        return (eventTotalCompleted / (float)eventRequiredAmount) * 100f;
    }

    public float GetEventProgressDecimal()
    {
        if (eventRequiredAmount <= 0)
            return 0f;
        
        return eventTotalCompleted / (float)eventRequiredAmount;
    }
    
    public void UpdateEventProgress()
    {
        float percentage = GetEventProgressPercentage();
        float decimal_progress = GetEventProgressDecimal();

        try
        {
            Transform progressTextTransform = controlPanelRoot.transform.Find("UI/Event/EventStats/ActualAmount");
            if (progressTextTransform != null)
            {
                TextMeshProUGUI progressText = progressTextTransform.GetComponent<TextMeshProUGUI>();
                if (progressText != null)
                {
                    progressText.text = $"{eventTotalCompleted}/{eventRequiredAmount}";
                }
            }

            Transform percentageTextTransform = controlPanelRoot.transform.Find("UI/Event/EventStats/Percent");
            if (percentageTextTransform != null)
            {
                TextMeshProUGUI percentageText = percentageTextTransform.GetComponent<TextMeshProUGUI>();
                if (percentageText != null)
                {
                    percentageText.text = $"{percentage:F1}%";
                }
            }

            Transform progressBarTransform = controlPanelRoot.transform.Find("UI/Event/EventStats/Progress");
            if (progressBarTransform != null)
            {
                Image progressBar = progressBarTransform.GetComponent<Image>();
                if (progressBar != null)
                {
                    progressBar.fillAmount = Mathf.Clamp01(decimal_progress);
                }
            }
        }
        catch (Exception ex)
        {
            Logging.Error($"error updating event progress: {ex.Message}");
        }
    }

    public void UpdateEventLeaderboard(List<EventLeaderboardEntry> entries)
    {
        if (entries == null || entries.Count == 0)
        {
            Logging.Warning("no leaderboard entries to display");
            return;
        }

        var formattedLeaderboardText = "";

        foreach (var entry in entries)
        {
            if (entry.rank > 10) continue;
            string line = $"{entry.rank}. {entry.username} - {entry.count} completed Trials\n\n";
            formattedLeaderboardText += line;
        }
        
        try
        {
            Transform boardTextTransform = controlPanelRoot.transform.Find("UI/Event/EventStats/Leaderboard/BoardText");
            if (boardTextTransform != null)
            {
                TextMeshProUGUI boardText = boardTextTransform.GetComponent<TextMeshProUGUI>();
                if (boardText != null)
                {
                    boardText.text = formattedLeaderboardText;
                }
                else
                {
                    Logging.Error("text component on leaderboard is null");
                }
            }
            else
            {
                Logging.Error("leaderboard transform is null");
            }
        }
        catch (Exception ex)
        {
            Logging.Error($"error updating event leaderboard: {ex.Message}");
        }
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
    
    [Serializable]
    public class PlayerProfileData
    {
        public string playerId;
        public string username;
        public string bio;
        public CommunityTrialData[] uploadedTrials;
        public List<string> friends;
        public List<string> friendRequests;
    }
    
    [Serializable]
    public class NotificationData
    {
        public string notificationId;
        public string notificationType;
        public string type;
        public string fromPlayerId;
        public string fromUsername;
        public string message;
        public bool read;
        
        // challenge specific
        public string challengeId;
        public string trialServerName;
        public string trialLongName;
        public float time;
        public string createdAt;
    }
    
    [Serializable]
    public class NotificationsResponse
    {
        public NotificationData[] notifications;
        public PaginationData pagination;
    }
    
    [Serializable]
    public class FriendData
    {
        public string playerId;
        public string username;
    }
    
    [Serializable]
    public class FriendsResponse
    {
        public FriendData[] friends;
        public PaginationData pagination;
    }
    
    [Serializable]
    public class FriendRequestData
    {
        public string playerId;
        public string username;
    }
    
    [Serializable]
    public class FriendRequestsResponse
    {
        public FriendRequestData[] friendRequests;
        public PaginationData pagination;
    }
    
    [Serializable]
    public class SearchResult
    {
        public string playerId;
        public string username;
    }
    
    [Serializable]
    public class SearchResponse
    {
        public SearchResult[] results;
        public PaginationData pagination;
    }

    [Serializable]
    public class EventResponse
    {
        public string eventId;
        public string displayName;
        public string endDate;
        public string url;
        public int requiredAmount;
        public string createdAt;
        public bool active;
        public int totalCompleted;
        public Dictionary<string, int> contributions;
    }
    
    [Serializable]
    public class EventLeaderboardEntry
    {
        public string playerId;
        public string username;
        public int count;
        public int rank;
    }
    
    [Serializable]
    public class EventPlayerRankResponse
    {
        public int rank;
    }
}
