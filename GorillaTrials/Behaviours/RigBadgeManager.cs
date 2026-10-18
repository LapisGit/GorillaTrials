using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using GorillaLocomotion;
using GorillaTrials.Tools;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GorillaTrials.Behaviours;

public class RigBadgeManager : MonoBehaviour
{
    public static RigBadgeManager Instance;

    private Vector3 badgeLocalPosition = new(0.095f, 0.275f, 0.1f);
    private Vector3 badgeLocalEulerAngles = new(350f, 0, 0);
    private Vector3 badgeLocalScale = Vector3.one;

    public GameObject bronzeBadge, silverBadge, goldBadge, wrBadge, contribBadge, moderatorBadge, betaBadge;
    
    private GameObject activeBadge;

    public readonly UnityEvent onCosmeticUpdate = new();

    public async void Awake()
    {
        Instance = this;
        await InitializeBadgeObjects();
    }

    async Task InitializeBadgeObjects()
    {
        bronzeBadge = await AssetLoader.LoadAsset<GameObject>("BronzeBadge");
        silverBadge = await AssetLoader.LoadAsset<GameObject>("SilverBadge");
        goldBadge = await AssetLoader.LoadAsset<GameObject>("GoldBadge");
        wrBadge = await AssetLoader.LoadAsset<GameObject>("Trophy");
        contribBadge = await AssetLoader.LoadAsset<GameObject>("ContribBadge");
        moderatorBadge = await AssetLoader.LoadAsset<GameObject>("ModeratorBadge");
        betaBadge = await AssetLoader.LoadAsset<GameObject>("BetaBadge");
    }

    public GameObject SpawnLocalBadge(GameObject badgePrefab)
    {
        if (badgePrefab == null)
        {
            return null;
        }

        Transform badgeParent = GetBadgeParent(VRRig.LocalRig);
        if (badgeParent == null)
        {
            return null;
        }

        ClearBadge(VRRig.LocalRig);
        
        activeBadge = Instantiate(badgePrefab, badgeParent, false);
        ApplyConfiguredTransform(activeBadge.transform);
        
        activeBadge.AddComponent<BadgeMarker>();
        
        return activeBadge;
    }
    
    public void SpawnOtherPlayerBadge(GameObject badgePrefab, VRRig rig)
    {
        Transform badgeParent = GetBadgeParent(rig);
        if (badgeParent == null)
        {
            return;
        }

        ClearBadge(rig);

        var otherBadge = Instantiate(badgePrefab, badgeParent, false);
        ApplyConfiguredTransform(otherBadge.transform);
        
        otherBadge.AddComponent<BadgeMarker>();
    }

    private static Transform GetBadgeParent(VRRig rig)
    {
        if (rig == null)
        {
            Logging.Warning("rig is null");
            return null;
        }

        return rig.transform.Find("rig/body_pivot/body");
    }

    public void SetBadgeTransform(Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
    {
        badgeLocalPosition = localPosition;
        badgeLocalEulerAngles = localEulerAngles;
        badgeLocalScale = localScale;

        if (activeBadge != null)
        {
            ApplyConfiguredTransform(activeBadge.transform);
        }
    }

    public void ClearBadge(VRRig rig)
    {
        if (rig == null) return;

        var markers = rig.GetComponentsInChildren<BadgeMarker>(true);
        foreach (var marker in markers)
        {
            var obj = marker.gameObject;
            Destroy(obj);
            if (obj == activeBadge || rig == VRRig.LocalRig)
                activeBadge = null;
        }
    }

    // im just using this to destroy the badge object lol
    private class BadgeMarker : MonoBehaviour { }

    private void ApplyConfiguredTransform(Transform badgeTransform)
    {
        badgeTransform.localPosition = badgeLocalPosition;
        badgeTransform.localRotation = Quaternion.Euler(badgeLocalEulerAngles);
        badgeTransform.localScale = badgeLocalScale;
    }

    [System.Serializable]
    private class BadgeArray
    {
        public string[] badges;
    }
}