using GorillaLibrary.Behaviours;
using GorillaTrials.Behaviours.Networking;
using GorillaTrials.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace GorillaTrials.Behaviours;

public class BadgeWardrobe : WardrobeCategory
{
    public BadgeWardrobe instance;
    
    public override string Title => "Badges";
    
    private int _selectedBadgeIndex = -1;
    private List<int> _ownedBadgeIds = new();
    
    public void Awake()
    {
        instance = this;
        RigBadgeManager.Instance.onCosmeticUpdate.AddListener(UpdateCosmetics);
    }

    public void StartFetchingInventory()
    {
        StartCoroutine(FetchInventory());
    }

    public IEnumerator FetchInventory()
    {
        string url = $"{Constants.ServerURL}/inventory";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch badge inventory: {request.error}");
                Logging.Error($"Response: {request.downloadHandler.text}");
                yield break;
            }
            
            try
            {
                string jsonResponse = request.downloadHandler.text;
                BadgeArray badgeArray = JsonUtility.FromJson<BadgeArray>($"{{\"badges\":{jsonResponse}}}");
                
                if (badgeArray?.badges != null && badgeArray.badges.Length > 0)
                {
                    _ownedBadgeIds.Clear();
                    foreach (string badgeIdStr in badgeArray.badges)
                    {
                        if (int.TryParse(badgeIdStr, out int badgeId))
                        {
                            _ownedBadgeIds.Add(badgeId);
                        }
                    }
                    
                    UpdateCosmetics();
                }
                else
                {
                    Logging.Warning("No badges found in inventory response");
                }
            }
            catch (System.Exception ex)
            {
                Logging.Error($"Failed to parse badge inventory: {ex}");
            }
        }
    }

    public override void ApplyCosmetic(CosmeticWardrobe.CosmeticWardrobeSelection selection, int index)
    {
        selection.displayHead.SetCosmeticActiveArray([], []);
        
        foreach (Transform child in selection.displayHead.transform)
        {
            Destroy(child.gameObject);
        }
        
        GameObject badgePrefab = GetBadgeForIndex(index);
        
        if (badgePrefab != null)
        {
            GameObject badge = Instantiate(badgePrefab, selection.displayHead.transform, false);
            badge.name = badgePrefab.name;
            
            ApplyBadgeTransform(badge.transform);
        }
        
        selection.selectButton.enabled = badgePrefab != null;
        selection.selectButton.isOn = index == _selectedBadgeIndex;
        selection.selectButton.UpdateColor();
    }
    
    private void ApplyBadgeTransform(Transform badgeTransform)
    {
        badgeTransform.localPosition = new Vector3(0.0929f, -0.1155f, 0.0601f);
        badgeTransform.localRotation = Quaternion.Euler(90f, 0, 0);
        badgeTransform.localScale = Vector3.one;
    }
    
    private GameObject GetBadgeForIndex(int index)
    {
        if (index < 0 || index >= _ownedBadgeIds.Count)
            return null;
        
        int badgeId = _ownedBadgeIds[index];
        
        return badgeId switch
        {
            0 => RigBadgeManager.Instance.bronzeBadge,
            1 => RigBadgeManager.Instance.silverBadge,
            2 => RigBadgeManager.Instance.goldBadge,
            3 => RigBadgeManager.Instance.wrBadge,
            4 => RigBadgeManager.Instance.contribBadge,
            5 => RigBadgeManager.Instance.moderatorBadge,
            6 => RigBadgeManager.Instance.betaBadge,
            _ => null
        };
    }

    private int GetBadgeIdAtIndex(int index)
    {
        if (index < 0 || index >= _ownedBadgeIds.Count)
            return -1;
        return _ownedBadgeIds[index];
    }

    public override void SelectCosmetic(int index)
    {
        GameObject badgePrefab = GetBadgeForIndex(index);
        int badgeId = GetBadgeIdAtIndex(index);
        
        if (index == _selectedBadgeIndex)
        {
            RigBadgeManager.Instance.ClearBadge(VRRig.LocalRig);
            _selectedBadgeIndex = -1;
            if (NetworkBadgeSolution.Instance != null)
            {
                NetworkBadgeSolution.Instance.SetProperty("BadgeIndex", -1);
            }
            UpdateCosmetics();
            return;
        }

        if (badgePrefab != null)
        {
            RigBadgeManager.Instance.ClearBadge(VRRig.LocalRig);
            _selectedBadgeIndex = index;
            RigBadgeManager.Instance.SpawnLocalBadge(badgePrefab);
            if (NetworkBadgeSolution.Instance != null)
            {
                NetworkBadgeSolution.Instance.SetProperty("BadgeIndex", badgeId);
            }
            UpdateCosmetics();
        }
    }

    public override int GetSize()
    {
        return _ownedBadgeIds.Count;
    }

    public override void OnActivated(bool hasActivated)
    {
        if (hasActivated)
        {
            UpdateCosmetics();
        }
        else
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    [System.Serializable]
    private class BadgeArray
    {
        public string[] badges;
    }
}